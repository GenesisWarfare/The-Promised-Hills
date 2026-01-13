# Prefab Setup Helper Guide

## Creating Gold Coin Prefab

### Option 1: Use Existing Coin from 2DRPK Package
1. In Unity, navigate to `Assets/2DRPK/Prefabs/Coin.prefab`
2. Right-click the prefab → **Copy**
3. Navigate to `Assets/Prefabs/`
4. Right-click → **Paste**
5. Rename if needed (e.g., `GoldCoin.prefab`)

### Option 2: Create New Coin Prefab
1. In Unity, go to `Assets/Prefabs/` folder
2. Right-click → **Create → Prefab Variant** (or create empty GameObject)
3. Add components:
   - **SpriteRenderer** (assign coin sprite)
   - **Animator** (assign coin animation controller)
   - **Collider2D** (if needed for interaction)
4. Drag the coin GameObject from Hierarchy to Prefabs folder to create prefab

### Adding Coin to Scenes
1. Open the scene where you want the coin
2. Drag the `GoldCoin.prefab` from `Assets/Prefabs/` into the scene
3. Position it where needed
4. The animation should play automatically if Animator is set up correctly

---

## Creating Tank Prefab(s)

### Step 1: Create Tank GameObject
1. In Unity, create a new GameObject (right-click in Hierarchy → **Create Empty**)
2. Name it `Tank_Level_1` (or appropriate level)
3. Add components:
   - **SpriteRenderer**
     - Assign sprite: `Assets/Sprites/Units/Merkava_Tank.png`
   - **Animator** (if you have tank animations)
   - **BoxCollider2D** or **CircleCollider2D**
   - **Unit** or **AnimatedUnit** script (depending on if it has animations)

### Step 2: Configure Unit Script
1. Select the tank GameObject
2. In Inspector, configure the Unit/AnimatedUnit component:
   - **Speed**: Set appropriate speed (e.g., 1.5f for slower tank)
   - **Max Health**: Set health (e.g., 100 for tank)
   - **Attack Damage**: Set damage (e.g., 15 for tank)
   - **Attack Interval**: Set attack speed (e.g., 1.0f for slower attacks)
   - **Unit Cost**: Set cost if it's a player unit (e.g., 50)
   - **Direction**: Set to `(1, 0)` for player units (right) or `(-1, 0)` for enemy units (left)

### Step 3: Set Tag
- For player tanks: Tag = `PlayerUnit`
- For enemy tanks: Tag = `EnemyUnit`

### Step 4: Create Prefab
1. Drag the configured tank GameObject from Hierarchy to `Assets/Prefabs/OwnUnits/` (for player) or `Assets/Prefabs/EnemyUnits/` (for enemy)
2. Name it appropriately (e.g., `Tank_Level_1.prefab`)

### Step 5: Add to Scene
1. Open the battlefield scene
2. Drag the tank prefab into the scene
3. Position it at the spawn point or where needed

---

## Quick Prefab Creation Checklist

### For Coin:
- [ ] SpriteRenderer with coin sprite
- [ ] Animator with coin animation controller
- [ ] Collider2D (optional, for pickup/interaction)
- [ ] Prefab saved in `Assets/Prefabs/`

### For Tank:
- [ ] SpriteRenderer with tank sprite
- [ ] Unit or AnimatedUnit script configured
- [ ] Collider2D for collision detection
- [ ] Correct tag (PlayerUnit or EnemyUnit)
- [ ] Stats configured (health, damage, speed, cost)
- [ ] Prefab saved in appropriate folder (`OwnUnits/` or `EnemyUnits/`)

---

## Notes
- Make sure all sprites are imported with correct settings (Sprite 2D mode)
- Animation controllers should be set up before creating prefabs
- Test prefabs in a scene before using them in gameplay
- Consider creating multiple tank variants (Level 1, Level 2, etc.) with different stats
