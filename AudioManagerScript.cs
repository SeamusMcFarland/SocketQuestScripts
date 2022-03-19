using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{

    public AudioSource sfxSource, musicSource, pullingSource;

    public AudioClip partBreak, partDetatch, partDrop, partDamage, pointGain, explode;

    GameManagerScript gmS;

    bool breakPlayed, damagePlayed;

    // Start is called before the first frame update
    void Start()
    {
        gmS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        pullingSource.volume = gmS.GetPullVolume();
        breakPlayed = false;
        damagePlayed = false;
    }

    public void PlaySound(string s)
    {
        if (s == "partbreak")
        {
            if (!breakPlayed)
            {
                breakPlayed = true;
                sfxSource.PlayOneShot(partBreak);
            }
        }
        else if (s == "partdetatch")
        {
            sfxSource.PlayOneShot(partDetatch);
        }
        else if(s == "partdrop")
        {
            sfxSource.PlayOneShot(partDrop);
        }
        else if(s == "partdamage")
        {
            if (!damagePlayed)
            {
                damagePlayed = true;
                sfxSource.PlayOneShot(partDamage);
            }
        }
        else if(s == "pointgain")
        {
            sfxSource.PlayOneShot(pointGain);
        }
        else if (s == "explode")
        {
            sfxSource.PlayOneShot(explode);
        }
    }


}
