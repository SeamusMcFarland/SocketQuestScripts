using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CarPartScript : MonoBehaviour
{
    bool grabbed;
    bool detatched;
    GameManagerScript gmS;
    Rigidbody2D rb;

    ParticleSystem ps;

    Vector3 realMousePos;
    Camera cam;

    Vector2 savedPos;

    float xDiff, yDiff; // for mouse difference
    float shake;
    float breakage;


    Collider2D coll;

    public Sprite detatchedMaterial;
    public Sprite brokenMaterial;
    SpriteRenderer sr;

    public int type;
    bool smashed;

    AudioManagerScript audioMS;

    /*
    float distanceModifier;

    const float PULL_FORCE = 10f;
    const float MIN_FORCE_DISTANCE = 2f;
    const float MIN_BUFFER_DISTANCE = 1f;
    const float GRAB_SHAKE = 0.03f;
    */

    bool grabDisabled;

    // Start is called before the first frame update
    void Start()
    {
        grabDisabled = false;

        audioMS = GameObject.FindGameObjectWithTag("audiomanager").GetComponent<AudioManagerScript>();

        ps = GetComponentInChildren<ParticleSystem>();
        ps.Stop();

        smashed = false;

        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        
        gmS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
        rb = GetComponent<Rigidbody2D>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        savedPos = transform.position;

        breakage = 0;

        sr.sortingOrder = gmS.RequestOrder();
    }

    private void Update()
    {
        CheckMouseOver();

        if (grabbed)
        {
            if (Input.GetMouseButtonUp(0))
            {
                breakage = 0;
                gmS.SetPullVolume(breakage / gmS.GetMaxBreakage(type));
                if (!detatched)
                {
                    gmS.SetPullVolume(0);
                    gmS.ReleaseGrabLock();
                    grabbed = false;
                    transform.position = savedPos;
                }
                else // if (gmS.GetOverDropArea())
                {
                    gmS.ReleaseGrabLock();
                    grabbed = false;
                    Drop();
                    StartCoroutine("DelayDropSound");
                }
            }
            else if (!detatched)
            {
                print("breakage/max: "  + (breakage / gmS.GetMaxBreakage(type)) + " at: " + Time.time);
                breakage += Time.deltaTime;
                shake = Vector2.Distance(cam.ScreenToWorldPoint(Input.mousePosition), transform.position) / 25f;
                if (shake > 0.15f)
                    shake = 0.15f;
                transform.position = new Vector2(savedPos.x + Random.Range(-shake, shake), savedPos.y + Random.Range(-shake, shake));
                if (breakage >= gmS.GetMaxBreakage(type))
                {
                    BreakOff();
                    audioMS.PlaySound("partdetatch");
                }
            }
            else // if detatched
            {
                transform.position = new Vector2(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
            }
        }
        else
        {
            if (!detatched)
            {
                breakage = 0;
            }
        }
        if(breakage / gmS.GetMaxBreakage(type) > 0)
            gmS.SetPullVolume(breakage / gmS.GetMaxBreakage(type));
    }

    IEnumerator DelayDropSound()
    {
        yield return new WaitForSeconds(1f);
        audioMS.PlaySound("partdrop");
    }

    public void Smash()
    {
        if (!detatched)
        {
            if (!smashed)
            {
                audioMS.PlaySound("partdamage");
                ps.Play();
                StartCoroutine("EndSparks", 1f);
                smashed = true;
                sr.sprite = brokenMaterial;
            }
            else
            {
                audioMS.PlaySound("partbreak");
                BreakOff();
                Drop();
            }
        }
    }

    IEnumerator EndSparks(float time)
    {
        yield return new WaitForSeconds(time);
        ps.Stop();
    }

    public void BreakOff()
    {
        gmS.IncreaseModifier(type);
        ps.Play();
        StartCoroutine("EndSparks", 0.5f);
        detatched = true;
        breakage = 0;
        gmS.SetPullVolume(breakage / gmS.GetMaxBreakage(type));
        sr.sortingOrder = 100;
        if(!smashed)
            sr.sprite = detatchedMaterial;
    }

    public void Drop()
    {
        grabDisabled = true;
        breakage = 0;
        coll.isTrigger = false;
        coll.enabled = false;
        if (Vector2.Distance(gmS.GetMouseVelocity(), Vector2.zero) > 0f)
            rb.velocity = gmS.GetMouseVelocity() / Vector2.Distance(gmS.GetMouseVelocity(), Vector2.zero) * 15f;
        else // should technically never occur
            rb.velocity = new Vector2(Random.Range(-4f, 4f), Random.Range(2f, 8f));
        rb.AddTorque(Random.Range(-1000f,1000f));

    }

    private void CheckMouseOver()
    {
        if (gmS.GetMouseOverName() == this.name)
            FunctionalMouseOver();
    }

    private void FunctionalMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && !grabDisabled && gmS.RequestGrabLock())
        {
            grabbed = true;
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        if (grabbed)
        {
            print("grabbed at: " + Time.deltaTime);
            if (Input.GetMouseButtonUp(0))
            {
                gmS.ReleaseGrabLock();
                grabbed = false;
            }
            else
            {
                CheckPullForce();
                //transform.position = new Vector2(transform.position.x + Random.Range(-GRAB_SHAKE, GRAB_SHAKE), transform.position.y + Random.Range(-GRAB_SHAKE, GRAB_SHAKE));
            }
        }
        else
        {
            rb.AddForce(new Vector2(-rb.velocity.x / 0.01f, -rb.velocity.y / 0.01f));
        }
    }
    
    private void CheckPullForce()
    {
        xDiff = cam.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x;
        yDiff = cam.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y;
        //distanceModifier = Mathf.Pow(Mathf.Pow(xDiff, 2f) + Mathf.Pow(yDiff, 2f) , 0.8f);

        if (Mathf.Abs(xDiff) > MIN_FORCE_DISTANCE)
            rb.AddForce(new Vector2(xDiff, 0) * PULL_FORCE);
        else
            rb.AddForce(new Vector2(-rb.velocity.x, 0) * PULL_FORCE);

        if (Mathf.Abs(yDiff) > MIN_FORCE_DISTANCE)
            rb.AddForce(new Vector2(0, yDiff) * PULL_FORCE);
        else
            rb.AddForce(new Vector2(0, -rb.velocity.y) * PULL_FORCE);

        if(xDiff > MIN_FORCE_DISTANCE)
            rb.AddForce(new Vector2(xDiff, 0) * PULL_FORCE / distanceModifier);
        else if(xDiff > MIN_BUFFER_DISTANCE)
            rb.AddForce(new Vector2(-rb.velocity.x, 0) * PULL_FORCE / distanceModifier);

        if (yDiff > MIN_FORCE_DISTANCE)
            rb.AddForce(new Vector2(0, yDiff) * PULL_FORCE / distanceModifier);
        else if (yDiff > MIN_BUFFER_DISTANCE)
            rb.AddForce(new Vector2(0, -rb.velocity.y) * PULL_FORCE / distanceModifier);
    }
    */


}
