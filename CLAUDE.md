# Project TD: guidance for Claude Code

Project TD is a 2D incremental tower-defense game in Unity 6.6 (`6000.6.3f1`), using URP with the 2D Renderer. This repository root is the Unity project root. See `README.md` for the layout and `PROJECT_PLAN.md` for the current phase.

**The immediate goal is a playable game, not a perfect framework.** The first milestone is: an enemy walks down a path and a tower shoots it.

## Working rules

1. Inspect the existing project state (code, scenes, `PROJECT_PLAN.md`, git status) before modifying anything.
2. Do not invent completed systems. Describe only what actually exists.
3. Do not rewrite working systems unnecessarily.
4. Prefer simple, understandable gameplay code.
5. Use data-driven design (e.g. ScriptableObject definitions) where it provides clear value, not everywhere by default.
6. Do not build future systems until current gameplay requires them.
7. Prefer composition over premature abstraction.
8. Keep gameplay logic reasonably separated from presentation where useful.
9. Never modify files outside this repository without explicit permission.
10. Never destructively replace or delete assets without understanding why they exist.
11. Respect Unity `.meta` files: every asset and folder under `Assets/` has one, and they must be committed together.
12. Preserve Unity GUIDs when moving or refactoring assets. Move them in the Unity Editor, or move each `.meta` file with its asset.
13. Never manually modify generated Unity directories: `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`.
14. Do not create architecture simply because it might be useful later. No speculative base-class hierarchies, factories, service locators, DI frameworks, global event buses or interfaces without a concrete need.
15. Record important architectural decisions (a short note in `Docs/` is enough).
16. Update `PROJECT_PLAN.md` at meaningful milestones.
17. Verify actual behavior (compile, run tests, play in the editor or batch mode) before claiming something works. Say clearly what was and wasn't verified.
18. Do not push to Git remotes unless explicitly instructed.
19. Keep commits focused if commits are requested.
20. The immediate goal is a playable game, not a perfect framework. Refactor when real gameplay gives evidence that it's needed.

## Conventions

- Game content lives under `Assets/_Game/`. The template's `Assets/Settings/` holds the URP pipeline assets that the project settings reference. Leave them in place unless you move them in the editor.
- Tests go in `Assets/_Game/Tests/EditMode` and `Assets/_Game/Tests/PlayMode` (Unity Test Framework).
- Design documents live in `Docs/Design/`. Don't redesign the game or fill in missing design decisions. Ask instead.
- When a folder that only held a `.gitkeep` gets real content, the `.gitkeep` can be removed.

## Useful commands

Batch-mode compile/import check (the editor must not already have the project open):

```
"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe" -batchmode -quit -projectPath . -logFile -
```
