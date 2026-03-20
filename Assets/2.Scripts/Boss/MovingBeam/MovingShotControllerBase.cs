using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType { Smooth, Impulse }
public enum MovementCurveType { Linear, EaseIn, EaseOut, EaseInOut, Custom }
public abstract class MovingShotControllerBase : MonoBehaviour
{
    protected virtual void OnBeforeBeamCreate() { }
    protected virtual void OnAfterBeamCreate(GameObject visualBeam) { }
    protected virtual void OnBeforeBeamMove() { }
    protected virtual void OnAfterBeamMove() { }
    protected virtual void OnBeforeBeamDestroy(GameObject visualBeam) { }
    protected virtual void OnShotComplete(bool wasClockwise) { }
    protected virtual void OnPatternComplete() { }
    [Header("표시용 빔 오프셋")]
    public Vector2 visualBeamOffset = new Vector2(0f, -1f);

    [Header("이동 설정")]
    [Tooltip("스윕 시작 위치")]
    public Vector2 startPos = new Vector2(-4f, 3f);
    [Tooltip("스윕 끝 위치")]
    public Vector2 endPos = new Vector2(4f, 2f);
    [Tooltip("시작 위치까지 이동하는 속도")]
    public float moveToStartSpeed = 10f;
    [Tooltip("이동 시작 전 대기 시간")]
    public float moveDelay = 1f;

    [Header("시작 지연")]
    [Tooltip("패턴 시작 전 대기 시간")]
    public float startDelay = 0f;

    [Header("표시용 빔 설정")]
    [Tooltip("빔 프리팹")]
    public GameObject visualBeamPrefab;
    [Tooltip("빔 표시 여부")]
    public bool showVisualBeam = true;
    [Tooltip("빔 발사 각도")]
    public float visualBeamAngle = -90f;
    [Tooltip("빔 회전 오프셋")]
    public float visualBeamRotationOffset = 0f;
    [Tooltip("이동 완료 후 빔 유지 시간")]
    public float beamLingerTime = 0.5f;

    [Header("빔 트레일 설정")]
    [Tooltip("빔 트레일 사용 여부")]
    public bool useBeamTrail = true;
    [Tooltip("이전 빔들 남기는 간격")]
    public float beamTrailInterval = 0.1f;
    [Tooltip("트레일 빔의 알파값")]
    public float trailBeamAlpha = 0.6f;
    [Tooltip("트레일 빔 페이드 시간")]
    public float trailBeamFadeTime = 0.5f;

    [Header("에너지 볼 설정")]
    [Tooltip("에너지 볼 프리팹")]
    public GameObject energyBallPrefab;
    [Tooltip("에너지 볼 사용 여부")]
    public bool useEnergyBall = true;
    [Tooltip("에너지 볼 오프셋 (플레이어 기준)")]
    public Vector2 energyBallOffset = new Vector2(0f, 0f);

    [Header("탄환 생성 설정")]
    [Tooltip("탄환 프리팹")]
    public GameObject bulletPrefab;
    [Tooltip("탄환 생성 여부")]
    public bool spawnBullets = true;
    [Tooltip("오프셋별 생성 지연")]
    public float bulletSpawnDelay = 0.05f;
    [Tooltip("탄환 발사 방향을 이동 방향 기준으로 설정 (끄면 표시용 빔 각도 사용)")]
    public bool useMovementDirection = true;
    [Tooltip("이동 방향 기준 추가 각도 오프셋")]
    public float angleOffset = 0f;

    [Header("탄환 생성 타이밍")]
    [Tooltip("탄환 그룹 생성 간격")]
    public float bulletGroupInterval = 0.2f;
    [Tooltip("생성 기준 (켜면 시간 기반, 끄면 경로 분할 기반)")]
    public bool useTimeBasedSpawning = true;

    [Header("빔 길이 설정")]
    [Tooltip("경로를 나눌 세그먼트 수 (경로 분할 기반일 때만 사용)")]
    [Min(1)]
    public float virtualBeamLength = 5f;

    [Header("반복 설정")]
    [Tooltip("스윕 반복 횟수")]
    [Min(1)]
    public int numShots = 3;
    [Tooltip("스윕 간 대기")]
    public float shotInterval = 1f;

    [Header("탄환 오프셋 설정")]
    public float[] bulletOffsets = new float[] { -5f, -3f, -1f, 0f, 1f, 3f, 5f };

    [Header("탄환 배치 설정")]
    [Tooltip("오프셋당 탄환 수 (1: 단일, 2 이상: 수직 분산)")]
    [Min(1)]
    public int bulletsPerSpawn = 2;
    [Tooltip("탄환 간 수직 간격")]
    public float bulletSpacing = 0.4f;
    [Tooltip("탄환 이동 방향 (켜면 오른쪽, 끄면 왼쪽)")]
    public bool clockwise = true;

    [Header("탄환 속도 설정")]
    [Tooltip("기본 속도")]
    public float bulletSpeed = 3f;
    [Tooltip("스윕 진행 중 속도 커브 사용")]
    public bool useBulletSpeedCurve = true;
    [Tooltip("속도 커브 (X축: 진행도, Y축: 속도 배율)")]
    public AnimationCurve bulletSpeedCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.5f);

    [Header("탄환 가속 설정")]
    [Tooltip("가속 사용 여부")]
    public bool useBulletAcceleration = false;
    [Tooltip("초반 속도")]
    public float bulletStartSpeed = 0.5f;
    [Tooltip("최종 속도")]
    public float bulletFinalSpeed = 3f;
    [Tooltip("가속 지속 시간")]
    public float bulletAccelDuration = 2f;
    [Tooltip("가속 커브 타입")]
    public BulletAccelerationCurve bulletAccelerationCurve = BulletAccelerationCurve.EaseIn;

    [Header("이동 방식 설정")]
    [Tooltip("이동 타입")]
    public MoveType moveType = MoveType.Impulse;
    [Tooltip("부드럽게 이동할 때 걸리는 시간")]
    public float smoothDuration = 2f;
    [Tooltip("부드러운 이동 커브 타입")]
    public MovementCurveType smoothCurveType = MovementCurveType.EaseInOut;
    public AnimationCurve smoothCustomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("순간 이동 연출 시간")]
    public float impulseDuration = 1f;
    [Tooltip("순간 이동 후 대기 시간")]
    public float impulseWaitDuration = 1.5f;
    [Tooltip("순간 이동 커브")]
    public AnimationCurve impulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("사운드 설정")]
    [Tooltip("빔 발사 사운드")]
    public AudioClip beamFireSound;
    [Tooltip("빔 이동 중 루프 사운드")]
    public AudioClip beamLoopSound;
    [Tooltip("빔 종료 사운드")]
    public AudioClip beamEndSound;
    [Tooltip("탄환 발사 사운드")]
    public AudioClip bulletSpawnSound;

    [Header("애니메이션 설정")]
    [Tooltip("이동 중 애니 정지 여부")]
    public bool disableAnimationDuringMove = true;

    [Header("빔 종료 후 이동")]
    [Tooltip("빔 종료 후 위로 이동할지 여부")]
    public bool moveUpAfterBeam = true;
    [Tooltip("위로 이동할 거리")]
    public float moveUpDistance = 2f;
    [Tooltip("위로 이동하는 속도")]
    public float moveUpSpeed = 5f;

    protected List<GameObject> beamTrailList = new List<GameObject>();
    protected List<GameObject> activeBullets = new List<GameObject>();
    protected GameObject activeEnergyBall;
    protected GameObject activeVisualBeam;

    protected virtual void Start()
    {
        StartCoroutine(PatternSequence());
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.2f);
        Gizmos.DrawWireCube(startPos, Vector3.one * 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(endPos, 0.2f);
        Gizmos.DrawWireCube(endPos, Vector3.one * 0.3f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPos, endPos);

        Gizmos.color = Color.white;
        for (int i = 1; i < virtualBeamLength; i++)
        {
            float t = i / virtualBeamLength;
            Vector2 point = Vector2.Lerp(startPos, endPos, t);
            Gizmos.DrawWireCube(point, Vector3.one * 0.15f);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawLine(startPos, endPos);
    }

    protected virtual IEnumerator PatternSequence()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        for (int shot = 0; shot < numShots; shot++)
        {
            yield return StartCoroutine(ExecuteMovingShot());
            if (shot < numShots - 1 && shotInterval > 0f)
                yield return new WaitForSeconds(shotInterval);
        }

        OnPatternComplete();
    }

    protected virtual IEnumerator ExecuteMovingShot()
    {
        yield return StartCoroutine(MoveToStartPosition());
        Vector2 moveDir = (endPos - startPos).normalized;
        float moveDirAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float offsetAngle = visualBeamAngle;
        float fireAngle = useMovementDirection ? moveDirAngle + angleOffset : visualBeamAngle;
        bool dynamicClockwise = moveDir.x > 0f ? true : false;
        bool originalClockwise = clockwise;
        clockwise = dynamicClockwise;

        OnBeforeBeamCreate();
        if (useEnergyBall && energyBallPrefab != null)
        {
            activeEnergyBall = Instantiate(energyBallPrefab, (Vector2)transform.position + energyBallOffset, Quaternion.identity);
        }

        Animator beamAnimator = null;
        FlandreBeam beamScript = null;

        if (showVisualBeam && visualBeamPrefab != null)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, visualBeamAngle + visualBeamRotationOffset);
            activeVisualBeam = Instantiate(visualBeamPrefab, (Vector2)transform.position + visualBeamOffset, rotation);
            beamScript = activeVisualBeam.GetComponent<FlandreBeam>();
            if (beamScript != null) beamScript.DisableAutoDestroy();

            beamAnimator = activeVisualBeam.GetComponent<Animator>();
            if (beamAnimator != null && disableAnimationDuringMove)
            {
                beamAnimator.enabled = false;
                beamAnimator.Play("Idle", 0, 0f);
            }

            if (beamFireSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(beamFireSound);
            }
        }

        OnAfterBeamCreate(activeVisualBeam);

        if (moveDelay > 0f)
            yield return new WaitForSeconds(moveDelay);

        OnBeforeBeamMove();
        if (beamLoopSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(beamLoopSound);
        }

        if (moveType == MoveType.Smooth)
        {
            yield return StartCoroutine(SmoothMove(activeVisualBeam, fireAngle, offsetAngle));
        }

        transform.position = endPos;
        OnAfterBeamMove();

        if (activeVisualBeam != null && beamAnimator != null && disableAnimationDuringMove)
        {
            beamAnimator.enabled = true;
            beamAnimator.Play("EndAnimation");

            if (beamEndSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(beamEndSound);
            }
        }

        if (beamLingerTime > 0f)
        {
            yield return new WaitForSeconds(beamLingerTime);
        }
        else if (beamAnimator != null)
        {
            float animLength = GetAnimationLength(beamAnimator, "EndAnimation");
            yield return new WaitForSeconds(animLength);
        }

        OnBeforeBeamDestroy(activeVisualBeam);
        if (activeVisualBeam != null)
        {
            Destroy(activeVisualBeam);
            activeVisualBeam = null;
        }
        if (activeEnergyBall != null)
        {
            Destroy(activeEnergyBall);
            activeEnergyBall = null;
        }

        ClearBeamTrail();

        if (moveUpAfterBeam)
        {
            yield return StartCoroutine(MoveAfterBeam());
        }

        clockwise = originalClockwise;
        OnShotComplete(dynamicClockwise);
    }

    protected virtual IEnumerator MoveToStartPosition()
    {
        if (moveToStartSpeed > 0f)
        {
            while (Vector2.Distance(transform.position, startPos) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, startPos, moveToStartSpeed * Time.deltaTime);
                yield return null;
            }
        }
        transform.position = startPos;
    }

    protected virtual IEnumerator MoveAfterBeam()
    {
        Vector2 targetPos = (Vector2)transform.position + Vector2.up * moveUpDistance;

        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveUpSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }

    protected virtual IEnumerator SmoothMove(GameObject visualBeam, float fireAngle, float offsetAngle)
    {
        float elapsed = 0f;
        float trailTimer = 0f;
        float bulletSpawnTimer = 0f;
        int segmentIndex = 0;

        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            trailTimer += Time.deltaTime;
            bulletSpawnTimer += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / smoothDuration);
            float curveT = ApplyCurve(t, smoothCurveType);
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, curveT);

            UpdatePosition(currentPos, visualBeam);

            if (useBeamTrail && trailTimer >= beamTrailInterval)
            {
                CreateBeamTrail(visualBeam);
                trailTimer = 0f;
            }

            if (spawnBullets)
            {
                float dynamicSpeed = useBulletSpeedCurve ? bulletSpeed * bulletSpeedCurve.Evaluate(t) : bulletSpeed;

                if (useTimeBasedSpawning)
                {
                    if (bulletSpawnTimer >= bulletGroupInterval)
                    {
                        SpawnBullets(currentPos, fireAngle, offsetAngle, dynamicSpeed);
                        bulletSpawnTimer = 0f;
                    }
                }
                else
                {
                    float totalDistance = Vector2.Distance(startPos, endPos);
                    float traveledDistance = totalDistance * curveT;
                    float segmentDistance = totalDistance / virtualBeamLength;

                    if (traveledDistance >= segmentIndex * segmentDistance && segmentIndex < virtualBeamLength)
                    {
                        SpawnBullets(currentPos, fireAngle, offsetAngle, dynamicSpeed);
                        segmentIndex++;
                    }
                }
            }

            yield return null;
        }

        UpdatePosition(endPos, visualBeam);
    }

    protected virtual IEnumerator ImpulseMove(GameObject visualBeam, float fireAngle, float offsetAngle)
    {
        float totalDistance = Vector2.Distance(startPos, endPos);
        float segmentDistance = totalDistance / virtualBeamLength;
        int currentSegment = 0;
        float elapsed = 0f;
        float trailTimer = 0f;
        float bulletSpawnTimer = 0f;

        while (elapsed < impulseDuration)
        {
            elapsed += Time.deltaTime;
            trailTimer += Time.deltaTime;
            bulletSpawnTimer += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / impulseDuration);
            float curveT = impulseCurve.Evaluate(t);
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, curveT);

            UpdatePosition(currentPos, visualBeam);

            if (useBeamTrail && trailTimer >= beamTrailInterval)
            {
                CreateBeamTrail(visualBeam);
                trailTimer = 0f;
            }

            float dynamicSpeed = useBulletSpeedCurve ? bulletSpeed * bulletSpeedCurve.Evaluate(t) : bulletSpeed;

            if (useTimeBasedSpawning)
            {
                if (bulletSpawnTimer >= bulletGroupInterval)
                {
                    SpawnBullets(currentPos, fireAngle, offsetAngle, dynamicSpeed);
                    bulletSpawnTimer = 0f;
                }
            }
            else
            {
                float traveledDistance = totalDistance * curveT;
                if (traveledDistance >= currentSegment * segmentDistance && currentSegment < virtualBeamLength)
                {
                    SpawnBullets(currentPos, fireAngle, offsetAngle, dynamicSpeed);
                    currentSegment++;
                }
            }

            yield return null;
        }

        UpdatePosition(endPos, visualBeam);

        if (impulseWaitDuration > 0f)
        {
            yield return new WaitForSeconds(impulseWaitDuration);
        }
    }

    protected virtual void UpdatePosition(Vector2 currentPos, GameObject visualBeam)
    {
        transform.position = currentPos;
        if (visualBeam != null)
            visualBeam.transform.position = currentPos + visualBeamOffset;
        if (activeEnergyBall != null)
            activeEnergyBall.transform.position = (Vector2)transform.position + energyBallOffset;
    }

    protected virtual void CreateBeamTrail(GameObject originalBeam)
    {
        if (originalBeam == null || visualBeamPrefab == null) return;

        Quaternion rotation = Quaternion.Euler(0f, 0f, visualBeamAngle + visualBeamRotationOffset);
        GameObject trailBeam = Instantiate(visualBeamPrefab, originalBeam.transform.position, rotation);

        FlandreBeam trailBeamScript = trailBeam.GetComponent<FlandreBeam>();
        if (trailBeamScript != null) trailBeamScript.DisableAutoDestroy();

        SpriteRenderer spriteRenderer = trailBeam.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = trailBeamAlpha;
            spriteRenderer.color = color;
        }

        beamTrailList.Add(trailBeam);
        StartCoroutine(FadeOutBeamTrail(trailBeam));
    }

    protected virtual IEnumerator FadeOutBeamTrail(GameObject trailBeam)
    {
        float elapsed = 0f;
        SpriteRenderer spriteRenderer = trailBeam.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            while (elapsed < trailBeamFadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(trailBeamAlpha, 0f, elapsed / trailBeamFadeTime);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
                yield return null;
            }
        }

        if (beamTrailList.Contains(trailBeam))
            beamTrailList.Remove(trailBeam);
        Destroy(trailBeam);
    }

    protected virtual void ClearBeamTrail()
    {
        foreach (GameObject beam in beamTrailList)
        {
            if (beam != null)
            {
                StopCoroutine("FadeOutBeamTrail");
                Destroy(beam);
            }
        }
        beamTrailList.Clear();
    }

    protected virtual float GetAnimationLength(Animator animator, string stateName)
    {
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        foreach (AnimatorClipInfo clip in clips)
        {
            if (clip.clip.name == stateName)
                return clip.clip.length;
        }
        return 0f;
    }

    protected virtual void SpawnBullets(Vector2 position, float fireAngle, float offsetAngle, float speed)
    {
        if (!spawnBullets || bulletPrefab == null || bulletOffsets.Length == 0) return;

        position += visualBeamOffset;
        Vector2 beamDir = new Vector2(Mathf.Cos(offsetAngle * Mathf.Deg2Rad), Mathf.Sin(offsetAngle * Mathf.Deg2Rad));

        for (int i = 0; i < bulletOffsets.Length; i++)
        {
            Vector2 bulletPos = position + beamDir * bulletOffsets[i];
            StartCoroutine(SpawnBulletWithDelay(bulletPos, fireAngle, speed));
        }
    }

    protected virtual IEnumerator SpawnBulletWithDelay(Vector2 position, float fireAngle, float speed)
    {
        yield return new WaitForSeconds(bulletSpawnDelay);

        Vector2 perpendicular = new Vector2(-Mathf.Sin(fireAngle * Mathf.Deg2Rad), Mathf.Cos(fireAngle * Mathf.Deg2Rad));
        float totalWidth = bulletSpacing * (bulletsPerSpawn - 1);

        for (int i = 0; i < bulletsPerSpawn; i++)
        {
            float offset = bulletsPerSpawn == 1 ? 0f : -totalWidth / 2f + bulletSpacing * i;
            Vector2 bulletPos = position + perpendicular * offset;
            SpawnSingleBullet(bulletPos, fireAngle, speed);
        }
    }

    protected virtual void SpawnSingleBullet(Vector2 position, float fireAngle, float speed)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, fireAngle);
        GameObject bulletObj = Instantiate(bulletPrefab, position, rotation);

        activeBullets.Add(bulletObj);

        if (bulletSpawnSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(bulletSpawnSound);
        }

        BeamBullet bullet = bulletObj.GetComponent<BeamBullet>();

        if (bullet != null)
        {
            Vector2 moveDir = new Vector2(Mathf.Cos(fireAngle * Mathf.Deg2Rad), Mathf.Sin(fireAngle * Mathf.Deg2Rad));
            moveDir = clockwise ? moveDir : -moveDir;

            if (useBulletAcceleration)
            {
                bullet.SetAcceleratedMovement(moveDir, bulletStartSpeed, bulletFinalSpeed, bulletAccelDuration, bulletAccelerationCurve);
            }
            else
            {
                bullet.SetStraightMovement(moveDir, speed);
            }
        }
    }

    protected virtual float ApplyCurve(float t, MovementCurveType curveType)
    {
        switch (curveType)
        {
            case MovementCurveType.Linear: return t;
            case MovementCurveType.EaseIn: return t * t;
            case MovementCurveType.EaseOut: return 1f - (1f - t) * (1f - t);
            case MovementCurveType.EaseInOut: return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            case MovementCurveType.Custom: return smoothCustomCurve.Evaluate(t);
            default: return t;
        }
    }

    public virtual void ClearAllProjectiles()
    {
        StopAllCoroutines();

        if (activeVisualBeam != null)
        {
            Destroy(activeVisualBeam);
            activeVisualBeam = null;
        }

        if (activeEnergyBall != null)
        {
            Destroy(activeEnergyBall);
            activeEnergyBall = null;
        }

        foreach (GameObject beam in beamTrailList)
        {
            if (beam != null)
            {
                Destroy(beam);
            }
        }
        beamTrailList.Clear();

        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        activeBullets.Clear();
    }

    protected virtual void OnDestroy()
    {
        ClearAllProjectiles();
    }
}
