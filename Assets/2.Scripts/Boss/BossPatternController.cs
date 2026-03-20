using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatternController : MonoBehaviour
{
    [Header("패턴 순서")]
    [SerializeField] protected List<PatternSlot> patternOrder;
    [SerializeField] protected bool loopPatterns = true;
    [SerializeField] protected List<PatternDataManager> patternTemplates;
    [SerializeField] protected int repeatCount = 10;

    [Header("이동 설정")]
    [SerializeField] protected Vector2 movePosition;
    [SerializeField] protected float moveSpeed = 5f;

    [Header("패턴 딜레이 설정")]
    [SerializeField] protected float delayBeforePattern = 1.5f;

    [Header("애니메이션 설정")]
    [SerializeField] protected Animator animator;

    protected List<PatternDataManager> patternsToRun = new List<PatternDataManager>();
    protected bool isPatternRunning = false;

    protected virtual void Awake()
    {
        InitializePatterns();
        InitializeAnimator();
    }

    protected virtual void InitializePatterns()
    {
        patternsToRun.Clear();
        foreach (var template in patternTemplates)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                patternsToRun.Add(template);
            }
        }
    }

    protected virtual void InitializeAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    protected virtual void Start()
    {
        StartCoroutine(PatternSequence());
    }
    protected virtual IEnumerator PatternSequence()
    {
        if (patternOrder == null || patternOrder.Count == 0)
        {
            yield break;
        }

        isPatternRunning = true;

        yield return StartCoroutine(MoveToPosition());
        yield return new WaitForSeconds(delayBeforePattern);

        int executedCount = 0;
        int index = 0;

        while (loopPatterns || executedCount < repeatCount)
        {
            PatternSlot slot = patternOrder[index];

            if (slot.template != null)
            {
                yield return StartCoroutine(RunPattern(slot.template));
                yield return new WaitForSeconds(slot.template.delayAfterPattern);
            }

            index = (index + 1) % patternOrder.Count;
            executedCount++;
        }

        isPatternRunning = false;
    }
    protected virtual IEnumerator MoveToPosition()
    {
        SetMovingState(true);

        while (Vector2.Distance(transform.position, movePosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, movePosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = movePosition;
        SetMovingState(false);
    }

    protected virtual void SetMovingState(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
        }
    }
    protected virtual IEnumerator RunPattern(PatternDataManager data)
    {
        float baseAngle = GetInitialAngle();

        for (int set = 0; set < data.setsToShoot; set++)
        {
            PlayFireSound(data.fireSound);

            float angleStep = 360f / data.bulletsPerSet;

            for (int i = 0; i < data.bulletsPerSet; i++)
            {
                float angle = baseAngle + i * angleStep;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = (Vector2)transform.position + dir * data.spawnOffset;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

                GameObject bulletObj = Instantiate(data.bulletPrefab, spawnPos, rotation);
                FairyBullet bullet = bulletObj.GetComponent<FairyBullet>();

                bullet.Freeze();
                StartCoroutine(DelayedLaunchWithControl(bullet, dir, set, data));
            }

            baseAngle -= data.rotationStep;
            yield return new WaitForSeconds(data.shootInterval);
        }
    }

    protected virtual float GetInitialAngle()
    {
        return 90f;
    }

    protected virtual void PlayFireSound(AudioClip fireSound)
    {
        if (fireSound != null)
        {
            SoundManager.Instance.PlaySFX(fireSound);
        }
    }
    protected virtual IEnumerator DelayedLaunchWithControl(FairyBullet bullet, Vector2 dir, int setNumber, PatternDataManager data)
    {
        yield return new WaitForSeconds(data.freezeTime);

        float currentInitialSpeed = data.initialSpeed + setNumber * data.speedIncreasePerSet;
        float currentBurstSpeed = data.burstSpeed + setNumber * data.speedIncreasePerSet;
        float currentFinalSpeed = data.finalSpeed + setNumber * data.speedIncreasePerSet;

        bullet.Launch(dir, currentInitialSpeed);
        float t = 0f;
        while (t < data.accelerationTime)
        {
            bullet.SetSpeed(Mathf.Lerp(currentInitialSpeed, currentBurstSpeed, t / data.accelerationTime));
            t += Time.deltaTime;
            yield return null;
        }
        bullet.SetSpeed(currentBurstSpeed);
        yield return new WaitForSeconds(data.burstDuration);
        t = 0f;
        while (t < data.decelerationTime)
        {
            bullet.SetSpeed(Mathf.Lerp(currentBurstSpeed, currentFinalSpeed, t / data.decelerationTime));
            t += Time.deltaTime;
            yield return null;
        }

        bullet.SetSpeed(currentFinalSpeed);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(movePosition, 0.3f);
        Gizmos.DrawWireCube(movePosition, Vector3.one * 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, movePosition);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, movePosition);
    }
}
