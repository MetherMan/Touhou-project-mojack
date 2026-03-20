using UnityEngine;

public class FlandreBeam : MonoBehaviour
{
    [SerializeField] public float lifetime = 1f;
    [SerializeField] private bool autoDestroy = true;

    [Header("플레이어 피격 이펙트 & 사운드")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound; 

    private void Awake()
    {
        transform.SetParent(null);
        if (!gameObject.CompareTag("EnemyBeam"))
        {
            gameObject.tag = "EnemyBeam";
        }
    }

    private void Start()
    {
        if (!autoDestroy) return;
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                lifetime = clipInfo[0].clip.length;
            }
        }
        Destroy(gameObject, lifetime);
    }

    public void SetLifetime(float time)
    {
        lifetime = time;
    }

    public void DisableAutoDestroy()
    {
        autoDestroy = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1);
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
                }

               
                if (hitSound != null && SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(hitSound);
                }
            }
        }
    }
}
