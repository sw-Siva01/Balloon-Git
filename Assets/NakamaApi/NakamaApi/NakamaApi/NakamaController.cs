using System;
using System.Collections;
using System.Collections.Generic;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;

public class NakamaController : MonoBehaviour
{
    public string Host;
    public string Transport;
    public int port;
    public IClient client;
    public ISession session = null;
    public static NakamaController Instance;
    private void Awake()
    {
        Instance = this;
    }
    public async void InitlizeNakamaClient()
    {
        client = new Client(Transport,Host,port,"defaultkey");
        client.Timeout = 10;
        try
        {
            session = await client.AuthenticateCustomAsync(APIController.instance.userDetails.Id);
            Debug.Log("login success");
        }
        catch(Exception ex)
        {
            Debug.Log($"InitlizeNakamaClient error ::: { ex.Message }");
        }
    }
    public async void ExecuteRPC(string RPC_Name, Dictionary<string, string> payload,Action<string> action)
    {
        try
        {
            var response = await client.RpcAsync(session,RPC_Name,payload.ToJson());
            action.Invoke(response.Payload);
            Debug.Log(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.Log($"ExecuteRPC error ::: {ex.Message}");
        }
    }
}
