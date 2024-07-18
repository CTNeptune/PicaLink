using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorHelper : MonoBehaviour
{
    Button[] childButtons = new Button[0];
    PuzzleEditor puzzleEditor;

    private void OnEnable()
    {
        if (!puzzleEditor)
        {
            puzzleEditor = GameObject.FindObjectOfType<PuzzleEditor>();
        }
        if (childButtons.Length == 0)
        {
            childButtons = transform.GetComponentsInChildren<Button>();
        }
    }
    public void SetColor()
    {
        for(int i = 0; i <= childButtons.Length - 1; i++)
        {
            childButtons[i].interactable = !childButtons[i].name.Equals(puzzleEditor.colorName);
        }
    }
}
