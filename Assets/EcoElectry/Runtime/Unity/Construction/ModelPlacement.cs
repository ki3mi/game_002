using System;
using System.Collections.Generic;
using EcoElectry.Domain.Construction;
using UnityEngine;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;

namespace EcoElectry.Unity.Construction
{
    internal sealed class ModelPlacement : IDisposable
    {
        private readonly struct Candidate
        {
            public Candidate(GridCell anchor, GridFootprint footprint, GridAvailability availability, string obstruction)
            {
                Anchor = anchor;
                Footprint = footprint;
                Availability = availability;
                Obstruction = obstruction;
            }

            public GridCell Anchor { get; }
            public GridFootprint Footprint { get; }
            public GridAvailability Availability { get; }
            public string Obstruction { get; }
            public bool CanPlace => Availability == GridAvailability.Available && Obstruction == null;
        }

        private readonly Transform surface;
        private readonly GameObject gridLines;
        private readonly GameObject highlight;
        private readonly Mesh lineMesh;
        private readonly Mesh highlightMesh;
        private readonly Material lineMaterial;
        private readonly Material highlightMaterial;
        private readonly Material ghostMaterial;
        private readonly GridFootprint size;
        private readonly RaycastHit[] viewHits = new RaycastHit[8];
        private readonly Stack<GameObject> placedModels = new Stack<GameObject>();
        private PlacementGrid grid;
        private GameObject ghost;
        private int turn;

        public ModelPlacement(Transform parent, Shader shader, Vector3 origin, Vector2Int cells, float cellSize)
        {
            CellSize = cellSize;
            size = new GridFootprint(cells.x, cells.y);

            surface = new GameObject("Superficie de construcción").transform;
            surface.SetParent(parent, false);
            surface.position = origin;
            surface.rotation = Quaternion.identity;

            lineMaterial = new Material(shader) { color = new Color(0.85f, 1f, 0.97f, 0.50f) };
            highlightMaterial = new Material(shader) { renderQueue = 3001 };
            ghostMaterial = new Material(shader) { renderQueue = 3002 };
            lineMesh = CreateLineMesh(size, cellSize);
            highlightMesh = CreateCellMesh();
            gridLines = CreateOverlay("Grilla", lineMesh, lineMaterial);
            highlight = CreateOverlay("Huella", highlightMesh, highlightMaterial);
            gridLines.SetActive(false);
            highlight.SetActive(false);
        }

        public float CellSize { get; }
        public GridModel Selected { get; private set; }
        public int Count => grid.Count;
        public string Hint { get; private set; }

        public GridFootprint Footprint(GridModel model)
        {
            return model.Footprint(CellSize).Rotated(model == Selected ? turn : 0);
        }

        public void Reset(IEnumerable<Bounds> obstacles)
        {
            Cancel();
            while (placedModels.Count > 0) RemoveVisual(placedModels.Pop());
            grid = new PlacementGrid(size);
            foreach (Bounds obstacle in obstacles) Reserve(obstacle);
        }

        public void Select(GridModel model)
        {
            Cancel();
            Selected = model;
            turn = 0;
            ghost = GridModelVisual.Create(model, surface, ghostMaterial);
        }

        public void Rotate()
        {
            if (Selected != null) turn = (turn + 1) & 3;
        }

        public void Cancel()
        {
            if (ghost != null) RemoveVisual(ghost);
            ghost = null;
            Selected = null;
            HidePreview();
        }

        public void HidePreview()
        {
            gridLines.SetActive(false);
            highlight.SetActive(false);
            if (ghost != null) ghost.SetActive(false);
            Hint = null;
        }

        public void Preview(Ray? ray, CharacterController player)
        {
            if (Selected == null) return;

            gridLines.SetActive(true);
            Candidate candidate = default;
            bool found = ray.HasValue && TryProject(ray.Value, player, out candidate);
            highlight.SetActive(found);
            ghost.SetActive(found && candidate.Availability != GridAvailability.Occupied);
            if (!found)
            {
                Hint = "Apunta a la grilla del suelo.";
                return;
            }

            PlaceAt(ghost.transform, candidate.Anchor, candidate.Footprint, turn);
            highlight.transform.localPosition = new Vector3(
                candidate.Anchor.Column * CellSize, 0.017f, candidate.Anchor.Row * CellSize);
            highlight.transform.localScale = new Vector3(
                candidate.Footprint.Width * CellSize, 1f, candidate.Footprint.Depth * CellSize);
            Color tint = candidate.CanPlace ? new Color(0.22f, 1f, 0.75f) : new Color(1f, 0.24f, 0.26f);
            highlightMaterial.color = new Color(tint.r, tint.g, tint.b, 0.29f);
            ghostMaterial.color = new Color(tint.r, tint.g, tint.b, 0.43f);
            Hint = Describe(candidate);
        }

        public bool TryPlace(Ray ray, CharacterController player)
        {
            if (Selected == null) return false;
            if (!TryProject(ray, player, out Candidate candidate))
            {
                Hint = "Apunta a la grilla del suelo.";
                return false;
            }

            Hint = Describe(candidate);
            if (!candidate.CanPlace) return false;

            GameObject model = GridModelVisual.Create(Selected, surface);
            PlaceAt(model.transform, candidate.Anchor, candidate.Footprint, turn);
            Vector3 modelSize = Selected.VisualBounds.size * Selected.Scale;
            var collision = model.AddComponent<BoxCollider>();
            collision.size = new Vector3(Mathf.Max(0.015f, modelSize.x), Mathf.Max(0.015f, modelSize.y),
                Mathf.Max(0.015f, modelSize.z));
            collision.center = Vector3.up * (collision.size.y * 0.5f);

            if (grid.TryPlace(Selected.Id, candidate.Anchor, Selected.Footprint(CellSize), turn) == null)
            {
                RemoveVisual(model);
                return false;
            }

            model.SetActive(true);
            placedModels.Push(model);
            return true;
        }

        public void Undo()
        {
            if (grid.Undo() == null) return;
            RemoveVisual(placedModels.Pop());
        }

        private bool TryProject(Ray ray, CharacterController player, out Candidate candidate)
        {
            candidate = default;
            if (ray.direction.y >= -0.0001f || !new Plane(Vector3.up, surface.position).Raycast(ray, out float distance))
                return false;

            Vector3 point = ray.GetPoint(distance) - surface.position;
            int column = Mathf.FloorToInt(point.x / CellSize);
            int row = Mathf.FloorToInt(point.z / CellSize);
            if (column < 0 || column >= size.Width || row < 0 || row >= size.Depth) return false;

            GridFootprint footprint = Footprint(Selected);
            var anchor = new GridCell(column - (footprint.Width - 1) / 2, row - (footprint.Depth - 1) / 2);
            GridAvailability availability = grid.Check(anchor, footprint);
            string obstruction = null;
            if (availability == GridAvailability.Available)
            {
                if (HitsPlayer(anchor, footprint, player.bounds))
                    obstruction = "Apártate del espacio de colocación.";
                else if (BlocksView(ray, distance, player))
                    obstruction = "Un objeto tapa la celda. Cambia el ángulo de cámara.";
            }

            candidate = new Candidate(anchor, footprint, availability, obstruction);
            return true;
        }

        private bool BlocksView(Ray ray, float distance, Collider player)
        {
            int hits = Physics.RaycastNonAlloc(ray, viewHits, Mathf.Max(0f, distance - 0.025f),
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
            {
                if (viewHits[i].collider != player) return true;
            }

            return false;
        }

        private bool HitsPlayer(GridCell anchor, GridFootprint footprint, Bounds player)
        {
            Vector3 bounds = Selected.VisualBounds.size * Selected.Scale;
            if ((turn & 1) != 0) (bounds.x, bounds.z) = (bounds.z, bounds.x);

            Vector3 center = surface.position + Center(anchor, footprint) + Vector3.up * (bounds.y * 0.5f);
            return new Bounds(center, bounds).Intersects(player);
        }

        private static string Describe(Candidate candidate)
        {
            switch (candidate.Availability)
            {
                case GridAvailability.Outside: return "La pieza sobresale de la grilla.";
                case GridAvailability.Reserved: return "Estas celdas están reservadas para el taller.";
                case GridAvailability.Occupied: return "Ya hay una pieza en esas celdas.";
                default: return candidate.Obstruction ?? "Clic para colocar · R para girar";
            }
        }

        private void Reserve(Bounds bounds)
        {
            Vector3 min = (bounds.min - surface.position) / CellSize;
            Vector3 max = (bounds.max - surface.position) / CellSize;
            int left = Mathf.Clamp(Mathf.FloorToInt(min.x + 0.0001f), 0, size.Width);
            int right = Mathf.Clamp(Mathf.CeilToInt(max.x - 0.0001f), 0, size.Width);
            int front = Mathf.Clamp(Mathf.FloorToInt(min.z + 0.0001f), 0, size.Depth);
            int back = Mathf.Clamp(Mathf.CeilToInt(max.z - 0.0001f), 0, size.Depth);
            if (right > left && back > front)
                grid.Reserve(new GridCell(left, front), new GridFootprint(right - left, back - front));
        }

        private Vector3 Center(GridCell anchor, GridFootprint footprint)
        {
            return new Vector3((anchor.Column + footprint.Width * 0.5f) * CellSize, 0f,
                (anchor.Row + footprint.Depth * 0.5f) * CellSize);
        }

        private void PlaceAt(Transform target, GridCell anchor, GridFootprint footprint, int quarterTurns)
        {
            target.localPosition = Center(anchor, footprint);
            target.localRotation = Quaternion.Euler(0f, quarterTurns * 90f, 0f);
        }

        private static void RemoveVisual(GameObject visual)
        {
            visual.SetActive(false);
            UObject.Destroy(visual);
        }

        private GameObject CreateOverlay(string objectName, Mesh mesh, Material material)
        {
            var overlay = new GameObject(objectName);
            overlay.transform.SetParent(surface, false);
            overlay.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = overlay.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return overlay;
        }

        private static Mesh CreateLineMesh(GridFootprint gridSize, float spacing)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float width = gridSize.Width * spacing;
            float depth = gridSize.Depth * spacing;

            for (int column = 0; column <= gridSize.Width; column++)
            {
                float x = column * spacing;
                float half = column % 2 == 0 ? 0.016f : 0.009f;
                AppendQuad(vertices, triangles, x - half, 0f, x + half, depth, 0.013f);
            }

            for (int row = 0; row <= gridSize.Depth; row++)
            {
                float z = row * spacing;
                float half = row % 2 == 0 ? 0.016f : 0.009f;
                AppendQuad(vertices, triangles, 0f, z - half, width, z + half, 0.013f);
            }

            return CreateMesh("Líneas de la grilla", vertices, triangles);
        }

        private static Mesh CreateCellMesh()
        {
            var vertices = new List<Vector3>(4);
            var triangles = new List<int>(6);
            AppendQuad(vertices, triangles, 0f, 0f, 1f, 1f, 0f);
            return CreateMesh("Área de la huella", vertices, triangles);
        }

        private static void AppendQuad(List<Vector3> vertices, List<int> triangles,
            float x0, float z0, float x1, float z1, float y)
        {
            int index = vertices.Count;
            vertices.Add(new Vector3(x0, y, z0));
            vertices.Add(new Vector3(x0, y, z1));
            vertices.Add(new Vector3(x1, y, z1));
            vertices.Add(new Vector3(x1, y, z0));
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        private static Mesh CreateMesh(string name, List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }

        public void Dispose()
        {
            UObject.Destroy(lineMesh);
            UObject.Destroy(highlightMesh);
            UObject.Destroy(lineMaterial);
            UObject.Destroy(highlightMaterial);
            UObject.Destroy(ghostMaterial);
        }
    }
}
