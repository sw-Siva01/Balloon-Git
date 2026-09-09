/*using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using UnityEngine;

/// <summary>
/// Network authority for the Balloon game.
///
/// GameController should ONLY:
/// - send player input
/// - display server state
/// - play animations/UI
///
/// Server owns:
/// - bet validation
/// - balance deduction
/// - multiplier
/// - crash/burst point
/// - auto cash-out
/// - payout
/// - wallet settlement
/// </summary>
public class BalloonNetworkController : MonoBehaviour
{
    public static BalloonNetworkController Instance { get; private set; }

    [Header("Colyseus")]
    [SerializeField]
    private string serverUrl = "ws://127.0.0.1:2567";

    [SerializeField]
    private string roomName = "balloon";

    [Header("Connection")]
    [SerializeField]
    private bool connectOnStart = true;

    public bool IsConnected => room != null;
    public bool IsConnecting { get; private set; }

    public string LastErrorCode { get; private set; }
    public string LastErrorMessage { get; private set; }

    private ColyseusClient client;
    private ColyseusRoom<Dictionary<string, object>> room;

    private string currentRoundId;

    // ---------------------------------------------------------------------
    // Events
    // ---------------------------------------------------------------------

    public event Action<PlayerStateMessage> OnPlayerState;
    public event Action<TickMessage> OnTick;
    public event Action<RoundResultMessage> OnRoundResult;
    public event Action<BalloonErrorMessage> OnError;
    public event Action<bool> OnConnectionChanged;

    // ---------------------------------------------------------------------
    // Unity
    // ---------------------------------------------------------------------

    private async void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (connectOnStart)
        {
            await Connect();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ---------------------------------------------------------------------
    // Connection
    // ---------------------------------------------------------------------

    public async Task<bool> Connect()
    {
        if (IsConnected)
            return true;

        if (IsConnecting)
            return false;

        IsConnecting = true;

        try
        {
            Debug.Log("[BalloonNetwork] Connecting to " + serverUrl);

            client = new ColyseusClient(serverUrl);

            Dictionary<string, object> options = BuildJoinOptions();

            room = await client.JoinOrCreate<Dictionary<string, object>>(
                roomName,
                options
            );

            Debug.Log(
                "[BalloonNetwork] Connected. SessionId = " +
                room.SessionId
            );

            RegisterMessages();

            OnConnectionChanged?.Invoke(true);

            // Ask server for the latest wallet balance.
            RefreshBalance();

            // Ask for history.
            RequestBetHistory();

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[BalloonNetwork] Connection failed: " +
                ex
            );

            room = null;
            client = null;

            OnConnectionChanged?.Invoke(false);

            return false;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private Dictionary<string, object> BuildJoinOptions()
    {
        Dictionary<string, object> options =
            new Dictionary<string, object>();

        *//*
         * IMPORTANT:
         *
         * Server BasePlatformRoom.onAuth() expects:
         *
         * {
         *     token?: string,
         *     ip?: string,
         *     ua?: string
         * }
         *
         * If demo mode is being used, token can be omitted.
         *
         * Replace GetPlayerToken() with your actual APIController
         * token property when you know the exact field.
         *//*

        string token = GetPlayerToken();

        if (!string.IsNullOrEmpty(token))
        {
            options["token"] = token;
        }

        options["ua"] = SystemInfo.operatingSystem;

        return options;
    }

    private string GetPlayerToken()
    {
        *//*
         * CONNECT THIS TO YOUR EXISTING AUTH SYSTEM.
         *
         * Example:
         *
         * return APIController.instance.authentication.token;
         *
         * Do not invent a token here.
         *//*

        return PlayerPrefs.GetString("player_token", "");
    }

    private void RegisterMessages()
    {
        if (room == null)
            return;

        room.OnMessage<PlayerStateMessage>(
            "player_state",
            message =>
            {
                Debug.Log(
                    $"[BalloonNetwork] player_state " +
                    $"phase={message.phase} " +
                    $"balance={message.balance} " +
                    $"multiplier={message.multiplier}"
                );

                OnPlayerState?.Invoke(message);
            }
        );

        room.OnMessage<TickMessage>(
            "tick",
            message =>
            {
                OnTick?.Invoke(message);
            }
        );

        room.OnMessage<RoundResultMessage>(
            "round_result",
            message =>
            {
                Debug.Log(
                    $"[BalloonNetwork] round_result " +
                    $"outcome={message.outcome} " +
                    $"multiplier={message.multiplier} " +
                    $"payout={message.payout}"
                );

                OnRoundResult?.Invoke(message);
            }
        );

        room.OnMessage<BalloonErrorMessage>(
            "error",
            message =>
            {
                LastErrorCode =
                    message.error != null
                        ? message.error.code
                        : "";

                LastErrorMessage =
                    message.error != null
                        ? message.error.message
                        : "";

                Debug.LogWarning(
                    $"[BalloonNetwork] ERROR " +
                    $"{LastErrorCode}: {LastErrorMessage}"
                );

                OnError?.Invoke(message);
            }
        );
    }

    // ---------------------------------------------------------------------
    // Balloon commands
    // ---------------------------------------------------------------------

    /// <summary>
    /// Start a new Balloon round.
    /// Server validates the amount and deducts the balance.
    /// </summary>
    public bool PlaceBet(
        float amount,
        float autoTarget = 0f,
        string clientSeed = null)
    {
        if (!IsConnected)
        {
            Debug.LogWarning(
                "[BalloonNetwork] Cannot place bet - not connected."
            );

            return false;
        }

        if (string.IsNullOrEmpty(clientSeed))
        {
            clientSeed = Guid.NewGuid().ToString("N");
        }

        currentRoundId =
            Guid.NewGuid().ToString("N");

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                ["clientRoundId"] = currentRoundId,
                ["betAmount"] = Math.Round(amount, 2),
                ["clientSeed"] = clientSeed,
                ["autoTarget"] = autoTarget
            };

        Debug.Log(
            $"[BalloonNetwork] start_round " +
            $"id={currentRoundId} amount={amount} auto={autoTarget}"
        );

        room.Send("start_round", data);

        return true;
    }

    /// <summary>
    /// Tell the server that the player is holding the Balloon button.
    ///
    /// Server starts advancing multiplier only while holding == true.
    /// </summary>
    public void SetHolding(bool holding)
    {
        if (!IsConnected)
            return;

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                ["holding"] = holding
            };

        room.Send("set_holding", data);
    }

    /// <summary>
    /// Request server-side cash-out.
    /// Server calculates payout using its multiplier.
    /// </summary>
    public void CashOut()
    {
        if (!IsConnected)
            return;

        Debug.Log("[BalloonNetwork] cash_out");

        room.Send("cash_out");
    }

    /// <summary>
    /// Refresh wallet balance.
    /// </summary>
    public void RefreshBalance()
    {
        if (!IsConnected)
            return;

        room.Send("refresh_balance");
    }

    /// <summary>
    /// Request betting history.
    /// </summary>
    public void RequestBetHistory()
    {
        if (!IsConnected)
            return;

        room.Send("request_bet_history");
    }

    /// <summary>
    /// Demo-only balance helper.
    /// Server ignores this for authenticated players.
    /// </summary>
    public void AddDemoBalance(float amount)
    {
        if (!IsConnected)
            return;

        room.Send(
            "add_balance",
            new Dictionary<string, object>
            {
                ["amount"] = amount
            }
        );
    }

    // ---------------------------------------------------------------------
    // Disconnect
    // ---------------------------------------------------------------------

    public async Task Disconnect()
    {
        if (room == null)
            return;

        try
        {
            await room.Leave();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[BalloonNetwork] Leave error: " + ex.Message
            );
        }

        room = null;
        client = null;

        OnConnectionChanged?.Invoke(false);
    }

    // ---------------------------------------------------------------------
    // Message models
    // ---------------------------------------------------------------------

    [Serializable]
    public class PlayerStateMessage
    {
        public float balance;
        public string currency;
        public string username;

        public string phase;
        public float betAmount;
        public float multiplier;
        public bool holding;
        public float autoTarget;
    }

    [Serializable]
    public class TickMessage
    {
        public float multiplier;
    }

    [Serializable]
    public class RoundResultMessage
    {
        public string clientRoundId;
        public string outcome;

        public float multiplier;
        public float payout;
        public float balance;

        public string revealedSeed;
        public string roundHash;
    }

    [Serializable]
    public class BalloonErrorMessage
    {
        public BalloonError error;
    }

    [Serializable]
    public class BalloonError
    {
        public string code;
        public string message;
        public bool retryable;
    }
}*/