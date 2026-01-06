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
    [Header("Footman Spawn Points")]
    [SerializeField] private Transform footmanLane1SpawnPoint;
    [SerializeField] private Transform footmanLane2SpawnPoint;
    [SerializeField] private Transform footmanLane3SpawnPoint;

    [Header("Archer Spawn Points")]
    [SerializeField] private Transform archerLane1SpawnPoint;
    [SerializeField] private Transform archerLane2SpawnPoint;
    [SerializeField] private Transform archerLane3SpawnPoint;

    [Header("Hero Spawn Points")]
    [SerializeField] private Transform heroLane1SpawnPoint;
    [SerializeField] private Transform heroLane2SpawnPoint;
    [SerializeField] private Transform heroLane3SpawnPoint;

    [Header("Unit Prefab")]
    [SerializeField] private GameObject playerUnitPrefab; // Default/fallback prefab
    private GameObject selectedUnitPrefab; // Currently selected unit prefab (set by UnitSelectionButton)

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

    /// <summary>
    /// Sets the selected unit prefab (called by UnitSelectionButton)
    /// </summary>
    public void SetSelectedUnitPrefab(GameObject prefab)
    {
        selectedUnitPrefab = prefab;
        Debug.Log($"PlayerLaneManager: Selected unit prefab set to {prefab.name}");
    }

    /// <summary>
    /// Gets the currently selected unit prefab, or falls back to default
    /// </summary>
    private GameObject GetSelectedUnitPrefab()
    {
        return selectedUnitPrefab != null ? selectedUnitPrefab : playerUnitPrefab;
    }

    public void SpawnInLane(int laneNumber)
    {
        GameObject unitPrefabToSpawn = GetSelectedUnitPrefab();
        if (unitPrefabToSpawn == null)
        {
            Debug.LogWarning($"Cannot spawn in lane {laneNumber}: unit prefab is null");
            return;
        }

        Transform spawnPoint = GetSpawnPointForUnit(unitPrefabToSpawn, laneNumber);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"Cannot spawn in lane {laneNumber}: spawnPoint is null for unit {unitPrefabToSpawn.name}");
            return;
        }

        // Get unit cost from prefab's Unit component
        Unit unitComponent = unitPrefabToSpawn.GetComponent<Unit>();
        int unitCost = unitComponent != null ? unitComponent.Cost : 0;

        // Check if player has enough money
        Player player = Player.Instance;
        if (player != null && !player.HasEnoughMoney(unitCost))
        {
            Debug.LogWarning($"PlayerLaneManager: Not enough money! Need {unitCost}, have {player.Money}");
            return;
        }

        // Spend money
        if (player != null && unitCost > 0)
        {
            player.SpendMoney(unitCost);
        }

        // Spawn the unit at spawn point position (including z position)
        Vector3 spawnPosition = spawnPoint.position;
        GameObject spawnedUnit = Instantiate(unitPrefabToSpawn, spawnPosition, Quaternion.identity);

        // Ensure z position matches spawn point
        Vector3 unitPos = spawnedUnit.transform.position;
        unitPos.z = spawnPosition.z;
        spawnedUnit.transform.position = unitPos;

        Debug.Log($"Spawned player unit in lane {laneNumber} at z={spawnPosition.z} (cost: {unitCost})");
    }

    /// <summary>
    /// Gets the spawn point for a specific unit type and lane
    /// </summary>
    private Transform GetSpawnPointForUnit(GameObject unitPrefab, int laneNumber)
    {
        if (unitPrefab == null) return null;

        string unitName = unitPrefab.name.ToLower();
        Transform spawnPoint = null;

        // Determine which spawn points to use based on unit type
        if (unitName.Contains("hero"))
        {
            switch (laneNumber)
            {
                case 1:
                    spawnPoint = heroLane1SpawnPoint;
                    break;
                case 2:
                    spawnPoint = heroLane2SpawnPoint;
                    break;
                case 3:
                    spawnPoint = heroLane3SpawnPoint;
                    break;
            }
        }
        else if (unitName.Contains("archer"))
        {
            switch (laneNumber)
            {
                case 1:
                    spawnPoint = archerLane1SpawnPoint;
                    break;
                case 2:
                    spawnPoint = archerLane2SpawnPoint;
                    break;
                case 3:
                    spawnPoint = archerLane3SpawnPoint;
                    break;
            }
        }
        else if (unitName.Contains("footman"))
        {
            switch (laneNumber)
            {
                case 1:
                    spawnPoint = footmanLane1SpawnPoint;
                    break;
                case 2:
                    spawnPoint = footmanLane2SpawnPoint;
                    break;
                case 3:
                    spawnPoint = footmanLane3SpawnPoint;
                    break;
            }
        }
        else
        {
            // Fallback: try to use hero spawn points if unit type not recognized
            Debug.LogWarning($"PlayerLaneManager: Unit type not recognized ({unitPrefab.name}), using hero spawn points as fallback");
            switch (laneNumber)
            {
                case 1:
                    spawnPoint = heroLane1SpawnPoint;
                    break;
                case 2:
                    spawnPoint = heroLane2SpawnPoint;
                    break;
                case 3:
                    spawnPoint = heroLane3SpawnPoint;
                    break;
            }
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"PlayerLaneManager: No spawn point found for {unitPrefab.name} in lane {laneNumber}");
        }

        return spawnPoint;
    }
}
