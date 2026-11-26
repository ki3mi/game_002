using System;
using System.Collections.Generic;
using System.Linq;
using EcoElectry.Unity.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string ChunkGeometryName = "ChunkGeometry";
        private const string ChunkMeshFolder = TerrainFolder + "/Chunks";

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar chunks de renderizado")]
        internal static void RefreshChunkRendering()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para preparar sus chunks.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            if (level == null) throw new InvalidOperationException("No se encontró la escena generada del nivel 00.");

            ConfigureChunkRendering(level.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudo guardar el renderizado por chunks.");
        }

        private static void ConfigureChunkRendering(Transform level)
        {
            Transform player = level.Find(CabRigName + "/" + DriveTruckName) ?? level.Find("Player");
            Transform terrain = level.Find("Terrain");
            if (player == null || terrain == null)
                throw new InvalidOperationException("El renderizado por chunks requiere el terreno y el personaje.");

            Transform[] groups = new[] { "Highway", "Forest", "SchoolExterior", "Maintenance", "ForestTrail" }
                .Select(level.Find).Where(group => group != null).ToArray();
            Transform previous = level.Find(ChunkGeometryName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform geometry = Group(level.gameObject.scene, level, ChunkGeometryName);
            EnsureFolder(ChunkMeshFolder);
            var cells = new Dictionary<Vector2Int, Transform>();

            foreach (Transform group in groups)
            foreach (MeshFilter filter in group.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null || !AssetDatabase.GetAssetPath(mesh).StartsWith(MeshFolder + "/", StringComparison.Ordinal))
                    continue;
                if (!filter.TryGetComponent(out MeshRenderer renderer)) continue;

                Bounds bounds = renderer.bounds;
                Vector3 insideMax = Vector3.Max(bounds.min, bounds.max - new Vector3(0.001f, 0f, 0.001f));
                if (RenderCell(bounds.min) == RenderCell(insideMax))
                {
                    renderer.enabled = true;
                    continue;
                }

                // The mesh collider stays on this object. Only the visible surface is partitioned.
                BakeChunkPieces(filter, renderer, geometry, cells);
                renderer.enabled = false;
            }

            ChunkRenderWindow window = level.GetComponent<ChunkRenderWindow>();
            if (window == null) window = level.gameObject.AddComponent<ChunkRenderWindow>();
            var data = new SerializedObject(window);
            data.FindProperty("player").objectReferenceValue = player;
            data.FindProperty("terrainRoot").objectReferenceValue = terrain;
            data.FindProperty("origin").vector2Value = new Vector2(TerrainX, TerrainZ);
            data.FindProperty("chunkSize").floatValue = TileSize;
            SerializedProperty scenery = data.FindProperty("sceneryRoots");
            scenery.arraySize = groups.Length + 1;
            for (int i = 0; i < groups.Length; i++)
                scenery.GetArrayElementAtIndex(i).objectReferenceValue = groups[i];
            scenery.GetArrayElementAtIndex(groups.Length).objectReferenceValue = geometry;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Vector2Int RenderCell(Vector3 position) => new Vector2Int(
            Mathf.FloorToInt((position.x - TerrainX) / TileSize),
            Mathf.FloorToInt((position.z - TerrainZ) / TileSize));

        private static Vector3 CellOrigin(Vector2Int cell) =>
            new Vector3(TerrainX + cell.x * TileSize, 0f, TerrainZ + cell.y * TileSize);

        private static void BakeChunkPieces(MeshFilter source, MeshRenderer renderer, Transform geometry,
            Dictionary<Vector2Int, Transform> cells)
        {
            var pieces = new Dictionary<Vector2Int, MeshBuilder>();
            Vector3[] vertices = source.sharedMesh.vertices;
            int[] triangles = source.sharedMesh.triangles;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = source.transform.TransformPoint(vertices[i]);

            var polygon = new List<Vector3>(8);
            var scratch = new List<Vector3>(8);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                Vector2Int first = RenderCell(Vector3.Min(a, Vector3.Min(b, c)));
                Vector2Int last = RenderCell(Vector3.Max(a, Vector3.Max(b, c)));
                for (int z = first.y; z <= last.y; z++)
                for (int x = first.x; x <= last.x; x++)
                {
                    var cell = new Vector2Int(x, z);
                    Vector3 origin = CellOrigin(cell);
                    polygon.Clear();
                    polygon.Add(a);
                    polygon.Add(b);
                    polygon.Add(c);
                    ClipPolygon(polygon, scratch, 0, origin.x, true);
                    ClipPolygon(scratch, polygon, 0, origin.x + TileSize, false);
                    ClipPolygon(polygon, scratch, 2, origin.z, true);
                    ClipPolygon(scratch, polygon, 2, origin.z + TileSize, false);
                    if (polygon.Count < 3) continue;
                    if (!pieces.TryGetValue(cell, out MeshBuilder piece))
                    {
                        piece = new MeshBuilder();
                        pieces.Add(cell, piece);
                    }
                    AppendPolygon(piece, polygon, origin);
                }
            }

            foreach (var pair in pieces)
            {
                if (pair.Value.Triangles.Count == 0) continue;
                Vector2Int cell = pair.Key;
                string suffix = $"_{cell.x:00}_{cell.y:00}";
                if (!cells.TryGetValue(cell, out Transform parent))
                {
                    parent = Group(geometry.gameObject.scene, geometry, "Chunk" + suffix);
                    parent.position = CellOrigin(cell);
                    cells.Add(cell, parent);
                }
                string path = $"{ChunkMeshFolder}/{source.sharedMesh.name}{suffix}.asset";
                CreateMeshObject(parent, path, source.name, pair.Value, renderer.sharedMaterial, false);
            }
        }

        private static void ClipPolygon(List<Vector3> input, List<Vector3> output, int axis, float boundary, bool keepAbove)
        {
            output.Clear();
            if (input.Count == 0) return;
            Vector3 previous = input[input.Count - 1];
            bool previousInside = keepAbove ? previous[axis] >= boundary : previous[axis] <= boundary;
            foreach (Vector3 point in input)
            {
                bool inside = keepAbove ? point[axis] >= boundary : point[axis] <= boundary;
                if (inside != previousInside)
                {
                    float t = (boundary - previous[axis]) / (point[axis] - previous[axis]);
                    Vector3 intersection = Vector3.LerpUnclamped(previous, point, t);
                    intersection[axis] = boundary;
                    output.Add(intersection);
                }
                if (inside) output.Add(point);
                previous = point;
                previousInside = inside;
            }
        }

        private static void AppendPolygon(MeshBuilder piece, List<Vector3> polygon, Vector3 origin)
        {
            Vector3 a = polygon[0];
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                Vector3 b = polygon[i];
                Vector3 c = polygon[i + 1];
                if (Vector3.Cross(b - a, c - a).sqrMagnitude <= 0.00000001f) continue;
                int first = piece.Vertices.Count;
                piece.Vertices.Add(a - origin);
                piece.Vertices.Add(b - origin);
                piece.Vertices.Add(c - origin);
                piece.Triangles.Add(first);
                piece.Triangles.Add(first + 1);
                piece.Triangles.Add(first + 2);
            }
        }
    }
}
