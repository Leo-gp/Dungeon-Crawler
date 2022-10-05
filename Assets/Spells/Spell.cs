using UnityEngine;

public abstract class Spell : MonoBehaviour
{
    // Attributes
    public float Speed { get; set; }
    public int Damage { get; set; }
    public Vector2 Direction { get; set; }

    // References
    protected Rigidbody2D rb;
    protected GameController gc;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gc = FindObjectOfType<GameController>();
        Destroy(gameObject, 30);
    }

    void FixedUpdate()
    {
        rb.velocity = Direction * Speed;
    }

    protected abstract void OnTriggerEnter2D(Collider2D col);
}
