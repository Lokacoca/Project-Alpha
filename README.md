# Project Alpha

A 3D multiplayer, movement-based first-person shooter built in Unity. Players express skill through both movement (sprinting, sliding, wallrunning) and aim. Inspired by Titanfall and RIVALS.

**Download:** a ready-to-run Windows build is available under [Releases](https://github.com/Lokacoca/Project-Alpha/releases).
<img width="400" height="205" alt="ezgif-3b0202e8f86e065d" src="https://github.com/user-attachments/assets/6abda9ba-6985-48f5-81f8-71235d94f0c5" />



> Developed as a gymnasium project at Tullängsgymnasium, including a formal technical report (see [Documentation](#documentation)).

## Highlights

- Responsive physics-based movement: jump, crouch, sprint, slide and wallrun
- Hitscan weapon system with procedural animations (weapon bob, aim down sights, accumulating recoil)
- Real-time multiplayer with automatic room creation and dynamic spawn handling (Photon PUN 2)
- Hierarchical animation system using Blend Trees and IK constraints
- 400+ FPS on the test machine

## Features

**Movement**
- Rigidbody-based movement with interpolation for smooth camera motion
- Dynamic FOV changes to convey speed
- Wallrunning based on collision normals, with cooldown logic to prevent sticking and exploits

**Combat**
- Raycast-based hitscan weapons with configurable range, spread, magazine size and reload time
- Damage handled over the network via RPCs, so it registers correctly regardless of which client fires
- Muzzle flash, bullet tracers, impact effects and bullet holes, synchronized across clients
- Ammo UI built with TextMeshPro, updated reactively (only on fire/reload events)

**Procedural weapon animation**
- Walking sway using a cosine oscillation
- Aim down sights using Lerp
- Recoil with separate vertical/horizontal Lerp recovery, reaching an equilibrium during sustained fire

**Multiplayer**
- `RoomManager` (singleton) connects to the Photon master server and uses `JoinOrCreateRoom` to put players in a session automatically
- Switches from single-player spawn to multiplayer spawn when a second player joins, coordinated by the master client through an RPC
- Position and rotation synced with Photon Transform View

**Character animation**
- Two-level Blend Tree: a 1D parent tree (standing vs. crouching/sliding) containing a 2D locomotion tree
- Smooth input-to-animation transitions using `Mathf.Lerp`
- Two-Bone IK and Multi-Aim constraints (Animation Rigging) for weapon grip and aiming posture

## Tech stack

| | |
|---|---|
| Engine | Unity 2022 LTS |
| Language | C# |
| Networking | Photon PUN 2 |
| Packages | Input System, Animation Rigging, ProBuilder, TextMeshPro |
| IDE | Visual Studio |

## Technical decisions

- **Hitscan over projectiles:** chosen to minimize network lag and maximize responsiveness.
- **Hybrid synchronization:** effects that matter for gameplay (tracers, impacts) are synced via RPC. Purely local effects (muzzle flash, weapon animation) run locally to reduce network load.
- **Modular weapons:** weapon stats are adjustable per weapon without changing code.
- **Rigidbody interpolation:** fixed jittery movement caused by physics updates running at a different rate than the frame rate.

## Performance

Over 400 FPS on an RTX 4060 Ti, Ryzen 7 5800X, 32 GB RAM.

## Known limitations

- The wallrun animation is not synchronized in multiplayer: other players do not see it.
- A hitscan bug affects roughly 10% of aiming angles, likely because the player's own model blocks the raycast. The planned fix is layer-based raycast filtering.
- Multiplayer supports two players.

## Getting started

<!-- Fill in how someone can run it, for example: -->
1. Clone the repository.
2. Open the project with Unity 2022 LTS.
3. Create a free Photon account and set your own App ID in the PUN settings.
4. Open the base scene and press Play. Run a second instance (build + editor) to test multiplayer.

## Documentation

The full technical report (in Swedish) is available in [`GymnasieProjekt_Rapport.pdf`](GymnasieProjekt_Rapport.pdf).

## Credits

- Movement, multiplayer and procedural animation implementations were based on tutorials by Brackeys, Dave / GameDevelopment and Dapper Dino.
- Character: "Banana Man" by BOXOPHOBIC (Unity Asset Store)
- Animations: "Human Basic Motions FREE" by Kevin Iglesias (Unity Asset Store)
