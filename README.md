# The Promised Hills

Lead your settlement through the ages of Israel — one battle at a time.

**The Promised Hills** is a historical strategy and defense game where you guide a settlement through Israel’s history, from ancient kingdoms to the modern state. Build defenses, recruit units, research technologies, and fight battles inspired by real events.

-[Itch.io](https://genesiswarfare.itch.io/the-promised-hills)

---

## Team Members
- **Yanai Levy**
- **Raphael Coeffic**

## 📜 Project Files
-[Formal Elements Document](formal-elements.md)
-[Market Research Document](https://github.com/GenesisWarfare/The-Promised-Hills/blob/main/Market_Research.md)
-[Levels Design Document](https://github.com/GenesisWarfare/The-Promised-Hills/blob/main/Levels-Design.md)

---

## Assets
- Assets/
  - Prefabs/
    - EnemySpawner.prefab
    - EnemySpawnPoint.prefab
    - Enemy_Base.prefab
    - My_Base.prefab
    - PlayerSpawnPoint.prefab
    - Soldier.prefab
    - Terrorist.prefab
  - Scenes/
    - Battlefield.unity
  - Scripts/
    - Base/
      - GameBase.cs
    - Button/
      - LaunchPlayer.cs
    - Spawners/
      - EnemySpawner.cs
    - Units/
      - Unit.cs
  - Settings/
    - (URP + Scene templates)
  - Sprites/

---

# 2D Battle Lane Prototype

This is the early combat prototype of the game. Units walk toward each other, engage, and attempt to destroy the opposing base.

## Features
- Click the on-screen image to spawn a soldier.
- Soldiers move right; terrorists move left.
- Units stop and attack when encountering enemies.
- Bases can be destroyed, ending the match.
- Enemy spawner generates terrorist units periodically.

## Structure

### Units
- **Unit.cs** – Base unit class (movement, health, attack).
- **Soldier.cs** – Moves right, friendly unit.
- **Terrorist.cs** – Moves left, enemy unit.

### Spawning
- **LaunchPlayer.cs** – Spawns soldiers when clicking the UI image.
- **EnemySpawner.cs** – Automatically spawns enemy terrorists.

### Bases
- **Base.cs** – Handles base health and defeat logic.

## How to Play
1. Click the image button near your base to spawn a soldier.
2. Soldiers walk forward automatically.
3. When they meet an enemy, they stop and fight.
4. Destroy the enemy base before yours falls.

---

## Playtest Questions

1. **What is the optimal spawn interval for enemy spawners to maintain challenging gameplay without overwhelming the player?**
   - Current default: 2 seconds
   - Consider: player unit spawn rate, unit combat duration, base health, and difficulty curve

2. **How many enemy spawners should be active per level, and what should be the maximum active units per spawner?**
   - Current limit: 5 active units per spawner
   - Consider: level difficulty progression, screen space, performance, and player strategy options

3. **What are the optimal unit stats (health, damage, speed, attack interval) for balanced combat between player and enemy units?**
   - Current defaults vary by unit type
   - Consider: time-to-kill, strategic depth, and player agency

4. **What is the optimal base health value to create engaging gameplay that allows for comebacks while maintaining tension?**
   - Current default: 200 health
   - Consider: unit damage values, spawn rates, and match duration

