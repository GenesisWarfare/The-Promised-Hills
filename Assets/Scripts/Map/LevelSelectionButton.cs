using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectionButton : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private SpriteRenderer spriteRenderer;
    private LevelManager levelManager;

    void Start()
    {
        Debug.Log($"[LevelSelectionButton] Start() - GameObject: '{gameObject.name}', levelNumber: {levelNumber}");
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
            Debug.Log($"[LevelSelectionButton] '{gameObject.name}' - SpriteRenderer found and color set");
        }
        else
        {
            Debug.LogWarning($"[LevelSelectionButton] '{gameObject.name}' - No SpriteRenderer found!");
        }

        levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            Debug.Log($"[LevelSelectionButton] '{gameObject.name}' - LevelManager found");
        }
        else
        {
            Debug.LogError($"[LevelSelectionButton] '{gameObject.name}' - LevelManager NOT FOUND!");
        }
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                Debug.Log($"[LevelSelectionButton] '{gameObject.name}' - Mouse click detected on button!");
                SelectLevel();
            }
        }

        // Visual feedback on hover
        if (spriteRenderer != null)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                spriteRenderer.color = hoverColor;
            }
            else
            {
                spriteRenderer.color = normalColor;
            }
        }
    }

    void SelectLevel()
    {
        Debug.Log($"=== LevelSelectionButton.SelectLevel() - '{gameObject.name}' clicked ===");
        Debug.Log($"[LevelSelectionButton] levelNumber: {levelNumber}, levelManager: {(levelManager != null ? "found" : "NULL")}");

        if (levelManager != null)
        {
            Debug.Log($"[LevelSelectionButton] Calling levelManager.SelectLevel({levelNumber})");
            levelManager.SelectLevel(levelNumber);
        }
        else
        {
            Debug.LogError($"[LevelSelectionButton] '{gameObject.name}' - LevelManager is NULL! Cannot select level. Trying to find it again...");
            // Try to find it again
            levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                Debug.Log($"[LevelSelectionButton] Found LevelManager on retry, calling SelectLevel({levelNumber})");
                levelManager.SelectLevel(levelNumber);
            }
            else
            {
                Debug.LogError($"[LevelSelectionButton] '{gameObject.name}' - LevelManager still not found after retry!");
            }
        }
    }

    public void SetSelected(bool selected)
    {
        // No visual feedback for selection - just normal/hover colors
        // This method exists for compatibility but does nothing visual
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }
}
