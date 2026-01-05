using UnityEngine;

/**
 * Special Attack Plane - Moves from left to right across the screen
 * Destroys itself when it reaches the right edge
 */
public class SpecialAttackPlane : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Visual Settings")]
    [SerializeField] private bool destroyOnExit = true; // Destroy when leaving screen

    private Camera mainCamera;
    private float rightEdge;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        // Calculate right edge of screen
        if (mainCamera != null)
        {
            rightEdge = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, mainCamera.nearClipPlane)).x;
        }
        else
        {
            // Fallback: use a large value
            rightEdge = 50f;
        }
    }

    void Update()
    {
        // Move right (maintain z = -2)
        Vector3 newPosition = transform.position + Vector3.right * moveSpeed * Time.deltaTime;
        newPosition.z = -2f;
        transform.position = newPosition;

        // Check if we've passed the right edge
        if (transform.position.x > rightEdge + 2f)
        {
            if (destroyOnExit)
            {
                Destroy(gameObject);
            }
        }
    }

}
