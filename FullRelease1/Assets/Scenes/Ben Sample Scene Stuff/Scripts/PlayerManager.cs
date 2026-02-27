using CMF;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Camera Controls")]
    [SerializeField] GameObject playerModel;
    [SerializeField] Transform playerPOVTarget;
    [SerializeField] float povSwitchSpeed = 0.2f;
    [SerializeField] float povSwitchCD = .1f;
    

    [Header("Zoom Controls")]
    [SerializeField] float zoomInSpeed = .1f;
    [SerializeField] float zoomInOffset = -1f;

    [Header("Flash light Controls")]
    [SerializeField] GameObject flashlight;
    [SerializeField] SoundSystem.SoundLib flashlightSFX;



    public static Action OnPlayerZoomIn;
    public static Action OnPlayerZoomOut;


    Vector3 startCamPos;

    float startSpeed;

    bool isZoomActive;
    bool isPOVSwitchCD;
    bool isPOVOnLeftSide;
    bool isHiding;

    private void Awake()
    {
        Instance = this;

        startCamPos = playerPOVTarget.localPosition;
        flashlight.SetActive(false);

        startSpeed = GetComponent<AdvancedWalkerController>().movementSpeed;
    }


    public void OnFlashlight()
    {
        if (isHiding) return;

        flashlight.SetActive(!flashlight.activeSelf);
        SoundManager.instance.playSound(flashlightSFX, transform);
    }

    public void OnPlayerZoom(InputValue value)
    {
        if(isHiding) return;

        if(!isZoomActive && value.Get<float>() > 0) // FPS
        {
            LeanTween.moveLocalZ(playerPOVTarget.gameObject, zoomInOffset, zoomInSpeed)
                .setOnComplete(() =>
                {
                    OnPlayerZoomIn?.Invoke();
                });


            if (isPOVOnLeftSide) SetFPSPOVSettings();

            isZoomActive = true;
        }
        else if(isZoomActive && value.Get<float>() <= 0) // Third Person
        {
            LeanTween.moveLocalZ(playerPOVTarget.gameObject, startCamPos.z, zoomInSpeed)
                .setOnStart(() =>
                {
                    OnPlayerZoomOut?.Invoke();
                });
            isZoomActive = false;
        }

 
    }

    public void OnSwitchPOV()
    {
        //Debug.Log("Input Recieved");

        if (isPOVSwitchCD || isHiding) return;

        StartCoroutine(SwitchPlayerPOV());

    }

    public void EnterHideMode()
    {
        GetComponent<AdvancedWalkerController>().movementSpeed = 0;

        isHiding = true;

        LeanTween.moveLocal(playerPOVTarget.gameObject, Vector3.down * .5f, zoomInSpeed)
            .setOnComplete(() =>
            {
                playerModel.SetActive(false);
            });


        if (isPOVOnLeftSide) SetFPSPOVSettings();

    }

    public void LeaveHideMode()
    {
        GetComponent<AdvancedWalkerController>().movementSpeed = startSpeed;

        LeanTween.moveLocal(playerPOVTarget.gameObject, startCamPos, zoomInSpeed)
            .setOnStart(() =>
            {
                playerModel.SetActive(true);
                OnPlayerZoomOut?.Invoke();
            });
        isHiding = false;
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
