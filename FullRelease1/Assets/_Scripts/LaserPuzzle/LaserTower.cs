using UnityEngine;

public class LaserTower : MonoBehaviour
{
    [Header("Laser Basics")]
    public bool isWin;
    public bool canFire;
    [SerializeField] bool isOn;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform laserPoint;
    [SerializeField] Material laserMat;

    [Header("Tower Basics")]
    [SerializeField] MeshRenderer headMesh;
    [SerializeField] Material redMat;
    [SerializeField] Material greenMat;

    [Header("Hit Basics")]
    public bool isMoved;
    [SerializeField] LaserTower laserTowerHit;
    Rigidbody rb;


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        rb = GetComponentInChildren<Rigidbody>();

        isMoved = true;
    }

    private void Update()
    {
        if (isWin)
        {
            if (canFire)
            {
                if (headMesh.material == redMat) headMesh.material = greenMat;
            }
            else
            {
                if (headMesh.material == greenMat) headMesh.material = redMat;
            }

            return;
        }

        if (canFire)
        {
            if (lineRenderer.enabled == false) lineRenderer.enabled = true;

            if (!isOn)
            {
                if (headMesh.material == redMat) headMesh.material = greenMat;

                isOn = true;
            }

            LaserShoot();
        }
        else
        {
            if (lineRenderer.enabled == true) lineRenderer.enabled = false;

            if (isOn)
            {
                if (headMesh.material == greenMat) headMesh.material = redMat;

                if (laserTowerHit != null)
                {
                    laserTowerHit.canFire = false;
                }

                isOn = false;
            }
        }
    }

    void LaserShoot()
    {
        RaycastHit hit;
        LayerMask layerIgnore = ~LayerMask.GetMask("LaserTower");

        if (Physics.Raycast(laserPoint.position, laserPoint.forward, out hit, Mathf.Infinity, layerIgnore))
        {
            if (hit.collider.CompareTag("LaserHead"))
            {
                laserTowerHit = hit.transform.parent.gameObject.GetComponent<LaserTower>();

                if (isMoved || Mathf.Approximately(rb.linearVelocity.sqrMagnitude, Vector3.zero.sqrMagnitude))
                {
                    DrawLaser(laserPoint.position, hit.point, Color.green);

                    isMoved = false;
                }

                laserTowerHit.canFire = true;
            }
            else
            {
                DrawLaser(laserPoint.position, hit.point, Color.red);

                if (laserTowerHit != null)
                {
                    laserTowerHit.canFire = false;
                }
            }
        }
    }

    void DrawLaser(Vector3 startPos, Vector3 endPos, Color color)
    {
        lineRenderer.material = laserMat;
        lineRenderer.material.color = color;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
}
