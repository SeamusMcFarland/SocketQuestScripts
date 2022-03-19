using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketScript : MonoBehaviour
{

    public float minX, maxX, minY, maxY;
    GameManagerScript gmS;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        gmS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
    }

    public void PlaceSocket()
    {
        transform.position = new Vector2(Random.Range(minX,maxX), Random.Range(minY, maxY));
    }

    // Update is called once per frame
    void Update()
    {
        CheckMouseOver();
    }

    private void CheckMouseOver()
    {
        if (gmS.GetMouseOverName() == this.name)
            FunctionalMouseOver();
    }

    private void FunctionalMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && gmS.RequestGrabLock())
        {
            gmS.Win();
            gmS.ReleaseGrabLock();
        }
    }
}
