using System;
using System.Collections.Generic;
using System.Linq;
using EcoElectry.Unity.Levels;
using EcoElectry.Unity.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level01MapBuilder
    {
        private const string ScenePath = "Assets/EcoElectry/Content/Scenes/Level01-LaPrimeraAula.unity";
        private const string LevelPrefabs = "Assets/EcoElectry/Content/Prefabs/Level01";
        private const string CampusAssets = LevelPrefabs + "/Campus";
        private const float LeftFence = -18.92f;
        private const float RightFence = 30.51f;
        private const float BackFence = 36f;
        private const float GateX = -2.20f;
        private const float VehicleX = -8f;
        private static readonly Vector3 ClassroomOne = new Vector3(-11f, 0f, 24f);
        private static readonly Vector3 ClassroomTwo = new Vector3(4f, 0f, 25f);
        private static readonly Vector3 PowerRoom = new Vector3(19f, 0f, 26f);
        private static readonly Vector3 Roof = new Vector3(13.75f, 3.35f, 29.3f);
        private static readonly Vector3 ShadedSite = new Vector3(-14.10f, 0.022f, 11.15f);

        [MenuItem("Tools/EcoElectry/Nivel 01/Montar mapa inicial")]
        private static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("El mapa inicial se monta en modo edición.");

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            GameObject[] roots = scene.GetRootGameObjects();
            if (scene.isDirty || roots.Any(root => root.name == "School"))
                throw new InvalidOperationException("Se requiere la escena base guardada del nivel 01 para montar el mapa inicial.");

            LevelRoot level = roots.Select(root => root.GetComponent<LevelRoot>()).Single(component => component != null);
            if (level.Definition == null || level.Definition.CreateRules().Id != "level-01")
                throw new InvalidOperationException("La escena debe conservar la definición level-01.");

            SceneManager.SetActiveScene(scene);
            EnsureFolder(CampusAssets);
            var palette = new CampusPalette();
            GameObject courtPrefab = Level01CourtBuilder.CreatePrefab();
            GameObject schoolSign = CreateSchoolSign(palette);

            Terrain terrain = Level01TerrainBuilder.Create(Group(null, "Terrain"));
            BuildRoad(Group(null, "Road"));
            BuildForest(terrain, Group(null, "Forest"));
            Transform school = Group(null, "School");
            Transform lighting = Group(null, "Lighting");
            Transform markers = Group(null, "LevelMarkers");
            BuildEntrance(Group(school, "Entrance"), schoolSign);
            BuildCourtyard(Group(school, "Courtyard"), palette);
            BuildClassrooms(Group(school, "Classrooms"), markers, palette);
            Place(courtPrefab, Group(school, "BasketballCourt"), new Vector3(18f, 0f, 12f));
            BuildElectricalRoom(Group(school, "ElectricalRoom"), markers, palette);

            Marker(markers, "PanelPatio", ShadedSite);
            Marker(markers, "PanelRoof", Roof + new Vector3(0f, 0.665f, 0f));
            Marker(markers, "RoofAccess", new Vector3(13.75f, 0f, 33.2f));

            Camera camera = roots.Select(root => root.GetComponent<Camera>()).Single(component => component != null);
            ConfigurePlayer(terrain, camera, markers);
            ConfigureLighting(roots, lighting, palette.Sky);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Unity no pudo guardar la escena del nivel 01.");

            Selection.activeGameObject = school.gameObject;
            Debug.Log("Mapa del nivel 01 guardado con su terreno, recorrido y referencias de instalación.");
        }

        private static void BuildRoad(Transform road)
        {
            Transform arrival = Group(road, "RemateDeAfirmado");
            for (int row = 0; row < 2; row++)
            for (int column = -1; column <= 1; column++)
                Place("CaminoPatio", "CaminoAfirmado", arrival,
                    new Vector3(VehicleX + column * 1.50f, -0.078f, -3.6f + row * 2.4f));

            Transform branch = Group(road, "RamalPeatonal");
            Place("CaminoPatio", "CaminoAfirmado", branch, new Vector3(-4.78f, -0.082f, -3.4f), 90f);
            Place("CaminoPatio", "CaminoAdoquinado", branch, new Vector3(GateX, -0.075f, -1.8f));
        }

        private static void BuildForest(Terrain terrain, Transform forest)
        {
            var random = new System.Random(72611);
            Transform trees = Group(forest, "Arboles");
            Transform shrubs = Group(forest, "Sotobosque");
            Transform horizon = Group(forest, "Horizonte");
            var footprints = new List<Vector2>();
            for (int bank = -1; bank <= 1; bank += 2)
            {
                for (int row = 0; row < 11; row++)
                for (int lane = 0; lane < 5; lane++)
                {
                    float z = Mathf.Clamp(-52f + row * 4.1f + Variation(random, -1.5f, 1.5f), -54.5f, -10.8f);
                    float x = Level01TerrainBuilder.RoadX(z) + bank * (5.85f + lane * 4.4f + Variation(random, -0.55f, 1.6f));
                    var point = new Vector2(x, z);
                    if (footprints.Any(other => (other - point).sqrMagnitude < 6.25f)) continue;
                    footprints.Add(point);
                    double variant = random.NextDouble();
                    string resource = lane == 0 || row >= 9
                        ? variant < 0.30 ? "ArbolPequeno" : "ArbolMediano"
                        : variant < 0.46 ? "ArbolGrande" : variant < 0.87 ? "ArbolMediano" : "ArbolPequeno";
                    Place("Arbol", resource, trees, OnTerrain(terrain, x, z),
                        Variation(random, 0f, 360f));
                }
                for (int index = 0; index < 33; index++)
                {
                    float z = Variation(random, -54.8f, -10.8f);
                    float x = Level01TerrainBuilder.RoadX(z) + bank * Variation(random, 4.40f, 19f);
                    if (footprints.Any(other => (other - new Vector2(x, z)).sqrMagnitude < 0.9f)) continue;
                    Place("Arbusto", "ArbustoBajo", shrubs, OnTerrain(terrain, x, z),
                        Variation(random, 0f, 360f));
                }
            }
            Transform bend = Group(forest, "CierreVisualCurva");
            Place("Arbol", "ArbolPequeno", bend, OnTerrain(terrain, -11.75f, -23.30f), 106f);
            Place("Arbol", "ArbolMediano", bend, OnTerrain(terrain, -14.10f, -25.10f), 72f);
            Place("Arbol", "ArbolPequeno", bend, OnTerrain(terrain, -11.17f, -20.25f), 15f);
            Place("Arbol", "ArbolPequeno", bend, OnTerrain(terrain, -4.35f, -18.50f), 131f);
            Place("Arbol", "ArbolPequeno", bend, OnTerrain(terrain, -18.55f, -37.80f), 87f);
            Place("Montana", "LomaFondo", horizon, new Vector3(-69f, -1.4f, 38f), -12f);
            Place("Montana", "LomaFondo", horizon, new Vector3(67f, -1.25f, 66f), 16f);
            Place("Montana", "LomaFondo", horizon, new Vector3(-17f, -0.65f, 75f), -8f);
            Place("Montana", "MontanaFondo", horizon, new Vector3(-60f, -2.5f, 94f), -12f);
            Place("Montana", "MontanaFondo", horizon, new Vector3(15f, -1.5f, 95f), 7f);
            Place("Montana", "MontanaFondo", horizon, new Vector3(80f, -2f, 91f), 25f);
        }

        private static void BuildEntrance(Transform entrance, GameObject sign)
        {
            Transform perimeter = Group(entrance, "Perimetro");
            for (int index = 0; index < 3; index++)
                FrontFence(perimeter, LeftFence + (index + 0.5f) * 2.91f);
            FrontFence(perimeter, -4.355f);
            for (int index = 0; index < 11; index++)
                FrontFence(perimeter, -1.50f + (index + 0.5f) * 2.91f);

            for (int index = 0; index < 12; index++)
            {
                float z = (index + 0.5f) * 3f;
                Place("Cerca", "CercaMalla", perimeter, new Vector3(LeftFence, 0f, z), 90f);
                Place("Cerca", "CercaMalla", perimeter, new Vector3(RightFence, 0f, z), 90f);
            }
            for (int index = 0; index < 17; index++)
            {
                float x = Mathf.Lerp(LeftFence + 1.5f, RightFence - 1.5f, index / 16f);
                Place("Cerca", "CercaMalla", perimeter, new Vector3(x, 0f, BackFence));
            }

            Place("AccesoEscolar", "PortonVehicular", entrance, new Vector3(VehicleX, 0f, 0f));
            GameObject pedestrian = Place("AccesoEscolar", "PuertaPeatonal", entrance, new Vector3(GateX, 0f, 0f));
            Transform hinge = FindPart(pedestrian, "PuertaPeatonal_PivoteBisagraIzquierda");
            hinge.localRotation *= Quaternion.AngleAxis(95f, Vector3.up);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hinge);
            Place(sign, entrance, new Vector3(1.80f, 0f, -0.45f));
            Place("Arbusto", "Seto", entrance, new Vector3(1.80f, 0f, -1.05f));
            Place("Arbusto", "ArbustoFlorido", entrance, new Vector3(4.15f, 0f, -0.65f), 30f);
            Place("Arbusto", "ArbustoFlorido", entrance, new Vector3(-11.65f, 0f, -0.95f), 98f);
        }

        private static void FrontFence(Transform parent, float x)
        {
            Place("Cerca", "CercaRejaMetalica", parent, new Vector3(x, 0f, 0f));
        }

        private static void BuildCourtyard(Transform courtyard, CampusPalette palette)
        {
            Transform layout = Group(courtyard, "Senderos");
            for (int index = 0; index < 3; index++)
                Place("CaminoPatio", "CaminoAdoquinado", layout, new Vector3(GateX, -0.075f, 2.20f + 4f * index));

            for (int column = 0; column < 5; column++)
            for (int row = 0; row < 3; row++)
                Place("SueloPatio", "SueloBaldosaExterior", layout,
                    new Vector3(-8f + column * 2.24f, -0.052f, 8.3f + row * 2.24f), (column + row) % 2 * 90f);

            for (int index = 0; index < 3; index++)
                Place("CaminoPatio", "CaminoAdoquinado", layout, new Vector3(-7.9f + index * 4f, -0.069f, 13.7f), 90f);

            for (int row = 0; row < 3; row++)
            for (int lane = 0; lane < 2; lane++)
            {
                float z = 14.92f + row * 2.4f;
                Place("CaminoPatio", "CaminoConcreto", layout, new Vector3(-11f + (lane - 0.5f) * 1.19f, -0.071f, z));
                Place("CaminoPatio", "CaminoConcreto", layout, new Vector3(-0.5f + (lane - 0.5f) * 1.19f, -0.071f, z));
            }

            Place("CaminoPatio", "CaminoAdoquinado", layout, new Vector3(4.19f, -0.071f, 10.55f), 90f);
            for (int index = 0; index < 8; index++)
                Place("CaminoPatio", "CaminoConcreto", layout, new Vector3(1.1f + index * 2.38f, -0.076f, 19.6f), 90f);
            for (int row = 0; row < 2; row++)
                Place("CaminoPatio", "CaminoConcreto", layout, new Vector3(18.55f, -0.071f, 20.65f + row * 2.39f));

            Transform greenery = Group(courtyard, "Jardin");
            Place("Arbol", "ArbolMediano", greenery, new Vector3(-11.8f, 0f, 8.95f), 28f);
            Place("Arbol", "ArbolPequeno", greenery, new Vector3(-16.2f, 0f, 4.45f), 65f);
            Place("Arbol", "ArbolPequeno", greenery, new Vector3(-16.8f, 0f, 15.55f), 170f);
            Place("Arbol", "ArbolPequeno", greenery, new Vector3(26.1f, 0f, 24.9f), 72f);
            Place("Arbusto", "ArbustoFlorido", greenery, new Vector3(-8.68f, 0f, 6.05f), 165f);
            Place("Arbusto", "ArbustoFlorido", greenery, new Vector3(-15.9f, 0f, 13.25f), 18f);
            Place("Arbusto", "Seto", greenery, new Vector3(2.05f, 0f, 2.1f), 90f);
            Place("Arbusto", "Seto", greenery, new Vector3(26.2f, 0f, 20.55f));
            Place("Poste", "PosteLuz", courtyard, new Vector3(4.5f, 0f, 15.65f), 210f);
            BuildDirectory(courtyard, palette);
            SiteOutline(courtyard, new Vector3(ShadedSite.x, 0.006f, ShadedSite.z), palette.Marking);
            SmallSign(courtyard, new Vector3(-14.1f, 0f, 12.2f), "PANEL · PATIO", palette);
        }

        private static void BuildClassrooms(Transform classrooms, Transform markers, CampusPalette palette)
        {
            Transform a = Group(classrooms, "CommunityClassroom", ClassroomOne, 180f);
            GameObject basic = Place("EdificioAula", "AulaModularBasica", a);
            OpenDoor(basic, new Vector3(0.50f, 0.50f, 2.23f), 92f,
                "AulaModularBasica_Puerta", "Collider_AulaModularBasica_Puerta");
            const float floorA = 0.50f;
            for (int row = 0; row < 3; row++)
            foreach (float x in new[] { -2.37f, -1.24f, 1.24f, 2.37f })
                Place("CarpetaEscolar", "CarpetaIndividual", a, new Vector3(x, floorA, 0.73f - row * 1.63f));

            Place("EscritorioDocente", "EscritorioBasico", a, new Vector3(-1.65f, floorA, -3.56f));
            Place("Silla", "SillaDocente", a, new Vector3(-1.65f, floorA, -4.12f));
            Place("Silla", "SillaEscolar", a, new Vector3(3.17f, floorA, -3.83f), 270f);
            Place("Pizarra", "PizarraTiza", a, new Vector3(0.12f, 1.30f, -4.83f));
            Place("LuminariaInterior", "TuboLED", a, new Vector3(0f, 3.50f, -1.37f));
            WallLabel(a, new Vector3(-1.62f, 2.75f, 2.23f), 180f, "AULA 01", palette);
            Place("Docente", "DocenteDePie", a, new Vector3(1.15f, floorA, -3.54f));
            Marker(markers, "Teacher", a.TransformPoint(new Vector3(1.15f, floorA, -3.54f)));
            Place("LuminariaExterior", "LuminariaSolarMural", a, new Vector3(1.50f, 2.42f, 2.22f));

            Transform b = Group(classrooms, "SecondClassroom", ClassroomTwo, 180f);
            GameObject expanded = Place("EdificioAula", "AulaModularAmpliada", b);
            OpenDoor(expanded, new Vector3(5.01f, 0.34f, 2.43f), -94f,
                "AulaModularAmpliada_Puerta_Hoja", "AulaModularAmpliada_Puerta_Manija",
                "Collider_AulaModularAmpliada_Puerta_Hoja");
            const float floorB = 0.32f;
            for (int cluster = 0; cluster < 3; cluster++)
            {
                float x = -3.75f + cluster * 2.40f;
                Place("CarpetaEscolar", "CarpetaDoble", b, new Vector3(x, floorB, 0.4f), 180f);
                Place("CarpetaEscolar", "CarpetaDoble", b, new Vector3(x, floorB, -1.72f));
            }
            Place("EscritorioDocente", "EscritorioBasico", b, new Vector3(-2.65f, floorB, -3.28f));
            Place("Silla", "SillaDocente", b, new Vector3(-2.65f, floorB, -3.81f));
            Place("Pizarra", "PizarraBlanca", b, new Vector3(-0.15f, 1.12f, -4.23f));
            Place("LuminariaInterior", "PanelLED", b, new Vector3(-2.20f, 3.15f, -0.90f));
            Place("LuminariaInterior", "PanelLED", b, new Vector3(1.24f, 3.15f, -0.90f));
            WallLabel(b, new Vector3(5.56f, 2.40f, 2.41f), 180f, "AULA 02", palette);
            Ramp(classrooms, "RampaAula02", new Vector3(-0.50f, 0.001f, 19.02f), 1.72f, 1.70f, 0.305f, palette.Cement);
        }

        private static void BuildElectricalRoom(Transform parent, Transform markers, CampusPalette palette)
        {
            Transform room = Group(parent, "SalaCompacta", PowerRoom, 180f);
            GameObject house = Place("SalaElectrica", "SalaElectricaCompacta", room);
            OpenDoor(house, new Vector3(0.93f, 0.14f, 1.1775f), 102f,
                "SalaElectricaCompacta_PuertaHoja", "SalaElectricaCompacta_Manilla",
                "SalaElectricaCompacta_ManillaPlaca", "Collider_SalaElectricaCompacta_PuertaHoja");

            Vector3 boardPoint = new Vector3(-0.58f, 1.30f, -1.035f);
            GameObject board = Place("TablerosElectrico", "TableroDistribucion", room, boardPoint);
            Behaviour registration = board.GetComponentsInChildren<MonoBehaviour>(true)
                .Single(behaviour => behaviour.GetType().Name == "ModularBoard");
            registration.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(registration);
            Marker(markers, "Board", room.TransformPoint(boardPoint));
            Marker(markers, "Battery", room.TransformPoint(new Vector3(-0.94f, 0.123f, -0.51f)));
            Marker(markers, "Caretaker", new Vector3(20.19f, 0f, 23.56f));
            Place("PersonalEscuela", "PersonalAdministrativo", parent, new Vector3(20.19f, 0f, 23.56f), 12f);
            WallLabel(room, new Vector3(-0.66f, 2.47f, 1.24f), 180f, "SALA ELÉCTRICA", palette, 1.24f);

            Transform shelter = Group(parent, "CubiertaSolar");
            Place("TechoPaneles", "TechoPanelesPlano", shelter, Roof);
            for (int column = -1; column <= 1; column += 2)
            for (int row = -1; row <= 1; row += 2)
            {
                Vector3 p = new Vector3(Roof.x + column * 2.67f, 0f, Roof.z + row * 1.75f);
                Block(shelter, "Pedestal", p + Vector3.up * 0.136f, new Vector3(0.42f, 0.272f, 0.42f), palette.Cement);
                Block(shelter, "Columna metálica", p + Vector3.up * 1.73f, new Vector3(0.145f, 3.24f, 0.145f), palette.Steel);
                Block(shelter, "Placa de unión", p + Vector3.up * 3.335f, new Vector3(0.32f, 0.09f, 0.32f), palette.Steel);
            }
            Block(shelter, "Viga de fachada", new Vector3(Roof.x, 3.30f, Roof.z - 1.75f),
                new Vector3(5.58f, 0.17f, 0.16f), palette.Steel);
            WallLabel(shelter, new Vector3(Roof.x, 3.08f, Roof.z - 1.84f), 0f, "CUBIERTA SOLAR", palette, 1.76f);
            for (int column = 0; column < 4; column++)
            for (int row = 0; row < 3; row++)
                Place("SueloPatio", "SueloConcreto", shelter, new Vector3(11.44f + column * 1.52f, -0.069f, 27.79f + row * 1.52f));
        }

        private static void ConfigurePlayer(Terrain terrain, Camera camera, Transform markers)
        {
            Vector3 position = OnTerrain(terrain, -26.65f, -51.35f);
            Transform spawn = Marker(markers, "PlayerSpawn", position, 8f);
            Transform player = Group(null, "Player", position, 8f);
            var controller = player.gameObject.AddComponent<CharacterController>();
            controller.height = 1.78f;
            controller.radius = 0.233f;
            controller.center = Vector3.up * 0.89f;
            controller.skinWidth = 0.031f;
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;
            controller.minMoveDistance = 0f;

            GameObject avatar = Place("PersonalEscuela", "PersonalMantenimiento", player);
            foreach (Collider collider in avatar.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
            }

            camera.transform.SetParent(player, true);
            Quaternion orbit = Quaternion.Euler(9f, 8f, 0f);
            camera.transform.SetPositionAndRotation(position + Vector3.up * 1.42f + orbit * (Vector3.back * 3.35f), orbit);
            camera.nearClipPlane = 0.06f;
            camera.farClipPlane = 255f;
            camera.fieldOfView = 58f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.tag = "MainCamera";

            var data = new SerializedObject(player.gameObject.AddComponent<ThirdPersonExplorer>());
            data.FindProperty("controls").objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(
                "Assets/ResourcePack/InputSystem_Actions.inputactions");
            data.FindProperty("spawn").objectReferenceValue = spawn;
            data.FindProperty("avatar").objectReferenceValue = avatar.transform;
            data.FindProperty("view").objectReferenceValue = camera;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureLighting(GameObject[] roots, Transform lighting, Material sky)
        {
            Light sun = roots.Select(root => root.GetComponent<Light>()).Single(light => light != null);
            sun.transform.SetParent(lighting, true);
            sun.transform.rotation = Quaternion.Euler(44f, -32f, 0f);
            sun.color = new Color32(255, 240, 211, 255);
            sun.intensity = 1.28f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.83f;
            sun.shadowNormalBias = 0.35f;
            foreach (GameObject volume in roots.Where(root => root.name == "Global Volume"))
                volume.transform.SetParent(lighting, true);

            RenderSettings.sun = sun;
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color32(179, 196, 210, 255);
            RenderSettings.ambientEquatorColor = new Color32(129, 144, 139, 255);
            RenderSettings.ambientGroundColor = new Color32(98, 102, 86, 255);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0083f;
            RenderSettings.fogColor = new Color32(169, 194, 196, 255);
        }

        private static float Variation(System.Random random, float low, float high)
        {
            return Mathf.Lerp(low, high, (float)random.NextDouble());
        }

        private static Vector3 OnTerrain(Terrain terrain, float x, float z)
        {
            return new Vector3(x, Level01TerrainBuilder.GroundAt(terrain, x, z), z);
        }

        private static void OpenDoor(GameObject instance, Vector3 hinge, float angle, params string[] pieces)
        {
            Vector3 pivot = instance.transform.TransformPoint(hinge);
            Quaternion rotation = Quaternion.AngleAxis(angle, instance.transform.up);
            Transform[] parts = pieces.Select(piece => FindPart(instance, piece)).ToArray();
            foreach (Transform part in parts)
            {
                if (parts.Any(other => part != other && part.IsChildOf(other))) continue;
                part.SetPositionAndRotation(pivot + rotation * (part.position - pivot), rotation * part.rotation);
                PrefabUtility.RecordPrefabInstancePropertyModifications(part);
            }
        }

        private static Transform FindPart(GameObject instance, string part)
        {
            return instance.GetComponentsInChildren<Transform>(true).Single(item => item.name == part);
        }

        private static Transform Group(Transform parent, string name, Vector3 position = default, float yaw = 0f)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = position;
            group.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return group;
        }

        private static Transform Marker(Transform root, string id, Vector3 point, float yaw = 0f)
        {
            return Group(root, id, point, yaw);
        }

        private static GameObject Place(string family, string part, Transform parent,
            Vector3 position = default, float yaw = 0f)
        {
            string path = $"Assets/ResourcePack/{family}/Prefabs/{part}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException($"No se encontró {path}.");
            return Place(prefab, parent, position, yaw);
        }

        private static GameObject Place(GameObject prefab, Transform parent, Vector3 position, float yaw = 0f)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            return instance;
        }
    }
}
