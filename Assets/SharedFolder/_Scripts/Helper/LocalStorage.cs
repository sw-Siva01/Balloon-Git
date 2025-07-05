using System.Runtime.InteropServices;
using UnityEngine;

public static class LocalStorage
{
    [DllImport("__Internal")]
    private static extern void SaveToLocalStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern string LoadFromLocalStorage(string key);

    [DllImport("__Internal")]
    private static extern void DeleteFromLocalStorage(string key);

    [DllImport("__Internal")]
    private static extern void ClearLocalStorage();

    public static void Save(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToLocalStorage(key, value);
#else
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
#endif
    }

    public static string Load(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LoadFromLocalStorage(key);
#else
        return PlayerPrefs.GetString(key, string.Empty);
#endif
    }

    public static void Delete(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DeleteFromLocalStorage(key);
#else
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
#endif
    }

    public static void Clear()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ClearLocalStorage();
#else
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
#endif
    }
}
