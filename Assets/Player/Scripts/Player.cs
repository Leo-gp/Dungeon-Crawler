using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private enum FacingDirection { Up, Right, Down, Left }

    [Header("Attributes")]
    [SerializeField] private int maxHealth;
    public int MaxHealth { get { return maxHealth; } private set { maxHealth = value; } }
    [SerializeField] private float maxStamina;
    public float MaxStamina { get { return maxStamina; } private set { maxStamina = value; } }
    [SerializeField] private int maxSpell;
    public int MaxSpell { get { return maxSpell; } private set { maxSpell = value; } }
    [SerializeField] private float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } private set { moveSpeed = value; } }
    [SerializeField] private float attackRange;
    public float AttackRange { get { return attackRange; } private set { attackRange = value; } }
    [SerializeField] private float attackRate;
    public float AttackRate { get { return attackRate; } private set { attackRate = value; } }
    [SerializeField] private int attackPower;
    public int AttackPower { get { return attackPower; } private set { attackPower = value; } }
    [SerializeField] private float dashDistance;
    public float DashDistance { get { return dashDistance; } private set { dashDistance = value; } }
    [SerializeField] private float dashSpeed;
    public float DashSpeed { get { return dashSpeed; } private set { dashSpeed = value; } }
    [SerializeField] private float dashRate;
    public float DashRate { get { return dashRate; } private set { dashRate = value; } }
    [SerializeField] private float spellSpeed;
    public float SpellSpeed { get { return spellSpeed; } private set { spellSpeed = value; } }
    [SerializeField] private int spellPower;
    public int SpellPower { get { return spellPower; } private set { spellPower = value; } }

    [Header("Settings")]
    [SerializeField] private int startingHealthAmount = 0;
    [SerializeField] private int startingSpellsAmount = 0;
    [SerializeField] private int staminaPerAttack = 0;
    [SerializeField] private int staminaPerDash = 0;
    [SerializeField] private float staminaRecoverSpeed = 0;
    [SerializeField] private float staminaRecoverAcceleration = 0;
    [SerializeField] private float staminaRecoverDelay = 0;

    [Header("References")]
    [SerializeField] private LayerMask enemyLayer = 0;
    [SerializeField] private LayerMask chestLayer = 0;
    [SerializeField] private CrystalBall crystalBallPrefab = null;
    [SerializeField] private AudioClip attackSound = null;
    [SerializeField] private AudioClip dashSound = null;
    [SerializeField] private AudioClip spellSound = null;
    [SerializeField] private AudioClip hitSound = null;

    // Read-only variables
    private readonly string anim_bool_Moving = "Moving";
    private readonly string anim_bool_FacingUp = "FacingUp";
    private readonly string anim_bool_FacingSide = "FacingSide";
    private readonly string anim_bool_FacingDown = "FacingDown";
    private readonly string anim_trigger_Attack = "Attack";
    private readonly string anim_trigger_Hit = "Hit";
    private readonly string playerLayerName = "Player";
    private readonly string enemyLayerName = "Enemy";
    private readonly string enemySpellLayerName = "EnemySpell";

    // Events
    public static event Action healthChangedEvent;
    public static event Action staminaChangedEvent;
    public static event Action spellChangedEvent;
    public static event Action keyCollectedEvent;

    // State control
    private bool moving;
    private bool dashing;
    private bool aimLocked;
    private FacingDirection facingDirection;
    private Vector2 moveDirection;
    private float lastAttackTime;
    private float lastDashTime;
    private bool canMove;

    private int _currentHealth;
    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
        set
        {
            value = Mathf.Min(MaxHealth, value);
            _currentHealth = value;
            healthChangedEvent?.Invoke();
        }
    }
    private float _currentStamina;
    public float CurrentStamina
    {
        get
        {
            return _currentStamina;
        }
        set
        {
            value = Mathf.Min(MaxStamina, value);
            _currentStamina = Mathf.Max(0, value);
            staminaChangedEvent?.Invoke();
        }
    }
    private int _currentSpellAmount;
    public int CurrentSpellAmount
    {
        get
        {
            return _currentSpellAmount;
        }
        set
        {
            value = Mathf.Min(MaxSpell, value);
            _currentSpellAmount = Mathf.Max(0, value);
            spellChangedEvent?.Invoke();
        }
    }

    private int _currentKeysAmount;
    public int CurrentKeysAmount
    {
        get
        {
            return _currentKeysAmount;
        }
        set
        {
            value = Mathf.Min(value, gc.KeysToFinish);
            _currentKeysAmount = value;
            keyCollectedEvent?.Invoke();
        }
    }

    // References
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Animator anim;
    private SpriteRenderer sr;
    private AudioSource audioSrc;
    private GameController gc;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSrc = GetComponent<AudioSource>();
        gc = FindObjectOfType<GameController>();
    }

    void Start()
    {
        CurrentHealth = startingHealthAmount;
        CurrentStamina = maxStamina;
        CurrentSpellAmount = startingSpellsAmount;
        CurrentKeysAmount = 0;
        facingDirection = FacingDirection.Right;
        canMove = true;
        StartCoroutine(StaminaRecover());
    }

    void Update()
    {
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        HandleFacingDirection();

        moving = canMove && (Mathf.Abs(rb.velocity.x) > 0.1f || Mathf.Abs(rb.velocity.y) > 0.1f);

        anim.SetBool(anim_bool_Moving, moving);

        if (Input.GetKeyDown(KeyCode.Mouse0) && CanAttack())
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Space) && CanDash())
        {
            StartCoroutine(Dash());
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && CanCastSpell())
        {
            CastSpell();
        }

        aimLocked = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.E))
        {
            var chest = SearchChest();
            if (chest != null)
                chest.Open();
        }

        Debug.DrawRay(transform.position, GetAimDirection() * attackRange, Color.red);
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(gc.PowerUpTag))
        {
            var powerUp = other.gameObject.GetComponent<PowerUp>();
            powerUp.Collect();
        }
    }

    private void HandleFacingDirection()
    {
        if (aimLocked || canMove == false)
        {
            return;
        }

        if (moveDirection.x > 0 && moveDirection.y == 0)
        {
            facingDirection = FacingDirection.Right;
        }
        else if (moveDirection.x < 0 && moveDirection.y == 0)
        {
            facingDirection = FacingDirection.Left;
        }
        else if (moveDirection.y > 0 && moveDirection.x == 0)
        {
            facingDirection = FacingDirection.Up;
        }
        else if (moveDirection.y < 0 && moveDirection.x == 0)
        {
            facingDirection = FacingDirection.Down;
        }

        if ((facingDirection == FacingDirection.Right && transform.localScale.x < 0) || (facingDirection == FacingDirection.Left && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        SetFacingAnimatorBool(facingDirection);
    }

    private void SetFacingAnimatorBool(FacingDirection facingDirection)
    {
        switch (facingDirection)
        {
            case FacingDirection.Up:
                anim.SetBool(anim_bool_FacingUp, true);
                anim.SetBool(anim_bool_FacingDown, false);
                anim.SetBool(anim_bool_FacingSide, false);
                break;
            case FacingDirection.Right:
                anim.SetBool(anim_bool_FacingSide, true);
                anim.SetBool(anim_bool_FacingUp, false);
                anim.SetBool(anim_bool_FacingDown, false);
                break;
            case FacingDirection.Down:
                anim.SetBool(anim_bool_FacingDown, true);
                anim.SetBool(anim_bool_FacingUp, false);
                anim.SetBool(anim_bool_FacingSide, false);
                break;
            case FacingDirection.Left:
                anim.SetBool(anim_bool_FacingSide, true);
                anim.SetBool(anim_bool_FacingUp, false);
                anim.SetBool(anim_bool_FacingDown, false);
                break;
        }
    }

    private Vector2 GetAimDirection()
    {
        switch (facingDirection)
        {
            case FacingDirection.Up:
                return Vector2.up;
            case FacingDirection.Right:
                return Vector2.right;
            case FacingDirection.Down:
                return Vector2.down;
            case FacingDirection.Left:
                return Vector2.left;
            default:
                return Vector2.zero;
        }
    }

    private void Attack()
    {
        anim.SetTrigger(anim_trigger_Attack);
        audioSrc.PlayOneShot(attackSound);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, GetAimDirection(), attackRange, enemyLayer);

        if (hit.collider != null)
        {
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            enemy.Hit(attackPower);
        }

        CurrentStamina -= staminaPerAttack;

        lastAttackTime = Time.time;
    }

    private bool CanAttack()
    {
        if (CurrentStamina < staminaPerAttack || dashing)
            return false;
        return Time.time - lastAttackTime > 1f / attackRate;
    }

    public void Hit(int amount)
    {
        amount = Mathf.Abs(amount);

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            anim.SetTrigger(anim_trigger_Hit);
        }
    }

    private void Die()
    {
        GetComponentInChildren<Camera>().transform.SetParent(null);
        gc.GameOver();
    }

    private IEnumerator StaminaRecover()
    {
        float staminaBonusIncrease = 0;
        while (true)
        {
            if (CurrentStamina < maxStamina)
            {
                if (Time.time - lastAttackTime > staminaRecoverDelay && Time.time - lastDashTime > staminaRecoverDelay)
                {
                    CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + (staminaRecoverSpeed * Time.deltaTime) + staminaBonusIncrease);
                    staminaBonusIncrease += staminaRecoverAcceleration * Time.deltaTime;
                }
                else
                {
                    staminaBonusIncrease = 0;
                }
            }
            yield return null;
        }
    }

    private IEnumerator Dash()
    {
        dashing = true;
        canMove = false;
        audioSrc.PlayOneShot(dashSound);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemyLayerName), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemySpellLayerName), true);
        lastDashTime = Time.time;
        CurrentStamina -= staminaPerDash;
        Color originalColor = sr.color;
        sr.color = new Color(0, 0, 0, 0.5f);
        Vector2 startPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 endPos = startPos + (moveDirection * dashDistance == Vector2.zero ? GetAimDirection() * dashDistance : moveDirection * dashDistance);
        float t = 0;
        while (t < 1)
        {
            rb.MovePosition(Vector2.Lerp(startPos, endPos, t));
            t += Time.deltaTime * dashSpeed;
            yield return null;
        }
        sr.color = originalColor;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemyLayerName), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemySpellLayerName), false);
        dashing = false;
        canMove = true;
    }

    private bool CanDash()
    {
        if (CurrentStamina < staminaPerDash || dashing)
            return false;
        return Time.time - lastDashTime > 1f / dashRate;
    }

    private void CastSpell()
    {
        audioSrc.PlayOneShot(spellSound);
        var spell = Instantiate(crystalBallPrefab, transform.position, Quaternion.identity);
        spell.Speed = SpellSpeed;
        spell.Damage = SpellPower;
        spell.Direction = GetAimDirection();
        CurrentSpellAmount--;
    }

    private bool CanCastSpell()
    {
        if (dashing)
            return false;
        return CurrentSpellAmount > 0;
    }

    private Chest SearchChest()
    {
        var other = Physics2D.OverlapCircleAll(transform.position, 1f, chestLayer);
        if (other.Length > 0)
            return other[0].GetComponent<Chest>();
        else
            return null;
    }
}
