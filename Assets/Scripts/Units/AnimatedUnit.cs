using UnityEngine;
using System.Collections;

/**
 * Animated Unit - Adds animation support to Unit
 * Works like pygame - just animated sprite moving across screen
 * Attack animation synchronized with damage dealing
 */
public class AnimatedUnit : Unit
{
    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public enum FlipBehavior
    {
        Auto,              // Flip when moving left (default)
        Inverted,          // Flip when moving right
        AlwaysFlipped,     // Always flip regardless of direction
        NeverFlipped       // Never flip regardless of direction
    }

    [Header("Sprite Flipping")]
    [SerializeField] private FlipBehavior flipBehavior = FlipBehavior.Auto;

    private const string ANIM_IDLE = "Idle";
    private const string ANIM_RUN = "Run";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_DEATH = "Death";

    [System.NonSerialized]
    protected bool isDead = false; // Protected so derived classes can access it, non-serialized to avoid conflicts
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Set initial flip based on behavior
        if (spriteRenderer != null)
        {
            UpdateFlipDirection();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isDead)
            UpdateAnimations();
    }

    protected virtual void UpdateAnimations()
    {
        if (animator == null || !animator.isInitialized)
            return;

        // Check if moving (not attacking, not blocked, no enemy/base)
        bool isMoving = (currentEnemyUnit == null && targetBase == null && !isBlockedByFriendly && !isAttacking);

        if (isAttacking)
        {
            // Keep attack state active (animation triggered in attack routine)
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
        if (spriteRenderer != null)
        {
            UpdateFlipDirection();
        }
    }

    // Override attack routines to trigger attack animation when dealing damage
    protected override IEnumerator AttackUnitRoutine(Unit enemy)
    {
        isAttacking = true;

        while (enemy != null && enemy.IsAlive && IsAlive)
        {
            // Trigger attack animation
            if (animator != null && animator.isInitialized)
            {
                animator.SetTrigger(ANIM_ATTACK);
            }

            // Deal damage
            enemy.TakeDamage(attackDamage);

            // Wait for next attack
            yield return new WaitForSeconds(attackInterval);
        }

        // Enemy died - immediately stop attacking and reset animation
        isAttacking = false;
        currentEnemyUnit = null;

        // Force animation back to run/idle immediately
        if (animator != null && animator.isInitialized)
        {
            animator.ResetTrigger(ANIM_ATTACK);
            // Force immediate transition to run
            UpdateAnimations();
        }
    }

    protected override IEnumerator AttackBaseRoutine(GameBase baseObj)
    {
        isAttacking = true;

        while (baseObj != null && baseObj.IsAlive() && IsAlive)
        {
            // Trigger attack animation
            if (animator != null && animator.isInitialized)
            {
                animator.SetTrigger(ANIM_ATTACK);
            }

            // Deal damage
            baseObj.TakeDamage(attackDamage);

            // Wait for next attack
            yield return new WaitForSeconds(attackInterval);
        }

        // Base destroyed - immediately stop attacking and reset animation
        isAttacking = false;
        targetBase = null;

        // Force animation back to run/idle immediately
        if (animator != null && animator.isInitialized)
        {
            animator.ResetTrigger(ANIM_ATTACK);
            // Force immediate transition to run
            UpdateAnimations();
        }
    }

    public override void TakeDamage(int dmg)
    {
        if (isDead)
            return;

        base.TakeDamage(dmg);

        if (animator != null && animator.isInitialized)
        {
            if (health <= 0)
            {
                isDead = true;
                animator.SetTrigger(ANIM_DEATH);
                animator.SetBool(ANIM_RUN, false);
                animator.SetBool(ANIM_IDLE, false);

                // Destroy after death animation
                StartCoroutine(DestroyAfterDeath());
            }
        }
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Updates the sprite flip direction based on the selected flip behavior
    /// </summary>
    private void UpdateFlipDirection()
    {
        if (spriteRenderer == null) return;

        switch (flipBehavior)
        {
            case FlipBehavior.Auto:
                // Flip when moving left (default behavior)
                spriteRenderer.flipX = direction.x < 0;
                break;

            case FlipBehavior.Inverted:
                // Flip when moving right (opposite of default)
                spriteRenderer.flipX = direction.x > 0;
                break;

            case FlipBehavior.AlwaysFlipped:
                // Always flip regardless of direction
                spriteRenderer.flipX = true;
                break;

            case FlipBehavior.NeverFlipped:
                // Never flip regardless of direction
                spriteRenderer.flipX = false;
                break;
        }
    }
}
