using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private EnemyAI enemyPrefab;

    [SerializeField]
    private float spawnInterval;

    [SerializeField]
    private int maxEnemiesNumber;

    [SerializeField]
    private Player player;

    private List<EnemyAI> spawnEnemies = new List<EnemyAI>();

    private float timeSinceLastSpawn;

    public float patrolInterval; //巡逻间隔

    private float sumTime;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceLastSpawn = spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        sumTime += Time.deltaTime;
        if (timeSinceLastSpawn > spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            if (spawnEnemies.Count < maxEnemiesNumber)
            {
                SpawnEnemy();
            }
        }
        if (sumTime > patrolInterval)
        {
            Patrol();
        }
    }

    void SpawnEnemy()
    {
        EnemyAI enemy =
            Instantiate(enemyPrefab, transform.position, transform.rotation);

        // int spawnPointIndex = spawnEnemies.Count % spawnPoints.Length;
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        enemy.Init(player, spawnPoints[spawnPointIndex]);
        spawnEnemies.Add (enemy);
    }

    void Patrol()
    {
        foreach (var enemy in spawnEnemies)
        {
            enemy
                .Init(player, spawnPoints[Random.Range(0, spawnPoints.Length)]);
        }
    }
}
