using UnityEngine;
using UnityEngine.InputSystem;

public class HidingSpot : MonoBehaviour
{

    [SerializeField] Transform entrySpotTransform;
    [SerializeField] Transform hidingTransform;

    [Header("Animation Settings")]
    [SerializeField] float moveToEntrySpeed = 1f;
    [SerializeField] float moveToHidingSpeed = 1.5f;

    Transform player;

    bool isHideSpotActive;
    InteractModule interactModule;


    private void Start()
    {
        player = PlayerManager.Instance.transform;
        interactModule = GetComponent<InteractModule>();
    }



    private void Update()
    {
        if(isHideSpotActive && InputManager.InteractPressed)
        {
            HUDManager.instance.HideInteract(gameObject);
            LeaveHidingSpot();
        }
    }

    public void TriggerEvent()
    {
        LeanTween.move(player.gameObject, entrySpotTransform, moveToEntrySpeed)
            .setOnComplete(() =>
            {
                LeanTween.move(player.gameObject, hidingTransform.position, moveToHidingSpeed);
                LeanTween.rotate(player.gameObject, hidingTransform.eulerAngles, moveToHidingSpeed);
                PlayerManager.Instance.EnterHideMode();
                isHideSpotActive = true;
                HUDManager.instance.DisplayInteract(gameObject, interactModule.displayText, true);
            });
    }

    public void LeaveHidingSpot()
    {
        isHideSpotActive = false;

        LeanTween.move(player.gameObject, entrySpotTransform, moveToEntrySpeed)
            .setOnComplete(() =>
            {
                PlayerManager.Instance.LeaveHideMode();
                interactModule.ResetInteract();
                PlayerManager.Instance.LeaveHideMode();
            });
    }

}
