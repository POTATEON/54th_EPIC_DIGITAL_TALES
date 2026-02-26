using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float runMultiplier = 1.6f;

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;

    [Header("Air Control")]
    public float airControlMultiplier = 0.8f;
    public float maxAirSpeed = 12f;
    public float airAcceleration = 8f;          // NEW: насколько быстро подтягиваемся к желаемой скорости в воздухе
                                                //      меньше = больше инерции, меньше контроля (попробуй 5–15)

    [Header("Animation Settings")]
    public float animationThreshold = 0.1f;
    public float animationSmoothTime = 0.05f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    // --- Private state ---
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool runPressed;

    // Jump state
    private bool jumpPressed;
    private bool isJumping;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpHeld;
    private float jumpHeldTimer;
    public float jumpHoldForce = 25f;
    public float maxJumpHoldTime = 1f;

    // Animation state
    private bool isWalking;
    private bool isRunning;
    private float smoothSpeed;
    private float speedVelocity;

    // Grounded state cached per-frame
    private bool wasGrounded;
    private bool isGrounded;

    // -------------------------------------------------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpHeld = true;
            jumpHeldTimer = 0f;
        }

        if (context.canceled)
        {
            jumpHeld = false;
            if (rb.linearVelocity.y > 0f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        runPressed = context.performed;
    }

    // -------------------------------------------------------------------------

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        // --- Coyote time ---
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // --- Jump buffer ---
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // --- Consume buffered jump ---
        bool canJump = coyoteTimer > 0f;
        if (jumpBufferTimer > 0f && canJump)
        {
            jumpPressed = true;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // --- Удержание прыжка ---
        if (jumpHeld && isJumping && jumpHeldTimer < maxJumpHoldTime)
        {
            jumpHeldTimer += Time.fixedDeltaTime;
            float holdRatio = 1f - (jumpHeldTimer / maxJumpHoldTime);
            rb.AddForce(Vector2.up * jumpHoldForce * holdRatio, ForceMode2D.Force);
        }

        // --- Сброс при приземлении ---
        if (isGrounded)
        {
            jumpHeld = false;
            jumpHeldTimer = 0f;
        }

        // --- Detect landing ---
        if (isJumping && isGrounded && !wasGrounded == false && rb.linearVelocity.y <= 0f)
            isJumping = false;

        // --- Sprite flip ---
        if (moveInput.x > 0f) spriteRenderer.flipX = false;
        else if (moveInput.x < 0f) spriteRenderer.flipX = true;

        // --- Animation speed smoothing ---
        if (isGrounded)
        {
            float targetSpeed = Mathf.Abs(rb.linearVelocity.x);
            smoothSpeed = Mathf.SmoothDamp(smoothSpeed, targetSpeed, ref speedVelocity, animationSmoothTime);
            isWalking = smoothSpeed > animationThreshold;
            isRunning = isWalking && runPressed && smoothSpeed > moveSpeed * 0.5f;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", smoothSpeed / moveSpeed);
            animator.SetBool("walkPressed", isWalking && isGrounded);
            animator.SetBool("runPressed", isRunning && isGrounded);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
            animator.SetBool("IsJumping", isJumping);
        }
    }

    void FixedUpdate()
    {
        // --- Horizontal movement ---
        float targetSpeed = moveInput.x * moveSpeed * (runPressed ? runMultiplier : 1f);

        if (isGrounded)
        {
            // На земле — мгновенная установка скорости (чёткое управление)
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            // В воздухе — AddForce к желаемой скорости, чтобы сохранялась инерция.
            // speedDiff определяет "тягу" к желаемому направлению, но не телепортирует скорость.
            float currentX = rb.linearVelocity.x;
            float speedDiff = targetSpeed * airControlMultiplier - currentX;
            rb.AddForce(new Vector2(speedDiff * airAcceleration, 0f), ForceMode2D.Force);

            // Clamp горизонтальной скорости в воздухе
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -maxAirSpeed, maxAirSpeed),
                rb.linearVelocity.y
            );
        }

        // --- Execute jump ---
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
            jumpPressed = false;

            if (animator != null)
                animator.SetTrigger("Jump");
        }

        // --- Тяжёлое падение ---
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Vector2.down * 20f, ForceMode2D.Force);
    }

    // -------------------------------------------------------------------------

    bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}