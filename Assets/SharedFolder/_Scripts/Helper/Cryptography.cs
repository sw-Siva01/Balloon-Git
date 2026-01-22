using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using GameDevWare.Serialization;

public class Cryptography
{
    public static List<char> cryptocharacters = new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray());
    private static readonly string key = "fghdesjkhguerhgjndfkngKLJDFHJhdsf";

    private static string sharedSecret = "my_shared_secret";
    //public static void InitCharacters()
    //{
    //    for (int i = 0; i < 26; i++)
    //    {
    //        cryptocharacters.Add((char)('a' + i));
    //    }
    //    for (int i = 0; i < 26; i++)
    //    {
    //        cryptocharacters.Add((char)('A' + i));
    //    }
    //    for (int i = 0; i < 10; i++)
    //    {
    //        cryptocharacters.Add((char)('0' + i));
    //    }
    //}

    public static string EncryptStr(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string encryptedData = Convert.ToBase64String(bytes);
        encryptedData = Encryptbase64String(encryptedData);
        return encryptedData;
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

        }
        return new string(buffer);
    }

    public static string DecryptStr(string encryptedData)
    {
        //DebugHelper.Log("Encoded base64 is :: " + encryptedData);
        string base64Decoded = Decryptbase64String(encryptedData);
        //DebugHelper.Log("Base64 decoded is :: " + base64Decoded);
        byte[] bytes = Convert.FromBase64String(base64Decoded);
        string originalData = Encoding.UTF8.GetString(bytes);
        //DebugHelper.Log("Original data is :: " + originalData);
        return originalData;
    }

    public static string Decryptbase64String(string encryptedText)
    {
        int shift = 0;
        char[] buffer = encryptedText.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (cryptocharacters.Contains(c))
            {
                shift = key[i % key.Length];
                int value = (cryptocharacters.IndexOf(c) - cryptocharacters.IndexOf((char)shift));
                if (value < 0)
                {
                    value += cryptocharacters.Count;
                }
                c = cryptocharacters[value];
                buffer[i] = c;
            }
        }
        return new string(buffer);
    }


    public static string GetEncryptedData(string message)
    {
        try
        {
            HMacResponse response = Json.Deserialize<HMacResponse>(DecryptStr(message));
            if (string.IsNullOrWhiteSpace(response.hmac))
            {
                return response.data;
            }
            bool valid = VerifyHMAC(response.data, response.hmac, sharedSecret);
            if (valid)
            {
                return response.data;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Parsing Data: " + ex);
            return null;
        }
    }


    private static bool VerifyHMAC(string token, string hmacReceived, string key)
    {
        string computedHmac = ComputeHMAC(token, key);
        return computedHmac == hmacReceived;
    }

    private static string ComputeHMAC(string message, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] msgBytes = Encoding.UTF8.GetBytes(message);

        using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
        {
            byte[] hash = hmac.ComputeHash(msgBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }


    public static string SetEncryptedData(string data, bool isSecureData = false)
    {
        HMacResponse response = new HMacResponse();
        response.data = data;
        response.hmac = "";
        if (isSecureData)
        {
            response.hmac = ComputeHMAC(data, sharedSecret);
        }
        Console.WriteLine("HMAC is :: " + response.hmac);
        return EncryptStr(JsonConvert.SerializeObject(response));
    }


}
[Serializable]
public class HMacResponse
{
    public string data;
    public string hmac;
}