using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Boss : MonoBehaviour
{
    [SerializeField] private int maxHP = 300;
    private int currentHP;

    [Header("보스 사망 설정")]
    [SerializeField] private string gameClearSceneName = "GameClear";
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathDelay = 2f;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    private BossPhaseManager phaseManager;
    private bool isIntroControlled = false;
    private bool isDead = false;

    private void Awake()
    {
        var introController = GetComponent<BossIntroController>();
        isIntroControlled = (introController != null);
        phaseManager = GetComponent<BossPhaseManager>();
    }

    private void Start()
    {
        currentHP = maxHP;
        if (!isIntroControlled && phaseManager != null)
        {
            phaseManager.Initialize();
        }

        if (BossHPBar.Instance != null)
        {
            BossHPBar.Instance.ShowHPBar(true);
            BossHPBar.Instance.UpdateHP(currentHP, maxHP);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        if (BossHPBar.Instance != null)
        {
            BossHPBar.Instance.UpdateHP(currentHP, maxHP);
        }
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsRespawning)
            {
                return;
            }
        }
        if (phaseManager != null)
        {
            phaseManager.CheckPhaseTransition();
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (BossHPBar.Instance != null)
        {
            BossHPBar.Instance.ShowHPBar(false);
        }
        if (deathSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(deathSound);
        }
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.StopBGM(deathDelay * 0.8f);
        }
        StartCoroutine(LoadGameClearScene());
    }

    private IEnumerator LoadGameClearScene()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(gameClearSceneName);
    }
}
