using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Runtime.InteropServices;

public class LoadButton : MonoBehaviour
{
    public Level level;
    public TextAsset puzzle;
    public PuzzleManager puzzleManager;
    public GridPopulator gridPopulator;
    public TextMeshProUGUI buttonText;

    public string folder;
    private string customJSON = "";

    public bool isCustom;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
    
    public void OnFileUpload(string url)
    {
        StartCoroutine(OutputRoutine(url));
    }

    private IEnumerator OutputRoutine(string url)
    {
        var loader = new WWW(url);
        yield return loader;
        customJSON = loader.text;
        LoadPuzzle();
    }
#endif

    SpawnLoadButtons spawnLoadButtons;

    private void Start()
    {
        spawnLoadButtons = GameObject.FindObjectOfType<SpawnLoadButtons>();
    }

    public void LoadPuzzlesInFolder()
    {
        if (isCustom)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("Choosing file...");
            UploadFile(gameObject.name, "OnFileUpload", ".json", false);
            return;
#endif
            
            return;
        }
        if (spawnLoadButtons)
        {
            spawnLoadButtons.LoadPuzzleList(level);
            puzzleManager.SetPuzzleFolder(level._LevelName);
        }
    }

    public void LoadPuzzle()
    {
        StartCoroutine(LoadPuzzleCoroutine());
    }

    IEnumerator LoadPuzzleCoroutine()
    {
        yield return puzzleManager.StartCoroutine(puzzleManager.ToggleLoadingScreen(true));
        puzzleManager.ClearSave();

        if (puzzleManager.IsEditModeEnabled())
        {
            puzzleManager.ToggleEditMode();
        }
        TitlePuzzleSolver solver = puzzleManager.GetComponent<TitlePuzzleSolver>();
        if (solver)
        {
            solver = puzzleManager.GetComponent<TitlePuzzleSolver>();
            solver.StopCoroutine("HoverRoutine");
            solver.ResetPuzzlePosition();
            //Hide the loader, assumed in position 0
            solver.HideUIElement(solver.UITransforms[0]);
            puzzleManager.enabled = true;
            solver.helperButtons.SetActive(true);
            solver.titleButtons.gameObject.SetActive(false);
            solver.menuButton.SetActive(true);
            solver.SetWinningText(puzzleManager.puzzleName);
        }
        if (puzzleManager && gridPopulator)
        {
            puzzleManager.UpdatePuzzleName(gameObject.name);
            puzzleManager.StartCoroutine(puzzleManager.LoadPuzzleManager(puzzle.text));
            gridPopulator.UpdateGrid();
            puzzleManager.SetEditMode(false);
            GetComponentInParent<ScrollRect>().transform.parent.gameObject.SetActive(false);
            if (solver)
            {
                solver.SetWinningText(puzzleManager.puzzleName);
            }
        }

        yield return puzzleManager.StartCoroutine(puzzleManager.ToggleLoadingScreen(false));
        yield return null;
    }

    public IEnumerator WebLoadPuzzle()
    {
        TitlePuzzleSolver solver = puzzleManager.GetComponent<TitlePuzzleSolver>();
        if (solver)
        {
            solver.StopCoroutine("HoverRoutine");
            solver.ResetPuzzlePosition();
            //Hide the loader, assumed in position 0
            solver.HideUIElement(solver.UITransforms[0]);
            puzzleManager.enabled = true;
            solver.helperButtons.SetActive(true);
            solver.titleButtons.gameObject.SetActive(false);
            solver.menuButton.SetActive(true);
            solver.SetWinningText(puzzleManager.puzzleName);
        }
        if (puzzleManager && gridPopulator)
        {
            puzzleManager.UpdatePuzzleName(gameObject.name);
            if (isCustom)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                yield return puzzleManager.LoadPuzzleManager(customJSON);
#else
                yield return puzzleManager.LoadPuzzleManager(folder, true);
#endif
            }
            if (!isCustom)
            {
                yield return puzzleManager.LoadPuzzleManager(folder + "/", false);
            }
            gridPopulator.UpdateGrid();
            puzzleManager.SetEditMode(false);
            GetComponentInParent<ScrollRect>().transform.parent.gameObject.SetActive(false);
            if (solver)
            {
                solver.SetWinningText(puzzleManager.puzzleName);
            }
        }
        yield return null;
    }
}
