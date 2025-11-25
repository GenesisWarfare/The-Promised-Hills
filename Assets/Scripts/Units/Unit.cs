using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 2f;
    public int maxHealth = 20;
    public int attackDamage = 5;
    public float attackInterval = 0.5f;

    [Header("Movement")]
    public Vector2 direction = Vector2.right;

    protected int health;
    protected Rigidbody2D rb;

    protected Unit currentEnemyUnit;
    protected GameBase targetBase;

    protected virtual void Awake()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    protected virtual void Update()
    {
        if (currentEnemyUnit != null || targetBase != null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = direction * speed;
    }

    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.TryGetComponent<Unit>(out Unit other) && IsEnemy(other))
        {
            currentEnemyUnit = other;
            StartCoroutine(AttackUnitRoutine(other));
        }
        else if (col.collider.TryGetComponent<GameBase>(out GameBase b) && IsEnemy(b))
        {
            targetBase = b;
            StartCoroutine(AttackBaseRoutine(b));
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.TryGetComponent<Unit>(out Unit other) && other == currentEnemyUnit)
            currentEnemyUnit = null;
        else if (col.collider.TryGetComponent<GameBase>(out GameBase b) && b == targetBase)
            targetBase = null;
    }

    protected virtual bool IsEnemy(Unit other)
    {
        return (this.CompareTag("PlayerUnit") && other.CompareTag("EnemyUnit")) ||
                (this.CompareTag("EnemyUnit") && other.CompareTag("PlayerUnit"));
    }


    protected virtual bool IsEnemy(GameBase b)
    {
        return Vector2.Dot((b.transform.position - transform.position), direction) > 0;
    }

    protected IEnumerator AttackUnitRoutine(Unit other)
    {
        while (other != null && other.health > 0 && health > 0)
        {
            other.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackInterval);
        }
        currentEnemyUnit = null;
    }

    protected IEnumerator AttackBaseRoutine(GameBase b)
    {
        while (b != null && b.IsAlive() && health > 0)
        {
            b.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackInterval);
        }
        targetBase = null;
    }

    public virtual void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0) Destroy(gameObject);
    }
}
