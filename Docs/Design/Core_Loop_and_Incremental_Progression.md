# Core Loop & Incremental Progression

## Purpose

This document defines how the tower-defense gameplay connects to the
game's long-term incremental progression.

The game should not be an incremental game where progression exists only
to make numbers larger. Permanent progression should gradually increase
the player's power **and reveal additional layers of the game's rules**.

> **The player continuously becomes more powerful while the game
> continuously gives them new reasons to rethink how that power is
> used.**

------------------------------------------------------------------------

## 1. Core Level Loop

``` text
ENTER LEVEL
    ↓
Inspect level conditions
    ↓
Choose available towers / starting strategy
    ↓
WAVES BEGIN
    ↓
Defeat enemies → earn temporary currency
    ↓
Build and upgrade towers
    ↓
New enemies / traits appear
    ↓
Adapt the defense
    ↓
Environmental or unexpected variables appear
    ↓
Boss / final wave
    ↓
LEVEL COMPLETE
    ↓
Rewards + medals + progression
    ↓
Permanent upgrades / skill tree / unlocks
    ↓
NEXT LEVEL
```

The familiar TD loop is the foundation. The identity comes from changing
enemy traits, world conditions, unexpected variables, elemental
strengths and weaknesses, reactions, tower combinations, incremental
unlocks, medals, mastery and long-term progression.

------------------------------------------------------------------------

## 2. Two Progression Timescales

### In-Level Progression

Temporary resources earned during a level can be spent on:

-   Placing towers
-   Upgrading towers
-   Changing specializations
-   Temporary abilities
-   Emergency defenses
-   Selling/rebuilding
-   Activating level mechanics

This currency normally resets when the level ends.

### Permanent Progression

Permanent resources survive between levels and can unlock:

-   Towers and tower archetypes
-   Elements
-   Upgrade branches
-   Elemental states
-   Elemental reactions
-   Skill-tree sections
-   Passive improvements
-   Worlds
-   Challenge systems
-   Mastery systems
-   Advanced and late-game mechanics

Permanent progression should move substantially slower than individual
level progression.

------------------------------------------------------------------------

## 3. Unlock Rules, Not Only Numbers

Stat upgrades such as +5% damage, +5% attack speed or +10% range are
useful, but they should support progression rather than define it.

Major milestones should introduce mechanics.

Examples:

**Elemental Theory** --- unlocks elemental towers.

**Pressurized Water** --- Water can apply Soaked.

**Conductivity** --- Lightning gains additional effects against Soaked
enemies.

**Elemental Reactions** --- compatible elemental states can interact.

**Advanced Reactions** --- more complex combinations become possible.

**Environmental Mastery** --- world conditions interact more deeply with
elemental systems.

**Light / Darkness** --- new late-game attribute families become
available.

The player should periodically feel:

> **"The game just became bigger."**

------------------------------------------------------------------------

## 4. Progression Tempo

The intended rhythm is:

> **Introduce → Learn → Experiment → Master → Pressure → Reward**

A major unlock should arrive when the player is beginning to want
another tool, rather than the game continuously throwing new tutorials
and mechanics at them.

Example:

``` text
Physical towers
      ↓
Learn positioning / targeting / upgrades
      ↓
Master physical strategies
      ↓
New defenses pressure those strategies
      ↓
Unlock Fire
      ↓
Learn and master Fire
      ↓
New challenges expose Fire's limitations
      ↓
Unlock Water
      ↓
Learn Water
      ↓
Eventually discover Fire + Water interactions
```

------------------------------------------------------------------------

## 5. Progression Eras

Exact hours and level counts should be determined through playtesting.

### Era I --- Foundations

Tower placement, targeting, range, attack speed, damage, armor,
piercing, explosives, AoE, economy and basic upgrades.

### Era II --- First Element

The first element, such as Fire, introduces burning, damage over time,
elemental resistance, weaknesses and anti-regeneration.

### Era III --- Elemental Expansion

Additional elements such as Water, Frost and Lightning slowly become
available.

### Era IV --- Elemental Reactions

Previously unlocked elements begin interacting: Water + Fire → Steam,
Water + Lightning → Conductivity, Water + Frost → Freeze.

### Era V --- Mastery

The player combines elements, archetypes, upgrades, environmental
conditions, enemy weaknesses and multiple reactions.

### Era VI --- Advanced Attributes

Nature, Earth, Wind, Poison, Metal and other advanced attributes can
expand the existing reaction network.

### Era VII --- Hidden / Ancient Systems

Very late progression can reveal Light, Darkness, Eclipse interactions,
advanced multi-element reactions and hidden skill-tree branches.

------------------------------------------------------------------------

## 6. Skill Tree

The skill tree should contain:

### Statistical Nodes

Small improvements such as damage, duration, range or attack speed.

### Mechanical Nodes

Nodes that alter rules: Water applies Soaked, Fire disables
regeneration, Lightning chains through Soaked enemies, Frozen enemies
become Brittle, Steam creates lingering fog.

### Unlock Nodes

Major branches such as Fire, Water, Elemental Reactions, Advanced
Elemental Theory, Light and Darkness.

Mechanical and unlock nodes should feel substantially more important
than normal stat nodes.

------------------------------------------------------------------------

## 7. The Skill Tree Should Grow

The full tree does not need to be visible immediately.

``` text
EARLY

PHYSICAL
├── Ballistics
├── Piercing
└── Explosives
```

Later:

``` text
PHYSICAL ───────── FIRE
    │                │
    │                ├── Burning
    │                └── Combustion
    │
    └──────── ELEMENTAL THEORY
```

Much later:

``` text
                  [ ??? ]
                     │
        ┌────────────┴────────────┐
      LIGHT                    DARKNESS
```

Unlocking progression layers can physically expand the tree, preserving
mystery and creating the feeling:

> **"Wait... there's more?"**

------------------------------------------------------------------------

## 8. Medals & Challenges

Completing a level is only the first layer of mastery.

Possible medals:

-   Complete the level
-   Lose no lives
-   Use only certain tower families
-   Complete under a time limit
-   Increased enemy HP
-   No upgrades during battle
-   Limited tower slots
-   Nightmare/extreme completion

Medals can contribute to permanent progression. Milestones can reward
upgrades, new options, modifiers, cosmetics or major progression
rewards.

> **Mastering existing content contributes to future progression.**

------------------------------------------------------------------------

## 9. Tower Mastery

Tower families can gain mastery through meaningful use.

Mastery could unlock:

-   Alternate upgrade branches
-   Cosmetic evolution
-   Specialized mechanics
-   Small permanent bonuses
-   New interactions
-   Tower-specific challenges

It should reward experimentation without making unused towers
permanently inferior just because another tower has more playtime.

------------------------------------------------------------------------

## 10. World Progression

Worlds or regions can introduce:

-   Environmental rules
-   Enemy families
-   Traits
-   Bosses
-   Elemental pressures
-   Map mechanics
-   Progression branches

A world should introduce a **new strategic question**, not merely
stronger enemies.

------------------------------------------------------------------------

## 11. Prestige / Reset Philosophy

Prestige may exist, but it should not exist merely because incremental
games traditionally use resets.

Avoid making the core loop:

> Reset everything → gain +50% damage → repeat.

A major reset should ideally represent a progression transition.

Possible rewards include:

-   A new skill-tree branch
-   New element classes
-   Hidden mechanics
-   Changes to earlier towers
-   New world modifiers
-   Advanced reactions
-   Additional progression systems

The player loses some short-term progress but gains access to a deeper
version of the game.

------------------------------------------------------------------------

## 12. Prestige Should Change the Game

Conceptual example:

``` text
FIRST ASCENSION
      ↓
World progression partially resets
      ↓
Important discoveries remain
      ↓
ELEMENTAL THEORY becomes available
      ↓
Earlier levels gain new possibilities
```

Later ascensions could expand elemental reactions, enemy mutations and
environmental interactions.

A very late progression transition could reveal previously hidden
Light/Darkness systems.

These are concepts, not finalized prestige stages.

------------------------------------------------------------------------

## 13. Earlier Content Should Gain New Meaning

Unlocking new mechanics should make returning to old maps interesting.

A level originally solved using:

> Ballista + Cannon

could later be approached using:

> Water + Lightning conductivity

or:

> Fire + Water steam control

or eventually advanced Light/Dark interactions.

Medals, challenges, mastery and prestige can encourage this without
making earlier content disposable.

------------------------------------------------------------------------

## 14. Multiple Progression Goals

At any time the player can potentially:

-   Push the next level
-   Earn a missing medal
-   Unlock a skill-tree node
-   Increase tower mastery
-   Save toward an element
-   Complete a challenge
-   Defeat a boss
-   Discover a reaction
-   Prepare for a major progression milestone

This prevents progression from becoming one straight grind.

------------------------------------------------------------------------

## 15. Unexpected Variables Inside the Loop

Adaptive variables interact directly with the player's build.

Example:

``` text
WAVE 12

⚠ WEATHER SHIFT

Heavy Rain begins in 2 waves.

Fire effectiveness ↓
Enemies periodically become Soaked
Lightning conductivity ↑
Frost buildup ↑
```

Other events can include enemy mutations, route changes, temporary
buffs/nerfs, merchants, resource opportunities, environmental hazards
and boss modifications.

Unexpected events should create **decisions**, not unavoidable
punishment.

------------------------------------------------------------------------

## 16. Difficulty Growth

Difficulty should increasingly come from **complexity and interaction**,
not only inflated statistics.

``` text
Can your towers deal enough damage?
        ↓
Can they handle armor?
        ↓
Armor + regeneration?
        ↓
Armor + regeneration during heavy rain?
        ↓
Can the build survive when a mutation changes the optimal reaction?
```

Numbers still increase, but strategic complexity becomes increasingly
important.

------------------------------------------------------------------------

## 17. Incremental Power Fantasy

The player should become genuinely powerful.

Returning to an old challenge and completely destroying it is part of
the incremental reward. New worlds then introduce problems requiring the
player to use that increased power intelligently.

``` text
BECOME STRONGER
      ↓
DOMINATE OLD PROBLEM
      ↓
DISCOVER NEW PROBLEM
      ↓
ADAPT
      ↓
BECOME STRONGER
      ↓
MASTER NEW SYSTEM
      ↓
DISCOVER DEEPER SYSTEM
```

------------------------------------------------------------------------

## 18. Core Progression Principles

> **Progression should reveal mechanics as well as increase power.**

> **Major systems need time to breathe before another major system
> appears.**

> **Old content should gain new strategic meaning as the player unlocks
> new tools.**

> **Prestige should deepen the game, not exist only to multiply
> numbers.**

> **Difficulty should increasingly come from interactions and strategic
> pressure rather than pure stat inflation.**

> **The player should usually have multiple meaningful progression goals
> available.**

> **The player should occasionally feel extremely powerful. Incremental
> progression needs payoff.**

------------------------------------------------------------------------

## Long-Term Vision

The game begins as something understandable:

> **Place towers. Kill enemies. Upgrade. Survive.**

Over time:

> **Build towers → specialize them → manipulate elements → apply states
> → trigger reactions → respond to environments → exploit weaknesses →
> counter mutations → master challenges → unlock deeper progression
> systems.**

Eventually:

> **The player realizes the simple tower-defense game they started with
> was only the first layer of a much larger system.**

That gradual discovery is a core part of the incremental experience.
