using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float runMultiplier = 1.5f;

    [Header("Components")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool runPressed;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            jumpPressed = true;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        runPressed = context.performed;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Interact pressed!");
        }
    }

    void Update()
    {
        // Поворот спрайта
        if (moveInput.x > 0 && spriteRenderer != null)
            spriteRenderer.flipX = false;
        else if (moveInput.x < 0 && spriteRenderer != null)
            spriteRenderer.flipX = true;

        // Анимации
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", IsGrounded());
        }
    }

    void FixedUpdate()
    {
        // Движение
        float speed = moveSpeed * (runPressed ? runMultiplier : 1f);
        Vector2 targetVelocity = moveInput * speed;

        rb.linearVelocity = new Vector2(targetVelocity.x, rb.linearVelocity.y);

        // Прыжок
        if (jumpPressed)
        {
            // Ограничиваем горизонтальную скорость в прыжке
            float maxAirSpeed = moveSpeed * 0.5f; // 50% от обычной скорости
            if (Mathf.Abs(rb.linearVelocity.x) > maxAirSpeed)
            {
                rb.linearVelocity = new Vector2(
                    Mathf.Sign(rb.linearVelocity.x) * maxAirSpeed,
                    rb.linearVelocity.y
                );
            }

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPressed = false;
        }

        // ПРОСТОЕ ОГРАНИЧЕНИЕ: не даем двигаться слишком быстро в воздухе
        if (!IsGrounded())
        {
            // Максимальная скорость в воздухе = 30% от обычной
            float maxAirSpeed = moveSpeed * 0.3f;

            // Ограничиваем горизонтальную скорость
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -maxAirSpeed, maxAirSpeed),
                rb.linearVelocity.y
            );
        }
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}