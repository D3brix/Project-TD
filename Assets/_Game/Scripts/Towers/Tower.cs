using ProjectTD.Combat;
using ProjectTD.Enemies;
using ProjectTD.Waves;
using UnityEngine;

namespace ProjectTD.Towers
{
    /// <summary>
    /// Re-picks a target every frame among the spawner's active enemies and fires a projectile
    /// at it whenever the attack cooldown allows. Its stats come from its current <see cref="TowerLevel"/>:
    /// the base level, or a tier of the branch it was developed along (see <see cref="TowerProgression"/>).
    /// A tower does nothing until <see cref="Initialize"/> gives it an enemy source (placed towers only).
    /// </summary>
    public class Tower : MonoBehaviour
    {
        [SerializeField] string displayName = "Tower";
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] Transform turret;
        [SerializeField] Transform firePoint;
        [SerializeField] Transform rangeIndicator;
        [SerializeField] SpriteRenderer rangeFill;
        [SerializeField] SpriteRenderer rangeRing;
        [SerializeField] SpriteRenderer baseSprite;
        [SerializeField] Transform barrel;

        [Tooltip("Radius of the ground this tower occupies, for placement checks.")]
        [SerializeField, Min(0.1f)] float footprintRadius = 0.4f;
        [Tooltip("The tower as built; its cost is the build price.")]
        [SerializeField] TowerLevel baseLevel = new TowerLevel();
        [Tooltip("Development paths. The first upgrade commits the tower to one of them.")]
        [SerializeField] TowerBranch[] branches = new TowerBranch[0];
        [SerializeField] TargetingMode targetingMode = TargetingMode.First;

        WaveSpawner enemySource;
        TowerProgression progression;
        float cooldown;

        public string DisplayName => displayName;
        public float FootprintRadius => footprintRadius;
        public int BuildCost => baseLevel.cost;
        public TowerProgression Progression => progression ??= new TowerProgression(baseLevel, branches);
        public TowerLevel Stats => Progression.Current;
        public float Range => Stats.range;
        public float Damage => Stats.damage;
        public float AttackInterval => Stats.attackInterval;

        public Enemy CurrentTarget { get; private set; }
        public int ShotsFired { get; private set; }

        public void Initialize(WaveSpawner enemySource)
        {
            this.enemySource = enemySource;
            UpdateRangeIndicator();
        }

        /// <summary>Applies the next tier of <paramref name="branchIndex"/>. Paying for it is the caller's job.</summary>
        internal bool ApplyUpgrade(int branchIndex)
        {
            if (!Progression.Upgrade(branchIndex))
                return false;

            UpdateRangeIndicator();
            UpdateBranchLook();
            return true;
        }

        public void ShowRange(bool visible, Color color)
        {
            if (rangeIndicator == null)
                return;

            rangeIndicator.gameObject.SetActive(visible);
            if (rangeFill != null)
                rangeFill.color = new Color(color.r, color.g, color.b, 0.1f);
            if (rangeRing != null)
                rangeRing.color = new Color(color.r, color.g, color.b, 0.85f);
        }

        void Update()
        {
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

            if (enemySource == null)
                return;

            CurrentTarget = TargetSelector.Select(enemySource.ActiveEnemies, transform.position, Range, targetingMode);
            if (CurrentTarget == null)
                return;

            if (turret != null)
            {
                Vector2 toTarget = CurrentTarget.Position - (Vector2)turret.position;
                turret.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg);
            }

            if (cooldown <= 0f)
                Fire(CurrentTarget);
        }

        void OnDisable()
        {
            CurrentTarget = null;
        }

        void Fire(Enemy target)
        {
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            Projectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            // Bigger hits look bigger, so light and heavy towers read differently at a glance.
            projectile.transform.localScale *= Mathf.Clamp(Mathf.Sqrt(Damage / baseLevel.damage), 0.75f, 2f);
            projectile.Launch(target, Damage);

            cooldown = AttackInterval;
            ShotsFired++;
        }

        /// <summary>A specialised tower takes its branch's colour and barrel, and grows a little with each tier.</summary>
        void UpdateBranchLook()
        {
            TowerBranch branch = Progression.Branch;
            if (branch == null)
                return;

            if (baseSprite != null)
            {
                baseSprite.color = branch.color;
                baseSprite.transform.localScale = Vector3.one * (0.7f + 0.06f * Progression.Tier);
            }
            if (barrel != null)
            {
                barrel.localScale = new Vector3(branch.barrelScale.x, branch.barrelScale.y, 1f);
                barrel.localPosition = new Vector3(branch.barrelScale.x / 2f, 0f, 0f);
            }
        }

        void UpdateRangeIndicator()
        {
            if (rangeIndicator != null)
                rangeIndicator.localScale = Vector3.one * (Range * 2f);
        }

        void OnValidate()
        {
            if (rangeIndicator != null && baseLevel != null)
                rangeIndicator.localScale = Vector3.one * (baseLevel.range * 2f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? Range : baseLevel.range);
        }
    }
}
