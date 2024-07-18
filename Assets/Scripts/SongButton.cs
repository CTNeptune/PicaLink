using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongButton : MonoBehaviour
{
    public MusicController musicController;

    private void Start()
    {
        TMPro.TextMeshProUGUI text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text)
        {
            text.text = gameObject.name;
        }
    }

    public void TriggerPlayback()
    {
        if (musicController)
        {
            for(int i = 0; i <= musicController.songs.Count - 1; i++)
            {
                if(musicController.songs[i].name == gameObject.name)
                {
                    musicController.songPositionInQueue = i;
                    musicController.PlaySong();
                }
            }
        }
    }
}
