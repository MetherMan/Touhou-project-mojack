using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("오디오 소스")]
    private AudioSource sfxSource;
    private AudioSource bgmSource;

    [Header("BGM 설정")]
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.playOnAwake = false;
        sfxSource.volume = defaultSFXVolume;
        sfxSource.loop = false;
        sfxSource.priority = 128;
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.volume = defaultBGMVolume;
        bgmSource.loop = true;
        bgmSource.priority = 0;
    }
    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] 오디오 클립이 비어 있습니다.");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("[SoundManager] SFX 오디오 소스가 없습니다.");
            return;
        }

        sfxSource.PlayOneShot(clip, defaultSFXVolume);
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySound(clip);
    }
    public void PlayBGM(AudioClip bgm)
    {
        if (bgm == null)
        {
            Debug.LogWarning("[SoundManager] BGM 오디오 클립이 비어 있습니다.");
            return;
        }

        if (bgmSource == null)
        {
            Debug.LogError("[SoundManager] BGM 오디오 소스가 없습니다.");
            return;
        }
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
        bgmSource.clip = bgm;
        bgmSource.volume = defaultBGMVolume;
        bgmSource.Play();

        Debug.Log($"[SoundManager] BGM 재생 시작: {bgm.name}");
    }
    public void PlayBGM(AudioClip bgm, float fadeTime)
    {
        if (bgm == null)
        {
            Debug.LogWarning("[SoundManager] BGM 오디오 클립이 비어 있습니다.");
            return;
        }

        if (bgmSource == null)
        {
            Debug.LogError("[SoundManager] BGM 오디오 소스가 없습니다.");
            return;
        }

        StartCoroutine(CrossfadeBGM(bgm, fadeTime));
    }
    private IEnumerator CrossfadeBGM(AudioClip newBGM, float fadeTime)
    {
        float startVolume = bgmSource.volume;
        if (bgmSource.isPlaying)
        {
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }
        }
        bgmSource.Stop();
        bgmSource.clip = newBGM;
        bgmSource.volume = 0f;
        bgmSource.Play();
        float elapsed2 = 0f;
        while (elapsed2 < fadeTime)
        {
            elapsed2 += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, defaultBGMVolume, elapsed2 / fadeTime);
            yield return null;
        }

        bgmSource.volume = defaultBGMVolume;
        Debug.Log($"[SoundManager] BGM 페이드 완료: {newBGM.name}");
    }
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            Debug.Log("[SoundManager] BGM 정지");
        }
    }
    public void StopBGM(float fadeTime)
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutBGM(fadeTime));
        }
    }

    private IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = defaultBGMVolume;
        Debug.Log("[SoundManager] BGM 페이드 아웃 완료");
    }
    public void SetBGMVolume(float volume)
    {
        defaultBGMVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = defaultBGMVolume;
        }
        Debug.Log($"[SoundManager] BGM 볼륨: {defaultBGMVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        defaultSFXVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = defaultSFXVolume;
        }
        Debug.Log($"[SoundManager] SFX 볼륨: {defaultSFXVolume}");
    }
    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
    public AudioClip GetCurrentBGM()
    {
        return bgmSource != null ? bgmSource.clip : null;
    }
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
            Debug.Log("[SoundManager] BGM 일시정지");
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
            Debug.Log("[SoundManager] BGM 재개");
        }
    }
}
