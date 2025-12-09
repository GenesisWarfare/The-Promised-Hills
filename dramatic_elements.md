# Game Dev course - Assignment 5
**File:** yourgame.pdf answers

## Challenge:
a. The player should be able to strategize,budget and press the launch buttons and when buying players or guns.  
b. The player would pick between the different eras of the game (with ranging difficulty) where background, character cast and stats, and the enemy’s ability to adapt changes. In later levels, the enemy would send structured sets of soldiers, for example, a tank unit and behind it gunmen, who would be protected by the tank but could still shoot from afar. Soldier units and bases will be the main objects changing.

---

## Era System

### Objects
- EraManager
- EraData
- UnitData

### What they do
- EraManager switches backgrounds, unit lists, difficulty.  
- EraData stores the visuals, available soldiers, enemy behavior style.  
- UnitData defines cost, movement speed, attack stats per era.

### Functions
- EraManager.SelectEra( EraData era )  
- EraManager.ApplyEraSettings()

c. I suggest a technique where, the enemy could either launch soldiers randomly, launch at a scale similar to the player’s launches, or would strategize as described above, all dynamically decided according to the players base health, seeing as lower base health would correspond to lower skill at the game, enemy’s skills could be dependant on that.

---

## Flow:
a. The player’s attention will be centered on monitoring base health and unit’s battles on ground, as they’d have to follow how well it goes for him, but in order to prevent distractions the soldiers’ battles will be fully animated and with sounds, and a suggested idea would be to attract the players attention when things are not going well for his base or soldiers with an arrow pointing at the direction of the problem, whether it’s their soldiers dying or their base losing health.  
b. The player's control of the game is direct and the battlefield is nearly fully decided by the player's actions, seeing as they would be sending the soldiers and placing the turrets.  
c. When killing an enemy soldier, the player’s xp bar level will rise, that being shown on the xp bar. When winning a battle, the player is greeted with a “Victory” screen and in-game gifts, when losing the player will be shown a “Defeat” screen and will be granted the option to try again.

---

## Entertainment:
a. The player could potentially design their own player asset (Which functions as the main general or king of their base) with customization options such as different hats including crowns and army helmets. There might also be a lucky wheel meta game that will ensure the players come back each day for a roll.  
Other ideas include base design with customizable options.  
b. Among the gamer types our game targets casual gamers, achievers, and education gamers most.  
c. We could also potentially target mobile gamers, by uploading the game to the play services in an adapted form for mobile.

---

## Emotions:
a. Among the emotions we expect the player to feel are excitement from the intense battles and strategy taking place, satisfaction from winning, impatience with the need for budgeting and waiting for soldiers to launch, and stress from not managing to protect their base.  
b. The game’s design puts the battlefield right in the players face, making sure the player is fully aware of the ongoing situation and can react emotionally accordingly, by either feeling satisfaction with the success of their strategy or stress at the loss of their soldiers or base health.

---

## Backstory:
a. Since the game is a historic game that starts with the Kingdom of Israel, the background for the current setting is the establishment of the kingdom and the union of the tribes of Israel under King Saul.  
b. Textual Vis-Novel style story telling is our main option for supplying the player with the necessary background information.

---

## Characters:
### Objects
- UnitLauncher
- SpawnPoint
- UnitPool

### What they do
- Launch units when player presses the launch button.  
- Spawns them over time (small delay between soldiers).  
- Reuses soldier objects instead of creating new ones.

### Functions
- UnitLauncher.Launch(UnitData data, int count)

a. Main characters: The main cast at each level is the player's main soldiers such as bowman, swordsman, “Hero” (Super soldier, e.g. the biblical Samson).  
The resistance/antagonists are the cast of the enemy’s base, so when the player's base is Ancient Israel, the enemies cast would be composed of Philistine or Assyrian soldiers. In other eras for example level 4 - the modern State of Israel, the enemies could be terrorists or soldiers of surrounding nations.  
b. Side characters: Since all characters contribute the same relative portion to the game's progression there are no side characters.  
c. Characters like Judean Bowman would become at later stages a sniper for example or just any gunman. Same goes for swordsmen transitioning into typical modern fighters. The hero character changes to fit the era.  
d. No. All characters are merely soldiers stepping into the battlefield in order to kill the other side’s soldiers and take down the enemy base.

---

## Plot:
a.  
<img width="535" height="317" alt="dramatic_arc_ThePromisedHills" src="https://github.com/user-attachments/assets/09bc1168-bc48-4414-a397-81309bd6419a" />
b. The plot consists of the historic battles of previous Jewish independent bodies in Eretz Yisrael, skipping over occupation periods and diaspora periods. It explores the battles of Ancient Israel, Judah, the Maccabies and the modern State of Israel.  
The dramatic arc increases when a battle unfolds, and reaches its top when a final battle occurs.  
c. The player's behaviour should remain relatively consistent, constantly strategizing and sending troops to battle, with breaks between battles on the main map page.

---

## Worldbuilding:
a. Rules of nature : Consistent with real life rules of nature.  
Geography : The game’s map is centered around the land of Israel, showing a bit of the surrounding area.  
History : The game’s history is fully in line with real life history, with Egyptian occupation and the Bronze Age collapse happening right before the first level.  
b. Economy : The game has a single currency, the Shekel, with it the player buys soldier units and turrets, sending them to battle. The players get money as they kill enemy soldiers and as they progress on timeline, with each victory giving them more money.

### Economy / Budgeting
#### Objects
- EconomyManager
- MoneyUI

#### What they do
- Handles shekel balance.  
- Validates purchases.  
- Updates UI.

#### Functions
- EconomyManager.TryPurchase( UnitData unit )  
- EconomyManager.AddReward( int amount )

Society and politics : The politics of each level will be decided by the level’s era, geopolitics being the main interest in this game. All politics in the game are represented with war.  
c. The player will slowly learn the rules of the world, the world's history and economy with small textual explanations before each battle or mechanic appears for the first time, and will also get familiar with the mechanics due to the slowly increasing difficulty and nature of the game’s structure.
