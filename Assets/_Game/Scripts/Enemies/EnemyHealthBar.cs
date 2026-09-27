using UnityEngine;

namespace ProjectTD.Enemies
{
    /// <summary>Shrinks a left-anchored fill sprite to match the enemy's remaining health.</summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] Enemy enemy;
        [SerializeField] Transform fill;

        float fullWidth;
        float leftEdge;

        void Awake()
        {
            fullWidth = fill.localScale.x;
            leftEdge = fill.localPosition.x - fullWidth / 2f;
        }

        void LateUpdate()
        {
            float ratio = enemy.MaxHealth > 0f ? Mathf.Clamp01(enemy.Health / enemy.MaxHealth) : 0f;
            float width = fullWidth * ratio;

            fill.localScale = new Vector3(width, fill.localScale.y, 1f);
            fill.localPosition = new Vector3(leftEdge + width / 2f, fill.localPosition.y, fill.localPosition.z);
        }
    }
}
