using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public SoundDataStorage sounds;
    [Header("Sound Controls")]
    public AudioMixer audioMixer;


    [Header("Audio Source Management")]
    [SerializeField] int sfxPoolSize = 3;
    [SerializeField] AudioSource primaryMusicAudioSource;
    [SerializeField] AudioSource secondaryMusicAudioSource;
    [SerializeField] AudioSource audioSourcePrefab;

    bool songsFading;

    [Header("Music Settings")]
    [Min(0)]
    [SerializeField] float songFadeTime = .5f;
    [SerializeField] AnimationCurve fadeCurve;
    bool loadingNextSong;
    bool loopingQueue;

    [Header("Debug")]
    [SerializeField]private Queue<AudioSource> audioPool = new Queue<AudioSource>();
    List<repeatAudio> repeatingAudioList = new List<repeatAudio>();
    private Queue<AudioClip> songQueue = new Queue<AudioClip>();

    public class repeatAudio
    {
        public GameObject source;
        public string AudioID;
        public AudioSource affectedSource;
        public SoundDataStorage.SoundDataPack audioPack;
        public AudioClip clip;
        public repeatAudio(GameObject source, AudioSource affectedSource, SoundDataStorage.SoundDataPack audioPack, AudioClip clip, string AudioID)
        {
            this.source = source;
            this.affectedSource = affectedSource;
            this.audioPack = audioPack;
            this.clip = clip;
        }
    }


    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        InitializeAudioPool();



    }


    private void Update()
    {
        #region Repeating Audio System

        foreach (var item in repeatingAudioList)
        {
            if(item.affectedSource.gameObject.activeSelf && !item.clip && !item.affectedSource.isPlaying)
            {
                item.affectedSource.clip = item.audioPack.audioClips[Random.Range(0, item.audioPack.audioClips.Count)];
                item.affectedSource.Play();
            }
        }
        for(int i = 0; i < repeatingAudioList.Count; i++)
        {
            if (repeatingAudioList[i].source == null)
            {
                repeatingAudioList.RemoveAt(i);
                break;
            }
        }
        #endregion

        if(primaryMusicAudioSource.isPlaying && primaryMusicAudioSource.clip != null) 
        {
            float timeLeft = primaryMusicAudioSource.clip.length - primaryMusicAudioSource.time;
            
            if(!loadingNextSong && songQueue.Count > 0 && timeLeft <= songFadeTime) // Play next Song in Queue
            {
              
                if (loopingQueue)
                {
                    AudioClip temp = songQueue.Peek();
                    StartCoroutine(FadeInOutSongs(songQueue.Dequeue()));
                    songQueue.Enqueue(temp);
                }
                else
                {
                    StartCoroutine(FadeInOutSongs(songQueue.Dequeue()));

                }
            }
        }
    }


    public void setMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
    }
    public void setMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }
    public void setSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }


    public float getMasterVolume()
    {
        audioMixer.GetFloat("MasterVolume", out float value);

        return value;
    }
    public float getMusicVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float value);

        return value;
    }
    public float getSFXVolume()
    {
        audioMixer.GetFloat("SFXVolume", out float value);

        return value;
    }

    /// <summary>
    ///  Used To Play a Specified SFX Sound Once
    /// </summary>
    /// <param name="volume"> float value from 0 - 1 float | volume is still affected by SFX and Master volume Controls</param>
    public void playSound(string soundPath, Transform location, float volume = 1, bool pitchShift = false, bool randomSound = true, int soundIndex = 0)
    {

        string[] temp = soundPath.Split('.');

        string categoryName = temp[0];
        string soundName = temp[1];

        if (sounds == null)
        {
            Debug.LogWarning("There is no sound Library");
            return;
        }

        AudioClip clip = FindClip(categoryName,soundName,randomSound,soundIndex);

        if (clip == null)
        {
            Debug.LogWarning($"No Audio Clip found in {categoryName}.{soundName}");
            return;
        }

        if (instance.audioPool.Count > 0)
        {
            AudioSource audioSource = instance.audioPool.Dequeue();

            audioSource.transform.position = location.position;
            audioSource.gameObject.SetActive(true);
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitchShift ? Random.Range(0.5f, 1.5f) : 1;
            audioSource.Play();

            StartCoroutine(ReturnAudioSourceToPool(audioSource, clip.length));
        }
        else
        { // This will Create a new audio Source and add it back to the Queue to be used again with the rest of the Pool
            Debug.Log("Audio Pool is Empty | Adding new Source");
            AudioSource newAudioSource = Instantiate(audioSourcePrefab, transform);
            newAudioSource.name = "SFX Audio " + sfxPoolSize + 1;
            newAudioSource.gameObject.SetActive(true);
            newAudioSource.clip = clip;
            newAudioSource.volume = volume;
            newAudioSource.pitch = pitchShift ? Random.Range(0.75f, 1.25f) : 1;
            newAudioSource.Play();
            sfxPoolSize++;

            newAudioSource.transform.position = location.position;

            StartCoroutine(ReturnAudioSourceToPool(newAudioSource, clip.length));
        }
    }



    #region Repeating Sound System

    /// <summary>
    /// Creates a Dedicated AudioSource for Clip Use PlayRepeating() to play the audio
    /// </summary>
    /// <param name="identifier"> Used for PlayRepeating() and StopRepeating()</param>
    public void SetRepeatingAudio(GameObject obj, string categoryName, string soundName,string identifier, bool randomSound = true, int soundIndex = 0)
    {
        if(repeatingAudioList.Any(x => x.audioPack.name == soundName && x.source == obj))
        {
            Debug.LogWarning("Obj Already has a dedicated AudioSource for SoundPack");
            return;
        }

        AudioClip clip = null;
        SoundDataStorage.SoundDataPack soundPack = new SoundDataStorage.SoundDataPack();
        if(!randomSound) 
        {
            clip = FindClip(categoryName, soundName,randomSound,soundIndex);
        }
        else
        {
            try
            {
                soundPack = sounds.AudioList.First(x => x.categoryName == categoryName).sounds.First(y => y.name == soundName);
            }
            catch 
            {
                Debug.LogError($"Could not find Sound Data Pack for {categoryName}.{soundName}");
                return;
            }
        }

        AudioSource newSource = Instantiate(audioSourcePrefab, transform);
        newSource.name = obj.name + " Dedicated Repeating Audio";
        newSource.gameObject.SetActive(false);

        repeatingAudioList.Add(new repeatAudio(obj,newSource,soundPack,clip,identifier));

    }
    /// <summary>
    /// Start Playing the repeated Audio 
    /// </summary>
    /// <param name="identifier"> identifier set in SetRepeatingAudio()</param>
    /// <param name="volume"> float value from 0 - 1 float | volume is still affected by SFX and Master volume Controls</param>
    public static void PlayRepeating(GameObject obj, string identifier, float volume = 1f)
    {
        repeatAudio audioData;
        try
        {
            audioData = instance.repeatingAudioList.First(x => x.source == obj && x.AudioID == identifier);
        }
        catch 
        {
            Debug.LogWarning($"Could not Find Dedicated Audio for {obj.name}.{identifier}");
            return;
        }

        if (audioData.clip)
        {
            audioData.affectedSource.gameObject.SetActive(true);
            audioData.affectedSource.clip = audioData.clip;
            audioData.affectedSource.loop = true;

        }
        else
        {
            audioData.affectedSource.gameObject.SetActive(true);
            audioData.affectedSource.clip = audioData.audioPack.audioClips[Random.Range(0,audioData.audioPack.audioClips.Count)];
            audioData.affectedSource.loop = false;
        }

        audioData.affectedSource.volume = volume;
        audioData.affectedSource.Play();
    }
    /// <summary>
    /// Stop Playing the repeated Audio 
    /// </summary>
    /// <param name="identifier"> identifier set in SetRepeatingAudio()</param>
    public static void StopRepeating(GameObject obj, string identifier)
    {
        repeatAudio audioData;
        try
        {
            audioData = instance.repeatingAudioList.First(x => x.source == obj && x.AudioID == identifier);
        }
        catch
        {
            Debug.LogWarning($"Could not Find Dedicated Audio for {obj.name}.{identifier}");
            return;
        }

        audioData.affectedSource.Stop();
        audioData.affectedSource.gameObject.SetActive(false);

    }
    /// <summary>
    /// Deletes Audio Source
    /// </summary>
    public static void DeleteRepeating(GameObject obj, string identifier)
    {
        repeatAudio audioData;
        try
        {
            audioData = instance.repeatingAudioList.First(x => x.source == obj && x.AudioID == identifier);
        }
        catch
        {
            Debug.LogWarning($"Could not Find Dedicated Audio for {obj.name}.{identifier}");
            return;
        }

        instance.repeatingAudioList.Remove(audioData);
    }


    #endregion


    #region Music System


    public void RestartCurrentTrack()
    {
        if (songsFading) return;

        StartCoroutine(FadeTrackInNOut());
        
    }

    public void SwitchTracks(bool switchToPrimary)
    {
        if (switchToPrimary && primaryMusicAudioSource.volume > 0 || !switchToPrimary && secondaryMusicAudioSource.volume > 0 || songsFading) return;

        StartCoroutine(FadeBetweenTracks());
        
    }


    IEnumerator FadeTrackInNOut()
    {
        float fadeDuration = 1.5f;
        float timePassed = 0;
        songsFading = true;

        if (primaryMusicAudioSource.volume > 0)
        {
            while (timePassed / fadeDuration < 1)
            {
                timePassed += Time.deltaTime;
                primaryMusicAudioSource.volume = Mathf.Lerp(1, 0, timePassed / fadeDuration);
                yield return null;
            }

            primaryMusicAudioSource.Play();
            secondaryMusicAudioSource.Play();
            timePassed = 0;
            yield return new WaitForSeconds(.5f);

            while (timePassed / fadeDuration < 1)
            {
                timePassed += Time.deltaTime;
                primaryMusicAudioSource.volume = Mathf.Lerp(0, 1, timePassed / fadeDuration);
                yield return null;
            }
        }
        else if (secondaryMusicAudioSource.volume > 0)
        {
            while (timePassed / fadeDuration < 1)
            {
                timePassed += Time.deltaTime;
                secondaryMusicAudioSource.volume = Mathf.Lerp(1, 0, timePassed / fadeDuration);
                yield return null;
            }

            primaryMusicAudioSource.Play();
            secondaryMusicAudioSource.Play();
            timePassed = 0;
            yield return new WaitForSeconds(.5f);

            while (timePassed / fadeDuration < 1)
            {
                timePassed += Time.deltaTime;
                secondaryMusicAudioSource.volume = Mathf.Lerp(0, 1, timePassed / fadeDuration);
                yield return null;
            }
        }
        songsFading = false;
    }


    IEnumerator FadeBetweenTracks()
    {
        float fadeDuration = 2.5f;
        float timePassed = 0;
        songsFading = true;

        if(primaryMusicAudioSource.volume > 0)
        {
            while (timePassed / fadeDuration < 1)
            {
                timePassed += Time.deltaTime;
                primaryMusicAudioSource.volume = Mathf.Lerp(1,0, timePassed / fadeDuration);
                secondaryMusicAudioSource.volume = Mathf.Lerp(0, 1, timePassed / fadeDuration);
                yield return null;
            }
        }
        else if(secondaryMusicAudioSource.volume > 0)
        {
            while (timePassed / fadeDuration < 1)
            {
                timePassed += Time.deltaTime;
                secondaryMusicAudioSource.volume = Mathf.Lerp(1, 0, timePassed / fadeDuration);
                primaryMusicAudioSource.volume = Mathf.Lerp(0, 1, timePassed / fadeDuration);
                yield return null;
            }
        }
        songsFading = false;
    }

    IEnumerator FadeInOutSongs(AudioClip newClip)
    {
        loadingNextSong = true;
        float fadeDuration = 3f;
        float startVolume = primaryMusicAudioSource.volume;
        float timePassed = 0;

        // fade out
        while (timePassed / fadeDuration > 1f)
        {
            timePassed += Time.deltaTime;
   
            primaryMusicAudioSource.volume = Mathf.Lerp(startVolume, 0, timePassed / fadeDuration);
            yield return null;
        }
        timePassed = 0;
        primaryMusicAudioSource.Stop();
        primaryMusicAudioSource.clip = newClip;
        primaryMusicAudioSource.Play();

        while (timePassed / fadeDuration > 1f)
        {
            timePassed += Time.deltaTime;

            primaryMusicAudioSource.volume = Mathf.Lerp(0, startVolume, timePassed / fadeDuration);
            yield return null;
        }


        loadingNextSong = false;
    }


    #endregion

    IEnumerator ReturnAudioSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        source.gameObject.SetActive(false);
        audioPool.Enqueue(source);
    }

    // grabs either a random clip at specified location or a specific clip from the list
    public AudioClip FindClip(string categoryName, string soundName, bool randomSound = true, int soundIndex = default)
    {
        AudioClip clip = null;

        if (randomSound)
        {
            SoundDataStorage.SoundDataPack selectedPack;
            try
            {
                selectedPack =
                    instance.sounds.AudioList.First(x => x.categoryName == categoryName).sounds.FirstOrDefault(y => y.name == soundName);
            }
            catch
            {
                Debug.LogWarning($"No Audio Clip found in {categoryName}.{soundName}");
                selectedPack = default;
            }
            if (selectedPack.audioClips.Count <= 0) { Debug.LogWarning($"{categoryName}.{soundName} does not exist, Check Spelling");  return null; }
            clip = selectedPack.audioClips[Random.Range(0, selectedPack.audioClips.Count)];

        }
        else
        {
            try
            {
                clip = instance.sounds.AudioList.First(x => x.categoryName == categoryName).sounds.FirstOrDefault(y => y.name == soundName).audioClips[soundIndex];
            }
            catch
            {
                Debug.LogWarning("sound Index is out of Range of array");
            }
        }
        return clip;
    }

    // grabs either a random clip at specified location or a specific clip from the list
    public AudioClip FindClip(string soundPath, bool randomSound = true, int soundIndex = default)
    {
        AudioClip clip = null;

        string[] temp = soundPath.Split('.');

        string categoryName = temp[0];
        string soundName = temp[1];

        if (randomSound)
        {
            SoundDataStorage.SoundDataPack selectedPack;
            try
            {
                selectedPack =
                    instance.sounds.AudioList.First(x => x.categoryName == categoryName).sounds.FirstOrDefault(y => y.name == soundName);
            }
            catch
            {
                Debug.LogWarning($"No Audio Clip found in {categoryName}.{soundName}");
                selectedPack = default;
            }
            if (selectedPack.audioClips.Count <= 0) { Debug.LogWarning($"{categoryName}.{soundName} does not exist, Check Spelling"); return null; }
            clip = selectedPack.audioClips[Random.Range(0, selectedPack.audioClips.Count)];

        }
        else
        {
            try
            {
                clip = instance.sounds.AudioList.First(x => x.categoryName == categoryName).sounds.FirstOrDefault(y => y.name == soundName).audioClips[soundIndex];
            }
            catch
            {
                Debug.LogWarning("sound Index is out of Range of array");
            }
        }
        return clip;
    }

    void InitializeAudioPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource newAudioSource = Instantiate(audioSourcePrefab, transform);
            newAudioSource.name = "SFX Audio Source " + i+1;
            newAudioSource.gameObject.SetActive(false); // Initially inactive
            audioPool.Enqueue(newAudioSource);
        }
    }

}
