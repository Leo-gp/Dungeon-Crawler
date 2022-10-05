using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Sprite openedChest = null;
    //[SerializeField] private List<Enemy> enemiesToUnlock = null;
    [SerializeField] private AudioClip collectKeySound = null;

    // State control
    private bool opened;

    // References
    private Player player;
    private SpriteRenderer sr;
    private AudioSource audioSrc;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        sr = GetComponent<SpriteRenderer>();
        audioSrc = GetComponent<AudioSource>();
        //Enemy.enemyKilledEvent += RemoveEnemyToUnlock;
    }

    /*void OnDisable()
    {
        Enemy.enemyKilledEvent -= RemoveEnemyToUnlock;
    }*/

    public void Open()
    {
        if (CanOpen())
        {
            sr.sprite = openedChest;
            player.CurrentKeysAmount++;
            audioSrc.PlayOneShot(collectKeySound);
            opened = true;
        }
    }

    private bool CanOpen()
    {
        if (!opened /*&& enemiesToUnlock.Count == 0*/)
            return true;
        else
            return false;
    }

    /*private void RemoveEnemyToUnlock(Enemy enemy)
    {
        if (enemiesToUnlock.Contains(enemy))
            enemiesToUnlock.Remove(enemy);
    }*/
}
