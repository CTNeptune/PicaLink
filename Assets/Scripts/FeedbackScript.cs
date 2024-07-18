using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FeedbackScript : MonoBehaviour
{
    public string username;
    public string message;
    public Text sentText;
    public string[] reportType = new string[] {"Report", "Bug", "Feature", "Suggestion" };
    public int reportSelection;

    float cooldown = 0f;

#if !UNITY_IOS
    [DllImport("__Internal")]
    private static extern void SendMail(string username, string report, string sentMessage, string sentAction);
#endif
    private void Start()
    {
        StartCoroutine(CooldownTimer());
    }

    IEnumerator CooldownTimer()
    {
        while (true)
        {
            if(cooldown > 0f)
            {
                cooldown -= Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void UpdateUsername(string sentString)
    {
        username = sentString;
    }
    public void UpdateMessage(string sentString)
    {
        message = sentString;
    }
    public void UpdateReportType(int sentSelection)
    {
        reportSelection = sentSelection;
    }
    
    public void SendMail()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if(cooldown > 0)
        {
            sentText.text = "Please wait " + Mathf.CeilToInt(cooldown/60) + " minutes.";
        }
        else
        {
            SendMail(username, reportType[reportSelection], message, "sendMail");
            sentText.text = "Sent!";
            cooldown = 300f;
        }
#else
        StartCoroutine(Sender());
        //UnityWebRequest.EscapeURL(username);
        //UnityWebRequest.EscapeURL(message);
        //Application.OpenURL("https://www.theneptune.site/mail/unity_contact_me.php?name="+username+"&phone=None&email_address=noreply@theneptune.site&message="+message);
#endif
    }

    IEnumerator Sender()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", username);
        form.AddField("type", reportType[reportSelection]);
        form.AddField("message", message);
        form.AddField("action", "sendMail");

        UnityWebRequest www = UnityWebRequest.Post("https://www.theneptune.site/mail/unity_contact_me.php", form);
        
        yield return www.SendWebRequest();

        Debug.Log("Response: " + www.responseCode + " " + www.downloadHandler.text);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            sentText.text = "There was an error. :( Please email cneptune@theneptune.site.";
        }
        else
        {
            Debug.Log("Email sent!");
            sentText.text = "Sent!";
        }
    }
}
