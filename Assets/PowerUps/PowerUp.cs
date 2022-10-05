using System;
using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float effectValue;
    public float EffectValue { get { return effectValue; } private set { effectValue = value; } }

    [Header("Settings")]
    [SerializeField] private int chanceToSpawn;
    public int ChanceToSpawn { get { return chanceToSpawn; } private set { chanceToSpawn = value; } }

    [Header("References")]
    [SerializeField] protected AudioClip collectSound = null;

    // Events
    public static event Action<PowerUp> collectedEvent;

    // References
    protected Player player;
    protected SpriteRenderer sr;
    protected Collider2D col;
    protected AudioSource audioSrc;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        audioSrc = GetComponent<AudioSource>();
    }

    public abstract void Collect();

    protected void InvokeCollectedEvent(PowerUp powerUp)
    {
        collectedEvent?.Invoke(powerUp);
    }
}
