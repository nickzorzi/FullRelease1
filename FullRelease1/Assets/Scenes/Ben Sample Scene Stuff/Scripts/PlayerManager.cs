using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Camera Controls")]
    [SerializeField] Transform playerPOVTarget;
    [SerializeField] float povSwitchSpeed = 0.2f;
    [SerializeField] float povSwitchCD = .1f;
    [Space(15)]
    [SerializeField] float zoomDist = -1.5f;

    [Header("Flash light Controls")]
    [SerializeField] GameObject flashlight;
    [SerializeField] SoundSystem.SoundLib flashlightSFX;



    float startZoomDist;

    bool isZoomActive;
    bool isPOVSwitchCD;
    private void Awake()
    {
        startZoomDist = playerPOVTarget.localPosition.z;
        flashlight.SetActive(false);
    }


    public void OnFlashlight()
    {
        flashlight.SetActive(!flashlight.activeSelf);
        SoundManager.instance.playSound(flashlightSFX, transform);
    }

    public void OnPlayerZoom(InputValue value)
    {
        if(!isZoomActive && value.Get<float>() > 0)
        {
            LeanTween.moveLocalZ(
                playerPOVTarget.gameObject,
                zoomDist,
                0.15f);
            isZoomActive = true;
        }
        else if(isZoomActive && value.Get<float>() <= 0)
        {
            LeanTween.moveLocalZ(
                playerPOVTarget.gameObject,
                startZoomDist,
                0.15f);
            isZoomActive = false;
        }

 
    }

    public void OnSwitchPOV()
    {
        Debug.Log("Input Recieved");

        if (isPOVSwitchCD) return;

        StartCoroutine(SwitchPlayerPOV());

    }

    IEnumerator SwitchPlayerPOV()
    {
        isPOVSwitchCD = true;

        LeanTween.moveLocalX(
            playerPOVTarget.gameObject, 
            -playerPOVTarget.localPosition.x, 
            povSwitchSpeed);

        LeanTween.moveLocalX(
            flashlight,
            -flashlight.transform.localPosition.x,
            povSwitchSpeed);

        yield return new WaitForSeconds(povSwitchCD + povSwitchSpeed);

        isPOVSwitchCD = false;
    }


}
