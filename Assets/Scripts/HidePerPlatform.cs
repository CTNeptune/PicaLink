using UnityEngine;

public class HidePerPlatform : MonoBehaviour
{
    public bool hideOnStandalone, hideOnIOS, hideOnAndroid, hideOnWebGL, showInEditor;
    private void Awake()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                if (hideOnStandalone)
                {
                    gameObject.SetActive(false);
                }
#if UNITY_WEBGL
                if (hideOnWebGL)
                {
                    gameObject.SetActive(false);
                }
#endif
#if UNITY_IOS
                if (hideOnIOS)
                {
                    gameObject.SetActive(false);
                }
#endif
#if UNITY_ANDROID
                if (hideOnAndroid)
                {
                    gameObject.SetActive(false);
                }
#endif
                break;
            case RuntimePlatform.WindowsPlayer:
                if (hideOnStandalone)
                {
                    gameObject.SetActive(false);
                }
            break;
            case RuntimePlatform.OSXPlayer:
                if (hideOnStandalone)
                {
                    gameObject.SetActive(false);
                }
                break;
            case RuntimePlatform.LinuxPlayer:
                if (hideOnStandalone)
                {
                    gameObject.SetActive(false);
                }
                break;
            case RuntimePlatform.IPhonePlayer:
                if (hideOnIOS)
                {
                    gameObject.SetActive(false);
                }
                break;
            case RuntimePlatform.Android:
                if (hideOnAndroid)
                {
                    gameObject.SetActive(false);
                }
                break;
            case RuntimePlatform.WebGLPlayer:
                if (hideOnWebGL)
                {
                    gameObject.SetActive(false);
                }
                break;
        }
#if UNITY_EDITOR
        if (showInEditor)
        {
            gameObject.SetActive(true);
        }
#endif
    }
}
