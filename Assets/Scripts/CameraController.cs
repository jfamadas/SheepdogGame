using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private GameObject dogObject;
    private float distanceToDog_x;
    private float distanceToDog_y;
    private float distanceToDog_z;

    // Use this for initialization
    void Start () {
        dogObject = GameObject.FindGameObjectWithTag("Sheepdog");

        distanceToDog_x = 0.0f;
        distanceToDog_y = 60.0f;
        distanceToDog_z = 60.0f;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(dogObject.transform.position.x + distanceToDog_x, dogObject.transform.position.y + distanceToDog_y, dogObject.transform.position.z + distanceToDog_z);
	}
}
