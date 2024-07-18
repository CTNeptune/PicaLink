using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

public class AndroidBuildPrep : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/StreamingAssets/folders.txt", false))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/StreamingAssets/");
            for (int i = 0; i <= directoryInfo.GetDirectories().Length - 1; i++)
            {
                string directoryName = directoryInfo.GetDirectories()[i].Name;
                if(directoryName == "Music")
                {
                    directoryName = directoryName.ToLower();
                }
                sw.WriteLine(directoryName);
                using (StreamWriter fsw = new StreamWriter(Application.dataPath + "/StreamingAssets/" + directoryName + ".txt", false))
                {
                    DirectoryInfo subDirInfo = new DirectoryInfo(Application.dataPath + "/StreamingAssets/" + directoryName + "/");
                    if(directoryName != "music")
                    {
                        for (int j = 0; j <= subDirInfo.GetFiles("*.json").Length - 1; j++)
                        {
                            string puzzleName = subDirInfo.GetFiles("*.json")[j].Name;
                            fsw.WriteLine(puzzleName);
                        }
                    }
                    else
                    {
                        for (int j = 0; j <= subDirInfo.GetFiles("*").Length - 1; j++)
                        {
                            if (!subDirInfo.GetFiles("*")[j].Name.Contains(".meta"))
                            {
                                fsw.WriteLine(subDirInfo.GetFiles("*")[j].Name);
                            }
                        }
                    }
                    fsw.Close();
                }
            }
            sw.Close();
        }
    }
}
#endif
public class SpawnLoadButtons : MonoBehaviour
{
    public List<Level> levels;
    public GameObject folderButtonPrefab;
    public GameObject puzzleButtonPrefab;
    public PuzzleManager puzzleManager;
    public GridPopulator gridPopulator;

    private string[] lines;
    private PuzzleEditor puzzleEditor;

    bool cancelLoad = false;

    private void Awake()
    {
        puzzleEditor = GameObject.FindObjectOfType<PuzzleEditor>();
    }

    public void LoadFolderList()
    {
        cancelLoad = true;
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        foreach (Level level in levels)
            CreateLoadButton(level);

        lines = File.ReadAllLines(Application.persistentDataPath + "/picaLinkSave.txt");
        /*
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
        StopAllCoroutines();
        StartCoroutine(WebLoadFolderList());
#else
        if (transform.childCount != 0)
        {
            for (int i = 0; i <= transform.childCount - 1; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        string filePath = System.IO.Path.Combine(Application.dataPath, "StreamingAssets");
#if UNITY_IOS
        filePath = Application.streamingAssetsPath;
#endif
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        for (int i = 0; i <= directoryInfo.GetDirectories().Length - 1; i++)
        {
            if(directoryInfo.GetDirectories()[i].Name != "music")
            {
                CreateLoadButton(directoryInfo.GetDirectories()[i].Name, false);
            }
        }
        //Custom puzzle loader
#endif
        */
#if UNITY_STANDALONE || UNITY_WEBGL
        CreateLoadButton("Custom Puzzle...", true);
#endif
    }

    IEnumerator WebLoadFolderList()
    {
        if (transform.childCount != 0)
        {
            for (int i = 0; i <= transform.childCount - 1; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        string filePath = "";
        List<string> folders = new List<string>();
#if UNITY_WEBGL
        filePath = Application.streamingAssetsPath; //Application.dataPath + "/StreamingAssets/";
        if (filePath.Contains("ssl.hwcdn.net"))
        {
            filePath = Application.streamingAssetsPath + "/folders.txt";
            using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath))
            {
                yield return webRequest.SendWebRequest();
                string result = webRequest.downloadHandler.text;
                StringReader stringReader = new StringReader(result);
                string line;
                int lineCount = 0;
                while ((line = stringReader.ReadLine()) != null)
                {
                    lineCount++;
                    if(line != "music")
                    {
                        folders.Add(line);
                    }
                }
                stringReader.Close();
                Debug.Log(lineCount + " folders found.");
            }
        }
        else
        {
            using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
                MatchCollection matches = regex.Matches(webRequest.downloadHandler.text);

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    foreach (Match match in matches)
                    {
                        if (!match.Success) { continue; }
                        Debug.Log(match.Groups["name"]);
                        if (!match.Groups["name"].Value.Contains("Parent Directory") && !match.Groups["name"].Value.Contains("Description") && !match.Groups["name"].Value.Contains(".json") && !match.Groups["name"].Value.Contains(".txt") && !match.Groups["name"].Value.Contains("Music"))
                        {
                            string tempString = match.Groups["name"].Value;
                            //tempString = tempString.Substring(1, tempString.Length - 2);
                            //tempString = tempString.Substring(0, tempString.Length - 1);
                            folders.Add(tempString);
                        }
                    }
                }
            }
        }
#endif
#if UNITY_ANDROID
        filePath = filePath = Application.streamingAssetsPath + "/folders.txt";
        using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath))
        {
            yield return webRequest.SendWebRequest();
            string result = webRequest.downloadHandler.text;
            StringReader stringReader = new StringReader(result);
            string line;
            int lineCount = 0;
            while((line = stringReader.ReadLine()) != null)
            {
                lineCount++;
                folders.Add(line);
            }
            stringReader.Close();
            Debug.Log(lineCount + " folders found.");
        }
#endif
        for (int i = 0; i <= folders.Count - 1; i++)
        {
            CreateLoadButton(folders[i], false);
        }
        //Custom puzzle loader
#if UNITY_WEBGL
        CreateLoadButton("Custom Puzzle...", true);
#endif
        cancelLoad = false;
        yield return null;
    }

    void CreateLoadButton(Level level)
    {
        LoadButton loadButton = Instantiate(folderButtonPrefab).GetComponent<LoadButton>();
        loadButton.name = level._LevelName;
        loadButton.level = level;
        loadButton.puzzleManager = puzzleManager;
        loadButton.gridPopulator = gridPopulator;
        loadButton.transform.SetParent(transform);
        loadButton.transform.localScale = Vector3.one;
        loadButton.isCustom = false;
        loadButton.buttonText.text = level._LevelName;
    }

    void CreateLoadButton(string buttonName, bool isCustom)
    {
        LoadButton loadButton = Instantiate(folderButtonPrefab).GetComponent<LoadButton>();
        loadButton.name = buttonName;
#if UNITY_WEBGL
        if (Application.streamingAssetsPath.Contains("theneptune"))
        {
            loadButton.buttonText.text = buttonName.Substring(0, buttonName.Length - 1);
        }
        else
        {
            loadButton.buttonText.text = buttonName;
        }
#endif
        loadButton.buttonText.text = buttonName;
        loadButton.puzzleManager = puzzleManager;
        loadButton.gridPopulator = gridPopulator;
        loadButton.transform.SetParent(transform);
        loadButton.transform.localScale = Vector3.one;
        loadButton.isCustom = isCustom;
    }

    public void LoadPuzzleList(string folderName)
    {
        
        /*
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
        StartCoroutine(WebLoadPuzzleList(folderName));
#else
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        string filePath = Application.dataPath + "/StreamingAssets/" + folderName;
#if UNITY_IOS
        filePath = System.IO.Path.Combine(Application.streamingAssetsPath, folderName);
#endif
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        cancelLoad = false;
        StartCoroutine(AsynchLoadPuzzles(directoryInfo, folderName));
#endif
        */
    }

    IEnumerator AsynchLoadPuzzles(DirectoryInfo directoryInfo, string folderName)
    {
        int i = 0;
        if (!File.Exists(Application.persistentDataPath + "/picaLinkSave.txt"))
        {
            Debug.Log("No save file, creating one now...");
            File.Create(Application.persistentDataPath + "/picaLinkSave.txt");
        }
        while (i <= directoryInfo.GetFiles("*.json").Length - 1)
        {
            if (cancelLoad == true)
            {
                cancelLoad = false;
                break;
            }
            //Debug.Log(directoryInfo.GetFiles("*.json")[i].Name);
            LoadButton loadButton = Instantiate(puzzleButtonPrefab).GetComponent<LoadButton>();
            string buttonName = directoryInfo.GetFiles("*.json")[i].Name;
            buttonName = buttonName.Remove(buttonName.Length - 5, 5);
            loadButton.name = buttonName;

            loadButton.buttonText.text = buttonName;
            loadButton.puzzleManager = puzzleManager;
            loadButton.gridPopulator = gridPopulator;
            loadButton.transform.SetParent(transform);
            loadButton.transform.localScale = Vector3.one;
            loadButton.folder = folderName;
            if (puzzleEditor)
            {
                //Debug.Log(buttonName + " is complete.");
                loadButton.buttonText.text = "";
                //Set the button's image to the image we just made, and destroy the temporary puzzle manager.
                loadButton.GetComponent<UnityEngine.UI.Image>().sprite = CreateSprite(loadButton, File.ReadAllText(directoryInfo.GetFiles("*.json")[i].FullName));
            }
            else
            {
                for(int j = 0; j <= lines.Length - 1; j++)
                {
                    if (lines[j] == folderName + "/" + buttonName)
                    {
                        //Debug.Log(buttonName + " is complete.");
                        loadButton.buttonText.text = "";
                        //Set the button's image to the image we just made, and destroy the temporary puzzle manager.
                        loadButton.GetComponent<UnityEngine.UI.Image>().sprite = CreateSprite(loadButton, File.ReadAllText(directoryInfo.GetFiles("*.json")[i].FullName));
                    }
                }
            }
            i++;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private Sprite CreateSprite(LoadButton loadButton, string fullName)
    {
        //Create a puzzle manager and load the puzzle from it into a temporary component
        PuzzleManager tempPuzzleManager = loadButton.gameObject.AddComponent<PuzzleManager>();
        JsonUtility.FromJsonOverwrite(fullName, tempPuzzleManager);
        //Create an image that's as wide and high as the puzzle, and set it to use point filtering
        Texture2D puzzleImage = new Texture2D(tempPuzzleManager.gridWidth, tempPuzzleManager.gridHeight);
        puzzleImage.filterMode = FilterMode.Point;
        //Initialize the image with a white color
        Color[] tempPixels = new Color[puzzleImage.GetPixels().Length];
        for (int k = 0; k <= tempPixels.Length - 1; k++)
        {
            tempPixels[k] = Color.white;
        }
        puzzleImage.SetPixels(tempPixels);
        //Draw the correct paths on the image with each correct path's color
        for (int k = 0; k <= tempPuzzleManager.correctPaths.Count - 1; k++)
        {
            for (int l = 0; l <= tempPuzzleManager.correctPaths[k].pathCoords.Count - 1; l++)
            {
                puzzleImage.SetPixel(Mathf.RoundToInt(tempPuzzleManager.correctPaths[k].pathCoords[l].x), Mathf.RoundToInt(tempPuzzleManager.correctPaths[k].pathCoords[l].y), tempPuzzleManager.correctPaths[k].pathColor);
            }
        }
        Destroy(tempPuzzleManager);
        //Flip the image, then apply
        Color[] flippedImage = puzzleImage.GetPixels();
        for (int k = 0; k <= puzzleImage.width - 1; k++)
        {
            try
            {
                System.Array.Reverse(flippedImage, k * puzzleImage.width, puzzleImage.width);
            }
            catch (Exception)
            {
                //Debug.LogWarning(k + " " + puzzleImage.width + " " + k * puzzleImage.width);
            }
        }
        System.Array.Reverse(flippedImage, 0, flippedImage.Length);
        puzzleImage.SetPixels(flippedImage);
        puzzleImage.Apply();
        return Sprite.Create(puzzleImage, new Rect(0f, 0f, puzzleImage.width, puzzleImage.height), new Vector2(0.5f, 0.5f), 100f);
    }

    IEnumerator WebLoadPuzzleList(string pathName)
    {
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        string filePath = "";
        PuzzleEditor puzzleEditor = GameObject.FindObjectOfType<PuzzleEditor>();
#if UNITY_WEBGL
        filePath = Application.streamingAssetsPath + "/" + pathName;
#endif
#if UNITY_ANDROID
        filePath = Application.streamingAssetsPath + "/" + pathName;
#endif
        List<string> files = new List<string>();
#if UNITY_WEBGL
        if (Application.streamingAssetsPath.Contains("ssl.hwcdn.net"))
        {
            using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath + ".txt"))
            {
                yield return webRequest.SendWebRequest();
                string result = webRequest.downloadHandler.text;
                Debug.Log(result);
                StringReader stringReader = new StringReader(result);
                string line;
                int lineCount = 0;
                while ((line = stringReader.ReadLine()) != null)
                {
                    lineCount++;
                    files.Add(line);
                }
                stringReader.Close();
                Debug.Log(lineCount + " folders found.");
            }
        }
        else
        {
            using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
                MatchCollection matches = regex.Matches(webRequest.downloadHandler.text);

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    foreach (Match match in matches)
                    {
                        if (!match.Success) { continue; }
                        //Debug.Log(match.Groups["name"]);
                        if (!match.Groups["name"].Value.Contains("Parent Directory") && !match.Groups["name"].Value.Contains("Description") && match.Groups["name"].Value.Contains(".json"))
                        {
                            string tempString = match.Groups["name"].Value;
                            //tempString = tempString.Substring(1, tempString.Length - 1);
                            //tempString = tempString.Substring(0, tempString.Length-1);
                            files.Add(tempString);
                        }
                    }
                }
            }
        }
#endif
#if UNITY_ANDROID
        using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath + ".txt"))
        {
            yield return webRequest.SendWebRequest();
            string result = webRequest.downloadHandler.text;
            Debug.Log(result);
            StringReader stringReader = new StringReader(result);
            string line;
            int lineCount = 0;
            while ((line = stringReader.ReadLine()) != null)
            {
                lineCount++;
                files.Add(line);
            }
            stringReader.Close();
            Debug.Log(lineCount + " folders found.");
        }
#endif
        for (int i = 0; i <= files.Count - 1; i++)
        {
            using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get(filePath + "/" + files[i]))
            {
                yield return webRequest.SendWebRequest();
                //Debug.Log(filePath + files[i]);
                LoadButton loadButton = Instantiate(puzzleButtonPrefab).GetComponent<LoadButton>();
                string buttonName = files[i];
                buttonName = buttonName.Remove(buttonName.Length - 5, 5);
                loadButton.name = buttonName;
                loadButton.buttonText.text = buttonName;
                loadButton.puzzleManager = puzzleManager;
                loadButton.gridPopulator = gridPopulator;
                loadButton.transform.SetParent(transform);
                loadButton.transform.localScale = Vector3.one;
                loadButton.folder = pathName;
                string urlPath = filePath.Remove(0, Application.streamingAssetsPath.Length + 1);
                if (puzzleEditor)
                {
                    loadButton.buttonText.text = "";
                    //Set the button's image to the image we just made, and destroy the temporary puzzle manager.
                    loadButton.GetComponent<UnityEngine.UI.Image>().sprite = CreateSprite(loadButton, webRequest.downloadHandler.text);
                }
                else
                {
                    for (int j = 0; j <= lines.Length - 1; j++)
                    {
                        if (lines[j] + ".json" == urlPath + 
#if UNITY_ANDROID
                            "/" + 
#endif
                            files[i] || puzzleEditor)
                        {
                            loadButton.buttonText.text = "";
                            //Set the button's image to the image we just made, and destroy the temporary puzzle manager.
                            loadButton.GetComponent<UnityEngine.UI.Image>().sprite = CreateSprite(loadButton, webRequest.downloadHandler.text);
                        }
                    }
                }
            }
        }
        yield return null;
    }

    internal void LoadPuzzleList(Level level)
    {
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        foreach (var puzzle in level._Levels)
        {
            LoadButton loadButton = Instantiate(puzzleButtonPrefab).GetComponent<LoadButton>();
            string buttonName = puzzle.name;
            loadButton.name = buttonName;

            loadButton.buttonText.text = buttonName;
            loadButton.puzzleManager = puzzleManager;
            loadButton.gridPopulator = gridPopulator;
            loadButton.transform.SetParent(transform);
            loadButton.transform.localScale = Vector3.one;
            loadButton.puzzle = puzzle;

            if (puzzleEditor)
            {
                //Debug.Log(buttonName + " is complete.");
                loadButton.buttonText.text = "";
                //Set the button's image to the image we just made, and destroy the temporary puzzle manager.
                loadButton.GetComponent<UnityEngine.UI.Image>().sprite = CreateSprite(loadButton, puzzle.ToString());
            }
            else
            {
                for (int j = 0; j <= lines.Length - 1; j++)
                {
                    if (lines[j] == level._LevelName + "/" + buttonName)
                    {
                        //Debug.Log(buttonName + " is complete.");
                        loadButton.buttonText.text = "";
                        //Set the button's image to the image we just made, and destroy the temporary puzzle manager.
                        loadButton.GetComponent<UnityEngine.UI.Image>().sprite = CreateSprite(loadButton, puzzle.ToString());
                    }
                }
            }
        }
    }
}
