using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Camera Controls")]
    [SerializeField] Transform playerPOVTarget;
    [SerializeField] float povSwitchSpeed = 0.2f;
    [SerializeField] float povSwitchCD = .1f;
    

    [Header("Zoom Controls")]
    [SerializeField] GameObject playerModel;
    [SerializeField] float zoomInSpeed = .1f;


    [Header("Flash light Controls")]
    [SerializeField] GameObject flashlight;
    [SerializeField] SoundSystem.SoundLib flashlightSFX;



    public static Action OnPlayerZoomIn;
    public static Action OnPlayerZoomOut;


    Vector3 startCamPos;

    bool isZoomActive;
    bool isPOVSwitchCD;
    bool isPOVOnLeftSide;


    private void Awake()
    {
        startCamPos = playerPOVTarget.localPosition;
        flashlight.SetActive(false);
    }


    public void OnFlashlight()
    {
        flashlight.SetActive(!flashlight.activeSelf);
        SoundManager.instance.playSound(flashlightSFX, transform);
    }

    public void OnPlayerZoom(InputValue value)
    {
        if(!isZoomActive && value.Get<float>() > 0) // FPS
        {
            LeanTween.moveLocal(playerPOVTarget.gameObject, Vector3.down * .5f, zoomInSpeed)
                .setOnComplete(() =>
                {
                    playerModel.SetActive(false);
                    OnPlayerZoomIn?.Invoke();
                });


            if (isPOVOnLeftSide) SetFPSPOVSettings();

            isZoomActive = true;
        }
        else if(isZoomActive && value.Get<float>() <= 0) // Third Person
        {
            LeanTween.moveLocal(playerPOVTarget.gameObject, startCamPos, zoomInSpeed)
                .setOnStart(() =>
                {
                    playerModel.SetActive(true);
                    OnPlayerZoomOut?.Invoke();
                });
            isZoomActive = false;
        }

 
    }

    public void OnSwitchPOV()
    {
        //Debug.Log("Input Recieved");

        if (isPOVSwitchCD || isZoomActive) return;

        StartCoroutine(SwitchPlayerPOV());

    }

    void SetFPSPOVSettings()
    {
        isPOVOnLeftSide = false;
        LeanTween.moveLocalX(
            flashlight,
            -flashlight.transform.localPosition.x,
            povSwitchSpeed);
    }


    IEnumerator SwitchPlayerPOV()
    {
        isPOVSwitchCD = true;

        LeanTween.moveLocalX(
            playerPOVTarget.gameObject, 
            -playerPOVTarget.localPosition.x, 
            povSwitchSpeed);

        isPOVOnLeftSide = !isPOVOnLeftSide;

        LeanTween.moveLocalX(
            flashlight,
            -flashlight.transform.localPosition.x,
            povSwitchSpeed);

        yield return new WaitForSeconds(povSwitchCD + povSwitchSpeed);

        isPOVSwitchCD = false;
    }


}
