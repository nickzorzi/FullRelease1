using UnityEngine;

public class ScannerCheck : MonoBehaviour
{
    public RectTransform leftHand;
    public RectTransform rightHand;
    public RectTransform leftScanner;
    public RectTransform rightScanner;

    void Start()
    {
        
    }

    void Update()
    {
        Rect leftH = leftHand.GetWorldRect();
        Rect rightH = rightHand.GetWorldRect();
        Rect leftS = leftScanner.GetWorldRect();
        Rect rightS = rightScanner.GetWorldRect();

        if (leftH.Overlaps(leftS) && rightH.Overlaps(rightS))
        {
            Debug.Log("Scanners sequenced");
        }
    }
}
