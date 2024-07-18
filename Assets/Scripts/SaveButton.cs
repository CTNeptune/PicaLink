using UnityEngine;
using TMPro;

public class SaveButton : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public GridPopulator gridPopulator;
    public TextMeshProUGUI buttonText;

    public string folder;

    public SpawnSaveButtons spawnSaveButtons;
    public GameObject confirmBox;

    private void Start()
    {
        spawnSaveButtons = GameObject.FindObjectOfType<SpawnSaveButtons>();
    }

    public void LoadPuzzlesInFolder()
    {
        if (spawnSaveButtons)
        {
            spawnSaveButtons.savePath = folder;
            spawnSaveButtons.LoadPuzzleList(gameObject.name);
        }
    }

    public void SavePuzzle()
    {
        if (puzzleManager && gridPopulator)
        {
            puzzleManager.UpdatePuzzleName(gameObject.name);
            puzzleManager.SavePuzzleManager(folder, false, confirmBox);
        }
    }
}
