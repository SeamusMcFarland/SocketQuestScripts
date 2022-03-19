using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupScript : MonoBehaviour
{

    GameManagerScript gmS;
    public SpriteRenderer crossSR;
    Collider2D coll;
    SpriteRenderer sr;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
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
        transform.position = new Vector3(-8f, 8f, transform.position.z);
        rb.velocity = new Vector2(4f, -4f);
        rb.AddTorque(Random.Range(-400f, 400f));
    }

    private void FunctionalMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && gmS.RequestGrabLock())
        {
            coll.enabled = false;
            sr.enabled = false;
            crossSR.enabled = true;
            gmS.SledgehammerMode();
        }
    }

    public void EndPowerup()
    {
        gmS.ReleaseGrabLock();
        crossSR.enabled = false;
    }

}
