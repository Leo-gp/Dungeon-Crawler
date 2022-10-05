using UnityEngine;

public class CrystalBall : Spell
{
    protected override void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag(gc.EnemyTag))
        {
            var enemy = col.gameObject.GetComponent<Enemy>();
            enemy.Hit(Damage);
        }
        Destroy(gameObject);
    }
}
