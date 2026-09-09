using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Colyseus;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// Mirrors RouletteColyseusManager.cs's connection/reconnect/JWT-exchange/
// SessionState plumbing exactly — only the message set differs (start_round /
// set_holding / cash_out / tick / round_result instead of spin / spin_result),
// because Balloon's round is a live-ticking per-player cash-out, not a
// one-shot spin.
public class BalloonColyseusManager : MonoBehaviour
{
    public static BalloonColyseusManager Instance { get; private set; }

    private Client _client;
    private Room<BalloonRoomState> _room;

    public event Action<PlayerStateData> OnPlayerStateReceived;
    public event Action<TickData> OnTick;
    public event Action<RoundResultData> OnRoundResult;
    public event Action<ErrorData> OnErrorReceived;
    public event Action OnConnected;
    public event Action<string> OnConnectionFailed;
    public event Action OnReconnecting;
    public event Action<List<Betlist>> OnBetHistoryReceived;

    private const string PrefRoomId = "balloon_colyseus_room_id";
    private const string PrefToken = "balloon_colyseus_token";
    private const string PrefSeed = "balloon_client_seed";
    private string _clientSeed;
    private string _sessionEndReason;
    private bool _isConnecting = false;
    public bool IsSessionFailed { get; private set; } = false;
    private string _playerJwt;
    private bool _isSoftReconnect;
    private bool _roundInFlight;

    public float Balance { get; private set; }
    public string Currency { get; private set; } = "USD";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern string GetSessionItem(string key);
    [DllImport("__Internal")] static extern void   SetSessionItem(string key, string value);
    [DllImport("__Internal")] static extern void   RemoveSessionItem(string key);
    [DllImport("__Internal")] static extern void   NotifySessionExpired();
    [DllImport("__Internal")] static extern void   NotifyGameVersion(string version);
    string GetPref(string key)             => GetSessionItem(key);
    void   SetPref(string key, string val) => SetSessionItem(key, val);
    void   DeletePref(string key)          => RemoveSessionItem(key);
#else
    static string GetPref(string key) => PlayerPrefs.GetString(key, "");
    static void SetPref(string key, string val) { PlayerPrefs.SetString(key, val); PlayerPrefs.Save(); }
    static void DeletePref(string key) { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
#endif

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _clientSeed = GetPref(PrefSeed);
        if (string.IsNullOrEmpty(_clientSeed))
        {
            _clientSeed = Guid.NewGuid().ToString("N");
            SetPref(PrefSeed, _clientSeed);
        }

        BalloonGameConfig.Init();
#if UNITY_WEBGL && !UNITY_EDITOR
        NotifyGameVersion(Application.version);
#endif
        _client = new Client(BalloonGameConfig.ServerUrl);
    }

    void Start() => ConnectAsync().Forget();

    void OnApplicationPause(bool paused)
    {
        if (!paused) return;
        var roomToLeave = _room;
        _room = null;
        _ = roomToLeave?.Leave();
        ConnectAsync().Forget();
    }

    private async UniTask<string> ExchangeSessionCodeAsync(string code)
    {
        var url = $"{BalloonGameConfig.ApiUrl}/game/session";
        var json = $"{{\"code\":\"{code}\"}}";
        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest().ToUniTask();

        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"Session exchange failed: {req.error} — {req.downloadHandler.text}");

        var response = JsonUtility.FromJson<SessionResponse>(req.downloadHandler.text);
        if (string.IsNullOrEmpty(response?.token))
            throw new Exception("Session exchange returned empty token");

        return response.token;
    }

    private async UniTaskVoid ConnectAsync()
    {
        if (_isConnecting) return;
        _isConnecting = true;

        var savedRoomId = GetPref(PrefRoomId);
        var savedToken = GetPref(PrefToken);

        string jwtToken = null;
        if (!BalloonGameConfig.IsB2BMode)
        {
            DeletePref(PrefRoomId);
            DeletePref(PrefToken);
            savedRoomId = null;
            savedToken = null;
        }

        if (BalloonGameConfig.IsB2BMode)
        {
            if (!string.IsNullOrEmpty(_playerJwt))
            {
                jwtToken = _playerJwt;
            }
            else
            {
                try
                {
                    jwtToken = await ExchangeSessionCodeAsync(BalloonGameConfig.SessionCode);
                    _playerJwt = jwtToken;
                    Debug.Log("BalloonColyseusManager: session code exchanged for JWT.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"BalloonColyseusManager: session exchange failed — {e.Message}");
                    _isConnecting = false;
                    _isSoftReconnect = false;
                    IsSessionFailed = true;
#if UNITY_WEBGL && !UNITY_EDITOR
                    NotifySessionExpired();
#endif
                    OnConnectionFailed?.Invoke("SESSION_EXPIRED");
                    return;
                }
            }
        }

        var joinOptions = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(jwtToken))
            joinOptions["token"] = jwtToken;

        try
        {
            if (!BalloonGameConfig.IsB2BMode && !string.IsNullOrEmpty(savedRoomId) && !string.IsNullOrEmpty(savedToken))
            {
                try
                {
                    var rt = new ReconnectionToken { RoomId = savedRoomId, Token = savedToken };
                    _room = await _client.Reconnect<BalloonRoomState>(rt);
                    Debug.Log("Reconnected to BalloonRoom.");
                }
                catch
                {
                    Debug.Log("Reconnection failed — joining fresh.");
                    DeletePref(PrefRoomId);
                    DeletePref(PrefToken);
                    _room = await _client.JoinOrCreate<BalloonRoomState>("balloon", joinOptions);
                }
            }
            else
            {
                _room = await _client.JoinOrCreate<BalloonRoomState>("balloon", joinOptions);
                Debug.Log("Joined BalloonRoom.");
            }

            SetPref(PrefRoomId, _room.ReconnectionToken.RoomId);
            SetPref(PrefToken, _room.ReconnectionToken.Token);

            RegisterHandlers();
            _isSoftReconnect = false;
            _isConnecting = false;
            SessionState.isOnline = true;
            OnConnected?.Invoke();
        }
        catch (Exception e)
        {
            _isConnecting = false;
            Debug.LogError($"BalloonColyseusManager: connection failed — {e.Message}");
            if (_isSoftReconnect)
            {
                _isSoftReconnect = false;
                _playerJwt = null;
                IsSessionFailed = true;
                OnConnectionFailed?.Invoke("SESSION_EXPIRED");
                return;
            }
            if (e.Message.Contains("GAME_MISMATCH"))
            {
                OnConnectionFailed?.Invoke("GAME_MISMATCH");
                return;
            }
            OnConnectionFailed?.Invoke(e.Message);
            await UniTask.Delay(3000);
            ConnectAsync().Forget();
        }
    }

    private void RegisterHandlers()
    {
        _room.OnMessage<PlayerStateData>("player_state", data =>
        {
            Balance = data.balance;
            Currency = string.IsNullOrEmpty(data.currency) ? Currency : data.currency;
            BalloonGameConfig.SetCurrency(Currency);
            SessionState.entryAmountDetails.currency_type = Currency;
            if (!string.IsNullOrEmpty(data.username))
            {
                SessionState.name = data.username;
                SessionState.Id = data.username;
            }
            OnPlayerStateReceived?.Invoke(data);
            SessionEvents.OnUserDetailsUpdate?.Invoke();
        });

        _room.OnMessage<TickData>("tick", data => OnTick?.Invoke(data));

        _room.OnMessage<RoundResultData>("round_result", data =>
        {
            Balance = data.balance;
            _roundInFlight = false;
            OnRoundResult?.Invoke(data);
        });

        _room.OnMessage<ErrorWrapper>("error", data =>
        {
            if (data?.error == null) return;
            DebugHelper.Log($"[error msg] code={data.error.code} message={data.error.message}");
            if (data.error.code == "SESSION_EXPIRED" || data.error.code == "SESSION_TIMEOUT")
                _sessionEndReason = data.error.code;
            _roundInFlight = false;
            OnErrorReceived?.Invoke(data.error);
        });

        _room.OnMessage<BalloonBetHistoryData>("bet_history", data =>
        {
            if (data?.bets == null || data.bets.Count == 0) return;
            var list = new List<Betlist>();
            foreach (var b in data.bets)
                list.Add(new Betlist { id = b.id, bet_amount = b.bet_amount, win_amount = b.win_amount, dateTime = b.dateTime }); // no matchID — Balloon rounds have no "match" concept
            OnBetHistoryReceived?.Invoke(list);
        });

        _room.OnLeave += code =>
        {
            DebugHelper.Log($"[OnLeave] code={code} sessionEndReason='{_sessionEndReason}' roundInFlight={_roundInFlight}");
            SessionState.isOnline = false;
            if (_roundInFlight)
            {
                _roundInFlight = false;
                OnErrorReceived?.Invoke(new ErrorData { code = "CONNECTION_LOST", message = "Connection lost during round.", retryable = true });
            }
            if (!string.IsNullOrEmpty(_sessionEndReason))
            {
                IsSessionFailed = true;
                OnConnectionFailed?.Invoke(_sessionEndReason);
                return;
            }
            SessionEvents.OnInternetStatusChange?.Invoke(NetworkStatus.NetworkIssue);
            if (!_isConnecting)
                ConnectAsync().Forget();
        };
    }

    public bool IsConnected => _room != null;

    public void SoftReconnect()
    {
        IsSessionFailed = false;
        _sessionEndReason = "";
        _isSoftReconnect = true;
        OnReconnecting?.Invoke();
        ConnectAsync().Forget();
    }

    // clientRoundId should be a fresh Guid per round (GameController generates
    // one the moment the pump button is pressed) — it's the idempotency key
    // the server uses to dedupe a retried start_round.
    public void SendStartRound(string clientRoundId, float betAmount, float autoTarget = 0f)
    {
        _roundInFlight = true;
        _ = _room?.Send("start_round", new { clientRoundId, betAmount, clientSeed = _clientSeed, autoTarget });
    }

    // Send on both press (holding=true) and release (holding=false) of the
    // pump button — replaces the local isPressed flag driving IncrementMultiplier().
    public void SendSetHolding(bool holding) =>
        _ = _room?.Send("set_holding", new { holding });

    public void SendCashOut() =>
        _ = _room?.Send("cash_out", new { });

    public void RequestBalanceRefresh(string unused = null) =>
        _ = _room?.Send("refresh_balance", new { });

    public void RequestBetHistory() =>
        _ = _room?.Send("request_bet_history", new { });

    // string parameter: called via Unity's SendMessage from index.html's postMessage
    // bridge (iqplay_topup), which can only pass strings — mirrors ColyseusManager.AddBalance.
    public void AddBalance(string amountStr)
    {
        if (float.TryParse(amountStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float amount))
            _ = _room?.Send("add_balance", new { amount });
    }

    // Called via SendMessage from index.html's visibilitychange listener — Balloon
    // has no separate AudioManager-equivalent bridge target, so this lives here
    // alongside the other JS-bridge-facing methods.
    public void NotifyFocusChange(string visibleStr)
    {
        bool visible = visibleStr == "1";
        SessionState.isInFocus = visible;
        SessionEvents.OnSwitchingTab?.Invoke(visible);
    }

    // Optimistic local balance display update (iqplay_balance_update bridge message)
    // — same pattern as ColyseusManager.DirectBalanceUpdate. Followed by a real
    // refresh_balance round trip so the server-held balance stays authoritative.
    public void DirectBalanceUpdate(string amountStr)
    {
        if (!float.TryParse(amountStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float newBalance)) return;

        Balance = newBalance;
        OnPlayerStateReceived?.Invoke(new PlayerStateData { balance = newBalance, currency = Currency, username = SessionState.name });
        RequestBalanceRefresh();
    }
}

[Serializable] public class SessionResponse { public string token; }
[Serializable] public class ErrorData { public string code; public string message; public bool retryable; }
[Serializable] public class ErrorWrapper { public ErrorData error; }

[Serializable]
public class PlayerStateData
{
    public float balance;
    public string currency;
    public string username;
    public string phase;        // idle | inflating | resolved
    public float betAmount;
    public float multiplier;
    public bool holding;
    public float autoTarget;
}

[Serializable]
public class TickData
{
    public float multiplier;
}

[Serializable]
public class RoundResultData
{
    public string clientRoundId;
    public string outcome;        // "cashed_out" | "burst"
    public float multiplier;
    public float payout;
    public string revealedSeed;   // only present on "burst"
    public string roundHash;      // only present on "burst"
    public float balance;
}

// Wire shape from the "bet_history" message — mapped to Betlist (the UI model,
// defined in BetHistoryHandler.cs) before being handed to subscribers.
[Serializable] public class BalloonBetHistoryEntry { public string id; public double bet_amount; public double win_amount; public string dateTime; }
[Serializable] public class BalloonBetHistoryData { public List<BalloonBetHistoryEntry> bets; }