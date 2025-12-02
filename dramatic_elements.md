# Game Dev Course - Assignment 5
**File:** yourgame.pdf answers

## Challenge
1. The player should be able to strategize, budget, and press the launch buttons when buying players or guns.
2. The player would pick between different eras of the game (with varying difficulty), where background, character cast, stats, and enemy behavior change.  
   - In later levels, the enemy could send structured sets of soldiers (e.g., a tank unit with gunmen behind it).  
   - Soldier units and bases are the main objects that change.

---

## Era System

### Objects
- **EraManager**
- **EraData**
- **UnitData**

### What They Do
- **EraManager**: switches backgrounds, unit lists, difficulty.  
- **EraData**: stores visuals, available soldiers, enemy behavior style.  
- **UnitData**: defines cost, movement speed, attack stats per era.

### Functions
- `EraManager.SelectEra(EraData era)`  
- `EraManager.ApplyEraSettings()`

### Suggested Enemy Behavior
- Enemy can launch soldiers randomly, proportionally to the player's actions, or strategically, depending on the player's base health.

---

## Flow
1. Player attention focuses on base health and units on the battlefield.  
   - Soldiers’ battles are fully animated with sounds.  
   - Arrows indicate problems (soldiers dying or base losing health).
2. Player controls the battlefield directly by sending soldiers and placing turrets.
3. Rewards and consequences:  
   - Killing enemy soldiers increases XP bar.  
   - Winning triggers a "Victory" screen and in-game gifts.  
   - Losing triggers a "Defeat" screen with option to retry.

---

## Entertainment
1. Player customization: design main general/king with hats, crowns, army helmets.  
   - Optional meta-game: lucky wheel for daily engagement.
2. Base customization options.
3. Target audience: casual gamers, achievers, educational gamers, mobile gamers.

---

## Emotions
1. Expected player emotions: excitement, satisfaction, impatience, stress.
2. Battlefield design ensures emotional engagement: success or failure is immediately visible.

---

## Backstory
1. Historical setting: starts with the Kingdom of Israel under King Saul.
2. Storytelling: visual-novel style for background info.

---

## Characters

### Objects
- **UnitLauncher**
- **SpawnPoint**
- **UnitPool**

### What They Do
- Launch units when player presses launch button.  
- Spawn units over time.  
- Reuse soldier objects instead of creating new ones.

### Functions
- `UnitLauncher.Launch(UnitData data, int count)`

### Main Characters
- Player soldiers (e.g., bowman, swordsman, hero).  
- Enemy soldiers vary by era (Philistines, Assyrians, terrorists, modern soldiers).

### Side Characters
- None (all characters contribute equally to progression).

### Character Evolution
- Soldiers transition with eras (e.g., bowman → sniper).  
- Hero character changes to fit the era.

### Combat Role
- All characters are soldiers; no non-combat roles.

---

## Plot

<img width="535" height="317" alt="dramatic_arc_ThePromisedHills" src="https://github.com/user-attachments/assets/09bc1168-bc48-4414-a397-81309bd6419a" />

1. Focuses on historic Jewish battles in Eretz Yisrael, skipping occupation/diaspora periods.  
2. Dramatic arc: builds through battles, peaks in final battle.  
3. Player behavior: consistent strategizing and sending troops, with breaks between battles on the main map.

---

## Worldbuilding

### Nature, Geography, History
- Rules consistent with real life.  
- Map centered around Israel, including surrounding areas.  
- Historical accuracy maintained (e.g., Egyptian occupation, Bronze Age collapse).

### Economy / Budgeting

#### Objects
- **EconomyManager**
- **MoneyUI**

#### What They Do
- Handles shekel balance.  
- Validates purchases.  
- Updates UI.

#### Functions
- `EconomyManager.TryPurchase(UnitData unit)`  
- `EconomyManager.AddReward(int amount)`

- Single currency (Shekel) used to buy units and turrets.  
- Money rewarded for kills and victories.

### Society and Politics
- Level politics determined by era.  
- All politics represented through war.

### Learning Curve
- Players gradually learn world rules, history, and economy with textual explanations and increasing difficulty.
