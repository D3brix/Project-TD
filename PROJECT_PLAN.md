# Project TD: project plan

High-level roadmap. Each phase should end with something playable and verified before the next one starts. Refine phases into tasks only when they begin.

**Current phase: Phase 1, core TD prototype (complete; tower placement deferred).** Next: Phase 2.

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

## Phase 2: First complete level
- ~10 waves
- Basic enemy variety (e.g. Basic, Fast, Armored, Regenerating)
- Tower placement / purchasing (spending currency)
- Tower upgrades (starting towers e.g. Ballista, Cannon, Mage)
- Win / loss flow (restart, level end)
- Basic HUD

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
