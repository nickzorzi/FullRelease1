using UnityEngine;

public class HandsController : MonoBehaviour
{
    public bool isLeft;

    void Start()
    {
        
    }

    void Update()
    {
        if (isLeft)
        {
            if (InputManager.Mouse1Held)
            {
                transform.position = new Vector2(transform.position.x + InputManager.Look.x, transform.position.y + InputManager.Look.y);
            }
        }
        else
        {
            if (InputManager.PlayerZoomHeld)
            {
                transform.position = new Vector2(transform.position.x + InputManager.Look.x, transform.position.y + InputManager.Look.y);
            }
        }
    }
}
