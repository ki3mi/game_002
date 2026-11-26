using System;
using System.Linq;
using EcoElectry.Unity.Levels;
using EcoElectry.Unity.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string CabRigName = "Conducción";
        private const string DriveTruckName = "Camioneta eléctrica";
        private const string TruckPrefabPath = ThirdPartyRoot + "/Ezereal/ElectricTruck/Prefabs/Electric Truck.prefab";
        private const string PlayerInputPath = "Assets/ResourcePack/InputSystem_Actions.inputactions";
        private const string CabinCameraName = "Vista de cabina";
        private const string WalkingCameraName = "Vista a pie";
        private const string ArrivalZoneName = "Zona de llegada al colegio";
        private const string DriverExitName = "Salida por la puerta del conductor";

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar conducción nocturna")]
        internal static void RefreshNightIntroduction()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para montar la conducción.");
            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            if (level == null) throw new InvalidOperationException("No se encontró el mapa del nivel 00.");

            SceneManager.SetActiveScene(scene);
            ConfigureNightIntroduction(level.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudo guardar la conducción nocturna.");
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureNightIntroduction(Transform level)
        {
            var controls = AssetDatabase.LoadMainAssetAtPath(PlayerInputPath);
            if (controls == null) throw new InvalidOperationException("Falta el recurso de controles del nivel 00.");

            Transform start = RequiredChild(level, "Markers/VehicleStart");
            Transform parking = RequiredChild(level, "Markers/VehicleParking");
            Transform fallback = RequiredChild(level, "Markers/OnFootExit");
            Transform entrance = RequiredChild(level, "Markers/Level1Door");
            // Keep both axles on the asphalt when migrating the old marker at the end of the road.
            if (Mathf.Abs(start.position.x) < 1f && Mathf.Abs(start.position.z) < 1f)
                start.position += start.forward * 5.5f;

            VolumeProfile profile = GetIntroductionNightProfile();
            Transform rig = level.Find(CabRigName) ?? Group(level.gameObject.scene, level, CabRigName);
            Transform truck = GetDriveTruck(level, rig, start);
            IntroductionTruckDrive drive = ConfigureTruck(truck, start, controls);
            Camera cockpit = BuildCabinView(truck, profile);

            RemoveChild(truck, DriverExitName);
            Transform driverExit = Group(truck.gameObject.scene, truck, DriverExitName, new Vector3(-1.54f, 0f, 0.38f));
            driverExit.gameObject.isStatic = false;
            CharacterController explorer = BuildWalkingPlayer(level, fallback, entrance, controls, profile);
            ConfigureIntroductionNightLights(level);
            OrientApproachSigns(level);
            OrientCampusSignText(level.Find("SchoolExterior/RotuloColegio"));
            KeepLargeFacadeMeshesUnbatched(level.Find("SchoolExterior/BuildingA"));

            ChunkRenderWindow chunks = level.GetComponent<ChunkRenderWindow>();
            if (chunks == null)
            {
                ConfigureChunkRendering(level);
                chunks = level.GetComponent<ChunkRenderWindow>();
            }
            var chunkSetup = new SerializedObject(chunks);
            chunkSetup.FindProperty("player").objectReferenceValue = truck;
            chunkSetup.ApplyModifiedPropertiesWithoutUndo();
            BuildArrivalZone(rig, drive, cockpit, explorer, driverExit, fallback, parking, entrance, chunks);
        }

        private static Transform GetDriveTruck(Transform level, Transform rig, Transform start)
        {
            Transform truck = rig.Find(DriveTruckName) ??
                              level.Find("Highway/Estacionamiento/ElectricTruck_Reference_Scale1");
            if (truck == null)
                return PlacePrefab(LoadPrefab(TruckPrefabPath), rig, start.position, start.eulerAngles.y,
                    Vector3.one, DriveTruckName).transform;

            truck.SetParent(rig, true);
            truck.name = DriveTruckName;
            return truck;
        }

        private static IntroductionTruckDrive ConfigureTruck(Transform truck, Transform start, Object controls)
        {
            foreach (Transform node in truck.GetComponentsInChildren<Transform>(true))
            {
                if (!node.gameObject.isStatic) continue;
                node.gameObject.isStatic = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(node.gameObject);
            }
            foreach (MeshRenderer renderer in truck.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.name.Contains("Wheel Placeholder", StringComparison.Ordinal) &&
                    renderer.name != "Interior Steering Wheel Placeholder")
                    continue;
                renderer.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }

            Rigidbody body = truck.GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.angularDamping = 0.72f;
            body.linearDamping = 0.035f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);

            var wheelByName = truck.GetComponentsInChildren<WheelCollider>(true).ToDictionary(w => w.name);
            var nodeByName = truck.GetComponentsInChildren<Transform>(true).ToLookup(t => t.name);
            string[] names = { "Front Left", "Front Right", "Rear Left", "Rear Right" };
            var wheels = new WheelCollider[names.Length];
            var drive = truck.GetComponent<IntroductionTruckDrive>() ?? truck.gameObject.AddComponent<IntroductionTruckDrive>();
            var data = new SerializedObject(drive);
            data.FindProperty("controls").objectReferenceValue = controls;
            data.FindProperty("steeringWheel").objectReferenceValue = nodeByName["Interior Steering Wheel"].SingleOrDefault();
            SerializedProperty configuration = data.FindProperty("wheels");
            configuration.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (!wheelByName.TryGetValue(name + " Wheel Collider", out WheelCollider wheel))
                    throw new InvalidOperationException("Falta la rueda " + name + " de la camioneta.");
                Transform visual = nodeByName["Wheel Mesh " + name].SingleOrDefault();
                if (visual == null) throw new InvalidOperationException("Falta la malla de rueda " + name + ".");

                wheels[i] = wheel;
                wheel.mass = 42f;
                wheel.wheelDampingRate = 0.85f;
                wheel.forceAppPointDistance = 0.16f;
                PrefabUtility.RecordPrefabInstancePropertyModifications(wheel);
                SerializedProperty entry = configuration.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("collider").objectReferenceValue = wheel;
                entry.FindPropertyRelative("visual").objectReferenceValue = visual;
                entry.FindPropertyRelative("steers").boolValue = i < 2;
            }

            SeatTruckOnRoad(truck, body, wheels, start);
            Light[] headlights = ConfigureHeadlights(truck);
            Renderer[] lenses = truck.GetComponentsInChildren<Renderer>(true)
                .Where(r => r.name.StartsWith("Headlight Low Beam ", StringComparison.Ordinal)).ToArray();
            Material originalLens = AssetDatabase.LoadAssetAtPath<Material>(
                ThirdPartyRoot + "/Ezereal/ElectricTruck/Materials/Lights/White Light.mat");
            if (originalLens == null) throw new InvalidOperationException("Falta el material original de los faros.");
            Material glow = IntroductionGlow(originalLens, "M_Level00_FaroEncendido", new Color(1.6f, 1.72f, 1.93f));
            Material unlit = IntroductionGlow(originalLens, "M_Level00_FaroApagado", Color.black);
            unlit.DisableKeyword("_EMISSION");
            foreach (Renderer lens in lenses)
            {
                lens.sharedMaterial = glow;
                PrefabUtility.RecordPrefabInstancePropertyModifications(lens);
            }
            foreach (Renderer lens in truck.GetComponentsInChildren<Renderer>(true))
            {
                if (!lens.name.StartsWith("Headlight High Beam ", StringComparison.Ordinal)) continue;
                lens.sharedMaterial = unlit;
                PrefabUtility.RecordPrefabInstancePropertyModifications(lens);
            }
            SetObjectArray(data.FindProperty("lowBeams"), headlights);
            SetObjectArray(data.FindProperty("headlightLenses"), lenses);
            data.FindProperty("unlitLens").objectReferenceValue = unlit;
            data.ApplyModifiedPropertiesWithoutUndo();
            return drive;
        }

        private static void SeatTruckOnRoad(Transform truck, Rigidbody body, WheelCollider[] wheels, Transform start)
        {
            Physics.SyncTransforms();
            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(start.forward, Vector3.up));
            float height = 0f;
            foreach (WheelCollider wheel in wheels)
            {
                Vector3 center = truck.InverseTransformPoint(wheel.transform.TransformPoint(wheel.center));
                Vector3 above = start.position + rotation * center + Vector3.up * 3f;
                RaycastHit[] ground = Physics.RaycastAll(above, Vector3.down, 6f, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
                float surface = float.NegativeInfinity;
                foreach (RaycastHit hit in ground)
                    if (hit.collider.gameObject.scene == truck.gameObject.scene &&
                        !hit.collider.transform.IsChildOf(truck) && hit.normal.y > 0.8f)
                        surface = Mathf.Max(surface, hit.point.y);
                if (float.IsNegativeInfinity(surface))
                    throw new InvalidOperationException("El anclaje VehicleStart debe tener calzada debajo de las cuatro ruedas.");

                JointSpring spring = wheel.suspensionSpring;
                float compression = body.mass * Mathf.Abs(Physics.gravity.y) / (wheels.Length * spring.spring);
                float extension = Mathf.Clamp(wheel.suspensionDistance * (1f - spring.targetPosition) - compression,
                    0f, wheel.suspensionDistance);
                height += surface - center.y + extension + wheel.radius;
            }
            Vector3 position = start.position;
            position.y = height / wheels.Length + 0.025f;
            truck.SetPositionAndRotation(position, rotation);
            PrefabUtility.RecordPrefabInstancePropertyModifications(truck);
        }

        private static Light[] ConfigureHeadlights(Transform truck)
        {
            var lights = truck.GetComponentsInChildren<Light>(true)
                .Where(l => l.name.Contains("Beam Spot Light", StringComparison.Ordinal)).ToArray();
            Light[] lowBeams = lights.Where(l => l.name.Contains("Low Beam", StringComparison.Ordinal)).ToArray();
            if (lowBeams.Length != 2) throw new InvalidOperationException("Se esperaban los dos focos bajos del prefab.");

            foreach (Light light in lights)
            {
                bool low = Array.IndexOf(lowBeams, light) >= 0;
                light.gameObject.SetActive(low);
                light.enabled = low;
                if (low)
                {
                    float side = Mathf.Sign(truck.InverseTransformPoint(light.transform.position).x);
                    light.transform.SetPositionAndRotation(truck.TransformPoint(0.568f * side, 0.905f, 2.40f),
                        truck.rotation * Quaternion.Euler(2.1f, side * 0.65f, 0f));
                    TuneIntroductionSpot(light, new Color32(224, 236, 255, 255), 108f, 113f, 32f, 65f, true, 1);
                    light.shadowNearPlane = 0.22f;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(light.transform);
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(light);
                PrefabUtility.RecordPrefabInstancePropertyModifications(light.gameObject);
            }
            return lowBeams;
        }

        private static Camera BuildCabinView(Transform truck, VolumeProfile profile)
        {
            RemoveChild(truck, CabinCameraName);
            Camera camera = CreateIntroductionCamera(truck, CabinCameraName, new Vector3(-0.455f, 1.55f, -0.12f), 68f);
            camera.transform.localRotation = Quaternion.Euler(3.5f, 0f, 0f);
            HorrorCorridorImage.AddVolume(camera, profile);
            return camera;
        }

        private static CharacterController BuildWalkingPlayer(Transform level, Transform fallback, Transform entrance,
            Object controls, VolumeProfile profile)
        {
            Transform player = level.Find("Player") ?? Group(level.gameObject.scene, level, "Player");
            player.gameObject.SetActive(false);
            player.gameObject.isStatic = false;
            var previous = player.GetComponent<ThirdPersonExplorer>();
            if (previous != null) Object.DestroyImmediate(previous);
            RemoveChild(player, "PersonalMantenimiento");
            RemoveChild(player, "Cámara de exploración");
            RemoveChild(player, WalkingCameraName);
            player.SetPositionAndRotation(fallback.position, Quaternion.LookRotation(FlatDirection(fallback.position, entrance.position)));
            var capsule = player.GetComponent<CharacterController>() ?? player.gameObject.AddComponent<CharacterController>();
            capsule.height = 1.85f;
            capsule.center = Vector3.up * 0.925f;
            capsule.radius = 0.28f;
            capsule.skinWidth = 0.025f;
            capsule.stepOffset = 0.24f;
            capsule.slopeLimit = 45f;
            capsule.minMoveDistance = 0f;

            Camera camera = CreateIntroductionCamera(player, WalkingCameraName, Vector3.up * 1.65f, 67f);
            HorrorCorridorImage.Build(camera, profile);
            var signal = new SerializedObject(camera.GetComponent<CorridorSignalGlitch>());
            signal.FindProperty("displacement").floatValue = 0.0025f;
            signal.FindProperty("flickerDepth").floatValue = 0.20f;
            signal.ApplyModifiedPropertiesWithoutUndo();

            Transform lamp = Group(player.gameObject.scene, camera.transform, "Linterna", new Vector3(0.1f, -0.12f, 0.02f));
            lamp.gameObject.isStatic = false;
            Light flashlight = lamp.gameObject.AddComponent<Light>();
            TuneIntroductionSpot(flashlight, new Color32(255, 226, 188, 255), 22f, 23f, 29f, 62f, true, 1);
            flashlight.enabled = false;
            HorrorCorridorExplorer explorer = player.GetComponent<HorrorCorridorExplorer>() ??
                                              player.gameObject.AddComponent<HorrorCorridorExplorer>();
            var data = new SerializedObject(explorer);
            data.FindProperty("controls").objectReferenceValue = controls;
            data.FindProperty("view").objectReferenceValue = camera;
            data.FindProperty("flashlight").objectReferenceValue = flashlight;
            data.FindProperty("flashlightStartsOn").boolValue = false;
            data.FindProperty("flashlightFlickerDepth").floatValue = 0.36f;
            data.FindProperty("walkSpeed").floatValue = 2.25f;
            data.FindProperty("brightnessPreferenceKey").stringValue = "EcoElectry.Introduction.Night.Brightness";
            data.FindProperty("grainPreferenceKey").stringValue = "EcoElectry.Introduction.Night.Grain";
            data.ApplyModifiedPropertiesWithoutUndo();
            return capsule;
        }

        private static Camera CreateIntroductionCamera(Transform parent, string name, Vector3 localPosition, float fov)
        {
            var go = new GameObject(name) { tag = "MainCamera" };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = fov;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 675f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = IntroductionNightFog;
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderShadows = true;
            go.AddComponent<AudioListener>();
            return camera;
        }

        private static void BuildArrivalZone(Transform rig, IntroductionTruckDrive vehicle, Camera cockpit,
            CharacterController explorer, Transform driverExit, Transform fallback, Transform parking, Transform entrance,
            ChunkRenderWindow chunks)
        {
            RemoveChild(rig, ArrivalZoneName);
            Transform zone = Group(rig.gameObject.scene, rig, ArrivalZoneName);
            zone.SetPositionAndRotation(parking.position, parking.rotation);
            BoxCollider trigger = zone.gameObject.AddComponent<BoxCollider>();
            trigger.center = new Vector3(0f, 1.25f, 0f);
            trigger.size = new Vector3(22f, 3.4f, 27f);
            trigger.isTrigger = true;

            var data = new SerializedObject(zone.gameObject.AddComponent<VehicleArrivalTransition>());
            data.FindProperty("vehicle").objectReferenceValue = vehicle;
            data.FindProperty("vehicleBody").objectReferenceValue = vehicle.GetComponent<Rigidbody>();
            data.FindProperty("cockpitView").objectReferenceValue = cockpit.gameObject;
            data.FindProperty("explorer").objectReferenceValue = explorer;
            data.FindProperty("driverExit").objectReferenceValue = driverExit;
            data.FindProperty("fallbackExit").objectReferenceValue = fallback;
            data.FindProperty("parking").objectReferenceValue = parking;
            data.FindProperty("schoolEntrance").objectReferenceValue = entrance;
            data.FindProperty("chunks").objectReferenceValue = chunks;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void OrientApproachSigns(Transform level)
        {
            Transform signs = level.Find("Highway");
            if (signs == null) return;
            foreach (Transform sign in signs)
                if (sign.name.StartsWith("Senal_", StringComparison.Ordinal) && sign.forward.z < 0f)
                    sign.Rotate(0f, 180f, 0f, Space.World);
        }

        private static void KeepLargeFacadeMeshesUnbatched(Transform building)
        {
            if (building == null) return;
            foreach (MeshFilter filter in building.GetComponentsInChildren<MeshFilter>(true))
            {
                // Unity corrupts some submeshes of this imported facade when it combines them at runtime.
                if (filter.sharedMesh == null || filter.sharedMesh.vertexCount <= ushort.MaxValue) continue;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(filter.gameObject);
                if ((flags & StaticEditorFlags.BatchingStatic) == 0) continue;
                GameObjectUtility.SetStaticEditorFlags(filter.gameObject, flags & ~StaticEditorFlags.BatchingStatic);
                PrefabUtility.RecordPrefabInstancePropertyModifications(filter.gameObject);
            }
        }

        private static Transform RequiredChild(Transform parent, string path) =>
            parent.Find(path) ?? throw new InvalidOperationException("No se encontró " + path + " en el nivel 00.");

        private static void RemoveChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        private static void SetObjectArray<T>(SerializedProperty property, T[] values) where T : Object
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
