using UnityEngine;
using UnityEngine.InputSystem;

public class MapDragger : MonoBehaviour
{
    [Header("Boundaries")]
    [SerializeField] private float minX = -3.5f;
    [SerializeField] private float maxX = 7f;
    [SerializeField] private float minY = -6f;
    [SerializeField] private float maxY = 7f;

    [Header("Level Selection Square")]
    [SerializeField] private GameObject levelSelectionSquare; // Assign the level selection square GameObject

    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private SpriteRenderer mapSpriteRenderer;
    private SettingsManager settingsManager;

    void Start()
    {
        mapSpriteRenderer = GetComponent<SpriteRenderer>();
        // Find SettingsManager to check if settings are open
        settingsManager = FindFirstObjectByType<SettingsManager>();
    }

    void Update()
    {
        // Find SettingsManager if not found yet
        if (settingsManager == null)
        {
            settingsManager = FindFirstObjectByType<SettingsManager>();
        }

        // Don't allow dragging if settings are open
        if (settingsManager != null && settingsManager.IsSettingsOpen())
        {
            isDragging = false; // Stop any ongoing drag
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
        worldMousePosition.z = transform.position.z;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Don't start dragging if settings are open
            if (settingsManager != null && settingsManager.IsSettingsOpen())
            {
                return;
            }

            // Check if click is on the map (not on level selection square)
            if (IsClickOnMap(worldMousePosition))
            {
                isDragging = true;
                lastMousePosition = worldMousePosition;
            }
        }

        if (isDragging)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 delta = worldMousePosition - lastMousePosition;
                Vector3 newPosition = transform.position + delta;

                // Clamp to boundaries
                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

                transform.position = newPosition;
                lastMousePosition = worldMousePosition;
            }
            else
            {
                isDragging = false;
            }
        }
    }

    bool IsClickOnMap(Vector3 worldPoint)
    {
        // Check if click is on level selection square first
        if (levelSelectionSquare != null)
        {
            Collider2D squareCollider = levelSelectionSquare.GetComponent<Collider2D>();
            if (squareCollider != null && squareCollider.OverlapPoint(worldPoint))
            {
                return false; // Click is on level selection square, don't drag
            }
        }

        // Check if click is on the map itself
        if (mapSpriteRenderer != null)
        {
            Bounds mapBounds = mapSpriteRenderer.bounds;
            return mapBounds.Contains(worldPoint);
        }

        // Fallback: check with collider
        Collider2D mapCollider = GetComponent<Collider2D>();
        if (mapCollider != null)
        {
            return mapCollider.OverlapPoint(worldPoint);
        }

        return false;
    }
}
