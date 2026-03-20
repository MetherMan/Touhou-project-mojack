using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerShooting : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    private float fireTimer = 0f;
    private UIManager ui;

    [Header("총알 개수 설정")]
    [SerializeField] private float bulletSpacing = 0.5f;
    [SerializeField] private float diagonalSpreadAngle = 15f;

    [Header("폭탄 설정")]
    [SerializeField] private GameObject bombEffectPrefab;
    [SerializeField] private AudioClip bombSound;
    [SerializeField] private Vector3 bombOffset = new Vector3(0, 2f, 0);
    [SerializeField] private float bombDuration = 3f;
    [SerializeField] private int bombDamage = 50;

    [Header("폭탄 충돌 이펙트")]
    [SerializeField] private GameObject bombHitEffectPrefab;

    private bool isUsingBomb = false;
    private List<GameObject> activeBullets = new List<GameObject>();
    private GameObject activeBombEffect = null;
    private Coroutine bombCoroutine = null;

    void Start()
    {
        ui = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                FireByPowerLevel();
                fireTimer = fireRate;
            }
        }
        else
        {
            fireTimer = 0f;
        }
        if (Input.GetKeyDown(KeyCode.X) && !isUsingBomb)
        {
            UseBomb();
        }
        activeBullets.RemoveAll(b => b == null);
    }

    void FireByPowerLevel()
    {
        if (ui == null) return;
        int power = ui.GetPower();
        if (bulletPrefab.TryGetComponent<PlayerBullet>(out var bulletComp) && bulletComp.ShootSound != null)
        {
            SoundManager.Instance.PlaySFX(bulletComp.ShootSound);
        }
        if (power < 10)
        {
            SpawnBullet(0, 0);
        }
        else if (power < 30)
        {
            SpawnBullet(-bulletSpacing / 2f, 0);
            SpawnBullet(bulletSpacing / 2f, 0);
        }
        else if (power < 50)
        {
            SpawnBullet(-bulletSpacing, 0);
            SpawnBullet(0, 0);
            SpawnBullet(bulletSpacing, 0);
        }
        else if (power < 60)
        {
            SpawnBullet(-bulletSpacing, 0);
            SpawnBullet(0, 0);
            SpawnBullet(bulletSpacing, 0);
            SpawnBullet(-bulletSpacing * 2f, diagonalSpreadAngle);
            SpawnBullet(bulletSpacing * 2f, -diagonalSpreadAngle);
        }
        else if (power < 70)
        {
            SpawnBullet(-bulletSpacing * 1.5f, 0);
            SpawnBullet(-bulletSpacing / 2f, 0);
            SpawnBullet(bulletSpacing / 2f, 0);
            SpawnBullet(bulletSpacing * 1.5f, 0);
            SpawnBullet(-bulletSpacing * 2.5f, diagonalSpreadAngle);
            SpawnBullet(bulletSpacing * 2.5f, -diagonalSpreadAngle);
        }
        else
        {
            SpawnBullet(-bulletSpacing * 2f, 0);
            SpawnBullet(-bulletSpacing, 0);
            SpawnBullet(0, 0);
            SpawnBullet(bulletSpacing, 0);
            SpawnBullet(bulletSpacing * 2f, 0);
            SpawnBullet(-bulletSpacing * 3f, diagonalSpreadAngle);
            SpawnBullet(bulletSpacing * 3f, -diagonalSpreadAngle);
        }
    }

    void SpawnBullet(float xOffset, float angleOffset)
    {
        Vector3 spawnPos = firePoint.position + new Vector3(xOffset, 0, 0);
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angleOffset);

        activeBullets.Add(bullet);
    }

    void UseBomb()
    {
        if (ui == null || !ui.UseBomb())
        {
            return;
        }

        isUsingBomb = true;

        if (bombSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(bombSound);
        }

        if (bombEffectPrefab != null)
        {
            Vector3 bombPosition = transform.position + bombOffset;
            activeBombEffect = Instantiate(bombEffectPrefab, bombPosition, Quaternion.identity);
        }

        bombCoroutine = StartCoroutine(BombEffect(activeBombEffect));
    }

    IEnumerator BombEffect(GameObject bombEffect)
    {
        yield return new WaitForSeconds(0.3f);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            if (bombHitEffectPrefab != null)
            {
                Instantiate(bombHitEffectPrefab, enemy.transform.position, Quaternion.identity);
            }

            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(bombDamage);
            }

            Boss boss = enemy.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(bombDamage);
            }
        }

        float elapsed = 0f;
        while (elapsed < bombDuration)
        {
            GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
            foreach (GameObject bullet in enemyBullets)
            {
                if (bullet == null) continue;
                Animator animator = bullet.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("isBomb");
                    Destroy(bullet, 0.3f);
                }
                else
                {
                    Destroy(bullet);
                }
            }

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        if (bombEffect != null)
        {
            Animator bombAnimator = bombEffect.GetComponent<Animator>();
            if (bombAnimator != null)
            {
                bombAnimator.SetTrigger("isGone");
                Destroy(bombEffect, 0.3f);
            }
            else
            {
                Destroy(bombEffect);
            }
        }

        activeBombEffect = null;
        isUsingBomb = false;
    }
    public void ClearAllProjectiles()
    {
        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        activeBullets.Clear();
        if (activeBombEffect != null)
        {
            Destroy(activeBombEffect);
            activeBombEffect = null;
        }
        if (bombCoroutine != null)
        {
            StopCoroutine(bombCoroutine);
            bombCoroutine = null;
            isUsingBomb = false;
        }
    }
    private void OnDisable()
    {
        if (bombCoroutine != null)
        {
            StopCoroutine(bombCoroutine);
            isUsingBomb = false;
        }
    }
}
