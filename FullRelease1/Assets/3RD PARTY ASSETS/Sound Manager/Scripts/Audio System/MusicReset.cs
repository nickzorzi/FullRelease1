using UnityEngine;

public class MusicReset : MonoBehaviour
{
    void OnDisable()
    {
        SoundManager.instance.RestartCurrentTrack();
    }
}
