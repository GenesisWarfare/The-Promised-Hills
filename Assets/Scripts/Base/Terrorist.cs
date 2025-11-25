using UnityEngine;

public class Terrorist : Unit
{
    protected override void Awake()
    {
        base.Awake();
        speed = 1.5f;
        maxHealth = 15;
        attackDamage = 10;
        attackInterval = 0.7f;
        health = maxHealth;
        direction = Vector2.left;
    }
}
