﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SimpleJSON;
using Nakama.TinyJson;
using System.Linq;
using Satori;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Nakama.Helpers
{
    public class NakamaManager : MonoBehaviour
    {
        public bool isCreateUser;
        public bool isSocketOpen;
        #region FIELDS

        private const string UdidKey = "udid";
        public string masterID;
        [SerializeField] private NakamaConnectionData connectionData = null;

        public IClient client = null;
        public ISession session = null;
        public ISocket socket = null;
     
        #endregion

        #region EVENTS

        public event Action onConnecting = null;
        public event Action onConnected = null;
        public event Action onDisconnected = null;
        public event Action onLoginSuccess = null;
        public event Action onLoginFail = null;

        #endregion

        #region PROPERTIES

        public static NakamaManager Instance { get; private set; } = null;
        public string Username { get => session == null ? string.Empty : session.Username; }
        public bool IsLoggedIn { get => socket != null && socket.IsConnected; }
        public ISocket Socket { get => socket; }
        public ISession Session { get => session; }
        public IClient Client { get => client; }

        #endregion

        #region BEHAVIORS

        int randomNumber;
       
        private void Awake()
        {
            Instance = this;
            randomNumber = UnityEngine.Random.Range(1000, 900000);
        }


        //public string backupServer = "turbogames.utwebapps.com";
        public async void AutoLogin(Action<bool> action, bool isTryBackupServer = false)
        {
            try
            {
                client = new Client(connectionData.Scheme, isTryBackupServer ? connectionData.backupHost : connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
                CustomMobileLogin(APIController.instance.userDetails.Id + randomNumber, (async (status, message) =>
                {
                    if (status)
                    {
                        await OpenSocket();
                        action.Invoke(true);
                    }
                    else
                    {
                        if (isTryBackupServer)
                        {
                            Debug.Log("try backup server");
                            action.Invoke(false);
                        }
                        else
                        {
                            AutoLogin(action, true);
                            Debug.Log("try live server server");
                        }
                        Debug.Log("Status failed");
                    }
                }
                ), true);
            }
            catch (Exception ex)
            {
                action.Invoke(false);
                Debug.LogError("Error ::: " + ex.Message);
            }
        }


        //public async void AutoLogin(Action<bool> action)
        //{
        //    try
        //    {
        //        client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
        //        CustomMobileLogin(APIController.instance.userDetails.Id + randomNumber, (async (status, message) =>
        //        {
        //            if (status)
        //            {
        //                await OpenSocket();
        //                action.Invoke(true);
        //            }
        //            else
        //            {
        //                action.Invoke(false);
        //                Debug.Log("Status failed");
        //            }
        //        }
        //        ), true);
        //    }
        //    catch(Exception ex)
        //    {
        //        action.Invoke(false);
        //        Debug.LogError("Error ::: "+ex.Message);
        //    }

        //}



        private void OnApplicationQuit()
        {
            if (socket != null)
                socket.CloseAsync();
        }

        public RetryConfiguration retryConfiguration = new RetryConfiguration(1000, 1);
   
      
        public async void CustomMobileLogin(string mobileNo,Action<bool,string> action,bool isCreate = true)
        {
            //GameController.Instance.Loading.gameObject.SetActive(true);
            try
            {
                Debug.Log("try register"+ mobileNo);
                string DisplayName;
                session = await client.AuthenticateCustomAsync(mobileNo, mobileNo, isCreate, null, retryConfiguration);
                Debug.Log("Login Success");
                //Task.Delay(1000).Wait();
                if (isCreateUser)
                    await client.UpdateAccountAsync(session, mobileNo, mobileNo.Substring(0, 10), null, null, null, null, retryConfiguration);
                onLoginSuccess?.Invoke();
              //  GameController.Instance.Loading.gameObject.SetActive(false);
                action?.Invoke(true, "success");
                //success.Invoke(JsonUtility.ToJson(session));
            }
            catch (Nakama.ApiResponseException ex)
            {
                Debug.Log("Login Failed " + ex.Message + " : " + ex.GrpcStatusCode);
                //GameController.Instance.Loading.gameObject.SetActive(false);
                action.Invoke(false, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogFormat("Error: {0}", ex.Message);

                action.Invoke(false, ex.Message);
//                GameController.Instance.Loading.gameObject.SetActive(false);
                Debug.LogFormat("Error: {0}", ex.Message);
            }

        }
    
        bool isRunWebGLGame = false;
        public async Task OpenSocket()
        {
            ////Debug.LogError("scoket init ");
            int count = 0;
            if (socket == null)
            {
                GameObject go = GameObject.Find("[Nakama Socket]");
                if (go != null)
                {
                    Destroy(go);
                }

                socket = client.NewSocket(true);
                while (socket.IsConnecting)
                {
                    //Debug.LogError("scoket init try to connect");
                    await UniTask.Delay(500);
                }
            }
            socket.Connected -= Socket_Connected;
            Debug.Log("scoket init 1");
            socket.Closed -= CloseSocket;
            Debug.Log("scoket init 2");
            socket.Connected += Socket_Connected;
            Debug.Log("scoket init 1");
            socket.Closed += CloseSocket;
            socket.ReceivedError += (err) =>{
                Debug.Log("socket error " + err.Message);
            };
            Debug.Log("scoket init 2");
            //Debug.LogError("scoket init ... " + socket.IsConnecting);
            Debug.Log("scoket init ... end");
            try
            {
                Debug.Log("scoket init try");
                await socket.ConnectAsync(session);
                //Debug.LogError("scoket init success");
            }
            catch
            {
                //Debug.LogError("scoket init failed");
                socket = null;
                //if (GameController.Instance.isInGame)
                //{
                //    await UniTask.Delay(System.TimeSpan.FromSeconds(1));
                //    OpenSocket();
                //}
                //else
                //{
                //    ServerConnectionFailed(() => { OpenSocket(); });
                //}
                await UniTask.Delay(System.TimeSpan.FromSeconds(1));
                await OpenSocket();
                return;
            }
        }
        public double lastCalledTime = 0;
        public async void CloseSocket()
        {
            socket = null;
            isSocketOpen = false;
            Debug.Log("Socket closed"   + APIController.instance.isOnline);
            //while(!APIController.instance.isOnline)
            //{
            //    await UniTask.Delay(100);
            //}
            await OpenSocket();
        }

        private void Socket_Connected()
        {
            Debug.Log("socket connected");
      
        }
 
        string matchId;

        public async Task SendMessage(long opcode, string message, Action<bool> action = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (action != null)
                {
                    action.Invoke(false);
                }
                socket = null;
                NakamaManager.Instance.CloseSocket();
            }
            try
            {
              //  Debug.Log("message send "+opcode);
                if (Socket.IsConnected)
                {

                  //  Debug.Log("Socket Connected" + opcode);

                    await socket.SendMatchStateAsync(matchId, opcode, message);
                    if (action != null)
                    {
                        action.Invoke(true);
                    }
                }
                else
                {
                    action?.Invoke(false);
                }
               // //Debug.LogError("send message success");
            }
            catch (Exception ex)
            {
                if (action != null)
                {
                    action.Invoke(false);
                }
                socket = null;
                NakamaManager.Instance.CloseSocket();
            }
        }
      
        public async void SendRPC(string rpc, string payload,Action<string> action,bool isCheckNetwork = false)
        {
            Debug.Log($"Rpc called {rpc} {payload} {isCheckNetwork.ToString()}");
            try
            {
                if (client == null || session == null)
                    action.Invoke(null);
                EncryptedPayload encryptpayload = new EncryptedPayload();
                encryptpayload.data = EncryptString(payload);
                encryptpayload.value = EncryptString(APIController.instance.userDetails.operatorDomainUrl);
                var output = await client.RpcAsync(session, EncryptString(rpc), encryptpayload.ToJson(), retryConfiguration);
                Debug.Log($"Rpc Done Output {output.Payload}");
                encryptpayload = JsonUtility.FromJson<EncryptedPayload>(output.Payload);
                string outputPayload = DecryptString(encryptpayload.data);
                action.Invoke(outputPayload);
            }
            catch(Exception ex)
            {
                if (!isCheckNetwork)
                {
                    bool isDone = false;
                    while (!isDone)
                    {
                        Debug.Log($"Rpc try to called again {rpc} {payload} {isCheckNetwork.ToString()}");
                        APIController.instance.CheckInternetandProcess((success) =>
                        {
                            if (success)
                            {
                                isDone = true;  
                                SendRPC(rpc, payload, action, true);
                            }
                        });
                        await UniTask.Delay(2000);
                        return;

                    }
                }
                else
                {
                    action.Invoke(ex.Message);
                }
            }
        }
        private static readonly string key = "Hs9INfoebjwQwtrGRMD1hPaNAMrvGXxX"; // Replace with your key

        public static string EncryptString(string plainText)
        {
            int shift = 0;
            char[] buffer = plainText.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {

                char c = buffer[i];
                shift = key[i % key.Length] - 32;
                int value = c;
                if (c >= 32 && c <= 126)
                {
                    value = (c + shift);
                    if (value > 126)
                    {

                        value = (32 + (value - 127));
                    }
                    c = (char)value;
                    buffer[i] = c;
                }
            }
            return new string(buffer);
        }
        public static string DecryptString(string encryptedText)
        {
            int shift = 0;

            char[] buffer = encryptedText.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];
                shift = key[i % key.Length] - 32;

                int value = c;
                if (c >= 32 && c <= 126)
                {
                    value = (c - shift);
                    if (value < 32)
                    {
                        value = (127 - (32 - value));
                    }
                    c = (char)value;
                    buffer[i] = c;
                }
            }
            return new string(buffer);
        }
        public void LogOut()
        {
            socket.CloseAsync();
        }

        public string message;

        public class EncryptedPayload
        {
            public string data;
            public string value;
        }
        #endregion
    }
}