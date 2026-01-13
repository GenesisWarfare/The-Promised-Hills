using UnityEngine;
using System.Collections;

/**
 * Simple Unit - Pygame Style
 * - Image moving across screen with animation
 * - No physics pushing - just Transform movement
 * - Only interacts with what we detect manually
 */
public class Unit : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 2f;
    public int maxHealth = 20;
    public int attackDamage = 5;
    public float attackInterval = 0.5f;
    [Header("Unit Cost (Player Units Only)")]
    [SerializeField] private int unitCost = 20; // Cost to spawn this unit (only used for player units)

    [Header("Movement")]
    public Vector2 direction = Vector2.right;

    [Header("Collision Detection")]
    public float collisionCheckDistance = 0.5f;
    public float collisionCheckRadius = 0.3f;
    public Vector2 collisionCheckOffset = Vector2.zero; // Offset for collision check position (Y for height)

    protected int health;
    protected Unit currentEnemyUnit;
    protected GameBase targetBase;
    protected bool isBlockedByFriendly = false;
    protected float lockedYPosition;

    // Public property to check health from other units
    public int Health => health;
    public bool IsAlive => health > 0;
    public int Cost => unitCost; // Public property to get unit cost

    protected virtual void Awake()
    {
        health = maxHealth;
        lockedYPosition = transform.position.y;
    }

    protected virtual void Update()
    {
        // Lock Y position (stay on lane) but preserve Z position
        Vector3 pos = transform.position;
        float originalZ = pos.z; // Preserve original z position
        pos.y = lockedYPosition;
        pos.z = originalZ; // Restore z position
        transform.position = pos;

        // Check for collisions manually (no physics) - do this first so we can detect enemies
        CheckCollisions();

        // If fighting, stop moving
        if (currentEnemyUnit != null || targetBase != null)
        {
            return;
        }

        // If blocked by friendly, stop
        if (isBlockedByFriendly)
        {
            return;
        }

        // Move forward only if not attacking and not blocked (explicit check before movement)
        if (currentEnemyUnit == null && targetBase == null && !isBlockedByFriendly)
        {
            transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        }
    }


    protected virtual void CheckCollisions()
    {
        // Check for enemy units or bases in front
        // Apply offset to the check position (useful for adjusting vertical position)
        Vector2 checkPos = (Vector2)transform.position + collisionCheckOffset + direction.normalized * collisionCheckDistance;
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, collisionCheckRadius);

        isBlockedByFriendly = false;
        Unit foundEnemy = null;
        GameBase foundBase = null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject)
                continue;

            // Check for enemy unit
            Unit other = hit.GetComponent<Unit>();
            if (other != null)
            {
                if (IsEnemy(other))
                {
                    foundEnemy = other;
                }
                else
                {
                    // Friendly unit in front - check if it's actually in front
                    Vector2 toOther = (Vector2)other.transform.position - (Vector2)transform.position;
                    float dot = Vector2.Dot(toOther.normalized, direction.normalized);
                    if (dot > 0.5f)
                    {
                        isBlockedByFriendly = true;
                    }
                }
            }

            // Check for base
            GameBase baseObj = hit.GetComponent<GameBase>();
            if (baseObj != null && IsEnemy(baseObj))
            {
                foundBase = baseObj;
            }
        }

        // Also check if we've reached the screen edge (for invisible bases)
        if (foundBase == null)
        {
            foundBase = CheckScreenEdgeForBase();
        }

        // Handle enemy unit collision
        if (foundEnemy != null && currentEnemyUnit == null)
        {
            currentEnemyUnit = foundEnemy;
            StartCoroutine(AttackUnitRoutine(foundEnemy));
        }
        // If we're already attacking, verify the enemy is still valid
        else if (currentEnemyUnit != null)
        {
            // If the current enemy is dead, clear it (attack routine will also clear it, but this ensures immediate stop)
            if (!currentEnemyUnit.IsAlive)
            {
                currentEnemyUnit = null;
            }
            // Otherwise, keep currentEnemyUnit set so we continue attacking and don't move
        }

        // Handle base collision
        if (foundBase != null && targetBase == null)
        {
            targetBase = foundBase;
            StartCoroutine(AttackBaseRoutine(foundBase));
        }
    }

    private GameBase CheckScreenEdgeForBase()
    {
        // Get screen bounds
        Camera cam = Camera.main;
        if (cam == null) return null;

        float screenLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

        // Check if we're at the edge of the screen
        float edgeThreshold = 0.3f;

        // Player units move right, enemy units move left
        if (this.CompareTag("PlayerUnit"))
        {
            // Player unit moving right - check if reached right edge (enemy base)
            if (transform.position.x >= screenRight - edgeThreshold)
            {
                // Find enemy base
                BaseManager baseManager = FindFirstObjectByType<BaseManager>();
                if (baseManager != null)
                {
                    GameBase enemyBase = baseManager.GetEnemyBase();
                    if (enemyBase != null && enemyBase.IsAlive())
                    {
                        return enemyBase;
                    }
                }
            }
        }
        else if (this.CompareTag("EnemyUnit"))
        {
            // Enemy unit moving left - check if reached left edge (player base)
            if (transform.position.x <= screenLeft + edgeThreshold)
            {
                // Find player base
                BaseManager baseManager = FindFirstObjectByType<BaseManager>();
                if (baseManager != null)
                {
                    GameBase playerBase = baseManager.GetPlayerBase();
                    if (playerBase != null && playerBase.IsAlive())
                    {
                        return playerBase;
                    }
                }
            }
        }

        return null;
    }

    protected virtual bool IsEnemy(Unit other)
    {
        return (this.CompareTag("PlayerUnit") && other.CompareTag("EnemyUnit")) ||
               (this.CompareTag("EnemyUnit") && other.CompareTag("PlayerUnit"));
    }

    protected virtual bool IsEnemy(GameBase baseObj)
    {
        if (this.CompareTag("PlayerUnit"))
        {
            return !baseObj.isPlayerBase;
        }
        else if (this.CompareTag("EnemyUnit"))
        {
            return baseObj.isPlayerBase;
        }
        return false;
    }

    protected virtual IEnumerator AttackUnitRoutine(Unit enemy)
    {
        while (enemy != null && enemy.IsAlive && IsAlive)
        {
            enemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackInterval);
        }
        currentEnemyUnit = null;
    }

    protected virtual IEnumerator AttackBaseRoutine(GameBase baseObj)
    {
        while (baseObj != null && baseObj.IsAlive() && IsAlive)
        {
            baseObj.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackInterval);
        }
        targetBase = null;
    }

    public virtual void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            // Give money to player if this is an enemy unit
            if (this.CompareTag("EnemyUnit"))
            {
                Player player = Player.Instance;
                if (player != null)
                {
                    player.OnEnemyKilled(this);
                }
            }

            Destroy(gameObject);
        }
    }
}
