using UnityEngine;

public class WaterSlowdown : MonoBehaviour
{
    public float normalDrag = 0f;
    public float waterDrag = 5f;
    public float normalGravity = 1f;
    public float waterGravity = 0.5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalDrag = rb.linearDamping;
        normalGravity = rb.gravityScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            rb.linearDamping = waterDrag;
            rb.gravityScale = waterGravity;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            rb.linearDamping = normalDrag;
            rb.gravityScale = normalGravity;
        }
    }
}