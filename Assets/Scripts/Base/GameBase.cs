using UnityEngine;

public class GameBase : MonoBehaviour
{
    public int maxHealth = 200;
    private int health;

    public bool isPlayerBase = false;

    void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0) Die();
    }

    public bool IsAlive() => health > 0;

    void Die()
    {
        Debug.Log((isPlayerBase ? "Player" : "Enemy") + " base destroyed!");
        Destroy(gameObject);
    }
}
