using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    //[SerializeField] private float shortJumpMultiplier = 0.5f;
    [SerializeField] private float jumpCutMultiplier = 0.3f;
    [SerializeField] private float jumpCoyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Air Control")]
    [SerializeField] private float airDeceleration = 3f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerControls playerControls;

    private Vector2 moveInput;
    private bool isGrounded;

    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()  { playerControls.Player.Enable(); }
    private void OnDisable() { playerControls.Player.Disable(); }

    private void Update()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();

        // Земля
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // Coyote Time
        if (isGrounded)
            lastGroundedTime = jumpCoyoteTime;
        else
            lastGroundedTime -= Time.deltaTime;

        // Сброс isJumping при приземлении
        if (isGrounded && isJumping)
            isJumping = false;

        // Jump buffer
        if (playerControls.Player.Jump.WasPressedThisFrame())
            lastJumpPressedTime = jumpBufferTime;
        else
            lastJumpPressedTime -= Time.deltaTime;

        // Прыжок
        if (lastJumpPressedTime > 0 && lastGroundedTime > 0 && !isJumping)
        {
            Jump();
            lastJumpPressedTime = 0;
        }

        // Короткий прыжок
        if (playerControls.Player.Jump.WasReleasedThisFrame() && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // Поворот спрайта
        if (moveInput.x != 0)
            spriteRenderer.flipX = moveInput.x < 0;

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        float currentSpeed = moveSpeed;
        if (playerControls.Player.Run.IsPressed())
            currentSpeed *= runMultiplier;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
        }
        else
        {
            if (moveInput.x != 0)
            {
                rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
            }
            else
            {
                float dampedX = Mathf.MoveTowards(rb.linearVelocity.x, 0f, airDeceleration * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(dampedX, rb.linearVelocity.y);
            }
        }
    }

    private void UpdateAnimator()
    {
        bool isRunning = playerControls.Player.Run.IsPressed() && moveInput.x != 0;
        bool isWalking = moveInput.x != 0 && !isRunning;

        animator.SetBool("walkPressed", isWalking);
        animator.SetBool("runPressed", isRunning);
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsJumping", isJumping);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isJumping = true;
        animator.SetTrigger("Jump");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}