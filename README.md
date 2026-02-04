# Bombing Run - 3D Airplane Shooting & Bombing Game

# Notes for Current Build

- The current build adds an overlay menu for the Game Scene and a Menu Scene with a start game button.
- The intention of the current prompt is to wire the workflow for handling the main Menu scene and the game (pause) menu in the Game scene so the player can successfully navigate the menus to start the game, pause the game and interact with the game menu to resume the game, restart the game, or quit to the main menu.

# Menu Architecture:

- **Assets/Scenes/Menu**:
   - **Build Index**: 1
   - **MenuManager**: Empty Game Object with `Assets/MainMenu.cs` attached.
   - **StartButton**: Button UI Game Object assigned to the `startButton` GameObject in `Assets/MainMenu.cs`.

- **Assets/Scenes/Game**:
   - **Build Index**: 0
   - **MenuButton**: Button UI Game Object child of `Canvas` set as `menuButton` GameObject in `Assets/GameManager.cs`.
   - **GameMenu.cs**: New Script to control the pause menu assigned as a component of `GameManager` empty Game Object in `Game` scene.
   - **Menu**: Panel UI Game Object child of `Canvas` set as `menuPanel` GameObject in `Assets/GameManager.cs`. `Menu` is structured with the following children:
      - **ResumeButton**: Button UI Game Object set as `resumeButton` GameObject in `Assets/GameMenu.cs`.
      - **RestartButton**: Button UI Game Object set as `restartButton` GameObject in `Assets/GameMenu.cs`.
      - **QuitButton**: Button UI Game Object set as `quitButton` GameObject in `Assets/GameMenu.cs`.


## Overview
Bombing Run is a 3D arcade-style airplane game where players control an airplane flying forward, engaging in aerial combat with enemy aircraft (bogies) and ground targets (turrets and bunkers). The game features intuitive controls for aiming and maneuvering, with dedicated buttons for shooting, bombing, boosting, and braking.


## Project Structure
- **Assets/Scenes/**: Contains all game scenes
- **Assets/PlaneController.cs**: Handles player airplane movement, shooting, bombing, boost/brake mechanics, and camera controls
- **Assets/GameManager.cs**: Manages game state, enemy spawning, scoring, and game progression
- **Assets/Turret.cs**: Controls ground turrets that engage the player
- **Assets/Bogie.cs**: Controls enemy aircraft with dynamic flight patterns
- **Assets/Bunker.cs**: Manages destructible ground bunkers
- **Assets/Bullet.cs**: Handles projectile behavior and damage
- **Assets/Bomb.cs**: Controls bomb physics and explosion effects
- **Assets/HealthPickup.cs & AmmoPickup.cs**: Manages collectible items


## Prefabs
- **Airplane.prefab**: Player aircraft
- **AirReticle.prefab**: Aiming reticle for aerial targets
- **GroundReticle.prefab**: Bomb targeting reticle
- **Turret.prefab**: Ground-based enemy
- **Bogie.prefab**: Enemy aircraft
- **Bunker.prefab**: Ground target
- **Bullet.prefab**: Player and enemy projectiles
- **Bomb.prefab**: Player's bombing weapon
- **Health.prefab & Ammo.prefab**: Collectible items


## Controls
- **Movement**: Control the airplane's trajectory using touch/mouse input
- **Shoot Button**: Fire bullets at the AirReticle position
- **Bomb Button**: Drop bombs toward the GroundReticle position
- **Boost Button**: Temporarily increase the plane's speed (hold)
- **Brake Button**: Temporarily decrease the plane's speed (hold)
- The plane maintains forward momentum while allowing directional control


## Gameplay Features
- Dynamic enemy AI with varied attack patterns
- Ground and air combat scenarios
- Collectible health and ammo pickups
- Destructible ground targets
- Predictive bomb targeting system
- Enemy turrets with tracking behavior
- Health and ammunition management
- Speed boost and brake system with shared resource pool
- Resource regeneration when not using boost or brake
- Special Magnet Bogies that can absorb bullets and fire them back as powerful projectiles

## Enemy Types
- **Turrets**: Ground-based enemies that track and fire at the player
- **Bogies**: Standard enemy aircraft that follow various flight patterns
- **Bunkers**: Stationary ground targets that can be destroyed for points
- **Magnet Bogies**: Special enemies that can attract and absorb player bullets, then fire them back as a combined projectile

## Tuning Instructions

### MagnetBogie Settings
- **magnetRange**: Distance at which bullets start being attracted (default: 20)
- **magnetForce**: Strength of the attraction force (default: 50)
- **maxAbsorbedBullets**: Maximum number of bullets that can be absorbed (default: 10)
- **minAbsorptionTime**: Minimum time to absorb bullets before firing (default: 5 seconds)
- **vulnerabilityDuration**: Time the bogie is vulnerable after firing (default: 3 seconds)
- **Visual Feedback**: Magnet Bogies grow in size as they absorb more bullets
- **Color States**:
  - Cyan: Attracting bullets
  - Red: Vulnerable to damage

### Game Balance
- Adjust enemy spawn rates in GameManager.cs to control difficulty
- Modify player health, bomb count, and boost resources in PlaneController.cs
- Fine-tune bullet and bomb damage values for balanced gameplay


## How to Launch
1. Open the project in Unity
2. Open the main scene from `Assets/Scenes/`
3. Press Play to start the game
4. Use touch controls (or mouse in editor) to:
   - Move the plane
   - Aim using the reticles
   - Shoot and drop bombs using the action buttons
   - Hold boost or brake buttons to adjust speed


## Technical Details
- Built with Unity's latest input system (InputSystem_Actions)
- Optimized for performance with mobile considerations
- Uses TextMesh Pro for UI elements
- Implements custom materials for visual effects


## Development Notes
- The game uses Unity's new input system for controls
- Materials are organized in the Materials folder
- Project settings are configured in the Settings directory
- TextMesh Pro is used for all text rendering
- Boost and brake functionality share a common resource pool that regenerates over time
- Ground objects (turrets and bunkers) are carefully placed to ensure they remain fully on the ground plane


---
This project is actively maintained and updated with new features and improvements.