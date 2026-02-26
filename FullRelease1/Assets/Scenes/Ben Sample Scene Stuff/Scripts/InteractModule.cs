using System.Collections;
using UnityEngine;

public class InteractModule : MonoBehaviour
{

    [SerializeField] float distanceToInteract = 2f;
    [SerializeField] float distCheckSpeed = .3f;
    [SerializeField] string displayText;
    
    [Space(15)]
    [SerializeField] Vector3 playerPosOffset = Vector3.up;


    [Header("Debug")]
    [SerializeField] bool showDebugLines;


    Transform playerPos;

    bool isShowing;


    private void Start()
    {
        StartCoroutine(CheckDistanceRoutine());

        playerPos = PlayerManager.Instance.transform;
    }


    IEnumerator CheckDistanceRoutine()
    {
        while (true)
        {
            if (playerPos == null)
            {
                yield return new WaitForSeconds(distCheckSpeed);
                continue;
            }

            float distance = Vector3.Distance(transform.position, playerPos.position);

            // Player entered range
            if (distance <= distanceToInteract)
            {
                if (!isShowing)
                {
                    isShowing = HUDManager.instance.DisplayInteract(gameObject, displayText);
                }
            }
            // Player left range
            else
            {
                if (isShowing && HUDManager.instance.objInteractID == gameObject)
                {
                    HUDManager.instance.HideInteract(gameObject);
                    isShowing = false;
                }
            }

            yield return new WaitForSeconds(distCheckSpeed);
        }
    }



    private void OnDrawGizmosSelected()
    {
        if (!showDebugLines) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToInteract);
    }
}
