using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * Special Attack Button - Click to trigger special attack
 * Attach this to a GameObject with a SpriteRenderer and Collider2D
 * When clicked, spawns a special attack (plane) that moves across the screen
 */
public class SpecialAttackButton : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject specialAttackPrefab; // The plane/prefab to spawn
    [SerializeField] private float attackCost = 100f; // Cost in money (for future use)
    [SerializeField] private bool useCooldown = false; // Enable cooldown to prevent spam
    [SerializeField] private float cooldownTime = 5f; // Cooldown duration in seconds

    [Header("Sprite Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(-10f, 0f, -2f); // Spawn position (X, Y, Z)

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private bool canUseAttack = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
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

            if (hit != null && hit.gameObject == gameObject && canUseAttack)
            {
                TriggerSpecialAttack();
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
                if (hoverSprite != null)
                {
                    spriteRenderer.sprite = hoverSprite;
                }
            }
            else
            {
                Sprite spriteToUse = normalSprite != null ? normalSprite : originalSprite;
                if (spriteToUse != null)
                {
                    spriteRenderer.sprite = spriteToUse;
                }
            }
        }
    }

    void TriggerSpecialAttack()
    {
        if (specialAttackPrefab == null)
        {
            Debug.LogWarning("SpecialAttackButton: No special attack prefab assigned!");
            return;
        }

        // Use the spawn position set in the editor

        // Check if player has enough money
        Player player = Player.Instance;
        int cost = (int)attackCost;
        if (player != null && !player.HasEnoughMoney(cost))
        {
            Debug.LogWarning($"SpecialAttackButton: Not enough money! Need {cost}, have {player.Money}");
            return;
        }

        // Spend money
        if (player != null)
        {
            player.SpendMoney(cost);
        }

        // Spawn the special attack
        GameObject attack = Instantiate(specialAttackPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"SpecialAttackButton: Spawned special attack at {spawnPosition}");

        // Apply cooldown if enabled
        if (useCooldown)
        {
            canUseAttack = false;
            Invoke(nameof(ReenableAttack), cooldownTime);
        }
    }

    void ReenableAttack()
    {
        canUseAttack = true;
    }
}
