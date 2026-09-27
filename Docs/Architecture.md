# Architecture notes

Short record of decisions that shape the code. Add to it when a decision is made, not in advance.

## Phase 1: core prototype (2026-09-28)

**Pure logic, thin components.** Rules that must hold exactly (HP, "resolves once", wave completion, lives/currency, victory/defeat) live in plain C# classes with no Unity lifecycle: `EnemyState`, `PathFollower`, `WaveProgress`, `GameSession`. MonoBehaviours (`Enemy`, `Tower`, `Projectile`, `WaveSpawner`, `GameController`, `Hud`) wire those into the scene. The pure classes are covered by EditMode tests. The scene as a whole is covered by PlayMode tests that play it to victory and to defeat.

**An enemy resolves exactly once.** `EnemyState` has a single outcome: `Alive`, then either `Killed` or `ReachedEnd`. Every reward, life loss and wave count flows from that one transition, so none of them can happen twice.

**Movement is waypoint-based.** *(Superseded in Phase 2: the waypoints now define a smooth curve.)* A `WaypointPath` is a list of child transforms. `PathFollower` moves a set distance along the polyline, carrying leftover distance around corners. "Distance travelled" is also the measure targeting uses for "furthest along the path".

**Targeting is a static function plus an enum.** `TargetSelector.Select(enemies, origin, range, mode)` re-evaluates every frame. A new mode is a new `TargetingMode` value and a case in `IsBetter`. There is no strategy-class hierarchy until there are several modes that need one.

**Towers get enemies from the `WaveSpawner`.** The spawner owns the list of living enemies, and towers hold a reference to it. There is no global registry or event bus. If more things need the enemy list later, revisit this.

**Projectiles home in and never retarget.** If the target dies or escapes first, the projectile flies to the target's last known position and disappears without dealing damage. This avoids overkill damage landing on other enemies.

**Waves run one at a time.** *(Phase 2: the player now starts each wave.)* The next wave starts only after the current one is cleared, plus a short pause. Wave data is a serializable `WaveDefinition[]` on the `WaveSpawner` (count, interval, health multiplier). There is no ScriptableObject yet because there is only one level.

**No ScriptableObject definitions yet.** Enemy and tower stats are serialized fields on their prefabs. Introduce `EnemyDefinition`/`TowerDefinition` assets when there are several types to compare and balance (Phase 2).

**Namespaces** follow the script folders: `ProjectTD.Core`, `.Enemies`, `.Towers`, `.Combat`, `.Waves`, `.Levels`, `.Placement` (Phase 2), `.UI`. All runtime code is in the `ProjectTD` assembly (`Assets/_Game/Scripts/ProjectTD.asmdef`). Tests are in `ProjectTD.Tests.EditMode` and `ProjectTD.Tests.PlayMode`.

**HUD** uses uGUI legacy `Text` on a Screen Space - Camera canvas. This avoids importing TextMeshPro essentials for a throwaway HUD.

## Phase 2: first interactive level (2026-09-28)

**Roads should look discovered, not drawn.** The route is composed around terrain that explains each bend: a valley between two forests, a stream that can only be crossed at one narrow point (a bridge; lower down it runs through a rocky gully into a pond), and a rocky ridge that squeezes the road on the far bank. Curvature varies (long drifts, one broad bend, a horseshoe at the bridge, a tighter bend round the ridge's tip) rather than repeating. The route was tuned for play as well as looks: the inside of the bridge horseshoe is a small spot that sees about 1.8x as much road as a spot beside the straight, so where a tower goes matters.

**One curve for movement, visuals and placement.** `LevelPath`'s children are control points. `PathCurve` joins them with a centripetal Catmull-Rom spline (no cusps, passes through every point) and resamples it into a dense, evenly spaced polyline. Enemies walk that polyline with the unchanged `PathFollower`, so progress is still exact distance travelled, targeting and "reaches the end once" work as before, and movement stays deterministic. `RoadRenderer` builds the visible road mesh from the same points, and placement measures road clearance against them, so enemies never cut across what the player sees.

**Road art is a generated mesh.** Irregular edges, a soft verge, a worn centre and fading ruts come from deterministic noise along the curve. It uses a tiny vertex-colour shader (`ProjectTD/Vertex Color Unlit`) because the URP sprite shaders take their tint from per-sprite data that a `MeshRenderer` doesn't supply, which leaves the mesh invisible.

**Free placement with circle footprints.** `PlacementRules` (pure) decides whether a footprint is inside the battlefield, clear of the road (`LevelPath.BuildClearance` from the centre line), clear of blocked terrain and clear of other towers. Blocked terrain is any `PlacementBlocker` circle under the `Battlefield`. There are no terrain categories or grids.

**`TowerBuilder` is the only way to spend on towers.** Buying, upgrading and selling all go through it, and it spends via `GameSession.TrySpend`, which refuses unaffordable amounts and anything after the game ends. So currency changes exactly once per action that actually happens. `TowerInteraction` only turns pointer input (Input System `Mouse`/`Keyboard`) into requests and shows the ghost. `Hud` only displays state and forwards button clicks.

**Tower levels are a serialized table on the tower prefab.** `TowerLevel[]`: level 0 is the tower as built (its cost is the price), and later levels are upgrades. `TowerProgression` (pure) tracks level and total investment. Sell refund is a fixed 70% of everything invested, rounded down. There's still no ScriptableObject: with one tower type, the prefab is the definition. The HUD reads name, price and stats from the tower, and `TowerBuilder.AvailableTowers` lists what can be built, so a second tower type is another prefab in that list (plus a button).

**The player starts every wave.** `WaveSpawner.StartNextWave()` runs one wave and refuses while one is running. Wave completion is detected when the wave's last enemy is removed, not by a polling coroutine, so starting the next wave the instant one clears can't double-report.

**The end of the game freezes everything.** On victory or defeat `GameController` raises `GameEnded`, then halts the spawner (no spawns, enemies stop moving and ignore damage), and `TowerBuilder` disables all towers and locks building, upgrading and selling. `GameSession` already ignores currency and life changes after the end.

**Restart reloads the scene.** That is the simplest way to guarantee a clean initial state (currency, lives, waves, towers, enemies, projectiles, HUD) without per-system reset code.

**The camera fits the battlefield.** `BattlefieldCamera` sizes the orthographic camera so the whole battlefield sits between the HUD's top and bottom bars at any aspect ratio (Phase 1's spawn marker was partly off-screen).

**Economy (temporary numbers).** Combat numbers are unchanged from Phase 1 (enemy speed and HP, tower damage, fire rate, range, projectile speed, kill reward, spawn intervals of waves 1 to 3). New: tower price 25, starting gold 50 (two towers, or one plus its first upgrade), upgrades 20 / 35 / 50, and waves 4 and 5 (14 enemies at 2.5x HP, 16 at 3x). The price was lowered from a first try of 30 because the bigger, more open map gives each tower less road to cover than Phase 1's compact zigzag. A scripted player using the real economy wins all 5 waves with good placement, and loses on wave 4 with the same budget spent on weak spots.
