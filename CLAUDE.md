# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Third-Person Archer is a Unity 6 (6000.3.10f1) mobile game using the Universal Render Pipeline (URP). The Unity project root is `Third Person Archer/` inside this repository.

## Development

This is a Unity project — there is no CLI build command. All development is done through the **Unity Editor** (open `Third Person Archer/` as a Unity project). There are no automated tests to run from the command line.

Key third-party packages (see `Third Person Archer/Packages/manifest.json`):
- **DOTween** (`DG.Tweening`) — tweening/animation
- **Cinemachine 2.10.5** — camera control
- **MoreMountains Feedbacks** — game feel (hit effects, VFX triggers)
- **Unity AI Navigation 2.0.10** — NavMesh for enemy pathfinding
- **Post Processing 3.5.1** — visual post-processing
- **AppMetrica** — analytics

## Architecture

All custom game code lives in `Third Person Archer/Assets/Scripts/`.

### Actor System (`Actor/`)

`ActorController` (namespace `Actor`) is the central entity component attached to every game entity (player, enemy, projectile, etc.). On `Awake` it discovers its sibling components via `GetComponentsInChildren`:

- **`Input`** subclasses — raw input sources (move, aim, shoot, attack)
- **`System`** subclasses — behavioral logic (Health, Mover, Aimer, FpvController, etc.)
- **`Property`** subclasses — data values (Speed, Damage, MaxHealth, Ammo, etc.)

Systems and inputs receive a reference to `ActorController` through the `IActorIniter` interface (`InitActor(ActorController)`). Use `actor.TryGetSystem<T>()`, `actor.TryGetProperty<T>()`, and `actor.TryGetInput<T>()` to access components — these traverse the inheritance chain, not just exact type matches.

### State Machine (`State Machine/`)

A custom, MonoBehaviour-based hierarchical state machine:

- **`StateMashine`** — drives the machine; polls `MainState.GetNextState()` each frame and calls `Enter()`/`Exit()` on transitions.
- **`MainState : State`** — a state node. Contains a list of `StateTransition` children and `SubState` children (both discovered via `GetComponentsInChildren` in `Awake`).
- **`StateTransition`** — returns `IsTransit = true` + `NextState` when its condition is met. Transitions are active only while their parent state is active.
- **`SubState`** — side-effect components that run alongside a `MainState` (e.g., play animation, change camera, spawn VFX). They are entered/exited with their parent state.
- **`State`** — base class; toggles `this.enabled` on Enter/Exit.

States, transitions, and substates are all GameObjects or components in the scene hierarchy. This means the entire state graph for an actor is visible and editable in the Unity Inspector.

### Weapon System (`Weapons/`)

Weapons are broken into controller/holder/state files per weapon type:
- `Bow/` — bow with draw spring physics and FPV aim mode
- `CrossBow/` — crossbow with reload states
- `Spear/` — spear throw
- `Throw/` — boomerang and shuriken

`WeaponController` is the base. `ProjectileWeaponController` / `RangeWeaponController` extend it. `Shooter/` contains the shoot logic (`ProjectileShooter`, `RaycastShooter`). `Projectile` is itself an `ActorController` with its own Properties and Systems.

### Custom Animation System (`Animations/Custom/`)

The game uses a fully custom IK-driven animation system (no Unity Animator state machine for gameplay animations):

- **`AnimatorController`** — drives an `IActiveAnimation`, blending between `AnimationPose` states.
- **`BodyIK` / `BodyIKAnimatorController`** — procedural IK for the character body.
- **`HandAnimatorController`** — finger-level IK pose blending for hands.
- **`FpvAnimatorController`** — first-person view weapon animation.
- Animators come in three flavors: `Instant`, `Tween` (DOTween eased), and `Spring` (spring-damper physics).

### UI Systems

There are two coexisting UI systems:
- **`UI/`** — older component-based UI (health bars, ammo counts, crosshair, tabs, skin selector).
- **`UI New/`** — newer navigator/registry pattern. `UINavigator` + `ScreenRegistry` manage screen transitions. `ScreenView` is the base for all screens. A `ServiceLocator` provides access to UI services. Screens pass typed args via `IReceivesArgs<T>`.

### Other Key Systems

- **`Spring Motion/`** — reusable spring-damper math (`SpringFloat`, `SpringVector3`, etc.) used for camera sway and weapon feel.
- **`Elemental/`** — elemental damage types (fire, ice, etc.) via `ElementalController` and `ElementalType`.
- **`Sequence/`** — linear scripted sequences (`SequenceManager` runs `ISequence` steps: move, jump, shoot, delay).
- **`Skin/`** — player/weapon skin selection and application (`PlayerSkinDatabase`, `PlayerSkinController`).
- **`Upgrade/`** — stat upgrades (`Upgrader`, `UpgradeData`).
- **`Enemy/`** — enemy archetypes and stat configuration (`EnemyStatsConfig`, `EnemyStatsApplier`).
- **`Animations/Custom/Body/`** — `BodyIKWorldBinder` connects IK targets to world-space objects (e.g., bow aim target).
