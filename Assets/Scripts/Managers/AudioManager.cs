using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public enum SFXID
{
    ButtonClick,
    Success,
    Failure,
    SelectUpgrade,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager _instance;

    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("SFXs")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip successSoundSFX;
    [SerializeField] private AudioClip failureSoundSFX;
    [SerializeField] private AudioClip selectUpgradeSFX;

    [Header("BGMs")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private AudioClip inGameMusic;

    [Header("Events")]
    [SerializeField] private FloatPublisherSO setSFXSliderSO;
    [SerializeField] private FloatPublisherSO setBGMSliderSO;

    private Dictionary<SFXID, AudioClip> sfxMap;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<AudioManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXMap();
        }
        else
        {
            Debug.Log("Found more than one Audio Manager in the scene. Destroying the newest one");
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        SetAudioSliders();
        PlayBGM(bgm);
    }

    private void InitializeSFXMap()
    {
        sfxMap = new Dictionary<SFXID, AudioClip>
        {
            { SFXID.ButtonClick, buttonClickSFX },
            { SFXID.Success, successSoundSFX },
            { SFXID.Failure, failureSoundSFX },
            { SFXID.SelectUpgrade, selectUpgradeSFX },
        };
    }

    public void SetMusic(float volume)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    public void SetSFX(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    public void SetSFXSlider()
    {
        audioMixer.GetFloat("SFX", out float sfxVolume);
        float value = Mathf.Pow(10, sfxVolume / 20);
        setSFXSliderSO.RaiseEvent(value);
    }

    public void SetMusicSlider()
    {
        audioMixer.GetFloat("Music", out float musicVolume);
        float value = Mathf.Pow(10, musicVolume / 20);
        setBGMSliderSO.RaiseEvent(value);
    }

    public float GetSFXValueForSlider()
    {
        audioMixer.GetFloat("SFX", out float musicVolume);
        return Mathf.Pow(10, musicVolume / 20);
    }

    public float GetBGMValueForSlider()
    {
        audioMixer.GetFloat("Music", out float musicVolume);
        return Mathf.Pow(10, musicVolume / 20);
    }

    public AudioMixer GetAudioMixer() => audioMixer;

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(SFXID id)
    {
        if (sfxMap.TryGetValue(id, out var clip))
        {
            PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning($"SFX ID {id} not found.");
        }
    }

    public void SetBGMVolume(float volume) => bgmSource.volume = volume;
    public void SetSFXVolume(float volume) => sfxSource.volume = volume;

    public void PlayClickButtonSFX() => PlaySFX(SFXID.ButtonClick);
    public void PlaySelectUpgradeSFX() => PlaySFX(SFXID.SelectUpgrade);

    public void StartGame()
    {
        StopBGM();
        PlayBGM(inGameMusic);
    }

    public void PlayEndGameSoundSFX(bool result)
    {
        StopBGM();
        PlaySFX(result ? SFXID.Success : SFXID.Failure);
    }

    public void ExitGameMode()
    {
        StopBGM();
        PlayBGM(bgm);
    }

    public void SetAudioSliders()
    {
        SetSFXSlider();
        SetMusicSlider();
    }
}