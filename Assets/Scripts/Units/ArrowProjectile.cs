using UnityEngine;

/**
 * ArrowProjectile - Arrow that flies toward target and deals damage on hit
 */
public class ArrowProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private int damage;
    private float speed = 10f;
    private bool hasHit = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set a default color if no sprite assigned
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            // Create a simple colored square as arrow (temporary visual)
            spriteRenderer.color = Color.yellow;
        }

        // Destroy after 5 seconds if it doesn't hit anything
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (hasHit) return;

        // Move toward target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate arrow to face direction
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Check if reached target (within small threshold)
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        if (distanceToTarget < 0.2f)
        {
            OnReachedTarget();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Check if hit an enemy unit or base
        Unit unit = other.GetComponent<Unit>();
        if (unit != null)
        {
            // Check if it's an enemy (opposite tag)
            bool isEnemy = (this.CompareTag("PlayerArrow") && other.CompareTag("EnemyUnit")) ||
                          (this.CompareTag("EnemyArrow") && other.CompareTag("PlayerUnit"));

            if (isEnemy)
            {
                unit.TakeDamage(damage);
                OnHit();
            }
        }
        else
        {
            GameBase baseObj = other.GetComponent<GameBase>();
            if (baseObj != null)
            {
                // Check if it's an enemy base
                bool isEnemyBase = (this.CompareTag("PlayerArrow") && !baseObj.isPlayerBase) ||
                                  (this.CompareTag("EnemyArrow") && baseObj.isPlayerBase);

                if (isEnemyBase)
                {
                    baseObj.TakeDamage(damage);
                    OnHit();
                }
            }
        }
    }

    void OnReachedTarget()
    {
        // Arrow reached target position - try to find and damage target
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        
        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null)
            {
                bool isEnemy = (this.CompareTag("PlayerArrow") && hit.CompareTag("EnemyUnit")) ||
                              (this.CompareTag("EnemyArrow") && hit.CompareTag("PlayerUnit"));
                if (isEnemy)
                {
                    unit.TakeDamage(damage);
                    OnHit();
                    return;
                }
            }

            GameBase baseObj = hit.GetComponent<GameBase>();
            if (baseObj != null)
            {
                bool isEnemyBase = (this.CompareTag("PlayerArrow") && !baseObj.isPlayerBase) ||
                                  (this.CompareTag("EnemyArrow") && baseObj.isPlayerBase);
                if (isEnemyBase)
                {
                    baseObj.TakeDamage(damage);
                    OnHit();
                    return;
                }
            }
        }

        // No target found at position - just destroy
        OnHit();
    }

    void OnHit()
    {
        hasHit = true;
        Destroy(gameObject);
    }

    public void SetTarget(Vector3 target, int arrowDamage)
    {
        targetPosition = target;
        damage = arrowDamage;
    }

    public void SetSpeed(float arrowSpeed)
    {
        speed = arrowSpeed;
    }
}
