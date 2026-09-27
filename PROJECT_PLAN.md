# Project TD: project plan

High-level roadmap. Each phase should end with something playable and verified before the next one starts. Refine phases into tasks only when they begin.

**Current phase: Phase 2, first interactive level (complete).** Next: to be decided by the game director (enemy variety and more waves are still open, see below).

## Phase 0: Project setup ✅
- Unity 6.6 (`6000.6.3f1`) Universal 2D project at the repository root
- `Assets/_Game/` folder structure, `Docs/Design/`
- README, CLAUDE.md, project plan, Unity `.gitignore`

## Phase 1: Core TD prototype ✅
Development scene: `Assets/_Game/Scenes/Development/Prototype.unity` (1 map, 1 tower type with 2 pre-placed towers, 1 enemy type, 3 waves).
- Map / path ✅ waypoint path
- Enemy movement ✅
- Tower placement ⏸ deferred: towers are pre-placed in the scene (no shop/placement yet)
- Targeting ✅ "First" (furthest along the path, within range)
- Attacking ✅ homing projectiles
- Damage ✅
- Waves ✅ sequential waves, next wave after the current one is cleared
- Currency / lives ✅ kill rewards, lives lost at the end point
- Win / loss and a minimal HUD (lives, currency, wave, VICTORY/DEFEAT) ✅

Verified by EditMode tests (logic) and PlayMode tests that play the scene to victory and to defeat. Decisions: [Docs/Architecture.md](Docs/Architecture.md).

## Phase 2: First interactive level ✅
Goal: turn the combat prototype into a level the player actively plays. Same scene (`Prototype.unity`), same tower and enemy types, combat numbers unchanged.
- Larger battlefield (28 x 13 units, about 2.6x the Phase 1 view) that the camera fits at any aspect ratio ✅
- Organic medieval cart road ✅ smooth spline route shaped by the terrain (valley, stream crossing at a bridge, rocky ridge); the visible road is generated from the same curve enemies walk
- Smooth enemy movement ✅ along the curve; progress, targeting and end-of-path unchanged
- No pre-placed towers; free tower placement ✅ ghost with range preview, valid/invalid feedback and reason, cancel with right-click/Esc
- Placement validation ✅ road clearance, battlefield bounds, other towers, blocked terrain (water, rocks, trees)
- Tower purchase ✅ 25 gold, starting gold 50
- Tower selection ✅ shows its range circle, level, damage, attack rate and range, with Upgrade and Sell
- Upgrades ✅ three steps (Heavy Bolts, Quick Reload, Long Sights) at 20 / 35 / 50 gold
- Selling ✅ 70% of everything invested
- Manual wave start ✅ nothing spawns until the player starts a wave; no automatic next wave
- 5 waves (Phase 1's three plus two harder ones) ✅
- Victory/defeat freeze the game (no spawns, enemies stop, towers stop, no economy changes) and Restart reloads the level ✅

Verified by EditMode tests (curve, placement rules, tower progression, spending, camera fit, level geometry) and PlayMode tests on the real scene (placement and economy, wave flow, a scripted full win paid for with the real economy, defeat and frozen end state, restart). Decisions: [Docs/Architecture.md](Docs/Architecture.md).

Still open from the original Phase 2 outline (not started): ~10 waves, basic enemy variety (e.g. Fast, Armored, Regenerating), more starting tower types (e.g. Ballista, Cannon, Mage).

## Phase 3: Elemental prototype
- Fire
- Water
- Statuses (e.g. Soaked)
- First elemental interaction (e.g. Fire + Soaked → Steam)

## Phase 4: Adaptive level variables
- Weather (e.g. Heavy Rain)
- Enemy traits
- Changing conditions (e.g. Armored Wave, Swarm)

## Phase 5: Vertical slice polish
- Readability
- Effects
- Basic sound
- UX improvements
- Balance pass
- Testing

## Deliberately out of scope for now
Full skill tree, prestige, Light/Darkness, large numbers of elements/towers/enemies, procedural or AI map generation, multiplayer, backend/accounts/cloud saves, modding, generic editor frameworks, complex save architecture, monetization.

The core loop has to be proven fun first.
