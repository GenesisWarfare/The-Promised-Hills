using UnityEngine;

/**
 * Unit Selection Button - Drag and drop to spawn units
 * Press and hold the button, then drag to a lane (1, 2, or 3) and release to spawn
 * Works on both desktop (mouse) and mobile (touch)
 */
public class UnitSelectionButton : MonoBehaviour
{
    [Header("Unit Prefab")]
    [SerializeField] private GameObject unitPrefab; // The unit prefab this button represents (Hero, Archer, Footman)

    [Header("Visual Feedback")]
    [SerializeField] private Color draggingColor = new Color(1f, 1f, 0.5f, 1f); // Yellow tint when dragging
    [SerializeField] private Color normalColor = Color.white;

    [Header("Drag Preview")]
    [SerializeField] private Sprite dragPreviewSprite; // Sprite to show while dragging (optional - uses unit prefab sprite if not set)
    [SerializeField] private float dragPreviewScale = 1f; // Scale of the drag preview sprite

    private SpriteRenderer spriteRenderer;
    private PlayerLaneManager laneManager;
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private int currentHoveredLaneNumber = 0; // 0 = no lane, 1-3 = lane number
    private GameObject dragPreviewObject; // The sprite that follows the cursor/finger

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        else
        {
            Debug.LogWarning($"UnitSelectionButton: No SpriteRenderer found on {gameObject.name}! Add a SpriteRenderer component.");
        }

        // Find PlayerLaneManager
        laneManager = FindFirstObjectByType<PlayerLaneManager>();
        if (laneManager == null)
        {
            Debug.LogWarning("UnitSelectionButton: Could not find PlayerLaneManager in scene!");
        }
    }

    void Update()
    {
        Vector2 inputPos = MobileInputHelper.GetPosition();
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPos);

        // Check if button was pressed
        if (MobileInputHelper.WasPressedThisFrame())
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                StartDrag();
            }
        }

        // Handle dragging
        if (isDragging)
        {
            // Update input position for drag preview (cursor/finger may have moved)
            inputPos = MobileInputHelper.GetPosition();
            
            // Update drag preview position to follow cursor/finger
            if (dragPreviewObject != null)
            {
                Vector3 previewWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(inputPos.x, inputPos.y, Camera.main.nearClipPlane + 1f));
                previewWorldPos.z = -5f; // Keep in front
                dragPreviewObject.transform.position = previewWorldPos;
            }
            
            // Update world point for lane detection
            worldPoint = Camera.main.ScreenToWorldPoint(inputPos);

            // Check which lane we're hovering over based on Y position
            int hoveredLane = GetLaneAtPosition(worldPoint);
            
            // Update highlight (optional - can add visual feedback here if needed)
            if (hoveredLane != currentHoveredLaneNumber)
            {
                currentHoveredLaneNumber = hoveredLane;
                // You can add visual feedback here if you want to highlight lanes
            }

            // Check if released
            if (MobileInputHelper.WasReleasedThisFrame())
            {
                EndDrag(worldPoint);
            }
        }
    }

    private void StartDrag()
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning($"UnitSelectionButton: No unit prefab assigned to button {gameObject.name}!");
            return;
        }

        if (laneManager == null)
        {
            Debug.LogWarning("UnitSelectionButton: PlayerLaneManager not found!");
            return;
        }

        isDragging = true;
        dragStartPosition = MobileInputHelper.GetPosition();

        // Visual feedback on button
        if (spriteRenderer != null)
        {
            spriteRenderer.color = draggingColor;
        }

        // Create drag preview sprite that follows cursor/finger
        CreateDragPreview();

        Debug.Log($"UnitSelectionButton: Started dragging {unitPrefab.name}");
    }

    private void CreateDragPreview()
    {
        // Destroy existing preview if any
        if (dragPreviewObject != null)
        {
            Destroy(dragPreviewObject);
        }

        // Create new GameObject for drag preview
        dragPreviewObject = new GameObject("DragPreview_" + unitPrefab.name);
        dragPreviewObject.transform.SetParent(null); // Don't parent to anything

        // Add SpriteRenderer
        SpriteRenderer previewRenderer = dragPreviewObject.AddComponent<SpriteRenderer>();

        // Get sprite to use
        Sprite spriteToUse = dragPreviewSprite;
        if (spriteToUse == null)
        {
            // Try to get sprite from unit prefab
            SpriteRenderer unitSpriteRenderer = unitPrefab.GetComponent<SpriteRenderer>();
            if (unitSpriteRenderer != null)
            {
                spriteToUse = unitSpriteRenderer.sprite;
            }
        }

        if (spriteToUse != null)
        {
            previewRenderer.sprite = spriteToUse;
        }
        else
        {
            // Fallback: create a simple colored square
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            spriteToUse = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            previewRenderer.sprite = spriteToUse;
        }

        // Set properties
        previewRenderer.sortingOrder = 1000; // Render on top
        previewRenderer.color = new Color(1f, 1f, 1f, 0.8f); // Slightly transparent

        // Set initial position
        Vector2 previewInputPos = MobileInputHelper.GetPosition();
        Vector3 previewWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(previewInputPos.x, previewInputPos.y, Camera.main.nearClipPlane + 1f));
        previewWorldPos.z = -5f; // In front of most things
        dragPreviewObject.transform.position = previewWorldPos;
        dragPreviewObject.transform.localScale = Vector3.one * dragPreviewScale;
    }

    private void EndDrag(Vector2 worldPoint)
    {
        isDragging = false;

        // Destroy drag preview
        if (dragPreviewObject != null)
        {
            Destroy(dragPreviewObject);
            dragPreviewObject = null;
        }

        // Reset visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        // Check if dropped on a valid lane based on Y position proximity
        int laneNumber = GetLaneAtPosition(worldPoint);
        if (laneNumber > 0 && laneManager != null)
        {
            // Spawn unit in the lane
            laneManager.SetSelectedUnitPrefab(unitPrefab);
            laneManager.SpawnInLane(laneNumber);
            Debug.Log($"UnitSelectionButton: Dropped {unitPrefab.name} in lane {laneNumber}");
        }
        else
        {
            Debug.Log($"UnitSelectionButton: Drag ended but not dropped on a valid lane (worldPos: {worldPoint})");
        }

        // Reset hovered lane
        currentHoveredLaneNumber = 0;
    }

    void OnDestroy()
    {
        // Clean up drag preview if still exists
        if (dragPreviewObject != null)
        {
            Destroy(dragPreviewObject);
        }
    }

    /// <summary>
    /// Gets the lane number (1, 2, or 3) at the given world position based on Y proximity to spawn points
    /// Returns 0 if not close enough to any lane
    /// </summary>
    private int GetLaneAtPosition(Vector2 worldPoint)
    {
        if (laneManager == null || unitPrefab == null)
        {
            return 0;
        }

        return laneManager.GetLaneAtPosition(worldPoint, unitPrefab);
    }
}
