using System;

namespace ProjectTD.Enemies
{
    public enum EnemyOutcome
    {
        Alive,
        Killed,
        ReachedEnd
    }

    /// <summary>
    /// An enemy's health and fate, without any Unity dependencies.
    /// An enemy resolves exactly once: it is either killed or it reaches the end, never both, never twice.
    /// </summary>
    public class EnemyState
    {
        public float MaxHealth { get; }
        public float Health { get; private set; }
        public EnemyOutcome Outcome { get; private set; } = EnemyOutcome.Alive;
        public bool IsAlive => Outcome == EnemyOutcome.Alive;

        public EnemyState(float maxHealth)
        {
            if (maxHealth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be positive.");

            MaxHealth = maxHealth;
            Health = maxHealth;
        }

        /// <summary>Applies damage. Returns true only for the hit that kills the enemy.</summary>
        public bool ApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return false;

            Health = Math.Max(0f, Health - amount);
            if (Health > 0f)
                return false;

            Outcome = EnemyOutcome.Killed;
            return true;
        }

        /// <summary>Marks the enemy as having reached the end. Returns true only the first time, and only if it was alive.</summary>
        public bool ReachEnd()
        {
            if (!IsAlive)
                return false;

            Outcome = EnemyOutcome.ReachedEnd;
            return true;
        }
    }
}
