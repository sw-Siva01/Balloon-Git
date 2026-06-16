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
    /*//public static List<char> cryptocharacters = new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray());
    private static readonly string cryptoCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly string key = "fghdesjkhguerhgjndfkngKLJDFHJhdsf";

    private static string sharedSecret = "my_shared_secret";
  
    /// <summary>
    /// Encrypts a string using a custom encryption algorithm.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Byte[] EncryptStringReturnByte(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string base64String = Convert.ToBase64String(bytes);
        string dataFrom = EncryptBase64String(base64String);
        var ByteData = Encoding.UTF8.GetBytes(dataFrom);
        return ByteData;
    }

    /// <summary>
    /// Encrypts a string and returns it as a Base64 encoded string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string EncryptStr(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string encryptedData = Convert.ToBase64String(bytes);
        encryptedData = EncryptBase64String(encryptedData);
        return encryptedData;
    }

    /// <summary>
    /// Encrypts a plain text string using a custom algorithm based on a key and crypto characters.
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    private static string EncryptBase64String(string plainText)
    {
        char[] buffer = plainText.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (cryptoCharacters.Contains(c))
            {
                int shift = key[i % key.Length];
                int value = (cryptoCharacters.IndexOf(c) + cryptoCharacters.IndexOf((char)shift));
                if (value >= cryptoCharacters.Length)
                {
                    value -= cryptoCharacters.Length;
                }
                buffer[i] = cryptoCharacters[value];
            }
        }
        return new string(buffer);
    }

    /// <summary>
    /// Decrypts a Base64 encoded string and returns the original string.
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <returns></returns>
    public static string DecryptStr(string encryptedData)
    {
        string base64Decoded = DecryptBase64String(encryptedData);
        byte[] bytes = Convert.FromBase64String(base64Decoded);
        return Encoding.UTF8.GetString(bytes);
    }


    /// <summary>
    /// Decrypts a Base64 encoded string using a custom decryption algorithm based on a key and crypto characters.
    /// </summary>
    /// <param name="encryptedText"></param>
    /// <returns></returns>
    private static string DecryptBase64String(string encryptedText)
    {
        char[] buffer = encryptedText.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (cryptoCharacters.Contains(c))
            {
                int shift = key[i % key.Length];
                int value = (cryptoCharacters.IndexOf(c) - cryptoCharacters.IndexOf((char)shift));
                if (value < 0)
                {
                    value += cryptoCharacters.Length;
                }
                buffer[i] = cryptoCharacters[value];
            }
        }
        return new string(buffer);
    }*/

    private static readonly string cryptoCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly string key = "fghdesjkhguerhgjndfkngKLJDFHJhdsf"; // Replace with your key
    private static string sharedSecret = "my_shared_secret";

    /// <summary>
    /// Encrypts a string using a custom encryption algorithm.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Byte[] EncryptStringReturnByte(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string base64String = Convert.ToBase64String(bytes);
        string dataFrom = EncryptBase64String(base64String);
        var ByteData = Encoding.UTF8.GetBytes(dataFrom);
        return ByteData;
    }

    /// <summary>
    /// Encrypts a string and returns it as a Base64 encoded string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string EncryptStr(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string encryptedData = Convert.ToBase64String(bytes);
        encryptedData = EncryptBase64String(encryptedData);
        return encryptedData;
    }

    /// <summary>
    /// Encrypts a plain text string using a custom algorithm based on a key and crypto characters.
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    private static string EncryptBase64String(string plainText)
    {
        char[] buffer = plainText.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (cryptoCharacters.Contains(c))
            {
                int shift = key[i % key.Length];
                int value = (cryptoCharacters.IndexOf(c) + cryptoCharacters.IndexOf((char)shift));
                if (value >= cryptoCharacters.Length)
                {
                    value -= cryptoCharacters.Length;
                }
                buffer[i] = cryptoCharacters[value];
            }
        }
        return new string(buffer);
    }

    /// <summary>
    /// Decrypts a Base64 encoded string and returns the original string.
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <returns></returns>
    public static string DecryptStr(string encryptedData)
    {
        string base64Decoded = DecryptBase64String(encryptedData);
        byte[] bytes = Convert.FromBase64String(base64Decoded);
        return Encoding.UTF8.GetString(bytes);
    }


    /// <summary>
    /// Decrypts a Base64 encoded string using a custom decryption algorithm based on a key and crypto characters.
    /// </summary>
    /// <param name="encryptedText"></param>
    /// <returns></returns>
    private static string DecryptBase64String(string encryptedText)
    {
        char[] buffer = encryptedText.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            if (cryptoCharacters.Contains(c))
            {
                int shift = key[i % key.Length];
                int value = (cryptoCharacters.IndexOf(c) - cryptoCharacters.IndexOf((char)shift));
                if (value < 0)
                {
                    value += cryptoCharacters.Length;
                }
                buffer[i] = cryptoCharacters[value];
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