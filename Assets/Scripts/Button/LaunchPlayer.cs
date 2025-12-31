using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * Launcher - Click to spawn player units
 * Attach this to a GameObject with a Collider2D (can be trigger)
 * Click on it to spawn units at the spawn point
 */
public class LaunchPlayer : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject playerUnitPrefab;
    public Transform spawnPoint;

    void Update()
    {
        bool mouseClicked = false;
        Vector2 mousePos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseClicked = true;
            mousePos = Mouse.current.position.ReadValue();
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            mouseClicked = true;
            mousePos = Input.mousePosition;
        }
#endif

        if (mouseClicked)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);

            if (hit != null && hit.gameObject == gameObject)
            {
                SpawnUnit();
            }
        }
    }

    public void SpawnUnit()
    {
        if (playerUnitPrefab != null && spawnPoint != null)
        {
            Instantiate(playerUnitPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
