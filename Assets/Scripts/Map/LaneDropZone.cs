using UnityEngine;

/**
 * LaneDropZone - Represents a lane where units can be dropped
 * Attach this to a GameObject with a Collider2D (set as trigger)
 * When a unit is dragged and dropped here, it spawns in this lane
 */
public class LaneDropZone : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private int laneNumber = 1; // 1, 2, or 3

    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = new Color(0.5f, 1f, 0.5f, 0.3f); // Light green tint
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f); // Transparent

    private SpriteRenderer spriteRenderer;
    private bool isHighlighted = false;

    public int LaneNumber => laneNumber;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // Try to get or add SpriteRenderer for visual feedback
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                // Create a simple white sprite
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                spriteRenderer.sprite = sprite;
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
            spriteRenderer.sortingOrder = -1; // Behind other sprites
        }

        // Ensure there's a Collider2D
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning($"LaneDropZone: No Collider2D found on {gameObject.name}! Adding BoxCollider2D.");
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// Called when a unit is being dragged over this lane
    /// </summary>
    public void Highlight()
    {
        if (!isHighlighted && spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
            isHighlighted = true;
        }
    }

    /// <summary>
    /// Called when a unit is no longer over this lane
    /// </summary>
    public void Unhighlight()
    {
        if (isHighlighted && spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
            isHighlighted = false;
        }
    }

    /// <summary>
    /// Check if a world position is within this lane's bounds
    /// </summary>
    public bool ContainsPoint(Vector2 worldPoint)
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            return col.OverlapPoint(worldPoint);
        }
        return false;
    }
}
