using System.Collections.Generic;
using UnityEngine;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent]
    public sealed class ChunkRenderWindow : MonoBehaviour
    {
        private const int Radius = 1;

        [SerializeField] private Transform player;
        [SerializeField] private Transform terrainRoot;
        [SerializeField] private Transform[] sceneryRoots;
        [SerializeField] private Vector2 origin;
        [SerializeField, Min(1f)] private float chunkSize = 300f;

        private readonly List<TerrainTile> terrainTiles = new();
        private readonly List<Scenery> scenery = new();
        private Vector2Int currentChunk;
        private bool ready;

        private void OnEnable()
        {
            if (player == null || terrainRoot == null || chunkSize <= 0f) return;

            foreach (Terrain terrain in terrainRoot.GetComponentsInChildren<Terrain>())
                terrainTiles.Add(new TerrainTile(terrain, ChunkAt(terrain.transform.position)));

            if (sceneryRoots != null)
            {
                foreach (Transform root in sceneryRoots)
                {
                    if (root != null) CollectScenery(root);
                }
            }

            ready = true;
            UpdateWindow(ChunkAt(player.position));
        }

        public void Follow(Transform target)
        {
            if (target == null) return;
            player = target;
            if (ready) UpdateWindow(ChunkAt(target.position));
        }

        public void RegisterStaticScenery(Transform root)
        {
            if (!ready || root == null) return;
            CollectScenery(root);
            UpdateWindow(currentChunk);
        }

        private void CollectScenery(Transform root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;
                Bounds bounds = renderer.bounds;
                // A renderer ending exactly on an edge belongs to the tile before that edge.
                Vector3 insideMax = Vector3.Max(bounds.min, bounds.max - new Vector3(0.001f, 0f, 0.001f));
                scenery.Add(new Scenery(renderer, ChunkAt(bounds.min), ChunkAt(insideMax)));
            }
        }

        private void LateUpdate()
        {
            if (!ready || player == null) return;
            Vector2Int next = ChunkAt(player.position);
            if (next != currentChunk) UpdateWindow(next);
        }

        private void OnDisable()
        {
            foreach (TerrainTile tile in terrainTiles) tile.SetVisible(true);
            foreach (Scenery item in scenery) item.SetVisible(true);
            terrainTiles.Clear();
            scenery.Clear();
            ready = false;
        }

        private Vector2Int ChunkAt(Vector3 point) => new(
            Mathf.FloorToInt((point.x - origin.x) / chunkSize),
            Mathf.FloorToInt((point.z - origin.y) / chunkSize));

        private void UpdateWindow(Vector2Int center)
        {
            currentChunk = center;
            var min = new Vector2Int(center.x - Radius, center.y - Radius);
            var max = new Vector2Int(center.x + Radius, center.y + Radius);

            foreach (TerrainTile tile in terrainTiles)
                tile.SetVisible(IntersectsWindow(tile.Cell, tile.Cell, min, max));
            foreach (Scenery item in scenery)
                item.SetVisible(IntersectsWindow(item.Min, item.Max, min, max));
        }

        private static bool IntersectsWindow(Vector2Int min, Vector2Int max, Vector2Int windowMin, Vector2Int windowMax) =>
            min.x <= windowMax.x && max.x >= windowMin.x && min.y <= windowMax.y && max.y >= windowMin.y;

        private readonly struct TerrainTile
        {
            private readonly Terrain terrain;
            private readonly bool drawHeightmap;
            private readonly bool drawFoliage;
            public readonly Vector2Int Cell;

            public TerrainTile(Terrain terrain, Vector2Int cell)
            {
                this.terrain = terrain;
                Cell = cell;
                drawHeightmap = terrain.drawHeightmap;
                drawFoliage = terrain.drawTreesAndFoliage;
            }

            public void SetVisible(bool visible)
            {
                if (terrain == null) return;
                bool heightmap = visible && drawHeightmap;
                bool foliage = visible && drawFoliage;
                if (terrain.drawHeightmap != heightmap) terrain.drawHeightmap = heightmap;
                if (terrain.drawTreesAndFoliage != foliage) terrain.drawTreesAndFoliage = foliage;
            }
        }

        private readonly struct Scenery
        {
            private readonly Renderer renderer;
            private readonly bool wasHidden;
            public readonly Vector2Int Min;
            public readonly Vector2Int Max;

            public Scenery(Renderer renderer, Vector2Int min, Vector2Int max)
            {
                this.renderer = renderer;
                wasHidden = renderer.forceRenderingOff;
                Min = min;
                Max = max;
            }

            public void SetVisible(bool visible)
            {
                bool hidden = wasHidden || !visible;
                if (renderer != null && renderer.forceRenderingOff != hidden)
                    renderer.forceRenderingOff = hidden;
            }
        }
    }
}
