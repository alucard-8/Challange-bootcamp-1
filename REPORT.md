# SOULBREAK — Systems Report

## The game
Top-down center defense. Monsters attack a soul reactor in **waves** from every direction.
Click to shoot (limited **ammo**, reload with R / right click), shoot the orbs enemies drop for bonus
souls or shield charges, chain fast kills into a **combo** for more souls, and fire the **Nova** blast
(SPACE) when overwhelmed. Between waves the **Soul Shop** opens with 5 random perks (out of 21)
you can buy or reroll. Every 5th wave is a **boss wave**.

## Thought process
I followed the Unity *Level up your code with design patterns and SOLID* e-book:

1. **Split the game into systems first, code second.** I listed what the game *does* (spawn waves,
   move, shoot, reload, reward, upgrade, display, end) and gave each its own class and folder.
2. **Single Responsibility.** Every script has one reason to change. `Enemy` moves and dies — it does
   not give souls, drop orbs, split, flash or dissolve. `EnemyView` draws it; `OrbDropper`,
   `EnemySplitter`, `SoulWallet`, `FxSystem` react to its death.
3. **Observer pattern with C# events.** Systems announce *what happened* (`OnAnyKilled`,
   `OnWaveCleared`, `OnSoulsChanged`, `OnHit`…). Listeners subscribe in `OnEnable` and unsubscribe in
   `OnDisable`. The publisher never knows who is listening.
4. **Strategy pattern for perks.** A perk is data (`PerkDefinition`) holding a list of `PerkEffect`
   strategies (`StatPerkEffect`, `ReactorPerkEffect`). The shop just calls `Apply()` — no switch on
   perk types. Adding a perk = making an asset.
5. **Weapon ring (Strategy again).** A ring of slots surrounds the reactor; the slot your aim passes
   through decides what your click fires. Each module is a `ShotPattern` strategy (Soul Laser,
   Triple Shot, Soul Cannon, Seeker Swarm). Weapon perks install modules; every wave the ring rolls a
   random number of slots (5-10) and reshuffles where the modules sit, and it slowly rotates.
6. **Mediator for the game loop.** `WaveManager` and `PerkShop` never reference each other.
   `GameManager` connects them: wave cleared → open shop → shop closed → next wave.
6. **Factory pattern.** `EnemyFactory` is the only place that knows how an enemy is assembled.
7. **Open/Closed via ScriptableObjects & a stat bag.** Enemy types, perks and reactor stats are
   assets. `PlayerStats` is a dictionary of `StatType` values, so a new upgradeable stat is one enum line.
8. **Dependency Inversion via interfaces.** The shooter only knows `IShootable` (enemies, orbs and
   enemy bullets all implement it); damage only goes through `IDamageable`.
9. **Model / View split.** Gameplay classes hold state; `ReactorView`, `EnemyView`, `HUD`, `ShopView`
   and `FxSystem` only display it. Deleting every view still leaves a working game.

## System breakdown

| System | Scripts | Publishes | Listens to |
|---|---|---|---|
| **Core** | `IDamageable`, `IShootable`, `InputReader` | — | — |
| **Reactor** | `Reactor`, `ReactorConfig`, `ReactorHealer`, `ReactorView` | `OnHealthChanged`, `OnShieldChanged`, `OnHit`, `OnDestroyed` | `Orb.OnAnyCollected` (heal) |
| **Enemies** | `Enemy`, `EnemyConfig`, `EnemyFactory`, `EnemyView`, `EnemyRangedAttack`, `EnemyProjectile`, `EnemySplitter`, `WaveManager` | `Enemy.OnAnyKilled`, `OnDamaged`, `OnWaveStarted`, `OnWaveCleared`, `OnAnyShotDown` | `Enemy.OnAnyKilled` (splitter) |
| **Player** | `PlayerStats`, `AmmoSystem`, `PlayerShooter`, `NovaAbility` | `OnStatChanged`, `OnAmmoChanged`, `OnReloadStarted`, `OnShotFired`, `OnUsed`, `OnReady` | `OnStatChanged` (ammo) |
| **Weapons** | `WeaponRing`, `WeaponRingView`, `ShotPattern` (+ `HitscanShotPattern`, `ProjectileShotPattern`), `PlayerProjectile` | `OnLayoutChanged`, `PlayerProjectile.OnAnyExploded` | — |
| **Orbs** | `Orb`, `OrbDropper` | `Orb.OnAnyCollected` | `Enemy.OnAnyKilled` |
| **Economy** | `SoulWallet`, `ComboSystem`, `SoulInterest` | `OnSoulsChanged`, `OnSoulsGained`, `OnComboChanged` | kills, orbs, `OnWaveCleared` |
| **Perks** | `PerkShop`, `PerkDefinition`, `PerkEffect` (+ `StatPerkEffect`, `ReactorPerkEffect`) | `OnOpened`, `OnClosed`, `OnOffersChanged`, `OnPerkBought` | — |
| **Game Flow** | `GameManager` (mediator) | `OnStateChanged` | `OnWaveCleared`, `shop.OnClosed`, `Reactor.OnDestroyed` |
| **Fx** | `FxSystem`, `FxAnimator`, `TracerFx`, `CameraShake` | — | shots, kills, orbs, nova, reactor hits |
| **UI** | `HUD`, `ShopView`, `FloatingText`, `UIFactory` | — | everything above |

## Example event chain — killing a Splitter
```
PlayerShooter --click--> AmmoSystem.TryConsume() --> IShootable.Shoot()
Enemy.TakeDamage() --> OnDamaged --> EnemyView flashes + updates health ring
Enemy health 0 --> Enemy.OnAnyKilled
    ├── ComboSystem   -> combo +1           -> OnComboChanged -> HUD
    ├── SoulWallet    -> souls x combo      -> OnSoulsChanged / OnSoulsGained -> HUD (+N popup)
    ├── OrbDropper    -> maybe spawns orb
    ├── EnemySplitter -> spawns 3 Splitlings
    ├── FxSystem      -> shockwave ring + dissolving corpse (shader)
    └── CameraShake   -> small shake
Last enemy dead --> WaveManager.OnWaveCleared
    ├── SoulInterest -> pays interest
    └── GameManager  -> PerkShop.Open() -> ShopView shows cards
```

## Shaders (URP 2D, hand-written HLSL in `Assets/Shaders`)
- **Reactor** — swirling energy core that dims/flickers with health, segmented shield ring (one
  segment per charge), red hit flash.
- **Enemy** — procedural spiky silhouette, glowing rim/eye, hit flash, health ring, noise **dissolve** on death.
- **Orb** — pulsing HDR core with swirling ring (bloom makes it glow).
- **Ring** — expanding shockwave for impacts, kills, orb pickups and Nova.
- **Tracer** — laser beam for shots (gold on critical hits).
- **Background** — drifting nebula, rippling grid and a glow around the reactor.
- Post-processing volume: bloom, vignette, slight chromatic aberration.

## Editor tool
`Soulbreak → Setup Scene` (in `Assets/Editor/SoulbreakSetup.cs`) creates every asset (sprite,
materials, 7 enemy types, 21 perks, post-processing) and wires the scene. Re-running it is safe.

## Controls
- **Left click** — shoot enemies / orbs / enemy bullets
- **R / Right click** — reload
- **Space** — Nova blast
- **1-5** or click — buy perks in the shop
- **R** — restart after game over
