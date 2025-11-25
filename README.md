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

