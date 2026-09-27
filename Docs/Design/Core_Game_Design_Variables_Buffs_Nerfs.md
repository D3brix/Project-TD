# Core Game Design --- Variables, Buffs & Nerfs

## Core Identity

> **The player builds an increasingly powerful defense, while the game
> continuously changes the conditions under which that defense
> operates.**

The central idea is **adaptation**. Buffs, nerfs, traits, and unexpected
variables should not exist only to make numbers larger or smaller. They
should create new problems that the player can solve in multiple ways.

------------------------------------------------------------------------

## 1. Three Types of Variables

### Enemy Traits

  Trait          Example Effect
  -------------- ------------------------------------------------------
  Armored        Takes 40% less physical damage
  Regeneration   Restores 2% HP per second
  Brittle        Takes 35% more explosive damage
  Splitting      Spawns two smaller enemies on death
  Adaptive       Gains resistance to repeatedly received damage types

### World Conditions

  Condition       Example Effect
  --------------- -----------------------------------------------------
  Heavy Rain      Fire is weaker, electricity chains farther
  High Winds      Projectiles become less accurate but may gain range
  Frozen Ground   Enemy movement behavior changes
  Darkness        Towers have reduced vision/range

### Player Modifiers

  Modifier      Example Effect
  ------------- --------------------------------------
  Overcharged   Towers gain 25% attack speed
  Shortage      Towers cost 20% more
  Minimalist    Maximum of five towers may be placed

------------------------------------------------------------------------

## 2. Buffs Should Usually Have Multiple Counters

A strong enemy trait should create a strategic problem rather than
prescribe exactly one solution.

### Example: Armored

**Effect:** Physical damage reduced by 60%.

Possible responses: - Magic damage bypasses armor. - Armor penetration
reduces its effectiveness. - Acid/corrosion gradually destroys armor. -
Critical attacks partially penetrate armor. - Stuns and slows give
weaker towers more time. - A sufficiently powerful physical build can
brute-force the resistance.

The goal is to avoid a system where every problem has exactly one
mandatory counter.

------------------------------------------------------------------------

## 3. Enemies Should Have Strengths and Weaknesses

### Iron Golem

-   **HP:** Very high
-   **Speed:** Low
-   **Armor:** Very high

**Resistant to** - Physical: +60% resistance - Frost: +30% resistance

**Weak to** - Electricity: +35% damage received - Corrosion: +50% damage
received

**Trait --- Heavy** - Cannot be knocked back.

A relatively small collection of base enemies can produce many
variants: - Goblin - Armored Goblin - Regenerating Goblin - Frost
Goblin - Armored Regenerating Goblin - Elite Frost Goblin - Corrupted
Goblin

------------------------------------------------------------------------

## 4. Unexpected Variables

Unexpected events should make the player think:

> **"How do I deal with this?"**

Rather than:

> **"The game randomly destroyed my build and there was nothing I could
> do."**

Major negative events should usually be **telegraphed before taking
effect**.

### Example: Weather Shift

**Heavy storm approaching --- begins in 2 waves**

-   Fire damage: -25%
-   Electricity chain range: +40%
-   Projectile speed: -15%

### Other Event Ideas

**Migration** --- next three waves contain flying enemies.

**Blood Moon** --- enemies move 25% faster but drop 40% more currency.

**Earthquake** --- one route closes while another opens.

**Merchant Caravan** --- temporary upgrade discounts.

**Mana Surge** --- elemental towers become significantly stronger for
two waves.

**Mutation** --- one enemy species gains a new trait.

Events can be negative, positive, or ideally a mixture of both.

------------------------------------------------------------------------

## 5. Buff + Nerf Combinations

### Berserk

-   +40% movement speed
-   +25% attack resistance
-   -30% maximum HP

### Frozen

-   -30% movement speed
-   +40% physical resistance
-   -50% fire resistance

### Overgrown

-   +100% regeneration
-   -25% movement speed
-   Fire damage disables regeneration

Tradeoffs change player decisions rather than merely increasing
difficulty.

------------------------------------------------------------------------

## 6. Combining Systems

### Example --- Level 27: The Dead Marsh

**World Condition: Heavy Rain** - Fire damage: -30% - Lightning
effectiveness: +50%

**Enemy Traits** - Regenerating: enemies recover 2% HP per second. -
Swarming: +60% enemy count, -35% individual enemy HP.

**Special Rule** - Every fifth wave triggers a mutation.

This encourages strategies involving AoE, electricity, anti-healing,
crowd control, or other combinations, while still leaving room for
alternative solutions.

------------------------------------------------------------------------

## Fundamental Design Rule

> **Modifiers should change decisions, not merely numbers.**

Less interesting: - Enemies have +10% HP. - Towers attack 15% slower.

More interesting: - Enemies resurrect unless killed while burning. -
Every tower gains damage the longer it attacks the same target. - Fire
disables enemy regeneration. - Killing an enemy causes nearby enemies to
temporarily become stronger. - Enemies adapt resistance toward
repeatedly received damage types.

The purpose of a modifier is to make the player reconsider **how they
play**, not simply require more damage.

------------------------------------------------------------------------

## Core Gameplay Tension

> **The player continuously becomes more powerful while the world
> continuously invents new reasons for them to rethink how they use that
> power.**

This tension is the foundation of the game's identity.

The player's progression should feel meaningful and powerful, but no
single strategy should remain universally optimal forever.
