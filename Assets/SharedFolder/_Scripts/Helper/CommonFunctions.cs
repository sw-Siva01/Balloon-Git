using System;
using UnityEngine;

public class CommonFunctions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        //DebugHelper.Log(unixTimestamp);
        RequestID = unixTimestamp.ToString();
        //RequestID = Guid.NewGuid().ToString();
    }
}

