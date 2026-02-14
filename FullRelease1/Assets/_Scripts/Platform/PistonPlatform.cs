using UnityEngine;

public class PistonPlatform : MonoBehaviour
{
    [SerializeField] Transform platform;
    
    [Space(15)]
    [SerializeField] float moveHeight = 10f;
    [SerializeField] float speed = 3f;

    [Space(15)]
    [SerializeField] bool currentState = false;

    Rigidbody platformRb;
    Vector3 targetPos;
    Vector3 savedPosition; // fix the problem if the platform is the root transform


    [HideInInspector] public bool stopped;

    private void Awake()
    {
        if (!platform) return;

        if (currentState) platform.transform.position = transform.position + Vector3.up * moveHeight;
        else platform.transform.position = transform.position;


        platformRb = platform.GetComponent<Rigidbody>();
        savedPosition = transform.position;
        targetPos = transform.position;
    }


    private void OnValidate()
    {
        if(!platform) return;

        if (currentState) platform.transform.position = transform.position + Vector3.up * moveHeight;
        else platform.transform.position = transform.position;

    }


    public void MovePlatformUp()
    {
        targetPos = savedPosition + Vector3.up * moveHeight;
        currentState = true;
    }

    public void MovePlatformDown()
    {
        targetPos = savedPosition;
        currentState = false;
    }

    void FixedUpdate()
    {
        if(stopped = Vector3.Distance(platformRb.transform.position, targetPos) < 0.05) return;
        platformRb.MovePosition(Vector3.MoveTowards(platformRb.position, targetPos, speed * Time.fixedDeltaTime));

    }


    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (!col) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * moveHeight, col.bounds.size);

        if (!platform) return;
        col = platform.GetComponent<Collider>();
        if (!col) return;

        Gizmos.DrawWireCube(transform.position + Vector3.up * (moveHeight - col.bounds.size.y / 2), col.bounds.size.y * Vector3.up + new Vector3(1f, 0, 1f));
    }

}
