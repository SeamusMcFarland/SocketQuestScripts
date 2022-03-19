using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePowerupScript : MonoBehaviour
{
    GameManagerScript gmS;
    Collider2D coll;
    SpriteRenderer sr;
    Rigidbody2D rb;

    List<GameObject> grenadeTargets = new List<GameObject>();
    List<GameObject> allParts = new List<GameObject>();

    AudioManagerScript audioMS;

    // Start is called before the first frame update
    void Start()
    {
        audioMS = GameObject.FindGameObjectWithTag("audiomanager").GetComponent<AudioManagerScript>();
        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        gmS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
        rb = GetComponent<Rigidbody2D>();

        coll.enabled = false;
        sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gmS.GetMouseOverName() == this.name)
            FunctionalMouseOver();
    }

    public void ActivatePowerup()
    {
        coll.enabled = true;
        sr.enabled = true;
        transform.position = new Vector3(8f, 8f, transform.position.z);
        rb.velocity = new Vector2(-4f, -4f);
        rb.AddTorque(Random.Range(-400f, 400f));
    }

    private void FunctionalMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && gmS.RequestGrabLock())
        {
            audioMS.PlaySound("explode");
            coll.enabled = false;
            sr.enabled = false;

            foreach (GameObject o in GameObject.FindGameObjectsWithTag("carpart"))
                if (Vector2.Distance(o.transform.position, transform.position) < 2.3f)
                    grenadeTargets.Add(o);


            foreach (GameObject o in grenadeTargets)
            {
                CarPartScript cpS = o.GetComponent<CarPartScript>();
                cpS.Smash();
            }

            grenadeTargets.Clear();
            foreach (GameObject o in GameObject.FindGameObjectsWithTag("carpart"))
                if (Vector2.Distance(o.transform.position, transform.position) < 0.8f)
                    grenadeTargets.Add(o);

            foreach (GameObject o in grenadeTargets)
            {
                CarPartScript cpS = o.GetComponent<CarPartScript>();
                cpS.Smash();
            }

            gmS.ReleaseGrabLock();
        }
    }
}
