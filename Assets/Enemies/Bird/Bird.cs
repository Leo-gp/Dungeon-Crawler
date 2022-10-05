using UnityEngine;

public class Bird : Enemy
{
    [Header("Inner Attributes")]
    [SerializeField] private int fireBallSpeed = 0;

    [Header("References")]
    [SerializeField] private FireBall fireBallPrefab = null;

    protected override void OnTargetReached()
    {
        if ((player.transform.position - transform.position).sqrMagnitude > Mathf.Pow(ai.endReachedDistance, 2))
            return;

        if (CanAttack())
        {
            Attack();
        }
    }

    protected override void Attack()
    {
        audioSrc.PlayOneShot(attackSound);
        var spell = Instantiate(fireBallPrefab, transform.position, Quaternion.identity);
        spell.Speed = fireBallSpeed;
        spell.Damage = attackPower;
        spell.Direction = (player.transform.position - transform.position).normalized;
        lastAttackTime = Time.time;
    }
}
