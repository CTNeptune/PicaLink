using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

[assembly:AssemblyVersion("0.5.*")]
public class AutoVersion : MonoBehaviour
{
    Text textComponent;
    private void Awake()
    {
        textComponent = GetComponent<Text>();
        if (!textComponent)
        {
            TMPro.TextMeshProUGUI textMesh = GetComponent<TMPro.TextMeshProUGUI>();
            textMesh.text += Assembly.GetExecutingAssembly().GetName().Version;
            return;
        }
        textComponent.text += Assembly.GetExecutingAssembly().GetName().Version;
    }
}
