using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.SceneManagement;

/**
 * Back to Map Button - Click to return to the map scene
 * Attach this to a GameObject with a SpriteRenderer and Collider2D
 * Click on it to load the Map scene
 */
public class BackToMapButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mapSceneName = "Map";

    [Header("Sprite Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Store the original sprite if not set in inspector
            originalSprite = spriteRenderer.sprite;

            // Use normal sprite if assigned, otherwise keep the original
            if (normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
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
                LoadMapScene();
            }
        }

        // Visual feedback on hover - swap sprites
        if (spriteRenderer != null)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(
#if ENABLE_INPUT_SYSTEM
                Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero
#else
                Input.mousePosition
#endif
            );
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.gameObject == gameObject)
            {
                // Use hover sprite if assigned, otherwise keep current
                if (hoverSprite != null)
                {
                    spriteRenderer.sprite = hoverSprite;
                }
            }
            else
            {
                // Use normal sprite if assigned, otherwise use original
                Sprite spriteToUse = normalSprite != null ? normalSprite : originalSprite;
                if (spriteToUse != null)
                {
                    spriteRenderer.sprite = spriteToUse;
                }
            }
        }
    }

    void LoadMapScene()
    {
        if (!string.IsNullOrEmpty(mapSceneName))
        {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}
