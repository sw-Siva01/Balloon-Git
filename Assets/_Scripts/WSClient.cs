using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using WebSocketState = NativeWebSocket.WebSocketState;
using WebSocket = NativeWebSocket.WebSocket;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


public class WSClient : MonoBehaviour
{
    public event Action<string> OnMessageReceived;

    public event Action<string> OnErrorReceived;


    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<Exception> OnError;

    public WebSocket _webSocket;
    private string _serverUrl;
    private bool _isManuallyDisconnected;
    private bool _isReconnecting;
    private float PingInterval = 2.5f;
    public float RequestTimeout = 5f;
    private float ReconnectInterval = 1f;
    private Stopwatch _stopwatch;
    public bool isPingRequired = false;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _reconnectionCoroutine;
    public bool isPingLogRequired = false;
    public int ping;
    private bool _isConnected => _webSocket?.State == WebSocketState.Open;
    private List<string> requestList = new List<string>();
    public bool IsConnected()
    {
        return _isConnected;
    }

    public async UniTask Connect(string url)
    {
        if (_isConnected)
        {
            //Debug.Log("Already connected. Skipping connection attempt.");
            return;
        }

        _serverUrl = url;
        _webSocket = new WebSocket(_serverUrl);
        _isManuallyDisconnected = false;
        _isReconnecting = false;
        _stopwatch = new Stopwatch();

        _webSocket.OnOpen += () =>
        {
            Debug.Log("WebSocket Connected.");
            OnConnected?.Invoke();
            if (_reconnectionCoroutine != null)
                StopCoroutine(_reconnectionCoroutine);

            // Start the heartbeat mechanism
            if (isPingRequired)
            {
                if (_heartbeatCoroutine != null) StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = StartCoroutine(SendHeartbeat());
            }
        };

        _webSocket.OnError += (error) =>
        {
            Debug.LogError($"WebSocket Error: {error}");
            OnError?.Invoke(new Exception(error));
        };

        _webSocket.OnClose += async (code) =>
        {
            Debug.Log($"WebSocket Closed with code: {code}");
            // OnDisconnected?.Invoke();
            await RestartReconnection();
        };

        _webSocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            if (message.Contains("error"))
                Debug.LogError(message);
            else
                ParseMessage(Cryptography.DecryptStr(message));
        };

        try
        {
            Debug.Log($"Attempting to connect to: {_serverUrl}");
            await _webSocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket Connection Error: {ex.Message}");
            OnError?.Invoke(ex);
            await RestartReconnection();
        }
    }

    private async UniTask RestartReconnection()
    {
        if (_isReconnecting && _webSocket != null)
        {
            if (_webSocket.State == WebSocketState.Open)
                await _webSocket.Close();

            _webSocket = null;
        }
        if (_reconnectionCoroutine != null)
            StopCoroutine(_reconnectionCoroutine);
        _reconnectionCoroutine = StartCoroutine(HandleDisconnection());
    }

    public async UniTask Disconnect()
    {
        if (_webSocket == null || !_isConnected)
        {
            Debug.Log("WebSocket already disconnected.");
            return;
        }

        try
        {
            _isManuallyDisconnected = true;
            OnDisconnected?.Invoke();
            await _webSocket.Close();
            Debug.Log("WebSocket Disconnected.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket Disconnect Error: {ex.Message}");
            OnError?.Invoke(ex);
        }
        finally
        {
            Cleanup();
        }
    }

    public async UniTask Send(WSMessage message)
    {
        if (!_isConnected)
        {
            await RestartReconnection();
            return;
        }
        try
        {
            string jsonMessage = JsonConvert.SerializeObject(message);
            string jMessage = JsonUtility.ToJson(message);
            if (message.Action != "heartbeat")
            {
                CheckTimeOut(message.RequestID).Forget();
            }
            string json = "";
            try
            {
                json = JsonConvert.SerializeObject(message);
                Debug.Log("Serialized JSON: " + json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Serialization failed: {ex.Message}");
            }
            //Debug.Log("Message: " + JsonConvert.SerializeObject(message, Formatting.Indented));
            Debug.Log(json + "=====Sent Request==== ");
            await _webSocket.SendText(Cryptography.EncryptStr(json));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending message: {ex.Message}");
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
                Debug.LogError("Request Timedout: " + requestID + "Check WebSocket State : " + (_webSocket == null));
                // OnDisconnected?.Invoke();
                // _ = _webSocket.Close();
                // if (_reconnectionCoroutine != null)
                //     StopCoroutine(_reconnectionCoroutine);
                Dictionary<string, string> response = new Dictionary<string, string>();
                _reconnectionCoroutine = StartCoroutine(HandleDisconnection());
                OnDisconnected?.Invoke();
                if (_webSocket == null)
                {
                    response["requestID"] = requestID;
                    response["message"] = "timeout";
                }
                else
                {
                    response["requestID"] = requestID;
                    response["message"] = "timeout";
                }
                OnErrorReceived?.Invoke(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                Debug.Log("Request Timedout: " + requestID);
            }
        }
    }

    public async UniTask SendText(string jsonMessage)
    {
        if (!_isConnected)
        {
            Debug.LogError("WebSocket is not connected.");
            await RestartReconnection();
            return;
        }

        try
        {
            Debug.Log(jsonMessage);
            await _webSocket.SendText(Cryptography.EncryptStr(jsonMessage));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending message: {ex.Message}");
            OnError?.Invoke(ex);
        }
    }

    private IEnumerator SendHeartbeat()
    {
        while (_isConnected)
        {
            yield return new WaitForSeconds(PingInterval);

            if (!_isConnected) yield break;

            if (_stopwatch.IsRunning)
            {
                Debug.LogError("Pong not received in time. Assuming disconnection.");
                OnDisconnected?.Invoke();
                if (_reconnectionCoroutine != null)
                    StopCoroutine(_reconnectionCoroutine);
                _ = _webSocket.Close();

                _reconnectionCoroutine = StartCoroutine(HandleDisconnection());
                _stopwatch.Stop();
                yield break;
            }

            _stopwatch.Restart();
            WSMessage heartbeatMessage = new WSMessage("heartbeat", "{}");
            _ = Send(heartbeatMessage);
        }
    }

    private IEnumerator HandleDisconnection()
    {
        if (_isManuallyDisconnected)
        {
            Debug.Log("Disconnection was manual. Skipping reconnect attempts.");
            yield break;
        }

        Cleanup();

        _isReconnecting = true;

        while (!_isManuallyDisconnected)
        {
            yield return new WaitForSeconds(ReconnectInterval);
            _ = Connect(_serverUrl);

            while (!_isConnected)
                yield return null;

            _isReconnecting = false;
        }
    }

    private void ParseMessage(string message)
    {
        if (message.Contains("pong"))
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();

            ping = (int)_stopwatch.ElapsedMilliseconds;
            if (isPingLogRequired)
                Debug.Log($"Ping: {ping} ms");
        }
        else
        {
            if (message.Contains("requestID"))
            {
                JObject jsonObject = JObject.Parse(message);
                string requestID = jsonObject["requestID"]?.ToString();
                requestList.Remove(requestID);
            }
            OnMessageReceived?.Invoke(message);
        }
    }

    private async void Cleanup()
    {
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }

        if (_reconnectionCoroutine != null)
        {
            StopCoroutine(_reconnectionCoroutine);
            _reconnectionCoroutine = null;
        }

        if (_webSocket != null)
        {
            if (_webSocket.State == WebSocketState.Open)
                await _webSocket.Close();

            _webSocket = null;
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (_isConnected)
            _webSocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        Debug.Log("Application quitting. Disconnecting WebSocket.");
        _isManuallyDisconnected = true;
        await Disconnect();
    }

    public static double GetResponseTime(string timeStamp)
    {
        DateTimeOffset current = DateTimeOffset.UtcNow;
        DateTimeOffset originalTime = DateTimeOffset.FromUnixTimeMilliseconds((long)Convert.ToDouble(timeStamp));
        //Debug.Log($"{current}: {originalTime}");
        return (current - originalTime).Milliseconds;

    }
}

[System.Serializable]
public class WSMessage
{
    public string Action { get; set; }
    public string RequestID { get; set; }
    public string Body { get; set; }

    public WSMessage(string action, string payload)
    {
        Action = action;
        SetRequestID();
        Body = payload;
    }

    void SetRequestID()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long unixTimestamp = now.ToUnixTimeMilliseconds();
        //Debug.Log(unixTimestamp);
        RequestID = unixTimestamp.ToString();
        //RequestID = Guid.NewGuid().ToString();
    }
}