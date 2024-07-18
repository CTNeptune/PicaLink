using UnityEngine;
using System.IO;

public class SpawnSaveButtons : MonoBehaviour
{
    public GameObject folderButtonPrefab;
    public GameObject puzzleButtonPrefab;
    public PuzzleManager puzzleManager;
    public GridPopulator gridPopulator;

    public string savePath;

    public GameObject newFolderParent, savePuzzleParent, confirmBox;

    public void LoadFolderList()
    {
        if (transform.childCount != 0)
        {
            for (int i = 0; i <= transform.childCount - 1; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        string filePath = Application.dataPath + "/StreamingAssets/";
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        for (int i = 0; i <= directoryInfo.GetDirectories().Length - 1; i++)
        {
            Debug.Log(directoryInfo.GetDirectories()[i].Name);
            SaveButton saveButton = Instantiate(folderButtonPrefab).GetComponent<SaveButton>();
            string buttonName = directoryInfo.GetDirectories()[i].Name;
            saveButton.name = buttonName;
            saveButton.buttonText.text = buttonName;
            saveButton.puzzleManager = puzzleManager;
            saveButton.gridPopulator = gridPopulator;
            saveButton.transform.SetParent(transform);
            saveButton.transform.localScale = Vector3.one;
            saveButton.folder = directoryInfo.GetDirectories()[i].Name;
            saveButton.spawnSaveButtons = this;
        }
        savePuzzleParent.SetActive(false);
        newFolderParent.SetActive(true);
    }

    public void LoadPuzzleList(string folderName)
    {
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        string filePath = Application.dataPath + "/StreamingAssets/" + folderName;
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        for (int i = 0; i <= directoryInfo.GetFiles("*.json").Length - 1; i++)
        {
            Debug.Log(directoryInfo.GetFiles("*.json")[i].Name);
            SaveButton saveButton = Instantiate(puzzleButtonPrefab).GetComponent<SaveButton>();
            string buttonName = directoryInfo.GetFiles("*.json")[i].Name;
            buttonName = buttonName.Remove(buttonName.Length - 5, 5);
            saveButton.name = buttonName;
            saveButton.buttonText.text = buttonName;
            saveButton.puzzleManager = puzzleManager;
            saveButton.gridPopulator = gridPopulator;
            saveButton.transform.SetParent(transform);
            saveButton.transform.localScale = Vector3.one;
            saveButton.folder = folderName;
            saveButton.confirmBox = confirmBox;
        }
        savePuzzleParent.SetActive(true);
        newFolderParent.SetActive(false);
    }

    public void RefreshPuzzleList()
    {
        LoadPuzzleList(savePath);
    }

    public void UpdatePath(string pathName)
    {
        savePath = pathName;
    }

    public void CreateFolder()
    {
        string filePath = Application.dataPath + "/StreamingAssets/";
        Directory.CreateDirectory(filePath + savePath);
        LoadFolderList();
    }

    public void Save()
    {
        puzzleManager.SavePuzzleManager(savePath, false, confirmBox);
    }

    public void ForceSave()
    {
        puzzleManager.SavePuzzleManager(savePath, true, confirmBox);
    }
}
