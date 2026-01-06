using UnityEngine;
using UnityEngine.InputSystem;

/**
 * MarchToWarButton - Image button that appears when a battlefield is selected
 * Actually loads the battlefield scene when clicked
 * Works with SpriteRenderer and Collider2D (outside Canvas)
 */
public class MarchToWarButton : MonoBehaviour
{
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private SpriteRenderer spriteRenderer;
    private BattlefieldInfoManager infoManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        else
        {
            Debug.LogWarning("MarchToWarButton: No SpriteRenderer found! Add a SpriteRenderer component.");
        }

        // Find BattlefieldInfoManager
        infoManager = FindFirstObjectByType<BattlefieldInfoManager>();
        if (infoManager == null)
        {
            Debug.LogError("MarchToWarButton: BattlefieldInfoManager not found!");
        }

        // Note: BattlefieldInfoManager will handle hiding/showing this button
        // Don't hide here to avoid conflicts with initialization order
    }

    void Update()
    {
        // Handle click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                OnMarchToWarClicked();
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

    void OnMarchToWarClicked()
    {
        if (infoManager != null)
        {
            infoManager.MarchToWar();
        }
        else
        {
            Debug.LogError("MarchToWarButton: Cannot march to war - BattlefieldInfoManager not found!");
        }
    }
}
