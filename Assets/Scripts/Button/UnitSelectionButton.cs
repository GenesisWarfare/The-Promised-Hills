using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * Unit Selection Button - Click to select which unit type to spawn
 * Attach this to a GameObject with a SpriteRenderer and Collider2D
 * When clicked, sets the selected unit type in PlayerLaneManager
 * Then pressing 1, 2, or 3 will spawn the selected unit type
 */
public class UnitSelectionButton : MonoBehaviour
{
    [Header("Unit Prefab")]
    [SerializeField] private GameObject unitPrefab; // The unit prefab this button represents (Hero, Archer, Footman)

    [Header("Visual Feedback")]
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f, 1f); // Yellow tint when selected
    [SerializeField] private Color normalColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private static UnitSelectionButton currentlySelectedButton;

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
    }

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
                OnButtonClicked();
            }
        }
    }

    private void OnButtonClicked()
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning($"UnitSelectionButton: No unit prefab assigned to button {gameObject.name}!");
            return;
        }

        // Find PlayerLaneManager in the scene
        PlayerLaneManager laneManager = FindFirstObjectByType<PlayerLaneManager>();
        if (laneManager == null)
        {
            Debug.LogWarning("UnitSelectionButton: Could not find PlayerLaneManager in scene!");
            return;
        }

        // Set the selected unit prefab
        laneManager.SetSelectedUnitPrefab(unitPrefab);

        // Visual feedback: highlight this button, unhighlight others
        if (currentlySelectedButton != null && currentlySelectedButton != this)
        {
            if (currentlySelectedButton.spriteRenderer != null)
            {
                currentlySelectedButton.spriteRenderer.color = currentlySelectedButton.normalColor;
            }
        }

        currentlySelectedButton = this;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = selectedColor;
        }

        Debug.Log($"UnitSelectionButton: Selected unit type: {unitPrefab.name}");
    }
}
