using System.Collections;
using NUnit.Framework;
using ProjectTD.Combat;
using ProjectTD.Enemies;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectTD.Tests
{
    public class EnemyAndProjectileTests
    {
        static readonly Vector2[] StraightPath = { new Vector2(0f, 0f), new Vector2(3f, 0f) };

        static Enemy CreateEnemy()
        {
            Enemy enemy = new GameObject("TestEnemy").AddComponent<Enemy>(); // defaults: 10 HP, 1.5 speed
            enemy.Initialize(StraightPath);
            return enemy;
        }

        static Projectile LaunchProjectile(Vector2 from, Enemy target, float damage)
        {
            Projectile projectile = new GameObject("TestProjectile").AddComponent<Projectile>();
            projectile.transform.position = from;
            projectile.Launch(target, damage);
            return projectile;
        }

        [UnityTest]
        public IEnumerator Killed_FiresOnce_AndEnemyIsRemoved()
        {
            Enemy enemy = CreateEnemy();
            int killed = 0, reachedEnd = 0;
            enemy.Killed += _ => killed++;
            enemy.ReachedEnd += _ => reachedEnd++;

            enemy.TakeDamage(100f);
            enemy.TakeDamage(100f);
            yield return null;

            Assert.AreEqual(1, killed);
            Assert.AreEqual(0, reachedEnd);
            Assert.IsTrue(enemy == null, "The killed enemy is destroyed.");
        }

        [UnityTest]
        public IEnumerator ReachedEnd_FiresOnce_AndEnemyCannotBeKilledAfterwards()
        {
            Enemy enemy = CreateEnemy();
            int killed = 0, reachedEnd = 0;
            enemy.Killed += _ => killed++;
            enemy.ReachedEnd += _ => reachedEnd++;

            enemy.Move(10f);
            enemy.Move(10f);
            enemy.TakeDamage(100f);
            yield return null;

            Assert.AreEqual(1, reachedEnd);
            Assert.AreEqual(0, killed);
            Assert.IsTrue(enemy == null, "The escaped enemy is destroyed.");
        }

        [UnityTest]
        public IEnumerator Enemy_WalksThePath_OverTime()
        {
            Enemy enemy = CreateEnemy();
            Vector2 start = enemy.Position;

            yield return new WaitForSeconds(0.5f);

            Assert.Greater(enemy.Position.x, start.x);
            Assert.AreEqual(0f, enemy.Position.y, 1e-5f);
            Object.Destroy(enemy.gameObject);
        }

        [UnityTest]
        public IEnumerator Projectile_TravelsToTarget_DamagesIt_AndIsRemoved()
        {
            Enemy enemy = CreateEnemy();
            enemy.enabled = false; // hold still
            Projectile projectile = LaunchProjectile(new Vector2(0f, 3f), enemy, 4f);

            yield return null;
            Assert.IsTrue(projectile != null, "The projectile travels over several frames rather than hitting instantly.");
            Assert.AreEqual(10f, enemy.Health);

            float timeout = Time.time + 2f;
            while (projectile != null && Time.time < timeout)
                yield return null;

            Assert.IsTrue(projectile == null, "The projectile is removed after hitting.");
            Assert.AreEqual(6f, enemy.Health);
            Object.Destroy(enemy.gameObject);
        }

        [UnityTest]
        public IEnumerator Projectile_WhoseTargetDiesFirst_DealsNoDamage_AndIsRemoved()
        {
            Enemy target = CreateEnemy();
            target.enabled = false;
            Enemy bystander = CreateEnemy(); // standing on the target's position
            bystander.enabled = false;
            Projectile projectile = LaunchProjectile(new Vector2(0f, 3f), target, 4f);

            yield return null;
            target.TakeDamage(100f); // killed by something else before the projectile arrives

            float timeout = Time.time + 2f;
            while (projectile != null && Time.time < timeout)
                yield return null;

            Assert.IsTrue(projectile == null, "The projectile cleans itself up.");
            Assert.AreEqual(10f, bystander.Health, "A projectile never hits anything other than its own target.");
            Object.Destroy(bystander.gameObject);
        }
    }
}
