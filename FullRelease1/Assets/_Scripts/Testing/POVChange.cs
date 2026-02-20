using CMF;
using UnityEngine;

public class POVChange : MonoBehaviour
{
    public CameraDistanceRaycaster camDisRay;
    public Transform mainCam;
    public Transform third;
    public Transform first;

    public GameObject playerSprite;
    public GameObject playerHand;

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

                playerSprite.SetActive(false);

                mainCam.position = first.position;

                mainCam.SetParent(first);

                playerHand.SetActive(true);
            }
            else
            {
                isFirstPOV = false;

                playerHand.SetActive(false);

                mainCam.position = third.position;

                mainCam.SetParent(third);

                playerSprite.SetActive(true);

                camDisRay.enabled = true;
            }
        }
    }
}
