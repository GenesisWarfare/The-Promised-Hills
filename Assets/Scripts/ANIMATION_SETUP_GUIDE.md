# Animation Setup Guide

This guide will help you set up sprite sheet animations for your units (soldiers, knights, etc.).

## Quick Start

### Step 1: Slice Your Sprite Sheets

1. **Select a sprite sheet** in the Project window (e.g., `Hero Knight 2/Sprites/Idle.png`)
2. **Right-click** → **"Setup Sprite Sheet Animation"**
3. In the window that opens:
   - Set **Pixels Per Unit** (usually 100 for pixel art)
   - Set **Frame Rate** (12 fps is standard for pixel art)
   - Enable **Auto-detect Grid Size** (or manually set columns x rows)
   - Set **Animation Name** (e.g., "Idle", "Run", "Attack")
4. Click **"Setup Animation"**

This will:
- Slice the sprite sheet into individual sprites
- Create an Animation Clip with all frames
- Save it in the same folder as the sprite sheet

### Step 2: Create Animator Controller

1. Go to **Assets** → **Create** → **Unit Animator Controller**
2. Set the controller name (e.g., "SoldierAnimator")
3. Select which animation states you want (Idle, Run, Attack, Death, TakeHit)
4. Click **"Create Controller"**

The controller will be created in `Assets/Animations/`

### Step 3: Assign Animation Clips to States

1. **Open the Animator Controller** (double-click it)
2. For each state (Idle, Run, Attack, etc.):
   - Click on the state
   - In the Inspector, find the **Motion** field
   - Drag the corresponding Animation Clip into it

### Step 4: Set Up Your Prefab

1. **Open or create your Soldier prefab**
2. **Add Components**:
   - `AnimatedUnit` script (instead of or in addition to `Unit`)
   - `Animator` component
3. **Configure the Animator**:
   - Drag your Animator Controller into the **Controller** field
4. **Configure AnimatedUnit**:
   - The script will auto-find the Animator and SpriteRenderer
   - Or manually assign them in the Inspector

## Animation States

The system uses these animation states:

- **Idle**: When the unit is standing still
- **Run**: When the unit is moving
- **Attack**: When the unit is attacking an enemy or base
- **Death**: When the unit dies
- **TakeHit**: When the unit takes damage (but doesn't die)

## File Structure

After setup, you should have:

```
Assets/
├── Sprites/
│   └── Hero Knight 2/
│       └── Sprites/
│           ├── Idle.png (sliced into sprites)
│           ├── Idle.anim (animation clip)
│           ├── Run.png
│           ├── Run.anim
│           └── ...
└── Animations/
    └── SoldierAnimator.controller
```

## Tips

1. **Sprite Sheet Format**: Make sure your sprite sheets have frames arranged in a grid
2. **Frame Rate**: 12 fps works well for most pixel art. Adjust based on your art style
3. **Pixels Per Unit**: Use 100 for pixel art to maintain crisp pixels
4. **Grid Detection**: If auto-detection doesn't work, manually set the grid size
5. **Animation Length**: You can adjust animation speed in the Animator Controller

## Troubleshooting

### Sprites show as triangles
- Make sure the sprite sheet is set to **Sprite (Multiple)** in import settings
- Check that sprites were properly sliced

### Animations don't play
- Verify the Animator Controller is assigned to the prefab
- Check that Animation Clips are assigned to the states
- Make sure the `AnimatedUnit` script is on the GameObject

### Wrong animation plays
- Check the Animator Controller parameters match the script
- Verify transitions are set up correctly
- Check that animation clips are assigned to the right states

## Next Steps

Once you have one unit working:
1. Create Animator Controllers for other unit types
2. Use the same Animation Clips or create new ones
3. Adjust animation speeds and transitions as needed

