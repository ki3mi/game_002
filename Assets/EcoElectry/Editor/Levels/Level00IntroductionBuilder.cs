using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string ScenePath = "Assets/EcoElectry/Content/Scenes/Level00-Introduction.unity";
        private const string ThirdPartyRoot = "Assets/EcoElectry/ThirdParty";
        private const string TerrainFolder = "Assets/EcoElectry/Content/Terrain/Level00";
        private const string MeshFolder = TerrainFolder + "/Meshes";
        private const string PrefabFolder = "Assets/EcoElectry/Content/Prefabs/Level00";
        private const string ModelsFolder = "Assets/EcoElectry/Content/Models/Level00";
        private const string MaterialsFolder = ModelsFolder + "/Materials";
        private const string RootName = "Level00Introduction";
        private const float RoadHalfWidth = 3.35f;
        private const float ShoulderOuter = 5.15f;
        private const float BarrierOffset = 6.15f;
        private const float SchoolX = 220f;
        private const float SchoolZ = 3480f;
        private const float GateX = 210f;
        private const float GateZ = 3390f;
        private const float PedestrianGateX = 220f;
        private const float CampusGrade = 14f;
        private const float ParkingSurface = CampusGrade + 0.08f;
        private const float GuardX = 189f;
        private const float GuardZ = 3400f;
        private const float TruckX = 198f;
        private const float TruckZ = 3418f;
        private const float VehicleGateScale = 1.6f;
        private const float PedestrianGateScale = 1.45f;

        private static readonly Vector3[] ForestTrailControls =
        {
            new Vector3(248f, 0f, 3442f),
            new Vector3(280f, 0f, 3443f),
            new Vector3(318f, 0f, 3448f),
            new Vector3(353f, 0f, 3470f),
            new Vector3(389f, 0f, 3503f),
            new Vector3(430f, 0f, 3535f)
        };

        private static readonly Vector3[] RoadControls =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0.4f, 230f),
            new Vector3(16f, 1.2f, 500f),
            new Vector3(65f, 3.3f, 790f),
            new Vector3(128f, 6.1f, 1090f),
            new Vector3(157f, 8.2f, 1390f),
            new Vector3(117f, 10.2f, 1655f),
            new Vector3(28f, 12.1f, 1915f),
            new Vector3(-74f, 14.0f, 2195f),
            new Vector3(-133f, 15.2f, 2460f),
            new Vector3(-111f, 14.6f, 2715f),
            new Vector3(-29f, 13.7f, 2935f),
            new Vector3(62f, 13.2f, 3130f),
            new Vector3(123f, 13.4f, 3270f),
            new Vector3(175f, 13.8f, 3350f),
            new Vector3(207f, 14f, 3374f),
            new Vector3(GateX, 14f, GateZ)
        };

        [MenuItem("Tools/EcoElectry/Nivel 00/Montar escena introductoria")]
        internal static void BuildScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("El mapa del nivel 00 se monta en modo edición.");

            EnsureFolder(TerrainFolder);
            EnsureFolder(MeshFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(ModelsFolder);
            EnsureFolder(MaterialsFolder);

            var path = BuildPath(RoadControls, 4f);
            if (path.Length < 3200f || path.Length > 4200f)
                throw new InvalidOperationException("La longitud del recorrido debe quedar entre 3 y 4 km.");

            Scene scene = OpenOrCreateScene();
            SceneManager.SetActiveScene(scene);
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == RootName)
                    Object.DestroyImmediate(root);

            Transform level = Group(scene, null, RootName);
            Palette palette = new Palette();
            TerrainGrid terrain = CreateTerrain(Group(scene, level, "Terrain"), Group(scene, level, "Forest"), path);
            BuildHighway(level, path, terrain, palette);
            BuildSchoolExterior(level, terrain, palette);
            BuildMaintenance(level, terrain);
            BuildForestTrail(level, terrain, palette);
            BuildMarkers(level, path, terrain);
            ConfigureNightIntroduction(level);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Unity no pudo guardar la escena del nivel 00.");
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = level.gameObject;
            Debug.Log("Nivel 00 montado: " + Mathf.RoundToInt(path.Length) +
                      " m de recorrido, terreno, accesos, colegio, mantenimiento y marcadores.");
        }

        private static Scene OpenOrCreateScene()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (scene.IsValid() && scene.isLoaded) return scene;

            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            return asset != null
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }

        private static Transform Group(Scene scene, Transform parent, string name, Vector3 position = default(Vector3))
        {
            var gameObject = new GameObject(name);
            if (parent != null)
                gameObject.transform.SetParent(parent, false);
            else if (scene.IsValid())
                SceneManager.MoveGameObjectToScene(gameObject, scene);
            gameObject.transform.localPosition = position;
            gameObject.isStatic = true;
            return gameObject.transform;
        }

        private static PathData BuildPath(IList<Vector3> controls, float spacing)
        {
            var dense = new List<Vector3>();
            const int subdivisions = 80;
            for (int segment = 0; segment < controls.Count - 1; segment++)
            {
                Vector3 p0 = controls[Mathf.Max(0, segment - 1)];
                Vector3 p1 = controls[segment];
                Vector3 p2 = controls[segment + 1];
                Vector3 p3 = controls[Mathf.Min(controls.Count - 1, segment + 2)];
                for (int i = 0; i < subdivisions; i++)
                    dense.Add(CatmullRom(p0, p1, p2, p3, i / (float)subdivisions));
            }
            dense.Add(controls[controls.Count - 1]);

            var lengths = new float[dense.Count];
            for (int i = 1; i < dense.Count; i++)
                lengths[i] = lengths[i - 1] + Vector3.Distance(dense[i - 1], dense[i]);

            var points = new List<Vector3>();
            int cursor = 1;
            for (float distance = 0f; distance < lengths[lengths.Length - 1]; distance += spacing)
            {
                while (cursor < lengths.Length - 1 && lengths[cursor] < distance) cursor++;
                float span = lengths[cursor] - lengths[cursor - 1];
                float t = span <= Mathf.Epsilon ? 0f : (distance - lengths[cursor - 1]) / span;
                points.Add(Vector3.Lerp(dense[cursor - 1], dense[cursor], t));
            }
            points.Add(dense[dense.Count - 1]);
            return new PathData(points);
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t +
                           (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                           (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static PathData GroundPath(TerrainGrid terrain, IList<Vector3> controls, float spacing, float lift)
        {
            PathData path = BuildPath(controls, spacing);
            var points = new List<Vector3>(path.Points.Count);
            foreach (Vector3 point in path.Points)
                points.Add(new Vector3(point.x, GroundAt(terrain, point.x, point.z) + lift, point.z));
            return new PathData(points);
        }

        private static Vector3 RoadAtZ(PathData path, float z)
        {
            if (z <= path.Points[0].z) return path.Points[0];
            int last = path.Points.Count - 1;
            if (z >= path.Points[last].z) return path.Points[last];

            int low = 0;
            int high = last;
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (path.Points[mid].z < z) low = mid;
                else high = mid;
            }
            float span = path.Points[high].z - path.Points[low].z;
            float t = span <= Mathf.Epsilon ? 0f : (z - path.Points[low].z) / span;
            return Vector3.Lerp(path.Points[low], path.Points[high], t);
        }

        private static void BuildHighway(Transform level, PathData path, TerrainGrid terrain, Palette palette)
        {
            Transform highway = Group(SceneManager.GetActiveScene(), level, "Highway");
            int split = 0;
            while (split < path.Points.Count - 1 && path.Points[split].z < 3130f) split++;
            Transform main = Group(SceneManager.GetActiveScene(), highway, "Autopista");
            Transform ramp = Group(SceneManager.GetActiveScene(), highway, "Ramal_AccesoColegio");
            BuildRoadSurfaces(main, path, 0, split, palette, "Main");
            BuildRoadSurfaces(ramp, path, split, path.Points.Count - 1, palette, "Access");
            BuildRoadMarkings(highway, path, palette);
            BuildBarriers(highway, path, palette);
            BuildReflectors(highway, path, palette);
            BuildHighwaySigns(highway, path, palette);
            BuildParking(highway, path, terrain, palette);
        }

        private static void BuildRoadSurfaces(Transform parent, PathData path, int first, int last, Palette palette, string prefix)
        {
            MeshBuilder road = new MeshBuilder();
            MeshBuilder leftShoulder = new MeshBuilder();
            MeshBuilder rightShoulder = new MeshBuilder();

            for (int i = first; i < last; i++)
            {
                Vector3 a = path.Points[i];
                Vector3 b = path.Points[i + 1];
                Vector3 rightA = PathRight(path, i);
                Vector3 rightB = PathRight(path, i + 1);

                Vector3 leftInnerA = a - rightA * RoadHalfWidth;
                Vector3 leftInnerB = b - rightB * RoadHalfWidth;
                Vector3 rightInnerA = a + rightA * RoadHalfWidth;
                Vector3 rightInnerB = b + rightB * RoadHalfWidth;
                road.AddQuad(leftInnerA + Vector3.up * 0.08f, leftInnerB + Vector3.up * 0.08f,
                             rightInnerB + Vector3.up * 0.08f, rightInnerA + Vector3.up * 0.08f);

                Vector3 leftOuterA = a - rightA * ShoulderOuter;
                Vector3 leftOuterB = b - rightB * ShoulderOuter;
                Vector3 rightOuterA = a + rightA * ShoulderOuter;
                Vector3 rightOuterB = b + rightB * ShoulderOuter;
                leftShoulder.AddQuad(leftOuterA + Vector3.up * 0.035f, leftOuterB + Vector3.up * 0.035f,
                                     leftInnerB + Vector3.up * 0.035f, leftInnerA + Vector3.up * 0.035f);
                rightShoulder.AddQuad(rightInnerA + Vector3.up * 0.035f, rightInnerB + Vector3.up * 0.035f,
                                      rightOuterB + Vector3.up * 0.035f, rightOuterA + Vector3.up * 0.035f);
            }

            CreateMeshObject(parent, MeshFolder + "/Level00_" + prefix + "_Asphalt.asset",
                prefix + "_Asphalt", road, palette.Asphalt, true);
            CreateMeshObject(parent, MeshFolder + "/Level00_" + prefix + "_ShoulderLeft.asset",
                prefix + "_ShoulderLeft", leftShoulder, palette.Shoulder, true);
            CreateMeshObject(parent, MeshFolder + "/Level00_" + prefix + "_ShoulderRight.asset",
                prefix + "_ShoulderRight", rightShoulder, palette.Shoulder, true);
        }

        private static Vector3 PathRight(PathData path, int index)
        {
            Vector3 previous = path.Points[Mathf.Max(0, index - 1)];
            Vector3 next = path.Points[Mathf.Min(path.Points.Count - 1, index + 1)];
            return Vector3.Cross(Vector3.up, FlatDirection(previous, next));
        }

        private static void BuildRoadMarkings(Transform parent, PathData path, Palette palette)
        {
            MeshBuilder edges = new MeshBuilder();
            MeshBuilder center = new MeshBuilder();
            const float edgeWidth = 0.16f;
            for (int i = 0; i < path.Points.Count - 1; i++)
            {
                Vector3 a = path.Points[i];
                Vector3 b = path.Points[i + 1];
                Vector3 right = Vector3.Cross(Vector3.up, FlatDirection(a, b)).normalized;
                AddRibbonSegment(edges, a, b, right, -RoadHalfWidth + 0.16f, edgeWidth, 0.11f);
                AddRibbonSegment(edges, a, b, right, RoadHalfWidth - 0.16f, edgeWidth, 0.11f);
            }

            for (float distance = 0f; distance < path.Length; distance += 13f)
            {
                float dashEnd = Mathf.Min(distance + 6f, path.Length);
                Vector3 a = path.AtDistance(distance);
                Vector3 b = path.AtDistance(dashEnd);
                Vector3 right = Vector3.Cross(Vector3.up, FlatDirection(a, b)).normalized;
                AddRibbonSegment(center, a, b, right, 0f, 0.15f, 0.12f);
            }

            CreateMeshObject(parent, MeshFolder + "/Level00_RoadMarkings_Edges.asset",
                "Lineas laterales", edges, palette.Paint, false);
            CreateMeshObject(parent, MeshFolder + "/Level00_RoadMarkings_Center.asset",
                "Linea central discontinua", center, palette.Paint, false);
        }

        private static void AddRibbonSegment(MeshBuilder builder, Vector3 a, Vector3 b, Vector3 right, float offset, float width, float lift)
        {
            Vector3 side = right * width * 0.5f;
            Vector3 centerA = a + right * offset + Vector3.up * lift;
            Vector3 centerB = b + right * offset + Vector3.up * lift;
            builder.AddQuad(centerA - side, centerB - side, centerB + side, centerA + side);
        }

        private static void BuildBarriers(Transform parent, PathData path, Palette palette)
        {
            Transform barriers = Group(SceneManager.GetActiveScene(), parent, "BarrerasFisicas");
            const float segment = 14f;
            int index = 0;
            for (float distance = 0f; distance < path.Length - 18f; distance += segment)
            {
                Vector3 point = path.AtDistance(distance + segment * 0.5f);
                Vector3 tangent = FlatDirection(path.AtDistance(Mathf.Max(0f, distance - 1f)), path.AtDistance(Mathf.Min(path.Length, distance + 1f)));
                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
                float yaw = Mathf.Atan2(tangent.x, tangent.z) * Mathf.Rad2Deg;
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.name = "Barrera_" + index.ToString("D3") + (side < 0 ? "_Izquierda" : "_Derecha");
                    block.transform.SetParent(barriers, false);
                    block.transform.position = point + right * (side * BarrierOffset) + Vector3.up * 0.47f;
                    block.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                    block.transform.localScale = new Vector3(0.38f, 0.94f, segment + 0.5f);
                    block.GetComponent<MeshRenderer>().sharedMaterial = palette.Barrier;
                    block.isStatic = true;
                }
                index++;
            }
        }

        private static void BuildReflectors(Transform parent, PathData path, Palette palette)
        {
            Transform reflectors = Group(SceneManager.GetActiveScene(), parent, "PostesReflectantes");
            int index = 0;
            for (float distance = 35f; distance < path.Length - 35f; distance += 56f)
            {
                Vector3 point = path.AtDistance(distance);
                Vector3 tangent = FlatDirection(path.AtDistance(distance - 1f), path.AtDistance(distance + 1f));
                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.name = "Reflector_" + index.ToString("D2") + (side < 0 ? "_I" : "_D");
                    marker.transform.SetParent(reflectors, false);
                    marker.transform.position = point + right * (side * 5.8f) + Vector3.up * 0.33f;
                    marker.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(tangent.x, tangent.z) * Mathf.Rad2Deg, 0f);
                    marker.transform.localScale = new Vector3(0.12f, 0.48f, 0.09f);
                    marker.GetComponent<MeshRenderer>().sharedMaterial = palette.Reflector;
                    Object.DestroyImmediate(marker.GetComponent<BoxCollider>());
                    marker.isStatic = true;
                }
                index++;
            }
        }

        private static void BuildHighwaySigns(Transform parent, PathData path, Palette palette)
        {
            for (int kilometer = 1; kilometer <= 3; kilometer++)
            {
                float distance = kilometer * 1000f;
                if (distance >= path.Length - 500f) break;
                CreateRoadSign(parent, path, distance, "km " + kilometer, palette, false);
            }
            CreateRoadSign(parent, path, path.Length - 235f, "COLEGIO", palette, true);
            CreateRoadSign(parent, path, path.Length - 130f, "MANTENIMIENTO", palette, true);
        }

        private static void CreateRoadSign(Transform parent, PathData path, float distance, string text, Palette palette, bool large)
        {
            Vector3 point = path.AtDistance(distance);
            Vector3 tangent = FlatDirection(path.AtDistance(Mathf.Max(0f, distance - 1f)),
                                            path.AtDistance(Mathf.Min(path.Length, distance + 1f)));
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            Transform sign = Group(SceneManager.GetActiveScene(), parent,
                "Senal_" + text.Replace(" ", "_"), point + right * 10.5f);
            sign.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            float width = large ? 3.6f : 1.6f;
            float height = large ? 1.15f : 0.72f;
            Block(sign, "Poste", new Vector3(0f, 1.15f, 0f), new Vector3(0.12f, 2.3f, 0.12f), palette.Steel);
            Block(sign, "Placa", new Vector3(0f, 2.22f, 0f), new Vector3(width, height, 0.13f), palette.Sign);
            Label(sign, new Vector3(0f, 2.22f, -0.075f), text, width - 0.18f, height - 0.14f, palette.SignText, large ? 1.1f : 0.7f);
            if (large)
                Label(sign, new Vector3(0f, 1.84f, -0.08f), "ACCESO DE SERVICIO", width - 0.22f, 0.18f, palette.SignText, 0.6f);
        }

        private static void BuildParking(Transform parent, PathData road, TerrainGrid terrain, Palette palette)
        {
            Transform parking = Group(SceneManager.GetActiveScene(), parent, "Estacionamiento");
            const float y = ParkingSurface;
            Block(parking, "Pavimento del estacionamiento",
                new Vector3(215f, y - 0.14f, 3421f), new Vector3(92f, 0.28f, 58f), palette.Asphalt);
            int last = road.Points.Count - 1;
            Vector3 end = road.Points[last] + Vector3.up * 0.08f;
            Vector3 side = PathRight(road, last) * RoadHalfWidth;
            var connector = new MeshBuilder();
            connector.AddQuad(end - side, new Vector3(GateX - RoadHalfWidth, y, 3392f),
                new Vector3(GateX + RoadHalfWidth, y, 3392f), end + side);
            CreateMeshObject(parking, MeshFolder + "/Level00_ParkingAccess.asset",
                "Conector de acceso", connector, palette.Asphalt, true);

            MeshBuilder markings = new MeshBuilder();
            const float paintY = y + 0.012f;
            foreach (float x in new[] { 176f, 195f, 231f, 250f })
            {
                for (int i = 0; i <= 11; i++)
                {
                    float z = 3406.8f + i * 3.2f;
                    AddWorldLine(markings, new Vector3(x, paintY, z), new Vector3(x + 6f, paintY, z), 0.14f);
                }
                AddWorldLine(markings, new Vector3(x, paintY, 3406.8f), new Vector3(x, paintY, 3442f), 0.14f);
            }
            CreateMeshObject(parking, MeshFolder + "/Level00_ParkingMarkings.asset",
                "Lineas de estacionamiento", markings, palette.Paint, false);

            PlacePrefab(LoadPrefab("Assets/ResourcePack/Poste/Prefabs/PosteLuz.prefab"), parking,
                new Vector3(170f, GroundAt(terrain, 170f, 3422f), 3422f), 0f, Vector3.one, "PosteLuz_Estacionamiento_Oeste");
            PlacePrefab(LoadPrefab("Assets/ResourcePack/Poste/Prefabs/PosteLuz.prefab"), parking,
                new Vector3(260f, GroundAt(terrain, 260f, 3422f), 3422f), 180f, Vector3.one, "PosteLuz_Estacionamiento_Este");
        }

        private static void AddWorldLine(MeshBuilder builder, Vector3 a, Vector3 b, float width)
        {
            Vector3 direction = FlatDirection(a, b);
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized * (width * 0.5f);
            builder.AddQuad(a - side, b - side, b + side, a + side);
        }

        private static void BuildSchoolExterior(Transform level, TerrainGrid terrain, Palette palette)
        {
            Transform school = Group(SceneManager.GetActiveScene(), level, "SchoolExterior");
            Transform buildingGroup = Group(SceneManager.GetActiveScene(), school, "BuildingA");
            GameObject buildingPrefab = LoadPrefab(ThirdPartyRoot + "/Kansai/BuildingA/Prefabs/A_Tou.prefab");
            Bounds localBounds = PrefabLocalBounds(buildingPrefab);
            const float buildingScale = 0.65f;
            float buildingGround = GroundAt(terrain, SchoolX, SchoolZ);
            float buildingY = buildingGround - localBounds.min.y * buildingScale;
            GameObject building = PlacePrefab(buildingPrefab, buildingGroup, new Vector3(SchoolX, buildingY, SchoolZ),
                180f, Vector3.one * buildingScale, "A_Tou_Scale0_65_Dim64x25x9m");
            ApplyUniversalMaterials(building);

            Transform canopy = Group(SceneManager.GetActiveScene(), school, "EntradaPrincipal",
                new Vector3(SchoolX, buildingGround, 3465.2f));
            Block(canopy, "Escalon_1", new Vector3(0f, 0.07f, 1.35f), new Vector3(9f, 0.14f, 2.0f), palette.Concrete);
            Block(canopy, "Escalon_2", new Vector3(0f, 0.15f, 0.45f), new Vector3(7.2f, 0.16f, 1.4f), palette.Concrete);
            Block(canopy, "Techo", new Vector3(0f, 3.45f, 0.55f), new Vector3(7.5f, 0.28f, 3.8f), palette.Roof);
            Block(canopy, "Pilar_izquierdo", new Vector3(-3.1f, 1.72f, -0.9f), new Vector3(0.28f, 3.44f, 0.28f), palette.Steel);
            Block(canopy, "Pilar_derecho", new Vector3(3.1f, 1.72f, -0.9f), new Vector3(0.28f, 3.44f, 0.28f), palette.Steel);
            Block(canopy, "Placa_entrada", new Vector3(0f, 2.85f, -1.0f), new Vector3(3.4f, 0.46f, 0.10f), palette.Sign);
            Label(canopy, new Vector3(0f, 2.85f, -1.06f), "ENTRADA", 3.1f, 0.32f, palette.SignText, 0.82f);

            Transform perimeter = Group(SceneManager.GetActiveScene(), school, "Perimetro");
            GameObject fencePrefab = CreateAdaptedFencePrefab(palette);
            const float vehicleHalf = 2.19f * VehicleGateScale;
            const float pedestrianHalf = 0.70f * PedestrianGateScale;
            BuildFenceLineX(perimeter, terrain, fencePrefab, 160f, GateX - vehicleHalf, GateZ, "Frontal_Oeste");
            BuildFenceLineX(perimeter, terrain, fencePrefab, GateX + vehicleHalf, PedestrianGateX - pedestrianHalf, GateZ, "Frontal_Centro");
            BuildFenceLineX(perimeter, terrain, fencePrefab, PedestrianGateX + pedestrianHalf, 280f, GateZ, "Frontal_Este");
            BuildFenceLineZ(perimeter, terrain, fencePrefab, 160f, GateZ, 3540f, false, false);
            BuildFenceLineZ(perimeter, terrain, fencePrefab, 280f, GateZ, 3540f, true, true);
            BuildFenceLineX(perimeter, terrain, fencePrefab, 160f, 280f, 3540f, "Fondo");

            GameObject vehicleGate = PlacePrefab(
                LoadPrefab("Assets/ResourcePack/AccesoEscolar/Prefabs/PortonVehicular.prefab"),
                perimeter, new Vector3(GateX, ParkingSurface, GateZ), 0f, new Vector3(VehicleGateScale, 1f, 1f), "PortonVehicular_Abierto");
            ApplyMaterialOverride(vehicleGate, palette.Steel);
            OpenGate(vehicleGate, "PortonVehicular_PivoteIzquierdo", 90f, "PortonVehicular_PivoteDerecho", -90f);
            DisableRootInteraction(vehicleGate);

            GameObject pedestrianGate = PlacePrefab(
                LoadPrefab("Assets/ResourcePack/AccesoEscolar/Prefabs/PuertaPeatonal.prefab"),
                perimeter, new Vector3(PedestrianGateX, CampusGrade, GateZ), 0f,
                new Vector3(PedestrianGateScale, 1f, 1f), "PuertaPeatonal_Abierta");
            ApplyMaterialOverride(pedestrianGate, palette.Steel);
            Transform pedestrianLeaf = pedestrianGate.transform.Find("PuertaPeatonal_Root/PuertaPeatonal_PivoteBisagraIzquierda");
            if (pedestrianLeaf != null)
            {
                pedestrianLeaf.localRotation = Quaternion.Euler(0f, 90f, 0f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(pedestrianLeaf);
            }
            DisableRootInteraction(pedestrianGate);

            Transform guard = Group(SceneManager.GetActiveScene(), school, "CasetaVigilancia",
                new Vector3(GuardX, ParkingSurface, GuardZ));
            BuildGuardBooth(guard, palette);
            BuildCampusSign(school, terrain, palette);
            BuildWalkway(school, terrain, palette);
            BuildCampusDecoration(school, terrain);
            BuildFrontGardens(school, terrain, fencePrefab);


            Transform scaleRecord = Group(SceneManager.GetActiveScene(), school,
                "A_Tou_Escala0_65_Dimensiones64x25x9m", new Vector3(SchoolX, buildingGround, SchoolZ));
            scaleRecord.gameObject.hideFlags = HideFlags.None;
            building.isStatic = true;

        }

        private static void BuildFenceLineX(Transform parent, TerrainGrid terrain, GameObject fencePrefab,
            float startX, float endX, float z, string label)
        {
            const float panelWidth = 2.91f;
            int count = Mathf.Max(1, Mathf.RoundToInt((endX - startX) / panelWidth));
            float step = (endX - startX) / count;
            for (int index = 0; index < count; index++)
            {
                float x = startX + (index + 0.5f) * step;
                PlacePrefab(fencePrefab, parent,
                    new Vector3(x, GroundAt(terrain, x, z), z), 0f, new Vector3(step / panelWidth, 1f, 1f),
                    $"Cerca_{label}_{index:00}");
            }
        }

        private static void BuildFenceLineZ(Transform parent, TerrainGrid terrain, GameObject fencePrefab, float x,
            float zStart, float zEnd, bool eastSide, bool openForestGap)
        {
            float step = 2.91f;
            int index = 0;
            for (float z = zStart + step * 0.5f; z < zEnd - step * 0.5f; z += step)
            {
                if (openForestGap && eastSide && z > 3437f && z < 3452f)
                    continue;
                PlacePrefab(fencePrefab, parent, new Vector3(x, GroundAt(terrain, x, z), z),
                    90f, Vector3.one, (eastSide ? "Cerca_Este_" : "Cerca_Oeste_") + index.ToString("D2"));
                index++;
            }
        }

        private static void BuildCampusSign(Transform parent, TerrainGrid terrain, Palette palette)
        {
            Transform sign = Group(SceneManager.GetActiveScene(), parent, "RotuloColegio",
                new Vector3(235f, GroundAt(terrain, 235f, 3386f), 3386f));
            sign.rotation = Quaternion.Euler(0f, 180f, 0f);
            Block(sign, "Base", new Vector3(0f, 0.18f, 0f), new Vector3(5.4f, 0.36f, 0.55f), palette.Concrete);
            Block(sign, "Poste_I", new Vector3(-2.05f, 1.28f, 0f), new Vector3(0.20f, 2.56f, 0.20f), palette.Steel);
            Block(sign, "Poste_D", new Vector3(2.05f, 1.28f, 0f), new Vector3(0.20f, 2.56f, 0.20f), palette.Steel);
            Block(sign, "Placa", new Vector3(0f, 2.13f, 0f), new Vector3(4.65f, 0.86f, 0.14f), palette.Sign);
            Label(sign, new Vector3(0f, 2.13f, -0.085f), "COLEGIO", 4.35f, 0.60f, palette.SignText, 1.1f);
            OrientCampusSignText(sign);
        }

        private static void OrientCampusSignText(Transform sign)
        {
            if (sign == null) return;
            Vector3 baseScale = sign.localScale;
            if (baseScale.x < 0f)
            {
                baseScale.x = -baseScale.x;
                sign.localScale = baseScale;
                foreach (Transform child in sign)
                {
                    Vector3 position = child.localPosition;
                    position.x = -position.x;
                    child.localPosition = position;
                }
            }
            TextMesh text = sign.GetComponentInChildren<TextMesh>();
            if (text == null) return;
            Vector3 textScale = text.transform.localScale;
            textScale.x = -Mathf.Abs(textScale.x);
            text.transform.localScale = textScale;
        }

        private static void BuildGuardBooth(Transform root, Palette palette)
        {
            const float width = 5f;
            const float depth = 4.2f;
            const float wallHeight = 2.8f;
            Block(root, "Piso", new Vector3(0f, 0.08f, 0f), new Vector3(width, 0.16f, depth), palette.Concrete);
            Block(root, "Muro_trasero", new Vector3(0f, wallHeight * 0.5f, depth * 0.5f - 0.10f),
                new Vector3(width, wallHeight, 0.20f), palette.Wall);
            Block(root, "Muro_izquierdo", new Vector3(-width * 0.5f + 0.10f, wallHeight * 0.5f, 0f),
                new Vector3(0.20f, wallHeight, depth), palette.Wall);
            Block(root, "Muro_derecho", new Vector3(width * 0.5f - 0.10f, wallHeight * 0.5f, 0f),
                new Vector3(0.20f, wallHeight, depth), palette.Wall);
            Block(root, "Frente_inferior", new Vector3(0f, 0.50f, -depth * 0.5f + 0.10f),
                new Vector3(width, 1.0f, 0.20f), palette.Wall);
            Block(root, "Frente_superior", new Vector3(0f, 2.58f, -depth * 0.5f + 0.10f),
                new Vector3(width, 0.44f, 0.20f), palette.Wall);
            const float front = -depth * 0.5f + 0.10f;
            Block(root, "Frente_izquierda", new Vector3(-1.575f, 1.68f, front),
                new Vector3(1.85f, 1.36f, 0.20f), palette.Wall);
            Block(root, "Frente_derecha", new Vector3(1.575f, 1.68f, front),
                new Vector3(1.85f, 1.36f, 0.20f), palette.Wall);
            Block(root, "Antepecho_ventanilla", new Vector3(0f, 1.14f, front),
                new Vector3(1.30f, 0.28f, 0.20f), palette.Wall);
            Block(root, "Dintel_ventanilla", new Vector3(0f, 2.205f, front),
                new Vector3(1.30f, 0.31f, 0.20f), palette.Wall);
            Transform grille = Group(SceneManager.GetActiveScene(), root, "Rejilla_comunicación", new Vector3(0f, 1.665f, front));
            for (int i = -6; i <= 6; i++)
                Block(grille, "Barrote_" + (i + 6), new Vector3(i * 0.105f, 0f, 0f),
                    new Vector3(0.04f, 0.77f, 0.09f), palette.Steel, false);
            for (int i = -1; i <= 1; i++)
                Block(grille, "Travesaño_" + (i + 1), new Vector3(0f, i * 0.36f, 0f),
                    new Vector3(1.30f, 0.045f, 0.09f), palette.Steel, false);
            grille.gameObject.AddComponent<BoxCollider>().size = new Vector3(1.30f, 0.77f, 0.10f);
            Block(root, "Techo", new Vector3(0f, wallHeight + 0.16f, 0f),
                new Vector3(width + 0.4f, 0.32f, depth + 0.4f), palette.Roof);
            Block(root, "Puerta", new Vector3(width * 0.5f - 0.48f, 1.08f, -0.7f),
                new Vector3(0.82f, 2.16f, 0.12f), palette.Door);
            Block(root, "Rótulo", new Vector3(0f, 3.05f, -depth * 0.5f - 0.03f),
                new Vector3(3.0f, 0.38f, 0.08f), palette.Sign);
            Label(root, new Vector3(0f, 3.05f, -depth * 0.5f - 0.08f), "VIGILANCIA", 2.8f, 0.27f, palette.SignText, 0.78f);
        }

        private static void BuildWalkway(Transform parent, TerrainGrid terrain, Palette palette)
        {
            BuildCobblestonePaths(parent);

            Transform entrance = Group(SceneManager.GetActiveScene(), parent, "CaminoMantenimiento",
                new Vector3(258f, GroundAt(terrain, 258f, 3465f) + 0.07f, 3465f));
            entrance.rotation = Quaternion.Euler(0f, 90f, 0f);
            Block(entrance, "Señal", new Vector3(1.0f, 1.3f, 0f), new Vector3(2.0f, 0.75f, 0.12f), palette.Sign);
            Label(entrance, new Vector3(1.0f, 1.3f, -0.075f), "MANTENIMIENTO", 1.8f, 0.55f, palette.SignText, 0.66f);
        }

        private static void BuildForestTrail(Transform level, TerrainGrid terrain, Palette palette)
        {
            PathData trail = GroundPath(terrain, ForestTrailControls, 1.2f, 0.13f);
            Transform group = Group(SceneManager.GetActiveScene(), level, "ForestTrail");
            var mesh = new MeshBuilder();
            AddRibbon(mesh, trail, 2.4f, terrain);
            CreateMeshObject(group, MeshFolder + "/Level00_ForestTrail.asset",
                "Sendero_al_bosque", mesh, palette.DirtPath, true);
        }

        private static void BuildMarkers(Transform level, PathData road, TerrainGrid terrain)
        {
            Transform markers = Group(SceneManager.GetActiveScene(), level, "Markers");
            Vector3 start = road.AtDistance(5.5f);
            Vector3 startDirection = FlatDirection(start, road.AtDistance(9f));
            Marker(markers, "VehicleStart", start, startDirection);
            Marker(markers, "VehicleParking", new Vector3(TruckX, ParkingSurface, TruckZ), Vector3.right);
            Marker(markers, "OnFootExit", new Vector3(TruckX + 0.8f, ParkingSurface + 0.04f, TruckZ + 2.1f), Vector3.back);
            Marker(markers, "GuardVoicePoint", new Vector3(GuardX, ParkingSurface + 1.665f, GuardZ - 1.65f), Vector3.back);
            Marker(markers, "ForestEdge", new Vector3(305f, GroundAt(terrain, 305f, 3445f), 3445f), Vector3.right);
            Marker(markers, "ForestDepth", new Vector3(430f, GroundAt(terrain, 430f, 3535f), 3535f), Vector3.right);
            Transform room = level.Find("Maintenance/SalaDeMantenimiento");
            Marker(markers, "VHSPlaybackPoint", room.TransformPoint(VhsAnchor), -room.forward);
            Marker(markers, "Level1Door", new Vector3(SchoolX, GroundAt(terrain, SchoolX, 3465f), 3465f), Vector3.back);
            Marker(markers, "BuildingA_Scale0_65_Dim64x25x9m",
                new Vector3(SchoolX, GroundAt(terrain, SchoolX, SchoolZ), SchoolZ), Vector3.forward);
            Marker(markers, "RouteLength_" + Mathf.RoundToInt(road.Length) + "m",
                new Vector3(0f, 0f, 0f), Vector3.forward);
        }

        private static void Marker(Transform parent, string name, Vector3 position, Vector3 forward)
        {
            Transform marker = Group(SceneManager.GetActiveScene(), parent, name, position);
            if (forward.sqrMagnitude > 0.001f)
                marker.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static void AddRibbon(MeshBuilder mesh, PathData path, float width, TerrainGrid terrain = null)
        {
            for (int i = 0; i < path.Points.Count - 1; i++)
            {
                Vector3 a = path.Points[i];
                Vector3 b = path.Points[i + 1];
                Vector3 rightA = PathRight(path, i) * (width * 0.5f);
                Vector3 rightB = PathRight(path, i + 1) * (width * 0.5f);
                Vector3 aLeft = a - rightA, aRight = a + rightA;
                Vector3 bLeft = b - rightB, bRight = b + rightB;
                if (terrain != null)
                {
                    aLeft.y = Mathf.Max(a.y, GroundAt(terrain, aLeft.x, aLeft.z) + 0.10f);
                    aRight.y = Mathf.Max(a.y, GroundAt(terrain, aRight.x, aRight.z) + 0.10f);
                    bLeft.y = Mathf.Max(b.y, GroundAt(terrain, bLeft.x, bLeft.z) + 0.10f);
                    bRight.y = Mathf.Max(b.y, GroundAt(terrain, bRight.x, bRight.z) + 0.10f);
                }
                mesh.AddQuad(aLeft, bLeft, bRight, aRight);
            }
        }

        private static void CreateMeshObject(Transform parent, string assetPath, string name,
            MeshBuilder builder, Material material, bool collider, List<Vector2> uv = null)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh == null)
            {
                mesh = new Mesh { name = System.IO.Path.GetFileNameWithoutExtension(assetPath) };
                AssetDatabase.CreateAsset(mesh, assetPath);
            }
            mesh.Clear();
            if (builder.Vertices.Count > 65000)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(builder.Vertices);
            if (uv != null) mesh.SetUVs(0, uv);
            mesh.SetTriangles(builder.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssetIfDirty(mesh);

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
            if (collider)
                go.AddComponent<MeshCollider>().sharedMesh = mesh;
            go.isStatic = true;
        }

        private static GameObject CreatePrimitiveBlock(Transform parent, string name, Vector3 position,
            Vector3 size, Material material, bool collider = true)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = size;
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            if (!collider)
                Object.DestroyImmediate(cube.GetComponent<BoxCollider>());
            cube.isStatic = true;
            return cube;
        }

        private static GameObject Block(Transform parent, string name, Vector3 center, Vector3 size, Material material, bool solid = true)
        {
            return CreatePrimitiveBlock(parent, name, center, size, material, solid);
        }

        private static GameObject PlacePrefab(GameObject prefab, Transform parent, Vector3 position,
            float yaw, Vector3 scale, string name)
        {
            if (prefab == null)
                throw new InvalidOperationException("No se pudo cargar un prefab de nivel 00.");
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) instance = Object.Instantiate(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = scale;
            instance.isStatic = true;
            return instance;
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                throw new InvalidOperationException("No se encontró el prefab requerido: " + path);
            return prefab;
        }

        private static GameObject CreateAdaptedFencePrefab(Palette palette)
        {
            const string sourcePath = "Assets/ResourcePack/Cerca/Prefabs/CercaRejaMetalica.prefab";
            const string outputPath = PrefabFolder + "/PF_FencePanel_Level00.prefab";
            GameObject source = PrefabUtility.LoadPrefabContents(sourcePath);
            try
            {
                var combinedByMaterial = new Dictionary<Material, List<CombineInstance>>();
                Matrix4x4 rootInverse = source.transform.worldToLocalMatrix;
                foreach (MeshFilter filter in source.GetComponentsInChildren<MeshFilter>(true))
                {
                    MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer == null || filter.sharedMesh == null) continue;
                    Material[] materials = renderer.sharedMaterials;
                    int subMeshCount = Mathf.Min(filter.sharedMesh.subMeshCount, materials.Length);
                    for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
                    {
                        Material material = materials[subMesh];
                        if (material == null) continue;
                        List<CombineInstance> combines;
                        if (!combinedByMaterial.TryGetValue(material, out combines))
                        {
                            combines = new List<CombineInstance>();
                            combinedByMaterial.Add(material, combines);
                        }
                        combines.Add(new CombineInstance
                        {
                            mesh = filter.sharedMesh,
                            subMeshIndex = subMesh,
                            transform = rootInverse * filter.transform.localToWorldMatrix
                        });
                    }
                }

                if (combinedByMaterial.Count == 0)
                    throw new InvalidOperationException("El prefab de cerca no contiene mallas combinables.");

                var root = new GameObject("PF_FencePanel_Level00");
                int index = 0;
                foreach (KeyValuePair<Material, List<CombineInstance>> pair in combinedByMaterial.OrderBy(item => item.Key.name))
                {
                    var combined = new Mesh { name = "FencePanel_Level00_" + index.ToString("D2") };
                    combined.CombineMeshes(pair.Value.ToArray(), true, true, false);
                    string meshPath = ModelsFolder + "/FencePanel_Level00_" + index.ToString("D2") + ".asset";
                    Mesh stored = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    if (stored == null)
                    {
                        AssetDatabase.CreateAsset(combined, meshPath);
                        stored = combined;
                    }
                    else
                    {
                        EditorUtility.CopySerialized(combined, stored);
                        Object.DestroyImmediate(combined);
                        EditorUtility.SetDirty(stored);
                        AssetDatabase.SaveAssetIfDirty(stored);
                    }

                    var meshPart = new GameObject("Geometry_" + index.ToString("D2"));
                    meshPart.transform.SetParent(root.transform, false);
                    meshPart.AddComponent<MeshFilter>().sharedMesh = stored;
                    meshPart.AddComponent<MeshRenderer>().sharedMaterial = palette.Steel;
                    index++;
                }

                BoxCollider box = root.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 1f, 0f);
                box.size = new Vector3(2.91f, 2f, 0.2f);
                root.isStatic = true;
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, outputPath);
                Object.DestroyImmediate(root);
                AssetDatabase.SaveAssets();
                return prefab;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        private static void ApplyUniversalMaterials(GameObject instance)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material adapted = AdaptMaterialToUniversal(materials[i]);
                    if (adapted != materials[i]) changed = true;
                    materials[i] = adapted;
                }
                if (!changed) continue;
                renderer.sharedMaterials = materials;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }

        private static void ApplyMaterialOverride(GameObject instance, Material material)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++) materials[i] = material;
                renderer.sharedMaterials = materials;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }

        private static Material AdaptMaterialToUniversal(Material source)
        {
            if (source == null) return null;
            Shader universal = Shader.Find("Universal Render Pipeline/Lit");
            if (universal == null || source.shader == universal) return source;

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string identity = string.IsNullOrEmpty(sourcePath) ? source.name : AssetDatabase.AssetPathToGUID(sourcePath);
            if (identity.Length > 8) identity = identity.Substring(0, 8);
            string path = MaterialsFolder + "/M_URP_" + SafeAssetName(source.name) + "_" + identity + ".mat";
            Material result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null)
            {
                result = new Material(universal) { name = "M_URP_" + SafeAssetName(source.name) };
                AssetDatabase.CreateAsset(result, path);
            }

            result.shader = universal;
            Texture albedo = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") :
                             source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : source.mainTexture;
            if (albedo != null)
            {
                result.SetTexture("_BaseMap", albedo);
                string sourceTextureProperty = source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                if (source.HasProperty(sourceTextureProperty))
                {
                    result.SetTextureScale("_BaseMap", source.GetTextureScale(sourceTextureProperty));
                    result.SetTextureOffset("_BaseMap", source.GetTextureOffset(sourceTextureProperty));
                }
            }

            Color baseColor = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") :
                              source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
            result.SetColor("_BaseColor", baseColor);
            if (source.HasProperty("_BumpMap")) result.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
            if (source.HasProperty("_BumpScale")) result.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
            if (source.HasProperty("_MetallicGlossMap")) result.SetTexture("_MetallicGlossMap", source.GetTexture("_MetallicGlossMap"));
            if (source.HasProperty("_Metallic")) result.SetFloat("_Metallic", source.GetFloat("_Metallic"));
            if (source.HasProperty("_Glossiness")) result.SetFloat("_Smoothness", source.GetFloat("_Glossiness"));
            if (source.HasProperty("_EmissionMap")) result.SetTexture("_EmissionMap", source.GetTexture("_EmissionMap"));
            if (source.HasProperty("_EmissionColor")) result.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));

            float mode = source.HasProperty("_Mode") ? source.GetFloat("_Mode") : 0f;
            bool alphaClip = mode == 1f || source.IsKeywordEnabled("_ALPHATEST_ON");
            result.SetFloat("_AlphaClip", alphaClip ? 1f : 0f);
            if (alphaClip)
            {
                result.EnableKeyword("_ALPHATEST_ON");
                if (source.HasProperty("_Cutoff")) result.SetFloat("_Cutoff", source.GetFloat("_Cutoff"));
            }
            if (mode >= 2f)
            {
                result.SetFloat("_Surface", 1f);
                result.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                result.renderQueue = (int)RenderQueue.Transparent;
            }
            if (source.IsKeywordEnabled("_EMISSION")) result.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(result);
            AssetDatabase.SaveAssetIfDirty(result);
            return result;
        }

        private static string SafeAssetName(string name)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return name.Replace(' ', '_').Replace('/', '_').Replace('\\', '_');
        }

        private static Bounds PrefabLocalBounds(GameObject prefab)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) instance = Object.Instantiate(prefab);
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            bool initialized = false;
            Bounds result = new Bounds();
            foreach (Renderer renderer in renderers)
            {
                Bounds bounds = renderer.bounds;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 point = instance.transform.InverseTransformPoint(
                        center + Vector3.Scale(extents, new Vector3(x, y, z)));
                    if (!initialized)
                    {
                        result = new Bounds(point, Vector3.zero);
                        initialized = true;
                    }
                    else result.Encapsulate(point);
                }
            }
            Object.DestroyImmediate(instance);
            return result;
        }

        private static void OpenGate(GameObject gate, string leftName, float leftAngle, string rightName, float rightAngle)
        {
            Transform left = gate.transform.Find("PortonVehicular_Root/" + leftName);
            if (left == null) left = gate.transform.Find(leftName);
            if (left != null)
            {
                left.localRotation = Quaternion.Euler(0f, leftAngle, 0f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(left);
            }

            if (string.IsNullOrEmpty(rightName)) return;
            Transform right = gate.transform.Find("PortonVehicular_Root/" + rightName);
            if (right == null) right = gate.transform.Find(rightName);
            if (right != null)
            {
                right.localRotation = Quaternion.Euler(0f, rightAngle, 0f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(right);
            }
        }

        private static void DisableRootInteraction(GameObject instance)
        {
            foreach (MonoBehaviour behaviour in instance.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null) continue;
                if (behaviour.GetType().Name.IndexOf("Interactable", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    behaviour.enabled = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
                }
            }
        }

        private static void Label(Transform parent, Vector3 position, string text, float width, float height, Color color, float characterSize)
        {
            Transform node = Group(SceneManager.GetActiveScene(), parent, text, position);
            var label = node.gameObject.AddComponent<TextMesh>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 128;
            label.fontStyle = FontStyle.Bold;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = characterSize;
            label.color = color;
            label.text = text;
            HorrorCorridorEnvironment.ConfigureSignText(label);
            MeshRenderer renderer = node.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Bounds bounds = renderer.localBounds;
            if (bounds.size.x > 0f && bounds.size.y > 0f)
                node.localScale = Vector3.one * Mathf.Min(width / bounds.size.x, height / bounds.size.y);
        }

        private static void Label(Transform parent, Vector3 position, string text, float width, float height, Material material, float characterSize)
        {
            Color color = material != null && material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : material != null && material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            Label(parent, position, text, width, height, color, characterSize);
        }

        private static float GroundAt(TerrainGrid terrain, float x, float z)
        {
            return terrain.HeightAt(x, z);
        }

        private static Vector3 FlatDirection(Vector3 a, Vector3 b)
        {
            Vector3 direction = b - a;
            direction.y = 0f;
            return direction.sqrMagnitude < 0.0001f ? Vector3.forward : direction.normalized;
        }

        private static float Smooth(float from, float to, float value)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string name = path.Substring(separator + 1);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static Material CreateMaterial(string name, Color color, string shaderName, float smoothness)
        {
            string path = TerrainFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(shaderName);
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private sealed class Palette
        {
            public readonly Material Asphalt;
            public readonly Material Shoulder;
            public readonly Material Paint;
            public readonly Material Barrier;
            public readonly Material Reflector;
            public readonly Material Concrete;
            public readonly Material Wall;
            public readonly Material Roof;
            public readonly Material Steel;
            public readonly Material Sign;
            public readonly Material SignText;
            public readonly Material Door;
            public readonly Material DirtPath;

            public Palette()
            {
                Asphalt = CreateMaterial("M_Level00_Asphalt", new Color32(45, 48, 48, 255), "Universal Render Pipeline/Lit", 0.12f);
                Shoulder = CreateMaterial("M_Level00_GravelShoulder", new Color32(91, 83, 65, 255), "Universal Render Pipeline/Lit", 0.05f);
                Paint = CreateMaterial("M_Level00_RoadPaint", new Color32(226, 221, 198, 255), "Universal Render Pipeline/Lit", 0.04f);
                Barrier = CreateMaterial("M_Level00_ConcreteBarrier", new Color32(119, 119, 108, 255), "Universal Render Pipeline/Lit", 0.12f);
                Reflector = CreateMaterial("M_Level00_Reflector", new Color32(244, 190, 74, 255), "Universal Render Pipeline/Lit", 0.18f);
                Concrete = CreateMaterial("M_Level00_Concrete", new Color32(142, 140, 126, 255), "Universal Render Pipeline/Lit", 0.12f);
                Wall = CreateMaterial("M_Level00_MaintenanceWall", new Color32(185, 175, 153, 255), "Universal Render Pipeline/Lit", 0.1f);
                Roof = CreateMaterial("M_Level00_Roof", new Color32(54, 61, 62, 255), "Universal Render Pipeline/Lit", 0.17f);
                Steel = CreateMaterial("M_Level00_Steel", new Color32(48, 69, 72, 255), "Universal Render Pipeline/Lit", 0.35f);
                Sign = CreateMaterial("M_Level00_Sign", new Color32(53, 82, 72, 255), "Universal Render Pipeline/Lit", 0.08f);
                SignText = CreateMaterial("M_Level00_SignText", new Color32(245, 231, 190, 255), "Universal Render Pipeline/Lit", 0f);
                Door = CreateMaterial("M_Level00_Door", new Color32(81, 71, 59, 255), "Universal Render Pipeline/Lit", 0.06f);
                DirtPath = CreateMaterial("M_Level00_DirtPath", new Color32(123, 106, 77, 255), "Universal Render Pipeline/Lit", 0.02f);
            }
        }

        private sealed class PathData
        {
            public readonly List<Vector3> Points;
            private readonly float[] distances;
            public float Length { get { return distances[distances.Length - 1]; } }

            public PathData(IList<Vector3> points)
            {
                Points = new List<Vector3>(points);
                distances = new float[Points.Count];
                for (int i = 1; i < Points.Count; i++)
                    distances[i] = distances[i - 1] + Vector3.Distance(Points[i - 1], Points[i]);
            }

            public Vector3 AtDistance(float distance)
            {
                distance = Mathf.Clamp(distance, 0f, Length);
                int low = 0;
                int high = distances.Length - 1;
                while (high - low > 1)
                {
                    int mid = (low + high) / 2;
                    if (distances[mid] < distance) low = mid;
                    else high = mid;
                }
                float span = distances[high] - distances[low];
                float t = span <= Mathf.Epsilon ? 0f : (distance - distances[low]) / span;
                return Vector3.Lerp(Points[low], Points[high], t);
            }

        }

        private sealed class MeshBuilder
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int> Triangles = new List<int>();

            public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int start = Vertices.Count;
                Vertices.Add(a);
                Vertices.Add(b);
                Vertices.Add(c);
                Vertices.Add(d);
                Triangles.Add(start);
                Triangles.Add(start + 1);
                Triangles.Add(start + 2);
                Triangles.Add(start);
                Triangles.Add(start + 2);
                Triangles.Add(start + 3);
            }
        }
    }
}
