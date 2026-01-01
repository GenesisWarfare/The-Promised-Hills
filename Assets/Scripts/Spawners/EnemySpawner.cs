using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Enemy spawner that automatically spawns enemies randomly in different lanes
 * No buttons needed - fully automatic
 */
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;

    [Header("Lane Spawn Points (for random spawning)")]
    [SerializeField] private Transform lane1SpawnPoint;
    [SerializeField] private Transform lane2SpawnPoint;
    [SerializeField] private Transform lane3SpawnPoint;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 3f;
    [SerializeField] private bool useRandomInterval = true;
    [SerializeField] private int maxActiveUnits = 5;

    private List<GameObject> activeUnits = new List<GameObject>();
    private List<Transform> availableSpawnPoints = new List<Transform>();

    void Start()
    {
        // Collect available spawn points
        if (lane1SpawnPoint != null) availableSpawnPoints.Add(lane1SpawnPoint);
        if (lane2SpawnPoint != null) availableSpawnPoints.Add(lane2SpawnPoint);
        if (lane3SpawnPoint != null) availableSpawnPoints.Add(lane3SpawnPoint);

        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("EnemySpawner: No spawn points assigned! Please assign at least one lane spawn point.");
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            CleanupDeadUnits();

            if (activeUnits.Count < maxActiveUnits && availableSpawnPoints.Count > 0)
            {
                // Randomly select a lane
                Transform randomSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
                GameObject spawnedUnit = Instantiate(enemyPrefab, randomSpawnPoint.position, Quaternion.identity);
                activeUnits.Add(spawnedUnit);
            }

            // Use random interval if enabled
            float waitTime = useRandomInterval
                ? Random.Range(minSpawnInterval, maxSpawnInterval)
                : spawnInterval;

            yield return new WaitForSeconds(waitTime);
        }
    }

    void CleanupDeadUnits()
    {
        activeUnits.RemoveAll(unit => unit == null);
    }
}
