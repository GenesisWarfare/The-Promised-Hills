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

    [Header("Movement")]
    public Vector2 direction = Vector2.right;

    [Header("Collision Detection")]
    public float collisionCheckDistance = 0.5f;
    public float collisionCheckRadius = 0.3f;

    protected int health;
    protected Unit currentEnemyUnit;
    protected GameBase targetBase;
    protected bool isBlockedByFriendly = false;
    protected float lockedYPosition;

    // Public property to check health from other units
    public int Health => health;
    public bool IsAlive => health > 0;

    protected virtual void Awake()
    {
        health = maxHealth;
        lockedYPosition = transform.position.y;
    }

    protected virtual void Update()
    {
        // Lock Y position (stay on lane)
        Vector3 pos = transform.position;
        pos.y = lockedYPosition;
        transform.position = pos;

        // If fighting, stop
        if (currentEnemyUnit != null || targetBase != null)
        {
            return;
        }

        // Check for collisions manually (no physics)
        CheckCollisions();

        // If blocked by friendly, stop
        if (isBlockedByFriendly)
        {
            return;
        }

        // Move forward (simple Transform movement - no physics)
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }

    private void CheckCollisions()
    {
        // Check for enemy units or bases in front
        Vector2 checkPos = (Vector2)transform.position + direction.normalized * collisionCheckDistance;
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

        // Handle enemy unit collision
        if (foundEnemy != null && currentEnemyUnit == null)
        {
            currentEnemyUnit = foundEnemy;
            StartCoroutine(AttackUnitRoutine(foundEnemy));
        }

        // Handle base collision
        if (foundBase != null && targetBase == null)
        {
            targetBase = foundBase;
            StartCoroutine(AttackBaseRoutine(foundBase));
        }
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
            Destroy(gameObject);
        }
    }
}
