using UnityEngine;

public class FireBall : Spell
{
    protected override void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag(gc.PlayerTag))
        {
            var player = col.gameObject.GetComponent<Player>();
            player.Hit(Damage);
        }
        Destroy(gameObject);
    }
}
