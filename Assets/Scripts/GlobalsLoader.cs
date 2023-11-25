using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO;

public class GlobalsLoader : MonoBehaviour
{
    //This script is used to load the global variables into the inspector

    [SerializeField]
    public GlobalVars globalVars;

    //Init global vars if they are null
    private void Awake()
    {
        if (globalVars == null)
        {
            globalVars = new GlobalVars();
        }
    }
    //in streamable assets, create a text file called GlobalVars.txt
    [HorizontalLine(color: EColor.Red)]

    [SerializeField] private string GlobalVarspath = "GlobalVars.json";
    [Button]
    private void SaveGlobalsToFileWithJSON()
    {
        //Write some text to the test.txt file
        string rootPath = Application.streamingAssetsPath;
        string filePath = Path.Combine(rootPath, GlobalVarspath);
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }
        string json = JsonUtility.ToJson(globalVars);
        File.WriteAllText(filePath, json);
        //print
        Debug.Log("Saved to " + filePath);

    }

    //create a button to load the global variables from a file
    [Button]
    private void LoadGlobalsFromFile()
    {
        string rootPath = Application.streamingAssetsPath;
        string filePath = Path.Combine(rootPath, GlobalVarspath);
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }
        string json = File.ReadAllText(filePath);
        globalVars = JsonUtility.FromJson<GlobalVars>(json);
    }

    //clear the global variables
    [Button]
    private void ClearGlobals()
    {
        globalVars = new GlobalVars();
    }

    //open folder in file explorer
    [Button]
    private void OpenFolder()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        string rootPath = Application.streamingAssetsPath;
        string filePath = Path.Combine(rootPath, GlobalVarspath);
        System.Diagnostics.Process.Start(rootPath);
    }
}
