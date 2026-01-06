# The Promised Hills

Lead your settlement through the ages of Israel — one battle at a time.

**The Promised Hills** is a historical strategy and defense game where you guide a settlement through Israel’s history, from ancient kingdoms to the modern state. Build defenses, recruit units, and fight battles inspired by real events.

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
1. Click the image button near your base to choose the type of unit that you want to spwan.

2. Press 1, 2 or 3 to choose the lane for the spawn of the unit.

3. Units walk forward automatically.

4. They stop and fight when encountering an enemy.

5. Destroy the enemy base before your own falls.

---

## Playtest Questions

See [playtest.md](playtest.md) for detailed playtest questions and design considerations.

