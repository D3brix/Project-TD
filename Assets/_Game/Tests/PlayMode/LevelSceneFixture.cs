using System.Collections;
using NUnit.Framework;
using ProjectTD.Core;
using ProjectTD.Levels;
using ProjectTD.Placement;
using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectTD.Tests
{
    /// <summary>
    /// Loads the real development level before each test. Towers are placed through the same
    /// <see cref="TowerBuilder"/> API the player's clicks use, at spots known to be valid on this map.
    /// Any error or exception logged during a test fails it.
    /// </summary>
    public abstract class LevelSceneFixture
    {
        protected const string SceneName = "Prototype";

        // Known build spots on the level.
        protected static readonly Vector2 InsideHorseshoeSpot = new Vector2(2.2f, 2.8f);  // sees both legs and the bridge
        protected static readonly Vector2 HorseshoeTopSpot = new Vector2(2.4f, 4.2f);
        protected static readonly Vector2 RidgeBendSpot = new Vector2(7.8f, -3.0f);     // inside the bend round the ridge's tip
        protected static readonly Vector2 WestBankSpot = new Vector2(-2.7f, 2.1f);
        protected static readonly Vector2 NearSpawnSpot = new Vector2(-11.2f, 1.4f);

        // The basic tower's development branches, in the order the prefab lists them.
        protected const int Rapid = 0, Heavy = 1, Balanced = 2;

        protected GameController game;
        protected TowerBuilder builder;
        protected TowerInteraction interaction;
        protected LevelPath path;
        protected Tower towerPrefab;

        [UnitySetUp]
        public IEnumerator LoadLevel()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            FindLevelObjects();
        }

        [TearDown]
        public void ResetTimeScale()
        {
            Time.timeScale = 1f;
        }

        protected void FindLevelObjects()
        {
            game = Object.FindAnyObjectByType<GameController>();
            builder = Object.FindAnyObjectByType<TowerBuilder>();
            interaction = Object.FindAnyObjectByType<TowerInteraction>();
            path = Object.FindAnyObjectByType<LevelPath>();
            Assert.IsNotNull(game);
            Assert.IsNotNull(builder);
            Assert.IsNotNull(interaction);
            towerPrefab = builder.AvailableTowers[0];
        }

        protected Tower Place(Vector2 spot)
        {
            Tower tower = builder.TryPlace(towerPrefab, spot);
            Assert.IsNotNull(tower, $"Could not place a tower at {spot}: {builder.CheckPlacement(towerPrefab, spot)}");
            return tower;
        }

        protected static IEnumerator WaitFor(System.Func<bool> condition, float maxGameSeconds)
        {
            float deadline = Time.time + maxGameSeconds;
            while (!condition() && Time.time < deadline)
                yield return null;
            Assert.IsTrue(condition(), $"Condition not met within {maxGameSeconds} game seconds.");
        }

        protected static string HudText(string path) => GameObject.Find("HUD/" + path)?.GetComponent<Text>()?.text;
    }
}
