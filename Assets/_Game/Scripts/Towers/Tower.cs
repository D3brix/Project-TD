using ProjectTD.Combat;
using ProjectTD.Enemies;
using ProjectTD.Waves;
using UnityEngine;

namespace ProjectTD.Towers
{
    /// <summary>
    /// Re-picks a target every frame among the spawner's active enemies and fires a projectile
    /// at it whenever the attack cooldown allows.
    /// </summary>
    public class Tower : MonoBehaviour
    {
        [SerializeField] WaveSpawner enemySource;
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] Transform turret;
        [SerializeField] Transform firePoint;
        [SerializeField] Transform rangeIndicator;

        [SerializeField, Min(0.1f)] float range = 3f;
        [SerializeField, Min(0.05f)] float attackInterval = 0.6f;
        [SerializeField, Min(0f)] float damage = 3f;
        [SerializeField] TargetingMode targetingMode = TargetingMode.First;

        float cooldown;

        public Enemy CurrentTarget { get; private set; }
        public int ShotsFired { get; private set; }
        public float Range => range;

        void Update()
        {
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

            CurrentTarget = TargetSelector.Select(enemySource.ActiveEnemies, transform.position, range, targetingMode);
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

        void Fire(Enemy target)
        {
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            Projectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectile.Launch(target, damage);

            cooldown = attackInterval;
            ShotsFired++;
        }

        void OnValidate()
        {
            if (rangeIndicator != null)
                rangeIndicator.localScale = Vector3.one * (range * 2f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
