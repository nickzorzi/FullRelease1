using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractModule : MonoBehaviour
{

    [SerializeField] float distanceToInteract = 2f;
    [SerializeField] float distCheckSpeed = .3f;
    public string displayText;
    
    [Space(15)]
    [SerializeField] Vector3 playerPosOffset = Vector3.up;

    [Space(15)]
    public UnityEvent onInteracted;

    [Header("Debug")]
    [SerializeField] bool showDebugLines;


    Transform playerPos;

    bool isShowing;

    [HideInInspector]
    public bool isInteracted;

    private void Start()
    {
        StartCoroutine(CheckDistanceRoutine());

        playerPos = PlayerManager.Instance.transform;
    }

    private void Update()
    {
        if(!isInteracted && isShowing && InputManager.InteractPressed)
        {
            isInteracted = true;
            onInteracted?.Invoke();
            HUDManager.instance.HideInteract(gameObject);
            isShowing = false;
        }
    }

    public void ResetInteract()
    {
        isInteracted = false;
        StartCoroutine(CheckDistanceRoutine());
    }


    IEnumerator CheckDistanceRoutine()
    {
        while (!isInteracted)
        {
            if (playerPos == null)
            {
                yield return new WaitForSeconds(distCheckSpeed);
                continue;
            }

            float distance = Vector3.Distance(transform.position, playerPos.position + playerPosOffset);

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
