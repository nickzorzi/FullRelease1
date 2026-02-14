using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    CharacterController characterController;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8;
    [SerializeField] private float rotationSpeed;
    private float gravity = -9.8f;
    private float groundedGravity = -2f;
    private bool isJumping;
    private bool isAltGrounded;
    private float initialJumpVelocity;
    private float maxJumpHeight = 2f;
    private float maxJumpTime = 0.75f;

    private Vector3 velocity;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;
    Camera cam;

    [Header("Jump Logic")]
    [SerializeField] private int maxJumpCount = 2;
    private float timeSinceUngrounded;
    private float timeSinceJumpRequest;
    private bool ungroundedDueToJump;
    private int jumpCount = 0;
    private float coyoteTime = 0.2f;
    private bool isFalling;
    private float fallMultiplier = 2.0f;

    [Header("B.O.I.N.G Bot Logic")]
    [SerializeField] private GameObject boingBot;
    [SerializeField] private Rigidbody boingRB;
    [SerializeField] private Transform backpack;
    [SerializeField] private Transform hand;
    [Space(15)]
    [SerializeField] TrajectoryLine trajLine;
    [Space(15)]
    [SerializeField] private float throwStrength;
    [SerializeField] private Vector3 throwDir;
    [SerializeField] private bool inBackpack;
    [SerializeField] private bool inHand;
    public bool isDeployed;


    [SerializeField] float slideFriction = 3f;
    bool isOnSlope;
    Vector3 hitNormal; // slopes


    // PLATFORM SHINANIGANS

    Transform currentPlatform;
    Vector3 lastPlatformPosition;
    Vector3 platformVelocity;
    bool groundedOnPlatform;
    PistonPlatform currentPushPlatform;

    [HideInInspector] public bool logicPaused = false;

    private void Awake()
    {
        instance = this;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = false;

        characterController = GetComponent<CharacterController>();
        cam = cameraController.GetComponentInChildren<Camera>();
        setupJumpVariables();

        //B.O.I.N.G Setup
        if(!boingRB) boingRB = boingBot.GetComponent<Rigidbody>();
        PutBoingInBackPack();

        // Trajectory Link
        trajLine.projectile = boingRB;
        trajLine.launchPoint = boingBot.transform;
        trajLine.launchForce = throwStrength;
        trajLine.throwDir = throwDir;

        trajLine.gameObject.SetActive(false);

    }

    void Update()
    {
        if (logicPaused)
        {
            if (characterController.enabled == true) characterController.enabled = false;
            if (rb.isKinematic == true) rb.isKinematic = false;
            //if (rb.useGravity == false) rb.useGravity = true;
            if (capsuleCollider.enabled == false) capsuleCollider.enabled = true;
            
            return;
        }
        else
        {
            if (characterController.enabled == false) characterController.enabled = true;
            if (rb.isKinematic == false) rb.isKinematic = true;
            //if (rb.useGravity == true) rb.useGravity = false;
            if (capsuleCollider.enabled == true) capsuleCollider.enabled = false;
        }

        BOINGLogic();
        PlayerMovement();

    }


    public IEnumerator TakeControl(Camera otherCam, bool resetRobot = false)
    {
        cam.gameObject.SetActive(true);
        otherCam.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f); // Transition Buffer;

        if (resetRobot) PutBoingInBackPack();

        logicPaused = false;
    }

    public void SetPlayerLogicState(bool state)
    {
        cam.gameObject.SetActive(state);
        logicPaused = !state;
    }

    IEnumerator SwitchPOV()
    {
        yield return new WaitForSeconds(1f); // Buffer so WaitUntil Doesnt insta Trigger

        bool cancelled = false;

        while (true)
        {
            if(boingBot.transform.position.y < - 20) // Falls off The world
            {
                PutBoingInBackPack();
                cancelled = true;
                break;
            }

            //Debug.Log(boingRB.angularVelocity);
            if (Mathf.Approximately(boingRB.linearVelocity.sqrMagnitude, Vector3.zero.sqrMagnitude)) // So if the robot is not falling for 1 sec then change XD so fucking Jank
            {
                yield return new WaitForSeconds(.5f);

                if (Mathf.Approximately(boingRB.linearVelocity.sqrMagnitude, Vector3.zero.sqrMagnitude)) break;
            }

            yield return null;
        }

        if (!cancelled)
        {
            logicPaused = true;

            StartCoroutine(boingBot.GetComponent<RobotController>().TakeControl(cam));
            isDeployed = true;
        }
    }

    /*
        BOING robot Throwing and equiping Code -> BOING Robot Functions should be on it's own script 
        and once the object has landed, switch to that camera on the bot and control that bot? 

        We Have a button to switch between the 2 and have like a crt transition screen when flipping cameras?
    */

    public void PutBoingInBackPack()
    {
        boingRB.isKinematic = true;
        boingRB.useGravity = false;
        boingBot.transform.SetParent(backpack, false);
        boingBot.transform.position = backpack.position;
        boingBot.transform.localEulerAngles = new Vector3(-90,0,0);
        inBackpack = true;
        isDeployed = false;
    }

    //B.O.I.N.G Logic
    void BOINGLogic()
    {

        if (InputManager.FocusPressed && inHand)
        {
            throwDir = cam.transform.forward + Vector3.up * Mathf.Clamp((-cameraController.pitch / 10), 0,2);
            trajLine.throwDir = throwDir;
        }
        else if (inHand) // Trajectory Direction Fix
        {
            throwDir = transform.forward + Vector3.up * 2f;
            trajLine.throwDir = throwDir;
        }

        if (InputManager.EquipPressed)
        {
            if (inBackpack)
            {
                inBackpack = false;

                trajLine.gameObject.SetActive(true);

                boingBot.transform.SetParent(hand);
                boingBot.transform.position = hand.position;
                boingBot.transform.Rotate(90, 0, 0);

                inHand = true;
            }
            else if (inHand)
            {
                inHand = false;

                trajLine.gameObject.SetActive(false);

                boingBot.transform.SetParent(backpack);
                boingBot.transform.position = backpack.position;             // This Stuff Should be Replaced with an animation most likely? Idk 
                boingBot.transform.Rotate(-90, 0, 0);

                inBackpack = true;
            }
        }

        if (InputManager.ThrowPressed && inHand)
        {
            
            inHand = false;

            trajLine.gameObject.SetActive(false);

            boingBot.transform.SetParent(null);

            boingRB.isKinematic = false;
            boingRB.useGravity = true;

            boingRB.angularVelocity = Vector3.zero;
            boingRB.linearDamping = 0f;
            boingRB.angularDamping = 0f;


            boingRB.AddForce(throwDir.normalized * throwStrength, ForceMode.Impulse);

            StartCoroutine(SwitchPOV());

        }
        else if(InputManager.ThrowPressed && isDeployed)
        {
            logicPaused = true;
            StartCoroutine(boingBot.GetComponent<RobotController>().TakeControl(cam));
        }
    }

    void PlayerMovement()
    {
        //All Movement Logic
        //Debug.Log(isOnSlope + " || " + velocity.y);


        Gravity();
        Vector3 inputDirection = new Vector3(InputManager.Move.x, 0f, InputManager.Move.y);
        if (velocity.y < -5 && isOnSlope) inputDirection = -inputDirection / 3;
        
        
        Vector3 moveDirection = Vector3.zero;

        if (inputDirection != Vector3.zero)
        {
            Vector3 camForward = cameraController.transform.forward;
            Vector3 camRight = cameraController.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = camForward * inputDirection.z + camRight * inputDirection.x;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (isOnSlope)
        {
            moveDirection.x += (1f - hitNormal.y) * hitNormal.x * (1 - slideFriction);
            moveDirection.y += (1f - hitNormal.y) * hitNormal.z * (1 - slideFriction);
        }

        Vector3 horizontalMove = moveDirection * moveSpeed;

        if (InputManager.JumpPressed)
        {
            var canCoyoteJump = timeSinceUngrounded < coyoteTime && !isJumping;
            var canDoubleJump = jumpCount < maxJumpCount;

            if (characterController.isGrounded && isAltGrounded || canDoubleJump || canCoyoteJump)
            {
                isJumping = true;
                velocity.y = initialJumpVelocity;

                jumpCount++;
            }
        }


        if (currentPlatform != null)
        {
            platformVelocity =
                (currentPlatform.position - lastPlatformPosition) / Time.deltaTime;

            lastPlatformPosition = currentPlatform.position;
        }
        else
        {
            platformVelocity = Vector3.zero;
        }
        if (groundedOnPlatform)
        {
            // Small downward snap keeps controller glued
            platformVelocity.y -= 0.06f;
        }

        Vector3 finalMove =
            (new Vector3(horizontalMove.x, velocity.y, horizontalMove.z)
            + platformVelocity) * Time.deltaTime;




        //Debug.Log(finalMove);


        characterController.Move(finalMove);


        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= characterController.slopeLimit && Vector3.Angle(Vector3.up, hitNormal) < 87;
    }

    void Gravity()
    {
        FloorCheck();

        isFalling = !characterController.isGrounded && !isAltGrounded && (velocity.y <= 0.0f || !InputManager.JumpHeld);

        bool platformMovingUp =
            currentPushPlatform != null &&
            !currentPushPlatform.stopped &&
            platformVelocity.y > 0.01f;

        if (groundedOnPlatform && platformMovingUp && !isJumping)
        {
            velocity.y = 0f;
            return;
        }
        if (characterController.isGrounded && isAltGrounded && velocity.y < 0)
        {
            velocity.y = groundedGravity;

            isJumping = false;
            jumpCount = 0;
            timeSinceUngrounded = 0;
            moveSpeed = 8;
        }
        else if (isFalling)
        {
            float previousYVelocity = velocity.y;
            velocity.y = velocity.y + (gravity * fallMultiplier * Time.deltaTime);
            velocity.y = Mathf.Max((previousYVelocity + velocity.y) * 0.5f, -20.0f);
        }
        else
        {
            timeSinceUngrounded += Time.deltaTime;

            float previousYVelocity = velocity.y;
            velocity.y = velocity.y + (gravity * Time.deltaTime);
            velocity.y = (previousYVelocity + velocity.y) * 0.5f; //Velocity Verlet Integration
        }
    }


    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("LaserTower"))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

            Vector3 dir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            rb.AddForceAtPosition(dir, hit.point, ForceMode.Impulse);

            LaserTower towerScript = rb.transform.parent.GetComponent<LaserTower>();

            if (towerScript != null)
            {
                if (!towerScript.isMoved) towerScript.isMoved = true;
            }
        }

        hitNormal = hit.normal;

    }

    void FloorCheck()
    {
        RaycastHit hit;
        LayerMask layerIgnore = ~LayerMask.GetMask("Player");

        groundedOnPlatform = false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, characterController.height * 0.5f + 0.3f, layerIgnore))
        {
            isAltGrounded = hit.normal.y > 0.6f;

            if (hit.collider.CompareTag("Platform"))
            {
                groundedOnPlatform = true;

                Transform platform = hit.collider.transform;

                if (currentPlatform != platform)
                {
                    currentPlatform = platform;
                    lastPlatformPosition = platform.position;
                    currentPushPlatform = platform.GetComponent<PistonPlatform>() ?? platform.parent.GetComponent<PistonPlatform>();
                }
            }
            else
            {
                groundedOnPlatform = false;
                currentPlatform = null;
                currentPushPlatform = null;
            }
        }
        else
        {
            isAltGrounded = false;
            groundedOnPlatform = false;
            currentPlatform = null;
            currentPushPlatform = null;
        }
    }

}
