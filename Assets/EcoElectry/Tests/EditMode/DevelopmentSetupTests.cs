using System.Linq;
using EcoElectry.Unity.Levels;
using NUnit.Framework;
using UnityEditor;

namespace EcoElectry.Tests.EditMode
{
    public sealed class DevelopmentSetupTests
    {
        [Test]
        public void StartupAndIntroductionAreTheFirstEnabledBuildScenes()
        {
            const string sceneDirectory = "Assets/EcoElectry/Content/Scenes/";

            string[] firstScenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Take(2)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(firstScenePaths, Is.EqualTo(new[]
            {
                sceneDirectory + "Startup.unity",
                sceneDirectory + "Level00-Introduction.unity"
            }));

            foreach (string path in firstScenePaths)
                Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(path), Is.Not.Null, path);
        }

        [Test]
        public void FirstLevelAssetProvidesTheDocumentedBudgetAndReserve()
        {
            const string definitionPath = "Assets/EcoElectry/Content/Definitions/Levels/Level01-LaPrimeraAula.asset";

            var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(definitionPath);
            Assert.That(definition, Is.Not.Null);

            var rules = definition.CreateRules();
            Assert.That(rules.Id, Is.EqualTo("level-01"));
            Assert.That(rules.BudgetCredits, Is.EqualTo(100));
            Assert.That(rules.MinimumBatteryReservePercent, Is.EqualTo(20));
        }
    }
}
