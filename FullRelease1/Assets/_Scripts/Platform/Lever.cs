using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class Lever : MonoBehaviour
{

    [field: SerializeField] public Transform lever { get; private set; }
    [SerializeField] Lever[] linkLeverStations;

    [Space(15)]
    public UnityEvent leverOn;
    public UnityEvent leverOff;

    [Space(15)]
    [SerializeField] bool currentState = false;
    [SerializeField] float pressedCD = .5f;
    [SerializeField] bool cd;

    [Space(15)]
    [SerializeField] float interactRange = 3f;
    [SerializeField] bool playerInRange;


    private void Update()
    {
        if (CoreFunctions.PlayerProximityCheck(transform.position + Vector3.up, interactRange)) ActivateLever();
    }

    public void ActivateLever(bool linkMode = false)
    {
        if (cd) return;

        StartCoroutine(PressCooldown());

        if (currentState) onLeverOff();
        else onLeverOn();
        
        if(!linkMode)
        {
            foreach(Lever lever in linkLeverStations)
            {
                if (lever == this) { Debug.LogWarning("Lever Station Linked to itself: | " + name); continue; }
                lever.ActivateLever(true);
            }
        }

    }


    IEnumerator PressCooldown()
    {
        cd = true;
        yield return new WaitForSeconds(pressedCD);
        cd = false;
    }


    void onLeverOn(bool linkMode = false)
    {
        if (!linkMode)
        {
            //Debug.Log("Lever On!");
            leverOn?.Invoke();
        }

        currentState = true;
        LeanTween.rotateLocal(lever.gameObject, Vector3.left * 155, .5f); // 155 is just a rotation from blender I pulled
    }

    void onLeverOff(bool linkMode = false)
    {
        if (!linkMode)
        {
            //Debug.Log("Lever Off!");
            leverOff?.Invoke();
        }

        currentState = false;
        LeanTween.rotateLocal(lever.gameObject, new Vector3(-175,lever.eulerAngles.y, lever.eulerAngles.z), .5f); // this is getting canned for animation for sure
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + Vector3.up, interactRange);

        if (!lever) return;

        int count = leverOn.GetPersistentEventCount();

        if (count > 0)
        {

            Gizmos.color = Color.green;

            for(int i = 0; i < count; i++)
            {
                Object target = leverOn.GetPersistentTarget(i);


                if(target is Component comp)
                {
                    Gizmos.DrawLine(lever.position, comp.transform.position);
                }
                else if(target is GameObject go)
                {
                    Gizmos.DrawLine(lever.position, go.transform.position);
                }
            }
        }

        count = leverOff.GetPersistentEventCount();

        if (count > 0)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < count; i++)
            {
                Object target = leverOff.GetPersistentTarget(i);


                if (target is Component comp)
                {
                    Gizmos.DrawLine(transform.position + Vector3.up, comp.transform.position);
                }
                else if (target is GameObject go)
                {
                    Gizmos.DrawLine(transform.position + Vector3.up, go.transform.position);
                }
            }
        }

        if(linkLeverStations.Length > 0)
        {

            Gizmos.color = Color.yellow;

            foreach(Lever lever in linkLeverStations)
            {
                Gizmos.DrawLine(transform.position + Vector3.up, lever.lever.position);
            }
        }
    }

}
