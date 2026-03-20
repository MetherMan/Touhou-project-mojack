using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BeamPatternType
{
    Rotating240,
    SingleRotatingBeam
}

[System.Serializable]
public class BeamPatternSlot
{
    [Header("타이밍 설정")]
    public float startDelay = 0f;

    [Header("패턴 설정")]
    public BeamPatternType patternType;
    public BeamPatternData patternData;
    [Min(1)] public int repeatCount = 1;
}
public class FlandreBeamControllerBase : MonoBehaviour
{
    private List<GameObject> activeBeams = new List<GameObject>();
    private List<GameObject> activeBullets = new List<GameObject>();
    [Header("이동 설정")]
    [SerializeField] private Vector2 targetPosition = new Vector2(0f, 3f);
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    [Header("애니메이터")]
    [SerializeField] private Animator animator;

    [Header("빔 패턴 목록")]
    [SerializeField] private List<BeamPatternSlot> beamPatterns;
    [SerializeField] private bool loopPatterns = false;

    [Header("탄환 생성 설정")]
    [Tooltip("각 위치에서 생성할 탄환 개수 (좌우 대칭)")]
    [Min(1)]
    public int bulletsPerSpawn = 2;

    [Tooltip("동시에 생성되는 탄환 사이의 간격")]
    public float bulletSpacing = 0.4f;

    [Header("탄환 속도 설정")]
    [Tooltip("탄환 초기 속도")]
    public float bulletStartSpeed = 2f;

    [Tooltip("탄환 최종 속도")]
    public float bulletMaxSpeed = 8f;

    [Tooltip("최종 속도에 도달하는 시간 (초)")]
    public float accelerationTime = 2f;

    [Tooltip("가속 커브 타입")]
    public BulletAccelerationCurve accelerationCurve = BulletAccelerationCurve.EaseInOut;

    [Header("사운드 설정")]
    [Tooltip("빔 발사 사운드")]
    public AudioClip beamFireSound;
    [Tooltip("회전 빔 시작 사운드")]
    public AudioClip rotatingBeamStartSound;
    [Tooltip("탄환 발사 사운드")]
    public AudioClip bulletSpawnSound;

    [Header("사운드 쿨다운 설정")]
    [Tooltip("빔 발사 사운드 쿨다운 (초)")]
    public float beamSoundCooldown = 0.1f;
    [Tooltip("탄환 발사 사운드 쿨다운 (초)")]
    public float bulletSoundCooldown = 0.05f;

    private bool hasArrived = false;
    private float lastBeamSoundTime = -999f;
    private float lastBulletSoundTime = -999f;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        StartCoroutine(MoveToPositionAndStartPattern());
    }

    private IEnumerator MoveToPositionAndStartPattern()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }

        while (Vector2.Distance(transform.position, targetPosition) > arrivalThreshold)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
        hasArrived = true;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(BeamPatternSequence());
    }
    private IEnumerator BeamPatternSequence()
    {
        if (beamPatterns == null || beamPatterns.Count == 0)
        {
            yield break;
        }

        do
        {
            yield return null;
            float sequenceStartTime = Time.time;

            for (int i = 0; i < beamPatterns.Count; i++)
            {
                var slot = beamPatterns[i];
                if (slot?.patternData != null)
                {
                    StartCoroutine(ExecutePattern(slot, sequenceStartTime, i));
                }
            }

            float maxDuration = CalculateMaxPatternDuration();
            yield return new WaitForSeconds(maxDuration);

        } while (loopPatterns);
    }
    private float CalculateMaxPatternDuration()
    {
        float maxDuration = 0f;

        foreach (var slot in beamPatterns)
        {
            if (slot?.patternData == null) continue;

            float patternDuration = slot.startDelay;

            patternDuration += slot.patternData.shootInterval * slot.patternData.beamCount;
            patternDuration += slot.patternData.delayAfterPattern;
            patternDuration += slot.patternData.bulletSpawnDelay;
            patternDuration += 1f;

            patternDuration *= slot.repeatCount;

            if (patternDuration > maxDuration)
            {
                maxDuration = patternDuration;
            }
        }

        return maxDuration;
    }

    private IEnumerator ExecutePattern(BeamPatternSlot slot, float sequenceStartTime, int index)
    {
        float targetStartTime = sequenceStartTime + slot.startDelay;

        while (Time.time < targetStartTime)
        {
            yield return null;
        }

        for (int r = 0; r < slot.repeatCount; r++)
        {
            switch (slot.patternType)
            {
                case BeamPatternType.Rotating240:
                    yield return ExecuteRotatingBeam(slot.patternData);
                    break;
                case BeamPatternType.SingleRotatingBeam:
                    yield return ExecuteSingleRotatingBeam(slot.patternData);
                    break;
            }

            if (slot.patternData.delayAfterPattern > 0)
            {
                yield return new WaitForSeconds(slot.patternData.delayAfterPattern);
            }
        }
    }

    private IEnumerator ExecuteRotatingBeam(BeamPatternData data)
    {
        if (rotatingBeamStartSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(rotatingBeamStartSound);
        }

        Vector2 bossCenter = transform.position;
        float patternStartTime = Time.time;

        float actualShootInterval = data.shootInterval;
        if (data.syncWithRotationSpeed && data.beamCount > 1)
        {
            float totalAngle = Mathf.Abs(data.endAngle - data.startAngle);
            float rotationDuration = totalAngle / data.rotationSpeed;
            actualShootInterval = rotationDuration / (data.beamCount - 1);
        }

        for (int i = 0; i < data.beamCount; i++)
        {
            float targetTime = patternStartTime + (actualShootInterval * i);
            while (Time.time < targetTime)
            {
                yield return null;
            }

            float currentAngle = data.beamCount > 1
                ? Mathf.Lerp(data.startAngle, data.endAngle, (float)i / (data.beamCount - 1))
                : (data.startAngle + data.endAngle) / 2f;

            FireBeam(data, currentAngle, bossCenter);
        }
    }
private IEnumerator ExecuteSingleRotatingBeam(BeamPatternData data)
    {
        if (rotatingBeamStartSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(rotatingBeamStartSound);
        }

        if (data.beamPrefab == null)
        {
            yield break;
        }

        Vector2 bossCenter = transform.position;
        Vector2 dir = new Vector2(Mathf.Cos(data.startAngle * Mathf.Deg2Rad), Mathf.Sin(data.startAngle * Mathf.Deg2Rad));
        Vector2 spawnPos = bossCenter + dir * data.spawnOffset;

        Quaternion rotation = Quaternion.Euler(0f, 0f, data.startAngle - 90f);
        GameObject beam = Instantiate(data.beamPrefab, spawnPos, rotation);

        activeBeams.Add(beam);

        if (beam == null)
        {
            yield break;
        }

        FlandreBeam beamScript = beam.GetComponent<FlandreBeam>();
        if (beamScript != null)
        {
            beamScript.DisableAutoDestroy();
        }

        Animator beamAnimator = null;
        if (data.disableAnimationDuringRotation)
        {
            beamAnimator = beam.GetComponent<Animator>();
            if (beamAnimator != null)
            {
                beamAnimator.enabled = false;
            }
        }

        float currentAngle = data.startAngle;
        float totalAngle = Mathf.Abs(data.endAngle - data.startAngle);

        if (data.rotationSpeed <= 0)
        {
            Destroy(beam);
            yield break;
        }

        float rotationDuration = totalAngle / data.rotationSpeed;
        float elapsed = 0f;

        while (elapsed < rotationDuration && beam != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            currentAngle = Mathf.Lerp(data.startAngle, data.endAngle, t);

            dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
            spawnPos = bossCenter + dir * data.spawnOffset;

            beam.transform.position = spawnPos;
            beam.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle - 90f);

            yield return null;
        }

        if (beam == null)
        {
            yield break;
        }

        if (beamAnimator != null)
        {
            beamAnimator.enabled = true;
        }

        Destroy(beam, data.beamLifetime);
    }

    private void FireBeam(BeamPatternData data, float angle, Vector2 bossCenter)
    {
        if (Time.time - lastBeamSoundTime >= beamSoundCooldown)
        {
            if (beamFireSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(beamFireSound);
                lastBeamSoundTime = Time.time;
            }
        }

        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        CreateBeamWithBullets(data, dir, bossCenter, angle);
    }
private void CreateBeamWithBullets(BeamPatternData data, Vector2 direction, Vector2 bossCenter, float angle, Vector3? spawnPosOverride = null)
    {
        Vector2 spawnPos = spawnPosOverride.HasValue ? (Vector2)spawnPosOverride.Value : bossCenter + direction * data.spawnOffset;

        if (data.beamPrefab != null)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            GameObject beam = Instantiate(data.beamPrefab, spawnPos, rotation);

            activeBeams.Add(beam);

            if (data.disableAnimationDuringRotation)
            {
                Animator beamAnimator = beam.GetComponent<Animator>();
                if (beamAnimator != null)
                {
                    beamAnimator.enabled = false;
                }
            }

            Destroy(beam, 1f);
        }

        if (data.beamBulletPrefab != null && data.bulletOffsets.Length > 0)
        {
            for (int i = 0; i < data.bulletOffsets.Length; i++)
            {
                float offset = data.bulletOffsets[i];
                Vector2 bulletSpawnPos = spawnPos + direction * offset;
                StartCoroutine(SpawnBulletWithDelay(data, bulletSpawnPos, angle));
            }
        }
    }

    private IEnumerator SpawnBulletWithDelay(BeamPatternData data, Vector2 position, float angle)
    {
        yield return new WaitForSeconds(data.bulletSpawnDelay);

        Vector2 perpendicular = new Vector2(-Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));

        if (bulletsPerSpawn == 1)
        {
            SpawnSingleBullet(data, position, angle);
        }
        else if (bulletsPerSpawn == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                float offset = (i == 0 ? -bulletSpacing / 2f : bulletSpacing / 2f);
                Vector2 bulletPos = position + perpendicular * offset;
                SpawnSingleBullet(data, bulletPos, angle);
            }
        }
        else
        {
            float totalWidth = bulletSpacing * (bulletsPerSpawn - 1);
            for (int i = 0; i < bulletsPerSpawn; i++)
            {
                float offset = -totalWidth / 2f + (bulletSpacing * i);
                Vector2 bulletPos = position + perpendicular * offset;
                SpawnSingleBullet(data, bulletPos, angle);
            }
        }
    }

    private void SpawnSingleBullet(BeamPatternData data, Vector2 position, float angle)
    {
        GameObject bulletObj = Instantiate(data.beamBulletPrefab, position, Quaternion.Euler(0, 0, angle - 90f));

        activeBullets.Add(bulletObj);

        if (Time.time - lastBulletSoundTime >= bulletSoundCooldown)
        {
            if (bulletSpawnSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(bulletSpawnSound);
                lastBulletSoundTime = Time.time;
            }
        }

        BeamBullet bullet = bulletObj.GetComponent<BeamBullet>();
        if (bullet != null)
        {
            Vector2 localRight = bulletObj.transform.right;
            Vector2 moveDir = data.clockwise ? localRight : -localRight;

            bullet.SetAcceleratedMovement(
                moveDir,
                bulletStartSpeed,
                bulletMaxSpeed,
                accelerationTime,
                accelerationCurve
            );
        }
    }

    public bool HasArrived()
    {
        return hasArrived;
    }

    public void SetTargetPosition(Vector2 newTarget)
    {
        targetPosition = newTarget;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
        Gizmos.DrawSphere(targetPosition, 0.1f);

        if (Application.isPlaying && !hasArrived)
        {
            Gizmos.color = Color.cyan;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawLine(transform.position, targetPosition);
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(targetPosition, arrivalThreshold);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        Vector3 start = transform.position;
        Vector3 end = targetPosition;
        int segments = 20;

        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            Vector3 p1 = Vector3.Lerp(start, end, t1);
            Vector3 p2 = Vector3.Lerp(start, end, t2);

            if (i % 2 == 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
    public virtual void ClearAllProjectiles()
    {
        StopAllCoroutines();

        foreach (GameObject beam in activeBeams)
        {
            if (beam != null)
            {
                Destroy(beam);
            }
        }
        activeBeams.Clear();

        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        activeBullets.Clear();
    }
    private void OnDestroy()
    {
        ClearAllProjectiles();
    }
    private void LateUpdate()
    {
        activeBeams.RemoveAll(b => b == null);
        activeBullets.RemoveAll(b => b == null);
    }
}
