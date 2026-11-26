using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string FantasyTerrainPath =
            "Assets/Fantasy Forest Environment Free Sample/Scenes/New Terrain.asset";
        private const string UnderstoryName = "Sotobosque";
        private const float TileSize = 300f;
        private const float TerrainX = -480f;
        private const float TerrainZ = -100f;
        private const float TerrainY = -8f;
        private const float TerrainHeight = 40f;
        private const int TileColumns = 4;
        private const int TileRows = 13;
        private const int HeightResolution = 129;
        private const int SurfaceResolution = 128;

        private static TerrainGrid CreateTerrain(Transform parent, Transform forest, PathData road)
        {
            TerrainData source = AssetDatabase.LoadAssetAtPath<TerrainData>(FantasyTerrainPath);
            if (source == null)
                throw new InvalidOperationException("No se encontró el TerrainData de demoScene_free.");

            var grid = new TerrainGrid();
            var trail = BuildPath(ForestTrailControls, 2f);
            var demo = new DemoTerrain(source);
            Material material = CreateMaterial("M_Level00_Terrain", Color.white, "Universal Render Pipeline/Terrain/Lit", 0f);

            for (int row = 0; row < TileRows; row++)
            for (int column = 0; column < TileColumns; column++)
            {
                string name = $"FantasyForest_{column:00}_{row:00}";
                string assetName = column == 0 && row == 0 ? "Level00TerrainData" : name;
                string path = TerrainFolder + "/" + assetName + ".asset";
                TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
                if (data == null)
                {
                    data = Object.Instantiate(source);
                    data.name = assetName;
                    AssetDatabase.CreateAsset(data, path);
                }

                data.heightmapResolution = HeightResolution;
                data.alphamapResolution = SurfaceResolution;
                data.size = new Vector3(TileSize, TerrainHeight, TileSize);
                data.terrainLayers = source.terrainLayers;
                data.detailPrototypes = source.detailPrototypes;
                data.SetDetailResolution(SurfaceResolution, 16);
                data.SetHeights(0, 0, TileHeights(demo, road, column, row));
                PaintTile(data, demo, road, trail, column, row);
                data.SetTreeInstances(Array.Empty<TreeInstance>(), false);
                data.treePrototypes = source.treePrototypes;
                data.SetTreeInstances(TileTrees(demo, road, trail, column, row), true);
                EditorUtility.SetDirty(data);

                GameObject tile = Terrain.CreateTerrainGameObject(data);
                tile.name = name;
                tile.transform.SetParent(parent, false);
                tile.transform.position = new Vector3(TerrainX + column * TileSize, TerrainY, TerrainZ + row * TileSize);
                tile.isStatic = true;
                Terrain terrain = tile.GetComponent<Terrain>();
                terrain.materialTemplate = material;
                terrain.drawInstanced = true;
                terrain.heightmapPixelError = 3f;
                terrain.basemapDistance = 180f;
                terrain.detailObjectDistance = 85f;
                terrain.treeDistance = 740f;
                terrain.treeBillboardDistance = 110f;
                terrain.allowAutoConnect = false;
                grid.Tiles[column, row] = terrain;
            }

            for (int row = 0; row < TileRows; row++)
            for (int column = 0; column < TileColumns; column++)
                grid.Tiles[column, row].SetNeighbors(grid.At(column - 1, row), grid.At(column, row + 1),
                    grid.At(column + 1, row), grid.At(column, row - 1));

            // The terrain tiles own the forest instances so Unity can batch and cull them.
            Group(SceneManager.GetActiveScene(), forest, "Vegetación integrada en los terrenos Fantasy Forest");
            BuildUnderstory(forest, grid, road, trail);
            return grid;
        }

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar sotobosque")]
        internal static void RefreshUnderstory()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para actualizar el sotobosque.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            var forest = level != null ? level.transform.Find("Forest") : null;
            var terrainRoot = level != null ? level.transform.Find("Terrain") : null;
            if (forest == null || terrainRoot == null)
                throw new InvalidOperationException("El nivel 00 debe tener montado el terreno de Fantasy Forest.");

            TerrainGrid grid = TerrainGrid.FromRoot(terrainRoot);
            BuildUnderstory(forest, grid, BuildPath(RoadControls, 4f), BuildPath(ForestTrailControls, 2f));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildUnderstory(Transform forest, TerrainGrid grid, PathData road, PathData trail)
        {
            GameObject tree = LoadPrefab(FantasyTreePath);
            float crownTop = PrefabLocalBounds(tree).max.y;
            var existing = forest.Find(UnderstoryName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            Transform understory = Group(forest.gameObject.scene, forest, UnderstoryName);

            var random = new System.Random(361);
            for (int i = 0; i < 13; i++)
                PlaceBuriedTree(understory, grid, tree, crownTop, road, trail, trail, 37f + i * 13f,
                    (i % 2 == 0 ? 1f : -1f) * Mathf.Lerp(4.8f, 8.2f, (float)random.NextDouble()), random);

            foreach (float distance in new[] { 42f, 66f, 93f, 136f, 190f, 2810f, 3070f, 3220f })
                PlaceBuriedTree(understory, grid, tree, crownTop, road, trail, road, distance,
                    (random.NextDouble() < 0.5 ? 1f : -1f) * Mathf.Lerp(10.5f, 15f, (float)random.NextDouble()), random);
        }

        private static void PlaceBuriedTree(Transform parent, TerrainGrid grid, GameObject tree, float crownTop,
            PathData road, PathData trail, PathData corridor, float distance, float offset, System.Random random)
        {
            Vector3 tangent = FlatDirection(corridor.AtDistance(distance - 1f), corridor.AtDistance(distance + 1f));
            Vector3 point = corridor.AtDistance(distance) + Vector3.Cross(Vector3.up, tangent) * offset;
            if (!VegetationAllowed(point.x, point.z, road, trail, 2.8f)) return;

            float width = Mathf.Lerp(0.17f, 0.28f, (float)random.NextDouble());
            float height = Mathf.Lerp(0.23f, 0.37f, (float)random.NextDouble());
            float exposed = Mathf.Lerp(0.65f, 1.4f, (float)random.NextDouble());
            point.y = grid.HeightAt(point.x, point.z) + exposed - crownTop * height;
            GameObject shrub = PlacePrefab(tree, parent, point, (float)random.NextDouble() * 360f,
                new Vector3(width, height, width), $"Copa_semienterrada_{parent.childCount + 1:00}");
            foreach (Collider collider in shrub.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
            }
        }

        private static float[,] TileHeights(DemoTerrain demo, PathData road, int column, int row)
        {
            var heights = new float[HeightResolution, HeightResolution];
            int intervals = HeightResolution - 1;
            for (int z = 0; z <= intervals; z++)
            {
                float v = Mirror(row, (float)z / intervals);
                float worldZ = TerrainZ + (row * intervals + z) * TileSize / intervals;
                Vector3 roadPoint = RoadAtZ(road, worldZ);
                for (int x = 0; x <= intervals; x++)
                {
                    float u = Mirror(column, (float)x / intervals);
                    float worldX = TerrainX + (column * intervals + x) * TileSize / intervals;
                    float forestHeight = 5f + demo.Source.GetInterpolatedHeight(u, v);
                    float campus = 1f - Smooth(0f, 62f, CampusDistance(worldX, worldZ));
                    float height = Mathf.Lerp(forestHeight, CampusGrade, campus);
                    float corridor = 1f - Smooth(10f, 35f, Mathf.Abs(worldX - roadPoint.x));
                    corridor *= Smooth(-20f, -7f, worldZ) * (1f - Smooth(GateZ, GateZ + 2f, worldZ));
                    height = Mathf.Lerp(height, roadPoint.y - 0.17f, corridor);
                    heights[z, x] = Mathf.Clamp01((height - TerrainY) / TerrainHeight);
                }
            }
            return heights;
        }

        private static void PaintTile(TerrainData data, DemoTerrain demo, PathData road, PathData trail, int column, int row)
        {
            var alpha = new float[SurfaceResolution, SurfaceResolution, demo.Source.alphamapLayers];
            var details = new int[demo.Grass.Length][,];
            for (int layer = 0; layer < details.Length; layer++)
                details[layer] = new int[SurfaceResolution, SurfaceResolution];

            for (int z = 0; z < SurfaceResolution; z++)
            {
                float v = Mirror(row, (z + 0.5f) / SurfaceResolution);
                float worldZ = TerrainZ + (row + (z + 0.5f) / SurfaceResolution) * TileSize;
                for (int x = 0; x < SurfaceResolution; x++)
                {
                    float u = Mirror(column, (x + 0.5f) / SurfaceResolution);
                    float worldX = TerrainX + (column + (x + 0.5f) / SurfaceResolution) * TileSize;
                    int sourceX = Mathf.FloorToInt(u * demo.Source.alphamapWidth);
                    int sourceZ = Mathf.FloorToInt(v * demo.Source.alphamapHeight);
                    for (int layer = 0; layer < demo.Source.alphamapLayers; layer++)
                        alpha[z, x, layer] = demo.Paint[sourceZ, sourceX, layer];

                    // Leave a margin for detail quads and random placement inside each cell.
                    if (!VegetationAllowed(worldX, worldZ, road, trail, 3f)) continue;
                    int detailX = Mathf.FloorToInt(u * SurfaceResolution);
                    int detailZ = Mathf.FloorToInt(v * SurfaceResolution);
                    for (int layer = 0; layer < details.Length; layer++)
                        details[layer][z, x] = demo.Grass[layer][detailZ, detailX];
                }
            }
            data.SetAlphamaps(0, 0, alpha);
            for (int layer = 0; layer < details.Length; layer++)
                data.SetDetailLayer(0, 0, layer, details[layer]);
        }

        private static TreeInstance[] TileTrees(DemoTerrain demo, PathData road, PathData trail, int column, int row)
        {
            const int repeats = 3;
            var trees = new List<TreeInstance>();
            var random = new System.Random(26011 + row * TileColumns + column);
            for (int z = 0; z < repeats; z++)
            for (int x = 0; x < repeats; x++)
            {
                foreach (TreeInstance original in demo.Trees)
                {
                    float u = (x + Mirror(x + column, original.position.x)) / repeats;
                    float v = (z + Mirror(z + row, original.position.z)) / repeats;
                    float worldX = TerrainX + (column + u) * TileSize;
                    float worldZ = TerrainZ + (row + v) * TileSize;
                    if (random.NextDouble() > 0.83 || !VegetationAllowed(worldX, worldZ, road, trail, 6f)) continue;
                    var tree = original;
                    tree.position = new Vector3(u, 0f, v);
                    tree.rotation = (float)random.NextDouble() * Mathf.PI * 2f;
                    float scale = Mathf.Lerp(0.82f, 1.28f, (float)random.NextDouble());
                    tree.heightScale *= scale;
                    tree.widthScale *= scale;
                    trees.Add(tree);
                }
            }
            return trees.ToArray();
        }

        private static bool VegetationAllowed(float x, float z, PathData road, PathData trail, float clearance)
        {
            if (z >= -10f - clearance && z <= GateZ + clearance &&
                Mathf.Abs(x - RoadAtZ(road, z).x) < BarrierOffset + clearance + 1f)
                return false;
            if (CampusDistance(x, z) <= clearance) return false;

            Vector2 point = new Vector2(x, z);
            float trailMargin = clearance + 1.4f;
            if (z < ForestTrailControls[0].z - trailMargin || z > ForestTrailControls[^1].z + trailMargin ||
                x < ForestTrailControls[0].x - trailMargin || x > ForestTrailControls[^1].x + trailMargin)
                return true;
            for (int i = 1; i < trail.Points.Count; i++)
            {
                var a = new Vector2(trail.Points[i - 1].x, trail.Points[i - 1].z);
                var b = new Vector2(trail.Points[i].x, trail.Points[i].z);
                Vector2 ab = b - a;
                Vector2 closest = a + ab * Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude);
                if ((point - closest).sqrMagnitude < trailMargin * trailMargin) return false;
            }
            return true;
        }

        private static float CampusDistance(float x, float z)
        {
            float dx = Mathf.Max(0f, Mathf.Max(156f - x, x - 285f));
            float dz = Mathf.Max(0f, Mathf.Max(3381f - z, z - 3545f));
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static float Mirror(int index, float coordinate) => index % 2 == 0 ? coordinate : 1f - coordinate;

        private sealed class DemoTerrain
        {
            public readonly TerrainData Source;
            public readonly float[,,] Paint;
            public readonly int[][,] Grass;
            public readonly TreeInstance[] Trees;

            public DemoTerrain(TerrainData source)
            {
                Source = source;
                Paint = source.GetAlphamaps(0, 0, source.alphamapWidth, source.alphamapHeight);
                Trees = source.treeInstances;
                Grass = new int[source.detailPrototypes.Length][,];
                int resolution = source.detailResolution;
                for (int layer = 0; layer < Grass.Length; layer++)
                {
                    int[,] original = source.GetDetailLayer(0, 0, resolution, resolution, layer);
                    var reduced = new int[SurfaceResolution, SurfaceResolution];
                    for (int z = 0; z < resolution; z++)
                    for (int x = 0; x < resolution; x++)
                    {
                        int row = z * SurfaceResolution / resolution;
                        int column = x * SurfaceResolution / resolution;
                        reduced[row, column] = Mathf.Max(reduced[row, column], original[z, x]);
                    }
                    Grass[layer] = reduced;
                }
            }
        }

        private sealed class TerrainGrid
        {
            public readonly Terrain[,] Tiles = new Terrain[TileColumns, TileRows];

            public static TerrainGrid FromRoot(Transform root)
            {
                var grid = new TerrainGrid();
                foreach (Terrain tile in root.GetComponentsInChildren<Terrain>())
                {
                    int column = Mathf.RoundToInt((tile.transform.position.x - TerrainX) / TileSize);
                    int row = Mathf.RoundToInt((tile.transform.position.z - TerrainZ) / TileSize);
                    if (column >= 0 && column < TileColumns && row >= 0 && row < TileRows)
                        grid.Tiles[column, row] = tile;
                }
                return grid;
            }

            public Terrain At(int column, int row) =>
                column >= 0 && column < TileColumns && row >= 0 && row < TileRows ? Tiles[column, row] : null;

            public float HeightAt(float x, float z)
            {
                int column = Mathf.FloorToInt((x - TerrainX) / TileSize);
                int row = Mathf.FloorToInt((z - TerrainZ) / TileSize);
                Terrain terrain = At(column, row);
                if (terrain == null) throw new ArgumentOutOfRangeException(nameof(x), $"Punto fuera del terreno: ({x}, {z}).");
                return terrain.transform.position.y + terrain.SampleHeight(new Vector3(x, 0f, z));
            }
        }
    }
}
