using System.Linq;
using NUnit.Framework;
using ProjectTD.Core;
using ProjectTD.Towers;
using ProjectTD.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTD.Tests
{
    /// <summary>Static checks on the Phase 1 development scene: it is in the build and fully wired up.</summary>
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
                    Assert.IsNotNull(property.objectReferenceValue, $"{component.name}.{component.GetType().Name}.{property.name} is not assigned");
                    checkedReferences++;
                }
            }

            Assert.Greater(checkedReferences, 0);
        }

        [Test]
        public void HasTheCoreGameplayObjects()
        {
            var roots = scene.GetRootGameObjects();
            Assert.AreEqual(1, roots.SelectMany(r => r.GetComponentsInChildren<GameController>(true)).Count());
            Assert.AreEqual(1, roots.SelectMany(r => r.GetComponentsInChildren<WaveSpawner>(true)).Count());
            Assert.Greater(roots.SelectMany(r => r.GetComponentsInChildren<Tower>(true)).Count(), 0);
        }
    }
}
