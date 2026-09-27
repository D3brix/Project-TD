using System.Collections.Generic;
using ProjectTD.Core;
using ProjectTD.Levels;
using ProjectTD.Placement;
using UnityEngine;

namespace ProjectTD.Towers
{
    /// <summary>
    /// Owns the towers the player has built. Every purchase, upgrade and sale goes through here, so currency
    /// changes exactly once per action and only for actions that actually happen. Nothing can be bought,
    /// upgraded or sold once the game is over, and all towers stop when it ends.
    /// </summary>
    public class TowerBuilder : MonoBehaviour
    {
        [SerializeField] GameController game;
        [SerializeField] LevelPath path;
        [SerializeField] Battlefield battlefield;
        [SerializeField] Transform towerParent;
        [Tooltip("Towers the player can build on this level.")]
        [SerializeField] Tower[] availableTowers;
        [SerializeField, Range(0, 100)] int sellRefundPercent = 70;

        readonly List<Tower> towers = new List<Tower>();
        PlacementRules rules;

        public IReadOnlyList<Tower> Towers => towers;
        public IReadOnlyList<Tower> AvailableTowers => availableTowers;
        public int SellRefundPercent => sellRefundPercent;
        public bool IsLocked => game.Session.IsOver;

        PlacementRules Rules => rules ??= new PlacementRules(battlefield.Bounds, path.Points, path.BuildClearance, battlefield.GetBlockedAreas());

        void OnEnable()
        {
            game.GameEnded += HandleGameEnded;
        }

        void OnDisable()
        {
            game.GameEnded -= HandleGameEnded;
        }

        public bool CanAfford(Tower prefab) => game.Session.CanAfford(prefab.BuildCost);

        public PlacementResult CheckPlacement(Tower prefab, Vector2 position)
        {
            if (IsLocked)
                return PlacementResult.GameOver;

            var footprints = new List<CircleArea>(towers.Count);
            foreach (Tower tower in towers)
                footprints.Add(Footprint(tower, tower.transform.position));

            PlacementResult result = Rules.Check(Footprint(prefab, position), footprints);
            if (result == PlacementResult.Valid && !CanAfford(prefab))
                return PlacementResult.NotEnoughCurrency;
            return result;
        }

        /// <summary>Buys and places a tower if the spot is valid and affordable. Returns the new tower, or null (nothing spent).</summary>
        public Tower TryPlace(Tower prefab, Vector2 position)
        {
            if (CheckPlacement(prefab, position) != PlacementResult.Valid || !game.Session.TrySpend(prefab.BuildCost))
                return null;

            Tower tower = Instantiate(prefab, new Vector3(position.x, position.y, 0f), Quaternion.identity, towerParent);
            tower.name = $"{prefab.name} {towers.Count + 1}";
            tower.Initialize(game.Waves);
            tower.ShowRange(false, Color.white);
            towers.Add(tower);
            return tower;
        }

        public bool CanUpgrade(Tower tower) =>
            !IsLocked && towers.Contains(tower) && !tower.Progression.IsMaxLevel && game.Session.CanAfford(tower.Progression.Next.cost);

        /// <summary>Buys the tower's next level. Returns false (nothing spent) at max level, when unaffordable, or after the game ends.</summary>
        public bool TryUpgrade(Tower tower)
        {
            if (!CanUpgrade(tower) || !game.Session.TrySpend(tower.Progression.Next.cost))
                return false;

            return tower.ApplyUpgrade();
        }

        public int SellValue(Tower tower) => tower.Progression.SellValue(sellRefundPercent);

        /// <summary>Removes the tower and refunds <see cref="SellValue"/>. A tower can only be sold once, and not after the game ends.</summary>
        public bool TrySell(Tower tower)
        {
            if (IsLocked || tower == null || !towers.Remove(tower))
                return false;

            int refund = SellValue(tower);
            tower.enabled = false; // stops acting this frame; projectiles already in flight finish on their own
            Destroy(tower.gameObject);
            game.Session.AddCurrency(refund);
            return true;
        }

        /// <summary>The placed tower whose footprint contains <paramref name="point"/>, if any.</summary>
        public Tower TowerAt(Vector2 point)
        {
            foreach (Tower tower in towers)
                if (Vector2.Distance(tower.transform.position, point) <= tower.FootprintRadius)
                    return tower;
            return null;
        }

        static CircleArea Footprint(Tower tower, Vector2 position) => new CircleArea(position, tower.FootprintRadius);

        void HandleGameEnded(GameOutcome outcome)
        {
            foreach (Tower tower in towers)
                tower.enabled = false;
        }
    }
}
