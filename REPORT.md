# SOULBREAK Systems Report

## The game
Top down center defense. Monsters attack a soul reactor in **waves** from every direction.
Aim with the mouse and click to fire through the **weapon ring**, a ring of slots around the
reactor; the slot your aim passes through decides the shot. Fired modules empty out and swap in a
new random one. Kill enemies for **souls** (chained fast kills build a **combo**), shoot the orbs they
drop for bonus souls or a shield charge, and fire the **Nova** (SPACE) when overwhelmed. Between
waves the **Soul Shop** offers 5 random perks: *buffs* (stats) and *techniques* (new ring modules).
Every 5th wave is a boss wave. Ammo refills on its own, one round at a time.

## My thought process: how I divided the game

### 1. Start from the game loop, not from the code
Before writing anything I wrote the loop in one sentence:
*enemies spawn in waves, walk to the reactor, the player shoots them through the ring, kills give
souls, souls buy perks between waves, the run ends when the reactor breaks.*
Every noun in that sentence became a **system** and every verb became an **event** between systems:

| Part of the loop | System | What it announces |
|---|---|---|
| enemies spawn in waves | **Enemies** (`WaveManager`, `EnemyFactory`, `Enemy`) | `EnemySpawned`, `EnemyKilled`, `EnemyReachedReactor`, `OnWaveCleared` |
| walk to the reactor | **Reactor** (`Reactor`, `ReactorHealer`) | `OnHealthChanged`, `OnDestroyed` |
| the player shoots through the ring | **Player** + **Weapons** (`PlayerShooter`, `AmmoSystem`, `WeaponRing`, `ShotPattern`s) | `OnAmmoChanged`, `OnSlotEmptied` |
| kills give souls | **Economy** + **Orbs** (`SoulWallet`, `ComboSystem`, `OrbDropper`) | `OnSoulsChanged`, `OrbCollected` |
| souls buy perks between waves | **Perks** (`PerkShop`, `PerkEffect`s) | `OnPerkBought`, `OnClosed` |
| the run ends | **Game Flow** (`GameManager`, `PauseController`, `RunStats`) | `OnStateChanged` (game over) |

Everything else (UI, audio, effects, meta progression, leaderboard) only **reacts** to those
events, so they became their own systems that sit on the outside and listen.

### 2. One question decides where code goes: "who owns this data?"
* The reactor owns its health, so only `Reactor` changes it. Enemies don't damage it directly; they reach it and the reactor takes the hit.
* The wallet owns souls, so the shop asks `SoulWallet.TrySpend()` instead of editing a number.
* `TimeScaleService` owns `Time.timeScale`. Pause, hit stop and overdrive ask it for a layer instead of fighting over the value.
If two classes wanted to change the same data, that was the sign one of them was in the wrong system.

### 3. Split each system into subsystems by reason to change
A system got split as soon as one class had two jobs:
* **Logic vs display**: `Reactor` / `ReactorView`, `Enemy` / `EnemyView`, `WeaponRing` / `WeaponRingView`, `PerkShop` / `ShopView`. I can restyle a view without touching gameplay.
* **Input vs logic**: only controllers read the keyboard and mouse (`PlayerController`, `GameHotkeys`, `ShopHotkeys`, `MainMenuInput`). They call commands like `Fire()` or `TryBuy()`, so gameplay code never knows which key was pressed.
* **Data vs behaviour**: enemy types, perks, weapon modules, modes and difficulty are ScriptableObject assets. Adding content means adding an asset, not editing code.
* **HUD**: instead of one giant HUD script there is one small view per element (`AmmoView`, `SoulCounterView`, `ComboView`, `BannerView` and more), each listening to just the event it needs.

### 4. Pick how systems talk
I used the Observer pattern in two ways and chose between them with one rule:
* **ScriptableObject event channels** when the sender is spawned at runtime or many unrelated systems listen. An `Enemy` doesn't know the wallet, the combo, the FX or the wave manager exist; it raises `EnemyKilled` and they all react.
* **Plain C# events** when a single scene system reports its own state (`Reactor.OnHealthChanged`, `SoulWallet.OnSoulsChanged`). Listeners subscribe in `OnEnable` and unsubscribe in `OnDisable`.
When two systems needed to coordinate (wave cleared → open shop → shop closed → next wave), a small mediator, `GameManager`, connects them so they never reference each other.

### 5. Deadlock as the inspiration
Once the core loop worked, I took mechanics from **Deadlock** (Valve's hero shooter) to make each
run feel faster and more skill-based. Every one was added as a new subsystem that listens to events
that already existed, so no older system had to change:
* **Headshots**: enemies have a weak point (their eye). Hitting it deals bonus damage and triggers a popup, hit stop and sound.
* **Hit stop and impact flash**: big hits freeze time for a moment, like Deadlock's heavy hits.
* **Ultimate (Overdrive, Q)**: a meter fills from kills; when used, every owned technique fires at once with free ammo and slowed time.
* **Kill streaks with an announcer**: "RAMPAGE", "GODLIKE" and bonus souls.
* **Souls economy and shop**: like Deadlock's souls, kills give currency you spend on items between waves.
* **Elite enemies and a two-phase boss** that enrages at half health.
* **Item synergies**: certain technique pairs unlock a bonus effect.
* **Map events**: a Soul Urn to shoot before it escapes, a solar flare and a rejuvenator.

The full list is in the *Deadlock inspired features* section below.

## My approach to writing the code
* **One subsystem at a time** (the workshop prompting rule). The order followed the loop: Reactor → Enemies and waves → Shooting and ammo → Souls and orbs → Perks and shop → UI → audio and effects → meta progression → online. I compiled and play tested in Unity after each one before moving on.
* **New features plug in as listeners.** Later features like headshots, kill streaks, overdrive, the tutorial, the index and the leaderboard were added by subscribing to existing events. Older systems didn't need to change, which is how I knew the split was right.
* **Test before building on top.** Each subsystem had to work alone first (for example the wallet counting souls from `EnemyKilled` before any shop existed).
* **Interfaces over concrete classes.** Weapons hit an `IHostile` and the shooter clicks an `IShootable`, so new targets like the Soul Urn worked with no weapon changes.
* **UI built as prefabs**: every screen is a prefab with serialized references; the view scripts only react to events and animate.
* **Balance through data.** Tuning happened in assets and stat values, for example lowering the automatic reload from 2s to 1.7s per bullet after playtests, without touching any system logic.

## Following the design patterns ebook (*Level up your code with design patterns and SOLID*)

### Observer pattern, two flavors
1. **ScriptableObject event channels** (`Core/Events/EventChannel<T>`) for events that many
   unrelated systems care about or that runtime spawned objects raise. A channel is an asset in
   `Assets/Data/Events`; broadcasters call `Raise()`, listeners subscribe to `OnRaised`. Neither side
   references the other. There are **no static or global events** in the project.
   * `EnemySpawned` raised by `Enemy`, heard by `WaveManager` (counts living enemies)
   * `EnemyKilled` raised by `Enemy`, heard by `SoulWallet`, `ComboSystem`, `OrbDropper`, `EnemySplitter`, `FxSystem`, `CameraShake`, `WaveManager`
   * `EnemyReachedReactor` raised by `Enemy`, heard by `WaveManager`
   * `EnemyProjectileShotDown` raised by `EnemyProjectile`, heard by `FxSystem`
   * `OrbCollected` raised by `Orb`, heard by `SoulWallet`, `ReactorHealer`, `FxSystem`
   * `Explosions`, `ChainLightning` and `Beams` raised by weapon modules, heard by `FxSystem`
2. **Plain C# events** on scene systems for their own state (`Reactor.OnHealthChanged`,
   `SoulWallet.OnSoulsChanged`, `WaveManager.OnWaveCleared`, `PerkShop.OnPerkBought`,
   `PauseController.OnPauseChanged` and more). Listeners subscribe in `OnEnable`, unsubscribe in `OnDisable`.

### SOLID
* **S: Single responsibility.** One reason to change per class. Gameplay and display are split
  (`Reactor`/`ReactorView`, `Enemy`/`EnemyView`, `WeaponRing`/`WeaponRingView`, `PerkShop`/`ShopView`).
  The HUD is many small views on a shared `HUDCanvas` (`ReactorPanelView`, `SoulCounterView`,
  `AmmoView`, `NovaView`, `ComboView`, `BannerView`, `GameOverView` and more). Pause is three classes:
  `PauseController` (time), `PauseEffect` (post processing), `PauseMenuView` (UI).
* **O: Open/closed.** Enemy types, perks, weapon modules and channels are ScriptableObject assets:
  new content = new asset, no code change. `PlayerStats` is a stat bag keyed by `StatType`.
* **L: Liskov.** Every `ShotPattern` / `PerkEffect` subclass can replace its base without the caller
  (`PlayerShooter`, `PerkShop`) knowing which one it is.
* **I: Interface segregation.** Small interfaces: `IShootable` (can be clicked), `IDamageable`
  (can take damage), `IHostile` (a valid weapon target), `IWeakPoint` (a weak spot).
* **D: Dependency inversion.** Weapons, projectiles and the Nova target `IHostile`: they never
  reference the `Enemy` class. The shooter only knows `ShotPattern` and `IShootable`.

### Design patterns
* **Observer**: event channels + C# events (above).
* **Strategy**: `PerkEffect` (stat / reactor / weapon module effects) and `ShotPattern`
  (laser, projectile volleys, chain lightning, beam, orbital blades, black hole).
* **Factory**: `EnemyFactory` is the only place that assembles an enemy.
* **Mediator**: `GameManager` connects `WaveManager` and `PerkShop` (wave cleared → open shop →
  shop closed → next wave); they never reference each other.
* **Input separated from logic**: only controllers read input (`PlayerController`,
  `GameHotkeys`, `ShopHotkeys`, `MainMenuInput`) and send commands (`Fire()`,
  `TryUse()`, `Toggle()`, `TryBuy()`). Gameplay systems and views never read the keyboard or mouse.

## Deadlock inspired features (all plugged in as listeners; no existing system had to know about them)
* **Headshots**: `IWeakPoint` on enemies (their glowing eye). The hitscan shot checks it, deals `HeadshotMultiplier` damage and raises the `Headshots` channel, which triggers a popup, hit stop and sound.
* **Hit stop + impact flash**: `HitStop` listens to headshots, big kills, reactor hits and boss enrage and asks `TimeScaleService` for a 30 to 300 ms freeze; `ScreenFlashView` flashes.
* **TimeScaleService**: single owner of `Time.timeScale`: pause, hit stop, overdrive and game over are *layers* that multiply; they never overwrite each other.
* **Overdrive (ultimate, Q)**: `OverdriveSystem` fills from kills; while active every shot fires **all** owned techniques, ammo is free and time slows; a purple post process volume blends in.
* **Kill streaks**: `ComboSystem.OnMilestone` → `StreakRewards` (bonus souls) → the `Announcements` channel → `AnnouncerView` ("RAMPAGE", "GODLIKE" and more).
* **Elite enemies**: `EnemyModifier` flags (Shielded / Swift / Splitting / Blink) rolled by `WaveManager`; aura + shield bubble drawn by the enemy shader; 3x rewards.
* **Two phase boss**: `BossBrain` (added by the factory) summons minions, then enrages at 50% (raises `BossEnraged`) and fires bullet rings. `BossHealthView` shows the bar.
* **Technique synergies**: `SynergyDefinition` assets (e.g. Black Hole + Chain Lightning = *Event Horizon*). `SynergySystem` fires a bonus pattern when the trigger fires; shop cards show synergy hints.
* **Map events**: `MapEventSystem`: Soul Urn (shoot it before it escapes), Solar Flare (shield offline), Rejuvenator (kill for a power surge).
* **Danger awareness**: `EnemyRegistry` (built from channels) feeds `DangerView`: edge arrows for offscreen enemies + a pulsing red vignette. The reactor shader cracks and throbs at low health.
* **Audio**: `AudioManager` is a lazy singleton with a pooled set of `AudioSource`s that plays clips from the `AudioLibrary` asset (falling back to `Resources/AudioLibrary`); `AudioDirector` maps gameplay events to `Sfx` ids; `MusicDirector` drives music intensity and muffle; `UIAudio` covers UI clicks.
* **Meta progression**: `MetaProgress` (PlayerPrefs JSON) stores Soul Essence earned per run (`MetaRecorder`); the main menu **Ascension** screen unlocks starting techniques and reactor skins (`MetaCatalog` asset); `StartingLoadout` applies them.

## Second mode: STAR CORE (secret)
Click the main menu core **7 times quickly**: it collapses and reignites as a star. The Ascension
lobby shows a locked "???" card with a hint until then; afterwards that card toggles the mode.
* **ModeProfile** (ScriptableObject): a whole mode as data: enemy roster + boss, perk catalog, core/background/enemy materials, orb colors, music variant and a wording lexicon ("CORE" → "STAR", "SOULS" → "STARDUST"). New mode = new asset (**Open/Closed**).
* **ModeApplier**: mediator that runs first in the game scene and hands each system its data through a small setter (`SetRoster`, `SetCatalog`, `SetColors` and more). No gameplay system knows modes exist.
* **MovementPattern** (Strategy): `SpiralMovement` (planets on decaying orbits, gas giants holding orbit), `CometMovement` (slingshot loop then dive). Null = straight line, so Soul Reactor enemies are unchanged.
* **GravityWell**: the black hole boss "The Devourer" drags other bodies toward the star.
* **Shaders**: `Star` (plasma, corona, sunspots, magnetosphere), `Planet` (lit from the star with a terminator; rocky / gas bands / icy), `SpaceBackground` (parallax starfield + nebulae). They share the reactor and enemy property interface, so `ReactorView` / `EnemyView` drive them unchanged (substitutability for shaders).

## Core upgrades (both modes)
`CoreUpgradeView` listens to the shop and the ring: offense perks grow **spikes** (prominences on the
star), defense perks lock on **armor plates** (an asteroid belt), economy perks add a **golden halo**
(bigger corona), every owned technique adds an **orbiting satellite or moon**, and every 4 perks the
core reaches a new **tier** (brighter, pulse ring, announcement).

## System breakdown
* **Core**: `IShootable`, `IDamageable`, `IHostile`, `IWeakPoint`, `InputReader`, `EventChannel<T>`, `GameSettings`
* **Reactor**: `Reactor`, `ReactorConfig`, `ReactorHealer`, `ReactorView`, `CoreUpgradeView`
* **Enemies**: `Enemy`, `EnemyConfig`, `EnemyFactory`, `EnemyView`, `EnemyRegistry`, `EnemyModifier`, `EnemyBlink`, `EnemySplitter`, `EnemyRangedAttack`, `EnemyProjectile`, `BossBrain`, `WaveManager`, `MovementPattern`, `SpiralMovement`, `CometMovement`, `GravityWell`, channel types
* **Player**: `PlayerController` (input), `PlayerStats`, `AmmoSystem` (auto recharge, no manual reload), `PlayerShooter`, `NovaAbility`, `OverdriveSystem`
* **Weapons**: `WeaponRing`, `WeaponRingView`, `ShotPattern` + strategies (`HitscanShotPattern`, `ProjectileShotPattern`, `ChainShotPattern`, `BeamShotPattern`, `OrbitShotPattern`, `VortexShotPattern`), `PlayerProjectile`, `OrbitBlades`, `Vortex`, `SynergyDefinition`, `SynergySystem`, channel types
* **Orbs**: `Orb`, `OrbDropper`, `OrbEventChannel`
* **Economy**: `SoulWallet`, `ComboSystem`, `SoulInterest`, `StreakRewards`
* **Perks**: `PerkShop`, `ShopHotkeys` (input), `PerkDefinition`, `PerkEffect` + `StatPerkEffect`, `ReactorPerkEffect`, `WeaponModulePerkEffect`, `AdminCheats` (debug only)
* **Audio**: `AudioManager`, `AudioLibrary`, `AudioDirector`, `MusicDirector`, `UIAudio`
* **Game Flow**: `GameManager` (mediator), `GameHotkeys` (input), `PauseController`, `TimeScaleService`, `RunStats`, `SceneFader`
* **Fx**: `FxSystem`, `CameraShake`, `PauseEffect`, `FxAnimator`, `TracerFx`, `LightningFx`, `SparkFx`, `HitStop`, `OverdriveEffect`
* **Map Events**: `MapEventSystem`, `SoulUrn`
* **Meta**: `MetaProgress`, `MetaRecorder`, `MetaCatalog`, `StartingLoadout`, `ModeProfile`, `ModeApplier`, `ModeLibrary`, `GameModeService`, `Difficulty`, `DifficultyApplier`, `CodexCatalog`, `CodexRecorder`, `AccountService`, `LeaderboardService`, `LeaderboardSubmitter`
* **UI**: `UIFactory`, `UITheme`, `UIStrings`, `Lex`, `ShopView`, `PauseMenuView`, `SettingsView`, `AccountView`, `LeaderboardPanel`, `HowToView`, `UIFontApplier`, `UIButtonFx`, `UIConfetti`, `GlitchText`, `FloatingText`, `FlyingMote`, `CursorStyle`, `HUDCanvas` + many HUD views
* **Menu**: `MainMenuView`, `MainMenuInput` (input), `MenuAmbience`, `MenuModeApplier`, `ReactorEasterEgg`, `AscensionView`, `DifficultyView`, `IndexView`

## Example event chain: killing a Splitter
```
PlayerController click => PlayerShooter.Fire() => AmmoSystem.TryConsume()
  => WeaponRing slot ShotPattern.Fire() => IShootable.Shoot()
Enemy.TakeDamage() => OnDamaged => EnemyView flashes + updates the health ring
Enemy health 0 => EnemyKilled channel.Raise(enemy)
    * ComboSystem: combo +1, then OnComboChanged, then ComboView
    * SoulWallet: souls x combo, then OnSoulsChanged, then SoulCounterView (+N popup)
    * OrbDropper: maybe spawns an orb (raises OrbCollected when shot)
    * EnemySplitter: spawns 3 Splitlings (each raises EnemySpawned)
    * WaveManager: living enemies minus 1
    * FxSystem: shockwave ring + dissolving corpse (shader)
    * CameraShake: small shake
Last enemy gone => WaveManager.OnWaveCleared
    * SoulInterest: pays interest
    * BannerView: "WAVE CLEARED"
    * GameManager: PerkShop.Open() => ShopView shows cards
```

## Shaders (hand written, `Assets/Shaders`)
Reactor (swirling core, segmented shield ring, hit flash), Enemy (procedural spiky body, hit flash,
health ring, noise dissolve), Orb, Ring (shockwaves), Tracer, Background (nebula + grid),
RingSegment (weapon ring slots with icons, reload sweep, pop), Vortex (black hole),
UIPanel (rounded glowing UI frames), UICard (buff vs technique shop cards). Post processing:
bloom, vignette, chromatic aberration; a separate pause volume desaturates and warps.

## Editor tool
`Soulbreak → Setup Everything` (`Assets/Editor/SoulbreakSetup.cs`) creates every asset (sprites,
materials, enemies, perks, weapon modules, event channels, UI theme, post processing), wires the
game scene, builds the main menu and registers both scenes in Build Settings. Safe to re run.

## UI
* Every screen is a **prefab** in `Assets/Prefabs/UI` (HUD, Shop, PauseMenu, Settings, Index, HowTo, MainMenu, Ascension, Difficulty, Leaderboard, Account) with its own Canvas. The scene fader is `Resources/SceneFader.prefab`.
* View scripts hold `[SerializeField]` references to their prefab objects, wire button listeners in `Awake`, and only animate or react to events.
* Elements whose count changes at runtime (ammo and shield pips, damage numbers, soul motes, popups, edge arrows, confetti) are inactive templates inside the prefab, cloned with `UIFactory.Spawn`.
* `UIFontApplier` (on each canvas) applies the Bahnschrift OS font at runtime, since OS fonts cannot be stored in prefabs or redistributed.
* Text and colors live in the `UIStrings` and `UITheme` ScriptableObjects, so views hold no hardcoded copy.
* `SafeAreaFitter` wraps the content of the HUD, main menu, pause menu and shop so nothing sits under notches or cut edges.
* Settings keeps only volume, music, sound effects, screen shake, damage numbers and tutorial, plus a RESET button wired to `SettingsView.ResetAll()`.

## Online and web build
* `AccountService` signs players in as guests, or with a username and password, through Unity Gaming Services. Progress is synced with Cloud Save and the newer save wins.
* `LeaderboardService` posts every finished run (through `LeaderboardSubmitter`, which listens to `OnRunRecorded`) and fetches the global top 10. Every call fails soft, so the game works offline.
* `LeaderboardPanel` slides in from the right edge in the main menu and pause menu: best score, last run, rank and top 10.
* `Soulbreak/Build WebGL` builds a 1920x1080 browser version for itch.io; progress is saved in the browser.

## Controls
* **Left click**: shoot through the weapon ring
* **Space**: Nova blast
* **Q**: Overdrive (when the meter is full)
* Click an enemy's **eye** for a headshot
* **1 to 5** or click: buy in the shop
* **ESC / P**: pause
* **R**: restart after game over
* **F1**: admin: grant everything (Editor / development builds only)
