using System.Collections;
using UnityEngine;

public class Boss360Pattern : MonoBehaviour
{
    [Header("탄환 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float spawnOffset = 1.5f;

    [Header("발사 설정")]
    [SerializeField] private int bulletCount = 12;
    [SerializeField] private float shootInterval = 0.1f;
    [SerializeField] private float secondShotRotation = 10f;
    [SerializeField] private AudioClip fireSound;

    [Header("속도 단계 설정")]
    [SerializeField] private float initialSpeed = 2f;
    [SerializeField] private float burstSpeed = 6f;
    [SerializeField] private float finalSpeed = 3f;

    [Header("속도 변화 타이밍")]
    [SerializeField] private float accelerationTime = 0.3f;
    [SerializeField] private float burstDuration = 0.5f;
    [SerializeField] private float decelerationTime = 0.4f;

    [Header("랜덤 무빙샷")]
    [SerializeField] private bool useRandomMovement = false;
    [SerializeField] private Vector2 moveAreaMin = new Vector2(-6, 2);
    [SerializeField] private Vector2 moveAreaMax = new Vector2(6, 6);
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.1f;
    [SerializeField] private int shotsPerPosition = 2;
    [SerializeField] private float positionChangeInterval = 2f;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Coroutine shootCoroutine;
    private Coroutine moveCoroutine;

    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        StopAllPatternCoroutines();
        if (useRandomMovement)
        {
            moveCoroutine = StartCoroutine(RandomMoveAndShoot());
        }
        else
        {
            shootCoroutine = StartCoroutine(ContinuousShoot());
        }
    }

    private void OnDisable()
    {
        StopAllPatternCoroutines();

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }
    }
    private void StopAllPatternCoroutines()
    {
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }

    private IEnumerator ContinuousShoot()
    {
        while (enabled)
        {
            Shoot360(0f);
            PlayFireSound();
            yield return new WaitForSeconds(shootInterval);

            Shoot360(secondShotRotation);
            PlayFireSound();
            yield return new WaitForSeconds(shootInterval);
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

            yield return StartCoroutine(MoveToPosition(randomPos));

            for (int i = 0; i < shotsPerPosition; i++)
            {
                if (!enabled) yield break; 

                Shoot360(0f);
                PlayFireSound();
                yield return new WaitForSeconds(shootInterval);

                if (!enabled) yield break;

                Shoot360(secondShotRotation);
                PlayFireSound();
                yield return new WaitForSeconds(shootInterval);
            }

            yield return new WaitForSeconds(positionChangeInterval);
        }
    }

    private void PlayFireSound()
    {
        if (fireSound != null)
        {
            SoundManager.Instance.PlaySFX(fireSound);
        }
    }

    private IEnumerator MoveToPosition(Vector2 target)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = target.x < transform.position.x;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }

        while (enabled && Vector2.Distance(transform.position, target) > arrivalThreshold)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (enabled)
        {
            transform.position = target;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void Shoot360(float angleOffset)
    {
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep + angleOffset;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;

            SpawnBullet(direction);
        }
    }

    private void SpawnBullet(Vector2 direction)
    {
        if (bulletPrefab == null) return;

        Vector2 spawnPos = (Vector2)transform.position + direction * spawnOffset;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        FairyBullet bullet = bulletObj.GetComponent<FairyBullet>();
        if (bullet != null)
        {
            bullet.SetDirection(direction);
            bullet.SetSpeed(initialSpeed);
            StartCoroutine(ControlBulletSpeed(bullet));
        }
    }

    private IEnumerator ControlBulletSpeed(FairyBullet bullet)
    {
        if (bullet == null) yield break;
        float t = 0f;
        while (t < accelerationTime && bullet != null)
        {
            float speed = Mathf.Lerp(initialSpeed, burstSpeed, t / accelerationTime);
            bullet.SetSpeed(speed);
            t += Time.deltaTime;
            yield return null;
        }

        if (bullet == null) yield break;
        bullet.SetSpeed(burstSpeed);
        yield return new WaitForSeconds(burstDuration);

        if (bullet == null) yield break;
        t = 0f;
        while (t < decelerationTime && bullet != null)
        {
            float speed = Mathf.Lerp(burstSpeed, finalSpeed, t / decelerationTime);
            bullet.SetSpeed(speed);
            t += Time.deltaTime;
            yield return null;
        }

        if (bullet != null)
        {
            bullet.SetSpeed(finalSpeed);
        }
    }

    private IEnumerator DelayedLaunchWithSpeedControl(BeamBullet bullet, Vector2 direction)
    {
        float totalAccelTime = accelerationTime + burstDuration + decelerationTime;
        bullet.SetAcceleratedMovement(direction, initialSpeed, finalSpeed, totalAccelTime, BulletAccelerationCurve.Linear);
        yield return null;
    }
}
