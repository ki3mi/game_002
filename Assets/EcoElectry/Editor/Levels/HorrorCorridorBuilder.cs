using System;
using EcoElectry.Unity.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    internal static class HorrorCorridorBuilder
    {
        private const string ScenePath = "Assets/EcoElectry/Content/Scenes/HorrorCorridorPrototype.unity";
        private const string InputPath = "Assets/ResourcePack/InputSystem_Actions.inputactions";
        private const string MusicPath = "Assets/Music/music_corridor.mp3";
        private const string IntroductionPath = "Assets/EcoElectry/Content/Scenes/Level00-Introduction.unity";
        private const string AnomalyPath = "Assets/EcoElectry/Content/Prefabs/Anomalias/Anomalia01_Filamentos.prefab";
        private const string StingPath = "Assets/EcoElectry/Content/Audio/HorrorPrologueSting.wav";

        [MenuItem("Tools/EcoElectry/Prototipos/Abrir pasillos de noche")]
        public static void OpenOrCreate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }

            BuildScene();
        }

        private static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            HorrorCorridorEnvironment.Build();
            Camera camera = CreateExplorer();
            HorrorCorridorImage.Build(camera);
            AddMusic(camera);
            AddPrologue(camera);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("No se pudo guardar el prototipo de pasillos.");
        }

        private static void AddPrologue(Camera camera)
        {
            var prologue = new SerializedObject(camera.transform.parent.gameObject.AddComponent<HorrorPrologue>());
            prologue.FindProperty("view").objectReferenceValue = camera;
            prologue.FindProperty("anomalyPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(AnomalyPath);
            prologue.FindProperty("sting").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(StingPath);
            prologue.FindProperty("introductionScene").stringValue = IntroductionPath;
            prologue.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void AddMusic(Camera camera)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
            if (clip == null)
            {
                Debug.LogWarning("No se encontró la música de los pasillos en " + MusicPath);
                return;
            }

            var source = camera.gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = true;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0.32f;
        }

        private static Camera CreateExplorer()
        {
            var player = new GameObject("Explorador");
            player.transform.position = new Vector3(0f, 0.03f, 0f);
            var body = player.AddComponent<CharacterController>();
            body.height = 1.85f;
            body.center = Vector3.up * 0.925f;
            body.radius = 0.28f;
            body.skinWidth = 0.025f;
            body.stepOffset = 0.16f;
            body.minMoveDistance = 0f;

            var head = new GameObject("Camara");
            head.tag = "MainCamera";
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = Vector3.up * 1.65f;
            var camera = head.AddComponent<Camera>();
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 55f;
            camera.fieldOfView = 67f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
            head.AddComponent<AudioListener>();

            var lamp = new GameObject("Linterna");
            lamp.transform.SetParent(head.transform, false);
            lamp.transform.localPosition = new Vector3(0.10f, -0.12f, 0.02f);
            var light = lamp.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 11.5f;
            light.innerSpotAngle = 28f;
            light.spotAngle = 64f;
            light.color = new Color(1f, 0.86f, 0.68f);
            light.intensity = 7.4f;
            light.shadows = LightShadows.Soft;
            light.shadowNearPlane = 0.08f;
            light.shadowBias = 0.025f;
            light.shadowNormalBias = 0.08f;

            var explorer = new SerializedObject(player.AddComponent<HorrorCorridorExplorer>());
            var controls = AssetDatabase.LoadMainAssetAtPath(InputPath);
            if (controls == null) throw new InvalidOperationException("Faltan las acciones del jugador en " + InputPath);
            explorer.FindProperty("controls").objectReferenceValue = controls;
            explorer.FindProperty("view").objectReferenceValue = camera;
            explorer.FindProperty("flashlight").objectReferenceValue = light;
            explorer.ApplyModifiedPropertiesWithoutUndo();
            return camera;
        }
    }
}
