using UnityEngine;
using System.Collections;

/**
 * ArcherUnit - Ranged unit that shoots arrows at enemies
 * Can attack from range, even when blocked by friendly units
 */
public class ArcherUnit : AnimatedUnit
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private float attackRange = 5f; // Range to detect and attack enemies
    [SerializeField] private GameObject arrowPrefab; // Arrow projectile prefab (optional - can be created dynamically)
    [SerializeField] private Transform arrowSpawnPoint; // Where to spawn arrows (optional - uses transform.position if null)
    [SerializeField] private float arrowSpeed = 10f; // Speed of arrow projectile

    private bool isRangedAttacking = false;
    private bool isDead = false;

    // Animation constants (same as AnimatedUnit)
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_RUN = "Run";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_DEATH = "Death";

    protected override void Update()
    {
        // Call base Update for basic movement and collision (but we'll override the behavior)
        // First, check for ranged targets (this happens before base collision checks)
        CheckRangedTargets();

        // If we have a ranged target, stop movement but continue attacking
        if (isRangedAttacking && (currentEnemyUnit != null || targetBase != null))
        {
            // Lock Y position
            Vector3 pos = transform.position;
            float originalZ = pos.z;
            pos.y = lockedYPosition;
            pos.z = originalZ;
            transform.position = pos;
            
            // Update animations
            if (!isDead)
                UpdateAnimations();
            return;
        }

        // No ranged target - use normal behavior but allow movement even if blocked by friendly
        // Lock Y position
        Vector3 position = transform.position;
        float z = position.z;
        position.y = lockedYPosition;
        position.z = z;
        transform.position = position;

        // Check melee collisions (for very close enemies or bases)
        CheckCollisions();

        // Move forward if not blocked OR if we have a ranged target (will be handled above)
        if (!isBlockedByFriendly && currentEnemyUnit == null && targetBase == null)
        {
            transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        }
        else if (isBlockedByFriendly && currentEnemyUnit == null && targetBase == null)
        {
            // Blocked by friendly and no target - stay idle
        }

        // Update animations (will call our overridden UpdateAnimations)
        if (!isDead)
            UpdateAnimations();
    }

    /**
     * Update animations for archer (overrides AnimatedUnit.UpdateAnimations to handle ranged attacks)
     */
    protected override void UpdateAnimations()
    {
        if (animator == null || !animator.isInitialized)
            return;

        // Check if moving (not attacking, not blocked, no enemy/base)
        // For archer: can be "moving" even if blocked by friendly if shooting
        bool isMoving = (!isRangedAttacking && currentEnemyUnit == null && targetBase == null && !isBlockedByFriendly);

        if (isRangedAttacking)
        {
            // Keep attack state active
            animator.SetBool(ANIM_RUN, false);
            animator.SetBool(ANIM_IDLE, false);
        }
        else if (isMoving)
        {
            animator.SetBool(ANIM_RUN, true);
            animator.SetBool(ANIM_IDLE, false);
        }
        else
        {
            animator.SetBool(ANIM_RUN, false);
            animator.SetBool(ANIM_IDLE, true);
        }

        // Update flip direction
        UpdateFlipDirectionForArcher();
    }

    /**
     * Check for enemies in range for ranged attacks
     * This happens before melee collision checks
     */
    private void CheckRangedTargets()
    {
        // Don't check if already attacking (will be handled by attack routine)
        if (isRangedAttacking && currentEnemyUnit != null && currentEnemyUnit.IsAlive)
        {
            return;
        }

        // Find all units in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        Unit closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject)
                continue;

            Unit other = hit.GetComponent<Unit>();
            if (other != null && IsEnemy(other) && other.IsAlive)
            {
                // Check if enemy is in front of us
                Vector2 toEnemy = (Vector2)other.transform.position - (Vector2)transform.position;
                float dot = Vector2.Dot(toEnemy.normalized, direction.normalized);

                // Enemy must be in front (dot > 0 means in front)
                if (dot > 0.3f) // Allow some angle tolerance
                {
                    float distance = Vector2.Distance(transform.position, other.transform.position);
                    if (distance < closestDistance)
                    {
                        closestEnemy = other;
                        closestDistance = distance;
                    }
                }
            }

            // Also check for enemy bases in range
            GameBase baseObj = hit.GetComponent<GameBase>();
            if (baseObj != null && IsEnemy(baseObj) && baseObj.IsAlive() && targetBase == null)
            {
                Vector2 toBase = (Vector2)baseObj.transform.position - (Vector2)transform.position;
                float dot = Vector2.Dot(toBase.normalized, direction.normalized);
                if (dot > 0.3f)
                {
                    float distance = Vector2.Distance(transform.position, baseObj.transform.position);
                    if (distance <= attackRange)
                    {
                        targetBase = baseObj;
                        StartCoroutine(AttackBaseRangedRoutine(baseObj));
                        return;
                    }
                }
            }
        }

        // If we found a closest enemy, attack it
        if (closestEnemy != null && currentEnemyUnit == null)
        {
            currentEnemyUnit = closestEnemy;
            StartCoroutine(AttackUnitRangedRoutine(closestEnemy));
        }
        else if (closestEnemy == null && currentEnemyUnit != null)
        {
            // Enemy went out of range or died
            currentEnemyUnit = null;
            isRangedAttacking = false;
        }
    }

    /**
     * Ranged attack routine - shoots arrows at enemy unit
     */
    private IEnumerator AttackUnitRangedRoutine(Unit enemy)
    {
        isRangedAttacking = true;

        while (enemy != null && enemy.IsAlive && IsAlive)
        {
            // Check if enemy is still in range
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance > attackRange)
            {
                // Enemy out of range
                currentEnemyUnit = null;
                isRangedAttacking = false;
                yield break;
            }

            // Trigger attack animation
            if (animator != null && animator.isInitialized)
            {
                animator.SetTrigger(ANIM_ATTACK);
            }

            // Shoot arrow
            ShootArrow(enemy.transform.position);

            // Wait for next attack
            yield return new WaitForSeconds(attackInterval);
        }

        // Enemy died or out of range
        isRangedAttacking = false;
        currentEnemyUnit = null;
    }

    /**
     * Ranged attack routine for bases
     */
    private IEnumerator AttackBaseRangedRoutine(GameBase baseObj)
    {
        isRangedAttacking = true;

        while (baseObj != null && baseObj.IsAlive() && IsAlive)
        {
            // Check if base is still in range
            float distance = Vector2.Distance(transform.position, baseObj.transform.position);
            if (distance > attackRange)
            {
                // Base out of range - move closer
                targetBase = null;
                isRangedAttacking = false;
                yield break;
            }

            // Trigger attack animation
            if (animator != null && animator.isInitialized)
            {
                animator.SetTrigger(ANIM_ATTACK);
            }

            // Shoot arrow at base
            ShootArrow(baseObj.transform.position);

            // Wait for next attack
            yield return new WaitForSeconds(attackInterval);
        }

        // Base destroyed
        isRangedAttacking = false;
        targetBase = null;
    }

    /**
     * Shoot an arrow at the target position
     */
    private void ShootArrow(Vector3 targetPosition)
    {
        // Determine spawn position
        Vector3 spawnPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        spawnPos.z = transform.position.z - 0.1f; // Slightly in front

        // Calculate direction to target
        Vector3 directionToTarget = (targetPosition - spawnPos).normalized;

        // Create arrow (simple approach - instant damage, or create projectile)
        // For now, we'll create a simple arrow projectile
        GameObject arrow = CreateArrow(spawnPos, directionToTarget, targetPosition);
        
        if (arrow != null)
        {
            ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
            if (arrowScript != null)
            {
                arrowScript.SetTarget(targetPosition, attackDamage);
            }
        }
        else
        {
            // Fallback: instant damage if arrow creation fails
            // Find what we're shooting at
            if (currentEnemyUnit != null && currentEnemyUnit.IsAlive)
            {
                currentEnemyUnit.TakeDamage(attackDamage);
            }
            else if (targetBase != null && targetBase.IsAlive())
            {
                targetBase.TakeDamage(attackDamage);
            }
        }
    }

    /**
     * Create an arrow GameObject
     */
    private GameObject CreateArrow(Vector3 position, Vector3 direction, Vector3 target)
    {
        GameObject arrow = null;

        // Try to use prefab if assigned
        if (arrowPrefab != null)
        {
            arrow = Instantiate(arrowPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
        }
        else
        {
            // Otherwise create a simple arrow GameObject
            arrow = new GameObject("Arrow");
            arrow.transform.position = position;
            arrow.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

            // Add SpriteRenderer (will need an arrow sprite assigned)
            SpriteRenderer sr = arrow.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10; // Render above units
            sr.color = Color.yellow; // Temporary visual

            // Add Collider2D for collision detection
            CircleCollider2D collider = arrow.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.1f;
        }

        // Set tag based on archer's tag
        if (this.CompareTag("PlayerUnit"))
        {
            arrow.tag = "PlayerArrow";
        }
        else if (this.CompareTag("EnemyUnit"))
        {
            arrow.tag = "EnemyArrow";
        }

        // Add ArrowProjectile component if not already present
        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
        if (arrowScript == null)
        {
            arrowScript = arrow.AddComponent<ArrowProjectile>();
        }
        arrowScript.SetTarget(target, attackDamage);
        arrowScript.SetSpeed(arrowSpeed);

        return arrow;
    }


    /**
     * Update flip direction for archer (similar to base class but accessible)
     */
    private void UpdateFlipDirectionForArcher()
    {
        if (spriteRenderer == null) return;

        // Use the same logic as AnimatedUnit (Auto behavior by default)
        // This matches the Auto flip behavior from AnimatedUnit
        spriteRenderer.flipX = direction.x < 0;
    }

    public override void TakeDamage(int dmg)
    {
        if (isDead)
            return;

        base.TakeDamage(dmg);

        Animator anim = GetComponent<Animator>();
        if (anim != null && anim.isInitialized)
        {
            if (health <= 0)
            {
                isDead = true;
                isRangedAttacking = false;
                anim.SetTrigger(ANIM_DEATH);
                anim.SetBool(ANIM_RUN, false);
                anim.SetBool(ANIM_IDLE, false);

                // Destroy after death animation
                StartCoroutine(DestroyAfterDeath());
            }
        }
    }

    private System.Collections.IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
