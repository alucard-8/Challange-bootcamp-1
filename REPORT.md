# SOULBREAK — Systems Report

## The game
Top-down center defense. Monsters come from every direction toward a soul reactor.
Click enemies to shoot them, shoot the orbs they drop for bonus souls or a shield charge,
spend souls on perks (keys 1-3), and fire the Nova blast (SPACE) when you're overwhelmed.

## Thought process
I followed the Unity *Level up your code with design patterns and SOLID* e-book:

1. **Split the game into systems first, code second.** Before writing anything I listed what the
   game *does* (spawn, move, shoot, reward, upgrade, display, end) and gave each one its own class.
2. **Single Responsibility.** Every script has one reason to change. The enemy moves and dies — it
   does not give souls, drop orbs, or update UI.
3. **Observer pattern with C# events.** Systems announce *what happened* (`OnAnyKilled`,
   `OnSoulsChanged`, `OnDestroyed`…). Other systems subscribe in `OnEnable` and unsubscribe in
   `OnDisable`. The publisher never knows who is listening.
4. **Dependency Inversion through interfaces.** The shooter only knows `IShootable`, enemies only
   damage an `IDamageable`. Adding a new shootable thing needs no shooter changes.
5. **Open/Closed through ScriptableObjects.** Enemy types, perks and reactor stats are data assets.
   New enemy or perk = new asset, no new code.
6. **Factory pattern.** `EnemyFactory` is the only place that knows how an enemy is assembled.
7. **Build one subsystem at a time** (as asked in the workshop), compiling and testing between each.

## System breakdown

| System | Scripts | Publishes | Listens to |
|---|---|---|---|
| **Core** | `IDamageable`, `IShootable`, `InputReader` | — | — |
| **Reactor** | `Reactor`, `ReactorConfig`, `ReactorHealer` | `OnHealthChanged`, `OnShieldChanged`, `OnDestroyed` | `Orb.OnAnyCollected` (heal) |
| **Enemies** | `Enemy`, `EnemyConfig`, `EnemyFactory`, `EnemySpawner` | `Enemy.OnAnyKilled`, `OnAnyReachedTarget`, `OnDifficultyChanged` | — |
| **Player** | `PlayerShooter`, `PlayerStats`, `NovaAbility` | `OnShotFired`, `OnTargetHit`, `OnStatsChanged`, `OnUsed`, `OnReady` | — |
| **Orbs** | `Orb`, `OrbDropper` | `Orb.OnAnyCollected` | `Enemy.OnAnyKilled` |
| **Economy** | `SoulWallet` | `OnSoulsChanged` | `Enemy.OnAnyKilled`, `Orb.OnAnyCollected` |
| **Perks** | `PerkShop`, `PerkConfig` | `OnPerkBought`, `OnPurchaseFailed` | — |
| **Game Flow** | `GameManager` | `OnStateChanged` | `Reactor.OnDestroyed` |
| **UI** | `HUD` | — | everything above |

## Example event chain — killing an enemy
```
PlayerShooter --click--> IShootable.Shoot()
Enemy.TakeDamage() --health 0--> Enemy.OnAnyKilled
    ├── SoulWallet   -> adds souls      -> OnSoulsChanged -> HUD redraws
    └── OrbDropper   -> maybe spawns orb
            Orb shot -> Orb.OnAnyCollected
                ├── SoulWallet    (soul orb)
                └── ReactorHealer (heal orb) -> Reactor.OnShieldChanged -> HUD
```
The enemy has no idea souls, orbs or a HUD exist. Any of those systems can be removed and the
rest of the game still runs — that's the point of the decoupling.

## Controls
- **Left click** — shoot enemies / orbs
- **1 / 2 / 3** — buy perks (Damage +1, Soul Gain +25%, Restore Shield)
- **Space** — Nova blast (clears enemies near the reactor, 12s cooldown)
- **R** — restart after game over
