using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

[Serializable]
public class PuzzleManager : MonoBehaviour
{
    public String puzzleName;
    private String puzzleFolder;
    public int gridWidth;
    public int gridHeight;
    public List<Path> correctPaths = new List<Path>();
    public List<Path> placedPaths = new List<Path>();

    private Camera mainCamera;
    private float mouseSensitivity = 1.0f;
    private Transform puzzleTransform;
    private List<CellClass> cellList = new List<CellClass>();

    private Transform loadingScreenTransform = null;

    private Path currentPath = new Path();

    private int remainingMoves = 0;

    private float doubleClickDelay = 0.5f;
    private float clearTimer = 0f;
    private Vector2 currentPos;
    private Vector2 prevPos;

    private bool editModeEnabled;
    private PuzzleEditor puzzleEditor;

    private Transform topLeftCorner;
    private Transform bottomRightCorner;

    private Vector3 touchStart;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncFiles();
#endif

    private void Start()
    {
        SetCamera(Camera.main);
        SetLoadingTransform(GameObject.Find("LoadingCanvas/Grid").transform);
    }

    public void SetPuzzleFolder(string folderName)
    {
        puzzleFolder = folderName;
    }

    public string GetPuzzleFolder()
    {
        return puzzleFolder;
    }

    public void SetPuzzleTransform(Transform sentTransform)
    {
        puzzleTransform = sentTransform;
    }

    public void SetCamera(Camera sentCamera)
    {
        mainCamera = sentCamera;
        ResetZoom();
    }

    public void SetLoadingTransform(Transform sentTrasform)
    {
        if (loadingScreenTransform != null)
        {
            Destroy(loadingScreenTransform.parent.gameObject);
        }
        loadingScreenTransform = sentTrasform;
        DontDestroyOnLoad(loadingScreenTransform.parent.gameObject);
    }

    public Camera GetCamera()
    {
        return mainCamera;
    }

    public Vector2 GetCurrentPos()
    {
        if (currentPos != null)
        {
            return currentPos;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public Vector2 GetPrevPos()
    {
        if (prevPos != null)
        {
            return prevPos;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public void SetPos(Vector2 sentPos)
    {
        currentPos = sentPos;
    }

    public bool IsEditModeEnabled()
    {
        return editModeEnabled;
    }

    public void ToggleEditMode()
    {
        if (!editModeEnabled)
        {
            if (placedPaths.Count > 0)
            {
                for (int i = 0; i <= placedPaths.Count - 1; i++)
                {
                    for (int j = 0; j <= placedPaths[i].pathCoords.Count - 1; j++)
                    {
                        GetCellAtCoords(placedPaths[i].pathCoords[j]).ClearLines();
                        GetCellAtCoords(placedPaths[i].pathCoords[j]).ResetColor();
                        GetCellAtCoords(placedPaths[i].pathCoords[j]).InitTextColor();
                        if (!GetCellAtCoords(placedPaths[i].pathCoords[j]).isStartOrEnd)
                        {
                            GetCellAtCoords(placedPaths[i].pathCoords[j]).cellText.text = " ";
                        }
                    }
                }
                placedPaths.RemoveRange(0, placedPaths.Count);
            }
            if (correctPaths.Count > 0)
            {
                puzzleEditor.placedPaths = correctPaths;
                for (int i = 0; i <= cellList.Count - 1; i++)
                {
                    if (cellList[i].isStartOrEnd)
                    {
                        cellList[i].isStartOrEnd = false;
                    }
                }
                for (int i = 0; i <= correctPaths.Count - 1; i++)
                {
                    currentPath = correctPaths[i];
                    for (int j = 0; j <= correctPaths[i].pathCoords.Count - 1; j++)
                    {
                        GetCellAtCoords(correctPaths[i].pathCoords[j]).SetColor(currentPath.pathColor);
                        if (GetCellAtCoords(correctPaths[i].pathCoords[j]).cellImage.color != currentPath.pathColor)
                        {
                            //whatever, force it
                            GetCellAtCoords(correctPaths[i].pathCoords[j]).cellImage.color = currentPath.pathColor;
                        }
                        if (j == 1)
                        {
                            DrawLinesAroundCells(GetCellAtCoords(correctPaths[i].pathCoords[j - 1]), GetCellAtCoords(correctPaths[i].pathCoords[j]), null, j);
                        }
                        if (j > 1)
                        {
                            DrawLinesAroundCells(GetCellAtCoords(correctPaths[i].pathCoords[j - 2]), GetCellAtCoords(correctPaths[i].pathCoords[j - 1]), GetCellAtCoords(correctPaths[i].pathCoords[j]), j);
                        }
                    }
                }
            }
        }
        else
        {
            if (correctPaths.Count == 0)
            {
                for (int i = 0; i <= puzzleEditor.placedPaths.Count - 1; i++)
                {
                    correctPaths.Add(puzzleEditor.placedPaths[i]);
                }
            }
            for (int i = 0; i <= correctPaths.Count - 1; i++)
            {
                currentPath = correctPaths[i];
                GetCellAtCoords(correctPaths[i].pathCoords[0]).isStartOrEnd = true;
                GetCellAtCoords(correctPaths[i].pathCoords[correctPaths[i].pathCoords.Count - 1]).isStartOrEnd = true;
                for (int j = 0; j <= correctPaths[i].pathCoords.Count - 1; j++)
                {
                    GetCellAtCoords(correctPaths[i].pathCoords[j]).ClearLines();
                    GetCellAtCoords(correctPaths[i].pathCoords[correctPaths[i].pathCoords.Count - 1]).SetColor(correctPaths[i].pathColor);
                    GetCellAtCoords(correctPaths[i].pathCoords[j]).InitTextColor();
                    GetCellAtCoords(correctPaths[i].pathCoords[j]).ResetColor();
                }
            }
        }
        editModeEnabled = !editModeEnabled;
        //This isn't how I should be doing this, but I don't feel like allocating and serializing variables and doing a bunch of checks for something as simple as toggling a hud element
        GameObject.Find("ToolCanvas/Toolbar/EditModeEnabled").SetActive(editModeEnabled);
    }

    public void SetEditMode(bool setMode)
    {
        editModeEnabled = setMode;
    }

    public void SetPuzzleEditor(PuzzleEditor sentPuzzleEditor)
    {
        puzzleEditor = sentPuzzleEditor;
    }

    public void ClearCellList()
    {
        cellList.RemoveRange(0, cellList.Count);
    }

    public void AddCellToList(CellClass cellToAdd)
    {
        cellList.Add(cellToAdd);
    }

    void Update()
    {
        //Implement Bresenham's line algorithm to get a list of every cell between the current position to the previous position
        if ((gridWidth > 0 || gridHeight > 0) && cellList.Count > 0)
        {
            try
            {
                GetCellsFromLine(prevPos, currentPos);
            }
            catch (Exception e)
            {

            }
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
#if !UNITY_WEBGL
        if (Input.GetButtonDown("Fire1"))
        {
            touchStart = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * 0.1f);
            Vector2 direction = touchZero.position - touchZeroPrevPos;
            Move(direction.x, direction.y, 0.05f);
        }
#endif
        if (Input.GetButtonDown("ZoomIn") || Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            Zoom(1f);
        }
        if (Input.GetButtonDown("ZoomOut") || Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            Zoom(-1f);
        }
        if (Input.GetButton("Fire2") || Input.GetButton("Fire3"))
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
#endif
            Move(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), mouseSensitivity);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Zoom(float scale)
    {
        if (GetCellAtCoords(Vector2.zero) == null)
        {
            return;
        }

        mainCamera.orthographicSize -= scale;

        if (mainCamera.orthographicSize < 1)
        {
            mainCamera.orthographicSize = 1;
        }
        if (!topLeftCorner || !bottomRightCorner)
        {
            topLeftCorner = GetCellAtCoords(Vector2.zero).transform;
            bottomRightCorner = GetCellAtCoords(new Vector2(gridWidth - 1, gridHeight - 1)).transform;
        }
        for (int i = 0; i <= cellList.Count - 1; i++)
        {
            if (cellList[i].cellImage.isVisible)
            {
                return;
            }
        }
        GridPopulator gridPopulator = GameObject.FindObjectOfType<GridPopulator>();
        mainCamera.transform.position = new Vector3((gridPopulator.puzzleSize.x / 2) - (gridPopulator.spriteSize.x / 2), (-gridPopulator.puzzleSize.y / 2) + (gridPopulator.spriteSize.y / 2), mainCamera.transform.position.z);
        puzzleTransform.transform.position = Vector3.zero;

    }

    public void ResetZoom()
    {
        if (mainCamera && gridWidth > 0 && gridHeight > 0)
        {
            if (Screen.width >= Screen.height)
            {
                mainCamera.orthographicSize = gridWidth + gridHeight;
            }
            else
            {
                mainCamera.orthographicSize = (gridWidth + gridHeight) * 2;
            }
            GridPopulator gridPopulator = GameObject.FindObjectOfType<GridPopulator>();
            mainCamera.transform.position = new Vector3((gridPopulator.puzzleSize.x / 2) - (gridPopulator.spriteSize.x / 2), (-gridPopulator.puzzleSize.y / 2) + (gridPopulator.spriteSize.y / 2), mainCamera.transform.position.z);
        }
    }

    void Move(float xDelta, float yDelta, float scale)
    {
        if (GetCellAtCoords(Vector2.zero) == null)
        {
            return;
        }
        Vector3 lastPosition = mainCamera.transform.position;
        if (!topLeftCorner || !bottomRightCorner)
        {
            topLeftCorner = GetCellAtCoords(Vector2.zero).transform;
            bottomRightCorner = GetCellAtCoords(new Vector2(gridWidth - 1, gridHeight - 1)).transform;
        }
        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - (xDelta * scale), mainCamera.transform.position.y - (yDelta * scale), mainCamera.transform.position.z);
        if (bottomRightCorner.position.x < mainCamera.ViewportToWorldPoint(Vector3.zero).x || topLeftCorner.position.x > mainCamera.ViewportToWorldPoint(Vector2.one).x || bottomRightCorner.position.y > mainCamera.ViewportToWorldPoint(Vector2.one).y || topLeftCorner.position.y < mainCamera.ViewportToWorldPoint(Vector2.zero).y)
        {
            mainCamera.transform.position = lastPosition;
        }
    }

    public void OnPointerEnterCell(CellClass cell)
    {
        if (editModeEnabled)
        {
            puzzleEditor.EditorEnterCell(cell);
            return;
        }
        //If the mouse button is not being held down
        if (currentPath == null || currentPath.pathCoords == null)
        {
            return;
        }
        if (!Input.GetButton("Fire1") || currentPath.pathCoords.Count == 0)
        {
            currentPos = cell.coords;
            return;
        }
        if (Input.GetButton("Fire1") && !CheckConnected(cell.coords, currentPos))
        {
            currentPos = cell.coords;
            return;
        }

        //If the start/end cell's color doesn't match the current path's color, stop
        if (cell.isStartOrEnd && cell.coords != currentPath.pathCoords[0])
        {
            if (cell.cellColor != currentPath.pathColor || remainingMoves != 1)
            {
                return;
            }
        }
        //If the manager detects that the user is not backtracking and this cell is not a start or end cell
        if (!CheckIfBacktracking() && cell.isStartOrEnd && CheckConnected(currentPath.pathCoords[currentPath.pathCoords.Count - 1], currentPos))
        {
            //Determine the next path coordinate index
            int nextPathCoordIndex = currentPath.pathCoords.Count + 1;
            //If the coordinates of this cell do not match the manager's first cell coordinates
            if (cell.coords != GetCellAtCoords(currentPath.pathCoords[0]).coords)
            {
                //Debug.Log(GetCellAtCoords(currentPath.pathCoords[0]).cellText.text + " " + nextPathCoordIndex + " " + cell.cellText.text);
                //If the text of the cell at the current path's first coordinates does not match the next coordinate's index
                if (GetCellAtCoords(currentPath.pathCoords[0]).cellText.text != cell.cellText.text || GetCellAtCoords(currentPath.pathCoords[0]).cellText.text != nextPathCoordIndex.ToString())
                {
                    //Stop everything
                    return;
                }
                if (currentPath.pathCoords.Count > 1)
                {
                    GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).cellText.text = " ";
                }
            }
        }
        //If the mouse button is pressed and this cell is not a start or end point and the number of moves remaining for the path is not zero
        if (!GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd && remainingMoves != 0)
        {
            //If the coordinates of this cell does not intersect a path and the manager detects the user is not backtracking
            if (!CheckIfPathIntersects(cell.coords) && !CheckIfBacktracking())
            {
                //If the coordinates of this cell and the previous coordinate's cell is greater than one, stop
                if (Vector2.Distance(currentPath.pathCoords[currentPath.pathCoords.Count - 1], cell.coords) > 1 || GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd || (cell.cellText.text != " " && GetCellAtCoords(currentPath.pathCoords[0]).cellText.text != cell.cellText.text))
                {
                    return;
                }
                //Erase the text in the current cell
                if (currentPath.pathCoords.Count > 1)
                {
                    GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).cellText.text = " ";
                }
            }
        }
        //New coordinates
        currentPos = cell.coords;
        //If the manager is out of moves in the current path and not backtracking, stop
        if (remainingMoves == 0 && !CheckIfBacktracking())
        {
            return;
        }
        //If the distance between this cell and this cell's coordinates is greater than one, stop
        if (!CheckConnected(currentPos, prevPos))
        {
            return;
        }
        //If the new coordinates intersects with an existing path, stop
        if (CheckIfPathIntersects(cell.coords))
        {
            return;
        }
        //Update the manager's previous position with the current position
        prevPos = currentPos;
        //If the user is backtracking through a path
        if (CheckIfBacktracking())
        {
            //If the cell at the last path coordinates of the current path is not a start or end cell, reset the cell color and text color, and erase its text
            if (!GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd)
            {
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).ClearLines();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).ResetColor();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).InitTextColor();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).cellText.text = " ";
            }
            //If the cell at the last path coordinates of the current path is a start or end cell, reset the cell's color and text color, but leave the text alone
            if (GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd)
            {
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).ClearLines();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).ResetColor();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).InitTextColor();
            }
            //Remove the current path's most recent path coordinate
            currentPath.pathCoords.RemoveAt(currentPath.pathCoords.Count - 1);
            //If there's at least one coordinate in the current path, update the cell's text
            if (currentPath.pathCoords.Count > 1)
            {
                cell.cellText.text = currentPath.pathCoords.Count.ToString();
                CellClass currentCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]);
                CellClass middleCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 2]);
                //Debug.Log(currentCell.coords + " " + middleCell.coords);
                if (middleCell.coords.x > currentCell.coords.x)
                {
                    currentCell.ClearLines();
                    currentCell.DrawLines(true, true, true, false);
                }
                if (middleCell.coords.x < currentCell.coords.x)
                {
                    currentCell.ClearLines();
                    currentCell.DrawLines(true, true, false, true);
                }
                if (middleCell.coords.y > currentCell.coords.y)
                {
                    currentCell.ClearLines();
                    currentCell.DrawLines(true, false, true, true);
                }
                if (middleCell.coords.y < currentCell.coords.y)
                {
                    currentCell.ClearLines();
                    currentCell.DrawLines(false, true, true, true);
                }
            }
            //Give back a move
            remainingMoves++;
        }
        //If no path was clicked
        if (!WasPathClicked())
        {
            Color nullColor = new Color(0f, 0f, 0f, 0f);
            if (currentPath.pathColor == nullColor)
            {
                return;
            }
            //Add the current cell to the current path's list of coordinates
            AddPathCoords(cell.coords);
            //If this cell is a start or end cell, set the start/end cell's image color to the cell's designated color and remove a move from the manager and stop
            if (cell.isStartOrEnd)
            {
                cell.SetColor(cell.cellColor);
                remainingMoves--;
                return;
            }
            //Set the cell's color to the current path's color
            cell.cellImage.color = currentPath.pathColor;
            //Set the cell's text to the current length of the path
            cell.cellText.text = currentPath.pathCoords.Count.ToString();
            //Remove a move
            remainingMoves--;
        }
    }

    public void OnPointerClickDownCell(CellClass cell)
    {
        if (editModeEnabled)
        {
            puzzleEditor.EditorClickDownCell(cell);
            return;
        }
        //If a path was clicked
        if (WasPathClicked())
        {
            if (placedPaths.Count > 0)
            {
                for (int i = 0; i <= placedPaths.Count - 1; i++)
                {
                    if (cell.coords == placedPaths[i].pathCoords[placedPaths[i].pathCoords.Count - 1] || cell.coords == placedPaths[i].pathCoords[0])
                    {
                        if (cell.coords == placedPaths[i].pathCoords[0])
                        {
                            currentPath = placedPaths[i];
                            currentPath.pathCoords.Reverse();
                        }
                        else
                        {
                            currentPath = placedPaths[i];
                        }
                        int tempInt = 0;
                        int.TryParse(GetCellAtCoords(currentPath.pathCoords[0]).cellText.text, out tempInt);
                        remainingMoves = tempInt - currentPath.pathCoords.Count;
                        currentPos = cell.coords;
                        prevPos = currentPath.pathCoords[currentPath.pathCoords.Count - 1];
                        cell.StopAllCoroutines();
                        cell.SetColor(placedPaths[i].pathColor);
                        break;
                    }
                }
            }
            //Double click the path to delete it
            if (clearTimer <= 0)
            {
                clearTimer = doubleClickDelay;
                StartCoroutine("ClearCountdown");
                return;
            }
            if (clearTimer > 0)
            {
                ClearPath(cell.coords);
                clearTimer = 0f;
            }
            return;
        }
        //Set the current position to this cell's coordinates
        currentPos = cell.coords;
        prevPos = currentPos;
        //If this cell isn't a start or end cell, stop
        if (!cell.isStartOrEnd)
        {
            return;
        }
        //Create a new path at this cell's coordinates
        NewPath(cell.coords);
        //If the path is a start or end cell, color the cell and set the remaining moves
        if (cell.isStartOrEnd)
        {
            cell.SetColor(cell.cellColor);
            currentPath.pathColor = cell.cellColor;
            remainingMoves = GetRemainingMoves(cell.coords);
        }
    }

    public void OnPointerClickUpCell(CellClass cell)
    {
        if (editModeEnabled)
        {
            puzzleEditor.EditorClickUpCell(cell);
            return;
        }
        if (CheckWinPuzzle() || currentPath.pathCoords == null)
        {
            return;
        }
        //If the current path's length is one and the cell is a one-cell path
        if (currentPath.pathCoords.Count == 1)
        {
            CellClass singleCell = GetCellAtCoords(currentPath.pathCoords[0]);
            if (singleCell.cellText.text == "1" || singleCell.cellText.text == " " || singleCell.coords == Vector2.zero)
            {
                return;
            }
            singleCell.ClearLines();
            singleCell.ResetColor();
            singleCell.InitTextColor();
            for (int i = 0; i <= placedPaths.Count - 1; i++)
            {
                //Removes the single-cell path in a scenario where the user clicked on a cell but didn't start drawing a path
                if (placedPaths[i].pathCoords.Count - 1 == 0 && placedPaths[i].pathCoords[placedPaths[i].pathCoords.Count - 1] == singleCell.coords)
                {
                    placedPaths.RemoveAt(i);
                    break;
                }
                for (int j = 0; j <= placedPaths[i].pathCoords.Count - 1; j++)
                {
                    if (singleCell.coords == placedPaths[i].pathCoords[j] && placedPaths[i].pathCoords.Count - 1 == 0)
                    {
                        placedPaths.RemoveAt(i);
                    }
                }
            }
        }
        if (!GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd)
        {
            if (currentPath.pathCoords[0] != Vector2.zero && currentPath.pathCoords.Count != 1)
            {
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).StartCoroutine("Blink");
            }
        }
        if (!GetCellAtCoords(currentPath.pathCoords[0]).isStartOrEnd && !GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd)
        {
            ClearPath(currentPath.pathCoords[0]);
        }
        SavePuzzle();
        currentPath = new Path();
        currentPath.StartPath(Vector2.zero);
    }

    public bool CheckConnected(Vector2 currentCoordinate, Vector2 newCoordinates)
    {
        if (editModeEnabled)
        {
            if (Vector2.Distance(currentCoordinate, newCoordinates) == 1.0f)
            {
                return true;
            }
        }
        else
        {
            if (currentPath.pathCoords.Count > 0)
            {
                if (Vector2.Distance(currentCoordinate, newCoordinates) == 1.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<CellClass> GetCellsFromLine(Vector2 start, Vector2 end)
    {
        float w = end.x - start.x;
        float h = end.y - start.y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        float longest = Math.Abs(w);
        float shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = (int)longest >> 1;

        List<CellClass> cellList = new List<CellClass>();

        for (int i = 0; i <= longest; i++)
        {
            cellList.Add(GetCellAtCoords(start));
            numerator += (int)shortest;
            if (!(numerator < longest))
            {
                numerator -= (int)longest;
                start.x += dx1;
                start.y += dy1;
            }
            else
            {
                start.x += dx2;
                start.y += dy2;
            }
        }
        if (!editModeEnabled)
        {
            for (int i = 0; i <= cellList.Count - 1; i++)
            {
                OnPointerEnterCell(cellList[i]);
            }
        }
        return cellList;
    }

    public void NewPath(Vector2 startCoords)
    {
        Path newPath = new Path();
        placedPaths.Add(newPath);
        currentPath = placedPaths[placedPaths.Count - 1];
        currentPath.StartPath(startCoords);
    }

    public void AddPathCoords(Vector2 coords)
    {
        if (currentPath != null)
        {
            if (currentPath.pathCoords.Count == 1)
            {
                CellClass pastCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]);
                CellClass middleCell = GetCellAtCoords(currentPos);
                DrawLinesAroundCells(pastCell, middleCell, null, currentPath.pathCoords.Count);
            }
            if (currentPath.pathCoords.Count > 1)
            {
                CellClass currentCell = GetCellAtCoords(currentPos);
                CellClass middleCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]);
                CellClass pastCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 2]);
                currentPath.pathCoords.Add(coords);
                DrawLinesAroundCells(pastCell, middleCell, currentCell, currentPath.pathCoords.Count);
                return;
            }
            else
            {
                currentPath.pathCoords.Add(coords);
            }
        }
    }

    public void DrawLinesAroundCells(CellClass pastCell, CellClass middleCell, CellClass currentCell, int pathCoordCount)
    {
        if (pathCoordCount == 1)
        {
            pastCell.ClearLines();
            if (pastCell.isStartOrEnd && middleCell.coords.x > pastCell.coords.x)
            {
                pastCell.DrawLines(true, true, true, false);
                middleCell.DrawLines(true, true, false, true);
            }
            if (pastCell.isStartOrEnd && middleCell.coords.x < pastCell.coords.x)
            {
                pastCell.DrawLines(true, true, false, true);
                middleCell.DrawLines(true, true, true, false);
            }
            if (pastCell.isStartOrEnd && middleCell.coords.y > pastCell.coords.y)
            {
                pastCell.DrawLines(true, false, true, true);
                middleCell.DrawLines(false, true, true, true);
            }
            if (pastCell.isStartOrEnd && middleCell.coords.y < pastCell.coords.y)
            {
                pastCell.DrawLines(false, true, true, true);
                middleCell.DrawLines(true, false, true, true);
            }
        }
        if (pathCoordCount > 1 && currentCell && middleCell)
        {
            currentCell.ClearLines();
            middleCell.ClearLines();
            if (middleCell.coords.x > currentCell.coords.x)
            {
                currentCell.DrawLines(true, true, true, false);
            }
            if (middleCell.coords.x < currentCell.coords.x)
            {
                currentCell.DrawLines(true, true, false, true);
            }
            if (middleCell.coords.y > currentCell.coords.y)
            {
                currentCell.DrawLines(true, false, true, true);
            }
            if (middleCell.coords.y < currentCell.coords.y)
            {
                currentCell.DrawLines(false, true, true, true);
            }
            //If current, middle and past cells share the same y or x coordinate, assume a straight line
            if (pastCell.coords.y == middleCell.coords.y && middleCell.coords.y == currentCell.coords.y)
            {
                //Debug.Log("Drawing top and bottom");
                middleCell.DrawLines(true, true, false, false);
            }
            if (pastCell.coords.x == middleCell.coords.x && middleCell.coords.x == currentCell.coords.x)
            {
                //Debug.Log("Drawing left and right");
                middleCell.DrawLines(false, false, true, true);
            }
            //If the last cell is to the left of the middle cell and the current cell is lower than the middle cell
            // ┐
            if (pastCell.coords.x < middleCell.coords.x && currentCell.coords.y > middleCell.coords.y)
            {
                //Debug.Log("Drawing top and right");
                middleCell.DrawLines(true, false, false, true);
                return;
            }
            //If the last cell is to the left of the middle cell and the current cell is above the middle cell
            // ┘
            if (pastCell.coords.x < middleCell.coords.x && currentCell.coords.y < middleCell.coords.y)
            {
                //Debug.Log("Drawing bottom and right");
                middleCell.DrawLines(false, true, false, true);
                return;
            }
            //If the last cell is to the right of the middle cell and the current cell is above the middle cell
            // └
            if (pastCell.coords.x > middleCell.coords.x && currentCell.coords.y < middleCell.coords.y)
            {
                //Debug.Log("Drawing bottom and left");
                middleCell.DrawLines(false, true, true, false);
                return;
            }
            //If the last cell is to the right of the middle cell and the current cell is lower than the middle cell
            //┌
            if (pastCell.coords.x > middleCell.coords.x && currentCell.coords.y > middleCell.coords.y)
            {
                //Debug.Log("Drawing top and left");
                middleCell.DrawLines(true, false, true, false);
                return;
            }
            //If the last cell shares the same X coordinate as the middle cell, is above the middle cell, and the current cell is to the left of the middle cell
            // ┐
            if (pastCell.coords.x == middleCell.coords.x && pastCell.coords.y > middleCell.coords.y && currentCell.coords.x < middleCell.coords.x)
            {
                //Debug.Log("Drawing top and right");
                middleCell.DrawLines(true, false, false, true);
                return;
            }
            //If the last cell shares the same X coordinate as the middle cell, is below the middle cell, and the current cell is to the right of the middle cell
            // └
            if (pastCell.coords.x == middleCell.coords.x && pastCell.coords.y < middleCell.coords.y && currentCell.coords.x > middleCell.coords.x)
            {
                //Debug.Log("Drawing bottom and left");
                middleCell.DrawLines(false, true, true, false);
                return;
            }
            //If the last cell shares the same X coordinate as the middle cell, is below the middle cell, and the current cell is to the right
            //┌
            if (pastCell.coords.x == middleCell.coords.x && pastCell.coords.y > middleCell.coords.y && currentCell.coords.x > middleCell.coords.x)
            {
                //Debug.Log("Drawing top and left");
                middleCell.DrawLines(true, false, true, false);
                return;
            }
            //If the last cell shares the same X coordinate as the middle cell, is above the middle cell, and the current cell is to the left
            // ┘
            if (pastCell.coords.x == middleCell.coords.x && pastCell.coords.y < middleCell.coords.y && currentCell.coords.x < middleCell.coords.x)
            {
                //Debug.Log("Drawing bottom and right");
                middleCell.DrawLines(false, true, false, true);
                return;
            }
        }
    }

    public bool CheckIfBacktracking()
    {
        if (currentPath.pathCoords.Count > 0)
        {
            if (currentPath.pathCoords.Count - 1 > 0)
            {
                return currentPos.Equals(currentPath.pathCoords[currentPath.pathCoords.Count - 2]);
            }
        }
        return false;
    }

    public bool CheckIfPathIntersects(Vector2 nextCoords)
    {
        if (placedPaths.Count > 0)
        {
            if (placedPaths[placedPaths.Count - 1].pathCoords.Count > 0)
            {
                for (int i = 0; i <= placedPaths.Count - 1; i++)
                {
                    for (int j = 0; j <= placedPaths[i].pathCoords.Count - 1; j++)
                    {
                        if (nextCoords == placedPaths[i].pathCoords[j] && !CheckIfBacktracking())
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool WasPathClicked()
    {
        for (int i = 0; i <= placedPaths.Count - 1; i++)
        {
            for (int j = 0; j <= placedPaths[i].pathCoords.Count - 1; j++)
            {
                if (currentPos == placedPaths[i].pathCoords[j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool CheckIsStartOrEndCell(Vector2 cellToCheck)
    {
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            if (correctPaths[i].pathCoords[0] == cellToCheck || correctPaths[i].pathCoords[correctPaths[i].pathCoords.Count - 1] == cellToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public Color GetStartOrEndColor(Vector2 cellToCheck)
    {
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            if (correctPaths[i].pathCoords[0] == cellToCheck || correctPaths[i].pathCoords[correctPaths[i].pathCoords.Count - 1] == cellToCheck)
            {
                return correctPaths[i].pathColor;
            }
        }
        return Color.white;
    }

    public int GetPathLength(Vector2 cellToCheck)
    {
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            if (correctPaths[i].pathCoords[0] == cellToCheck || correctPaths[i].pathCoords[correctPaths[i].pathCoords.Count - 1] == cellToCheck)
            {
                return correctPaths[i].pathCoords.Count;
            }
        }
        return 0;
    }

    public CellClass GetCellAtCoords(Vector2 cellCoords)
    {
        return cellList[(int)cellCoords.y * gridWidth + (int)cellCoords.x];
    }

    public List<CellClass> GetCellList()
    {
        return cellList;
    }

    public int GetRemainingMoves(Vector2 startOrEndCoords)
    {
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            for (int j = 0; j <= correctPaths[i].pathCoords.Count - 1; j++)
            {
                if (correctPaths[i].pathCoords[j] == startOrEndCoords)
                {
                    return correctPaths[i].pathCoords.Count - 1;
                }
            }
        }
        return 0;
    }

    public void ClearPath(Vector2 coordsOfPathCell)
    {
        for (int i = 0; i <= placedPaths.Count - 1; i++)
        {
            for (int j = 0; j <= placedPaths[i].pathCoords.Count - 1; j++)
            {
                if (placedPaths[i].pathCoords[j] == coordsOfPathCell)
                {
                    for (int k = 0; k <= placedPaths[i].pathCoords.Count - 1; k++)
                    {
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).ClearLines();
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).ResetColor();
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).StopAllCoroutines();
                        if (!GetCellAtCoords(placedPaths[i].pathCoords[k]).isStartOrEnd)
                        {
                            GetCellAtCoords(placedPaths[i].pathCoords[k]).cellText.text = " ";
                        }
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).InitTextColor();
                    }
                    placedPaths.RemoveAt(i);
                    currentPath = new Path();
                    currentPath.StartPath(Vector2.zero);
                    return;
                }
            }
        }
        SavePuzzle();
    }

    IEnumerator ClearCountdown()
    {
        while (clearTimer > 0)
        {
            clearTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void EndPath()
    {
        currentPath = new Path();
    }

    public void UpdateGridWidth(string newWidth)
    {
        int.TryParse(newWidth, out gridWidth);
    }

    public void UpdateGridHeight(string newHeight)
    {
        int.TryParse(newHeight, out gridHeight);
    }

    bool CheckWinPuzzle()
    {
        if (correctPaths.Count == 0 || GetComponent<PuzzleEditor>())
        {
            return false;
        }
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            if (GetCellAtCoords(correctPaths[i].pathCoords[0]).cellImage.color != correctPaths[i].pathColor || GetCellAtCoords(correctPaths[i].pathCoords[correctPaths[i].pathCoords.Count - 1]).cellImage.color != correctPaths[i].pathColor)
            {
                return false;
            }
        }
        if (placedPaths.Count != correctPaths.Count)
        {
            Debug.LogWarning("Number of placed paths did not match correct paths. Placed: " + placedPaths.Count + " Expected: " + correctPaths.Count);
        }
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            for (int j = 0; j <= correctPaths[i].pathCoords.Count - 1; j++)
            {
                if (GetCellAtCoords(correctPaths[i].pathCoords[j]).cellImage.color != correctPaths[i].pathColor)
                {
                    Debug.Log(GetCellAtCoords(correctPaths[i].pathCoords[j]).name + "Did not have matching color");
                    return false;
                }
            }
        }
        Debug.Log("Winner!");
        ClearSave();
        SaveGame();
        ResetZoom();
        GridPopulator gridPopulator = GameObject.FindObjectOfType<GridPopulator>();
        gridPopulator.ForceGridOff();
        for (int i = 0; i <= cellList.Count - 1; i++)
        {
            for (int j = 0; j <= cellList[i].transform.childCount - 1; j++)
            {
                cellList[i].transform.GetChild(j).gameObject.SetActive(false);
            }
            cellList[i].cellText.text = " ";
            cellList[i].interactable = false;
            //cellList[i].ResetColor();
            cellList[i].StopAllCoroutines();
            /* This breaks the game, find a better solution
            for (int j = 0; j <= correctPaths.Count - 1; j++)
            {
                for (int k = 0; k <= correctPaths[j].pathCoords.Count - 1; k++)
                {
                    CellClass tempCell = GetCellAtCoords(correctPaths[j].pathCoords[k]);
                    tempCell.SetColor(correctPaths[j].pathColor);
                }
            }
            */
        }
        TitlePuzzleSolver titlePuzzleSolver = GetComponent<TitlePuzzleSolver>();
        titlePuzzleSolver.ToggleUIElement(titlePuzzleSolver.UITransforms[titlePuzzleSolver.UITransforms.Count - 1]);
        return true;
    }

    public void CheckCorrectPaths()
    {
        for (int i = 0; i <= placedPaths.Count - 1; i++)
        {
            bool isPathRight = false;
            for (int j = 0; j <= correctPaths.Count - 1; j++)
            {
                if ((placedPaths[i].pathCoords[0] == correctPaths[j].pathCoords[0]
                    || placedPaths[i].pathCoords[0] == correctPaths[j].pathCoords[correctPaths[j].pathCoords.Count - 1])
                    && (placedPaths[i].pathCoords[placedPaths[i].pathCoords.Count - 1] == correctPaths[j].pathCoords[0]
                    || placedPaths[i].pathCoords[placedPaths[i].pathCoords.Count - 1] == correctPaths[j].pathCoords[correctPaths[j].pathCoords.Count - 1]))
                {
                    //Confirmed that the placed path shares the same start and end coordinates as a correct path
                    //Iterate through each cell of the placed path
                    //If the placed path shares cells with 
                    isPathRight = true;
                    for (int k = 0; k <= placedPaths[i].pathCoords.Count - 1; k++)
                    {
                        if (!correctPaths[j].pathCoords.Contains(placedPaths[i].pathCoords[k]))
                        {
                            isPathRight = false;
                        }
                    }
                }
            }
            if (!isPathRight)
            {
                for (int j = 1; j <= placedPaths[i].pathCoords.Count - 2; j++)
                {
                    GetCellAtCoords(placedPaths[i].pathCoords[j]).cellText.text = "X";
                }
            }
        }
    }

    public void FillOneLengthPaths()
    {
        for (int i = 0; i <= correctPaths.Count - 1; i++)
        {
            if (correctPaths[i].pathCoords.Count == 1)
            {
                CellClass singleCell;
                singleCell = GetCellAtCoords(correctPaths[i].pathCoords[0]);
                if (singleCell.cellImage.color != correctPaths[i].pathColor)
                {
                    singleCell.SetColor(correctPaths[i].pathColor);
                    placedPaths.Add(correctPaths[i]);
                }
            }
        }
        currentPath = new Path();
        currentPath.StartPath(Vector2.zero);
        if (!CheckWinPuzzle())
        {
            SavePuzzle();
        }
    }

    public void UpdatePuzzleName(string sentName)
    {
        puzzleName = sentName;
    }

    public void SavePuzzle()
    {
        if (puzzleEditor)
        {
            return;
        }
        string savePath = Application.persistentDataPath + "/" + puzzleName + "_save.json";
        string savedPuzzleJson = JsonUtility.ToJson(this);
        File.WriteAllText(savePath, savedPuzzleJson);
#if UNITY_WEBGL && !UNITY_EDITOR
            SyncFiles();
#endif
        Debug.Log("Saved.");
    }

    public void SaveGame()
    {
        string savePath = Application.persistentDataPath + "/picaLinkSave.txt";
        if (!File.Exists(savePath))
        {
            File.Create(savePath);
            return;
        }
        if (File.Exists(savePath))
        {
            string[] completedPuzzles = File.ReadAllLines(savePath);
            for (int i = 0; i <= completedPuzzles.Length - 1; i++)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (completedPuzzles[i] == puzzleFolder + puzzleName)
                {
                    return;
                }
#endif
#if UNITY_WEBGL && UNITY_EDITOR
                if (completedPuzzles[i] == puzzleFolder + "/" + puzzleName)
                {
                    return;
                }
#endif
            }
            using (StreamWriter writer = new StreamWriter(savePath, true))
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                writer.WriteLine(puzzleFolder + puzzleName);
#endif
#if UNITY_WEBGL && UNITY_EDITOR
                writer.WriteLine(puzzleFolder + "/" + puzzleName);
#endif
                writer.Close();
            }
        }
    }

    public IEnumerator LoadSave()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*_save.json");
        if (saveFiles.Length != 0)
        {
            Debug.Log("Save file: " + saveFiles[0]);
            JsonUtility.FromJsonOverwrite(File.ReadAllText(saveFiles[0]), this);
            GridPopulator gridPopulator = GameObject.FindObjectOfType<GridPopulator>();
            gridPopulator.UpdateGrid();
        }
        else
        {
            Debug.Log("No save found.");
        }
        yield return null;
    }

    public void ClearSave()
    {
        string savePath = Application.persistentDataPath + "/" + puzzleName + "_save.json";
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Deleted " + savePath);
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
    }

    public void SavePuzzleManager(string folder, bool force, GameObject confirmBox)
    {
        if (puzzleName == "")
        {
            return;
        }
        if (editModeEnabled)
        {
            correctPaths = puzzleEditor.placedPaths;
        }
        string filePath = Application.dataPath + "/StreamingAssets/" + folder + "/" + puzzleName + ".json";
        Debug.Log(filePath);
        if (folder == "")
        {
            Debug.LogWarning("Didn't save in a folder!");
            return;
        }
        if (File.Exists(filePath))
        {
            if (!force)
            {
                Debug.LogWarning("File exists!");
                confirmBox.SetActive(true);
                return;
            }
        }
        string puzzleJson = JsonUtility.ToJson(this);
        File.WriteAllText(filePath, puzzleJson);
        Debug.Log(File.ReadAllText(filePath));
    }

    public IEnumerator LoadPuzzleManager(string folderName, bool isSingleFile)
    {
        string filePath = "";
#if UNITY_WEBGL && !UNITY_EDITOR
        filePath = Application.streamingAssetsPath + "/" + folderName + "/" + puzzleName + ".json";
        Debug.Log(filePath);
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        JsonUtility.FromJsonOverwrite(www.downloadHandler.text, this);
        yield return null;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        filePath = Application.streamingAssetsPath + "/" + folderName + puzzleName + ".json";
        Debug.Log(filePath);
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        JsonUtility.FromJsonOverwrite(www.downloadHandler.text, this);
        yield return null;
#endif
#if UNITY_IOS
        if(folderName != "")
        {
            filePath = Application.dataPath + "/Raw/" + folderName + "/" + puzzleName + ".json";
        }
        else
        {
            filePath = Application.dataPath + "/Raw/" + puzzleName + ".json";
        }
#endif
#if UNITY_EDITOR
        filePath = Application.streamingAssetsPath + "/" + folderName + "/" + puzzleName + ".json";
        if (isSingleFile)
        {
            Debug.Log(folderName);
            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(folderName), this);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        else
        {
            Debug.Log(filePath);
            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        if (puzzleEditor)
        {
            puzzleEditor.placedPaths = correctPaths;
        }
        yield return null;
#endif
    }

    public IEnumerator LoadPuzzleManager(string JSONoverride)
    {
        try
        {
            JsonUtility.FromJsonOverwrite(JSONoverride, this);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        yield return null;
    }

    public IEnumerator ToggleLoadingScreen(bool showScreen)
    {
        if (!loadingScreenTransform)
        {
            yield return null;
        }
        float targetTime = 1f;
        float transitionTime = 0f;
        Vector3 targetScale;

        if (showScreen)
        {
            targetScale = new Vector3(1, 2, 1);
        }
        else
        {
            targetScale = new Vector3(0, 2, 1);
        }

        while (transitionTime < targetTime)
        {
            transitionTime += Time.deltaTime;
            if (transitionTime > targetTime)
            {
                transitionTime = targetTime;
            }
            loadingScreenTransform.localScale = Vector3.Lerp(loadingScreenTransform.localScale, targetScale, transitionTime / targetTime);
            //Check against size threshold to avoid AABB errors
            if (loadingScreenTransform.localScale.x < 0.0001f && !showScreen)
            {
                loadingScreenTransform.localScale = targetScale;
                yield return null;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void LoadScene(int sceneToLoad)
    {
        StartCoroutine(LoadSceneCoroutine(sceneToLoad));
    }

    IEnumerator LoadSceneCoroutine(int sceneToLoad)
    {
        TitlePuzzleSolver solver = GameObject.FindObjectOfType<TitlePuzzleSolver>();
        if (solver)
        {
            solver.StopAllCoroutines();
        }
        DontDestroyOnLoad(this.gameObject);
        yield return ToggleLoadingScreen(true);
        yield return SceneManager.LoadSceneAsync(sceneToLoad);
        yield return ToggleLoadingScreen(false);
        Destroy(this.gameObject);
        yield return null;
    }
}

[Serializable]
public class Path
{
    public Color pathColor;
    public List<Vector2> pathCoords;

    public void StartPath(Vector2 startCoords)
    {
        pathCoords = new List<Vector2>();
        pathCoords.Add(startCoords);
    }
}
