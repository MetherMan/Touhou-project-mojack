using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyMovement1 : MonoBehaviour
{

    public enum MovementPattern
    {
        StraightDown,
        SineWave,
        StopAndShoot,
        ZigZag,
        Circular
    }

    public enum AttackBehavior
    {
        None,
        AttackAndContinueDown,
        AttackAndGoUp
    }

    [Header("상승 공격 설정")]
    [SerializeField] private float attackUpSpeed = 5f;

    [Header("이동 패턴 선택")]
    [SerializeField] private MovementPattern movementPattern = MovementPattern.SineWave;

    [Header("공격 행동 패턴 선택")]
    [SerializeField] private AttackBehavior attackBehavior = AttackBehavior.AttackAndContinueDown;

    [Header("기본 이동 설정")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 2f;

    [Header("공격 관련 설정 (모든 패턴 공통)")]
    [SerializeField] private float minAttackHeight = 6f;
    [SerializeField] private float maxAttackHeight = 10f;
    [SerializeField] private float minStopDuration = 2f; 
    [SerializeField] private float maxStopDuration = 4f; 
    private float randomAttackHeight;
    private float randomStopDuration;

    [Header("지그재그 이동 설정")]
    [SerializeField] private float zigzagWidth = 1.5f;
    [SerializeField] private float zigzagSpeed = 3f;

    [Header("원형 이동 설정")]
    [SerializeField] private float circleRadius = 1.5f;
    [SerializeField] private float circleSpeed = 2f;

    [Header("상승 공격 곡선 설정")]
    [SerializeField] private float curveAmplitude = 1.5f;
    [SerializeField] private float curveFrequency = 1f;

    private Vector3 startPos;
    private float elapsedTime = 0f;
    private bool hasStopped = false;
    private float zigzagDirection = 1f;
    private float direction = -1f;

    private bool canAttack = false;

    void Start()
    {
        startPos = transform.position;
        canAttack = GetComponent<FairyShooter>() != null;
        if (!canAttack)
        {
            attackBehavior = AttackBehavior.None;
        }

        if (attackBehavior != AttackBehavior.None)
        {
            randomAttackHeight = Random.Range(minAttackHeight, maxAttackHeight);
            randomStopDuration = Random.Range(minStopDuration, maxStopDuration);
        }
    }

    void Update()
    {
        if (hasStopped) return; 

        elapsedTime += Time.deltaTime;
        if (attackBehavior != AttackBehavior.None && direction < 0)
        {
            if (transform.position.y <= randomAttackHeight)
            {
                hasStopped = true;
                StartCoroutine(ResumeMovementAfterStop());
                return; 
            }
        }
        switch (movementPattern)
        {
            case MovementPattern.StraightDown:
                MoveStraightDown();
                break;

            case MovementPattern.SineWave:
                MoveSineWave();
                break;

            case MovementPattern.StopAndShoot:
                MoveStopAndShoot();
                break;

            case MovementPattern.ZigZag:
                MoveZigZag();
                break;

            case MovementPattern.Circular:
                MoveCircular();
                break;
        }

        CheckOutOfBounds();
    }
    void MoveStraightDown()
    {
        float speed = (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
                        ? attackUpSpeed
                        : moveSpeed;

        float newY = startPos.y + direction * speed * elapsedTime;
        float newX = startPos.x;

        if (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
        {
            newX += Mathf.Sin(elapsedTime * curveFrequency) * curveAmplitude;
        }

        transform.position = new Vector3(newX, newY, startPos.z);
    }

    void MoveSineWave()
    {
        float speed = (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
                        ? attackUpSpeed
                        : moveSpeed;

        float newY = startPos.y + direction * speed * elapsedTime;
        float newX = startPos.x + Mathf.Sin(elapsedTime * frequency) * amplitude;

        if (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
        {
            newX += Mathf.Sin(elapsedTime * curveFrequency) * curveAmplitude;
        }

        transform.position = new Vector3(newX, newY, startPos.z);
    }

    void MoveZigZag()
    {
        float speed = (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
                        ? attackUpSpeed
                        : moveSpeed;

        float newY = startPos.y + direction * speed * elapsedTime;
        float zigzagOffset = Mathf.PingPong(elapsedTime * zigzagSpeed, zigzagWidth * 2) - zigzagWidth;
        float newX = startPos.x + zigzagOffset;

        if (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
        {
            newX += Mathf.Sin(elapsedTime * curveFrequency) * curveAmplitude;
        }

        transform.position = new Vector3(newX, newY, startPos.z);
    }

    void MoveCircular()
    {
        float speed = (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
                        ? attackUpSpeed
                        : moveSpeed;

        float newY = startPos.y + direction * speed * elapsedTime;
        float angle = elapsedTime * circleSpeed;
        float newX = startPos.x + Mathf.Cos(angle) * circleRadius;
        float circleY = Mathf.Sin(angle) * circleRadius;

        if (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
        {
            newX += Mathf.Sin(elapsedTime * curveFrequency) * curveAmplitude;
        }

        transform.position = new Vector3(newX, newY + circleY, startPos.z);
    }

    void MoveStopAndShoot()
    {
        float speed = (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
                        ? attackUpSpeed
                        : moveSpeed;

        float newY = startPos.y + direction * speed * elapsedTime;
        float newX = startPos.x;

        if (direction > 0 && attackBehavior == AttackBehavior.AttackAndGoUp)
        {
            newX += Mathf.Sin(elapsedTime * curveFrequency) * curveAmplitude;
        }

        transform.position = new Vector3(newX, newY, startPos.z);
    }

    IEnumerator ResumeMovementAfterStop()
    {
        if (canAttack)
        {
            GetComponent<FairyShooter>().StartAttack();
        }
        yield return new WaitForSeconds(randomStopDuration);

        if (canAttack)
        {
            GetComponent<FairyShooter>().StopAttack();
        }

        if (attackBehavior == AttackBehavior.AttackAndGoUp)
        {
            direction = 1f;
        }
        startPos = transform.position;
        elapsedTime = 0f;
        hasStopped = false; 
    }
    void CheckOutOfBounds()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.y < -0.1f || viewPos.y > 1.1f || viewPos.x < -0.1f || viewPos.x > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (Camera.main == null) return;
        float cameraHalfWidth = Camera.main.aspect * Camera.main.orthographicSize;
        float leftX = Camera.main.transform.position.x - cameraHalfWidth - 1f;
        float rightX = Camera.main.transform.position.x + cameraHalfWidth + 1f;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(leftX, minAttackHeight, 0), new Vector3(rightX, minAttackHeight, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(leftX, maxAttackHeight, 0), new Vector3(rightX, maxAttackHeight, 0));
    }
}
