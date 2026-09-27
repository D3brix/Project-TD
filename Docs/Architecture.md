# Architecture notes

Short record of decisions that shape the code. Add to it when a decision is made, not in advance.

## Phase 1: core prototype (2026-09-28)

**Pure logic, thin components.** Rules that must hold exactly (HP, "resolves once", wave completion, lives/currency, victory/defeat) live in plain C# classes with no Unity lifecycle: `EnemyState`, `PathFollower`, `WaveProgress`, `GameSession`. MonoBehaviours (`Enemy`, `Tower`, `Projectile`, `WaveSpawner`, `GameController`, `Hud`) wire those into the scene. The pure classes are covered by EditMode tests. The scene as a whole is covered by PlayMode tests that play it to victory and to defeat.

**An enemy resolves exactly once.** `EnemyState` has a single outcome: `Alive`, then either `Killed` or `ReachedEnd`. Every reward, life loss and wave count flows from that one transition, so none of them can happen twice.

**Movement is waypoint-based.** A `WaypointPath` is a list of child transforms. `PathFollower` moves a set distance along the polyline, carrying leftover distance around corners. "Distance travelled" is also the measure targeting uses for "furthest along the path".

**Targeting is a static function plus an enum.** `TargetSelector.Select(enemies, origin, range, mode)` re-evaluates every frame. A new mode is a new `TargetingMode` value and a case in `IsBetter`. There is no strategy-class hierarchy until there are several modes that need one.

**Towers get enemies from the `WaveSpawner`.** The spawner owns the list of living enemies, and towers hold a reference to it. There is no global registry or event bus. If more things need the enemy list later, revisit this.

**Projectiles home in and never retarget.** If the target dies or escapes first, the projectile flies to the target's last known position and disappears without dealing damage. This avoids overkill damage landing on other enemies.

**Waves run one at a time.** The next wave starts only after the current one is cleared, plus a short pause. Wave data is a serializable `WaveDefinition[]` on the `WaveSpawner` (count, interval, health multiplier). There is no ScriptableObject yet because there is only one level.

**No ScriptableObject definitions yet.** Enemy and tower stats are serialized fields on their prefabs. Introduce `EnemyDefinition`/`TowerDefinition` assets when there are several types to compare and balance (Phase 2).

**Namespaces** follow the script folders: `ProjectTD.Core`, `.Enemies`, `.Towers`, `.Combat`, `.Waves`, `.Levels`, `.UI`. All runtime code is in the `ProjectTD` assembly (`Assets/_Game/Scripts/ProjectTD.asmdef`). Tests are in `ProjectTD.Tests.EditMode` and `ProjectTD.Tests.PlayMode`.

**HUD** uses uGUI legacy `Text` on a Screen Space - Camera canvas. This avoids importing TextMeshPro essentials for a throwaway HUD.
