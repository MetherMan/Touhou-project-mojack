using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("사운드 관련")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip respawnSound;
    [SerializeField] private AudioClip gameOverBGM;

    [Header("플레이어 목숨/리스폰 설정")]
    [SerializeField] private int lives = 3;
    [SerializeField] private float respawnDelay = 0.3f;
    [SerializeField] private Vector3 respawnPos = new Vector3(-2.5f, -4.0f, 0);
    [SerializeField] private float moveUpTime = 0.5f;
    [SerializeField] private float invincibleTime = 1f;

    [Header("게임오버 설정")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private float bgmFadeTime = 1f;

    private bool isRespawning = false;
    private bool isDead = false;
    private Collider2D col;
    private SpriteRenderer sr;
    private PlayerController controller;
    private PlayerShooting shooting;
    public bool IsRespawning => isRespawning;
    public bool IsDead => isDead;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        controller = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
        shooting = GetComponent<PlayerShooting>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isRespawning && !isDead && collision.CompareTag("EnemyBullet"))
        {
            SoundManager.Instance.PlaySound(hitSound);
            FairyBullet bullet = collision.GetComponent<FairyBullet>();
            if (bullet != null && bullet.HitEffectPrefab != null)
            {
                Instantiate(bullet.HitEffectPrefab, collision.transform.position, Quaternion.identity);
            }
            Die();
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isRespawning && !isDead)
        {
            SoundManager.Instance.PlaySound(hitSound);
            Die();
        }
    }

    void Die()
    {
        lives--;
        SoundManager.Instance.PlaySound(deathSound);

        if (shooting != null)
        {
            shooting.ClearAllProjectiles();
        }

        if (lives <= 0)
        {
            isDead = true;
            StartCoroutine(GameOver());
        }
        else
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        isRespawning = true;
        col.enabled = false;
        controller.enabled = false;
        if (shooting != null) shooting.enabled = false;

        sr.enabled = false;
        transform.position = respawnPos;
        yield return new WaitForSeconds(respawnDelay);

        SoundManager.Instance.PlaySound(respawnSound);
        StartCoroutine(Blink(invincibleTime + moveUpTime));

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 1.5f, 0);

        controller.enabled = true;
        if (shooting != null) shooting.enabled = true;

        while (elapsed < moveUpTime)
        {
            float t = elapsed / moveUpTime;
            float smoothT = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        yield return new WaitForSeconds(invincibleTime);

        col.enabled = true;
        isRespawning = false;
    }

    IEnumerator Blink(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        sr.enabled = true;
    }

    IEnumerator GameOver()
    {

        col.enabled = false;
        controller.enabled = false;
        if (shooting != null) shooting.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SaveFinalScore();
        }

        if (gameOverBGM != null && BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBossBGM(gameOverBGM, bgmFadeTime);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM(bgmFadeTime);
        }

        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(gameOverDelay);

        if (SceneManager.GetSceneByName(gameOverSceneName).IsValid() ||
            Application.CanStreamedLevelBeLoaded(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        
    }

    public int GetLives()
    {
        return lives;
    }

    public void AddLife(int amount = 1)
    {
        lives += amount;
    }
}
