using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;


    [SerializeField] CanvasGroup FPS_HUD;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PlayerManager.OnPlayerZoomIn += ShowDot;
        PlayerManager.OnPlayerZoomOut += HideDot;

        FPS_HUD.alpha = 0;
    }


    void HideDot()
    {
        LeanTween.alphaCanvas(FPS_HUD, 0, .15f);
    }

    void ShowDot()
    {
        LeanTween.alphaCanvas(FPS_HUD, 1, .15f);
    }
}
