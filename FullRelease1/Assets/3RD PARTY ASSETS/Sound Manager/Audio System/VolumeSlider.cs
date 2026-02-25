using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{

    [System.Serializable]
    public enum VolumeCategory
    {
        Master,
        Music,
        SFX
    }

    public VolumeCategory affectVolume;
    [SerializeField] Slider slider;


    private void Start()
    {

        setupSlider();

    }





    void setupSlider()
    {
        switch(affectVolume)
        {
            case VolumeCategory.Master:
                slider.value = SoundManager.instance.getMasterVolume();
                slider.onValueChanged.AddListener(SoundManager.instance.setMasterVolume);
                break;

            case VolumeCategory.Music:
                slider.value = SoundManager.instance.getMusicVolume();
                slider.onValueChanged.AddListener(SoundManager.instance.setMusicVolume);
                break;

            case VolumeCategory.SFX:
                slider.value = SoundManager.instance.getSFXVolume();
                slider.onValueChanged.AddListener(SoundManager.instance.setSFXVolume);
                break;


        }
    }


}
