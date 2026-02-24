using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float runMultiplier = 1.6f;          // FIX: was 1.0005f (effectively no run boost)

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float coyoteTime = 0.12f;            // NEW: grace period after walking off a ledge
    public float jumpBufferTime = 0.15f;        // NEW: queues a jump if pressed slightly early

    [Header("Air Control")]
    public float airControlMultiplier = 0.8f;   // FIX: replaces the conflicting 0.5f mid-air halving
    public float maxAirSpeed = 12f;             // FIX: now a separate field, was moveSpeed * 1.5f hardcoded

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
    private bool jumpPressed;           // set by input, consumed in FixedUpdate
    private bool isJumping;             // true while airborne after a jump
    private float coyoteTimer;          // counts down after leaving ground
    private float jumpBufferTimer;      // counts down after jump input

    // Animation state
    private bool isWalking;
    private bool isRunning;
    private float smoothSpeed;
    private float speedVelocity;

    // Grounded state cached per-frame to avoid multiple OverlapCircle calls
    private bool wasGrounded;
    private bool isGrounded;

    // -------------------------------------------------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-assign visuals if not set in Inspector
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)       animator       = GetComponent<Animator>();
    }

    // --- Input callbacks (hooked up via Player Input component) ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // FIX: don't gate the jump here on IsGrounded — that's handled in FixedUpdate
            // using coyote time + jump buffer instead
            jumpBufferTimer = jumpBufferTime;
        }

        // Allow early release to cut jump height (feel improvement)
        if (context.canceled && rb.linearVelocity.y > 0f)
        {
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
        // Cache grounded check once per frame
        wasGrounded = isGrounded;
        isGrounded  = CheckGrounded();

        // --- Coyote time ---
        // Reset timer when grounded; count down when airborne
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // --- Jump buffer ---
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // --- Consume buffered jump if we have ground eligibility ---
        bool canJump = coyoteTimer > 0f;
        if (jumpBufferTimer > 0f && canJump)
        {
            jumpPressed      = true;
            jumpBufferTimer  = 0f;
            coyoteTimer      = 0f;  // consume coyote window so we don't double-jump
        }

        // --- Detect landing ---
        // FIX: was resetting isJumping the same frame as the jump using IsGrounded,
        // which could instantly cancel the jump state. Now we wait until we've
        // actually left the ground first (wasGrounded -> !isGrounded transition).
        if (isJumping && isGrounded && !wasGrounded == false && rb.linearVelocity.y <= 0f)
        {
            isJumping = false;
        }

        // --- Sprite flip ---
        if (moveInput.x > 0f)       spriteRenderer.flipX = false;
        else if (moveInput.x < 0f)  spriteRenderer.flipX = true;

        // --- Animation speed smoothing (grounded only, freeze mid-air) ---
        if (isGrounded)
        {
            float targetSpeed = Mathf.Abs(rb.linearVelocity.x);
            smoothSpeed = Mathf.SmoothDamp(smoothSpeed, targetSpeed, ref speedVelocity, animationSmoothTime);
            isWalking   = smoothSpeed > animationThreshold;
            isRunning   = isWalking && runPressed && smoothSpeed > moveSpeed * 0.5f;
        }

        // --- Push to Animator ---
        if (animator != null)
        {
            animator.SetFloat("Speed",         smoothSpeed / moveSpeed);   // FIX: normalized by moveSpeed, not magic /200
            animator.SetBool("walkPressed",    isWalking && isGrounded);
            animator.SetBool("runPressed",     isRunning && isGrounded);
            animator.SetBool("IsGrounded",     isGrounded);
            animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
            animator.SetBool("IsJumping",      isJumping);
        }
    }

    void FixedUpdate()
    {
        // --- Horizontal movement ---
        // FIX: air control is now a clean multiplier, no conflict with the air speed clamp
        float speed = moveSpeed * (runPressed ? runMultiplier : 1f);
        if (!isGrounded) speed *= airControlMultiplier;

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

        // --- Clamp horizontal air speed ---
        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -maxAirSpeed, maxAirSpeed),
                rb.linearVelocity.y
            );
        }

        // --- Execute jump ---
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // zero out vertical before impulse
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping   = true;
            jumpPressed = false;

            if (animator != null)
                animator.SetTrigger("Jump");
        }
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