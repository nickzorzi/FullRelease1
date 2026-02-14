using UnityEngine;

public static class CoreFunctions
{
    

    /// <summary>
    /// This checks if the player or robot are in interactable range only if the person does not have logic paused
    /// </summary>
    public static bool PlayerProximityCheck(Vector3 start, float distance, bool playerOnly = false)
    {

        //Debug.Log("Test");

        if (PlayerController.instance)
        {
            if (!PlayerController.instance.logicPaused && Vector3.Distance(start, PlayerController.instance.transform.position + Vector3.one) < distance ||
            !playerOnly && !RobotController.instance.logicPaused && Vector3.Distance(start, RobotController.instance.transform.position + Vector3.one) < distance)
            {
                Vector3 target = !PlayerController.instance.logicPaused ?
                    (PlayerController.instance.transform.position + Vector3.up - start).normalized :
                    (RobotController.instance.transform.position - start).normalized;

                if (InputManager.InteractPressed && Physics.Raycast(start, target, out RaycastHit hit))
                {
                    //Debug.Log(hit.collider.gameObject.name);
                    Debug.DrawRay(start, target, Color.yellow, 3);
                    if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("BOING")) return true;

                }
            }
        }
        else
        {
            //Debug.Log("New Character Version");

            if (PlayerRefTag.instance && Vector3.Distance(start, PlayerRefTag.instance.transform.position + Vector3.one) < distance)

            {
                Vector3 target = (PlayerRefTag.instance.transform.position + Vector3.up - start).normalized;


                if (InputManager.InteractPressed && Physics.Raycast(start, target, out RaycastHit hit))
                {
                    //Debug.Log(hit.collider.gameObject.name);
                    Debug.DrawRay(start, target, Color.yellow, 3);
                    if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("BOING")) return true;

                }
            }
        }

            return false;
    }



}
