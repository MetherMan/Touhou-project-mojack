using UnityEngine;

public class DropItemMove : MonoBehaviour
{
    [Header("낙하 속도 설정")]
    [SerializeField] private float startFallSpeed = 0.2f;
    [SerializeField] private float maxFallSpeed = 4f;
    [SerializeField] private float acceleration = 3f;

    private float currentSpeed;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f; 
        }

        currentSpeed = startFallSpeed;
    }

    void Update()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxFallSpeed, acceleration * Time.deltaTime);
        transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);
    }
}
