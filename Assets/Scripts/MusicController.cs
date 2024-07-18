using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text.RegularExpressions;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public enum PLAYBACK_TYPE
    {
        Linear,
        Repeat,
        Shuffle
    }
    public PLAYBACK_TYPE playbackType = PLAYBACK_TYPE.Linear;

    public AudioSource audioSource;
    public string musicDirectory;
    public string currentSongName;
    public int songPositionInQueue;

    [SerializeField]
    private GameObject songButtonPrefab;
    [SerializeField]
    private Transform scrollParent;
    [SerializeField]
    private TextMeshProUGUI nowPlayingText;
    [SerializeField]
    private TextMeshProUGUI playModeText;
    [SerializeField]
    private TextMeshProUGUI playPauseText;

    [SerializeField]
    private RectTransform playerTransform;
    Vector3 initTarget;
    bool toggled;
    bool isToggling;

    public List<AudioClip> songs;

    private void Awake()
    {
        RefreshSongs();
        initTarget = playerTransform.anchoredPosition;
    }

    IEnumerator WaitForEndOfSong()
    {
        yield return new WaitUntil(() => audioSource.isPlaying == false && audioSource.time == 0);
        switch (playbackType)
        {
            case PLAYBACK_TYPE.Linear:
                Debug.Log(songPositionInQueue + " " + songs.Count);
                if(songPositionInQueue >= songs.Count - 1)
                {
                    songPositionInQueue = 0;
                }
                else
                {
                    songPositionInQueue = Mathf.Clamp(songPositionInQueue + 1, 0, songs.Count - 1);
                }
                break;
            case PLAYBACK_TYPE.Shuffle:
                songPositionInQueue = UnityEngine.Random.Range(0, songs.Count - 1);
                break;
            case PLAYBACK_TYPE.Repeat:
                PlaySong();
                break;
        }
        PlaySong();
        yield return null;
    }

    public void SimulateEndSong()
    {
        audioSource.Stop();
    }

    public void TogglePlaybackMode()
    {
        if(playbackType == PLAYBACK_TYPE.Shuffle)
        {
            playbackType = PLAYBACK_TYPE.Linear;
        }
        else
        {
            playbackType++;
        }
        if (playModeText)
        {
            playModeText.text = playbackType.ToString();
        }
    }

    public void RefreshSongs()
    {
        foreach (var song in songs)
        {
            SongButton sb = GameObject.Instantiate(songButtonPrefab).GetComponent<SongButton>();
            sb.gameObject.transform.SetParent(scrollParent);
            sb.musicController = this;
            sb.gameObject.name = song.name;
        }
        /*
        if(musicDirectory == "")
        {
            musicDirectory = System.IO.Path.Combine(Application.streamingAssetsPath, "Music");
        }
        songs.Clear();
        StartCoroutine(BuildSongList(musicDirectory));
        */
    }

    IEnumerator BuildSongList(string currentDirectory)
    {
        int i = 0;
        DirectoryInfo directoryInfo = new DirectoryInfo(musicDirectory);
#if UNITY_WEBGL //&& !UNITY_EDITOR
        List<string> files = new List<string>();
        if (musicDirectory.Contains("ssl.hwcdn.net"))
        {
            string filePath = Application.streamingAssetsPath + "/music.txt";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(filePath))
            {
                    yield return webRequest.SendWebRequest();
                    if (webRequest.isNetworkError)
                    {
                        Debug.Log(webRequest.error);
                    }
                    string result = webRequest.downloadHandler.text;
                    Debug.Log(result);
                    StringReader stringReader = new StringReader(result);
                    string line;
                    int lineCount = 0;
                    while ((line = stringReader.ReadLine()) != null)
                    {
                        lineCount++;
                        files.Add(line);
                    }
                    stringReader.Close();
                    Debug.Log(lineCount + " songs found.");
            }
        }
        else
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(musicDirectory))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
                MatchCollection matches = regex.Matches(webRequest.downloadHandler.text);

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    foreach (Match match in matches)
                    {
                        if (!match.Success) { continue; }
                        if (!match.Groups["name"].Value.Contains("Parent Directory") && !match.Groups["name"].Value.Contains("Description") && !match.Groups["name"].Value.Contains(".json") && !match.Groups["name"].Value.Contains(".txt"))
                        {
                            string tempString = match.Groups["name"].Value;
                            Debug.Log(tempString);
                            files.Add(tempString);
                        }
                    }
                }
            }
        }
        
        while (i < files.Count)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(Application.streamingAssetsPath + "/Music/" + files[i].ToString(), AudioType.AUDIOQUEUE))
            {
                ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }

                DownloadHandlerAudioClip downloadHandler = (DownloadHandlerAudioClip)www.downloadHandler;

                if (downloadHandler.isDone)
                {
                    AudioClip audioClip = downloadHandler.audioClip;
                    if (audioClip != null)
                    {
                        audioClip.name = Regex.Replace(System.IO.Path.GetFileNameWithoutExtension(files[i]), "([0-9])\\d+\\s-\\s", "");
                        songs.Add(DownloadHandlerAudioClip.GetContent(www));
                        SongButton sb = GameObject.Instantiate(songButtonPrefab).GetComponent<SongButton>();
                        sb.gameObject.transform.SetParent(scrollParent);
                        sb.musicController = this;
                        sb.gameObject.name = audioClip.name;
                    }
                }
            }
            i++;
        }
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
        while (i < directoryInfo.GetFiles().Length - 1)
        {
            if (directoryInfo.GetFiles()[i].ToString().EndsWith(".meta"))
            {
                i++;
                yield return new WaitForEndOfFrame();
            }
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(directoryInfo.GetFiles()[i].ToString(), AudioType.UNKNOWN))
            {
                ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                yield return www.SendWebRequest();

                if(www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }

                DownloadHandlerAudioClip downloadHandler = (DownloadHandlerAudioClip)www.downloadHandler;

                if (downloadHandler.isDone)
                {
                    AudioClip audioClip = downloadHandler.audioClip;
                    if(audioClip != null)
                    {
                        audioClip.name = Regex.Replace(System.IO.Path.GetFileNameWithoutExtension(directoryInfo.GetFiles()[i].Name), "([0-9])\\d+\\s-\\s", "");
                        songs.Add(DownloadHandlerAudioClip.GetContent(www));
                        SongButton sb = GameObject.Instantiate(songButtonPrefab).GetComponent<SongButton>();
                        sb.gameObject.transform.SetParent(scrollParent);
                        sb.musicController = this;
                        sb.gameObject.name = audioClip.name;
                    }
                }
            }
            i++;
        }
        
#endif
        yield return null;
    }

    public void PlaySong()
    {
        try
        {
            audioSource.clip = songs[songPositionInQueue];
        }catch(Exception e)
        {
            Debug.LogError(e);
        }
        if (audioSource)
        {
            if(audioSource.clip != null)
            {
                audioSource.Play();
                currentSongName = audioSource.clip.name;
                nowPlayingText.text = "Now Playing: " + currentSongName;
                playPauseText.text = "Pause";
                StartCoroutine(WaitForEndOfSong());
            }
        }
    }

    public void PausePlay()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            playPauseText.text = "Play";
        }
        else
        {
            audioSource.UnPause();
            playPauseText.text = "Pause";
        }
    }

    public void NextOrPreviousSong(int queueDelta)
    {
        songPositionInQueue = Mathf.Clamp(songPositionInQueue + queueDelta, 0, songs.Count - 1);
        PlaySong();
    }

    public void ToggleMusicPlayer()
    {
        if (!isToggling)
        {
            StartCoroutine(ShowHide());
            toggled = !toggled;
        }
    }

    IEnumerator ShowHide()
    {
        if (playerTransform)
        {
            isToggling = true;
            Vector3 target = Vector3.zero;
            if (toggled)
            {
                target = initTarget;
            }
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
                playerTransform.anchoredPosition = Vector3.Lerp(playerTransform.anchoredPosition, target, t);
                yield return new WaitForEndOfFrame();
            }
        }
        isToggling = false;
        yield return null;
    }
}
