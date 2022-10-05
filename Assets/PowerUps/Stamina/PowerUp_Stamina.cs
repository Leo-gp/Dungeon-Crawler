using UnityEngine;

public class PowerUp_Stamina : PowerUp
{
    public override void Collect()
    {
        player.CurrentStamina += Mathf.FloorToInt(EffectValue);
        audioSrc.PlayOneShot(collectSound);
        sr.enabled = false;
        col.enabled = false;
        Destroy(gameObject, 1);
        InvokeCollectedEvent(this);
    }
}
