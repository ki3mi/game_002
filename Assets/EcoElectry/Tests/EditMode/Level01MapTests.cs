using System.Linq;
using EcoElectry.Unity.Levels;
using EcoElectry.Unity.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoElectry.Tests.EditMode
{
    public sealed class Level01MapTests
    {
        private const string ScenePath = "Assets/EcoElectry/Content/Scenes/Level01-LaPrimeraAula.unity";

        [Test]
        public void MapDependsOnLevel01AssetsAndSharedResources()
        {
            string[] dependencies = AssetDatabase.GetDependencies(ScenePath, true);
            Assert.That(dependencies, Does.Contain("Assets/EcoElectry/Content/Definitions/Levels/Level01-LaPrimeraAula.asset"));
            Assert.That(dependencies, Does.Contain("Assets/EcoElectry/Content/Terrain/Level01/RuralTerrainData.asset"));
            Assert.That(dependencies, Does.Contain("Assets/EcoElectry/Content/Prefabs/Level01/PF_BasketballCourt.prefab"));
            Assert.That(dependencies, Does.Contain("Assets/EcoElectry/Content/Prefabs/Level01/PF_BasketballHoop.prefab"));
            Assert.That(dependencies, Does.Contain("Assets/EcoElectry/Content/Prefabs/Level01/PF_SchoolSign.prefab"));
            Assert.That(dependencies.Where(path => path.Contains("SandboxElectrical") || path.Contains("SandboxDefinition")
                || path.Contains("GridModelCatalog")), Is.Empty);
        }

        [Test]
        public void MapHasOneExplorerAndKeepsItsCampusPrefabsLinkedAtSourceScale()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                Assert.That(roots.Select(root => root.name), Is.SupersetOf(new[]
                    { "LevelRoot", "Player", "Terrain", "Road", "Forest", "School", "Lighting", "LevelMarkers" }));

                var levels = roots.SelectMany(root => root.GetComponentsInChildren<LevelRoot>()).ToArray();
                Assert.That(levels, Has.Length.EqualTo(1));
                Assert.That(levels[0].Definition.CreateRules().Id, Is.EqualTo("level-01"));
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<ThirdPersonExplorer>()).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<Camera>()).Count(camera => camera.enabled),
                    Is.EqualTo(1));

                Transform school = roots.Single(root => root.name == "School").transform;
                AssertLinked(school.Find("Entrance/PuertaPeatonal"), "PuertaPeatonal");
                AssertLinked(school.Find("Classrooms/CommunityClassroom/AulaModularBasica"), "AulaModularBasica");
                AssertLinked(school.Find("Classrooms/SecondClassroom/AulaModularAmpliada"), "AulaModularAmpliada");
                AssertLinked(school.Find("ElectricalRoom/SalaCompacta/SalaElectricaCompacta"), "SalaElectricaCompacta");
                AssertLinked(school.Find("BasketballCourt/PF_BasketballCourt"), "PF_BasketballCourt");

                Transform markers = roots.Single(root => root.name == "LevelMarkers").transform;
                foreach (string marker in new[] { "PlayerSpawn", "Teacher", "Caretaker", "Board", "Battery", "PanelPatio", "PanelRoof", "RoofAccess" })
                    Assert.That(markers.Find(marker), Is.Not.Null, marker);
                Assert.That(markers.Find("PanelRoof").position.y, Is.GreaterThan(markers.Find("PanelPatio").position.y + 3f));
            }
            finally
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void AssertLinked(Transform instance, string resource)
        {
            Assert.That(instance, Is.Not.Null, resource);
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
            Assert.That(source, Is.Not.Null, resource);
            Assert.That(AssetDatabase.GetAssetPath(source), Does.EndWith("/" + resource + ".prefab"));
            Assert.That(instance.localScale, Is.EqualTo(source.transform.localScale), resource);
        }
    }
}
