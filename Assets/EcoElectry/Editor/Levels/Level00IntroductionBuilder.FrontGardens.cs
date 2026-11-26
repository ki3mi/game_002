using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string FrontGardenGroupName = "Jardines del frontis";

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar jardines del frontis")]
        internal static void RefreshFrontGardens()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para actualizar los jardines.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), item => item.name == RootName);
            Transform school = level != null ? level.transform.Find("SchoolExterior") : null;
            Transform terrain = level != null ? level.transform.Find("Terrain") : null;
            if (school == null || terrain == null)
                throw new InvalidOperationException("El nivel 00 necesita el edificio y el terreno del campus.");

            GameObject fence = LoadPrefab(PrefabFolder + "/PF_FencePanel_Level00.prefab");
            BuildFrontGardens(school, TerrainGrid.FromRoot(terrain), fence);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudieron guardar los jardines del frontis.");
            AssetDatabase.SaveAssets();
        }

        private static void BuildFrontGardens(Transform school, TerrainGrid terrain, GameObject fence)
        {
            Transform building = school.Find("BuildingA");
            MeshCollider facade = building != null ? building.GetComponentInChildren<MeshCollider>() : null;
            if (facade == null)
                throw new InvalidOperationException("Falta la colisión de la fachada del edificio A.");

            // The two long boxes on A_Tou enclose the empty recesses as well as the wings.
            BoxCollider[] boxes = building.GetComponentsInChildren<BoxCollider>(true)
                .Where(box => box.size.z > 30f)
                .OrderBy(box => box.transform.TransformPoint(box.center).x)
                .ToArray();
            if (boxes.Length != 2)
                throw new InvalidOperationException("No se encontraron las dos cajas de los entrantes de A_Tou.");

            Physics.SyncTransforms();
            var west = MeasureFrontGarden(facade, terrain, WorldBoxBounds(boxes[0]), true);
            var east = MeasureFrontGarden(facade, terrain, WorldBoxBounds(boxes[1]), false);
            Bounds panel = PrefabLocalBounds(fence);
            Transform previous = school.Find(FrontGardenGroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform gardens = Group(school.gameObject.scene, school, FrontGardenGroupName);
            Terrain terrainTile = terrain.At(
                Mathf.FloorToInt((west[0].center - TerrainX) / TileSize),
                Mathf.FloorToInt((west[0].z - TerrainZ) / TileSize));
            Material sod = CreateMaterial("M_Level00_CespedFrontis", new Color32(187, 194, 172, 255),
                "Universal Render Pipeline/Lit", 0.10f);
            TerrainLayer lawn = terrainTile.terrainData.terrainLayers.FirstOrDefault();
            sod.SetTexture("_BaseMap", lawn != null ? lawn.diffuseTexture : null);
            EditorUtility.SetDirty(sod);

            var plants = new NatureCampusPlants();
            GameObject tree = LoadPrefab(FantasyTreePath);
            BuildFrontGarden(gardens, terrain, west, fence, panel, sod, plants, tree, "Oeste");
            BuildFrontGarden(gardens, terrain, east, fence, panel, sod, plants, tree, "Este");

            foreach (BoxCollider box in boxes)
            {
                box.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(box);
            }
        }

        private static Bounds WorldBoxBounds(BoxCollider box)
        {
            var bounds = new Bounds(box.transform.TransformPoint(box.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
                bounds.Encapsulate(box.transform.TransformPoint(box.center +
                    Vector3.Scale(box.size * 0.5f, new Vector3(x, y, z))));
            return bounds;
        }

        private static List<(float min, float max, float center, float z)> MeasureFrontGarden(
            MeshCollider facade, TerrainGrid terrain, Bounds region, bool west)
        {
            float front = region.min.z - 0.13f;
            float depth = Mathf.Clamp(region.size.x * 0.73f, 5.2f, 10.6f);
            float inner = west ? region.max.x : region.min.x;
            float direction = west ? -1f : 1f;
            float top = facade.bounds.max.y + 1f;
            var rows = new List<(float min, float max, float center, float z)>();
            for (int row = 0; row <= 10; row++)
            {
                float z = front + depth * row / 10f;
                float length = region.size.x;
                for (float d = 0f; d <= region.size.x; d += 0.24f)
                {
                    float x = inner + direction * d;
                    if (!facade.Raycast(new Ray(new Vector3(x, top, z), Vector3.down), out RaycastHit hit, 50f) ||
                        hit.point.y < terrain.HeightAt(x, z) + 0.45f) continue;
                    length = d - 0.44f;
                    break;
                }
                if (length < 3.3f) break;
                float outer = inner + direction * length;
                rows.Add((Mathf.Min(inner, outer), Mathf.Max(inner, outer), (inner + outer) * 0.5f, z));
            }
            if (rows.Count < 4)
                throw new InvalidOperationException("El entrante de la fachada no deja espacio para el jardín.");
            return rows;
        }

        private static void BuildFrontGarden(Transform gardens, TerrainGrid terrain,
            List<(float min, float max, float center, float z)> rows, GameObject fence, Bounds panel,
            Material sod, NatureCampusPlants plants, GameObject tree, string side)
        {
            Transform garden = Group(gardens.gameObject.scene, gardens, "Jardín " + side);
            const int across = 22;
            int along = (rows.Count - 1) * 2;
            var mesh = new MeshBuilder();
            var uv = new List<Vector2>();
            for (int z = 0; z <= along; z++)
            for (int x = 0; x <= across; x++)
            {
                Vector3 point = GardenGround(terrain, rows, (float)x / across, (float)z / along);
                mesh.Vertices.Add(point);
                uv.Add(new Vector2(point.x * 0.53f, point.z * 0.53f));
                if (x == 0 || z == 0) continue;
                int last = mesh.Vertices.Count - 1;
                int previousRow = last - across - 1;
                mesh.Triangles.Add(previousRow - 1);
                mesh.Triangles.Add(last - 1);
                mesh.Triangles.Add(last);
                mesh.Triangles.Add(previousRow - 1);
                mesh.Triangles.Add(last);
                mesh.Triangles.Add(previousRow);
            }
            CreateMeshObject(garden, ModelsFolder + "/Level00_RelieveFrontis_" + side + ".asset",
                "Relieve de césped", mesh, sod, true, uv);

            Transform enclosure = Group(garden.gameObject.scene, garden, "Cerca ornamental");
            var outline = new List<Vector2>(rows.Select(row => new Vector2(row.min, row.z)));
            for (int i = rows.Count - 1; i >= 0; i--)
                outline.Add(new Vector2(rows[i].max, rows[i].z));
            for (int i = outline.Count - 2; i >= 1; i--)
            {
                Vector2 prior = (outline[i] - outline[i - 1]).normalized;
                Vector2 next = (outline[i + 1] - outline[i]).normalized;
                if (Vector2.Dot(prior, next) > 0.999f) outline.RemoveAt(i);
            }
            for (int i = 0; i < outline.Count; i++)
                FenceGardenEdge(enclosure, terrain, fence, panel, outline[i], outline[(i + 1) % outline.Count]);

            Transform planting = Group(garden.gameObject.scene, garden, "Árboles y sotobosque");
            bool west = side == "Oeste";
            PlaceGardenTree(0.34f, 0.29f, 0.32f, west ? 37f : 174f);
            PlaceGardenTree(0.71f, 0.69f, 0.24f, west ? 121f : 68f);
            var random = new System.Random(west ? 361 : 724);
            for (int i = 0; i < 15; i++)
            {
                float u = Mathf.Lerp(0.12f, 0.88f, (float)random.NextDouble());
                float v = Mathf.Lerp(0.13f, 0.90f, (float)random.NextDouble());
                Vector3 point = GardenGround(terrain, rows, u, v);
                var set = i % 3 == 0 ? plants.Shrubs : i % 3 == 1 ? plants.Flowers : plants.Grass;
                set.Place(planting, point, Quaternion.Euler(0f, 47f * i, 0f), 1f);
            }
            void PlaceGardenTree(float u, float v, float scale, float yaw)
            {
                Vector3 point = GardenGround(terrain, rows, u, v);
                GameObject instance = PlacePrefab(tree, planting, point, yaw, Vector3.one * scale, "Árbol de jardín");
                DisablePlantColliders(instance);
            }
        }

        private static Vector3 GardenGround(TerrainGrid terrain,
            List<(float min, float max, float center, float z)> rows, float u, float v)
        {
            float row = v * (rows.Count - 1);
            int index = Mathf.Min(Mathf.FloorToInt(row), rows.Count - 2);
            var a = rows[index];
            var b = rows[index + 1];
            float x = Mathf.Lerp(Mathf.Lerp(a.min, a.max, u), Mathf.Lerp(b.min, b.max, u), row - index);
            float z = Mathf.Lerp(a.z, b.z, row - index);
            float envelope = Mathf.Sin(u * Mathf.PI) * Mathf.Sin(v * Mathf.PI);
            float slope = 0.72f + 0.76f * Mathf.PerlinNoise(x * 0.31f, z * 0.29f);
            return new Vector3(x, terrain.HeightAt(x, z) + 0.026f + envelope * envelope * slope, z);
        }

        private static void FenceGardenEdge(Transform enclosure, TerrainGrid terrain,
            GameObject fence, Bounds panel, Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float length = direction.magnitude;
            int count = Mathf.Max(1, Mathf.CeilToInt(length / panel.size.x));
            float yaw = -Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            for (int i = 0; i < count; i++)
            {
                Vector2 center = Vector2.Lerp(start, end, (i + 0.5f) / count);
                PlacePrefab(fence, enclosure, new Vector3(center.x, terrain.HeightAt(center.x, center.y), center.y),
                    yaw, new Vector3((length / count + 0.025f) / panel.size.x, 0.60f, 1f), "Panel de jardín");
            }
        }
    }
}
