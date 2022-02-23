using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour {
    [Header("Player Control Attributes")]
    public float speedMovementFast;
    public float speedMovementSlow;
    public float speedMovement;
    public float speedRotation;

    private float turnY;
    private Rigidbody rb;

    public float barkRate;
    public float biteRate;
    private float nextBark;
    private float nextBite;
    public GameObject bark;
    public GameObject bite;

    public float animationSpeed;
    private Animator animator;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        barkRate = 1.0f;
        biteRate = 1.0f;
        //speedMovement = 40f;
        //speedRotation = 10f;
        animator = GetComponent<Animator>();
        animationSpeed = 0.0f;
        speedMovementFast = 20f;
        speedMovementSlow = 10f;
        speedMovement = 0f;
        speedRotation = 5f;
}       

    // Update is called once per frame
    void Update()
    {
        // Run
        if (Input.GetButton("Run"))
        {
            speedMovement = speedMovementFast;
        }
        else
        {
            speedMovement = speedMovementSlow;
        };

        // Bite 
        if (Input.GetButton("Bite") && Time.time > nextBite)
        {
            nextBite = Time.time + biteRate;
            Instantiate(bite, rb.position, rb.rotation);
            //GetComponent<AudioSource>().Play();
        };
        
        // Bark 
        if (Input.GetButton("Bark") && Time.time > nextBark)
        {
            nextBark = Time.time + barkRate;
            Instantiate(bark, rb.position, rb.rotation);
            //GetComponent<AudioSource>().Play();
        };

    }
    void FixedUpdate(){

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 inputMove = new Vector3(moveX * speedRotation, 0.0f, moveZ * speedMovement);
        Vector3 movement = transform.forward * inputMove.z;
        movement.y = rb.velocity.y; // Do not modify gravity 

        rb.velocity = movement; // Add inputMove.z to Rigidbodys forward facing velocity
        turnY += inputMove.x; // Rotate the player based on InputMove.x
        rb.rotation = Quaternion.Euler(0.0f, turnY, 0.0f); // Apply the rotation

        // Control animation
        animationSpeed = rb.velocity.magnitude / speedMovementFast;
        animator.SetFloat("Speed", animationSpeed);
    }
}
