using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlapEffectScript : MonoBehaviour
{
    Text theText;
    public SlapEffectScript chainSES;
    AudioManagerScript audioMS;
    int savedFontSize;

    // Start is called before the first frame update
    void Start()
    {
        theText = GetComponent<Text>();
        savedFontSize = theText.fontSize;
        theText.enabled = false;
        audioMS = GameObject.FindGameObjectWithTag("audiomanager").GetComponent<AudioManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Slap()
    {
        theText.fontSize = savedFontSize * 2;
        theText.enabled = true;
        StartCoroutine("Fall");
    }

    IEnumerator Fall()
    {
        yield return new WaitForSeconds(0.01f);
        theText.fontSize -= 3;
        if (theText.fontSize <= savedFontSize)
            Punch();
        else
            StartCoroutine("Fall");
    }

    private void Punch()
    {
        audioMS.PlaySound("pointgain");
        if(chainSES != null)
            chainSES.Slap();
    }

}
