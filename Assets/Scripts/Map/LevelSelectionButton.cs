using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectionButton : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color lockedColor = Color.gray; // Color when kingdom is locked

    private SpriteRenderer spriteRenderer;
    private LevelManager levelManager;
    private BattleProgressManager progressManager;
    private bool isUnlocked = false;

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

        // Wait a frame for BattleProgressManager to load data, then check unlock status
        StartCoroutine(CheckUnlockStatusAfterDelay());
    }

    /**
     * Wait a moment for BattleProgressManager to load, then check unlock status
     */
    System.Collections.IEnumerator CheckUnlockStatusAfterDelay()
    {
        yield return null; // Wait one frame
        yield return null; // Wait another frame for cloud save to load
        
        if (progressManager == null)
        {
            progressManager = BattleProgressManager.Instance;
        }
        
        if (progressManager != null)
        {
            isUnlocked = progressManager.IsKingdomUnlocked(levelNumber);
        }
        else
        {
            // If no progress manager, assume level 1 is unlocked
            isUnlocked = (levelNumber == 1);
        }
        
        UpdateVisualState();
    }

    /**
     * Update visual state based on unlock status
     */
    void UpdateVisualState()
    {
        if (spriteRenderer != null)
        {
            if (isUnlocked)
            {
                spriteRenderer.color = normalColor;
            }
            else
            {
                spriteRenderer.color = lockedColor;
            }
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

        // Visual feedback on hover (only if unlocked)
        if (spriteRenderer != null)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                if (isUnlocked)
                {
                    spriteRenderer.color = hoverColor;
                }
                else
                {
                    spriteRenderer.color = lockedColor;
                }
            }
            else
            {
                if (isUnlocked)
                {
                    spriteRenderer.color = normalColor;
                }
                else
                {
                    spriteRenderer.color = lockedColor;
                }
            }
        }
    }

    void SelectLevel()
    {
        // Check if kingdom is unlocked before allowing selection
        if (progressManager != null)
        {
            isUnlocked = progressManager.IsKingdomUnlocked(levelNumber);
        }

        if (!isUnlocked)
        {
            Debug.LogWarning($"[LevelSelectionButton] '{gameObject.name}' - Kingdom {levelNumber} is locked! Cannot select.");
            return;
        }

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
