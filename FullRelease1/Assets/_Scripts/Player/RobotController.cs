using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{

    public static RobotController instance;

    CharacterController characterController;

    Transform ParentContainer;

    [Header("Movement")]
    [SerializeField] float grabForBackpackArea = 4f;
    [SerializeField] private float moveSpeed = 8;
    [SerializeField] private float rotationSpeed;

    [Tooltip("The Amount of time taken to fix rotation on robot")]
    [SerializeField] float upRightTime = .5f;

    private float gravity = -9.8f;
    private float groundedGravity = -2f;
    Rigidbody rb;
    [SerializeField] private BoxCollider boingCollider;
    

    private Vector3 velocity;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;
    Camera cam;

    //[Header("Jump Logic")]
    private bool isFalling;
    private float fallMultiplier = 2.0f;

    [HideInInspector] public bool logicPaused = true;

    Collider grabTrigger;


    [Header("Grab Basics")]
    [SerializeField] bool isPull;
    private Vector3 grabPoint;
    [SerializeField] Transform player;

    // PLATFORM SHINANIGANS

    Transform currentPlatform;
    Vector3 lastPlatformPosition;
    Vector3 platformVelocity;
    bool groundedOnPlatform;
    PistonPlatform currentPushPlatform;

    // SLOPE SHINANIGANS

    [SerializeField] float slideFriction = 3f;
    bool isOnSlope;
    Vector3 hitNormal; // slopes


    private void Awake()
    {
        instance = this;

        ParentContainer = transform.parent; 

        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        grabTrigger = GetComponent<Collider>();

        cam = cameraController.GetComponentInChildren<Camera>();


        cam.gameObject.SetActive(false);

        characterController.enabled = false;
        grabTrigger.enabled = false;
        logicPaused = true;
    }




    private void Update()
    {
        pickUpBOING();

        if (logicPaused)
        {
            if (PlayerController.instance.isDeployed)
            {
                if (rb.isKinematic == true) rb.isKinematic = false;
                if (characterController.enabled == true) characterController.enabled = false;
            }
            
            return; // Dont ask, just accept the ActivelyControlled
        }
        else
        {
            if (PlayerController.instance.isDeployed)
            {
                if (rb.isKinematic == false) rb.isKinematic = true;
                if (characterController.enabled == false) characterController.enabled = true;
            }
        }

        if (characterController.isGrounded)
        {
            rb.isKinematic = true;
        }

        if (InputManager.ThrowPressed) // Switch Control To Player
        {
            logicPaused = true;
            characterController.enabled = false;
            grabTrigger.enabled = true;
            StartCoroutine(PlayerController.instance.TakeControl(cam));
            return;
        }

        grabPoint = transform.position + new Vector3(0, 1.5f, 0);

        if (InputManager.EquipPressed)
        {
            GrabPlayer();
        }

        if (isPull)
        {
            player.gameObject.transform.position = Vector3.MoveTowards(player.position, grabPoint - new Vector3(0, 1, 0), 15 * Time.deltaTime);

            if (player.position == grabPoint - new Vector3(0, 1, 0)) isPull = false;
        }

        PlayerMovement();
    }

    void outOfBoundsCheck() // --------------------------------------------- Check For Robot falling out of the world
    {
        if(transform.position.y < -20)
        {
            StopAllCoroutines();
            logicPaused = true;
            characterController.enabled = false;
            grabTrigger.enabled = true;
            StartCoroutine(PlayerController.instance.TakeControl(cam, true));
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(grabPoint, 0.1f);
        Gizmos.DrawLine(grabPoint, player.position + new Vector3(0, 1, 0));
    }

    public void SetPlayerLogicState(bool state)
    {
        cam.gameObject.SetActive(state);
        logicPaused = !state;
    }

    public IEnumerator TakeControl(Camera otherCam)
    {
        // it started as camera but the audio listener warning was annoying
        cam.gameObject.SetActive(true);
        otherCam.gameObject.SetActive(false);

        transform.SetParent(ParentContainer);
        transform.SetAsFirstSibling();

        grabTrigger.enabled = false;

        yield return new WaitForSeconds(.5f); // Transition Buffer;

        //Debug.Log(transform.localEulerAngles.x + " | " + transform.localEulerAngles.z);

        if(Mathf.RoundToInt(transform.rotation.x) != 0 || Mathf.RoundToInt(transform.rotation.z) != 0)
        {
            LeanTween.move(gameObject, transform.position + Vector3.up, .5f)
                .setOnComplete(() =>
                {
                    LeanTween.rotateLocal(
                        gameObject,
                        new Vector3(0f, transform.localEulerAngles.y, 0f),
                        upRightTime
                    ).setEaseOutSine();
                });
            yield return new WaitForSeconds(upRightTime + 1);
        }



        logicPaused = false;
        characterController.enabled = true;
    }


    /*
        // I Removed The Jump Logic for the robot, unless we want that idk;
    */
    void PlayerMovement()
    {
        //All Movement Logic
        Gravity();
        outOfBoundsCheck();

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
            platformVelocity.y -= 0.05f;
        }

        Vector3 finalMove =
            (new Vector3(horizontalMove.x, velocity.y, horizontalMove.z)
            + platformVelocity) * Time.deltaTime;


        characterController.Move(finalMove);


        isOnSlope = !(Vector3.Angle(Vector3.up, hitNormal) <= characterController.slopeLimit);
    }

    void Gravity()
    {
        FloorCheck();

        isFalling = !characterController.isGrounded && (velocity.y <= 0.0f || !InputManager.JumpHeld);

        bool platformMovingUp =
            currentPushPlatform != null &&
            !currentPushPlatform.stopped &&
            platformVelocity.y > 0.01f;

        if (groundedOnPlatform && platformMovingUp)
        {
            velocity.y = 0f;
            return;
        }
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = groundedGravity;


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

            float previousYVelocity = velocity.y;
            velocity.y = velocity.y + (gravity * Time.deltaTime);
            velocity.y = (previousYVelocity + velocity.y) * 0.5f; //Velocity Verlet Integration
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    void GrabPlayer()
    {
        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("Player", "Obstacle");

        Vector3 direction = (player.position + new Vector3(0, 1, 0)) - grabPoint;

        if (Physics.SphereCast(grabPoint, 0.5f, direction, out hit, direction.magnitude, layerMask))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                return;
            }

            if (hit.collider.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player Hit");

                isPull = true;
            }
        }
        else
        {
            Debug.Log("miss");
        }
    }

    void FloorCheck()
    {
        RaycastHit hit;
        LayerMask layerIgnore = ~LayerMask.GetMask("Player");

        groundedOnPlatform = false;

        if (Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            out hit,
            characterController.height * 0.5f + 0.3f,
            layerIgnore))
        {

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

            groundedOnPlatform = false;
            currentPlatform = null;
            currentPushPlatform = null;
        }
    }

    void pickUpBOING()
    {
        if (Vector3.Distance(transform.position, PlayerController.instance.transform.position) > grabForBackpackArea || !PlayerController.instance.isDeployed) return; // Not in the area

        if (!logicPaused) return;
        //Debug.Log("In Area for Pick Up");

        Vector3 start = transform.position + Vector3.up;
        Vector3 target = (PlayerController.instance.transform.position + Vector3.up - start).normalized;

        if (InputManager.InteractPressed && Physics.Raycast(start, target, out RaycastHit hit))
        {
            if (!hit.collider.CompareTag("Player")) return;
             
            // This should be Replaced with an Animation or the robot moving towards the backpack and then triggering this function (SCOPE)
            PlayerController.instance.PutBoingInBackPack();
            grabTrigger.enabled = false;

        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, grabForBackpackArea);
    }

}
