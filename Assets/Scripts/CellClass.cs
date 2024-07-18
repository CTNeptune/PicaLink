using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CellClass : MonoBehaviour
{
    public Vector2 coords;
    public PuzzleManager manager;

    public bool isStartOrEnd;
    public Color cellColor = Color.white;
    public TMPro.TextMeshPro cellText;
    public bool interactable = true;

#pragma warning disable 0649
    [SerializeField]
    LineRenderer innerUpLine, innerDownLine, innerLeftLine, innerRightLine;
#pragma warning restore 0649

    public SpriteRenderer cellImage;

    private void Awake()
    {
        cellImage = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (interactable && !EventSystem.current.IsPointerOverGameObject())
        {
            manager.SetPos(coords);
            manager.OnPointerClickDownCell(this);
        }
    }

    void OnMouseOver()
    {
        if (interactable && !EventSystem.current.IsPointerOverGameObject())
        {
            manager.OnPointerEnterCell(this);
        }
    }

    void OnMouseUp()
    {
        if (interactable && !EventSystem.current.IsPointerOverGameObject())
        {
            manager.OnPointerClickUpCell(manager.GetCellAtCoords(manager.GetCurrentPos()));
        }
    }

    public void InitTextColor()
    {
        if(isStartOrEnd)
        {
            cellText.color = cellColor;
        }
        else
        {
            cellText.color = Color.white;
        }
    }

    public IEnumerator Blink()
    {
        Color blinkColor = cellImage.color;
        while (true)
        {
            if(cellImage.color == Color.white)
            {
                cellImage.color = blinkColor;
            }
            else
            {
                cellImage.color = Color.white;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ResetColor()
    {
        cellImage.color = Color.white;
    }

    public void SetColor(Color color)
    {
        cellColor = color;
        cellImage.color = color;
        
        if(cellImage.color == Color.black)
        {
            cellText.color = Color.white;
        }
        if (cellImage.color == Color.white)
        {
            cellText.color = Color.black;
        }
    }

    public void DrawLines(bool up, bool down, bool left, bool right)
    {
        if (up)
        {
            innerUpLine.enabled = up;
        }
        if (down)
        {
            innerDownLine.enabled = down;
        }
        if (left)
        {
            innerLeftLine.enabled = left;
        }
        if (right)
        {
            innerRightLine.enabled = right;
        }
    }

    public void ClearLines()
    {
        innerUpLine.enabled = false;
        innerDownLine.enabled = false;
        innerLeftLine.enabled = false;
        innerRightLine.enabled = false;
    }
}
