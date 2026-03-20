using UnityEngine;

public class FairyBullet : MonoBehaviour
{
    public GameObject HitEffectPrefab => hitEffectPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    private Vector2 moveDir;
    [SerializeField] private float speed = 2.5f;
    private float rotationSpeed = 0f;
    private bool isActive = true;

    public void SetDirection(Vector2 dir)
    {
        moveDir = dir.normalized;
    }

    public void SetSpeed(float s)
    {
        speed = s;
    }
    public void SetRotation(float rotSpeed)
    {
        rotationSpeed = rotSpeed;
    }

    public void Freeze()
    {
        isActive = false;
    }

    public void Launch(Vector2 direction, float launchSpeed)
    {
        moveDir = direction.normalized;
        speed = launchSpeed;
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;
        if (rotationSpeed != 0f)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            moveDir = Rotate2D(moveDir, rotationThisFrame);
            float currentAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle - 90f);
        }
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);

        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
        {
            Destroy(gameObject);
        }
    }
    private Vector2 Rotate2D(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            Destroy(gameObject);
        }
    }
}
