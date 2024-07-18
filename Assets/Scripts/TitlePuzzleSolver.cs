using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitlePuzzleSolver : MonoBehaviour
{
    PuzzleManager puzzleManager;
    GridPopulator gridPopulator;

    public Transform titleButtons;
    Vector3 titleTarget;

    public List<Transform> UITransforms;
    public List<Vector3> UIUpPositions;
    public List<Vector3> UIDownPositions;

    public GameObject helperButtons;
    public GameObject editorButtons;
    public GameObject menuButton;

    public RectTransform musicButton;
    MusicController musicController;

    public TextMeshProUGUI winningText;

    Transform puzzleTransform;

    Vector2 currentRes;

    void Start()
    {
        gridPopulator = GameObject.FindObjectOfType<GridPopulator>();
        puzzleManager = GetComponent<PuzzleManager>();
        StartTitleSequenceMethod();
        currentRes = new Vector2(Screen.width, Screen.height);
        StartCoroutine(CheckResolution());
        for (int i = 0; i <= UITransforms.Count - 1; i++)
        {
            UITransforms[i].position = UIDownPositions[i];
        }
        titleButtons.position = new Vector3(titleButtons.position.x, titleButtons.position.y - (Screen.height), titleButtons.position.z);
        if (musicButton)
        {
            musicButton.anchoredPosition = new Vector3(musicButton.anchoredPosition.x + 64, 0, 0);
            musicController = GameObject.FindObjectOfType<MusicController>();
        }
    }

    public void StartTitleSequenceMethod()
    {
        StartCoroutine(StartTitleSequence());
    }

    public void SetWinningText(string sentName)
    {
        if (winningText)
        {
            winningText.text = sentName;
        }
    }

    IEnumerator CheckResolution()
    {
        while (true)
        {
            if (currentRes.x != Screen.width || currentRes.y != Screen.height)
            {
                SetUI();
                currentRes = new Vector2(Screen.width, Screen.height);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator StartTitleSequence()
    {
        StopCoroutine(HoverRoutine());

        SetUI();

        titleButtons.position = new Vector3(titleButtons.position.x, titleButtons.position.y - (Screen.height), titleButtons.position.z);

        puzzleManager = GetComponent<PuzzleManager>();

        puzzleManager.SaveGame();

        if (puzzleManager.correctPaths.Count == 0)
        {
            yield return puzzleManager.LoadSave();
            if(puzzleManager.puzzleName != "")
            {
                StopAllCoroutines();
                StartCoroutine(CheckResolution());
                helperButtons.SetActive(true);
                titleButtons.gameObject.SetActive(false);
                menuButton.SetActive(true);
                SetWinningText(puzzleManager.puzzleName);
                yield return null;
            }
        }
        puzzleManager.puzzleName = "PicaLink";
        yield return puzzleManager.LoadPuzzleManager("", false);
        gridPopulator.UpdateGrid();
        puzzleManager.ResetZoom();
        for (int i = 0; i <= puzzleManager.GetCellList().Count - 1; i++)
        {
            puzzleManager.GetCellList()[i].interactable = false;
            Destroy(puzzleManager.GetCellList()[i].GetComponent<BoxCollider2D>());
            puzzleManager.GetCellList()[i].GetComponent<CellClass>().enabled = false;
        }

        puzzleManager.enabled = false;

        StartCoroutine("FitPuzzle");
        Invoke("DelayedStart", 1);
    }

    IEnumerator FitPuzzle()
    {
        try{
            Transform topLeftCorner = puzzleManager.GetCellAtCoords(Vector2.zero).transform;
            Transform bottomRightCorner = puzzleManager.GetCellAtCoords(new Vector2(puzzleManager.gridWidth - 1, puzzleManager.gridHeight - 1)).transform;
            var upperLeftScreen = new Vector3(0, Screen.height, 0);
            var lowerRightScreen = new Vector3(Screen.width, 0, 0);
            for (int i = 0; i < 1; i++)
            {
                if (bottomRightCorner.position.x > puzzleManager.GetCamera().ScreenToWorldPoint(lowerRightScreen).x || topLeftCorner.position.x < puzzleManager.GetCamera().ViewportToWorldPoint(upperLeftScreen).x)
                {
                    i--;
                    //Debug.Log(bottomRightCorner.position + " " + puzzleManager.GetCamera().ScreenToWorldPoint(lowerRightScreen).x);
                    puzzleManager.GetCamera().orthographicSize += 2.0f;
                }
            }
        }
#pragma warning disable 0168
        catch (Exception e)
        {
            
        }
#pragma warning restore 0168
        yield return null;
    }

    void DelayedStart()
    {
        StartCoroutine("SolvePuzzle");
    }

    IEnumerator SolvePuzzle()
    {
        List<Path> paths = puzzleManager.correctPaths;
        bool skipAnim = false;
        for(int i = 0; i <= paths.Count - 1; i++)
        {
            for(int j = 0; j <= paths[i].pathCoords.Count - 1; j++)
            {
                puzzleManager.GetCellAtCoords(new Vector2(paths[i].pathCoords[j].x, paths[i].pathCoords[j].y)).cellImage.color = paths[i].pathColor;
                if (Input.GetButtonDown("Fire1"))
                {
                    skipAnim = true;
                }
                if (!skipAnim)
                {
                    yield return new WaitForSeconds(0.015f);
                }
            }
        }
        gridPopulator.ForceGridOff();
        for (int i = 0; i <= puzzleManager.GetCellList().Count - 1; i++)
        {
            puzzleManager.GetCellList()[i].cellText.text = "";
        }
        Invoke("MoveToTop", 1f);
        yield return null;
    }

    void MoveToTop()
    {
        StartCoroutine("MoveToTopRoutine");
    }

    public void ResetPuzzlePosition()
    {
        if (!puzzleTransform)
        {
            puzzleTransform = GameObject.Find("PuzzleObject").transform;
        }
        puzzleTransform.position = Vector3.zero;
    }

    IEnumerator MoveToTopRoutine()
    {
        if (!puzzleTransform)
        {
            puzzleTransform = GameObject.Find("PuzzleObject").transform;
        }
        Vector3 worldPoint = puzzleManager.GetCamera().ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height - Screen.height / 10, 0));
        float lerpTime = 1f;
        float currentLerpTime = 0f;
        while (puzzleTransform.position.y <= worldPoint.y)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > lerpTime)
            {
                currentLerpTime = lerpTime;
            }
            float t = currentLerpTime / lerpTime;
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            puzzleTransform.position = Vector3.Lerp(puzzleTransform.position, new Vector3(puzzleTransform.position.x, worldPoint.y, 0f), t);
            titleButtons.gameObject.SetActive(true);
            titleButtons.position = Vector3.Lerp(titleButtons.position, new Vector3(titleButtons.position.x, titleTarget.y, 0f), t);
            if (worldPoint.y - puzzleTransform.position.y <= 0)
            {
                StartCoroutine("HoverRoutine");
                yield return null;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    IEnumerator HoverRoutine()
    {
        if (musicController)
        {
            musicController.PlaySong();
            StartCoroutine(ShowMusicButton());
            musicController = null;
        }
        Transform puzzleTransform = GameObject.Find("PuzzleObject").transform;
        Vector3 origPos = puzzleTransform.position;
        float scale = 3f;
        float timeSinceStarted = 0f;
        while (true)
        {
            timeSinceStarted += Time.deltaTime;
            float distance = Mathf.Abs(Mathf.Sin(timeSinceStarted * scale));
            puzzleTransform.position = origPos + Vector3.up * distance * scale;
            yield return new WaitForEndOfFrame();
        }
    }

    void SetUI()
    {
        if (titleButtons)
        {
            if (titleTarget == null)
            {
                titleTarget = titleButtons.position;
            }
        }
        UIUpPositions.Clear();
        UIDownPositions.Clear();
        Debug.Log(Screen.orientation + " " + Screen.width + " " + Screen.height);
        for (int i = 0; i <= UITransforms.Count - 1; i++)
        {
            if(UIUpPositions.Count != UITransforms.Count)
            {
                UIUpPositions.Add(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            }
            UIDownPositions.Add(new Vector3(UIUpPositions[i].x, UIUpPositions[i].y - (Screen.width + Screen.height), UITransforms[i].position.z));
        }
        puzzleManager.ResetZoom();
        StartCoroutine(FitPuzzle());
    }

    public void HideUIElement(Transform uiTransform)
    {
        Vector3 positionDown = Vector3.zero;
        for (int i = 0; i <= UITransforms.Count - 1; i++)
        {
            if (UITransforms[i] == uiTransform)
            {
                positionDown = UIDownPositions[i];
            }
        }
        StartCoroutine(MoveUI(uiTransform, positionDown));
    }

    IEnumerator ShowMusicButton()
    {
        if (musicButton)
        {
            float lerpTime = 1f;
            float currentLerpTime = 0f;
            while (currentLerpTime < 1f)
            {
                currentLerpTime += Time.deltaTime;
                if (currentLerpTime > lerpTime)
                {
                    currentLerpTime = lerpTime;
                }
                float t = currentLerpTime / lerpTime;
                t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                musicButton.anchoredPosition = Vector3.Lerp(musicButton.anchoredPosition, new Vector3(musicButton.rect.width, 0f, 0f), t);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }
    }

    public void ToggleUIElement(Transform uiTransform)
    {
        Vector3 positionUp = Vector3.zero;
        Vector3 positionDown = Vector3.zero;
        for (int i = 0; i <= UITransforms.Count - 1; i++)
        {
            if(UITransforms[i] == uiTransform)
            {
                positionUp = UIUpPositions[i];
                positionDown = UIDownPositions[i];
                break;
            }
        }
        if (!uiTransform.gameObject.activeSelf)
        {
            uiTransform.gameObject.SetActive(true);
        }
        IEnumerator coroutine;
        if (positionUp.y - uiTransform.position.y == 0)
        {
            coroutine = MoveUI(uiTransform, positionDown);
        }
        else
        {
            coroutine = MoveUI(uiTransform, positionUp);
        }
        StartCoroutine(coroutine);
    }

    IEnumerator MoveUI(Transform uiElement, Vector3 target)
    {
        float lerpTime = 1f;
        float currentLerpTime = 0f;
        while (currentLerpTime < 1f)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > lerpTime)
            {
                currentLerpTime = lerpTime;
            }
            float t = currentLerpTime / lerpTime;
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            uiElement.position = Vector3.Lerp(uiElement.position, new Vector3(uiElement.position.x, target.y, 0f), t);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
