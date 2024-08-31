using System;
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

        public static List<char> cryptocharacters = new List<char>();
        private void Awake()
        {
            Instance = this;
            randomNumber = UnityEngine.Random.Range(1000, 900000);
            for (int i = 0; i < 26; i++)
            {
                cryptocharacters.Add((char)('a' + i));
            }
            for (int i = 0; i < 26; i++)
            {
                cryptocharacters.Add((char)('A' + i));
            }
            for (int i = 0; i < 10; i++)
            {
                cryptocharacters.Add((char)('0' + i));
            }
        }
        public static string Encryptbase64String(string plainText)
        {
            int shift = 0;
            char[] buffer = plainText.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];
                if (cryptocharacters.Contains(c))
                {
                    shift = key[i % key.Length];
                    int value = (cryptocharacters.IndexOf(c) + cryptocharacters.IndexOf((char)shift));
                    if (value >= cryptocharacters.Count)
                    {
                        value = ((value - cryptocharacters.Count));
                    }
                    c = cryptocharacters[value];
                    buffer[i] = c;
                }
                else
                {
                    Debug.Log("notfound" + c);
                }
            }
            return new string(buffer);
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


        public async void CustomMobileLogin(string mobileNo, Action<bool, string> action, bool isCreate = true)
        {
            //GameController.Instance.Loading.gameObject.SetActive(true);
            try
            {
                Debug.Log("try register" + mobileNo);
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
        bool isTrytoOpenSocket = false;
        public string checkInternetUrl = "https://6rugffwb323fkm7j7umild4vjm0hfcfm.lambda-url.ap-south-1.on.aws/";
        public async UniTask OpenSocket()
        {
            if (isTrytoOpenSocket)
            {
                return;
            }
            isTrytoOpenSocket = true;
            ////Debug.LogError("scoket init ");
            ///
            bool isOnline = false;
            while (!isOnline)
            {
                WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_FORMDATA
                   ,
                   checkInternetUrl,
                   new List<KeyValuePojo>(),
                   (bool isSuccess, string error, string body) =>
                   {
                       isOnline = isSuccess;
                   }, 3);
                Debug.Log("checking internet for socket");
                await UniTask.Delay(3000);
            }
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
            isTrytoOpenSocket = false;
            socket.Connected -= Socket_Connected;
            Debug.Log("scoket init 1");
            socket.Closed -= CloseSocket;
            Debug.Log("scoket init 2");
            socket.Connected += Socket_Connected;
            Debug.Log("scoket init 1");
            socket.Closed += CloseSocket;
            socket.ReceivedError += (err) => {
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
            Debug.Log("Socket closed" + APIController.instance.isOnline);
            //while(!APIController.instance.isOnline)
            //{
            //    await UniTask.Delay(100);
            //}
            await OpenSocket();
        }

        private void Socket_Connected()
        {
            Debug.Log("socket connected");
            APIController.instance.StartSession();
            APIController.instance.GetNetworkStatus(true.ToString());
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

        public async UniTask MasterLogin(Action<bool> action, bool isTryBackupServer = false)
        {
            try
            {
                client = new Client(connectionData.Scheme, isTryBackupServer ? connectionData.backupHost : connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
                CustomMobileLogin("2222222222", (async (status, message) =>
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



        bool isRunMaster = false;
        public async void ValidateRPC(string rpc, string payload, Action<string> action)
        {
            Debug.Log(rpc + "start validation ... 1");
            await UniTask.Delay(7000);
            bool haveInternet = false;
            while (!haveInternet)
            {
                Debug.Log(rpc + "ValidateRPC While Loop Entered");

                APIController.instance.CheckInternetandProcess((success) =>
                {
                    Debug.Log(rpc + "ValidateRPC While Loope CheckInternetandProcess ");

                    if (success)
                    {
                        Debug.Log(rpc + "ValidateRPC While Loope success ");

                        haveInternet = true;
                    }
                    else
                    {
                        haveInternet = false;
                    }
                    Debug.Log(rpc + "ValidateRPC Checking Internet HaveInternet " + haveInternet);

                });
                //int count = 0;
                //while (!haveInternet && count < 15)
                //{
                //    await UniTask.Delay(100);
                //    count++;
                //}

                await UniTask.Delay(100);

                Debug.Log("CashoutBtnFn While Loope Completed ");

            }
            Debug.Log("start validation ... 3");
            if (isNeedTOCheck)
            {
                Debug.Log("start validation ... 4");
                SendRPC("rpc_ValidateMatch", payload, action);
                Debug.Log("start validation ... 5");

            }
        }
        bool isNeedTOCheck = false;
        public async void SendRPC(string rpc, string payload, Action<string> action)
        {
            try
            {
                if (client == null || session == null)
                {
                    if (!isRunMaster)
                    {
                        isRunMaster = true;
                    }
                    await MasterLogin((success) => {
                        isRunMaster = false;
                    });
                    while (isRunMaster)
                    {
                        await UniTask.Delay(500);
                    }
                }
                //                    action.Invoke(null);
                EncryptedPayload encryptpayload = new EncryptedPayload();
                encryptpayload.data = EncryptString(payload);
                encryptpayload.value = EncryptString(string.IsNullOrWhiteSpace(APIController.instance.userDetails.operatorDomainUrl) ? "" : APIController.instance.userDetails.operatorDomainUrl);
                if (rpc == "rpc_CreateAndJoin")
                {
                    isNeedTOCheck = true;
                    ValidateRPC(rpc, payload, action);
                }
                Debug.Log(rpc + " send ::_" + payload);
                var output = await client.RpcAsync(session, EncryptString(rpc),
                encryptpayload.ToJson(), retryConfiguration);
                if (rpc == "rpc_CreateAndJoin")
                {
                    isNeedTOCheck = false;
                }
                encryptpayload = JsonConvert.DeserializeObject<EncryptedPayload>(output.Payload);
                string outputPayload = DecryptString(encryptpayload.data);
                Debug.Log(rpc + " recived ::_" + outputPayload);
                action.Invoke(outputPayload);
#if UNITY_WEBGL && !UNITY_EDITOR
                APIController.GetUpdatedBalance();
#endif
            }
            catch (Exception ex)
            {
                action.Invoke(ex.Message);
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
