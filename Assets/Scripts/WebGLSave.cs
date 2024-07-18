using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

public class WebGLSave : MonoBehaviour
{
    PuzzleEditor puzzleEditor;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

    public void OnFileDownload() {
        Debug.Log("File Successfully Downloaded");
    }
#endif

    public void StartSave()
    {
        if (!puzzleEditor)
        {
            puzzleEditor = FindObjectOfType<PuzzleEditor>();
        }
        if (puzzleEditor)
        {
            puzzleEditor.puzzleManager.correctPaths = puzzleEditor.placedPaths;
            SavePuzzleExternal(JsonUtility.ToJson(puzzleEditor.puzzleManager));
        }
    }

    void SavePuzzleExternal(string puzzleJSON)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var bytes = Encoding.UTF8.GetBytes(puzzleJSON);
        DownloadFile(gameObject.name, "OnFileDownload", puzzleEditor.puzzleManager.puzzleName + ".json", bytes, bytes.Length);
#else
        
#endif
    }
}
