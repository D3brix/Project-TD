# Project TD

A 2D incremental tower-defense game made in Unity.

It starts as familiar tower defense and slowly shows how deep its rules go: elemental attributes, statuses and reactions, tower combinations, enemy traits, changing level variables, and long-term progression (skill tree, medals, tower mastery).

> Progression does not only make the player stronger. It progressively reveals the depth of the game's rules.

## Status

**Phase 1: core prototype.** Open `Assets/_Game/Scenes/Development/Prototype.unity` and press Play: enemies walk a path in 3 waves, two pre-placed towers shoot them, kills earn currency, escaped enemies cost lives, and the game ends in VICTORY or DEFEAT. Placeholder art only. See [PROJECT_PLAN.md](PROJECT_PLAN.md) and [Docs/Architecture.md](Docs/Architecture.md).

## Unity

- **Editor:** Unity 6.6 (`6000.6.3f1`). The authoritative version is in `ProjectSettings/ProjectVersion.txt`.
- **Template:** Universal 2D (URP with the 2D Renderer)
- **Art direction:** stylized, illustrated 2D with clean, readable shapes. Not pixel art, not realistic.

### Opening the project

1. Unity Hub → **Projects** → **Add** → **Add project from disk**.
2. Select this repository's root folder (the folder that contains `Assets/`).
3. Open it with editor `6000.6.3f1`. The first open rebuilds `Library/`, which takes a few minutes.

## Structure

```
Assets/
  _Game/            All Project TD game content (code, art, data, prefabs, scenes, tests)
    Art/ Audio/ Data/ Materials/ Prefabs/ Scenes/ Scripts/ Settings/ Tests/
  Settings/         URP pipeline assets from the Unity 2D template (referenced by project settings)
Packages/           Unity package manifest
ProjectSettings/    Unity project settings
Docs/Design/        Game design documents
```

- **Game code:** `Assets/_Game/Scripts/`
- **Static game data** (ScriptableObjects, when they are needed): `Assets/_Game/Data/`
- **Design documents:** `Docs/Design/`. The existing design documents (core variables/buffs/nerfs, tower and elemental progression, core loop and incremental progression) will be collected here later.

Empty folders contain a `.gitkeep` file so Git tracks them along with their `.meta` files.

## Tests

In the editor: **Window → General → Test Runner** (EditMode and PlayMode tabs). From the command line, with the editor closed:

```
"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults editmode.xml
"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults playmode.xml
```

## Other docs

- [PROJECT_PLAN.md](PROJECT_PLAN.md): high-level roadmap
- [CLAUDE.md](CLAUDE.md): working rules for AI-assisted development
