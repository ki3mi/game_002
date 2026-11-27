using System;
using EcoElectry.Unity.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string IntroductionRenderingFolder = "Assets/EcoElectry/Content/Rendering";
        private const string NightProfilePath = IntroductionRenderingFolder + "/Level00NightProfile.asset";
        private const string SkyFolder = "Assets/EcoElectry/AllSkyFree/";
        private const string ApproachLightsName = "Farolas de acceso";
        private const float FourthApproachLightZ = 3347f;
        private static readonly string[] IntroductionSkyMaterials =
        {
            "Cold Sunset/Cold Sunset Equirect.mat",
            "Deep Dusk/Deep Dusk Equirect.mat",
            "Cold Night/Cold Night Equirect.mat"
        };
        private static readonly Color IntroductionNightFog = new Color32(16, 24, 39, 255);

        private static VolumeProfile GetIntroductionNightProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(NightProfilePath);
            if (profile != null) return profile;
            if (!AssetDatabase.CopyAsset(IntroductionRenderingFolder + "/HorrorCorridorProfile.asset", NightProfilePath))
                throw new InvalidOperationException("No se pudo copiar el perfil visual de pasillos.");

            profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(NightProfilePath);
            if (profile.TryGet(out FilmGrain grain))
            {
                grain.intensity.Override(0.22f);
                EditorUtility.SetDirty(grain);
            }
            if (profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.Override(0.20f);
                EditorUtility.SetDirty(vignette);
            }
            if (profile.TryGet(out ColorAdjustments colors))
            {
                colors.saturation.Override(-21f);
                colors.postExposure.Override(0.32f);
                EditorUtility.SetDirty(colors);
            }
            return profile;
        }

        private static Transform ConfigureIntroductionNightLights(Transform level, PathData road)
        {
            Transform lighting = level.Find("Lighting") ?? Group(level.gameObject.scene, level, "Lighting");
            Transform moon = lighting.Find("Luna") ?? lighting.Find("Directional Light") ??
                             Group(level.gameObject.scene, lighting, "Luna");
            moon.name = "Luna";
            moon.rotation = IntroductionSkyCycle.InitialMoonRotation;
            Light moonlight = moon.GetComponent<Light>() ?? moon.gameObject.AddComponent<Light>();
            moonlight.type = LightType.Directional;
            moonlight.color = new Color32(153, 185, 238, 255);
            moonlight.intensity = 0.23f;
            moonlight.shadows = LightShadows.Soft;
            moonlight.shadowStrength = 0.79f;

            RenderSettings.sun = moonlight;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color32(35, 45, 64, 255);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.27f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = IntroductionNightFog;
            RenderSettings.fogDensity = 0.006f;

            RemoveChild(lighting, "Luces del colegio");
            Transform fixtures = Group(level.gameObject.scene, lighting, "Luces del colegio");
            Transform approach = BuildApproachLights(level, road);
            foreach (Renderer diffuser in level.GetComponentsInChildren<Renderer>())
            {
                if (diffuser.transform.IsChildOf(approach)) continue;
                if (diffuser.name == "PosteLuz_Difusor") AddParkingLamp(fixtures, diffuser);
                else if (diffuser.name == "LuminariaSolarPoste_Difusor") AddWalkwayLamp(fixtures, diffuser);
            }
            AddGuardLight(level, fixtures);
            AddEntranceLight(level, fixtures);
            return moon;
        }

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar farolas de acceso")]
        internal static void RefreshApproachLights()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || scene.isDirty || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 guardado en modo edición para colocar las farolas.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            if (level == null) throw new InvalidOperationException("No se encontró el mapa del nivel 00.");

            BuildApproachLights(level.transform, BuildPath(RoadControls, 4f));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudieron guardar las farolas de acceso.");
            AssetDatabase.SaveAssets();
        }

        private static Transform BuildApproachLights(Transform level, PathData road)
        {
            var terrain = TerrainGrid.FromRoot(RequiredChild(level, "Terrain"));
            Transform highway = RequiredChild(level, "Highway");
            GameObject prefab = LoadPrefab("Assets/ResourcePack/Poste/Prefabs/PosteLuz.prefab");
            RemoveChild(highway, ApproachLightsName);
            Transform fixtures = Group(level.gameObject.scene, highway, ApproachLightsName);

            var stations = new[] { 3250f, 3282f, 3315f, FourthApproachLightZ, 3376f };
            for (int i = 0; i < stations.Length; i++)
            {
                float z = stations[i];
                Vector3 center = RoadAtZ(road, z);
                Vector3 forward = FlatDirection(RoadAtZ(road, z - 1f), RoadAtZ(road, z + 1f));
                Vector3 outward = Vector3.Cross(Vector3.up, forward) * (i % 2 == 0 ? 1f : -1f);
                Vector3 position = center + outward * (BarrierOffset + 1.35f);
                position.y = GroundAt(terrain, position.x, position.z);
                PlaceApproachLamp(prefab, fixtures, "Farola de carretera " + (i + 1), position, center + forward * 3f);
            }

            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 gate = new Vector3(GateX, ParkingSurface, GateZ + 4f);
                PlaceApproachLamp(prefab, fixtures, "Farola del portón", gate + Vector3.right * (6f * side), gate);

                Vector3 entrance = new Vector3(SchoolX, CampusGrade + 0.105f, 3462f);
                PlaceApproachLamp(prefab, fixtures, "Farola de la entrada",
                    entrance + Vector3.right * (5f * side), entrance + Vector3.forward * 2f);
            }

            Vector3 parking = RequiredChild(level, "Markers/VehicleParking").position;
            PlaceApproachLamp(prefab, fixtures, "Farola de la plaza de llegada",
                parking + new Vector3(-7.5f, 0f, 6.5f), parking);
            return fixtures;
        }

        private static void PlaceApproachLamp(GameObject prefab, Transform parent, string name, Vector3 position,
            Vector3 target)
        {
            Vector3 inward = FlatDirection(position, target);
            float yaw = Quaternion.LookRotation(Vector3.Cross(inward, Vector3.up)).eulerAngles.y;
            GameObject pole = PlacePrefab(prefab, parent, position, yaw, Vector3.one, name);
            Renderer diffuser = Array.Find(pole.GetComponentsInChildren<Renderer>(), r => r.name == "PosteLuz_Difusor");
            if (diffuser == null) throw new InvalidOperationException("Al poste de luz le falta el difusor.");

            Vector3 emitter = diffuser.bounds.center + Vector3.down * 0.19f;
            Light light = AddIntroductionLight(pole.transform, "Luz de acceso", emitter, Quaternion.LookRotation(target - emitter));
            TuneIntroductionSpot(light, new Color32(255, 218, 167, 255), 190f, 28f, 68f, 108f, false, 0);
            SetDiffuserGlow(diffuser, "M_Level00_FarolaEncendida", new Color(2.2f, 1.65f, 1.05f));
        }

        private static IntroductionSkyCycle ConfigureIntroductionSkyCycle(Transform moon, Rigidbody truck, PathData road,
            Transform parking)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(IntroductionRenderingFolder + "/IntroductionSkyBlend.shader");
            var fogShader = AssetDatabase.LoadAssetAtPath<Shader>(IntroductionRenderingFolder + "/IntroductionDistanceFog.shader");
            var cubemaps = new Cubemap[IntroductionSkyMaterials.Length];
            for (int index = 0; index < cubemaps.Length; index++)
            {
                string path = SkyFolder + IntroductionSkyMaterials[index];
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                cubemaps[index] = material == null ? null : material.GetTexture("_Tex") as Cubemap;
                if (cubemaps[index] == null)
                    throw new InvalidOperationException("Falta el cubemap de " + path);
            }

            if (shader == null || fogShader == null)
                throw new InvalidOperationException("Falta el shader del cielo o de la niebla del nivel 00.");
            var route = new IntroductionSkyCycle.RouteSample[road.Points.Count + 2];
            float startDistance = 0f;
            Vector3 startCenter = RoadAtZ(road, VehicleStartZ);
            Vector3 gate = road.Points[^1];
            Vector3 bend = new Vector3(gate.x, parking.position.y, parking.position.z - 3f);
            for (int i = 0; i < route.Length; i++)
            {
                Vector3 point = i < road.Points.Count ? road.Points[i] : i == road.Points.Count ? bend : parking.position;
                float distance = 0f;
                if (i > 0)
                {
                    var previous = route[i - 1];
                    if (point.z <= previous.position.z)
                        throw new InvalidOperationException("El eje de la carretera debe avanzar hacia el estacionamiento.");
                    distance = previous.distance + Vector3.Distance(previous.position, point);
                    if (previous.position.z < VehicleStartZ && point.z >= VehicleStartZ)
                        startDistance = previous.distance + Vector3.Distance(previous.position, startCenter);
                }
                route[i] = new IntroductionSkyCycle.RouteSample(point, distance);
            }

            var cycle = moon.GetComponent<IntroductionSkyCycle>() ?? moon.gameObject.AddComponent<IntroductionSkyCycle>();
            cycle.Configure(shader, fogShader, cubemaps, truck, route, startDistance);
            EditorUtility.SetDirty(cycle);
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyFolder + IntroductionSkyMaterials[0]);
            return cycle;
        }

        private static void AddParkingLamp(Transform fixtures, Renderer diffuser)
        {
            var position = diffuser.bounds.center + Vector3.down * 0.19f;
            float inward = Mathf.Sign(216f - position.x);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(0.48f * inward, -1f, 0.14f));
            Light light = AddIntroductionLight(fixtures, "Farola del estacionamiento", position, rotation);
            TuneIntroductionSpot(light, new Color32(255, 222, 172, 255), 173f, 31f, 78f, 128f, true, 1);
            SetDiffuserGlow(diffuser, "M_Level00_FarolaEncendida", new Color(2.2f, 1.65f, 1.05f));
        }

        private static void AddWalkwayLamp(Transform fixtures, Renderer diffuser)
        {
            Vector3 position = diffuser.bounds.center + Vector3.down * 0.17f;
            Light light = AddIntroductionLight(fixtures, "Farola del sendero", position, Quaternion.Euler(90f, 0f, 0f));
            TuneIntroductionSpot(light, new Color32(203, 219, 247, 255), 32f, 13f, 82f, 128f, false, 0);
            SetDiffuserGlow(diffuser, "M_Level00_SenderoEncendido", new Color(1.4f, 1.8f, 2.1f));
        }

        private static void AddGuardLight(Transform level, Transform fixtures)
        {
            Transform booth = level.Find("SchoolExterior/CasetaVigilancia");
            if (booth == null) return;
            Material shade = CreateMaterial("M_Level00_CasetaDifusor", new Color32(224, 157, 89, 255),
                "Universal Render Pipeline/Lit", 0.20f);
            Material glowing = IntroductionGlow(shade, "M_Level00_CasetaEncendida", new Color(2f, 1.13f, 0.47f));
            Vector3 ceilingPosition = fixtures.InverseTransformPoint(booth.TransformPoint(0f, 2.65f, -0.92f));
            Block(fixtures, "Difusor interior de vigilancia", ceilingPosition, new Vector3(0.75f, 0.06f, 0.22f), glowing, false);

            Light interior = AddIntroductionLight(fixtures, "Luz cálida de vigilancia",
                booth.TransformPoint(0f, 2.34f, -1.25f), booth.rotation);
            interior.type = LightType.Point;
            interior.range = 9f;
            interior.intensity = 16f;
            interior.color = new Color32(255, 185, 111, 255);
            interior.shadows = LightShadows.Soft;
            interior.shadowBias = 0.042f;
            interior.shadowNormalBias = 0.095f;
            SetIntroductionShadowTier(interior, 0);

            Light apron = AddIntroductionLight(fixtures, "Luz de paso de vigilancia",
                booth.TransformPoint(0f, 2.91f, -2.35f), booth.rotation * Quaternion.Euler(74f, 180f, 0f));
            TuneIntroductionSpot(apron, new Color32(255, 202, 143, 255), 17f, 10f, 75f, 113f, false, 0);

            GameObject sideLamp = PlacePrefab(LoadPrefab("Assets/ResourcePack/LuminariaExterior/Prefabs/LuminariaSolarMural.prefab"),
                fixtures, booth.TransformPoint(2.52f, 2.48f, -0.62f), booth.eulerAngles.y + 90f,
                Vector3.one * 1.2f, "Aplique lateral de vigilancia");
            foreach (Renderer diffuser in sideLamp.GetComponentsInChildren<Renderer>())
                if (diffuser.name.Contains("Difusor", StringComparison.Ordinal))
                    SetDiffuserGlow(diffuser, "M_Level00_ApliqueVigilanciaEncendido", new Color(1.85f, 1.12f, 0.53f));
            Light side = AddIntroductionLight(fixtures, "Luz lateral de vigilancia",
                booth.TransformPoint(2.77f, 2.42f, -0.62f),
                Quaternion.LookRotation(booth.TransformDirection(new Vector3(0.58f, -1f, -0.16f))));
            TuneIntroductionSpot(side, new Color32(255, 207, 159, 255), 14f, 8.5f, 58f, 103f, false, 0);
        }

        private static void AddEntranceLight(Transform level, Transform fixtures)
        {
            Transform entrance = level.Find("SchoolExterior/EntradaPrincipal");
            if (entrance == null) return;
            Light light = AddIntroductionLight(fixtures, "Luz del acceso principal",
                entrance.TransformPoint(0f, 3.10f, -0.94f), entrance.rotation * Quaternion.Euler(81f, 180f, 0f));
            TuneIntroductionSpot(light, new Color32(227, 233, 249, 255), 38f, 14f, 75f, 116f, false, 0);
        }

        private static Light AddIntroductionLight(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            Transform node = Group(parent.gameObject.scene, parent, name);
            node.SetPositionAndRotation(position, rotation);
            return node.gameObject.AddComponent<Light>();
        }

        private static void TuneIntroductionSpot(Light light, Color color, float intensity, float range, float inner, float outer,
            bool shadows, int shadowTier)
        {
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = outer;
            light.innerSpotAngle = inner;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowBias = 0.04f;
            light.shadowNormalBias = 0.10f;
            if (shadows) SetIntroductionShadowTier(light, shadowTier);
        }

        private static void SetIntroductionShadowTier(Light light, int tier)
        {
            UniversalAdditionalLightData data = light.GetUniversalAdditionalLightData();
            data.enabled = true;
            var setup = new SerializedObject(data);
            setup.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue = tier;
            setup.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(data);
        }

        private static void SetDiffuserGlow(Renderer renderer, string name, Color color)
        {
            if (renderer.sharedMaterial == null) return;
            renderer.sharedMaterial = IntroductionGlow(renderer.sharedMaterial, name, color);
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }

        private static Material IntroductionGlow(Material source, string name, Color color)
        {
            EnsureFolder(MaterialsFolder);
            string path = MaterialsFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
