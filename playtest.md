# Playtest Questions

This document outlines key design questions that need to be answered through playtesting and iteration. These questions focus on finding the optimal balance between challenge, engagement, and player agency in The Promised Hills.

---

## 1. What is the optimal spawn interval for enemy spawners to maintain challenging gameplay without overwhelming the player?

**Current Default:** 2 seconds

**Context:**
The spawn interval directly impacts the game's difficulty curve and pacing. Too fast, and players feel overwhelmed and unable to strategize. Too slow, and the game becomes trivial with no sense of urgency. The spawn interval must work in harmony with other game systems—particularly unit combat duration, player spawn capabilities, and base health values.

**Considerations:**
- **Player unit spawn rate:** How quickly can players respond to threats? If players can spawn units faster than enemies, the game becomes too easy. If enemies spawn faster than players can react, frustration sets in.
- **Unit combat duration:** How long do units take to kill each other? If units die quickly, faster spawn rates might be acceptable. If combat is prolonged, slower spawns prevent screen clutter.
- **Base health:** With higher base health, players can afford more mistakes, allowing for faster enemy spawns. Lower base health requires more careful pacing.
- **Difficulty curve:** Early levels should have slower spawns to teach mechanics, while later levels can increase intensity.


---

## 2. How many enemy spawners should be active per level, and what should be the maximum active units per spawner?

**Current Limit:** 5 active units per spawner

**Context:**
The number of spawners and their unit limits determine the strategic complexity and visual density of each level. Multiple spawners can create interesting tactical situations where players must manage threats from different directions, while a single spawner creates a more focused, linear challenge. The maximum active units per spawner prevents screen clutter and ensures performance remains stable, but also creates natural "waves" as units die and new ones spawn.

**Considerations:**
- **Level difficulty progression:** Early levels might use 1-2 spawners with lower unit limits, while later levels could feature 3-5 spawners with higher limits.
- **Screen space:** Too many units on screen simultaneously can make it difficult to see what's happening and make strategic decisions.
- **Performance:** Each active unit requires physics calculations, collision detection, and rendering. More units means more computational overhead.
- **Player strategy options:** Multiple spawners create opportunities for flanking, resource management, and prioritization decisions. A single spawner is more straightforward but potentially less engaging.


---

## 3. What are the optimal unit stats (health, damage, speed, attack interval) for balanced combat between player and enemy units?

**Current Defaults:** Vary by unit type (configurable in editor)


**Considerations:**
- **Time-to-kill (TTK):** How long does it take for one unit to kill another? Fast TTK creates quick, decisive combat but reduces strategic options. Slow TTK allows for more tactical play but can feel sluggish.
- **Strategic depth:** Different stat combinations create different optimal strategies. High damage, low health units favor aggressive play. High health, low damage units favor attrition.
- **Player agency:** Players should feel their decisions matter. If units die too quickly, players can't react. If combat is too slow, players feel powerless.
- **Visual feedback:** Attack intervals should feel responsive. Too fast and players can't see individual attacks. Too slow and combat feels unresponsive.

---

## 4. What is the optimal base health value to create engaging gameplay that allows for comebacks while maintaining tension?

**Current Default:** 200 health

**Considerations:**
- **Unit damage values:** Base health must be balanced against how much damage units deal. If units deal 5 damage, 200 health means 40 hits to destroy—is this the right number?
- **Spawn rates:** With faster enemy spawns, higher base health gives players more time to respond. With slower spawns, lower health maintains tension.
- **Match duration:** Base health directly impacts how long matches last. Too short and players don't have time to engage with systems. Too long and players get fatigued.
- **Comeback potential:** Players should be able to recover from early mistakes. Base health that's too low makes early damage feel like an automatic loss.


---

## 5. How many player spawn buttons should be available per level, and where should they be positioned for optimal strategic gameplay?

**Current Setup:** Configurable in editor


**Considerations:**
- **Strategic positioning:** Spawn points should be positioned to allow players to respond to threats from different directions, but not so close to the base that they feel like a safety net.
- **Screen layout:** Spawn buttons should be easily accessible without blocking important visual information about unit positions and combat.
- **Difficulty scaling:** Early levels might have 1-2 spawn points near the player base for simplicity, while later levels could feature 3-4 spawn points at various strategic locations.
- **Player agency:** More spawn points give players more tactical options but also more decisions to make. Balance between simplicity and depth.
- **Visual clarity:** Players should be able to quickly identify which spawn button to use. Clear visual indicators and positioning help reduce cognitive load.


---

## Additional Notes

These questions are interconnected—changing one value often requires adjusting others. For example, increasing enemy spawn rate might require increasing base health to maintain balance. The goal is to find a cohesive set of values that work together to create engaging, challenging gameplay.

Regular playtesting with diverse players (both experienced and new) will be essential to answering these questions. Document observations, measure metrics, and iterate based on data and feedback.

