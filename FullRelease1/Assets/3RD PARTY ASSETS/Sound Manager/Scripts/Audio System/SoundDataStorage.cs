using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Sound Library", menuName ="Scriptable Objects/Sound Library")]
public class SoundDataStorage : ScriptableObject
{
    [System.Serializable]
    public struct SoundDataPack
    {
        [Tooltip("This is used to find and play sounds from this DataPack")]
        public string name;
        public List<AudioClip> audioClips;
    }
    [System.Serializable]
    public struct SoundCategory
    {
        public string categoryName;
        public List<SoundDataPack> sounds;
    }

    
    public List<SoundCategory> AudioList = new List<SoundCategory>();


}
