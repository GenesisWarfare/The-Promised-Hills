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

    void OnEnable()
    {
        // Refresh visual state when object becomes active (e.g., returning to map scene)
        if (spriteRenderer != null)
        {
            // Start coroutine to check unlock status
            StartCoroutine(CheckUnlockStatusAfterDelay());
        }
    }

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

        // Initialize progress manager
        progressManager = BattleProgressManager.Instance;

        // Wait for BattleProgressManager to load data, then check unlock status
        StartCoroutine(CheckUnlockStatusAfterDelay());
    }

    /**
     * Wait for BattleProgressManager to load progress, then check unlock status
     */
    System.Collections.IEnumerator CheckUnlockStatusAfterDelay()
    {
        // Wait for BattleProgressManager to be available
        if (progressManager == null)
        {
            progressManager = BattleProgressManager.Instance;
        }
        
        if (progressManager != null)
        {
            // Wait for progress to be loaded (important for first-time map load)
            int maxWaitTime = 50; // 5 seconds max wait
            int waited = 0;
            while (!progressManager.IsProgressLoaded && waited < maxWaitTime)
            {
                yield return new WaitForSeconds(0.1f);
                waited++;
            }
            
            if (progressManager.IsProgressLoaded)
            {
                Debug.Log($"[LevelSelectionButton] '{gameObject.name}' - Progress loaded, updating visual state");
            }
        }
        
        // Update unlock status and visual
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
        // Refresh unlock status from progress manager
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
        
        if (spriteRenderer != null)
        {
            if (isUnlocked)
            {
                spriteRenderer.color = normalColor;
                Debug.Log($"[LevelSelectionButton] '{gameObject.name}' - Kingdom {levelNumber} is UNLOCKED (normal color)");
            }
            else
            {
                spriteRenderer.color = lockedColor;
                Debug.Log($"[LevelSelectionButton] '{gameObject.name}' - Kingdom {levelNumber} is LOCKED (gray color)");
            }
        }
    }

    /**
     * Public method to refresh visual state (called by LevelManager)
     */
    public void RefreshVisualState()
    {
        UpdateVisualState();
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
