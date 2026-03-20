using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 관련")]
    [SerializeField] private float moveSpeed = 5.0f;
    private float inputX;
    private float inputY;
    private Rigidbody2D rb2D;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        inputX = (int)Input.GetAxisRaw("Horizontal");
        inputY = (int)Input.GetAxisRaw("Vertical");

        if (inputX < 0)
            animator.SetInteger("Move", -1);
        else if (inputX > 0)
            animator.SetInteger("Move", 1);
        else
            animator.SetInteger("Move", 0);
    }

    public void ResetInput()
    {
        inputX = 0;
        inputY = 0;
        rb2D.velocity = Vector2.zero;
        animator.SetInteger("Move", 0);
    }

    private void FixedUpdate()
    {
        Vector2 moveDir = new Vector2(inputX, inputY).normalized;
        rb2D.velocity = moveDir * moveSpeed;
    }
    private void OnDisable()
    {
        inputX = 0;
        inputY = 0;
        if (rb2D != null)
            rb2D.velocity = Vector2.zero;
        if (animator != null)
            animator.SetInteger("Move", 0);
    }
}
