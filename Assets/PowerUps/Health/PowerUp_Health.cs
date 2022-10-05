using UnityEngine;

public class PowerUp_Health : PowerUp
{
    public override void Collect()
    {
        player.CurrentHealth += Mathf.FloorToInt(EffectValue);
        audioSrc.PlayOneShot(collectSound);
        sr.enabled = false;
        col.enabled = false;
        Destroy(gameObject, 1);
        InvokeCollectedEvent(this);
    }
}