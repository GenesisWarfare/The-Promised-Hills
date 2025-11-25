using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchPlayer : MonoBehaviour
{
    public GameObject playerUnitPrefab;
    public Transform spawnPoint;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                SpawnUnit();
            }
        }
    }

    void SpawnUnit()
    {
        if (playerUnitPrefab != null && spawnPoint != null)
            Instantiate(playerUnitPrefab, spawnPoint.position, Quaternion.identity);
    }
}
