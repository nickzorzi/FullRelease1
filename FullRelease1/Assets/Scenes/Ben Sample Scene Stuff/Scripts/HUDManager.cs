using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;


    [SerializeField] CanvasGroup fps_HUD;

    [Header("Interact Settings")]
    [SerializeField] InputActionReference interactAction;
    [SerializeField] CanvasGroup interact_HUD;
    [SerializeField] TMP_Text interactTMP;
    [Space(5)]
    [SerializeField] float interactDisplaySpeed = 0.25f;
    [SerializeField] Material debugMaterial;


    [HideInInspector]
    public GameObject objInteractID; // use this so we will only allow 1 interact at a time instead of getting overridden


    Material objInteractStartMat;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PlayerManager.OnPlayerZoomIn += ShowDot;
        PlayerManager.OnPlayerZoomOut += HideDot;

        fps_HUD.alpha = 0;
        interact_HUD.alpha = 0;
    }


    void HideDot()
    {
        LeanTween.alphaCanvas(fps_HUD, 0, .15f);
    }

    void ShowDot()
    {
        LeanTween.alphaCanvas(fps_HUD, 1, .15f);
    }

    public bool DisplayInteract(GameObject objID, string interactText, bool forceThrough = false)
    {
        if (objInteractID != null && !forceThrough) return false;

        objInteractID = objID;


        // This is debugging Remove shit when we want
        if (objID.GetComponent<MeshRenderer>())
        {
            objInteractStartMat = objID.GetComponent<MeshRenderer>().material;
            objID.GetComponent<MeshRenderer>().material = debugMaterial;
        }



        LeanTween.alphaCanvas(interact_HUD,1,interactDisplaySpeed);

        string bindingDisplay = interactAction.action.GetBindingDisplayString();
        string interactDisplayText = string.IsNullOrEmpty(interactText) ? string.Empty : " with " + interactText;

        interactTMP.text = $"Press [<b>{bindingDisplay}</b>] to Interact{interactDisplayText}";

        return true;
    }

    public void HideInteract(GameObject objID)
    {
        if(objInteractID == null || objInteractID != objID) return;

        // This is debugging Remove shit when we want
        if (objID.GetComponent<MeshRenderer>())
        {
            objID.GetComponent<MeshRenderer>().material = objInteractStartMat;
        }

        objInteractID = null;
        objInteractStartMat = null;

        LeanTween.alphaCanvas(interact_HUD, 0, interactDisplaySpeed);


    }
}
