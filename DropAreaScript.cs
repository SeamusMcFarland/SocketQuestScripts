using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DropAreaScript : MonoBehaviour
{

    GameManagerScript gmS;
    bool detectAlt; // prevents from constantly setting overdroparea
    Camera cam;
    float xDropArea;

    // Start is called before the first frame update
    void Start()
    {
        xDropArea = 3.6f;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        detectAlt = true;
        gmS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(cam.ScreenToWorldPoint(Input.mousePosition).x) > xDropArea && detectAlt)
        {
            gmS.SetOverDropArea(true);
            detectAlt = false;
        }
        else if (Mathf.Abs(cam.ScreenToWorldPoint(Input.mousePosition).x) < xDropArea && !detectAlt)
        {
            gmS.SetOverDropArea(false);
            detectAlt = true;
        }
    }
}
