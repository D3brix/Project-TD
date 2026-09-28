using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Towers
{
    /// <summary>
    /// One row of a tower's stats. The base level is the tower as built (its cost is the build price);
    /// every branch tier is an upgrade bought for its cost. Stats are absolute, not deltas.
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
        public float DamagePerSecond => damage / attackInterval;
    }

    /// <summary>
    /// A development path: how a tower attacks (e.g. many light shots, or few heavy ones), not what element it uses.
    /// Its tiers are bought in order, and a tower that takes one branch can never take another.
    /// </summary>
    [Serializable]
    public class TowerBranch
    {
        public string name = "Branch";
        [Tooltip("Short identity and trade-off, shown before the player commits.")]
        public string strength = "";
        public string weakness = "";
        public Color color = Color.white;
        [Tooltip("Symbol for the branch in the HUD, so branches are told apart by shape as well as colour.")]
        public Sprite icon;
        [Tooltip("Barrel size on the map once this branch is chosen, so specialised towers are recognisable.")]
        public Vector2 barrelScale = new Vector2(0.5f, 0.16f);
        public TowerLevel[] tiers = { new TowerLevel() };
    }

    /// <summary>
    /// A tower's development: it starts at its base level, the player commits it to one branch with the first
    /// upgrade, and later upgrades follow that branch only. Also tracks everything invested.
    /// Pure logic, no Unity lifecycle. Paying for upgrades is the caller's job (see <see cref="TowerBuilder"/>).
    /// </summary>
    public class TowerProgression
    {
        public const int NoBranch = -1;

        readonly TowerLevel baseLevel;
        readonly TowerBranch[] branches;

        /// <summary>The committed branch, or <see cref="NoBranch"/> while the tower is still at its base level.</summary>
        public int BranchIndex { get; private set; } = NoBranch;

        /// <summary>0 at the base level, then 1, 2, ... for each tier bought in the committed branch.</summary>
        public int Tier { get; private set; }

        public IReadOnlyList<TowerBranch> Branches => branches;
        public TowerLevel Base => baseLevel;
        public bool HasBranch => BranchIndex != NoBranch;
        public TowerBranch Branch => HasBranch ? branches[BranchIndex] : null;
        public TowerLevel Current => HasBranch ? branches[BranchIndex].tiers[Tier - 1] : baseLevel;
        public bool IsMaxLevel => HasBranch ? Tier >= branches[BranchIndex].tiers.Length : branches.Length == 0;

        /// <summary>Build cost plus every upgrade bought so far.</summary>
        public int TotalInvested { get; private set; }

        public TowerProgression(TowerLevel baseLevel, IReadOnlyList<TowerBranch> branches)
        {
            this.baseLevel = baseLevel ?? throw new ArgumentNullException(nameof(baseLevel));
            this.branches = new TowerBranch[branches?.Count ?? 0];
            for (int i = 0; i < this.branches.Length; i++)
            {
                if (branches[i].tiers == null || branches[i].tiers.Length == 0)
                    throw new ArgumentException($"Branch {branches[i].name} has no tiers.", nameof(branches));
                this.branches[i] = branches[i];
            }

            TotalInvested = baseLevel.cost;
        }

        /// <summary>
        /// The level an upgrade along <paramref name="branchIndex"/> would buy next, or null if that branch is not open to
        /// this tower (another branch was chosen, or this one is complete).
        /// </summary>
        public TowerLevel NextIn(int branchIndex)
        {
            if (branchIndex < 0 || branchIndex >= branches.Length)
                return null;
            if (!HasBranch)
                return branches[branchIndex].tiers[0];
            if (branchIndex != BranchIndex || IsMaxLevel)
                return null;
            return branches[branchIndex].tiers[Tier];
        }

        /// <summary>Moves one tier along <paramref name="branchIndex"/>, committing to it if needed. Returns false (and changes nothing) if not allowed.</summary>
        public bool Upgrade(int branchIndex)
        {
            TowerLevel next = NextIn(branchIndex);
            if (next == null)
                return false;

            BranchIndex = branchIndex;
            Tier++;
            TotalInvested += next.cost;
            return true;
        }

        /// <summary>Refund for selling: a fixed percentage of everything invested, rounded down.</summary>
        public int SellValue(int refundPercent) => TotalInvested * Mathf.Clamp(refundPercent, 0, 100) / 100;
    }
}
