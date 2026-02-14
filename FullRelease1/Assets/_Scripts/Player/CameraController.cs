using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    public Transform playerTransform;
    public Vector3 pivotOffset = Vector3.up;

    [Space(15)]
    public float mouseSens;
    public float pitch = 0;
    public float yaw = 0;

    public LayerMask collisionMask;
    public float maxCameraDistance = 8f;
    public float minCameraDistance = 2f;
    public float sphereRadius = 0.3f;
    private float currentDistance;

    void Start()
    {
        cam = transform.GetChild(0).GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        if (!cam || cam.gameObject.activeSelf == false) return;

        yaw += InputManager.Look.x * mouseSens * Time.deltaTime;

        if (Mathf.Abs(yaw) > 360) yaw += yaw > 0 ? -360 : 360; // Just added this to keep the number small

        //yaw = Mathf.Clamp(yaw, -80f, 80f);
        pitch -= InputManager.Look.y * mouseSens * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -20f, 80f);

        transform.localEulerAngles = new Vector3(pitch, yaw, 0f); //Cam Rotate

        Vector3 targetPivotPos = playerTransform.position + pivotOffset;
        transform.position = Vector3.Lerp(transform.position, targetPivotPos, Time.deltaTime * 10f); //CamPivot Pos Match

        Vector3 desiredCameraPos = transform.position - transform.forward * maxCameraDistance;

        RaycastHit hit;
        float targetDistance = maxCameraDistance;

        if (Physics.SphereCast(transform.position, sphereRadius, -transform.forward, out hit, maxCameraDistance, collisionMask))
        {
            targetDistance = hit.distance - sphereRadius;
        }

        targetDistance = Mathf.Clamp(targetDistance, minCameraDistance, maxCameraDistance);

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 100f);

        cam.transform.localPosition = new Vector3(0f, 0f, -currentDistance);
    }


}
