# Tank Commander - QUICK START GUIDE

## Get Playing in 3 Steps!

### Step 1: Open in Unity (1 minute)
1. Launch Unity Hub
2. Click "Add" → Navigate to `unity-project` folder
3. Select Unity 2022.3 LTS
4. Click "Open"

### Step 2: Install Required Package (1 minute)
1. Go to **Window > Package Manager**
2. Install **Input System**
3. Install **TextMeshPro** (will auto-prompt)
4. Done!

### Step 3: Build the Game (30 seconds)
1. Open `Assets/Scenes/GameScene.unity`
2. Create empty GameObject in scene
3. Add component: `PlayableGameBuilder`
4. Right-click component → **🎮 BUILD COMPLETE PLAYABLE GAME**
5. Press **▶ PLAY**!

## That's It!

You now have a fully playable tank combat game with:
- ✅ Player tank (WASD + mouse controls)
- ✅ 5 AI enemy tanks with smart behaviors
- ✅ Complete arena with walls and cover
- ✅ Working HUD (health, ammo, score)
- ✅ Explosions and visual effects
- ✅ Win/lose conditions

## Controls

| Action | Keyboard/Mouse | Gamepad |
|--------|---------------|---------|
| Move Forward/Back | W/S | Left Stick |
| Turn Left/Right | A/D | Left Stick |
| Aim Turret | Mouse | Right Stick |
| Fire | Left Click | Right Trigger |
| Secondary Fire | Right Click | Left Trigger |
| Reload | R | X (Xbox) / Square (PS) |
| Change Camera | C | B (Xbox) / Circle (PS) |
| Pause | ESC | Start |

## What Just Happened?

The `PlayableGameBuilder` automatically:
1. ✅ Created all manager systems (Game, UI, Audio)
2. ✅ Built a complete arena with ground and walls
3. ✅ Added cover obstacles for tactical gameplay
4. ✅ Spawned player and enemy tanks
5. ✅ Set up camera to follow player
6. ✅ Created complete HUD with health/ammo display
7. ✅ Configured AI with NavMesh pathfinding
8. ✅ Applied all visual effects (explosions, muzzle flashes)

## Customization

Want to tweak the game? Adjust these settings on `PlayableGameBuilder`:

```
Number Of Enemies: 5          // How many AI opponents
Arena Size: 80               // Size of the battlefield
Number Of Obstacles: 10      // Cover objects
Match Duration: 300          // Seconds until game ends
```

## Camera Views

Press **C** to cycle through camera modes:
1. **Third-Person** - Classic behind-the-tank view
2. **Top-Down** - Tactical overhead view
3. **First-Person** - View from the turret
4. **Fixed Angle** - Cinematic angle

## Game Modes

The game supports multiple modes (configured in GameManager):
- **Arcade** - Last tank standing
- **Deathmatch** - Score-based combat
- **Team Battle** - Red vs Blue teams
- **Capture the Flag** - Objective-based
- **King of the Hill** - Territory control

## Building for Release

Want to create an executable?

1. **File > Build Settings**
2. Add scenes:
   - `MainMenu.unity` (index 0)
   - `GameScene.unity` (index 1)
3. Select platform (Windows, Mac, Linux)
4. Click **Build**
5. Share your game!

## Troubleshooting

### Tank doesn't move
- ✅ Check Input System is installed
- ✅ Verify `TankInputActions.inputactions` exists in Assets/InputSystem
- ✅ Ensure tank has `InputManager` component

### No explosions visible
- ✅ Explosions use procedural generation - no assets needed!
- ✅ Check console for errors
- ✅ Verify `ExplosionEffect` script exists

### AI doesn't move
- ✅ Bake NavMesh: **Window > AI > Navigation > Bake**
- ✅ Ensure ground has `NavigationStatic` flag
- ✅ Check `NavMeshAgent` component on AI tanks

### UI not showing
- ✅ Canvas must have `CanvasScaler` set to "Scale With Screen Size"
- ✅ Verify `EventSystem` exists in scene
- ✅ Check `UIManager` references are assigned

## Next Steps

### Add More Content
- Create different tank models (replace cubes with 3D models)
- Add weapon variety (missiles, machine guns, lasers)
- Design multiple arena maps
- Add power-ups and pickups

### Enhance Visuals
- Import particle effects for explosions
- Add post-processing (bloom, color grading)
- Create terrain with Unity Terrain tools
- Add skybox for atmosphere

### Add Audio
- Find free SFX from freesound.org
- Add to AudioManager sound library
- Create music tracks for menu/gameplay
- Implement 3D spatial audio

### Multiplayer (Advanced)
- Install Netcode for GameObjects
- Set up NetworkManager
- Implement player spawning over network
- Add lobby system

## Performance Tips

- **Object Pooling**: Already implemented for projectiles!
- **LOD Groups**: Add for distant tanks
- **Occlusion Culling**: Enable in large arenas
- **Bake Lighting**: For static objects

## Support & Resources

- **Unity Input System**: [Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- **NavMesh**: [Tutorial](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- **C# Scripting**: [Unity Learn](https://learn.unity.com/)

## File Structure

```
unity-project/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity         # Main menu (optional)
│   │   └── GameScene.unity        # Main gameplay scene
│   ├── Scripts/
│   │   ├── Core Systems/
│   │   │   ├── TankController.cs
│   │   │   ├── WeaponSystem.cs
│   │   │   ├── Health.cs
│   │   │   └── Projectile.cs
│   │   ├── AI/
│   │   │   └── TankAI.cs
│   │   ├── Camera/
│   │   │   └── CameraController.cs
│   │   ├── Managers/
│   │   │   ├── GameManager.cs
│   │   │   ├── UIManager.cs
│   │   │   └── AudioManager.cs
│   │   ├── Effects/
│   │   │   ├── ExplosionEffect.cs
│   │   │   └── MuzzleFlash.cs
│   │   └── Setup/
│   │       ├── PlayableGameBuilder.cs  # ⭐ USE THIS!
│   │       ├── GameSetup.cs
│   │       └── SpawnPoint.cs
│   ├── InputSystem/
│   │   └── TankInputActions.inputactions
│   └── Prefabs/
│       ├── BasicTank.prefab       # (optional - auto-created)
│       └── BasicProjectile.prefab # (optional - auto-created)
└── Documentation/
    ├── QUICKSTART.md              # You are here!
    ├── SETUP_GUIDE.md             # Detailed guide
    └── COMPLETION_STATUS.md       # Project status
```

## FAQ

**Q: Do I need 3D models?**
A: No! The game uses procedural geometry (cubes/cylinders). Works great for prototyping and retro-style games.

**Q: Can I use my own tank models?**
A: Yes! Replace the procedural geometry in `CreateTank()` method with your models.

**Q: Does this work in 2D?**
A: No, this is a 3D game. For 2D, you'd need different scripts.

**Q: Can I sell games made with this?**
A: Check the LICENSE.md file. Generally yes for original content!

**Q: Where are the particle effects?**
A: They're procedurally generated! No particle assets needed. Check `ExplosionEffect.cs`.

**Q: How do I add sounds?**
A: Add AudioClips to `AudioManager` and call `PlaySound(clipName)`.

---

## 🎮 Ready to Build Your Own Tank Game?

The `PlayableGameBuilder` gave you a complete foundation. Now customize it:

1. **Tweak values** in inspector
2. **Add your art** (models, textures)
3. **Expand mechanics** (new weapons, abilities)
4. **Share your creation**!

Have fun! 🚀

---

**Pro Tip**: Check the Console window (Ctrl+Shift+C) for helpful debug messages during setup!
