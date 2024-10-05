using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
public class PostBuildProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        return;

        if (target == BuildTarget.WebGL)
        {
            // Path to the index.html file
            string indexHtmlPath = Path.Combine(pathToBuiltProject, "index.html");
            // Path to the new index.php file
            string indexPhpPath = Path.Combine(pathToBuiltProject, "index.php");
            // Check if the index.html file exists
            if (File.Exists(indexHtmlPath))
            {
                // Rename the file to index.php
                File.Move(indexHtmlPath, indexPhpPath);
                Debug.Log("Renamed index.html to index.php successfully.");
            }
            else
            {
                Debug.LogError("index.html file not found.");
            }
        }
    }
}