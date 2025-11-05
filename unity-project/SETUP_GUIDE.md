# Tank Commander - Unity Setup Guide

## Quick Start

This guide will help you set up and run the Tank Commander game in Unity.

## Prerequisites

- Unity 2022.3 LTS or newer
- Visual Studio 2022 or Visual Studio Code with C# extension

## Installation Steps

### 1. Open Project in Unity

1. Launch Unity Hub
2. Click "Add" and navigate to the `unity-project` folder
3. Select Unity 2022.3 LTS as the editor version
4. Click "Open"

### 2. Install Required Packages

Once the project opens, install these packages via **Window > Package Manager**:

- **Universal Render Pipeline (URP)** - For modern graphics rendering
- **Input System** - Already configured for tank controls
- **TextMeshPro** - For UI text (auto-installs when needed)
- **Cinemachine** (Optional) - For advanced camera features
- **Post Processing** (Optional) - For visual effects

### 3. Project Structure Overview

```
Assets/
├── Scenes/
│   ├── GameScene.unity          # Main gameplay scene
│   └── MainMenu.unity           # (Create this for menu)
├── Scripts/                     # All C# game logic
│   ├── TankController.cs        # Tank movement
│   ├── WeaponSystem.cs          # Weapon firing
│   ├── Health.cs                # Health/damage
│   ├── Projectile.cs            # Bullet physics
│   ├── TankAI.cs                # Enemy AI
│   ├── CameraController.cs     # Camera system
│   ├── GameManager.cs           # Game state
│   ├── PlayerInfo.cs            # Player stats
│   ├── InputManager.cs          # Input handling
│   ├── UIManager.cs             # HUD/menus
│   ├── AudioManager.cs          # Sound system
│   ├── ObjectPooler.cs          # Performance optimization
│   └── SpawnPoint.cs            # Spawn locations
├── Prefabs/
│   ├── BasicTank.prefab         # Player/AI tank
│   └── BasicProjectile.prefab  # Bullet/shell
├── InputSystem/
│   └── TankInputActions.inputactions # Input configuration
└── Materials/                   # (Add your materials here)
```

## Setting Up Your First Scene

### 1. Open GameScene

1. Navigate to `Assets/Scenes/GameScene.unity`
2. Double-click to open it

### 2. Create Game Manager

1. Create empty GameObject: **GameObject > Create Empty**
2. Rename it to "GameManager"
3. Add scripts: `GameManager`, `UIManager`, `AudioManager`, `ObjectPooler`

### 3. Add Spawn Points

1. Create empty GameObject: **GameObject > Create Empty**
2. Rename to "SpawnPoint_01"
3. Add `SpawnPoint` component
4. Position at (0, 1, 0)
5. Duplicate (Ctrl+D) and position at (10, 1, 0), (−10, 1, 0), etc.

### 4. Instantiate Tank

1. Drag `Assets/Prefabs/BasicTank.prefab` into the scene
2. Position at a spawn point location
3. The tank is pre-configured with:
   - TankController (movement)
   - WeaponSystem (shooting)
   - Health (HP management)
   - InputManager (player controls)

### 5. Add Camera

The scene already has a Main Camera. To set up the camera system:

1. Select Main Camera
2. Add `CameraController` component
3. Assign the tank as the "Target" in the inspector
4. Set Camera Mode to "ThirdPerson"

### 6. Configure Input

The Input System is already configured in `Assets/InputSystem/TankInputActions.inputactions`:

- **WASD**: Tank movement
- **Mouse**: Turret rotation
- **Left Click**: Fire primary weapon
- **Right Click**: Fire secondary weapon
- **R**: Reload
- **C**: Change camera view
- **ESC**: Pause

## Setting Up UI

### 1. Create Canvas

1. **GameObject > UI > Canvas**
2. Set Canvas Scaler to "Scale With Screen Size"
3. Reference Resolution: 1920x1080

### 2. Add HUD Elements

Create these UI elements as children of Canvas:

**Health Bar:**
- **GameObject > UI > Slider**
- Rename to "HealthBar"
- Position at bottom-left

**Ammo Counter:**
- **GameObject > UI > Text - TextMeshPro**
- Rename to "AmmoText"
- Position at bottom-right

**Crosshair:**
- **GameObject > UI > Image**
- Rename to "Crosshair"
- Anchor to center
- Use a crosshair sprite

**Score/Time:**
- Create TextMeshPro objects at top of screen

### 3. Link UI to UIManager

1. Select GameManager
2. Find UIManager component
3. Drag HUD elements into the inspector fields

## Adding Enemies

1. Drag `BasicTank` prefab into scene
2. Select the tank
3. Remove `InputManager` component (AI doesn't use player input)
4. Add `TankAI` component
5. Set AI behavior type (Patrol, Guard, Chase, etc.)
6. Configure detection range and aggression

## Testing the Game

### 1. Play Mode

1. Click the Play button (▶) at top of editor
2. Use WASD to move
3. Mouse to aim turret
4. Left-click to fire

### 2. Camera Controls

- Press **C** to cycle through camera views:
  - Third-person follow
  - Top-down tactical
  - First-person turret
  - Fixed angle

### 3. Debug Mode

Enable Gizmos in Game view to see:
- Tank AI field of view cones
- Spawn point indicators
- Collision boundaries

## Common Issues & Solutions

### Tank falls through ground
- Select Ground object
- Add **Mesh Collider** component
- Enable "Convex" if needed

### Tank doesn't move
- Check that TankController has a Rigidbody
- Ensure Rigidbody mass is reasonable (1000kg)
- Verify Input System is installed

### Projectiles don't spawn
- Check that WeaponSystem has FirePoint assigned
- Ensure BasicProjectile prefab is assigned
- Verify projectile has Rigidbody and Collider

### Camera doesn't follow
- Assign tank to CameraController's "target" field
- Check camera offset values
- Ensure tank has the "Player" tag

### UI doesn't update
- Link UI elements in UIManager inspector
- Check that EventSystem exists in scene
- Verify Canvas is set to Screen Space - Overlay

## Performance Tips

1. **Object Pooling**: The ObjectPooler is set up for projectiles
2. **NavMesh**: Bake NavMesh for AI pathfinding (**Window > AI > Navigation**)
3. **Occlusion Culling**: Enable for large scenes
4. **LOD Groups**: Add for distant tanks

## Next Steps

1. **Create Main Menu**:
   - New scene: `MainMenu.unity`
   - Add UI with buttons
   - Wire up UIManager

2. **Add More Content**:
   - Design arena layouts
   - Create different tank models
   - Add weapon varieties
   - Build particle effects

3. **Multiplayer** (Advanced):
   - Install Netcode for GameObjects
   - Implement NetworkManager
   - Add player spawning over network

4. **Build Game**:
   - **File > Build Settings**
   - Add scenes to build
   - Select target platform
   - Click "Build"

## Resources

- [Unity Input System Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [NavMesh Tutorial](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- [URP Setup Guide](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

## Support

For issues, check:
1. Console window for errors (Ctrl+Shift+C)
2. Inspector for missing references
3. Project settings for Input System backend

---

**You're all set!** Press Play and start commanding your tank! 🎮
