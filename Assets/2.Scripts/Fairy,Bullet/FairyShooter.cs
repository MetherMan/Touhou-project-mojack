using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyShooter : MonoBehaviour
{
    public enum ShootPattern
    {
        Circle,
        AimedCircle,
        LaserStream
    }

    [Header("탄환 관리")]
    private List<GameObject> activeBullets = new List<GameObject>();
    private Coroutine laserStreamCoroutine;

    [Header("탄환 정지")]
    [SerializeField] private float bulletFreezeTime = 0.3f;

    [Header("탄환 프리팹")]
    [SerializeField] private GameObject layer1BulletPrefab;
    [SerializeField] private GameObject layer2BulletPrefab;
    [SerializeField] private GameObject layer3BulletPrefab;
    [SerializeField] private GameObject layer4BulletPrefab;
    [SerializeField] private GameObject layer5BulletPrefab;
    [SerializeField] private GameObject layer6BulletPrefab;
    [SerializeField] private GameObject layer7BulletPrefab;

    [Header("요정 탄환 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ShootPattern shootPattern = ShootPattern.AimedCircle;

    [Header("원형 패턴 설정")]
    [SerializeField] private int bulletCount = 10;
    [SerializeField] private float fireRate = 1.0f;
    [SerializeField] private float bulletSpeed = 5f;

    [Header("레이저 스트림 설정")]
    [SerializeField] private int layer1BulletCount = 15;
    [SerializeField] private int layer2BulletCount = 15;
    [SerializeField] private int layer3BulletCount = 15;
    [SerializeField] private int layer4BulletCount = 15;
    [SerializeField] private int layer5BulletCount = 15;
    [SerializeField] private int layer6BulletCount = 15;
    [SerializeField] private int layer7BulletCount = 15;
    [SerializeField] private float spreadAngle = 30f;

    [Header("개별 탄환 발사 딜레이")]
    [SerializeField] private float bulletLaunchDelay = 0.01f;

    [Header("레이어 속도")]
    [SerializeField] private float[] initialSpeeds = new float[7];
    [SerializeField] private float[] burstSpeeds = new float[7];
    [SerializeField] private float[] finalSpeeds = new float[7];

    [Header("속도 변화 타이밍")]
    [SerializeField] private float accelerationTime = 0.3f;
    [SerializeField] private float burstDuration = 0.5f;
    [SerializeField] private float decelerationTime = 0.4f;

    private float fireTimer = 0f;
    private GameObject player;
    private bool hasUsedLaserStream = false;
    private bool isAttacking = false;

    [Header("체력 관련")]
    [SerializeField] private int maxHP = 10;
    private int currentHP;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        currentHP = maxHP;

        if (initialSpeeds.Length == 7 && initialSpeeds[0] == 0)
        {
            initialSpeeds = new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f };
            burstSpeeds = new float[] { 3f, 4f, 5f, 6f, 7f, 8f, 9f };
            finalSpeeds = new float[] { 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f };
        }
    }

    private void Update()
    {
        if (!isAttacking) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireRate;

            if (shootPattern == ShootPattern.Circle)
                ShootCircle();
            else if (shootPattern == ShootPattern.AimedCircle)
                ShootAimedCircle();
        }
    }

    void ShootCircle()
    {
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            SpawnBullet(dir);
        }
    }

    void ShootAimedCircle()
    {
        if (player == null) return;

        Vector2 playerDir = (player.transform.position - firePoint.position).normalized;
        float playerAngle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = playerAngle + i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            SpawnBullet(dir);
        }
    }

    IEnumerator ShootLaserStream()
    {
        if (player == null) yield break;

        FairyMovement1 movement = GetComponent<FairyMovement1>();
        if (movement != null) movement.enabled = false;

        Vector2 baseDirection = (player.transform.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - spreadAngle / 2f;

        List<BulletData> bulletDataList = new List<BulletData>();

        GameObject[] layerPrefabs = { layer1BulletPrefab, layer2BulletPrefab, layer3BulletPrefab, layer4BulletPrefab, layer5BulletPrefab, layer6BulletPrefab, layer7BulletPrefab };
        int[] layerCounts = { layer1BulletCount, layer2BulletCount, layer3BulletCount, layer4BulletCount, layer5BulletCount, layer6BulletCount, layer7BulletCount };

        for (int layer = 0; layer < 7; layer++)
        {
            float angleStep = layerCounts[layer] > 1 ? spreadAngle / (layerCounts[layer] - 1) : 0f;
            for (int i = 0; i < layerCounts[layer]; i++)
            {
                GameObject prefab = layerPrefabs[layer] != null ? layerPrefabs[layer] : bulletPrefab;
                GameObject bulletObj = Instantiate(prefab, firePoint.position, Quaternion.identity);

                FairyBullet bullet = bulletObj.GetComponent<FairyBullet>();
                if (bullet != null)
                {
                    bullet.Freeze();
                    float spreadedAngle = startAngle + i * angleStep;
                    Vector2 spreadDir = new Vector2(Mathf.Cos(spreadedAngle * Mathf.Deg2Rad), Mathf.Sin(spreadedAngle * Mathf.Deg2Rad)).normalized;
                    StartCoroutine(DelayedLaunchBullet(bullet, i * bulletLaunchDelay, baseDirection, initialSpeeds[layer]));

                    bulletDataList.Add(new BulletData
                    {
                        bullet = bullet,
                        spreadDirection = spreadDir,
                        initialSpeed = initialSpeeds[layer],
                        burstSpeed = burstSpeeds[layer],
                        finalSpeed = finalSpeeds[layer]
                    });
                }
            }
        }

        StartCoroutine(ControlBulletSpeedAndSpread(bulletDataList, baseDirection));

        if (movement != null) movement.enabled = true;
    }

    IEnumerator DelayedLaunchBullet(FairyBullet bullet, float delay, Vector2 direction, float speed)
    {
        yield return new WaitForSeconds(bulletFreezeTime + delay);
        if (bullet != null)
            bullet.Launch(direction, speed);
    }

    IEnumerator ControlBulletSpeedAndSpread(List<BulletData> bullets, Vector2 baseDirection)
    {
        float elapsed = 0f;
        while (elapsed < accelerationTime)
        {
            float t = elapsed / accelerationTime;
            foreach (var data in bullets)
            {
                if (data.bullet != null)
                {
                    data.bullet.SetDirection(Vector2.Lerp(baseDirection, data.spreadDirection, t));
                    data.bullet.SetSpeed(Mathf.Lerp(data.initialSpeed, data.burstSpeed, t));
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < burstDuration)
        {
            foreach (var data in bullets) if (data.bullet != null) data.bullet.SetSpeed(data.burstSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < decelerationTime)
        {
            float t = elapsed / decelerationTime;
            foreach (var data in bullets)
            {
                if (data.bullet != null) data.bullet.SetSpeed(Mathf.Lerp(data.burstSpeed, data.finalSpeed, t));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var data in bullets) if (data.bullet != null) data.bullet.SetSpeed(data.finalSpeed);
    }

    void SpawnBullet(Vector2 direction)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        FairyBullet bullet = bulletObj.GetComponent<FairyBullet>();
        if (bullet != null)
        {
            bullet.SetDirection(direction);
            bullet.SetSpeed(bulletSpeed);
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void StartAttack()
    {
        isAttacking = true;
        fireTimer = 0f;

        if (shootPattern == ShootPattern.LaserStream && !hasUsedLaserStream)
        {
            laserStreamCoroutine = StartCoroutine(ShootLaserStream());
            hasUsedLaserStream = true;
        }
    }

    public void StopAttack()
    {
        isAttacking = false;
        hasUsedLaserStream = true;

        if (laserStreamCoroutine != null)
        {
            StopCoroutine(laserStreamCoroutine);
            laserStreamCoroutine = null;
        }
    }

    private class BulletData
    {
        public FairyBullet bullet;
        public Vector2 spreadDirection;
        public float initialSpeed;
        public float burstSpeed;
        public float finalSpeed;
    }
}
