using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepSight_mode2 : MonoBehaviour
{
    [Header("References")]
    private SphereCollider col;     // Reference to the sheep colider object.
    public Rigidbody rb;            // Reference to the sheep rigidbody object.
    private GameObject sheepdog;    // Reference to the dog object. 
    private GameObject barkObject;  // Reference to the bark object. 
    private GameObject biteObject;  // Reference to the bite object. 
    public GameObject[] sheepList;  // Reference to the sheeps the current sheep has in its visual/hearing range.
    public GameObject[] fenceList;


    [Header("Sheep Senses")]
    public float fieldOfViewAngle;          // Field of view.
    public float distanceToSee;             // Maximum distance for which the sheep sees an obstacle (the dog, another sheep...).
    public float distanceToHear;            // Maximum distance for which the sheep hears the dog/'another sheep' steps.
    public float distanceToGetBarked;       // Maximum distance for which the sheep is affected by the dog's Bark.
    public float distanceToGetBitten;       // Maximum distance for which the sheep is affected by the dog's Bite.
    public float desiredSeparation;         // Minimum separation each sheep tries to maintain from the other sheeps.
    public float desiredSeparationFence;
    public float distanceToSeeDog;


    [Header("Movement")]
    private float baseSpeedMovement = 2f;    // Sheep's base movement speed.
    public float incrementSpeedBite;  // Increase in sheep's speed when bited.
    public float incrementSpeedBark;  // Increase in sheep's speed when barked.
    private float speedRotation = 1f;       // Sheep's rotation speed.
    private float timeBarkEffect = 3f;      // Bark's increase speed duration (seconds).
    private float timeBiteEffect = 10f;     // Bite's increase speed duration (seconds).


    [Header("Weights")]
    private float cohesion_weight;          // Weight of the cohesion vector when computing the movement vector.
    private float separation_weight;        // Weight of the separation vector when computing the movement vector.
    private float align_weight;             // Weight of the align vector when computing the movement vector.
    private float fence_weight;


    [Header("Points")]
    public Vector3 personalLastSighting;    // POINT: Last position where the dog was seen.
    

    [Header("Directions")]
    private Vector3 directionRun = Vector3.zero;        // DIRECTION: Direction in which the sheep has to move away from the dog 
    private Vector3 desired_direction = Vector3.zero;   // DIRECTION: Direction from the sheep position to the centroid
    private Vector3 randomDirection = Vector3.zero;


    [Header("Checkers")]
    public bool dogInSight;     // Is the dog inside sheep's vision?
    public bool dogHeard;       // Is the dog inside sheep's hearing range?
    private bool isRunning;     // Is the sheep running away from the dog?
    public bool isBeingBitten;  // Is the sheep in the duration time after a bite?
    public bool isBeingBarked;  // Is the sheep in the duration time after a bark?
    private bool notSeen;       // ????
    private bool isEating;
    private bool personalDogComing;          // Boolean, True if the dog is moving towards the sheep when it was sighted.

    private float personalRelativeSpeedDog;   // Float. How fast the dog is approaching the sheep
    private float lastTimeBitten;
    private float lastTimeBarked;
    private float turnY;
    private float currentSpeed;
    private int numSheeps;
    private int waitCounter = 0;
    private int eatCounter = 0;


    [Header("Debug variables")]
    public Vector3 positionSheepDog;
    public Vector3 startRay;
    public float angleDebug;
    private float aux;
    private float aux2;
    private Vector3 auxVec;

    // Variables used for animation
    private float realSpeed;
    private float max_speed;
    private Animator animator;
    private float animation_speed_increment;

    // Awake function is exectuted at the very begining to set all references.
    void Awake()
    {
        col = GetComponent<SphereCollider>();
        sheepdog = GameObject.FindGameObjectWithTag("Sheepdog");
        sheepList = GameObject.FindGameObjectsWithTag("Sheep");
        fenceList = GameObject.FindGameObjectsWithTag("Fence");
        rb = GetComponent<Rigidbody>();
        isRunning = false;
        isBeingBitten = false;
        isBeingBarked = false;
        isEating = false;

        fieldOfViewAngle = 110f;
        distanceToSee = 50f;
        distanceToSeeDog = 50f;
        distanceToHear = 35f;
        distanceToGetBarked = 40f;
        distanceToGetBitten = 10f;
        desiredSeparation = 10.0f;
        desiredSeparationFence = 30.0f;
        cohesion_weight = 1.0f;
        separation_weight = 1.0f;
        align_weight = 0.8f;
        fence_weight = 1.0f;
        incrementSpeedBite = 10f;
        incrementSpeedBark = 10f;
        eatCounter = 0;
        //speedMovement = 40f;
        //speedRotation = 4f;

        animator = GetComponent<Animator>();
        animation_speed_increment = 1.5f;
    }

    // Update is called once per frame.
    void Update()
    {
        max_speed = 0.7f * sheepdog.GetComponent<DogController_mode2>().speedMovementFast;
        realSpeed = animation_speed_increment * rb.velocity.magnitude / (max_speed);
        float incSpeed = Mathf.Min(Mathf.Max(animation_speed_increment * rb.velocity.magnitude / (max_speed), 0.3f), 2.0f);
        animator.SetFloat("Speed", realSpeed);
        animator.SetFloat("IncrementSpeed", incSpeed);
    }


    /// <summary>
    /// This function is called whenever the sheep colides to another object.
    /// </summary>
    /// <param name="other">Game Object to which the sheep has collided</param>
    private void OnTriggerStay(Collider other)
    {
        barkObject = GameObject.FindGameObjectWithTag("Bark"); // WHY THIS IS HERE??
        biteObject = GameObject.FindGameObjectWithTag("Bite"); // WHY THIS IS HERE??

        ////////////////////////////////
        // 1. Collider == Sheepdog?   //
        ////////////////////////////////
        if (other.gameObject == sheepdog)
        {
            // Set by default dogInSight and dogHeard to false. If sight or hear conditions are satisfied, this will be set to True. 
            dogInSight = false;
            dogHeard = false;
            personalDogComing = false;
            personalRelativeSpeedDog = 0;

            /////////////////////////////////////
            // 1.1. Can the sheep see the dog? //
            /////////////////////////////////////

            // Get the angle between the dog and the forward direction. 
            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            angleDebug = angle; // DEBUGGING
            Vector3 cross = Vector3.Cross(direction, transform.forward);
            if (cross.y < 0) angleDebug = -angleDebug;
            // Check if this angle is inside the fieldOfView range.
            if (angle < fieldOfViewAngle * 0.5f)
            {
                // Use raycast to know there are no obstacles between the sheepdog and the sheep. This will also be used to get the distance. 
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, distanceToSeeDog))
                {
                    if (hit.collider.gameObject == sheepdog)
                    {
                        // Dog has been seen so we set its boolean to true.
                        dogInSight = true;
                        getApproachingVelocity();
                        personalLastSighting = sheepdog.transform.position;
                    }
                }
            }


            //////////////////////////////////////
            // 1.2. Can the sheep hear the dog? //
            //////////////////////////////////////

            if (Vector3.Distance(transform.position, sheepdog.transform.position) < distanceToHear)
            {
                // Dog has been heard so we set its boolean to true.
                dogHeard = true;
                getApproachingVelocity();
                personalLastSighting = sheepdog.transform.position;
            }
        }

        ////////////////////////////////
        // 2. Collider == Bark?       //
        ////////////////////////////////
        if (other.gameObject == barkObject)
        {
            if (Vector3.Distance(transform.position, barkObject.transform.position) < distanceToGetBarked)
            {
                isBeingBarked = true;
                lastTimeBarked = Time.time;
                dogHeard = true;
                dogInSight = true;
                personalLastSighting = barkObject.transform.position;
            }
        }


        ////////////////////////////////
        // 3. Collider == Bite?       //
        ////////////////////////////////
        if (other.gameObject == biteObject)
        {
            if (Vector3.Distance(transform.position, biteObject.transform.position) < distanceToGetBitten)
            {
                isBeingBitten = true;
                lastTimeBitten = Time.time;
                dogHeard = true;
                dogInSight = true;
                personalLastSighting = biteObject.transform.position;
            }
        }
    }


    /// <summary>
    /// This function computes the velocity of the dog with respect to the sheep and updates the parameters controlling if the dog is approching and how fast.
    /// </summary>
    /// <returns></returns>
    private void getApproachingVelocity()
    {
        Vector3 vectorDogSheep = transform.position - sheepdog.transform.position;
        personalRelativeSpeedDog = Vector3.Dot(sheepdog.GetComponent<Rigidbody>().velocity.normalized, vectorDogSheep.normalized);
        personalDogComing = personalRelativeSpeedDog > 0;
    }
    /// <summary>
    /// This function returns a direction vector which tries to maintain the cohesion of the sheeps group
    /// </summary>
    /// <returns></returns>
    private Vector3 Cohesion()
    {
        Vector3 centroid = Vector3.zero;
        Vector3 centroidDirection = Vector3.zero;
        numSheeps = 0;

        for (int i = 0; i < sheepList.Length; i++)
        {
            GameObject currentSheep = sheepList[i];
            notSeen = true;

            // Sheeps in view range 
            Vector3 direction = currentSheep.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            angleDebug = angle;
            Vector3 cross = Vector3.Cross(direction, transform.forward);
            if (cross.y < 0) angleDebug = -angleDebug;
            // Check if the angle is less than the fieldOfView angle
            if (angle < 280.0f & currentSheep.transform.position != transform.position)
            {
                // Use raycast to know there are no obstacles between the sheeps and the sheep. This will also be used to get the distance. 
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, distanceToSee))
                {
                    if (hit.collider.gameObject.tag == "Fence")
                    {
                        continue;
                    }
                    else
                    {
                        centroid += currentSheep.transform.position - transform.position;
                        numSheeps += 1;
                        notSeen = false;
                    }
                }
            }
            /*
            // Sheeps in hearing range.

            if (Vector3.Distance(transform.position, currentSheep.transform.position) < distanceToHear && notSeen)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, distanceToHear))
                {
                    // The sheeps won't take see or hear sheeps accross the fence.
                    if (hit.collider.gameObject.tag != "Fence")
                    {
                        centroid += currentSheep.transform.position;
                        numSheeps += 1;
                        notSeen = false;
                    }
                }
            }
            */
            //Normalize The Separation direction.
            
            if (numSheeps > 0)
            {
                //centroid = centroid / (numSheeps * 1.0f);
                centroidDirection = centroid; //;
                centroidDirection = Vector3.Normalize(centroidDirection);
            }
        }
        return centroidDirection;
    }

    /// <summary>
    /// This function returns a direction vector which tries to maintain the distance between the sheeps
    /// </summary>
    /// <returns></returns>
    private Vector3 Separation()
    {
        Vector3 separation_val = Vector3.zero;
        numSheeps = 0;

        for (int i = 0; i < sheepList.Length; i++)
        {
            GameObject currentSheep = sheepList[i];
            notSeen = true;
            // Sheeps in view range 
            Vector3 direction = currentSheep.transform.position - transform.position;
            float distance = Vector3.Distance(currentSheep.transform.position, transform.position);
            float angle = Vector3.Angle(direction, transform.forward);
            angleDebug = angle;
            Vector3 cross = Vector3.Cross(direction, transform.forward);
            if (cross.y < 0) angleDebug = -angleDebug;

            // Check if the angle is less than the fieldOfView angle and wether the sheep is too close
            if (angle < 280.0f & distance < desiredSeparation & currentSheep.transform.position != transform.position)
            {
                // Use raycast to know there are no obstacles between the sheepdog and the sheep. This will also be used to get the distance. 
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, distanceToSee))
                {
                    if (hit.collider.gameObject.tag == "Fence")
                    {
                        continue;
                    }
                    else
                    {
                        Vector3 diff = Vector3.Normalize(-direction);
                        //The closer the sheep, the more we have to get away from it.
                        diff = diff / distance;
                        separation_val += diff;
                        numSheeps += 1;
                    }
                }
            }
        }
        //Normalize The Separation direction.
        if (numSheeps > 0)
        {
            //separation_val = separation_val / (numSheeps * 1.0f);
            separation_val = Vector3.Normalize(separation_val);
        }
        return separation_val;
    }

    private Vector3 Separation_fence()
    {
        Vector3 separation_val = Vector3.zero;
        int numFence = 0;

        for (int i = 0; i < fenceList.Length; i++)
        {
            GameObject currentFence = fenceList[i];
            notSeen = true;
            // Fence in view range 
            Vector3 direction = currentFence.transform.position - transform.position;
            float distance = Vector3.Distance(currentFence.transform.position, transform.position);
            float angle = Vector3.Angle(direction, transform.forward);
            angleDebug = angle;
            Vector3 cross = Vector3.Cross(direction, transform.forward);
            if (cross.y < 0) angleDebug = -angleDebug;

            // Check if the angle is less than the fieldOfView angle and wether the fence is too close
            if (angle < 180.0f & distance < desiredSeparationFence & currentFence.transform.position != transform.position)
            {
                Vector3 diff = Vector3.Normalize(-direction);
                //The closer the sheep, the more we have to get away from it.
                diff = diff / distance;
                separation_val += diff;
                numFence += 1;
                    
            }
        }
        //Normalize The Separation direction.
        if (numFence > 0)
        {
            //separation_val = separation_val / (numSheeps * 1.0f);
            separation_val = Vector3.Normalize(separation_val);
        }
        return separation_val;
    }

    /// <summary>
    /// This function returns a direction vector which tries to maintain the sheep direction to the same direction the group is moving to.
    /// </summary>
    /// <returns></returns>
    private Vector3 Align()
    {
        Vector3 align_velocity = Vector3.zero;
        numSheeps = 0;
        for (int i = 0; i < sheepList.Length; i++)
        {
            GameObject currentSheep = sheepList[i];
            notSeen = true;
            // Sheeps in view range 
            Vector3 direction = currentSheep.transform.position - transform.position;
            float distance = Vector3.Distance(currentSheep.transform.position, transform.position);
            float angle = Vector3.Angle(direction, transform.forward);
            angleDebug = angle;
            Vector3 cross = Vector3.Cross(direction, transform.forward);
            if (cross.y < 0) angleDebug = -angleDebug;

            // Check if the angle is less than the fieldOfView angle and wether the sheep is too close
            if (angle < 180.0f & currentSheep.transform.position != transform.position)
            {
                // Use raycast to know there are no obstacles between the sheepdog and the sheep. This will also be used to get the distance. 
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, distanceToSee))
                {
                    if (hit.collider.gameObject.tag == "Fence")
                    {
                        continue;
                    }
                    else
                    {
                        align_velocity += currentSheep.GetComponent<Rigidbody>().velocity;
                        numSheeps += 1;
                    }
                }
            }
        }
        //Normalize The Separation direction.
        if (numSheeps > 0)
        {
            align_velocity = align_velocity / (numSheeps * 1.0f);
            align_velocity = Vector3.Normalize(align_velocity);
        }
        return align_velocity;
    }

    /// <summary>
    /// Executed every timeDelta.
    /// </summary>
    void FixedUpdate()
    {
        Debug.Log("UPDATE " + name);
        // Implement running mechanism. We should run in the oposite direction of the dog. 
        float moveZ = 0;
        float moveX = 0;
        // Perform flocking using Reynolds's boids algorithm
        Vector3 cohesion_direction = Cohesion();        // Term that forces the sheep to go to the centroid.
        Vector3 separation_direction = Separation();    // Tries to separate from the other sheeps.
        Vector3 align_direction = Align();              // Tries to have the same velocity vector than its neighbours.
        Vector3 fence_direction = Separation_fence();
        Vector3 dog_direction = Vector3.zero;
        cohesion_weight = 1.0f;
        separation_weight = 1.0f;
        align_weight = 2.0f;
        fence_weight = 1.0f;

        if (!dogInSight & !dogHeard)
        {
            align_weight = 0.5f;
            if (waitCounter == 0)
            {
                randomDirection = Random.Range(0.5f, 2.0f) * Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));

                waitCounter = Random.Range(100, 500);
            }
            // desired_direction = cohesion_weight * cohesion_direction + separation_weight *
            //    separation_direction + align_weight * align_direction + fence_weight * fence_direction;
            desired_direction = cohesion_weight * cohesion_direction + separation_weight *
                separation_direction + fence_weight * fence_direction + align_weight * align_direction;
            desired_direction += randomDirection;
            waitCounter -= 1;

            // Sheep stops to eat grass.

            if (!isEating)
            {
                float randomStop = Random.Range(0f, 1f);
                if (randomStop < 0.0002f)
                {
                    isEating = true;
                    eatCounter = Random.Range(0, 200);
                }
            }
            if (eatCounter > 0)
            {
                desired_direction = Vector3.zero;
                eatCounter += -1;
                if (eatCounter == 0)
                {
                    isEating = false;
                }
            }
        }

        
        if (dogInSight || dogHeard)
        {
            //Compute direction in which to run 
            cohesion_weight = cohesion_weight * 3.0f;
            separation_weight = 1.5f * separation_weight;
            align_weight = align_weight * 3.0f;
            fence_weight = fence_weight * 1.5f;

            dog_direction = Mathf.Pow(distanceToSee, 2) / Mathf.Pow(Vector3.Distance(transform.position, personalLastSighting), 2) * Vector3.Normalize(transform.position - personalLastSighting);
            isRunning = true;
            isEating = false;
        }

        if (!dogInSight && !dogHeard)
        {
            isRunning = false;
        }

        if (isBeingBarked && Time.time > lastTimeBarked + timeBarkEffect)
        {
            isBeingBarked = false;
        }

        if (isBeingBitten && Time.time > lastTimeBitten + timeBiteEffect)
        {
            isBeingBitten = false;
        }

        //////////////////////////////////////
        // The Dog has been seen            //
        //////////////////////////////////////
        if (isRunning)  // The dog has been seen
        {
            float weightDogComming = 1.0f;
            // Compute weight dog comming
            if (personalDogComing)
            {
                weightDogComming = 0.6f + (0.4f * personalRelativeSpeedDog);
            }
            else
            {
                weightDogComming = 0.6f;
            }
            //Make velocity inversely proportional to distance to sheepdog
            currentSpeed = Mathf.Min(sheepdog.GetComponent<DogController_mode2>().speedMovementFast * 0.6f,
                2 * weightDogComming * baseSpeedMovement * Mathf.Pow(distanceToSee, 3) / Mathf.Pow(Vector3.Distance(transform.position, personalLastSighting), 3));
            aux = currentSpeed;
            if (isBeingBitten)
            {
                currentSpeed = currentSpeed + incrementSpeedBite;
                cohesion_weight = 0.1f * cohesion_weight;
                separation_weight = 0.1f * separation_weight;
                align_weight = 0.1f * align_weight;
                fence_weight = 0.1f * fence_weight;
                dog_direction = 5.0f * dog_direction;

            }
            else if (isBeingBarked)
            {
                currentSpeed = currentSpeed - 0.45f * incrementSpeedBark;
                cohesion_weight = 1.25f * cohesion_weight;
                separation_weight = 0.1f * separation_weight;
                fence_weight = 0.65f * fence_weight;
                align_weight = 0.7f * align_weight;
                dog_direction = 1.5f * dog_direction;
            }
            desired_direction = cohesion_weight * cohesion_direction + separation_weight *
               separation_direction + align_weight * align_direction + fence_weight * fence_direction + dog_direction;
            float angle = Vector3.Angle(desired_direction, transform.forward);
            moveX = angle / 180;

            //Get direction of rotation
            Vector3 cross = Vector3.Cross(desired_direction, transform.forward);
            if (cross.y > 0) moveX = -moveX;

            if (Vector3.Distance(transform.position, personalLastSighting) < distanceToSee * 2)
            {
                moveZ = 1 - angle / 180;
            }
            else
            {
                moveZ = 0;
            }

            Vector3 inputMove = new Vector3(moveX * currentSpeed, 0.0f, moveZ * currentSpeed);
            Vector3 movement = transform.forward * inputMove.z;
            movement.y = rb.velocity.y; // Do not modify gravity 

            rb.velocity = movement; // Add inputMove.z to Rigidbodys forward facing velocity
            auxVec = movement;
            turnY += inputMove.x; // Rotate the player based on InputMove.x
            rb.rotation = Quaternion.Euler(0.0f, turnY, 0.0f); // Apply the rotation
        }
        //aux2 += 1f;
        //////////////////////////////////////
        // The dog is unseen                //
        //////////////////////////////////////
        if (!isRunning && numSheeps > 0) // The dog is unseen
        {
            //aux += 1f;
            float angle = Vector3.Angle(desired_direction, transform.forward);
            moveX = angle / 180;

            //Get direction of rotation
            Vector3 cross = Vector3.Cross(desired_direction, transform.forward);
            if (cross.y > 0) moveX = -moveX;

            if (Vector3.Distance(transform.position, transform.position + desired_direction) < distanceToSee * 2)
            {
                moveZ = 1 - angle / 180;
            }
            else
            {
                moveZ = 0;
            }
            currentSpeed = Mathf.Min(2 * baseSpeedMovement, baseSpeedMovement * Vector3.SqrMagnitude(desired_direction));
            aux = currentSpeed;
            Vector3 inputMove = new Vector3(moveX * speedRotation, 0.0f, moveZ * currentSpeed);
            Vector3 movement = transform.forward * inputMove.z;
            movement.y = rb.velocity.y; // Do not modify gravity 

            rb.velocity = movement; // Add inputMove.z to Rigidbodys forward facing velocity
            auxVec = movement;
            turnY += inputMove.x; // Rotate the player based on InputMove.x
            rb.rotation = Quaternion.Euler(0.0f, turnY, 0.0f); // Apply the rotation

        }
        Debug.Log("UPDATE-END " + name);
    }
}