using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarkBiteScript : MonoBehaviour {
    private float startTime;
    private float timeToDestroy;

	// Use this for initialization
	void Start () {
        startTime = Time.time;
        timeToDestroy = 1.0f;

    }
	
	// Update is called once per frame
	void Update () {
        if (Time.time > startTime + timeToDestroy)
        {
            GetComponent<AudioSource>().Play();
            Destroy(gameObject);
        }
	}
}
