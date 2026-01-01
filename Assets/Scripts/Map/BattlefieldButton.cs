using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BattlefieldButton : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string sceneName = "";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private SpriteRenderer spriteRenderer;
    private LevelManager levelManager;

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
                LoadBattlefield();
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

