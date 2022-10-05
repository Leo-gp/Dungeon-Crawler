using UnityEngine;

public class Minotaur : Enemy
{
    [Header("Inner Attributes")]
    [SerializeField] private float attackRadius = 0;

    // Read-only variables
    private readonly string anim_bool_Moving = "Moving";
    protected readonly string anim_trigger_Attack = "Attack";

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRadius);    
    }

    void Update()
    {
        moving = ai.canMove && (Mathf.Abs(ai.velocity.x) > 0.1f || Mathf.Abs(ai.velocity.y) > 0.1f);

        anim.SetBool(anim_bool_Moving, moving);

        if ((facingRight && transform.localScale.x < 0) || (!facingRight && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        if (ai.velocity.x != 0)
        {
            facingRight = ai.velocity.x > 0;
        }
    }

    protected override void OnTargetReached()
    {
        if ((player.transform.position - transform.position).sqrMagnitude > Mathf.Pow(ai.endReachedDistance, 2))
            return;

        if (CanAttack())
        {
            anim.SetTrigger(anim_trigger_Attack); // This animation will call the attack method
        }
    }

    protected override void Attack()
    {
        audioSrc.PlayOneShot(attackSound);

        var col = Physics2D.OverlapCircleAll(transform.position, attackRadius, playerLayer);
        if (col.Length > 0)
        {
            player.Hit(attackPower);
        }

        lastAttackTime = Time.time;
    }

    public override void Hit(int amount)
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
}
