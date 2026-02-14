using UnityEngine;

public class PlayerRefTag : MonoBehaviour
{
    public static PlayerRefTag instance;

    private void Awake()
    {
        instance = this;
    }
}
