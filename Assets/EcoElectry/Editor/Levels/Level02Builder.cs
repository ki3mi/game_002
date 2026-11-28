using System;
using EcoElectry.Unity.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    internal sealed partial class Level02Builder
    {
        internal const string ScenePath = "Assets/EcoElectry/Content/Scenes/Level02.unity";
        private const string InputPath = "Assets/ResourcePack/InputSystem_Actions.inputactions";
        private const string CorridorMaterials = "Assets/EcoElectry/Content/Models/HorrorCorridor";
        private const string InteriorMaterials = "Assets/EcoElectry/Content/Models/Level01Interior";
        private const string ProfilePath = "Assets/EcoElectry/Content/Rendering/HorrorCorridorProfile.asset";

        private static readonly Vector3 WorldOrigin = new Vector3(220f, 14.15f, 3465f);
        private const float YFloor1 = 0f;
        private const float YLandingMid = 2.20f;
        private const float YFloor2 = 4.40f;
        private const float StoryHeight = 4.15f;

        private Transform root;
        private Transform stairwellGroup;
        private Transform atriumGalleryGroup;
        private Transform westWingGroup;
        private Transform eastWingGroup;
        private Transform northBridgeGroup;
        private Transform serviceShortcutGroup;
        private Transform classroomsAndLabsGroup;
        private Transform adminSuiteGroup;
        private Transform audiovisualGroup;
        private Transform electricalRoomGroup;
        private Transform restroomsGroup;
        private Transform ventilationGroup;
        private Transform exteriorViewsGroup;
        private Transform markersGroup;

        private Material matPlaster;
        private Material matDado;
        private Material matTrim;
        private Material matIron;
        private Material matCork;
        private Material matPaper;
        private Material matFloor;
        private Material matCeiling;
        private Material matFluorescent;
        private Material matWayfinding;
        private Material matEmergencyLens;
        private Material matWood;
        private Material matTileFloor;
        private Material matCeramic;
        private Material matLaminate;
        private Material matRubber;
        private Material matBookBlue;
        private Material matBookBrick;
        private Material matBookGreen;
        private Material matDoorGlass;
        private Material matDuctMetal;

        [MenuItem("Tools/EcoElectry/Niveles/Abrir Nivel 02 (Segundo piso)")]
        public static void OpenOrCreate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                return;
            }

            BuildAndSaveScene();
        }

        [MenuItem("Tools/EcoElectry/Niveles/Reconstruir Nivel 02 (Segundo piso)")]
        public static void RebuildMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAndSaveScene();
        }

        public static void BuildAndSaveScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var builder = new Level02Builder();
            builder.BuildAll();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("No se pudo guardar la escena Level02 en " + ScenePath);
            AssetDatabase.SaveAssets();
        }

        private void BuildAll()
        {
            LoadMaterials();
            ConfigureRenderSettings();

            var rootGo = new GameObject("Level02");
            root = rootGo.transform;
            root.position = WorldOrigin;

            stairwellGroup = CreateZone("Stairs");
            atriumGalleryGroup = CreateZone("AtriumGallery");
            westWingGroup = CreateZone("WestWing");
            eastWingGroup = CreateZone("EastWing");
            northBridgeGroup = CreateZone("NorthServiceBridge");
            serviceShortcutGroup = CreateZone("ServiceShortcut");
            classroomsAndLabsGroup = CreateZone("ScienceAndClassrooms");
            adminSuiteGroup = CreateZone("AdminAndComputerWing");
            audiovisualGroup = CreateZone("AudiovisualRoom");
            electricalRoomGroup = CreateZone("ElectricalRoom");
            restroomsGroup = CreateZone("Restrooms");
            ventilationGroup = CreateZone("Ventilation");
            exteriorViewsGroup = CreateZone("ExteriorViews");
            markersGroup = CreateZone("Markers");

            BuildStairwell();
            BuildFloor2Shell();
            BuildRoomsAndProps();
            BuildVentilationSystem();
            BuildExteriorCourtyardAndMoonlight();
            BuildMarkers();
            CreateExplorer();
        }

        private Transform CreateZone(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            return go.transform;
        }

        private void LoadMaterials()
        {
            matPlaster = LoadMat(CorridorMaterials + "/Plaster.mat");
            matDado = LoadMat(CorridorMaterials + "/Dado.mat");
            matTrim = LoadMat(CorridorMaterials + "/Trim.mat");
            matIron = LoadMat(CorridorMaterials + "/Iron.mat");
            matCork = LoadMat(CorridorMaterials + "/Cork.mat");
            matPaper = LoadMat(CorridorMaterials + "/Paper.mat");
            matFloor = LoadMat(CorridorMaterials + "/Floor.mat");
            matCeiling = LoadMat(CorridorMaterials + "/Ceiling.mat");
            matFluorescent = LoadMat(CorridorMaterials + "/Fluorescent.mat");
            matWayfinding = LoadMat(CorridorMaterials + "/Wayfinding.mat");
            matEmergencyLens = LoadMat(CorridorMaterials + "/EmergencyLens.mat");

            matWood = LoadMat(InteriorMaterials + "/Wood.mat");
            matTileFloor = LoadMat(InteriorMaterials + "/TileFloor.mat");
            matCeramic = LoadMat(InteriorMaterials + "/Ceramic.mat");
            matLaminate = LoadMat(InteriorMaterials + "/Laminate.mat");
            matRubber = LoadMat(InteriorMaterials + "/Rubber.mat");
            matBookBlue = LoadMat(InteriorMaterials + "/BookBlue.mat");
            matBookBrick = LoadMat(InteriorMaterials + "/BookBrick.mat");
            matBookGreen = LoadMat(InteriorMaterials + "/BookGreen.mat");

            matDoorGlass = LoadMat("Assets/ResourcePack/Ventana/Materials/M_VentanaFija_VidrioAzulClaro.mat");
            matDuctMetal = LoadMat("Assets/ResourcePack/Canaleta/Materials/M_Metal_Galvanizado_CanaletaBajoPiso.mat");
        }

        private static Material LoadMat(string path)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) throw new InvalidOperationException("Material requerido no encontrado: " + path);
            return mat;
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.sun = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.192f, 0.220f, 0.262f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.090f, 0.114f, 0.148f, 1f);
            RenderSettings.fogDensity = 0.020f;
        }

        private void CreateExplorer()
        {
            var player = new GameObject("Explorador");
            player.transform.SetParent(root, false);
            player.transform.localPosition = new Vector3(0f, YFloor1 + 0.025f, 22.10f);
            player.transform.localRotation = Quaternion.identity;

            var body = player.AddComponent<CharacterController>();
            body.height = 1.85f;
            body.center = Vector3.up * 0.925f;
            body.radius = 0.28f;
            body.slopeLimit = 45f;
            body.stepOffset = 0.22f;
            body.skinWidth = 0.025f;
            body.minMoveDistance = 0f;

            var head = new GameObject("Camara");
            head.tag = "MainCamera";
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = Vector3.up * 1.65f;

            var camera = head.AddComponent<Camera>();
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 110f;
            camera.fieldOfView = 67f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.07f, 1f);
            head.AddComponent<AudioListener>();

            var lamp = new GameObject("Linterna");
            lamp.transform.SetParent(head.transform, false);
            lamp.transform.localPosition = new Vector3(0.10f, -0.12f, 0.02f);
            var light = lamp.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 11.5f;
            light.innerSpotAngle = 28f;
            light.spotAngle = 64f;
            light.color = new Color(1f, 0.86f, 0.68f, 1f);
            light.intensity = 7.4f;
            light.shadows = LightShadows.Soft;
            light.shadowNearPlane = 0.08f;
            light.shadowBias = 0.025f;
            light.shadowNormalBias = 0.08f;

            var controls = AssetDatabase.LoadMainAssetAtPath(InputPath);
            if (controls == null) throw new InvalidOperationException("Faltan las acciones del jugador en " + InputPath);

            var explorer = new SerializedObject(player.AddComponent<HorrorCorridorExplorer>());
            explorer.FindProperty("controls").objectReferenceValue = controls;
            explorer.FindProperty("view").objectReferenceValue = camera;
            explorer.FindProperty("flashlight").objectReferenceValue = light;
            explorer.FindProperty("flashlightFlickerDepth").floatValue = 0f;
            explorer.FindProperty("brightnessPreferenceKey").stringValue = "EcoElectry.Level02.Brightness";
            explorer.FindProperty("grainPreferenceKey").stringValue = "EcoElectry.Level02.FilmGrain";
            explorer.ApplyModifiedPropertiesWithoutUndo();

            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            HorrorCorridorImage.Build(camera, profile);
            var glitch = head.GetComponent<CorridorSignalGlitch>();
            if (glitch != null)
            {
                var glitchSo = new SerializedObject(glitch);
                glitchSo.FindProperty("displacement").floatValue = 0f;
                glitchSo.FindProperty("flickerDepth").floatValue = 0f;
                glitchSo.ApplyModifiedPropertiesWithoutUndo();
            }

            Type doorInteractorType = Type.GetType("EcoElectry.Unity.Interaction.SchoolDoorInteractor, Assembly-CSharp");
            if (doorInteractorType != null)
            {
                Component doorInteractor = player.AddComponent(doorInteractorType);
                var doorSo = new SerializedObject(doorInteractor);
                doorSo.FindProperty("view").objectReferenceValue = camera;
                doorSo.FindProperty("reach").floatValue = 2.25f;
                doorSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void BuildMarkers()
        {
            CreateMarker(markersGroup, "PlayerSpawn", new Vector3(0f, YFloor1 + 0.025f, 22.10f), 0f);
            CreateMarker(markersGroup, "Level01Connection", new Vector3(0f, YFloor1, 20.85f), 180f);
            CreateMarker(markersGroup, "UpperFloorLanding", new Vector3(3.44f, YFloor2, 24.50f), 180f);
            CreateMarker(markersGroup, "Level03Connection", new Vector3(-3.46f, YFloor2, 23.20f), 0f);
            CreateMarker(markersGroup, "ServiceShortcut", new Vector3(6.68f, YFloor2, 27.60f), 0f);

            Transform repairs = Group("RepairCandidates", markersGroup);
            CreateMarker(repairs, "Repair_ElectricalRoomMainCabinet", new Vector3(6.20f, YFloor2 + 1.20f, 41.20f), 0f);
            CreateMarker(repairs, "Repair_ComputerLabUPS", new Vector3(20.90f, YFloor2 + 1.25f, 38.00f), 90f);
            CreateMarker(repairs, "Repair_ScienceLabBreaker", new Vector3(-14.30f, YFloor2 + 1.35f, 32.80f), 90f);
            CreateMarker(repairs, "Repair_AudiovisualBooth", new Vector3(0.95f, YFloor2 + 1.10f, 40.90f), 0f);
            CreateMarker(repairs, "Repair_UpperLandingSubpanel", new Vector3(5.85f, YFloor2 + 1.35f, 22.58f), 0f);

            Transform parts = Group("PartsSearchCandidates", markersGroup);
            CreateMarker(parts, "Parts_SciencePrepBench", new Vector3(-19.80f, YFloor2 + 0.90f, 38.30f), 0f);
            CreateMarker(parts, "Parts_PrincipalOfficeCabinet", new Vector3(21.60f, YFloor2 + 1.00f, 29.80f), 90f);
            CreateMarker(parts, "Parts_ArchiveShelf", new Vector3(9.40f, YFloor2 + 1.15f, 25.20f), 180f);
            CreateMarker(parts, "Parts_StorageCloset2F", new Vector3(-3.46f, YFloor2 + 0.95f, 30.10f), 180f);

            Transform appearances = Group("AppearanceCandidates", markersGroup);
            CreateMarker(appearances, "Appearance_StairwellMidLanding", new Vector3(3.44f, YLandingMid, 31.40f), 180f);
            CreateMarker(appearances, "Appearance_CourtyardGalleryWest", new Vector3(-11.80f, YFloor2, 21.10f), 90f);
            CreateMarker(appearances, "Appearance_LightwellWindow", new Vector3(-7.90f, YFloor2, 30.70f), 180f);
            CreateMarker(appearances, "Appearance_AudiovisualProjectionWindow", new Vector3(-0.90f, YFloor2, 39.40f), 270f);
            CreateMarker(appearances, "Appearance_ComputerLabBackRow", new Vector3(19.80f, YFloor2, 37.90f), 225f);
        }

        private static void CreateMarker(Transform parent, string name, Vector3 localPos, float yaw)
        {
            var m = new GameObject(name).transform;
            m.SetParent(parent, false);
            m.localPosition = localPos;
            m.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
