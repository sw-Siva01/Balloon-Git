using System;
using Cysharp.Threading.Tasks;
using Colyseus;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;
using Colyseus.Schema;
using UnityEngine.Networking;
using System.Threading;
using UnityEngine.Scripting;


// Server response wrapper types
[Serializable]
public class ServerResponseWrapper
{
    public string requestID;
    public PayloadWrapper payload;
}

[Serializable]
public class PayloadWrapper
{
    public string OpCode;
    public string Message;
}

public class NetworkClient : MonoBehaviour
{

    public Colyseus_SocketController colyseus_SocketController;

    // public string HostAddress = "ws://localhost:2567";
    // public string GameName = "";

    public ColyseusClient _client;
    public ColyseusRoom<EmptyState> _room;
    public event Action<string> OnRawMessageReceived;
    public event Action<string> OnErrorReceived;
    public event Action<Exception> OnError;
    public event Action OnDisconnected;
    public event Action OnConnected;
    public float RequestTimeout = 5f;
    public static Action OnDisconnect;

    private List<string> requestList = new List<string>();
    private Coroutine _reconnectionCoroutine;

    public bool _isManuallyDisconnected;

    private string _serverUrl;
    private string _gameName;
    private string _environment;
    private float ReconnectInterval = 1f;

    private void Awake()
    {
        //Initialize();
    }

    void subscribeToEvents()
    {
        // Subscribe to any events if needed
        // For example, you can subscribe to OnRawMessageReceived here
    }

    public void Initialize(string url, string GameName, string environment = "production")
    {
        _serverUrl = url;
        _gameName = GameName;
        _environment = environment;
        //_client = new ColyseusClient(url);


        Debug.Log("Initializing NetworkClient with URL: " + _serverUrl + " and GameName: " + GameName);
        CreateGame(GameName, _serverUrl, environment);
    }

    public bool IsConnected()
    {
        return _client != null && _room != null;
    }


    bool roomActive;
    bool clientActive;

    private bool _isRetryingPing = false;

    public async UniTask<bool> TryPingAndJoin(string GameName, string url, string environment)
    {
        if (_isRetryingPing) return false; // prevent overlap
        _isRetryingPing = true;
        Debug.Log("Trying to ping and join...");
        const int maxRetries = 2;
        int attempts = 0;

        while (attempts < maxRetries)
        {
            using (var pingRequest = UnityWebRequest.Get(url + "/Ping"))
            {
                try
                {
                    await pingRequest.SendWebRequest().ToUniTask();
                    if (pingRequest.result == UnityWebRequest.Result.Success)
                    {
                        _isRetryingPing = false;
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"Ping failed: {pingRequest.error}. Retrying...");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception during ping: {ex.Message}");
                }
            }

            attempts++;
            await UniTask.Delay(1000); // delay between retries
        }

        _isRetryingPing = false;
        return false; // failed
    }

    public async void CreateGame(string GameName, string url, string environment)
    {
        if (IsConnected())
        {
            DebugHelper.Log("Already connected. Skipping connection attempt.");
            return;
        }
        bool success = await TryPingAndJoin(GameName, url, environment);
        if (!success)
        {
            Debug.LogWarning("Ping and join failed. Returning after delay...");
            return;
        }
        JoinOrCreateGame(GameName, url, environment).Forget();
    }

    public async UniTask<bool> TryPing()
    {
        bool success = await TryPingAndJoin(_gameName, _serverUrl, _environment);

        return success;
    }


    public async UniTask JoinOrCreateGame(string GameName, string url, string environment)
    {

        Debug.Log("Join or create Game called");
        if (IsConnected())
        {
            DebugHelper.Log("Already connected. Skipping connection attempt.");
            return;
        }
        try
        {
            colyseus_SocketController = GetComponent<Colyseus_SocketController>();
            string userId = APIController.instance.authentication.Id;
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload.Add("userId", userId);
            payload.Add("timestamp", timestamp.ToString());
            payload.Add("operatorName", APIController.instance.authentication.operatorname);
            string signature = Cryptography.SetEncryptedData(JsonConvert.SerializeObject(payload), true);
            UnityWebRequest pingwww = UnityWebRequest.Get(url + "/Ping");
            pingwww.SetRequestHeader("Content-Type", "application/json");
            string tokenUrl = url + "/Auth_Tocken";
            string requestUrl = $"{tokenUrl}?data={signature}";

            Debug.Log("Token request URL: " + requestUrl);

            UnityWebRequest www = UnityWebRequest.Get(requestUrl);

            www.SetRequestHeader("Content-Type", "application/json");
            try
            {
                await www.SendWebRequest().ToUniTask();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error Fetching Auth Tocken: " + ex.Message);
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("game quit due to " + www.error);

                APIController.DisconnectGame(www.error);
                return;
            }

            string json = www.downloadHandler.text;
            string data = Cryptography.GetEncryptedData(json);
            Debug.Log(json + "Decrypted data: " + data);
            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }
            _client = new ColyseusClient(url);
            string providerName = Cryptography.SetEncryptedData(APIController.instance.userDetails.game_Id + "_" + APIController.instance.userDetails.gameId + "_" + APIController.instance.userDetails.isBlockApiConnection.ToString() + "_" + APIController.instance.userDetails.currency_type + "_" + APIController.instance.authentication.environment);
            string UID = Cryptography.SetEncryptedData(APIController.instance.authentication.Id);
            string GName = Cryptography.SetEncryptedData(APIController.instance.authentication.gamename, true);
            string env = Cryptography.SetEncryptedData(APIController.instance.authentication.environment, true);
            string op = Cryptography.SetEncryptedData(APIController.instance.authentication.operatorname, true);
            string tocken = Cryptography.SetEncryptedData(data, true);


            DebugHelper.Log("Joining room: " + GameName);
            try
            {
                _room = await _client.JoinOrCreate<EmptyState>(GameName, new Dictionary<string, object>
                {
                    { "ProviderName", providerName },
                    { "userId", UID },
                    { "gameName", GName },
                    { "environment", env },
                    { "operator", op },
                    { "token", tocken }
                });
            }
            catch (Exception ex)
            {
                Debug.Log("ERROR===>" + ex);
                await UniTask.Delay(1500);
                CreateGame(GameName, url, environment);
                return;
            }

            DebugHelper.Log("Room: " + _room.RoomId);
            OnConnected?.Invoke();
            _isManuallyDisconnected = false;
            _isReconnecting = false;
            _room.OnMessage<byte[]>("__playground_message_types", (msg) =>
            {
                Debug.Log("Received playground message (ignored).");
            });
            _room.OnMessage<byte[]>("server_msg", (bytes) =>
            {
                Debug.Log("----SERVER MESSAGE----" + bytes.Length);
                var message = Encoding.UTF8.GetString(bytes);
                Debug.Log("----SERVER MESSAGE----" + message);
                try
                {
                    string decryptedMsg = Cryptography.GetEncryptedData(message);
                    ParseMessage(decryptedMsg);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError($"Parsing Error: {ex.Message}");
                    DebugHelper.LogError(message);
                }
            });

            _room.OnStateChange += (state, isFirstState) =>
            {
                Debug.Log("State updated. clientCount: " + state.clientCount);
            };

            _room.OnMessage<string>("", (message) =>
            {
                Debug.Log("Unnamed message received from server: " + message);
            });

            _room.OnLeave += (code) => ServerDisconected(code);

            _room.OnError += (code, message) =>
            {
                Debug.Log($"[NetworkClient] Error: {code} - {message}");
                OnError?.Invoke(new Exception(message));
            };
        }
        catch (Exception ex)
        {
            DebugHelper.LogError($"Error joining or creating game: {ex.Message}");
            await UniTask.Delay(5000);
            if (!IsConnected())
                CreateGame(GameName, url, environment);
        }
    }

    private void ParseMessage(string message)
    {
        if (message.Contains("pong"))
        {

        }
        else
        {
            if (message.Contains("requestID"))
            {
                JObject jsonObject = JObject.Parse(message);
                string requestID = jsonObject["requestID"]?.ToString();
                requestList.Remove(requestID);
            }
            OnRawMessageReceived?.Invoke(message);
        }
    }

    private async void ServerDisconected(object code)
    {
        Debug.Log($"[NetworkClient] Disconnected from server: {code}");
        await RestartReconnection();
    }

    public async void SendClientMsg(WSMessage data, string TaskID = "", CancellationToken cancellationToken = default)
    {
        Debug.Log($"SendClientMsg {TaskID} _room:{_room} _client:{_client}");

        try
        {
            // Wait for room and client initialization
            while ((_room == null || _client == null) && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            WSMessage message = data;

            string json = JsonConvert.SerializeObject(message);
            DebugHelper.Log($"Serialized JSON: {json}");

            if (message.Action != "heartbeat")
            {
                CheckTimeOut(message.RequestID).Forget();
            }

            // Wait until online
            while (!colyseus_SocketController.IsOnline() && !cancellationToken.IsCancellationRequested)
            {
                if (!colyseus_SocketController.GetTaskStatus(TaskID))
                    return;

                await UniTask.Delay(50, cancellationToken: cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            DebugHelper.Log(json + "=====Sent Request==== ");

            byte[] bytedata = Encoding.UTF8.GetBytes(Cryptography.SetEncryptedData(json));

            // --- ✅ Convert Task -> UniTask for cancellation support ---
            var sendTask = _room.Send("client_msg", bytedata);
            await sendTask.AsUniTask().AttachExternalCancellation(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            DebugHelper.LogWarning($"SendClientMsg cancelled: {TaskID}");
        }
        catch (Exception ex)
        {
            DebugHelper.LogError($"Error sending message: {ex.Message}");
            OnError?.Invoke(ex);
        }
    }


    public async UniTask CheckTimeOut(string requestID)
    {
        requestList.Add(requestID);
        await UniTask.Delay((int)(RequestTimeout * 1000));
        if (requestList.Contains(requestID))
        {
            try
            {
                Dictionary<string, string> response = new Dictionary<string, string>();
                Debug.Log("HANDLE DISCONNECTION" + requestID);
                if (_reconnectionCoroutine != null)
                    StopCoroutine(_reconnectionCoroutine);
                _reconnectionCoroutine = StartCoroutine(HandleDisconnection());
                response["requestID"] = requestID;
                response["message"] = "timeout";
                OnErrorReceived?.Invoke(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                DebugHelper.Log("Request Timedout: " + requestID);
            }
        }
    }

    public bool _isReconnecting;
    private IEnumerator HandleDisconnection(bool handleServer = true)
    {
        Cleanup();
        _isReconnecting = true;
        Debug.Log("Handling disconnection...");
        while (!_isManuallyDisconnected)
        {
            Debug.Log("Handling disconnection...1");
            yield return new WaitForSeconds(ReconnectInterval);
            // if (!IsConnected())
            //     CreateGame(_gameName, _serverUrl, _environment);
            if (handleServer)
                _ = CheckServerStatus();
            while (!IsConnected())
                yield return null;
            _isReconnecting = false;
            if (IsConnected())
                yield break;
        }
    }

    private async UniTask RestartReconnection()
    {
        if (_isReconnecting)
        {
            if (IsConnected())
            {
                Debug.Log($"[NetworkClient] Leaving room: {_room.RoomId}");
                await _room.Leave();
            }
            _room = null;
            _client = null;
        }
        if (_reconnectionCoroutine != null)
            StopCoroutine(_reconnectionCoroutine);
        Debug.Log("HANDLE DISCONNECTION1");
        _reconnectionCoroutine = StartCoroutine(HandleDisconnection(false));
    }

    private async void Cleanup()
    {
        if (_reconnectionCoroutine != null)
        {
            StopCoroutine(_reconnectionCoroutine);
            _reconnectionCoroutine = null;
        }
        // _serverStatusCts?.Cancel();
        // _serverStatusCts?.Dispose();
        if (IsConnected())
        {
            Debug.Log($"[NetworkClient] Leaving room: {_room.RoomId}");
            _ = _room.Leave();
            _room = null;
            _client = null;
        }
    }

    private async void OnApplicationQuit()
    {
        DebugHelper.Log("Application quitting. Disconnecting WebSocket.");
        _isManuallyDisconnected = true;
        await Disconnect();
    }

    public async UniTask Disconnect()
    {
        if (!IsConnected())
        {
            DebugHelper.Log("WebSocket already disconnected.");
            return;
        }

        try
        {
            _isManuallyDisconnected = true;
            OnDisconnected?.Invoke();
            Debug.Log($"[NetworkClient] Leaving room: {_room.RoomId}");
            await _room.Leave();
            DebugHelper.Log("WebSocket Disconnected.");
        }
        catch (Exception ex)
        {
            DebugHelper.LogError($"WebSocket Disconnect Error: {ex.Message}");
            OnError?.Invoke(ex);
        }
        finally
        {
            Cleanup();
        }
    }
    private CancellationTokenSource _serverStatusCts;
    public async UniTask CheckServerStatus()
    {
        _serverStatusCts?.Cancel();
        _serverStatusCts?.Dispose();
        _serverStatusCts = new CancellationTokenSource();
        var token = _serverStatusCts.Token;
        Debug.Log("Checking server for internet: " + Time.time);
        try
        {
            await UniTask.Delay(1000, cancellationToken: token);

            if (!colyseus_SocketController.IsOnline())
            {
                Debug.Log("Server offline: " + Time.time);
                OnDisconnected?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Server check cancelled: " + Time.time);
        }
    }
}

[Preserve]
public partial class EmptyState : Schema
{
    [Colyseus.Schema.Type(0, "int32")]
    public int clientCount = 0;
    [Colyseus.Schema.Type(1, "array", typeof(ArraySchema<ReflectionType>))]
    public ArraySchema<ReflectionType> Items = new ArraySchema<ReflectionType>();

    [Preserve]
    public EmptyState() { }


}
public class ReflectionType : Schema
{
    [Colyseus.Schema.Type(0, "string")]
    public string ExampleField = "";

    public ReflectionType() { } // ✅ REQUIRED: default constructor
}