using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 2f;
    [SerializeField] private int maxActiveUnits = 5;

    private List<GameObject> activeUnits = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            CleanupDeadUnits();

            if (activeUnits.Count < maxActiveUnits)
            {
                GameObject spawnedUnit = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                activeUnits.Add(spawnedUnit);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void CleanupDeadUnits()
    {
        activeUnits.RemoveAll(unit => unit == null);
    }
}
