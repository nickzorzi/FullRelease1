using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static float MouseSens;



    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
