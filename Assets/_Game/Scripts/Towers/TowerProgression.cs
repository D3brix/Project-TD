using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Towers
{
    /// <summary>
    /// One row of a tower's stats. Level 0 is the tower as built (its cost is the build price);
    /// every later level is an upgrade bought for its cost.
    /// </summary>
    [Serializable]
    public class TowerLevel
    {
        public string label = "Basic";
        [Min(0)] public int cost = 30;
        [Min(0f)] public float damage = 3f;
        [Min(0.05f)] public float attackInterval = 0.6f;
        [Min(0.1f)] public float range = 3f;

        public float AttacksPerSecond => 1f / attackInterval;
    }

    /// <summary>
    /// A tower's current level and how much has been invested in it. Pure logic, no Unity lifecycle.
    /// Paying for levels is the caller's job (see <see cref="TowerBuilder"/>).
    /// </summary>
    public class TowerProgression
    {
        readonly TowerLevel[] levels;

        public int LevelIndex { get; private set; }
        public int LevelCount => levels.Length;
        public TowerLevel Current => levels[LevelIndex];
        public bool IsMaxLevel => LevelIndex >= levels.Length - 1;
        public TowerLevel Next => IsMaxLevel ? null : levels[LevelIndex + 1];

        /// <summary>Build cost plus every upgrade bought so far.</summary>
        public int TotalInvested { get; private set; }

        public TowerProgression(IReadOnlyList<TowerLevel> levels)
        {
            if (levels == null || levels.Count == 0)
                throw new ArgumentException("A tower needs at least one level.", nameof(levels));

            this.levels = new TowerLevel[levels.Count];
            for (int i = 0; i < levels.Count; i++)
                this.levels[i] = levels[i];

            TotalInvested = this.levels[0].cost;
        }

        /// <summary>Moves to the next level. Returns false (and changes nothing) at max level.</summary>
        public bool Upgrade()
        {
            if (IsMaxLevel)
                return false;

            LevelIndex++;
            TotalInvested += Current.cost;
            return true;
        }

        /// <summary>Refund for selling: a fixed percentage of everything invested, rounded down.</summary>
        public int SellValue(int refundPercent) => TotalInvested * Mathf.Clamp(refundPercent, 0, 100) / 100;
    }
}
