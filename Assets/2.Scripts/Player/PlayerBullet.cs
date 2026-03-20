using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("탄환 관련 설정")]
    [SerializeField] private float playerBulletSpeed = 10.0f;
    [SerializeField] private int damage = 1;

    [Header("이펙트 및 사운드")]
    [SerializeField] private GameObject hitEffectPrefab;  
    [SerializeField] private AudioClip hitSound;            

    public AudioClip ShootSound;

    void Update()
    {
        transform.Translate(Vector2.up * playerBulletSpeed * Time.deltaTime);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Boss boss = collision.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            if (hitSound != null)
            {
                SoundManager.Instance.PlaySFX(hitSound);
            }
            Destroy(gameObject);
        }
    }
}
