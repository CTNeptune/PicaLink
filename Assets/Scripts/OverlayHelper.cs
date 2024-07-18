using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using TMPro;

public class OverlayHelper : MonoBehaviour
{
    SpriteRenderer sprite;
    public TMP_InputField offsetXField, offsetYField, scaleField;
    public TextMeshProUGUI errorText;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetTexture(TextMeshProUGUI textMesh)
    {
        StartCoroutine(DownloadImage(textMesh.text));
    }

    IEnumerator DownloadImage(string URL)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
        yield return www.SendWebRequest();
        if(www.isNetworkError || www.isHttpError)
        {
            if (errorText)
            {
                errorText.text = www.error;
                errorText.CrossFadeAlpha(1f, 0f, true);
                errorText.CrossFadeAlpha(0f, 3f, false);
            }
            Debug.Log(www.error);
        }
        else
        {
            Texture2D receivedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            receivedTexture.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(receivedTexture, new Rect(0f, 0f, receivedTexture.width, receivedTexture.height), new Vector2(.5f, 0.5f), 100f);
            sprite.sprite = newSprite;
            GridPopulator gridPopulator = GameObject.FindObjectOfType<GridPopulator>();
            transform.localScale = new Vector3(gridPopulator.spriteSize.x * 100f, gridPopulator.spriteSize.y * 100f, gridPopulator.spriteSize.x * 100f);
            transform.localPosition = new Vector3(0 + ((gridPopulator.puzzleSize.x - gridPopulator.spriteSize.x) / 2), 0 - ((gridPopulator.puzzleSize.y - gridPopulator.spriteSize.y) / 2), 0f);
            if (scaleField)
            {
                scaleField.text = transform.localScale.x.ToString();
            }
            if (offsetXField)
            {
                offsetXField.text = transform.localPosition.x.ToString();
            }
            if (offsetYField)
            {
                offsetYField.text = transform.localPosition.y.ToString();
            }
        }
        yield return null;
    }

    public void SetScale(string scale)
    {
        float newScale;
        float.TryParse(scale, out newScale);
        transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void SetOffsetX(string offset)
    {
        float newOffset;
        float.TryParse(offset, out newOffset);
        transform.localPosition = new Vector2(newOffset, transform.localPosition.y);
    }

    public void SetOffsetY(string offset)
    {
        float newOffset;
        float.TryParse(offset, out newOffset);
        transform.localPosition = new Vector2(transform.localPosition.x, newOffset);
    }

    public void SetOpacity(float opacity)
    {
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, opacity);
    }
}
