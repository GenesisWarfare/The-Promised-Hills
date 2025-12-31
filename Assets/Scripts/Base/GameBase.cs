using UnityEngine;
using System;

public class GameBase : MonoBehaviour
{
    public int maxHealth = 200;
    private int health;

    public bool isPlayerBase = false;

    // Event for health changes (for health bar updates)
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth

    // Public property to get current health
    public int Health => health;
    public float HealthPercent => maxHealth > 0 ? (float)health / maxHealth : 0f;

    void Awake()
    {
        health = maxHealth;
    }

    void Start()
    {
        // Notify health bar of initial health
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health < 0) health = 0;
        
        // Notify health bar of health change
        OnHealthChanged?.Invoke(health, maxHealth);
        
        if (health <= 0) Die();
    }

    public bool IsAlive() => health > 0;

    void Die()
    {
        Debug.Log((isPlayerBase ? "Player" : "Enemy") + " base destroyed!");
        OnHealthChanged?.Invoke(0, maxHealth);
        Destroy(gameObject);
    }
}
