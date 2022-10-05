using Pathfinding;
using System;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] protected int maxHealth = 0;
    [SerializeField] protected float attackRate = 0;
    [SerializeField] protected int attackPower = 0;

    [Header("References")]
    [SerializeField] protected AudioClip attackSound = null;
    [SerializeField] protected AudioClip hitSound = null;
    [SerializeField] protected LayerMask playerLayer = 0;

    // State control
    protected int currentHealth;
    protected float lastAttackTime;
    protected bool moving;
    protected bool facingRight;
    protected bool dead;

    // Read-only variables
    protected readonly string anim_trigger_Hit = "Hit";
    protected readonly string anim_bool_Dead = "Dead";

    // Events
    public static event Action<Enemy> enemyKilledEvent;

    // References
    protected Player player;
    protected SpriteRenderer sr;
    protected BoxCollider2D col;
    protected Animator anim;
    protected AudioSource audioSrc;
    protected AIPath ai;
    protected AIDestinationSetter aiDest;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        ai = GetComponent<AIPath>();
        aiDest = GetComponent<AIDestinationSetter>();
        AIPath.onTargetReached += OnTargetReached;
    }

    void Start()
    {
        currentHealth = maxHealth;
        ai.canMove = true;
        aiDest.target = player.transform;
    }

    void OnDisable()
    {
        AIPath.onTargetReached -= OnTargetReached;
    }

    public virtual void Hit(int amount)
    {
        amount = Mathf.Abs(amount);

        currentHealth -= amount;

        anim.SetTrigger(anim_trigger_Hit);

        audioSrc.PlayOneShot(hitSound);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected abstract void OnTargetReached();

    protected bool CanAttack()
    {
        return !dead && Time.time - lastAttackTime > 1f / attackRate;
    }

    protected abstract void Attack();

    protected Vector2 GetAimDirection()
    {
        if (facingRight)
            return Vector2.right;
        else
            return Vector2.left;
    }

    protected void Die()
    {
        dead = true;
        anim.SetBool(anim_bool_Dead, dead);
        col.enabled = false;
        Destroy(gameObject, 3);
        enemyKilledEvent?.Invoke(this);
    }

    public void CanMove()
    {
        ai.canMove = true;
    }

    public void CannotMove()
    {
        ai.canMove = false;
    }

    public void CanSearch()
    {
        ai.canSearch = true;
    }

    public void CannotSearch()
    {
        ai.canSearch = false;
    }
}
