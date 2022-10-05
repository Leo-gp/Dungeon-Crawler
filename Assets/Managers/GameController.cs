using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameOverScreen = null;
    [SerializeField] private GameObject victoryScreen = null;
    [SerializeField] private GameObject pauseScreen = null;

    [Header("Settings")]
    [SerializeField] private int keysToFinish = 0;
    public int KeysToFinish { get { return keysToFinish; } }

    [Header("References")]
    [SerializeField] private AudioClip victorySound = null;
    [SerializeField] private AudioClip gameOverSound = null;

    // State control
    private bool paused;

    // Read-only variables
    public string PlayerTag { get { return "Player"; } }
    public string EnemyTag { get { return "Enemy"; } }
    public string PowerUpTag { get { return "PowerUp"; } }

    // References
    private Player player;
    private AudioSource audioSrc;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        audioSrc = GetComponent<AudioSource>();
        Player.keyCollectedEvent += AttemptVictoryCall;
        Time.timeScale = 1f;
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
    }

    void OnDisable()
    {
        Player.keyCollectedEvent -= AttemptVictoryCall;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void AttemptVictoryCall()
    {
        if (player.CurrentKeysAmount >= KeysToFinish)
        {
            Victory();
        }
    }

    private void Victory()
    {
        audioSrc.PlayOneShot(victorySound);
        Time.timeScale = 0f;
        victoryScreen.SetActive(true);
    }

    public void GameOver()
    {
        audioSrc.PlayOneShot(gameOverSound);
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);
    }

    public void TogglePause()
    {
        if (paused)
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;
            EnableAllCharacters();
            paused = false;
        }
        else
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;
            DisableAllCharacters();
            paused = true;
        }
    }

    private void EnableAllCharacters()
    {
        player.enabled = true;
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            enemy.enabled = true;
        }
    }

    private void DisableAllCharacters()
    {
        player.enabled = false;
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            enemy.enabled = false;
        }
    }
}
