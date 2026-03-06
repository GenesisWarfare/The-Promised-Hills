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
        if (MobileInputHelper.WasPressedThisFrame())
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(MobileInputHelper.GetPosition());
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);

            if (hit != null && hit.gameObject == gameObject)
            {
                LoadMapScene();
            }
        }

        // Visual feedback on hover - swap sprites
        if (spriteRenderer != null)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(MobileInputHelper.GetPosition());
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
        // Save money before going back to map
        Player player = Player.Instance;
        if (player != null)
        {
            player.SaveMoneyNow();
        }

        if (!string.IsNullOrEmpty(mapSceneName))
        {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}
