using CMF;
using System;
using UnityEngine;

public class POVChange : MonoBehaviour
{
    public CameraDistanceRaycaster camDisRay;
    public Transform mainCam;
    public Transform third;
    public Transform first;

    private bool isFirstPOV;
    
    void Start()
    {
        camDisRay = GetComponent<CameraDistanceRaycaster>();
    }

    void Update()
    {
        if (InputManager.EquipPressed)
        {
            if (!isFirstPOV)
            {
                isFirstPOV = true;

                camDisRay.enabled = false;

                mainCam.position = first.position;

                mainCam.SetParent(first);
            }
            else
            {
                isFirstPOV = false;

                mainCam.position = third.position;

                mainCam.SetParent(third);

                camDisRay.enabled = true;
            }
        }
    }
}
