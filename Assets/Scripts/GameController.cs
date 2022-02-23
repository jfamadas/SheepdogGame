using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {

    public Text timerText;
    public Text infoSheepsInText;

    private GameObject[] sheepList;  // Reference to all sheeps
    private Collider finishVolume; // Reference to finish volume game object
    private float startTime;
    private bool allin;
    public float secondsToFinish;
    public float finishCounter = 0.0f;

    // Use this for initialization
    void Start () {
        startTime = Time.time;
        sheepList = GameObject.FindGameObjectsWithTag("Sheep");
        finishVolume = GameObject.FindGameObjectWithTag("FinishVolume").GetComponent<Collider>();
        allin = false;
        finishCounter = 0.0f;
        secondsToFinish = 5.0f;
    }
	

	// Update is called once per frame
	void Update () {
        float t = Time.time - startTime;

        if (t > 220)
        {
            timerText.text = ("YOU LOST");
            timerText.fontSize = 80;
            FindObjectOfType<GameManager>().GameOver();
        }
        else
        {
            string minutes = ((int)t / 60).ToString();
            string seconds = (t % 60).ToString("f1");   // the "f" parameter defines how many decimals you want
            timerText.text = minutes + ":" + seconds;
        }

        // Check how many sheeps are in the fence
        int sheepsIn = 0;
        

        for (int i = 0; i < sheepList.Length; i++)
        {
            if (finishVolume.bounds.Contains(sheepList[i].transform.position))
            {
                sheepsIn += 1;
            }
        }

        // Check whether all sheeps are in the fence
        if (sheepsIn>= sheepList.Length)
        {
            if (!allin)
            {
                finishCounter = Time.time;
            }
            allin = true;
        }
        else
        {
            allin = false;
            finishCounter = 0.0f;
        }


        // Print info about position of the sheeps
        if (!allin)
        {
            infoSheepsInText.text = string.Format("Sheeps in: {0}/{1}", sheepsIn, sheepList.Length);
        }
        else
        {
            secondsToFinish = Mathf.Max(finishCounter+5.0f-Time.time,0.0f);   // the "f" parameter defines how many decimals you want
            if (secondsToFinish <= 0.0f)
            {
                FindObjectOfType<GameManager>().Win(t, SceneManager.GetActiveScene().buildIndex);
            }
            infoSheepsInText.text = string.Format("Sheeps in: {0}/{1}\nTime to finish: {2:0.00}", sheepsIn, sheepList.Length, secondsToFinish);
        }
        
    }
}
