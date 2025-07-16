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

namespace Aws_Gateway
{
    public class WSClient : MonoBehaviour
    {
        AWS_SocketController awsController;
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

        public async UniTask CheckServerStatus()
        {
            DebugHelper.Log("Checking server for internet" + Time.time);
            await UniTask.Delay(1000);
            if (!awsController.IsOnline())
            {
                DebugHelper.Log("Checking server for internet =  false" + Time.time);
                OnDisconnected?.Invoke();
                return;
            }
        }


        public async UniTask Connect(string url)
        {
            awsController = GetComponent<AWS_SocketController>();

            if (_isConnected)
            {
                //DebugHelper.Log("Already connected. Skipping connection attempt.");
                return;
            }

            _serverUrl = url;
            _webSocket = new WebSocket(_serverUrl);
            _isManuallyDisconnected = false;
            _isReconnecting = false;
            _stopwatch = new Stopwatch();

            _webSocket.OnOpen += () =>
            {
                DebugHelper.Log("WebSocket Connected.");
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
                DebugHelper.LogError($"WebSocket Error: {error}");
                OnError?.Invoke(new Exception(error));
            };

            _webSocket.OnClose += async (code) =>
            {
                DebugHelper.Log($"WebSocket Closed with code: {code}");
                // OnDisconnected?.Invoke();
                await RestartReconnection();
            };

            _webSocket.OnMessage += (bytes) =>
            {
                var message = Encoding.UTF8.GetString(bytes);
                try
                {
                    ParseMessage(Cryptography.DecryptStr(message));
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError($"Parsing Error: {ex.Message}");
                    DebugHelper.LogError(message);
                    //string error = "{\"requestID\":\"1734356995159\",\"payload\":{\"OpCode\":\"LambdaResponse\",\"Message\":\"{\\\"code\\\":502,\\\"message\\\":\\\"{ message: \\\\\\\"Internal server error\\\\\\\"}\\\"}\"}}";
                    //ParseMessage(error);
                    //ParseMessage();
                }
            };

            try
            {
                DebugHelper.Log($"Attempting to connect to: {_serverUrl}");
                await _webSocket.Connect();
            }
            catch (Exception ex)
            {
                DebugHelper.LogError($"WebSocket Connection Error: {ex.Message}");
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
            DebugHelper.Log("HANDLE DISCONNECTION1");
            _reconnectionCoroutine = StartCoroutine(HandleDisconnection(false));
        }

        public async UniTask Disconnect()
        {
            if (_webSocket == null || !_isConnected)
            {
                DebugHelper.Log("WebSocket already disconnected.");
                return;
            }

            try
            {
                _isManuallyDisconnected = true;
                OnDisconnected?.Invoke();
                await _webSocket.Close();
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

        public async UniTask Send(WSMessage data, string TaskID)
        {
            // if (!_isConnected)
            // {
            //     await RestartReconnection();
            //     return;
            // }
            WSMessage message = data;
            DebugHelper.Log(JsonConvert.SerializeObject(message) + "??????" + TaskID);
            string id = TaskID;
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
                    DebugHelper.Log("Serialized JSON: " + json);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError($"Serialization failed: {ex.Message}");
                }

                DebugHelper.Log("Checking if Server Connected");
                while (!awsController.IsOnline())
                {
                    if (!awsController.GetTaskStatus(id))
                    {
                        return;
                    }
                    await UniTask.Delay(50);
                }
                //string json = "";
                try
                {
                    json = JsonConvert.SerializeObject(message);
                    DebugHelper.Log("Serialized JSON: " + json);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError($"Serialization failed: {ex.Message}");
                }
                DebugHelper.Log(json + "=====Sent Request==== ");
                await _webSocket.SendText(Cryptography.EncryptStr(json));
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
                    DebugHelper.LogError("Request Timedout: " + requestID + "Check WebSocket State : " + (_webSocket == null));
                    // OnDisconnected?.Invoke();
                    // _ = _webSocket.Close();
                    // if (_reconnectionCoroutine != null)
                    //     StopCoroutine(_reconnectionCoroutine);
                    Dictionary<string, string> response = new Dictionary<string, string>();
                    DebugHelper.Log("HANDLE DISCONNECTION" + requestID);
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

        public async UniTask SendText(string jsonMessage)
        {
            if (!_isConnected)
            {
                DebugHelper.LogError("WebSocket is not connected.");
                await RestartReconnection();
                return;
            }

            try
            {
                DebugHelper.Log(jsonMessage);
                await _webSocket.SendText(Cryptography.EncryptStr(jsonMessage));
            }
            catch (Exception ex)
            {
                DebugHelper.LogError($"Error sending message: {ex.Message}");
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
                    DebugHelper.LogError("Pong not received in time. Assuming disconnection.");
                    OnDisconnected?.Invoke();
                    if (_reconnectionCoroutine != null)
                        StopCoroutine(_reconnectionCoroutine);
                    _ = _webSocket.Close();
                    DebugHelper.Log("HANDLE DISCONNECTION2");
                    _reconnectionCoroutine = StartCoroutine(HandleDisconnection(false));
                    _stopwatch.Stop();
                    yield break;
                }

                _stopwatch.Restart();
                WSMessage heartbeatMessage = new WSMessage("heartbeat", "{}");
                _ = Send(heartbeatMessage, "");
            }
        }

        private IEnumerator HandleDisconnection(bool handleServer = true)
        {
            if (_isManuallyDisconnected)
            {
                DebugHelper.Log("Disconnection was manual. Skipping reconnect attempts.");
                yield break;
            }

            Cleanup();

            _isReconnecting = true;

            while (!_isManuallyDisconnected)
            {
                yield return new WaitForSeconds(ReconnectInterval);
                _ = Connect(_serverUrl);
                if (handleServer)
                    _ = CheckServerStatus();
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
                    DebugHelper.Log($"Ping: {ping} ms");
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
            DebugHelper.Log("Application quitting. Disconnecting WebSocket.");
            _isManuallyDisconnected = true;
            await Disconnect();
        }

        public static double GetResponseTime(string timeStamp)
        {
            DateTimeOffset current = DateTimeOffset.UtcNow;
            DateTimeOffset originalTime = DateTimeOffset.FromUnixTimeMilliseconds((long)Convert.ToDouble(timeStamp));
            return (current - originalTime).Milliseconds;
        }
    }
}