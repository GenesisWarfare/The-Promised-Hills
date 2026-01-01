using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * Manages player unit spawning via keyboard shortcuts
 * Press 1, 2, or 3 to spawn units in lanes 1, 2, or 3
 */
public class PlayerLaneManager : MonoBehaviour
{
    [Header("Lane Spawn Points")]
    [SerializeField] private Transform lane1SpawnPoint;
    [SerializeField] private Transform lane2SpawnPoint;
    [SerializeField] private Transform lane3SpawnPoint;

    [Header("Unit Prefab")]
    [SerializeField] private GameObject playerUnitPrefab;

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            SpawnInLane(1);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            SpawnInLane(2);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            SpawnInLane(3);
        }
#else
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnInLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnInLane(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnInLane(3);
        }
#endif
    }

    public void SpawnInLane(int laneNumber)
    {
        Transform spawnPoint = GetSpawnPointForLane(laneNumber);
        if (spawnPoint != null && playerUnitPrefab != null)
        {
            Instantiate(playerUnitPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log($"Spawned player unit in lane {laneNumber}");
        }
        else
        {
            Debug.LogWarning($"Cannot spawn in lane {laneNumber}: spawnPoint or prefab is null");
        }
    }

    private Transform GetSpawnPointForLane(int laneNumber)
    {
        switch (laneNumber)
        {
            case 1:
                return lane1SpawnPoint;
            case 2:
                return lane2SpawnPoint;
            case 3:
                return lane3SpawnPoint;
            default:
                Debug.LogWarning($"Invalid lane number: {laneNumber}");
                return null;
        }
    }
}
