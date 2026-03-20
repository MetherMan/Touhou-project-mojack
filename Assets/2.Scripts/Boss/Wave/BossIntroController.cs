using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BossIntroController : MonoBehaviour
{

    [Header("BGM 설정")]
    [Tooltip("보스 전투 BGM")]
    public AudioClip bossBattleBGM;

    [Tooltip("BGM 페이드 시간")]
    public float bgmFadeTime = 1f;

    [Header("등장 연출 설정")]
    [Tooltip("등장 시작 위치 (보통 화면 위쪽)")]
    public Vector2 spawnPosition = new Vector2(0f, 8f);

    [Tooltip("이동할 목표 위치 (대화 위치)")]
    public Vector2 dialoguePosition = new Vector2(0f, 3f);

    [Tooltip("이동 속도")]
    public float moveSpeed = 2f;

    [Tooltip("이동 커브 (부드러운 감속)")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("등장 이펙트")]
    [Tooltip("등장 이펙트 프리팹")]
    public GameObject spawnEffect;

    [Tooltip("등장 사운드")]
    public AudioClip spawnSound;

    [Tooltip("등장 시 페이드인 효과 사용")]
    public bool useFadeIn = true;

    [Tooltip("페이드인 시간")]
    public float fadeInDuration = 0.5f;

    [Header("대화 설정")]
    [Tooltip("대화 UI 프리팹")]
    public GameObject dialogueUI;
    [Tooltip("대화 매니저")]
    public DialogueManager dialogueManager;

    [Tooltip("대화 데이터 에셋")]
    public ScriptableObject dialogueData;

    [Tooltip("대화 시작 전 대기 시간")]
    public float dialogueDelay = 0.5f;

    [Header("전투 시작 설정")]
    [Tooltip("전투 시작 전 대기 시간")]
    public float battleStartDelay = 0.3f;

    [Tooltip("전투 시작 사운드")]
    public AudioClip battleStartSound;

    [Tooltip("전투 시작 이펙트")]
    public GameObject battleStartEffect;

    [Header("이벤트")]
    public UnityEvent OnIntroStart;
    public UnityEvent OnMoveComplete;
    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;
    public UnityEvent OnBattleStart;

    [Header("디버그")]
    [SerializeField] private bool skipIntro = false;
    [SerializeField] private bool skipDialogue = false;

    private SpriteRenderer spriteRenderer;
    private bool introComplete = false;
    private bool battleStarted = false;

    public bool IntroComplete => introComplete;
    public bool BattleStarted => battleStarted;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.position = spawnPosition;
        if (useFadeIn && spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (skipIntro)
        {
            transform.position = dialoguePosition;
            OnBattleStart?.Invoke();
            battleStarted = true;
            introComplete = true;
            yield break;
        }
        DisablePlayerForIntro();
        yield return StartCoroutine(SpawnIntro());
        yield return StartCoroutine(MoveToDialoguePosition());
        if (!skipDialogue)
        {
            yield return StartCoroutine(PlayDialogue());
        }
        yield return StartCoroutine(StartBattle());

        introComplete = true;
    }

    private void DisablePlayerForIntro()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        var playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }
    }

    private void EnablePlayerForBattle()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        var playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.enabled = true;
        }
    }

    private IEnumerator SpawnIntro()
    {
        OnIntroStart?.Invoke();
        Debug.Log("[BossIntro] 보스 등장!");
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPosition, Quaternion.identity);
        }
        if (spawnSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(spawnSound);
        }
        if (useFadeIn && spriteRenderer != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
                yield return null;
            }

            Color finalColor = spriteRenderer.color;
            finalColor.a = 1f;
            spriteRenderer.color = finalColor;
        }

        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator MoveToDialoguePosition()
    {
        Debug.Log("[BossIntro] 대화 위치로 이동 중...");

        float distance = Vector2.Distance(transform.position, dialoguePosition);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        Vector2 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = moveCurve.Evaluate(t);

            transform.position = Vector2.Lerp(startPos, dialoguePosition, curveT);
            yield return null;
        }

        transform.position = dialoguePosition;
        OnMoveComplete?.Invoke();
        Debug.Log("[BossIntro] 이동 완료!");
    }

    private IEnumerator PlayDialogue()
    {
        if (dialogueDelay > 0f)
        {
            yield return new WaitForSeconds(dialogueDelay);
        }

        OnDialogueStart?.Invoke();
        Debug.Log("[BossIntro] 대화 시작!");

        var dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null && dialogueData != null)
        {
            dialogueManager.StartDialogue((DialogueData)dialogueData);
            yield return new WaitUntil(() => dialogueManager.IsComplete());
        }
        else
        {
            Debug.LogWarning("[BossIntro] DialogueManager 또는 DialogueData가 없습니다!");
            yield return new WaitForSeconds(3f);
        }

        OnDialogueEnd?.Invoke();
        Debug.Log("[BossIntro] 대화 종료!");
    }

    private IEnumerator StartBattle()
    {
        if (battleStartDelay > 0f)
        {
            yield return new WaitForSeconds(battleStartDelay);
        }

        Debug.Log("[BossIntro] 전투 시작!");
        if (bossBattleBGM != null && BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBossBGM(bossBattleBGM, bgmFadeTime);
            Debug.Log("[BossIntro] 보스 BGM 시작!");
        }
        if (battleStartEffect != null)
        {
            Instantiate(battleStartEffect, transform.position, Quaternion.identity);
        }
        if (battleStartSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(battleStartSound);
        }
        EnableBossPatterns();
        EnablePlayerForBattle();
        OnBattleStart?.Invoke();
        battleStarted = true;

        Debug.Log("[BossIntro] 전투 준비 완료!");
    }

    private void EnableBossPatterns()
    {
        var phaseManager = GetComponent<BossPhaseManager>();
        if (phaseManager != null)
        {
            Debug.Log("[BossIntro] BossPhaseManager 초기화 및 시작");
            phaseManager.Initialize();
            return;
        }
        var patterns = GetComponents<MonoBehaviour>();
        foreach (var pattern in patterns)
        {
            if (pattern != this &&
                !(pattern is BossPhaseManager) &&
                pattern.GetType().Name.Contains("Pattern"))
            {
                pattern.enabled = true;
                Debug.Log($"[BossIntro] 패턴 활성화: {pattern.GetType().Name}");
            }
        }
    }
    public void OnDialogueComplete()
    {
        Debug.Log("[BossIntro] 대화 완료 신호 수신");
    }
    [ContextMenu("인트로 건너뛰기")]
    public void SkipIntro()
    {
        StopAllCoroutines();
        transform.position = dialoguePosition;
        OnBattleStart?.Invoke();
        battleStarted = true;
        introComplete = true;
        EnableBossPatterns();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPosition, 0.5f);
        Gizmos.DrawIcon(spawnPosition, "sv_icon_dot0_pix16_gizmo", true);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(dialoguePosition, 0.5f);
        Gizmos.DrawIcon(dialoguePosition, "sv_icon_dot3_pix16_gizmo", true);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(spawnPosition, dialoguePosition);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.Label(spawnPosition + Vector2.up * 0.7f, "등장 위치");

        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.Label(dialoguePosition + Vector2.up * 0.7f, "대화 위치");
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        int segments = 10;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            Vector2 pos = Vector2.Lerp(spawnPosition, dialoguePosition, moveCurve.Evaluate(t));
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
    }
}
