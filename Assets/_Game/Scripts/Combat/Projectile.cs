using ProjectTD.Enemies;
using UnityEngine;

namespace ProjectTD.Combat
{
    /// <summary>
    /// Homes in on its target and damages it on arrival.
    /// If the target dies or leaves the map first, the projectile flies on to the target's
    /// last known position and disappears there without dealing damage.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float speed = 10f;
        [SerializeField, Min(0.01f)] float hitDistance = 0.1f;
        [SerializeField, Min(0.1f)] float maxLifetime = 5f;

        Enemy target;
        float damage;
        Vector2 destination;
        float age;

        public void Launch(Enemy target, float damage)
        {
            this.target = target;
            this.damage = damage;
            destination = target.Position;
        }

        void Update()
        {
            if (target != null && target.IsAlive)
                destination = target.Position;
            else
                target = null;

            transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, destination) <= hitDistance)
            {
                if (target != null)
                    target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            age += Time.deltaTime;
            if (age >= maxLifetime)
                Destroy(gameObject);
        }
    }
}
