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

    private SpriteRenderer spriteRenderer;
    private LevelManager levelManager;
    private BattlefieldInfoManager infoManager;

    void Start()
    {
        Debug.Log($"[BattlefieldButton] Start() - GameObject: '{gameObject.name}', levelNumber: {levelNumber}, sceneName: '{sceneName}'");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
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

        Debug.Log($"[BattlefieldButton] '{gameObject.name}' - Current active state: {gameObject.activeSelf}");
        // Don't hide here - LevelManager will handle visibility in Awake/Start
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
}

