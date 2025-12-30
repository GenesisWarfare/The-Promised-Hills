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
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        levelManager = FindFirstObjectByType<LevelManager>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
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
        Debug.Log($"LevelSelectionButton clicked - levelNumber: {levelNumber}, levelManager: {(levelManager != null ? "found" : "NULL")}");

        if (levelManager != null)
        {
            levelManager.SelectLevel(levelNumber);
        }
        else
        {
            Debug.LogError("LevelManager is NULL! Cannot select level.");
            // Try to find it again
            levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.SelectLevel(levelNumber);
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
