using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject health = null;
    [SerializeField] private GameObject healthIconPrefab = null;
    [SerializeField] private Image staminaBar = null;
    [SerializeField] private TextMeshProUGUI spellAmount = null;
    [SerializeField] private TextMeshProUGUI keysAmount = null;

    // State control
    private List<GameObject> healthIcons;

    // References
    private Player player;
    private GameController gc;

    void Awake()
    {
        healthIcons = new List<GameObject>();
        player = FindObjectOfType<Player>();
        gc = FindObjectOfType<GameController>();
        Player.healthChangedEvent += UpdateHealth;
        Player.staminaChangedEvent += UpdateStamina;
        Player.spellChangedEvent += UpdateSpellAmount;
        Player.keyCollectedEvent += UpdateKeysAmount;
    }

    void OnDisable()
    {
        Player.healthChangedEvent -= UpdateHealth;
        Player.staminaChangedEvent -= UpdateStamina;
        Player.spellChangedEvent -= UpdateSpellAmount;
        Player.keyCollectedEvent -= UpdateKeysAmount;
    }

    private void UpdateHealth()
    {
        int diff = player.CurrentHealth - healthIcons.Count;

        if (diff < 0)
        {
            for (int i = 0; i > diff; i--)
            {
                var item = healthIcons[healthIcons.Count - 1];
                healthIcons.Remove(item);
                Destroy(item);
            }
        }
        else if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                var item = Instantiate(healthIconPrefab, health.transform);
                healthIcons.Add(item);
            }
        }
    }

    private void UpdateStamina()
    {
        staminaBar.fillAmount = Mathf.Clamp01(player.CurrentStamina / player.MaxStamina);
    }

    private void UpdateSpellAmount()
    {
        spellAmount.text = "x" + player.CurrentSpellAmount.ToString();
    }

    private void UpdateKeysAmount()
    {
        keysAmount.text = player.CurrentKeysAmount.ToString() + "/" + gc.KeysToFinish;
    }
}
