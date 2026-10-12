# SOULBREAK — Systems Report

## The game
Top-down center defense. Monsters attack a soul reactor in **waves** from every direction.
Aim with the mouse and click to fire through the **weapon ring** — a ring of slots around the
reactor; the slot your aim passes through decides the shot. Fired modules empty out and reload a
new random one. Kill enemies for **souls** (chained fast kills build a **combo**), shoot the orbs they
drop for bonus souls or a shield charge, and fire the **Nova** (SPACE) when overwhelmed. Between
waves the **Soul Shop** offers 5 random perks: *buffs* (stats) and *techniques* (new ring modules).
Every 5th wave is a boss wave.

## How I worked (prompting rules)
I followed the workshop rule of building **subsystems, not the whole game**: first a plan of
systems, then one subsystem at a time (Reactor → Enemies/Spawner → Shooting → Souls/Orbs → Perks
→ UI), compiling and play-testing after each step in Unity. Later iterations added several related
subsystems per step (waves + ammo + combo, then shop + perks, then visuals), always as separate
classes wired through events, and each step was tested before moving on.

## Following the e-book (*Level up your code with design patterns and SOLID*)

### Observer pattern — two flavors
1. **ScriptableObject event channels** (`Core/Events/EventChannel<T>`) for events that many
   unrelated systems care about or that runtime-spawned objects raise. A channel is an asset in
   `Assets/Data/Events`; broadcasters call `Raise()`, listeners subscribe to `OnRaised`. Neither side
   references the other. There are **no static/global events** in the project.
   | Channel | Raised by | Listened to by |
   |---|---|---|
   | `EnemySpawned` | `Enemy` | `WaveManager` (counts living enemies) |
   | `EnemyKilled` | `Enemy` | `SoulWallet`, `ComboSystem`, `OrbDropper`, `EnemySplitter`, `FxSystem`, `CameraShake`, `WaveManager` |
   | `EnemyReachedReactor` | `Enemy` | `WaveManager` |
   | `EnemyProjectileShotDown` | `EnemyProjectile` | `FxSystem` |
   | `OrbCollected` | `Orb` | `SoulWallet`, `ReactorHealer`, `FxSystem` |
   | `Explosions` / `ChainLightning` / `Beams` | weapon modules | `FxSystem` |
2. **Plain C# events** on scene systems for their own state (`Reactor.OnHealthChanged`,
   `SoulWallet.OnSoulsChanged`, `WaveManager.OnWaveCleared`, `PerkShop.OnPerkBought`,
   `PauseController.OnPauseChanged`…). Listeners subscribe in `OnEnable`, unsubscribe in `OnDisable`.

### SOLID
- **S — Single responsibility.** One reason to change per class. Gameplay and display are split
  (`Reactor`/`ReactorView`, `Enemy`/`EnemyView`, `WeaponRing`/`WeaponRingView`, `PerkShop`/`ShopView`).
  The HUD is 8 small views (`ReactorPanelView`, `SoulCounterView`, `AmmoView`, `NovaView`,
  `ComboView`, `BannerView`, `GameOverView` on a shared `HUDCanvas`). Pause is three classes:
  `PauseController` (time), `PauseEffect` (post-processing), `PauseMenuView` (UI).
- **O — Open/closed.** Enemy types, perks, weapon modules and channels are ScriptableObject assets:
  new content = new asset, no code change. `PlayerStats` is a stat bag keyed by `StatType`.
- **L — Liskov.** Every `ShotPattern` / `PerkEffect` subclass can replace its base without the caller
  (`PlayerShooter`, `PerkShop`) knowing which one it is.
- **I — Interface segregation.** Small interfaces: `IShootable` (can be clicked), `IDamageable`
  (can take damage), `IHostile` (a valid weapon target).
- **D — Dependency inversion.** Weapons, projectiles and the Nova target `IHostile` — they never
  reference the `Enemy` class. The shooter only knows `ShotPattern` and `IShootable`.

### Design patterns
- **Observer** — event channels + C# events (above).
- **Strategy** — `PerkEffect` (stat / reactor / weapon-module effects) and `ShotPattern`
  (laser, projectile volleys, chain lightning, beam, orbital blades, black hole).
- **Factory** — `EnemyFactory` is the only place that assembles an enemy.
- **Mediator** — `GameManager` connects `WaveManager` and `PerkShop` (wave cleared → open shop →
  shop closed → next wave); they never reference each other.
- **Input separated from logic** — only controllers read input (`PlayerController`,
  `GameHotkeys`, `ShopHotkeys`, `MainMenuInput`) and send commands (`Fire()`, `Reload()`,
  `TryUse()`, `Toggle()`, `TryBuy()`). Gameplay systems and views never read the keyboard/mouse.

## Deadlock-inspired features (all plugged in as listeners — no existing system had to know about them)
| Feature | How it's built |
|---|---|
| **Headshots** | `IWeakPoint` on enemies (their glowing eye). The hitscan shot checks it, deals `HeadshotMultiplier` damage and raises the `Headshots` channel → popup, hit stop, sound. |
| **Hit stop + impact flash** | `HitStop` listens to headshots / big kills / reactor hits / boss enrage and asks `TimeScaleService` for a 30-300 ms freeze; `ScreenFlashView` flashes. |
| **TimeScaleService** | Single owner of `Time.timeScale`: pause, hit-stop, overdrive and game-over are *layers* that multiply — they never overwrite each other. |
| **Overdrive (ultimate, Q)** | `OverdriveSystem` fills from kills; while active every shot fires **all** owned techniques, ammo is free, time slows; a purple post-process volume blends in. |
| **Kill streaks** | `ComboSystem.OnMilestone` → `StreakRewards` (bonus souls) → `Announcements` channel → `AnnouncerView` ("RAMPAGE", "GODLIKE"...). |
| **Elite enemies** | `EnemyModifier` flags (Shielded / Swift / Splitting / Blink) rolled by `WaveManager`; aura + shield bubble drawn by the enemy shader; 3x rewards. |
| **Two-phase boss** | `BossBrain` (added by the factory): summons minions, then enrages at 50% (raises `BossEnraged`) and fires bullet rings. `BossHealthView` shows the bar. |
| **Technique synergies** | `SynergyDefinition` assets (e.g. Black Hole + Chain Lightning = *Event Horizon*). `SynergySystem` fires a bonus pattern when the trigger fires; shop cards show synergy hints. |
| **Map events** | `MapEventSystem`: Soul Urn (shoot it before it escapes), Solar Flare (shield offline), Rejuvenator (kill for a power surge). |
| **Danger awareness** | `EnemyRegistry` (built from channels) feeds `DangerView`: edge arrows for off-screen enemies + pulsing red vignette. Reactor shader cracks and throbs at low health. |
| **Procedural audio** | `SfxSynth` synthesizes every sound in code; `AudioDirector` maps events → sounds; `MusicSynth` is a procedural synthwave track whose layers follow `MusicDirector` intensity (waves, danger, boss, overdrive) and muffles while paused/shopping. |
| **Meta progression** | `MetaProgress` (PlayerPrefs JSON) stores Soul Essence earned per run (`MetaRecorder`); the main-menu **Ascension** screen unlocks starting techniques and reactor skins (`MetaCatalog` asset); `StartingLoadout` applies them. |

## System breakdown

| System | Scripts |
|---|---|
| **Core** | `IShootable`, `IDamageable`, `IHostile`, `InputReader`, `EventChannel<T>` |
| **Reactor** | `Reactor`, `ReactorConfig`, `ReactorHealer`, `ReactorView` |
| **Enemies** | `Enemy`, `EnemyConfig`, `EnemyChannels`, `EnemyFactory`, `EnemyView`, `EnemyRangedAttack`, `EnemyProjectile`, `EnemySplitter`, `WaveManager`, channel types |
| **Player** | `PlayerController` (input), `PlayerStats`, `AmmoSystem`, `PlayerShooter`, `NovaAbility` |
| **Weapons** | `WeaponRing`, `WeaponRingView`, `ShotPattern` + 6 strategies, `PlayerProjectile`, `OrbitBlades`, `Vortex`, channel types |
| **Orbs** | `Orb`, `OrbDropper`, `OrbEventChannel` |
| **Economy** | `SoulWallet`, `ComboSystem`, `SoulInterest` |
| **Perks** | `PerkShop`, `ShopHotkeys` (input), `PerkDefinition`, `PerkEffect` + `StatPerkEffect`, `ReactorPerkEffect`, `WeaponModulePerkEffect`, `AdminCheats` (debug only) |
| **Game Flow** | `GameManager` (mediator), `GameHotkeys` (input), `PauseController`, `SceneFader` |
| **Fx** | `FxSystem`, `CameraShake`, `PauseEffect`, `FxAnimator`, `TracerFx`, `LightningFx` |
| **UI** | `HUDCanvas` + 7 HUD views, `ShopView`, `PauseMenuView`, `UIFactory`, `UITheme`, `UIButtonFx`, `GlitchText`, `FloatingText`, `CursorStyle` |
| **Menu** | `MainMenuView`, `MainMenuInput` (input), `MenuAmbience` |

## Example event chain — killing a Splitter
```
PlayerController --click--> PlayerShooter.Fire() --> AmmoSystem.TryConsume()
  --> WeaponRing slot's ShotPattern.Fire() --> IShootable.Shoot()
Enemy.TakeDamage() --> OnDamaged --> EnemyView flashes + updates health ring
Enemy health 0 --> EnemyKilled channel.Raise(enemy)
    ├── ComboSystem   -> combo +1        -> OnComboChanged  -> ComboView
    ├── SoulWallet    -> souls x combo   -> OnSoulsChanged  -> SoulCounterView (+N popup)
    ├── OrbDropper    -> maybe spawns an orb (raises OrbCollected when shot)
    ├── EnemySplitter -> spawns 3 Splitlings (each raises EnemySpawned)
    ├── WaveManager   -> living enemies -1
    ├── FxSystem      -> shockwave ring + dissolving corpse (shader)
    └── CameraShake   -> small shake
Last enemy gone --> WaveManager.OnWaveCleared
    ├── SoulInterest -> pays interest
    ├── BannerView   -> "WAVE CLEARED"
    └── GameManager  -> PerkShop.Open() -> ShopView shows cards
```

## Shaders (hand-written, `Assets/Shaders`)
Reactor (swirling core, segmented shield ring, hit flash) · Enemy (procedural spiky body, hit flash,
health ring, noise dissolve) · Orb · Ring (shockwaves) · Tracer · Background (nebula + grid) ·
RingSegment (weapon-ring slots with icons, reload sweep, pop) · Vortex (black hole) ·
UIPanel (rounded glowing UI frames) · UICard (buff vs technique shop cards). Post-processing:
bloom, vignette, chromatic aberration; a separate pause volume desaturates and warps.

## Editor tool
`Soulbreak → Setup Everything` (`Assets/Editor/SoulbreakSetup.cs`) creates every asset (sprites,
materials, enemies, perks, weapon modules, event channels, UI theme, post-processing), wires the
game scene, builds the main menu and registers both scenes in Build Settings. Safe to re-run.

## Controls
- **Left click** — shoot through the weapon ring
- **R / Right click** — reload
- **Space** — Nova blast
- **Q** — Overdrive (when the meter is full)
- Click an enemy's **eye** for a headshot
- **1-5** or click — buy in the shop
- **ESC / P** — pause
- **R** — restart after game over
- **F1** — admin: grant everything (Editor / development builds only)
