using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{

    [SerializeField] Transform targetObj;

    
    [Header("Line Settings")]
    public int linePoints = 40;
    public float timeBetweenPoints = 0.1f;
    [SerializeField] LayerMask targetable;


    [HideInInspector] public Rigidbody projectile;
    [HideInInspector] public Transform launchPoint;
    [HideInInspector] public float launchForce = 10f;
    [HideInInspector] public Vector3 throwDir;


    LineRenderer line;


    void Awake()
    {
        line = GetComponent<LineRenderer>();
        targetObj.gameObject.SetActive(false);

    }

    void Update()
    {
        DrawTrajectory();
    }

    private void OnDisable()
    {
        if(targetObj) targetObj.gameObject.SetActive(false);
    }

    void DrawTrajectory()
    {
        line.positionCount = linePoints;
        targetObj.gameObject.SetActive(false);

        Vector3 startPosition = launchPoint.position;
        Vector3 startVelocity = throwDir.normalized * launchForce;

        Vector3 prevPoint = startPosition;

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;

            Vector3 point = startPosition +
                startVelocity * time + .5f * Physics.gravity * time * time;


            if (Physics.Raycast(prevPoint, point - prevPoint, out RaycastHit hit, 1, targetable, QueryTriggerInteraction.Ignore))
            {
                line.positionCount = i + 1;
                line.SetPosition(i, hit.point);
                targetObj.position = hit.point;
                targetObj.rotation = Quaternion.LookRotation(hit.normal);

                targetObj.gameObject.SetActive(true);
                break;
            }


            line.SetPosition(i, point);

            prevPoint = point;
        }
    }
}
