#if UNITY_EDITOR
using System.Collections;
using EcoElectry.Unity.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EcoElectry.Tests.PlayMode
{
    public sealed class LevelCompositionTests
    {
        private const string DefinitionPath = "Assets/EcoElectry/Content/Definitions/Levels/Level01-LaPrimeraAula.asset";
        private const string SessionSceneName = "Level01SessionTest";

        [UnityTest]
        public IEnumerator FirstLevelDefinitionCreatesSessionAndReleasesItWhenSceneUnloads()
        {
            // Exercise the session lifetime without hot-loading the rendered campus into the test runner's scene.
            Scene scene = SceneManager.CreateScene(SessionSceneName);
            LevelDefinition definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(DefinitionPath);
            Assert.That(definition, Is.Not.Null);
            var root = new GameObject("LevelRoot");
            root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, scene);
            var levelRoot = root.AddComponent<LevelRoot>();
            var data = new SerializedObject(levelRoot);
            data.FindProperty("definition").objectReferenceValue = definition;
            data.ApplyModifiedPropertiesWithoutUndo();
            root.SetActive(true);

            Assert.That(levelRoot.Session, Is.Not.Null);
            Assert.That(levelRoot.Session.Rules.Id, Is.EqualTo("level-01"));

            yield return SceneManager.UnloadSceneAsync(scene);
            Assert.That(levelRoot.Session, Is.Null);
        }

        [UnityTearDown]
        public IEnumerator UnloadSessionSceneAfterFailure()
        {
            Scene scene = SceneManager.GetSceneByName(SessionSceneName);
            if (scene.IsValid() && scene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(scene);
        }
    }
}
#endif
