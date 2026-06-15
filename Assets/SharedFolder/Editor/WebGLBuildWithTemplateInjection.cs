/*
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class WebGLBuildWithTemplateInjection : IPostprocessBuildWithReport, IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 256;

        Debug.Log("Post-build logic for WebGL triggered.");
        Dictionary<string, string> payload = new Dictionary<string, string>();
        payload["request_type"] = "GetTitleData";
        payload["game_name"] = StaticString.defaultGameName;
        string payloadJson = JsonConvert.SerializeObject(payload);
        string encryptPayload = Cryptography.EncryptStr(payloadJson);
        // Your C# runtime value
        string myValue = "<script> var titleInfo = '" + encryptPayload + "'; script.onload = function() {\r\n    if (typeof SetTitleInfo === 'function')\r\n    {\r\n        SetTitleInfo(titleInfo);  // <-- your parameter here\r\n    }\r\n    else\r\n    {\r\n        console.error(\"StartGame is not defined.\");\r\n    }\r\n}</script> </html>";


        // Example: Inject a C# value into index.html
        string buildPath = report.summary.outputPath;
        string indexPath = Path.Combine(buildPath, "index.html");

        if (File.Exists(indexPath))
        {
            string content = File.ReadAllText(indexPath);
            content = content.Replace("</html>", myValue); // your placeholder
            File.WriteAllText(indexPath, content);
            Debug.Log("Injected PLAYER_ID into index.html.");
        }
        else
        {
            Debug.LogWarning("index.html not found at " + indexPath);
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (string.IsNullOrEmpty(StaticString.defaultGameName))
        {
            throw new BuildFailedException("StaticString.defaultGameName is null or empty. Please set it before building. to solve this issue go to APIController.cs script and set APIController.defaultGameName as your gamename very you given correct gamename only otherwise you didnot recived auth data.");
        }
        Debug.Log("Pre-build check passed: StaticString.defaultGameName = " + StaticString.defaultGameName);
    }
}*/


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// WebGLBuildWithTemplateInjection is a Unity editor script that modifies the WebGL build process.
/// </summary>
public class WebGLBuildWithTemplateInjection : IPostprocessBuildWithReport, IPreprocessBuildWithReport
{
    #region INT
    public int callbackOrder => 0;
    #endregion

    /// <summary>
    /// OnPostprocessBuild is called after the build process for WebGL is completed.
    /// </summary>
    /// <param name="report"></param>
    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;


        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 256;


        Debug.Log("Post-build logic for WebGL triggered.");
        Dictionary<string, string> payload = new Dictionary<string, string>();
        payload["request_type"] = "GetTitleData";
        payload["game_name"] = StaticString.defaultGameName;
        string payloadJson = JsonConvert.SerializeObject(payload);
        string encryptPayload = Cryptography.EncryptStr(payloadJson);
        // Your C# runtime value
        string myValue = "var titleInfo = '" + encryptPayload + "';";


        // Example: Inject a C# value into index.html
        string buildPath = report.summary.outputPath;
        string indexPath = Path.Combine(buildPath, "index.js");

        if (File.Exists(indexPath))
        {
            string content = File.ReadAllText(indexPath);
            content = content.Replace("//replacetitleinfo//", myValue); // your placeholder
            File.WriteAllText(indexPath, content);
            Debug.Log("Injected TitleData into index.js.");
        }
        else
        {
            Debug.LogWarning("index.js not found at " + indexPath);
        }
    }

    /// <summary>
    /// OnPreprocessBuild is called before the build process for WebGL starts.
    /// </summary>
    /// <param name="report"></param>
    /// <exception cref="BuildFailedException"></exception>
    public void OnPreprocessBuild(BuildReport report)
    {
        if (string.IsNullOrEmpty(StaticString.defaultGameName))
        {
            throw new BuildFailedException("StaticString.defaultGameName is null or empty. Please set it before building. to solve this issue go to StaticString.cs script and set StaticString.defaultGameName as your gamename very you given correct gamename only otherwise you didnot recived auth data.");
        }
        Debug.Log("Pre-build check passed: StaticString.defaultGameName = " + StaticString.defaultGameName);
    }
}