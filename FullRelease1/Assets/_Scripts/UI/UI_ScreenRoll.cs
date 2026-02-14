using UnityEngine;
using UnityEngine.UI;

public class UI_ScreenRoll : MonoBehaviour
{
    [SerializeField] RawImage rollingImage;


    [Space(15)]
    [SerializeField] float speed;

    void Update() // UI should use Update, not FixedUpdate
    {
        Rect uv = rollingImage.uvRect;
        uv.y = Mathf.Repeat(uv.y + speed * Time.deltaTime, 1f);
        uv.x = Mathf.Repeat(uv.x + speed * Time.deltaTime, 1f);
        rollingImage.uvRect = uv;
    }

}
