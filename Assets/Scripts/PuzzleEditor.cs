using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleEditor : MonoBehaviour
{
    public PuzzleManager puzzleManager;

    public Vector2 currentPos;
    public Vector2 prevPos;
    public List<Path> placedPaths = new List<Path>();
    public Path currentPath;

    public Color pathColor = Color.black;
    public string colorName = "Black";

    private void Awake()
    {
        puzzleManager = GetComponent<PuzzleManager>();
        puzzleManager.SetPuzzleEditor(this);
        puzzleManager.gridWidth = 16;
        puzzleManager.gridHeight = 16;
        GameObject.FindObjectOfType<GridPopulator>().UpdateGrid();
    }

    public void EditorEnterCell(CellClass cell)
    {
        //If the mouse button is not being held down
        if (!Input.GetButton("Fire1") || currentPath.pathCoords.Count == 0)
        {
            currentPos = cell.coords;
            return;
        }
        if (Input.GetButton("Fire1") && !puzzleManager.CheckConnected(cell.coords, currentPos))
        {
            currentPos = cell.coords;
            return;
        }
        //If the manager detects that the user is not backtracking and this cell is not a start or end cell
        if (!CheckIfBacktracking() && cell.isStartOrEnd)
        {
            //Determine the next path coordinate index
            int nextPathCoordIndex = currentPath.pathCoords.Count + 1;
            //If the coordinates of this cell do not match the manager's first cell coordinates
            if (cell.coords != puzzleManager.GetCellAtCoords(currentPath.pathCoords[0]).coords)
            {
                //Debug.Log(GetCellAtCoords(currentPath.pathCoords[0]).cellText.text + " " + nextPathCoordIndex + " " + cell.cellText.text);
                //If the text of the cell at the current path's first coordinates does not match the next coordinate's index
                if (puzzleManager.GetCellAtCoords(currentPath.pathCoords[0]).cellText.text != cell.cellText.text || puzzleManager.GetCellAtCoords(currentPath.pathCoords[0]).cellText.text != nextPathCoordIndex.ToString())
                {
                    //Stop everything
                    return;
                }
            }
        }
        //If the coordinates of this cell does not intersect a path and the manager detects the user is not backtracking
        if (!CheckIfPathIntersects(cell.coords) && !CheckIfBacktracking())
        {
            //If the coordinates of this cell and the previous coordinate's cell is greater than one, stop
            if (Vector2.Distance(currentPath.pathCoords[currentPath.pathCoords.Count - 1], cell.coords) > 1 || puzzleManager.GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd)
            {
                return;
            }
            //Erase the text in the current cell
            puzzleManager.GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).cellText.text = "";
        }
        //New coordinates
        currentPos = cell.coords;
        //If the distance between this cell and this cell's coordinates is greater than one, stop
        if (!puzzleManager.CheckConnected(currentPos, prevPos))
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
            if (!puzzleManager.GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).isStartOrEnd)
            {
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).ClearLines();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).ResetColor();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).InitTextColor();
                GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]).cellText.text = "";
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
            GetCellAtCoords(currentPath.pathCoords[0]).cellText.text = currentPath.pathCoords.Count.ToString();
            GetCellAtCoords(currentPath.pathCoords[0]).cellColor = pathColor;
            GetCellAtCoords(currentPath.pathCoords[0]).SetColor(pathColor);
            cell.cellText.text = currentPath.pathCoords.Count.ToString();
            if (currentPath.pathCoords.Count > 1)
            {
                CellClass currentCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]);
                CellClass middleCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 2]);
                Debug.Log(currentCell.coords + " " + middleCell.coords);
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
        }
        //If no path was clicked
        if (!WasPathClicked())
        {
            //Add the current cell to the current path's list of coordinates
            AddPathCoords(cell.coords);
            //Set the cell's color to the current path's color
            cell.cellImage.color = currentPath.pathColor;
            //Set the cell's text to the current length of the path
            GetCellAtCoords(currentPath.pathCoords[0]).cellText.text = currentPath.pathCoords.Count.ToString();
            GetCellAtCoords(currentPath.pathCoords[0]).cellColor = pathColor;
            GetCellAtCoords(currentPath.pathCoords[0]).SetColor(pathColor);
            cell.cellText.text = currentPath.pathCoords.Count.ToString();
        }
    }

    public void EditorClickDownCell(CellClass cell)
    {
        //If a path was clicked
        if (WasPathClicked())
        {
            ClearPath(cell.coords);
            return;
        }
        //Set the current position to this cell's coordinates
        currentPos = cell.coords;
        prevPos = currentPos;
        //Create a new path at this cell's coordinates
        NewPath(cell.coords);
        //If the path is a start or end cell, color the cell and set the remaining moves
        cell.cellColor = pathColor;
        cell.SetColor(pathColor);
        cell.cellText.text = "1";
        currentPath.pathColor = pathColor;
    }

    public void EditorClickUpCell(CellClass cell)
    {
        
    }

    public void AddPathCoords(Vector2 coords)
    {
        if (currentPath != null)
        {
            if (currentPath.pathCoords.Count == 1)
            {
                CellClass pastCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]);
                CellClass middleCell = GetCellAtCoords(currentPos);
                pastCell.ClearLines();
                if (middleCell.coords.x > pastCell.coords.x)
                {
                    pastCell.DrawLines(true, true, true, false);
                    middleCell.DrawLines(true, true, false, true);
                }
                if (middleCell.coords.x < pastCell.coords.x)
                {
                    pastCell.DrawLines(true, true, false, true);
                    middleCell.DrawLines(true, true, true, false);
                }
                if (middleCell.coords.y > pastCell.coords.y)
                {
                    pastCell.DrawLines(true, false, true, true);
                    middleCell.DrawLines(false, true, true, true);
                }
                if (middleCell.coords.y < pastCell.coords.y)
                {
                    pastCell.DrawLines(false, true, true, true);
                    middleCell.DrawLines(true, false, true, true);
                }
            }
            if (currentPath.pathCoords.Count > 1)
            {
                CellClass currentCell = GetCellAtCoords(currentPos);
                CellClass middleCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 1]);
                CellClass pastCell = GetCellAtCoords(currentPath.pathCoords[currentPath.pathCoords.Count - 2]);
                Debug.Log(pastCell.coords + " " + middleCell.coords + " " + currentCell.coords);
                currentPath.pathCoords.Add(coords);
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
            else
            {
                currentPath.pathCoords.Add(coords);
            }
        }
    }

    public void NewPath(Vector2 startCoords)
    {
        Path newPath = new Path();
        placedPaths.Add(newPath);
        currentPath = placedPaths[placedPaths.Count - 1];
        currentPath.StartPath(startCoords);
    }

    public bool CheckIfPathIntersects(Vector2 nextCoords)
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
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).cellColor = Color.white;
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).cellText.text = "";
                        GetCellAtCoords(placedPaths[i].pathCoords[k]).InitTextColor();
                    }
                    placedPaths.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public CellClass GetCellAtCoords(Vector2 cellCoords)
    {
        if (puzzleManager.GetCellList().Count > 0)
        {
            for (int i = 0; i <= puzzleManager.GetCellList().Count - 1; i++)
            {
                if (puzzleManager.GetCellList()[i].coords == cellCoords)
                {
                    return puzzleManager.GetCellList()[i];
                }
            }
        }
        return puzzleManager.GetCellList()[0];
    }

    public bool CheckIfBacktracking()
    {
        if (placedPaths.Count > 0)
        {
            if (placedPaths[placedPaths.Count - 1].pathCoords.Count - 1 > 0)
            {
                return currentPos.Equals(placedPaths[placedPaths.Count - 1].pathCoords[placedPaths[placedPaths.Count - 1].pathCoords.Count - 2]);
            }
        }
        return false;
    }

    public void SetPathColor(string newColor)
    {
        switch (newColor)
        {
            case "Black":
                pathColor = Color.black;
                break;
            case "Red":
                pathColor = Color.red;
                break;
            case "Orange":
                pathColor = new Color(1f, 0.5f, 0f, 1f);
                break;
            case "Yellow":
                pathColor = Color.yellow;
                break;
            case "Green":
                pathColor = Color.green;
                break;
            case "DarkGreen":
                pathColor = new Color(0f, 0.5f, 0f, 1f);
                break;
            case "Cyan":
                pathColor = Color.cyan;
                break;
            case "Blue":
                pathColor = Color.blue;
                break;
            case "Brown":
                pathColor = new Color(0.43f, 0.29f, 0f, 1f);
                break;
            case "Tan":
                pathColor = new Color(0.76f, 0.65f, 0.42f, 1f);
                break;
            case "Purple":
                pathColor = new Color(0.5f, 0f, 1f, 1f);
                break;
            case "Magenta":
                pathColor = Color.magenta;
                break;
            case "DarkGrey":
                pathColor = new Color(0.25f, 0.25f, 0.25f, 1f);
                break;
            case "Grey":
                pathColor = Color.grey;
                break;
            default:
                pathColor = Color.black;
                break;
        }
        colorName = newColor;
    }

    public void RemoveAllPaths()
    {
        placedPaths.RemoveRange(0, placedPaths.Count);
        puzzleManager.correctPaths.RemoveRange(0, puzzleManager.correctPaths.Count);
        puzzleManager.placedPaths.RemoveRange(0, puzzleManager.placedPaths.Count);
    }

    public void SendPuzzleName(TMPro.TMP_InputField targetText)
    {
        targetText.text = puzzleManager.puzzleName;
    }

    public void ReceivePuzzleName(TMPro.TMP_InputField targetText)
    {
        if(targetText.text == "")
        {
            targetText.text = "Untitled";
        }
        puzzleManager.puzzleName = targetText.text;
    }
}
