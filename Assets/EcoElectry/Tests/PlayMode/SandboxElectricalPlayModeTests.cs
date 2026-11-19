#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using EcoElectry.Application.Electrical;
using EcoElectry.Domain.Electrical;
using EcoElectry.Unity.Electrical;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EcoElectry.Tests.PlayMode
{
    public sealed class SandboxElectricalPlayModeTests
    {
        private const string ScenePath = "Assets/EcoElectry/Content/Scenes/SandboxElectrical.unity";
        private SandboxElectricalRoot sandbox;

        [UnitySetUp]
        public IEnumerator OpenSandbox()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Additive));
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.isLoaded, Is.True);
            SceneManager.SetActiveScene(scene);
            sandbox = scene.GetRootGameObjects().Select(root => root.GetComponent<SandboxElectricalRoot>())
                .Single(root => root != null);
            Assert.That(sandbox.Session.Scenario.Id, Is.EqualTo("build"));
        }

        [UnityTest]
        public IEnumerator ComposesDirectlyAndConnectsTheThreeEquipmentLinks()
        {
            Assert.That(sandbox.IsFirstPerson, Is.False);
            sandbox.ToggleCamera();
            Assert.That(sandbox.IsFirstPerson, Is.True);
            sandbox.ToggleCamera();
            sandbox.SelectTool(CableTool.Measure);
            Assert.That(sandbox.IsFirstPerson, Is.True);
            sandbox.FinishIntervention();
            Assert.That(sandbox.IsFirstPerson, Is.False);

            sandbox.Place(Equipment.Panel);
            sandbox.Place(Equipment.Battery);
            sandbox.Place(Equipment.Lamp);
            Assert.That(sandbox.StartRoute(Terminal.SolarOut, CableFamily.Solar), Is.True);
            sandbox.ChooseDestination(Terminal.SolarIn);
            sandbox.FinishIntervention();
            Assert.That(sandbox.Session.Circuit.Cables, Is.Empty);
            Assert.That(ActiveLines(), Is.Zero);

            Connect(Terminal.SolarOut, Terminal.SolarIn, CableFamily.Solar);
            Connect(Terminal.BatterySocket, Terminal.StorageIn, CableFamily.Interior);
            Connect(Terminal.LightingOut, Terminal.LampIn, CableFamily.Interior,
                RoutePoint.FloorLeft, RoutePoint.WallLeft, RoutePoint.CeilingLeft,
                RoutePoint.CeilingRight, RoutePoint.WallRight);
            Assert.That(ActiveLines(), Is.GreaterThan(20));

            sandbox.SelectTool(CableTool.Measure);
            Assert.That(sandbox.ApplyTool(Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            sandbox.Session.SetSolar(true);
            sandbox.Session.SetBattery(true);
            sandbox.Session.SetBlades(true);
            Assert.That(sandbox.RunTest().LitMinutes, Is.EqualTo(120));
            sandbox.CloseReport();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShockRespawnsAndResetRebuildsExactlyOneInstallation()
        {
            Vector3 yard = sandbox.PlayerPosition;
            sandbox.LoadCase(1);
            Assert.That(sandbox.RunTest().Failure, Is.EqualTo(Failure.OpenCircuit));
            sandbox.CloseReport();
            string task = sandbox.Session.Task;
            sandbox.SelectTool(CableTool.Secure);
            Assert.That(sandbox.ApplyTool(Terminal.LampIn), Is.EqualTo(InteractionResult.Shock));
            Assert.That(Vector3.Distance(yard, sandbox.PlayerPosition), Is.GreaterThan(2));
            Assert.That(sandbox.Session.SpawnId, Is.EqualTo(SandboxSession.BoardSpawn));
            Assert.That(sandbox.Session.Task, Is.EqualTo(task));
            Assert.That(sandbox.IsIntervening, Is.False);

            sandbox.Session.SetSolar(false);
            sandbox.Session.SetBattery(false);
            sandbox.Session.SetBlades(false);
            sandbox.SelectTool(CableTool.Remove);
            int before = ActiveLines();
            Assert.That(sandbox.ApplyTool(Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            Assert.That(ActiveLines(), Is.LessThan(before));
            Assert.That(sandbox.Session.Circuit.At(Terminal.LampIn), Is.Null);

            sandbox.LoadCase(3);
            Assert.That(sandbox.RunTest().Failure, Is.EqualTo(Failure.InsufficientEnergy));
            sandbox.Session.SelectEfficientLamp(true);
            Assert.That(sandbox.RunTest().Passed, Is.True);
            sandbox.ResetCase();
            yield return null;
            Assert.That(sandbox.Session.Circuit.Cables.Count, Is.EqualTo(3));
            Assert.That(sandbox.Session.Report, Is.Null);
            Assert.That(sandbox.Session.LastMeasurement, Is.Null);
            Assert.That(sandbox.GetComponentsInChildren<Transform>().Count(child => child.name == "Trazados"), Is.EqualTo(1));
        }

        [UnityTearDown]
        public IEnumerator UnloadSandbox()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator GridPlacesModelInBothViewsAndSupportsUndoAndReset()
        {
            var panel = sandbox.PlaceableModels.Single(model => model.Source.name == "PanelSolar1");
            Assert.That(sandbox.SelectPlacementModel(panel.Id), Is.True);
            Assert.That(sandbox.TryPlaceModel(Down(-4.75f, 1.25f)), Is.False);
            Assert.That(sandbox.TryPlaceModel(Down(8.25f, -3.75f)), Is.False);

            Ray validPosition = Down(3.75f, -3.75f);
            Assert.That(sandbox.TryPlaceModel(validPosition), Is.True);
            Assert.That(sandbox.PlacedModelCount, Is.EqualTo(1));
            Assert.That(sandbox.TryPlaceModel(Down(4.25f, -3.75f)), Is.False);

            sandbox.ToggleCamera();
            Assert.That(sandbox.IsFirstPerson, Is.True);
            sandbox.UndoPlacement();
            Assert.That(sandbox.PlacedModelCount, Is.Zero);
            Assert.That(sandbox.TryPlaceModel(validPosition), Is.True);
            sandbox.ResetCase();
            Assert.That(sandbox.PlacedModelCount, Is.Zero);
            Assert.That(sandbox.TryPlaceModel(validPosition), Is.False);
            yield return null;

            Assert.That(sandbox.GetComponentsInChildren<Transform>().Count(child => child.name == "Superficie de construcción"),
                Is.EqualTo(1));
        }

        private static Ray Down(float x, float z) => new Ray(new Vector3(x, 8f, z), Vector3.down);

        private int ActiveLines() => sandbox.GetComponentsInChildren<LineRenderer>().Length;

        private void Connect(Terminal origin, Terminal destination, CableFamily family, params RoutePoint[] points)
        {
            Assert.That(sandbox.StartRoute(origin, family), Is.True);
            foreach (RoutePoint point in points) sandbox.AddRoutePoint(point);
            sandbox.ChooseDestination(destination);
            Assert.That(sandbox.ConfirmRoute(), Is.True);
            sandbox.SelectTool(CableTool.Strip);
            Assert.That(sandbox.ApplyTool(origin), Is.EqualTo(InteractionResult.Applied));
            Assert.That(sandbox.ApplyTool(destination), Is.EqualTo(InteractionResult.Applied));
            sandbox.SelectTool(CableTool.Secure);
            Assert.That(sandbox.ApplyTool(origin), Is.EqualTo(InteractionResult.Applied));
            Assert.That(sandbox.ApplyTool(destination), Is.EqualTo(InteractionResult.Applied));
            sandbox.FinishIntervention();
        }
    }
}
#endif
