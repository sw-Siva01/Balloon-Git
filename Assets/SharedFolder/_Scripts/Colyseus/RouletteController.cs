using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class RouletteController : MonoBehaviour
{
    public string gameID = "roulette";
    private NetworkClient _networkClient;
    private float pingStartTime;
    private float LambdaStartTime;

    public string lambdabodyTest = "";

    private async void Start()
    {

        while (_networkClient == null)
        {
            _networkClient = GetComponent<NetworkClient>();
            await UniTask.Delay(100);
        }

        //_networkClient.GameName = gameID;
        _networkClient.OnRawMessageReceived += HandleServerMessage;
        //_ = ConnectAndSetup();
    }


    public async UniTask ConnectAndSetup()
    {
        // await _networkClient.JoinOrCreateGame();

        //InvokeRepeating(nameof(SendHeartbeat), 5f, 10f);
    }


    private void HandleServerMessage(string rawJson)
    {
        try
        {
            var wrapper = JsonConvert.DeserializeObject<ServerResponseWrapper>(rawJson);
            if (wrapper == null) return;

            switch (wrapper.payload.OpCode)
            {
                case "Heartbeat":
                    {
                        float pingMs = (Time.realtimeSinceStartup - pingStartTime) * 1000f;
                        Debug.Log($"[Heartbeat] Ping: {pingMs:F1} ms");
                    }
                    break;

                case "GameResponse":
                    {
                        // Example: for roulette spin result
                        var gameResponse = JsonConvert.DeserializeObject<RouletteSpinResult>(wrapper.payload.Message);
                        Debug.Log($"[Roulette] Spin result: {gameResponse.prediction}");
                    }
                    break;

                case "LambdaResponse":
                    {
                        // Placeholder for lambda responses
                        float pingMs = (Time.realtimeSinceStartup - LambdaStartTime) * 1000f;
                        Debug.Log($"[LambdaResponse] {pingMs:F1} ms : {wrapper.payload.Message}");
                    }
                    break;

                default:
                    Debug.LogWarning("Unknown OpCode: " + wrapper.payload.OpCode);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse server message: " + ex.Message);
        }
    }

    public void SendSpinRequest()
    {
        var bodyDict = new Dictionary<string, string>
        {
            { "gameId", "roulette" },
            { "gameAction", "spin" },
            { "body", "{}" }
        };
        string payload = JsonConvert.SerializeObject(bodyDict);
        WSMessage msg = new WSMessage("gameservice", payload);
        _networkClient.SendClientMsg(msg);
    }

    private void SendHeartbeat()
    {
        pingStartTime = Time.realtimeSinceStartup;

        WSMessage heartbeatMessage = new WSMessage("heartbeat", "{}");
        _networkClient.SendClientMsg(heartbeatMessage);
    }

    public void TestLambda()
    {
        LambdaStartTime = Time.realtimeSinceStartup;
        WSMessage lambdaMessage = new WSMessage("lambda", lambdabodyTest);
        _networkClient.SendClientMsg(lambdaMessage);
    }



    [Serializable]
    public class RouletteSpinResult
    {
        public int prediction;
    }
}
