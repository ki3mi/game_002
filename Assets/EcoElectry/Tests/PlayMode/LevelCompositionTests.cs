using System.Collections;
using System.Linq;
using EcoElectry.Unity.Levels;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EcoElectry.Tests.PlayMode
{
    public sealed class LevelCompositionTests
    {
        private const string FirstLevelPath = "Assets/EcoElectry/Content/Scenes/Level01-LaPrimeraAula.unity";

        [UnityTest]
        public IEnumerator FirstLevelCreatesOneSessionAndReleasesItWhenUnloaded()
        {
            yield return SceneManager.LoadSceneAsync(FirstLevelPath, LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByPath(FirstLevelPath);
            Assert.That(scene.isLoaded, Is.True);

            LevelRoot[] roots = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<LevelRoot>(true))
                .ToArray();

            Assert.That(roots, Has.Length.EqualTo(1));
            LevelRoot levelRoot = roots[0];
            Assert.That(levelRoot.Session, Is.Not.Null);
            Assert.That(levelRoot.Session.Rules.Id, Is.EqualTo("level-01"));

            yield return SceneManager.UnloadSceneAsync(scene);
            Assert.That(levelRoot.Session, Is.Null);
        }

        [UnityTearDown]
        public IEnumerator UnloadFirstLevelAfterFailure()
        {
            Scene scene = SceneManager.GetSceneByPath(FirstLevelPath);
            if (scene.IsValid() && scene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(scene);
        }
    }
}
