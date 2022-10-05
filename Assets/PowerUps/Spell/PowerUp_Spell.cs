using UnityEngine;

public class PowerUp_Spell : PowerUp
{
    public override void Collect()
    {
        player.CurrentSpellAmount += Mathf.FloorToInt(EffectValue);
        audioSrc.PlayOneShot(collectSound);
        sr.enabled = false;
        col.enabled = false;
        Destroy(gameObject, 1);
        InvokeCollectedEvent(this);
    }
}