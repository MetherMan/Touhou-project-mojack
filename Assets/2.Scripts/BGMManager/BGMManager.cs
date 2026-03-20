using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [SerializeField] private AudioClip menuBGM;
    [SerializeField] private AudioClip gameBGM;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource bgmSource;
    private bool isBossBGMPlaying = false;
    private AudioClip cachedSceneBGM;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayBGMForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene" || scene.name == "MainMenu" || scene.name == "Title")
        {
            isBossBGMPlaying = false;
        }
        if (!isBossBGMPlaying)
        {
            PlayBGMForCurrentScene();
        }
    }

    private void PlayBGMForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AudioClip targetClip = null;

        if (sceneName == "MainMenu" || sceneName == "Title")
        {
            targetClip = menuBGM;
        }
        else if (sceneName == "GameScene")
        {
            targetClip = gameBGM;
        }

        if (targetClip != null)
        {
            cachedSceneBGM = targetClip;
            StartCoroutine(FadeAndPlayBGM(targetClip));
        }
    }

    private IEnumerator FadeAndPlayBGM(AudioClip newClip)
    {
        if (bgmSource.clip == newClip && bgmSource.isPlaying)
        {
            yield break;
        }

        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        bgmSource.clip = newClip;
        bgmSource.volume = 0;
        bgmSource.Play();

        float timer2 = 0;
        while (timer2 < fadeDuration)
        {
            timer2 += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0, 1, timer2 / fadeDuration);
            yield return null;
        }
    }
    public void PlayBossBGM(AudioClip bossBGM, float fadeTime = -1)
    {
        if (fadeTime < 0) fadeTime = fadeDuration;

        isBossBGMPlaying = true;
        StartCoroutine(FadeAndPlayBGMWithDuration(bossBGM, fadeTime));
    }
    public void StopBossBGM(float fadeTime = -1)
    {
        if (fadeTime < 0) fadeTime = fadeDuration;

        isBossBGMPlaying = false;

        if (cachedSceneBGM != null)
        {
            StartCoroutine(FadeAndPlayBGMWithDuration(cachedSceneBGM, fadeTime));
        }
    }

    private IEnumerator FadeAndPlayBGMWithDuration(AudioClip newClip, float customFadeDuration)
    {
        if (bgmSource.clip == newClip && bgmSource.isPlaying)
        {
            yield break;
        }

        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float timer = 0;
            while (timer < customFadeDuration)
            {
                timer += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / customFadeDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        bgmSource.clip = newClip;
        bgmSource.volume = 0;
        bgmSource.Play();

        float timer2 = 0;
        while (timer2 < customFadeDuration)
        {
            timer2 += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0, 1, timer2 / customFadeDuration);
            yield return null;
        }
    }
    public void StopBGM(float fadeTime = -1)
    {
        if (fadeTime < 0) fadeTime = fadeDuration;
        StartCoroutine(FadeOutBGM(fadeTime));
    }

    private IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = bgmSource.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
