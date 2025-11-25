using UnityEngine;

public class Soldier : Unit
{
    protected override void Awake()
    {
        base.Awake();
        speed = 2f;
        maxHealth = 25;
        attackDamage = 5;
        attackInterval = 0.5f;
        health = maxHealth;
        direction = Vector2.right;
    }
}
