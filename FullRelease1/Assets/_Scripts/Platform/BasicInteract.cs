using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BasicInteract : MonoBehaviour
{

    [SerializeField] float interactRange = 5f;
    [SerializeField] Vector3 scanOffset = Vector3.up * .5f;

    [Tooltip("If on, the robot cannot interact with Object")]
    [SerializeField] bool playerInteractOnly;

    [SerializeField] float inBetweenDelay = .5f;

    bool cd;

    public UnityEvent onInteract;


    private void Update()
    {
        if(!cd && CoreFunctions.PlayerProximityCheck(transform.position + scanOffset,interactRange,playerInteractOnly))
        {
            onInteract?.Invoke();
            StartCoroutine(delay());
        }   
    }


    IEnumerator delay()
    {
        cd = true;
        yield return new WaitForSeconds(inBetweenDelay);
        cd = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + scanOffset, interactRange);

        int count = onInteract.GetPersistentEventCount();

        if (count > 0)
        {

            Gizmos.color = Color.green;

            for (int i = 0; i < count; i++)
            {
                Object target = onInteract.GetPersistentTarget(i);


                if (target is Component comp)
                {
                    Gizmos.DrawLine(transform.position + scanOffset, comp.transform.position);
                }
                else if (target is GameObject go)
                {
                    Gizmos.DrawLine(transform.position + scanOffset, go.transform.position);
                }
            }
        }
    }
}
