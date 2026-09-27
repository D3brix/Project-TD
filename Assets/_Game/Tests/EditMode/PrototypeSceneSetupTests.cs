using System.Linq;
using NUnit.Framework;
using ProjectTD.Core;
using ProjectTD.Levels;
using ProjectTD.Placement;
using ProjectTD.Towers;
using ProjectTD.UI;
using ProjectTD.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ProjectTD.Tests
{
    /// <summary>Static checks on the development level: it is in the build, fully wired up, and its road is sound.</summary>
    public class PrototypeSceneSetupTests
    {
        const string ScenePath = "Assets/_Game/Scenes/Development/Prototype.unity";

        Scene scene;

        [OneTimeSetUp]
        public void OpenScene()
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        [OneTimeTearDown]
        public void CloseScene()
        {
            EditorSceneManager.CloseScene(scene, true);
        }

        T[] Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();

        [Test]
        public void SceneIsEnabledInBuildSettings()
        {
            Assert.IsTrue(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == ScenePath));
        }

        [Test]
        public void NoMissingScripts()
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject), $"Missing script on {t.name}");
        }

        [Test]
        public void AllSerializedReferencesOnGameComponentsAreAssigned()
        {
            var components = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Where(c => c.GetType().Namespace != null && c.GetType().Namespace.StartsWith("ProjectTD"));

            int checkedReferences = 0;
            foreach (MonoBehaviour component in components)
            {
                SerializedProperty property = new SerializedObject(component).GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script")
                        continue;
                    Assert.IsNotNull(property.objectReferenceValue, $"{component.name}.{component.GetType().Name}.{property.propertyPath} is not assigned");
                    checkedReferences++;
                }
            }

            Assert.Greater(checkedReferences, 0);
        }

        [Test]
        public void HasTheGameplayObjects_AndNoPrePlacedTowers()
        {
            Assert.AreEqual(1, Find<GameController>().Length);
            Assert.AreEqual(1, Find<WaveSpawner>().Length);
            Assert.AreEqual(1, Find<LevelPath>().Length);
            Assert.AreEqual(1, Find<Battlefield>().Length);
            Assert.AreEqual(1, Find<TowerBuilder>().Length);
            Assert.AreEqual(1, Find<TowerInteraction>().Length);
            Assert.AreEqual(1, Find<Hud>().Length);
            Assert.AreEqual(1, Find<EventSystem>().Length, "UI buttons need an EventSystem.");
            Assert.AreEqual(0, Find<Tower>().Length, "The level starts without towers: the player builds them.");
            Assert.IsNotEmpty(Find<TowerBuilder>()[0].AvailableTowers);
        }

        [Test]
        public void Road_IsSmooth_AndStaysInsideTheBattlefield()
        {
            LevelPath path = Find<LevelPath>()[0];
            Rect bounds = Find<Battlefield>()[0].Bounds;
            path.Rebuild();

            Assert.Less(PathCurveTests.MaxTurnPerSegment(path.Points), 6f, "No hard corners anywhere along the road.");
            foreach (Vector2 point in path.Points)
                Assert.IsTrue(bounds.Contains(point), $"Road point {point} is outside the battlefield.");
        }

        [Test]
        public void SpawnAndEndMarkers_AreFullyInsideTheBattlefield()
        {
            Rect bounds = Find<Battlefield>()[0].Bounds;
            foreach (string name in new[] { "SpawnMarker", "EndMarker" })
            {
                GameObject marker = scene.GetRootGameObjects().Single(r => r.name == name);
                foreach (SpriteRenderer sprite in marker.GetComponentsInChildren<SpriteRenderer>())
                {
                    Bounds b = sprite.bounds;
                    Assert.IsTrue(bounds.Contains(b.min) && bounds.Contains(b.max), $"{name}/{sprite.name} pokes out of the battlefield.");
                }
            }
        }

        [Test]
        public void InsideTheBridgeHorseshoe_ThereIsABuildableSpot_ThatCoversMuchMoreRoadThanTheWesternStraight()
        {
            LevelPath path = Find<LevelPath>()[0];
            Battlefield battlefield = Find<Battlefield>()[0];
            path.Rebuild();
            var rules = new PlacementRules(battlefield.Bounds, path.Points, path.BuildClearance, battlefield.GetBlockedAreas());
            const float footprint = 0.4f, range = 3f;

            var insideBend = new Vector2(2.2f, 2.8f);
            var besideStraight = new Vector2(-9.2f, 1.3f);
            Assert.AreEqual(PlacementResult.Valid, rules.Check(new CircleArea(insideBend, footprint), new CircleArea[0]));
            Assert.AreEqual(PlacementResult.Valid, rules.Check(new CircleArea(besideStraight, footprint), new CircleArea[0]));

            float bendCoverage = RoadLengthWithin(path, insideBend, range);
            float straightCoverage = RoadLengthWithin(path, besideStraight, range);
            TestContext.WriteLine($"Road length {path.Length:0.0}; covered from bend spot {bendCoverage:0.0}, from straight {straightCoverage:0.0}");
            Assert.Greater(bendCoverage, straightCoverage * 1.4f);
        }

        static float RoadLengthWithin(LevelPath path, Vector2 origin, float range)
        {
            float covered = 0f;
            for (int i = 1; i < path.Points.Count; i++)
                if (Vector2.Distance(path.Points[i], origin) <= range)
                    covered += Vector2.Distance(path.Points[i - 1], path.Points[i]);
            return covered;
        }
    }
}
