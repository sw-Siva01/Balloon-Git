using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;
using Newtonsoft.Json.Linq;

public class Cryptography
{
    public static List<char> cryptocharacters = new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray());
    private static readonly string key = "fghdesjkhguerhgjndfkngKLJDFHJhdsf";
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
        //InitCharacters();
        //Debug.Log(new string(cryptocharacters.ToArray()));
        //Debug.Log("data is :: " + data);
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string encryptedData = Convert.ToBase64String(bytes);
        //Debug.Log("base64 is :: " + encryptedData);
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
        //Debug.Log("Encoded base64 is :: " + encryptedData);
        string base64Decoded = Decryptbase64String(encryptedData);
        //Debug.Log("Base64 decoded is :: " + base64Decoded);
        byte[] bytes = Convert.FromBase64String(base64Decoded);
        string originalData = Encoding.UTF8.GetString(bytes);
        //Debug.Log("Original data is :: " + originalData);
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

}
