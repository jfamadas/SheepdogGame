using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogController_mode2 : MonoBehaviour {
    [Header("Player Control Attributes")]
    public float speedMovementFast;
    public float speedMovementSlow;
    public float speedMovement;
    public float speedRotation;
    public float distanceFar;
    public float distanceClose;
    public int actionCounter;
    public int iterationCounter;
    private Vector3 auxVec;

    private float turnY;
    private Rigidbody rb;

    public float barkRate;
    public float biteRate;
    public float nextBark;
    public float nextBite;
    public float stressLevel;
    public float decayFactor;
    public GameObject bark;
    public GameObject bite;

    public Vector3 targetDirection;
    public Vector3 targetPosition;
    public bool movingVertically;
    public bool movingHorizontally;
    public float animationSpeed;
    private Animator animator;

    public Vector3 currentPosition;
    public Vector3 positionToGo;
    private Vector3 randomDirection;
    private int waitCounter;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        distanceFar = 20.0f;
        distanceClose = 5.0f;
        barkRate = 1.0f;
        biteRate = 1.0f;
        speedMovementFast = 20f;
        speedMovementSlow = 10f;
        speedMovement = 0f;
        speedRotation = 5f;
        animator = GetComponent<Animator>();
        animationSpeed = 0.0f;
        targetDirection = Vector3.zero;
        targetPosition = transform.position;
        actionCounter = 0;
        iterationCounter = 100;
        decayFactor = 0.9f;
        stressLevel = 0.0f;
        nextBite = 0.0f;
        nextBark = 0.0f;
        movingVertically = false;
        movingHorizontally = false;
    }       

    // Update is called once per frame
    void Update()
    {

    }

    void stopMovement()
    {
        targetDirection = Vector3.zero;
        targetPosition = transform.position;
        movingVertically = false;
        movingHorizontally = false;
    }

    bool isMovementFinished()
    {
        bool movementFinished = true;

        if(targetDirection.x > 0)
        {
            movementFinished = movementFinished && transform.position.x >= targetPosition.x;
        }else if (targetDirection.x < 0)
        {
            movementFinished = movementFinished && transform.position.x <= targetPosition.x;
        }

        if (targetDirection.z > 0)
        {
            movementFinished = movementFinished && transform.position.z >= targetPosition.z;
        }
        else if (targetDirection.z < 0)
        {
            movementFinished = movementFinished && transform.position.z <= targetPosition.z;
        }

        return movementFinished;
    }

    void FixedUpdate(){
        currentPosition = transform.position;
        positionToGo = targetPosition;
        if (waitCounter == 0)
        {
            randomDirection = stressLevel * Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))) * 0.3f;

            waitCounter = Random.Range(100, 300);
        }
        if (!isMovementFinished()) {
            targetDirection += randomDirection;
        }
        waitCounter -= 1;
        
        Vector3 distanceToTarget = targetPosition - transform.position;

        float moveZ = 0;
        float moveX = 0;

        float angle = Vector3.Angle(targetDirection, transform.forward);
        moveX = angle / 180;
        
        //Get direction of rotation
        Vector3 cross = Vector3.Cross(targetDirection, transform.forward);
        if (cross.y > 0) moveX = -moveX;

        if (!isMovementFinished())
        {
            moveZ = 1 - angle / 180;
        }
        else
        {
            moveZ = 0;
            stopMovement();
        }

        Vector3 inputMove = new Vector3(moveX * speedMovement, 0.0f, moveZ * speedMovement);
        Vector3 movement = transform.forward * inputMove.z;
        movement.y = rb.velocity.y; // Do not modify gravity 

        rb.velocity = movement; // Add inputMove.z to Rigidbodys forward facing velocity
        auxVec = movement;
        turnY += inputMove.x; // Rotate the player based on InputMove.x
        rb.rotation = Quaternion.Euler(0.0f, turnY, 0.0f); // Apply the rotation

        // Control animation
        animationSpeed = rb.velocity.magnitude / speedMovementFast;
        animator.SetFloat("Speed", animationSpeed);

        // Bite if stress is too high:
        float biteprob = stressLevel * 0.1f;
        float randval = Random.Range(0f, 1f);
        if (randval<biteprob & stressLevel>0.65f)
        {
            nextBite = Time.time + biteRate;
            Instantiate(bite, rb.position, rb.rotation);
            stressLevel = stressLevel / 2.0f;
        }
        // Stress Level
        iterationCounter += -1;
        if (iterationCounter == 0)
        {
            iterationCounter = 100;
            stressLevel = decayFactor * stressLevel + (1.0f-decayFactor)*actionCounter/10f;
            actionCounter = 0;
        }
    }

    public void buttonPressed(int type)
    {
        switch (type)
        {
            case 0:
                stopMovement();
                speedMovement = 0;
                actionCounter += 1;
                break;
            case 1:
                targetDirection.z = -distanceClose;
                targetDirection.x = 0;
                speedMovement = speedMovementSlow;
                actionCounter += 1;
                break;
            case 2:
                targetDirection.z = distanceClose;
                targetDirection.x = 0;
                speedMovement = speedMovementSlow;
                actionCounter += 1;
                break;
            case 3:
                targetDirection.x = distanceClose;
                targetDirection.z = 0;
                speedMovement = speedMovementSlow;
                actionCounter += 1;
                break;
            case 4:
                targetDirection.x = -distanceClose;
                targetDirection.z = 0;
                speedMovement = speedMovementSlow;
                actionCounter += 1;
                break;
            case 5:
                targetDirection.z = -distanceFar;
                targetDirection.x = 0;
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 6:
                targetDirection.z = distanceFar;
                targetDirection.x = 0;
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 7:
                targetDirection.x = distanceFar;
                targetDirection.z = 0;
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 8:
                targetDirection.x = -distanceFar;
                targetDirection.z = 0;
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 9:
                targetDirection.x = -distanceClose / Mathf.Sqrt(2);
                targetDirection.z = -distanceClose / Mathf.Sqrt(2);
                speedMovement = speedMovementSlow;
                actionCounter += 2;
                break;
            case 10:
                targetDirection.x = distanceClose / Mathf.Sqrt(2);
                targetDirection.z = -distanceClose / Mathf.Sqrt(2);
                speedMovement = speedMovementSlow;
                actionCounter += 2;
                break;
            case 11:
                targetDirection.x = distanceClose / Mathf.Sqrt(2);
                targetDirection.z = distanceClose / Mathf.Sqrt(2);
                speedMovement = speedMovementSlow;
                actionCounter += 2;
                break;
            case 12:
                targetDirection.x = -distanceClose / Mathf.Sqrt(2);
                targetDirection.z = distanceClose / Mathf.Sqrt(2);
                speedMovement = speedMovementSlow;
                actionCounter += 2;
                break;
            case 13:
                targetDirection.x = -distanceFar / Mathf.Sqrt(2);
                targetDirection.z = -distanceFar / Mathf.Sqrt(2);
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 14:
                targetDirection.x = distanceFar / Mathf.Sqrt(2);
                targetDirection.z = -distanceFar / Mathf.Sqrt(2);
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 15:
                targetDirection.x = distanceFar / Mathf.Sqrt(2);
                targetDirection.z = distanceFar / Mathf.Sqrt(2);
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 16:
                targetDirection.x = -distanceFar / Mathf.Sqrt(2);
                targetDirection.z = distanceFar / Mathf.Sqrt(2);
                speedMovement = speedMovementFast;
                actionCounter += 2;
                break;
            case 17:
                if (Time.time > nextBite)
                {
                    nextBite = Time.time + biteRate;
                    Instantiate(bite, rb.position, rb.rotation);
                    actionCounter += 8;
                }
                break;
            case 18:
                if (Time.time > nextBark)
                {
                    nextBark = Time.time + barkRate;
                    Instantiate(bark, rb.position, rb.rotation);
                    actionCounter += 8;
                }
                break;
            default:
                targetDirection = Vector3.zero;
                break;
        }

        targetPosition = transform.position + targetDirection;
    }
}
