using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridPopulator : MonoBehaviour
{
    public GameObject cellPrefab;
    [SerializeField]
    PuzzleManager puzzleManager;
    [SerializeField]
    Grid grid;

    public Vector2 puzzleSize;
    public Vector2 spriteSize;

    private List<LineRenderer> gridLines = new List<LineRenderer>();

    public Material lineMaterial;

    private void Awake()
    {
        if (!puzzleManager)
        {
            puzzleManager = GameObject.FindObjectOfType<PuzzleManager>();
        }
        if (!grid)
        {
            grid = GetComponent<Grid>();
        }
        if(puzzleSize == Vector2.zero)
        {
            Sprite sprite = cellPrefab.GetComponent<SpriteRenderer>().sprite;
            spriteSize = new Vector2(sprite.texture.width, sprite.texture.height);
            puzzleSize = new Vector2(sprite.texture.width * puzzleManager.gridWidth, sprite.texture.height * puzzleManager.gridHeight);
        }
        //puzzleManager.SetPuzzleTransform(transform.parent.GetComponent<Transform>());
        //puzzleManager.SetCamera(Camera.main);
        //PopulateGrid(puzzleManager.gridWidth, puzzleManager.gridHeight);
    }

    public void UpdateGrid()
    {
        puzzleManager.ResetZoom();
        puzzleManager.ClearCellList();
        ClearGridLines();
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        PopulateGrid(puzzleManager.gridWidth, puzzleManager.gridHeight);
        Sprite sprite = cellPrefab.GetComponent<SpriteRenderer>().sprite;
        puzzleSize = new Vector2(sprite.texture.width * puzzleManager.gridWidth, sprite.texture.height * puzzleManager.gridHeight);
        if (puzzleManager.placedPaths.Count > 0)
        {
            for (int i = 0; i <= puzzleManager.placedPaths.Count - 1; i++)
            {
                for (int j = 0; j <= puzzleManager.placedPaths[i].pathCoords.Count - 1; j++)
                {
                    CellClass cell = puzzleManager.GetCellAtCoords(puzzleManager.placedPaths[i].pathCoords[j]);
                    cell.SetColor(puzzleManager.placedPaths[i].pathColor);

                    if (!cell.isStartOrEnd && j == puzzleManager.placedPaths[i].pathCoords.Count - 1)
                    {
                        cell.cellText.text = puzzleManager.placedPaths[i].pathCoords.Count.ToString();
                        cell.StartCoroutine("Blink");
                    }
                    if (j == 1)
                    {
                        puzzleManager.DrawLinesAroundCells(puzzleManager.GetCellAtCoords(puzzleManager.placedPaths[i].pathCoords[j - 1]), cell, null, j);
                    }
                    if (j > 1)
                    {
                        puzzleManager.DrawLinesAroundCells(puzzleManager.GetCellAtCoords(puzzleManager.placedPaths[i].pathCoords[j - 2]), puzzleManager.GetCellAtCoords(puzzleManager.placedPaths[i].pathCoords[j - 1]), cell, j);
                    }
                }
            }
        }
        puzzleManager.ResetZoom();
    }

    public void PopulateGrid(int gridWidth, int gridHeight)
    {
        //rectTransform.sizeDelta = new Vector2(gridWidth * gridLayout.cellSize.x, gridHeight * gridLayout.cellSize.y);
        Sprite cellSprite = cellPrefab.GetComponent<SpriteRenderer>().sprite;
        grid.cellSize = new Vector2(cellSprite.texture.width * 2, cellSprite.texture.height * 2);
        Debug.Log(grid.cellSize.x + " " + grid.cellSize.y + " " + grid.cellSize.x * grid.cellSize.y);
        for (int i = 0; i <= gridHeight - 1; i++)
        {
            for (int j = 0; j <= gridWidth- 1; j++)
            {
                GameObject tempCell = Instantiate(cellPrefab);
                CellClass cellClass = tempCell.GetComponent<CellClass>();
                cellClass.manager = puzzleManager;
                cellClass.coords = new Vector2(j, i);
                cellClass.isStartOrEnd = puzzleManager.CheckIsStartOrEndCell(cellClass.coords);
                if (cellClass.isStartOrEnd)
                {
                    cellClass.cellColor = puzzleManager.GetStartOrEndColor(cellClass.coords);
                    cellClass.InitTextColor();
                    cellClass.cellText.text = puzzleManager.GetPathLength(cellClass.coords).ToString();
                }
                puzzleManager.AddCellToList(cellClass);
                tempCell.transform.SetParent(this.gameObject.transform);
                tempCell.name = "Cell " + j + ", " + i;
                tempCell.transform.position = new Vector3(j * grid.cellSize.x / 2, -i * grid.cellSize.y / 2, 0f);
            }
            if (i != gridHeight - 1)
            {
                CreateGridLine(i, true);
            }
        }
        for(int i = 0; i <= gridWidth - 1; i++)
        {
            if (i != 0)
            {
                CreateGridLine(i, false);
            }
        }
    }

    void CreateGridLine(int index, bool horizontal)
    {
        GameObject lineObj = new GameObject();
        lineObj.transform.SetParent(transform);
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        if (horizontal)
        {
            lineObj.name = "HLine" + index;
            line.SetPosition(0, new Vector3(0 - (spriteSize.x / 2), (spriteSize.y * -index) - spriteSize.y / 2, 0));
            line.SetPosition(1, new Vector3((spriteSize.x * puzzleManager.gridWidth) - (spriteSize.x / 2), (spriteSize.y * -index) - spriteSize.y / 2, 0));
        }
        else
        {
            lineObj.name = "VLine" + index;
            line.SetPosition(0, new Vector3((spriteSize.x * index) - spriteSize.x / 2, (spriteSize.y / 2), 0));
            line.SetPosition(1, new Vector3((spriteSize.x * index) - spriteSize.x / 2, -(spriteSize.y * puzzleManager.gridHeight) + (spriteSize.y / 2), 0));
        }
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.material = lineMaterial;
        line.sortingOrder = 5;
        AddGridLineToList(line);
    }

    public void ClearGridLines()
    {
        gridLines.RemoveRange(0, gridLines.Count);
    }

    public void AddGridLineToList(LineRenderer lineToAdd)
    {
        gridLines.Add(lineToAdd);
    }

    public void ToggleGrid()
    {
        for(int i = 0; i <= gridLines.Count - 1; i++)
        {
            gridLines[i].enabled = !gridLines[i].enabled;
        }
    }

    public void ForceGridOff()
    {
        for (int i = 0; i <= gridLines.Count - 1; i++)
        {
            gridLines[i].enabled = false;
        }
    }

    public void SetGridColor(Color sentColor)
    {
        for (int i = 0; i <= gridLines.Count - 1; i++)
        {
            gridLines[i].startColor = sentColor;
            gridLines[i].endColor = sentColor;
        }
    }

    public void SetGridOpacity(float opacity)
    {
        Color newColor = new Color(gridLines[0].startColor.r, gridLines[0].startColor.g, gridLines[0].startColor.b, opacity);
        for (int i = 0; i <= gridLines.Count - 1; i++)
        {
            gridLines[i].startColor = newColor;
            gridLines[i].endColor = newColor;
        }
    }
}
