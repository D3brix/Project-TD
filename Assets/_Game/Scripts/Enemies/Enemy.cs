using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Enemies
{
    /// <summary>
    /// Walks a path, takes damage, and is removed when it is killed or reaches the end.
    /// Killed and ReachedEnd are mutually exclusive and each fires at most once (see <see cref="EnemyState"/>).
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField, Min(1f)] float maxHealth = 10f;
        [SerializeField, Min(0f)] float moveSpeed = 1.5f;
        [SerializeField, Min(0)] int reward = 5;
        [SerializeField, Min(1)] int livesCost = 1;

        EnemyState state;
        PathFollower path;
        bool halted;

        public event Action<Enemy> Killed;
        public event Action<Enemy> ReachedEnd;

        public bool IsAlive => state != null && state.IsAlive;
        public float Health => state?.Health ?? 0f;
        public float MaxHealth => state?.MaxHealth ?? maxHealth;
        public int Reward => reward;
        public int LivesCost => livesCost;
        public Vector2 Position => transform.position;

        /// <summary>How far this enemy has walked along its path. Larger means closer to the end.</summary>
        public float PathProgress => path?.DistanceTravelled ?? 0f;

        /// <param name="pathPoints">The route as a polyline; for a level this is the densely sampled smooth curve.</param>
        public void Initialize(IReadOnlyList<Vector2> pathPoints, float healthMultiplier = 1f)
        {
            state = new EnemyState(maxHealth * healthMultiplier);
            path = new PathFollower(pathPoints);
            transform.position = path.Position;
        }

        /// <summary>Freezes the enemy for good (the game is over): it stops moving and ignores damage.</summary>
        public void Halt()
        {
            halted = true;
        }

        void Update()
        {
            Move(moveSpeed * Time.deltaTime);
        }

        internal void Move(float distance)
        {
            if (!IsAlive || halted)
                return;

            path.Advance(distance);
            transform.position = path.Position;

            if (path.ReachedEnd && state.ReachEnd())
            {
                ReachedEnd?.Invoke(this);
                Destroy(gameObject);
            }
        }

        public void TakeDamage(float amount)
        {
            if (state == null || halted || !state.ApplyDamage(amount))
                return;

            Killed?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
