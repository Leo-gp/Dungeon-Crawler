using System.Collections;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PowerUp[] powerUpsPrefabs;
    [SerializeField] private Enemy[] enemiesPrefabs;
    [Header("Settings")]
    [SerializeField] private Vector2[] powerUpSpawnPositions;
    [SerializeField] private Vector2[] enemySpawnPositions;
    [SerializeField] private int maxSpawnedEnemies;
    [SerializeField] private float powerUpMinSpawnInterval;
    [SerializeField] private float powerUpMaxSpawnInterval;
    [SerializeField] private float enemyMinSpawnInterval;
    [SerializeField] private float enemyMaxSpawnInterval;

    // State control
    public PowerUp[] PowerUpsOccupiedPositions { get; private set; }
    public int SpawnedEnemies { get; private set; }

    // Hierarchy organization
    private GameObject powerUpsParentObj;
    private GameObject enemiesParentObj;

    void Awake()
    {
        PowerUp.collectedEvent += DisoccupyPosition;
        PowerUpsOccupiedPositions = new PowerUp[powerUpSpawnPositions.Length];
        powerUpsParentObj = new GameObject("PowerUps");
        powerUpsParentObj.transform.SetParent(this.transform);
        enemiesParentObj = new GameObject("Enemies");
        enemiesParentObj.transform.SetParent(this.transform);
        StartCoroutine(PowerUpSpawner());
        StartCoroutine(EnemySpawner());
        Enemy.enemyKilledEvent += DecreaseSpawnedEnemiesCount;
    }

    private void DecreaseSpawnedEnemiesCount(Enemy obj)
    {
        SpawnedEnemies--;
    }

    private IEnumerator PowerUpSpawner()
    {
        while (true)
        {
            int spawnPosRand = 0;
            do
            {
                yield return null;
                spawnPosRand = Random.Range(0, powerUpSpawnPositions.Length);
            } while (PowerUpsOccupiedPositions[spawnPosRand] != null);
            yield return new WaitForSeconds(Random.Range(powerUpMinSpawnInterval, powerUpMaxSpawnInterval));
            var powerUp = GetRandomPowerUp();
            var spawnPosition = powerUpSpawnPositions[spawnPosRand];
            var pos = new Vector3(spawnPosition.x, spawnPosition.y, powerUp.transform.position.z);
            var pwrUp = Instantiate(powerUp, pos, Quaternion.identity, powerUpsParentObj.transform);
            PowerUpsOccupiedPositions[spawnPosRand] = pwrUp;
        }
    }

    private IEnumerator EnemySpawner()
    {
        while (true)
        {
            if (SpawnedEnemies >= maxSpawnedEnemies)
            {
                yield return null;
                continue;
            }
            yield return new WaitForSeconds(Random.Range(enemyMinSpawnInterval, enemyMaxSpawnInterval + SpawnedEnemies));
            var enemy = GetRandomEnemy();
            var spawnPosition = enemySpawnPositions[Random.Range(0, enemySpawnPositions.Length)];
            var pos = new Vector3(spawnPosition.x, spawnPosition.y, enemy.transform.position.z);
            Instantiate(enemy, pos, Quaternion.identity, enemiesParentObj.transform);
            SpawnedEnemies++;
        }
    }

    private PowerUp GetRandomPowerUp()
    {
        int totalChance = 0;
        foreach (var powerUp in powerUpsPrefabs)
        {
            totalChance += powerUp.ChanceToSpawn;
        }
        int rand = Random.Range(1, totalChance + 1);
        int count = 0;
        for (int i = 0; i < powerUpsPrefabs.Length; i++)
        {
            if (powerUpsPrefabs[i].ChanceToSpawn >= rand - count)
            {
                return powerUpsPrefabs[i];
            }
            else
            {
                count += powerUpsPrefabs[i].ChanceToSpawn;
            }
        }
        return null;
    }

    private Enemy GetRandomEnemy()
    {
        return enemiesPrefabs[Random.Range(0, enemiesPrefabs.Length)];
    }

    private void DisoccupyPosition(PowerUp powerUp)
    {
        for (int i = 0; i < PowerUpsOccupiedPositions.Length; i++)
        {
            if (PowerUpsOccupiedPositions[i] == powerUp)
            {
                PowerUpsOccupiedPositions[i] = null;
                break;
            }
        }
    }
}
