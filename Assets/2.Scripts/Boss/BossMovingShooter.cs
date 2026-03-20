using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMovingShooter : MonoBehaviour
{
    [System.Serializable]
    public class MoveAndShootData
    {
        [Header("이동 설정")]
        public Vector2 targetPosition;
        public float moveSpeed = 3f;
        public float arrivalThreshold = 0.1f;

        [Header("발사 설정")]
        public int shotsAtThisPosition = 2;
        public float shotInterval = 0.5f;
        public PatternDataManager patternData;
    }

    [Header("이동/발사 시퀀스")]
    [SerializeField] private List<MoveAndShootData> moveShootSequence;
    [SerializeField] private bool loopSequence = true;

    [Header("랜덤 이동 모드 (시퀀스 대신)")]
    [SerializeField] private bool useRandomMovement = false;
    [SerializeField] private Vector2 moveAreaMin = new Vector2(-6, 2);
    [SerializeField] private Vector2 moveAreaMax = new Vector2(6, 6);
    [SerializeField] private int randomShotsPerPosition = 2;
    [SerializeField] private float randomShotInterval = 0.5f;
    [SerializeField] private PatternDataManager randomPatternData;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingParameter = "isMoving";

    [Header("스프라이트 플립")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("기본 원형 패턴 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletsPerShot = 12;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float spawnOffset = 1.5f;
    [SerializeField] private float freezeTime = 0.2f;

    private Coroutine currentPatternCoroutine;

    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        StopAllPatternCoroutines();
        StartPattern();

        Debug.Log($"[BossMovingShooter] 패턴 시작 (랜덤 이동: {useRandomMovement})");
    }

    private void OnDisable()
    {
        Debug.Log("[BossMovingShooter] 패턴 정지");
        StopAllPatternCoroutines();

        if (animator != null)
        {
            animator.SetBool(isMovingParameter, false);
        }
    }
    private void StopAllPatternCoroutines()
    {
        if (currentPatternCoroutine != null)
        {
            StopCoroutine(currentPatternCoroutine);
            currentPatternCoroutine = null;
        }
    }

    public void StartPattern()
    {
        if (useRandomMovement)
        {
            currentPatternCoroutine = StartCoroutine(RandomMoveAndShoot());
        }
        else
        {
            currentPatternCoroutine = StartCoroutine(SequentialMoveAndShoot());
        }
    }

    private IEnumerator SequentialMoveAndShoot()
    {
        if (moveShootSequence == null || moveShootSequence.Count == 0)
        {
            Debug.LogWarning("moveShootSequence가 비어 있습니다.");
            yield break;
        }

        int index = 0;

        while (enabled)
        {
            MoveAndShootData data = moveShootSequence[index];

            yield return StartCoroutine(MoveToPosition(data.targetPosition, data.moveSpeed, data.arrivalThreshold));

            for (int i = 0; i < data.shotsAtThisPosition; i++)
            {
                if (!enabled) yield break;

                if (data.patternData != null)
                {
                    yield return StartCoroutine(ShootPatternFromData(data.patternData));
                }
                else
                {
                    ShootCirclePattern();
                }

                yield return new WaitForSeconds(data.shotInterval);
            }

            index = (index + 1) % moveShootSequence.Count;

            if (!loopSequence && index == 0)
                break;
        }
    }

    private IEnumerator RandomMoveAndShoot()
    {
        while (enabled)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(moveAreaMin.x, moveAreaMax.x),
                Random.Range(moveAreaMin.y, moveAreaMax.y)
            );

            yield return StartCoroutine(MoveToPosition(randomPos, 3f, 0.1f));

            for (int i = 0; i < randomShotsPerPosition; i++)
            {
                if (!enabled) yield break;

                if (randomPatternData != null)
                {
                    yield return StartCoroutine(ShootPatternFromData(randomPatternData));
                }
                else
                {
                    ShootCirclePattern();
                }

                yield return new WaitForSeconds(randomShotInterval);
            }
        }
    }

    private IEnumerator MoveToPosition(Vector2 target, float speed, float threshold)
    {
        if (spriteRenderer != null)
        {
            if (target.x < transform.position.x)
            {
                spriteRenderer.flipX = true;
            }
            else if (target.x > transform.position.x)
            {
                spriteRenderer.flipX = false;
            }
        }

        if (animator != null)
        {
            animator.SetBool(isMovingParameter, true);
        }

        while (enabled && Vector2.Distance(transform.position, target) > threshold) 
        {
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        if (enabled)
        {
            transform.position = target;
        }

        if (animator != null)
        {
            animator.SetBool(isMovingParameter, false);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void ShootCirclePattern()
    {
        float angleStep = 360f / bulletsPerShot;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, rotation);
            FairyBullet bullet = bulletObj.GetComponent<FairyBullet>();

            if (bullet != null)
            {
                bullet.Freeze();
                StartCoroutine(DelayedLaunch(bullet, dir, bulletSpeed, freezeTime));
            }
        }
    }

    private IEnumerator ShootPatternFromData(PatternDataManager data)
    {
        float baseAngle = 90f;

        if (data.fireSound != null)
        {
            SoundManager.Instance.PlaySFX(data.fireSound);
        }

        for (int set = 0; set < data.setsToShoot; set++)
        {
            if (!enabled) yield break; 

            float angleStep = 360f / data.bulletsPerSet;

            for (int i = 0; i < data.bulletsPerSet; i++)
            {
                float angle = baseAngle + i * angleStep;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = (Vector2)transform.position + dir * data.spawnOffset;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

                GameObject bulletObj = Instantiate(data.bulletPrefab, spawnPos, rotation);
                FairyBullet bullet = bulletObj.GetComponent<FairyBullet>();

                if (bullet != null)
                {
                    bullet.Freeze();
                    StartCoroutine(DelayedLaunchWithControl(bullet, dir, set, data));
                }
            }

            baseAngle -= data.rotationStep;
            yield return new WaitForSeconds(data.shootInterval);
        }
    }

    private IEnumerator DelayedLaunch(FairyBullet bullet, Vector2 dir, float speed, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bullet != null)  
        {
            bullet.Launch(dir, speed);
        }
    }

    private IEnumerator DelayedLaunchWithControl(FairyBullet bullet, Vector2 dir, int setNumber, PatternDataManager data)
    {
        yield return new WaitForSeconds(data.freezeTime);

        if (bullet == null) yield break;  

        float currentInitialSpeed = data.initialSpeed + setNumber * data.speedIncreasePerSet;
        float currentBurstSpeed = data.burstSpeed + setNumber * data.speedIncreasePerSet;
        float currentFinalSpeed = data.finalSpeed + setNumber * data.speedIncreasePerSet;

        bullet.Launch(dir, currentInitialSpeed);
        float t = 0f;
        while (t < data.accelerationTime && bullet != null)
        {
            float speed = Mathf.Lerp(currentInitialSpeed, currentBurstSpeed, t / data.accelerationTime);
            bullet.SetSpeed(speed);
            t += Time.deltaTime;
            yield return null;
        }

        if (bullet == null) yield break;
        bullet.SetSpeed(currentBurstSpeed);
        yield return new WaitForSeconds(data.burstDuration);

        if (bullet == null) yield break;
        t = 0f;
        while (t < data.decelerationTime && bullet != null) 
        {
            float speed = Mathf.Lerp(currentBurstSpeed, currentFinalSpeed, t / data.decelerationTime);
            bullet.SetSpeed(speed);
            t += Time.deltaTime;
            yield return null;
        }

        if (bullet != null)
        {
            bullet.SetSpeed(currentFinalSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (useRandomMovement)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = (moveAreaMin + moveAreaMax) / 2f;
            Vector3 size = new Vector3(moveAreaMax.x - moveAreaMin.x, moveAreaMax.y - moveAreaMin.y, 0);
            Gizmos.DrawWireCube(center, size);
        }

        if (!useRandomMovement && moveShootSequence != null && moveShootSequence.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < moveShootSequence.Count; i++)
            {
                Vector3 pos = moveShootSequence[i].targetPosition;
                Gizmos.DrawWireSphere(pos, 0.5f);
                if (i < moveShootSequence.Count - 1)
                {
                    Gizmos.DrawLine(pos, moveShootSequence[i + 1].targetPosition);
                }
                else if (loopSequence && moveShootSequence.Count > 1)
                {
                    Gizmos.DrawLine(pos, moveShootSequence[0].targetPosition);
                }
            }
        }
    }

    [ContextMenu("패턴 바로 시작")]
    public void DebugStartPattern()
    {
        StartPattern();
    }
}
