using UnityEngine;

public class fadeTrackTest : MonoBehaviour
{




    private void OnDisable()
    {
        FadeTrackTest();
    }

    [ContextMenu("Switch Track Fade")]
    void FadeTrackTest()
    {
       SoundManager.instance.RestartCurrentTrack();
    }

}
