using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhasePattern
{
    [Tooltip("실행할 패턴 스크립트")]
    public MonoBehaviour pattern;

    [Tooltip("이 패턴이 시작되기까지의 딜레이 (초)")]
    public float startDelay = 0f;
}

[System.Serializable]
public class BossPhase
{
    [Tooltip("이 페이즈로 진입하는 체력 퍼센트 (예: 0.7 = 체력 70% 이하)")]
    [Range(0f, 1f)]
    public float triggerHPPercent = 0.7f;

    [Tooltip("이 페이즈에서 실행할 패턴들 (딜레이 설정 가능)")]
    public PhasePattern[] phasePatterns;

    [Tooltip("이 페이즈의 지속 시간 (초). 0이면 상시 유지)")]
    public float duration = 5f;

    [Tooltip("이 페이즈 종료 후 기본 패턴 복귀 여부")]
    public bool returnToDefault = true;

    [HideInInspector] public bool hasExecuted = false;
}

public class BossPhaseManager : MonoBehaviour
{
    [Header("보스 참조")]
    [SerializeField] private Boss boss;

    [Header("상시 기본 패턴 (페이즈 외 실행됨)")]
    [SerializeField] private MonoBehaviour[] defaultPatterns;

    [Header("페이즈 설정 (체력이 높은 순서로 배치)")]
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();

    [Header("특정 체력 이하일 때 변경할 상시 패턴 (선택 사항)")]
    [SerializeField] private float phaseChangeHPPercent = 0.4f;
    [SerializeField] private MonoBehaviour[] finalPhaseDefaultPatterns;

    private int currentPhaseIndex = -1;
    private bool isTransitioning = false;
    private Coroutine phaseCoroutine = null;
    private bool isInitialized = false;
    private bool hasSwitchedToFinalPattern = false;

    [Header("자동 체크 설정")]
    [SerializeField] private bool autoCheckEveryFrame = true;
    [SerializeField] private float checkInterval = 0.05f;
    private float lastCheckTime = 0f;

    private void Start()
    {
    }

    private void Update()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (playerHealth.IsRespawning || playerHealth.GetLives() <= 0)
            {
                return;
            }
        }
        if (autoCheckEveryFrame && isInitialized)
        {
            if (Time.time - lastCheckTime >= checkInterval)
            {
                CheckPhaseTransition();
                lastCheckTime = Time.time;
            }
        }
    }

    public void Initialize()
    {
        if (isInitialized) return;

        if (boss == null)
        {
            boss = GetComponent<Boss>();
        }

        phases.Sort((a, b) => b.triggerHPPercent.CompareTo(a.triggerHPPercent));

        SetPatternsEnabled(defaultPatterns, false);
        for (int i = 0; i < phases.Count; i++)
        {
            SetPhasePatternsEnabled(phases[i].phasePatterns, false);
            phases[i].hasExecuted = false;
        }

        SetPatternsEnabled(defaultPatterns, true);

        isInitialized = true;
    }

    public void CheckPhaseTransition()
    {
        if (boss == null || isTransitioning) return;

        float hpPercent = (float)boss.CurrentHP / boss.MaxHP;
        if (currentPhaseIndex == -1 && hpPercent <= phaseChangeHPPercent && !hasSwitchedToFinalPattern)
        {
            SwitchToFinalPattern();
        }
        for (int i = 0; i < phases.Count; i++)
        {
            if (!phases[i].hasExecuted && hpPercent <= phases[i].triggerHPPercent)
            {
                if (phaseCoroutine != null)
                {
                    StopCoroutine(phaseCoroutine);
                }
                phaseCoroutine = StartCoroutine(ActivatePhase(i));
                return;
            }
        }
    }

    private void SwitchToFinalPattern()
    {
        if (finalPhaseDefaultPatterns == null || finalPhaseDefaultPatterns.Length == 0)
            return;

        hasSwitchedToFinalPattern = true;
        SetPatternsEnabled(defaultPatterns, false);
        SetPatternsEnabled(finalPhaseDefaultPatterns, true);
    }

    private IEnumerator ActivatePhase(int index)
    {
        isTransitioning = true;
        currentPhaseIndex = index;
        BossPhase phase = phases[index];
        phase.hasExecuted = true;
        SetPatternsEnabled(defaultPatterns, false);
        if (hasSwitchedToFinalPattern)
        {
            SetPatternsEnabled(finalPhaseDefaultPatterns, false);
        }
        for (int i = 0; i < phases.Count; i++)
        {
            SetPhasePatternsEnabled(phases[i].phasePatterns, false);
        }
        yield return null;

        isTransitioning = false;
        foreach (var phasePattern in phase.phasePatterns)
        {
            if (phasePattern.pattern != null)
            {
                if (phasePattern.startDelay > 0)
                {
                    yield return new WaitForSeconds(phasePattern.startDelay);
                }

                phasePattern.pattern.enabled = false;
                yield return null;
                phasePattern.pattern.enabled = true;
            }
        }
        if (phase.duration > 0)
        {
            yield return new WaitForSeconds(phase.duration);
            SetPhasePatternsEnabled(phase.phasePatterns, false);

            if (phase.returnToDefault)
            {
                currentPhaseIndex = -1;

                float currentHP = (float)boss.CurrentHP / boss.MaxHP;
                if (currentHP <= phaseChangeHPPercent)
                {
                    if (!hasSwitchedToFinalPattern)
                    {
                        SwitchToFinalPattern();
                    }
                    else
                    {
                        SetPatternsEnabled(finalPhaseDefaultPatterns, true);
                    }
                }
                else
                {
                    SetPatternsEnabled(defaultPatterns, true);
                }
            }
            else
            {
                currentPhaseIndex = -1;
            }
        }

        isTransitioning = false;
    }

    private void SetPatternsEnabled(MonoBehaviour[] patterns, bool enable)
    {
        if (patterns == null) return;

        foreach (var p in patterns)
        {
            if (p != null)
            {
                p.enabled = enable;
            }
        }
    }

    private void SetPhasePatternsEnabled(PhasePattern[] phasePatterns, bool enable)
    {
        if (phasePatterns == null) return;

        foreach (var pp in phasePatterns)
        {
            if (pp.pattern != null)
            {
                pp.pattern.enabled = enable;
            }
        }
    }
}
