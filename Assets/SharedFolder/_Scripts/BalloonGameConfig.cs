using System.Runtime.InteropServices;

public static class BalloonGameConfig
{
    public static string ServerUrl   { get; private set; } = "ws://localhost:2567";
    public static string ApiUrl      { get; private set; } = "http://localhost:3000";
    public static string SessionCode { get; private set; } = "";
    public static string Currency    { get; private set; } = "USD";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern string GetServerUrl();
    [DllImport("__Internal")] private static extern string GetApiUrl();
    [DllImport("__Internal")] private static extern string GetSessionCode();
#endif

    public static void Init()
    {
#if UNITY_EDITOR
        var code = UnityEditor.EditorPrefs.GetString("IQPlay_EditorSessionCode_Balloon", "");
        if (!string.IsNullOrEmpty(code))
            SessionCode = code;
#elif UNITY_WEBGL
        var url = GetServerUrl();
        if (!string.IsNullOrEmpty(url))
            ServerUrl = url;

        var api = GetApiUrl();
        if (!string.IsNullOrEmpty(api))
            ApiUrl = api;

        var code = GetSessionCode();
        if (!string.IsNullOrEmpty(code))
            SessionCode = code;
#endif
    }

#if UNITY_EDITOR
    public static void SetEditorSessionCode(string code)
    {
        SessionCode = code;
        UnityEditor.EditorPrefs.SetString("IQPlay_EditorSessionCode_Balloon", code);
    }
#endif

    public static void SetCurrency(string currency)
    {
        if (!string.IsNullOrEmpty(currency))
            Currency = currency;
    }

    public static bool IsB2BMode => !string.IsNullOrEmpty(SessionCode);
}
