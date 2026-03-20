using UnityEngine;

public class BeamBullet : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] public float speed = 5f; 
    private Vector2 moveDirection;
    private bool isMoving = false;
    private bool useAcceleration = false;
    private float startSpeed, finalSpeed;
    private float accelDuration;
    private float elapsed = 0f;
    private BulletAccelerationCurve accelCurve = BulletAccelerationCurve.Linear;
    public void SetStraightMovement(Vector2 direction, float moveSpeed)
    {
        moveDirection = direction.normalized;
        speed = moveSpeed;
        isMoving = true;
        useAcceleration = false;
        SetRotation(direction);
    }
    public void SetAcceleratedMovement(
        Vector2 direction,
        float sSpeed,
        float fSpeed,
        float duration,
        BulletAccelerationCurve curve
    )
    {
        moveDirection = direction.normalized;
        startSpeed = sSpeed;
        finalSpeed = fSpeed;
        accelDuration = Mathf.Max(0.01f, duration);
        accelCurve = curve;
        elapsed = 0f;
        isMoving = true;
        useAcceleration = true;
        SetRotation(direction);
    }

    void Update()
    {
        if (!isMoving) return;

        float currentSpeed = speed;

        if (useAcceleration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / accelDuration);
            float nt = ApplyCurve(t, accelCurve);
            currentSpeed = Mathf.Lerp(startSpeed, finalSpeed, nt);
            if (t >= 1f)
            {
                currentSpeed = finalSpeed;
            }
        }

        transform.position += (Vector3)(moveDirection * currentSpeed * Time.deltaTime);
        CheckOutOfBounds();
    }

    private void SetRotation(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void CheckOutOfBounds()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.2f || viewPos.x > 1.2f || viewPos.y < -0.2f || viewPos.y > 1.2f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        if (collision.CompareTag("Player"))
            Destroy(gameObject);
    }
    private float ApplyCurve(float t, BulletAccelerationCurve curveType)
    {
        switch (curveType)
        {
            case BulletAccelerationCurve.EaseIn: return t * t;
            case BulletAccelerationCurve.EaseOut: return 1f - (1f - t) * (1f - t);
            case BulletAccelerationCurve.EaseInOut:
                return t < 0.5f
                    ? 2f * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            default: return t;
        }
    }
}

public enum BulletAccelerationCurve
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut
}
