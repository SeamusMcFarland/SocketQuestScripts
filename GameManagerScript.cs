using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Threading;

public class GameManagerScript : MonoBehaviour
{
    bool grabLock; // determines if already grabbing
    bool overDropArea;

    Vector2 savedMousePos;
    Vector2 changeInMousePos;
    Camera cam;

    public Text winText;
    public Text loseText;
    public Text timerText;

    bool gameOver;

    string mouseOverName;

    public GameObject[] allCarParts;
    public SocketScript socketS;
    public int numLayers;

    int rand;
    float currentX;
    float currentY;
    int currentLayer;
    List<Vector3> usedSpaces = new List<Vector3>();

    Vector3 TILE_NUM;

    GameObject[] carPoints;
    GameObject[] carPointsShuffle;

    GameObject[] carPartList;

    public GameObject reloadButton;

    int sortOrd;

    public SlapEffectScript scoreSES;

    float timeRemaining;

    AudioManagerScript audioMS;

    public int[] pointModifiers;
    public Text[] pointModifiersText;
    public Text timerPointsText;
    public Text totalPointsText;

    int totalPoints;

    bool sMode;
    bool smashing;
    int smashesLeft;

    public PowerupScript pScript;
    public GrenadePowerupScript p2Script;

    int powerupsEnabled;

    float pullVolume;

    // Start is called before the first frame update
    void Start()
    {
        pullVolume = 0;
        powerupsEnabled = 0;

        sMode = false;

        audioMS = GameObject.FindGameObjectWithTag("audiomanager").GetComponent<AudioManagerScript>();
        timeRemaining = 60f;

        sortOrd = 1;

        carPoints = GameObject.FindGameObjectsWithTag("carpoint");
        carPointsShuffle = GameObject.FindGameObjectsWithTag("carpoint");
        carPartList = GameObject.FindGameObjectsWithTag("carpoint");
        AquireAndShuffleCarPoints();


        TILE_NUM = new Vector3(10f, 8f, 5f);

        mouseOverName = "";
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        grabLock = false;
        overDropArea = false;
        savedMousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        socketS.PlaceSocket();
        PlaceCarParts();
        
    }

    private void AquireAndShuffleCarPoints()
    {
        for (int i = 0; i < carPoints.Length; i++)
            carPoints[i] = null;
        for (int i = 0; i < carPoints.Length; i++)
        {
            do
            {
                rand = Random.Range(0, carPoints.Length);
            }
            while (carPoints[rand] != null);
            carPoints[rand] = carPointsShuffle[i];
        }
    }

    public void PlaceCarParts()
    {
        float layerAdjust = 0.02f;
        for (int i = 0; i < carPoints.Length; i++)
        {
            rand = (int)Random.Range(0, allCarParts.Length);
            carPartList[i] = Instantiate(allCarParts[rand], new Vector3(carPoints[i].transform.position.x, carPoints[i].transform.position.y, carPoints[i].transform.position.z - layerAdjust), Quaternion.identity);
            carPartList[i].GetComponent<SpriteRenderer>().sortingOrder = i + 1; 
            carPartList[i].name = "Car Part " + i;
            layerAdjust += 0.01f;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            CheckPowerups();

            if (sMode && Input.GetMouseButtonDown(0) && !smashing && mouseOverName.Contains("Car Part"))
                SledgehammerSmash();
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                timerText.color = Color.red;
                Lose();
            }
            timerText.text = "" + ((float)Mathf.Round(timeRemaining * 100f)) / 100f;
        }
        else
        {
            RequestGrabLock();
        }


        if (Vector2.Distance((Vector2)cam.ScreenToWorldPoint(Input.mousePosition), savedMousePos) > 0)
        {
            changeInMousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition) - savedMousePos;
            savedMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        CheckMouseOver();
    }

    private void CheckPowerups()
    {
        if (powerupsEnabled == 0 && timeRemaining < 50f)
        {
            p2Script.ActivatePowerup();
            powerupsEnabled = 1;
        }
        else if (powerupsEnabled == 1 && timeRemaining < 35f)
        {
            pScript.ActivatePowerup();
            powerupsEnabled = 2;
        }
    }

    private void SledgehammerSmash()
    {
        // Play SFX here
        smashing = true;
        smashesLeft--;

        GameObject.Find(mouseOverName).GetComponent<CarPartScript>().Smash();
        smashing = false;
        if (smashesLeft < 1)
            EndSledgehammerSmash();
    }

    private void EndSledgehammerSmash()
    {
        sMode = false;
        pScript.EndPowerup();
    }

    public bool RequestGrabLock()
    {
        if (grabLock)
            return false;
        else
        {
            grabLock = true;
            return true;
        }
    }

    public void ReleaseGrabLock()
    {
        grabLock = false;
    }

    public bool GetOverDropArea()
    {
        return overDropArea;
    }

    public void SetOverDropArea(bool b)
    {
        overDropArea = b;
    }

    public Vector2 GetMouseVelocity()
    {
        return changeInMousePos;
    }

    public float GetMaxBreakage(int t)
    {
        float maxBreakage = -2f;
        switch (t)
        {
            case 0:
                maxBreakage = 2.4f;
                break;
            case 1:
                maxBreakage = 1.8f;
                break;
            case 2:
                maxBreakage = 1.5f;
                break;
            case 3:
                maxBreakage = 1.3f;
                break;
            case 4:
                maxBreakage = 0.3f;
                break;
            case 5:
                maxBreakage = 1f;
                break;
            case 6:
                maxBreakage = 0.2f;
                break;
        }
        return maxBreakage;
    }

    public void Win()
    {
        for (int i = 0; i < pointModifiers.Length; i++)
        {
            print("looprun");
            pointModifiersText[i].text = "+" + pointModifiers[i];
            totalPoints += pointModifiers[i];
        }
        timerPointsText.text = "+" + (int)(timeRemaining * 100f);
        totalPoints += (int) (timeRemaining * 100f);
        totalPointsText.text = "" + totalPoints;

        gameOver = true;
        winText.enabled = true;
        reloadButton.SetActive(true);
        scoreSES.Slap();
    }

    public void Lose()
    {
        gameOver = true;
        loseText.enabled = true;
        reloadButton.SetActive(true);
    }

    private void CheckMouseOver()
    {
        /*Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        Vector3 screenPos = cam.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(screenPos, Vector2.zero);

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);*/

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 5f;

        Vector2 v = Camera.main.ScreenToWorldPoint(mousePosition);

        Collider2D[] col = Physics2D.OverlapPointAll(v);


        if (col.Length > 0)
        {
            Collider2D chosenColl = col[0];
            foreach (Collider2D c in col)
            {
                if (chosenColl.transform.position.z > c.transform.position.z)
                    chosenColl = c;
            }

            SetMouseOverName(chosenColl.transform.name);
        }
    }

    private void SetMouseOverName(string s)
    {
        mouseOverName = s;
    }

    public string GetMouseOverName()
    {
        return mouseOverName;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int RequestOrder()
    {
        sortOrd++;
        return sortOrd;
    }

    public void IncreaseModifier(int t)
    {
        switch (t)
        {
            case 0:
            pointModifiers[0] += 100;
                break;
            case 1:
                pointModifiers[1] += 70;
                break;
            case 2:
                pointModifiers[2] += 80;
                break;
            case 3:
                pointModifiers[3] += 120;
                break;
            case 4:
                pointModifiers[4] += 20;
                break;
            case 5:
                pointModifiers[5] += 15;
                break;
            case 6:
                pointModifiers[6] += 10;
                break;
        }
    }

    public void SledgehammerMode()
    {
        sMode = true;
        smashesLeft = 8;
    }

    public float GetPullVolume()
    {
        print("getpull: " + pullVolume);
        return pullVolume;
    }

    public void SetPullVolume(float p)
    {
        print("setpull:" + p);
        pullVolume = p;
    }

    /*
     
     for (int i = 0; i < 30; i++)
        {
            currentLayer = 0;
            rand = (int)Random.Range(0, allCarParts.Length);
            bool breakout = false;
            while (currentLayer + GetDimensions(rand).z < TILE_NUM.z && !breakout)
            {
                currentX = 0;
                currentY = 0;
                while (currentY + GetDimensions(rand).y < TILE_NUM.y && !breakout)
                {
                    while (currentX + GetDimensions(rand).x < TILE_NUM.x  && !breakout)
                    {
                        if ()
                        {
                            breakout = true;
                        }
                        if (!breakout)
                            currentX += 1f;
                    }
                    if (!breakout)
                    {
                        currentX = 0;
                        currentY += 1f;
                    }
                }

                if (currentLayer > numLayers) // should never occur
                    break;
            }
            if (breakout)
            {
                for (int l = (int)currentLayer; l < (int)currentLayer + (int)GetDimensions(rand).z; l++)
                {
                    for (int y = (int)currentLayer; y < currentLayer + (int)GetDimensions(rand).y; y++)
                    {
                        for (int x = (int)currentLayer; x < (int)currentLayer + (int)GetDimensions(rand).x; x++)
                        {
                            usedSpaces = 
                        }
                    }
                }

                Instantiate(allCarParts[rand], new Vector3(currentX - (TILE_NUM.x / 2f), currentY - (TILE_NUM.y / 2f), currentLayer - (TILE_NUM.z / 2f)), Quaternion.identity);
            }
        }
     */
}
