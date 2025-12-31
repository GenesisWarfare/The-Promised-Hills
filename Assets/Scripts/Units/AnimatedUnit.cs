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
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private const string ANIM_IDLE = "Idle";
    private const string ANIM_RUN = "Run";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_DEATH = "Death";

    private bool isDead = false;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Flip sprite based on direction
        if (spriteRenderer != null && direction.x < 0)
            spriteRenderer.flipX = true;
    }

    protected override void Update()
    {
        base.Update();

        if (!isDead)
            UpdateAnimations();
    }

    private void UpdateAnimations()
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

        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
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
}
