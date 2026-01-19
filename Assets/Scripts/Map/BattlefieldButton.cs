using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BattlefieldButton : MonoBehaviour
{
    [Header("Battlefield Info")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string sceneName = "";
    [SerializeField] private string battlefieldName = ""; // e.g., "Ella Valley"
    [SerializeField] private string kingdomState = ""; // e.g., "Kingdom of Judah"
    [TextArea(5, 10)]
    [SerializeField] private string detailedInfo = ""; // Detailed information about the battlefield and kingdom

    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Sprite flagSprite; // Flag sprite to show when battle is won

    private SpriteRenderer spriteRenderer;
    private LevelManager levelManager;
    private BattlefieldInfoManager infoManager;
    private BattleProgressManager progressManager;
    private bool isWon = false;
    private Sprite originalSprite; // Store the original sprite to restore if needed

    void Awake()
    {
        // Initialize spriteRenderer early
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Store the original sprite immediately in Awake (before any changes)
            originalSprite = spriteRenderer.sprite;
        }
    }

    void OnEnable()
    {
        // Refresh visual state when object becomes active (e.g., returning to map scene)
        if (spriteRenderer != null && originalSprite == null)
        {
            // If original sprite wasn't captured, capture it now
            originalSprite = spriteRenderer.sprite;
        }

        // Refresh the win status visual when enabled
        StartCoroutine(CheckWinStatusAfterDelay());
    }

    void Start()
    {
        Debug.Log($"[BattlefieldButton] Start() - GameObject: '{gameObject.name}', levelNumber: {levelNumber}, sceneName: '{sceneName}'");

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            // Store the original sprite if not already stored
            if (originalSprite == null)
            {
                originalSprite = spriteRenderer.sprite;
            }
            spriteRenderer.color = normalColor;
            Debug.Log($"[BattlefieldButton] '{gameObject.name}' - SpriteRenderer found and color set");
        }
        else
        {
            Debug.LogWarning($"[BattlefieldButton] '{gameObject.name}' - No SpriteRenderer found!");
        }

        levelManager = FindFirstObjectByType<LevelManager>();

        // Register with LevelManager to ensure we're in the list
        if (levelManager != null)
        {
            Debug.Log($"[BattlefieldButton] '{gameObject.name}' - LevelManager found, registering...");
            levelManager.RegisterBattlefieldButton(this);
        }
        else
        {
            Debug.LogError($"[BattlefieldButton] '{gameObject.name}' - LevelManager NOT FOUND!");
        }

        // Find BattlefieldInfoManager
        infoManager = FindFirstObjectByType<BattlefieldInfoManager>();
        if (infoManager == null)
        {
            Debug.LogWarning($"[BattlefieldButton] '{gameObject.name}' - BattlefieldInfoManager not found! Battlefield selection won't work.");
        }

        // Find BattleProgressManager
        progressManager = BattleProgressManager.Instance;

        // Wait a frame for BattleProgressManager to load data, then check win status
        StartCoroutine(CheckWinStatusAfterDelay());

        Debug.Log($"[BattlefieldButton] '{gameObject.name}' - Current active state: {gameObject.activeSelf}");
        // Don't hide here - LevelManager will handle visibility in Awake/Start
    }

    /**
     * Wait for BattleProgressManager to load progress, then check win status
     */
    System.Collections.IEnumerator CheckWinStatusAfterDelay()
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
                Debug.Log($"[BattlefieldButton] '{gameObject.name}' - Progress loaded, updating visual state");
            }
        }

        // Update win status and visual
        if (progressManager != null)
        {
            CheckAndUpdateWinStatus();
        }
    }

    /**
     * Check if this battle is won and update the visual (replace sprite with flag)
     */
    void CheckAndUpdateWinStatus()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[BattlefieldButton] '{gameObject.name}' - SpriteRenderer is null, cannot update visual!");
                return;
            }
        }

        // Ensure we have the original sprite stored
        if (originalSprite == null && spriteRenderer.sprite != flagSprite)
        {
            originalSprite = spriteRenderer.sprite;
            Debug.Log($"[BattlefieldButton] '{gameObject.name}' - Captured original sprite: {originalSprite?.name}");
        }

        if (progressManager != null && !string.IsNullOrEmpty(sceneName))
        {
            isWon = progressManager.IsBattleWon(sceneName);
            if (isWon && spriteRenderer != null)
            {
                if (flagSprite != null)
                {
                    // Replace the existing sprite with the flag sprite
                    spriteRenderer.sprite = flagSprite;
                    Debug.Log($"[BattlefieldButton] '{gameObject.name}' - Battle won! Replaced sprite with flag. Current sprite: {spriteRenderer.sprite?.name}");
                }
                else
                {
                    Debug.LogWarning($"[BattlefieldButton] '{gameObject.name}' - Battle is won but flagSprite is not assigned!");
                }
            }
            else if (!isWon && spriteRenderer != null && originalSprite != null)
            {
                // Restore original sprite if battle is not won (shouldn't happen, but just in case)
                spriteRenderer.sprite = originalSprite;
            }
        }
        else
        {
            Debug.LogWarning($"[BattlefieldButton] '{gameObject.name}' - Cannot check win status: progressManager={progressManager != null}, sceneName='{sceneName}'");
        }
    }

    /**
     * Public method to refresh the visual state (can be called externally)
     */
    public void RefreshVisualState()
    {
        CheckAndUpdateWinStatus();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                SelectBattlefield();
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

    void SelectBattlefield()
    {
        // Instead of loading immediately, show info and enable March To War button
        if (infoManager != null)
        {
            // Use battlefield name from scene name if not set
            string displayName = string.IsNullOrEmpty(battlefieldName) ? sceneName : battlefieldName;
            string displayKingdom = string.IsNullOrEmpty(kingdomState) ? "Unknown" : kingdomState;

            infoManager.SelectBattlefield(this, displayName, displayKingdom, detailedInfo, sceneName);
        }
        else
        {
            Debug.LogWarning($"[BattlefieldButton] '{gameObject.name}' - BattlefieldInfoManager not found! Falling back to direct load.");
            // Fallback: load directly if info manager not found
            LoadBattlefield();
        }
    }

    void LoadBattlefield()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public string GetSceneName()
    {
        return sceneName;
    }

    public bool IsWon()
    {
        return isWon;
    }
}

