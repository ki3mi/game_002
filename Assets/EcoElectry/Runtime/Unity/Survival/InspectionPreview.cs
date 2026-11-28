using System;
using System.Collections.Generic;
using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace EcoElectry.Unity.Survival
{
    internal sealed partial class InspectionPreview : IDisposable
    {
        internal enum TargetKind { None, Latch, Slot, Cell, Terminal, Probe, Cover, Screw, Fuse, FuseSlot, FuseTerminal }

        internal readonly struct Hit
        {
            public Hit(TargetKind kind, int index = -1, BatteryCell cell = null, int terminal = -1)
            {
                Kind = kind;
                Index = index;
                Cell = cell;
                Terminal = terminal;
            }

            public TargetKind Kind { get; }
            public int Index { get; }
            public BatteryCell Cell { get; }
            public int Terminal { get; }
        }

        private const int PreviewLayer = 30;
        private const int PreviewMask = 1 << PreviewLayer;
        private const float NormalSize = 1.26f;
        private static readonly Color Graphite = new(.09f, .13f, .14f);
        private static readonly Color Rubber = new(.034f, .052f, .058f);
        private static readonly Color Steel = new(.41f, .46f, .47f);
        private static readonly Color Copper = new(.72f, .40f, .18f);
        private static readonly Color Marking = new(.60f, .73f, .65f);
        private static readonly Color Selected = new(.89f, .67f, .34f);
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private readonly GameObject stage;
        private readonly Camera camera;
        private readonly Transform pivot;
        private readonly Material surface;
        private readonly InspectionModels assets;
        private readonly MaterialPropertyBlock colors = new();
        private readonly Dictionary<Collider, Hit> targets = new();
        private readonly RaycastHit[] contactHits = new RaycastHit[16];
        private Transform model;
        private Transform caseHinge;
        private GameObject caseBattery;
        private bool compartment;
        private Vector2 angle;
        private float zoom;
        private Vector2 dragStart;
        private bool dragging;
        private bool dragged;

        public InspectionPreview(Material surface, InspectionModels assets)
        {
            this.surface = surface;
            this.assets = assets;
            stage = new GameObject("Visor de mantenimiento") { hideFlags = HideFlags.HideInHierarchy };
            stage.transform.position = new Vector3(10000, 0, 0);
            pivot = Group("Objeto", stage.transform, Vector3.zero);
            Transform eye = Group("Cámara", stage.transform, Vector3.zero);
            camera = eye.gameObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.062f, .093f, .110f);
            camera.orthographic = true;
            camera.nearClipPlane = .08f;
            camera.farClipPlane = 12;
            camera.allowHDR = false;
            camera.cullingMask = PreviewMask;
            Texture = new RenderTexture(960, 660, 24) { name = "Inspección de mantenimiento", antiAliasing = 2 };
            camera.targetTexture = Texture;
            camera.aspect = (float)Texture.width / Texture.height;
            AddLight(new Vector3(-2.2f, 2.7f, -3.7f), new Color(1f, .91f, .78f), 12f);
            AddLight(new Vector3(1.8f, 1.3f, -2.7f), new Color(.64f, .82f, 1f), 6.2f);
            ResetView();
            stage.SetActive(false);
        }

        public RenderTexture Texture { get; }

        private void AddLight(Vector3 position, Color color, float brightness)
        {
            Light light = Group("Luz de estudio", stage.transform, position).gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = brightness;
            light.color = color;
            light.cullingMask = PreviewMask;
        }

        public void ShowCase(bool opened, bool hasBattery)
        {
            BeginModel(false);
            Block("Fondo", new Vector3(0, -.23f, 0), new Vector3(1.81f, .15f, 1.13f), Graphite, true);
            foreach (float x in new[] { -.85f, .85f })
                Block("Costado", new Vector3(x, -.075f, 0), new Vector3(.102f, .31f, 1.13f), Graphite, true);
            foreach (float z in new[] { -.518f, .518f })
                Block("Frente", new Vector3(0, -.075f, z), new Vector3(1.62f, .31f, .099f), Graphite, true);
            caseHinge = Group("Bisagra", model, new Vector3(0, .09f, -.54f));
            Block("Tapa", new Vector3(0, .016f, .54f), new Vector3(1.82f, .103f, 1.13f), Graphite, true, caseHinge);
            foreach (float x in new[] { -.66f, .66f })
                Block("Refuerzo", new Vector3(x, .077f, .54f), new Vector3(.061f, .031f, .99f), Steel, false, caseHinge);
            Collider latch = Block("Cierre", new Vector3(0, -.032f, 1.128f), new Vector3(.28f, .18f, .075f), Copper, true, caseHinge)
                .GetComponent<Collider>();
            targets.Add(latch, new Hit(TargetKind.Latch));
            caseBattery = Group("Contenido", model, new Vector3(.11f, .015f, -.09f)).gameObject;
            Spawn(assets.Cell, caseBattery.transform);
            SetCaseOpen(opened, hasBattery);
            if (!opened) Rotate(new Vector2(-18, 8));
        }

        public void SetCaseOpen(bool opened, bool hasBattery)
        {
            caseHinge.localRotation = Quaternion.Euler(opened ? -105 : 0, 0, 0);
            caseBattery.SetActive(opened && hasBattery);
            if (opened) Rotate(new Vector2(156, 7) - angle);
        }

        public void ResetView()
        {
            dragging = false;
            angle = Vector2.zero;
            zoom = 1f;
            camera.transform.localPosition = compartment ? new Vector3(0, 0, -7f) : new Vector3(0, 1.56f, -4.1f);
            camera.transform.localRotation = Quaternion.LookRotation(-camera.transform.localPosition);
            camera.orthographicSize = compartment ? 1.76f : NormalSize;
            pivot.localPosition = compartment ? new Vector3(.31f, -.48f, 0) : Vector3.zero;
            pivot.localScale = Vector3.one;
            Rotate(compartment ? new Vector2(-13, -42) : Vector2.zero);
            if (repairModel)
            {
                camera.transform.SetLocalPositionAndRotation(new Vector3(0, 0, -7f), Quaternion.identity);
                camera.orthographicSize = 1.73f;
                Rotate(new Vector2(-7f, 8f));
            }
        }

        public void CancelPointer() => dragging = false;

        public bool HandlePointer(Rect area, out Hit selection, bool placingProbe = false)
        {
            selection = default;
            Event input = Event.current;
            Vector2 point = input.mousePosition;
            if (input.type == EventType.ScrollWheel && area.Contains(point))
            {
                if (compartment)
                {
                    zoom = Mathf.Clamp(zoom - input.delta.y * .054f, .71f, 1.23f);
                    pivot.localScale = Vector3.one * zoom;
                }
                else camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + input.delta.y * .064f,
                    repairModel ? 1.12f : .60f, repairModel ? 2.28f : 1.67f);
                input.Use();
                return true;
            }
            if (input.type == EventType.MouseDown && input.button == 0 && area.Contains(point))
            {
                dragStart = point;
                dragging = true;
                dragged = false;
                input.Use();
                return true;
            }
            if (dragging && input.type == EventType.MouseDrag)
            {
                dragged |= (point - dragStart).sqrMagnitude > 9;
                if (dragged) Rotate(new Vector2(-input.delta.x * .6f, -input.delta.y * .47f));
                input.Use();
                return true;
            }
            if (!dragging || input.type != EventType.MouseUp || input.button != 0) return false;
            dragging = false;
            if (!dragged && area.Contains(point))
            {
                Vector3 screen = new((point.x - area.x) / area.width, 1 - (point.y - area.y) / area.height, 0);
                selection = Pick(screen, placingProbe);
            }
            input.Use();
            return true;
        }

        internal Hit Pick(Vector3 point, bool placingProbe)
        {
            Physics.SyncTransforms();
            Ray ray = camera.ViewportPointToRay(point);
            if (!Physics.Raycast(ray, out RaycastHit hit, 12f, PreviewMask) ||
                !targets.TryGetValue(hit.collider, out Hit first)) return default;
            if (!placingProbe || first.Kind != TargetKind.Probe) return first;

            // A lead can cross another terminal on screen. The cell and casing still occlude that target.
            float distance = float.PositiveInfinity;
            Hit behindProbe = default;
            int count = Physics.RaycastNonAlloc(ray, contactHits, 12f, PreviewMask);
            for (int i = 0; i < count; i++)
            {
                RaycastHit candidate = contactHits[i];
                targets.TryGetValue(candidate.collider, out Hit target);
                if (target.Kind == TargetKind.Probe || candidate.distance >= distance) continue;
                distance = candidate.distance;
                behindProbe = target;
            }
            return behindProbe.Kind is TargetKind.Terminal or TargetKind.FuseTerminal ? behindProbe : first;
        }

        private void Rotate(Vector2 movement)
        {
            angle.x = Mathf.Repeat(angle.x + movement.x + 180, 360) - 180;
            angle.y = Mathf.Clamp(angle.y + movement.y, -85, 72);
            pivot.localRotation = Quaternion.Euler(angle.y, angle.x, 0);
        }

        private void BeginModel(bool showCompartment)
        {
            if (workspace != null)
            {
                workspace.gameObject.SetActive(false);
                Object.Destroy(workspace.gameObject);
                workspace = null;
            }
            if (model != null)
            {
                model.gameObject.SetActive(false);
                Object.Destroy(model.gameObject);
            }
            targets.Clear();
            compartment = showCompartment;
            repairModel = false;
            repairMotion = null;
            compartmentCells.Clear();
            movingCell = null;
            model = Group("Modelo", pivot, Vector3.zero);
            ResetView();
            stage.SetActive(true);
        }

        private Transform Spawn(GameObject prefab, Transform parent)
        {
            Transform prop = Object.Instantiate(prefab, parent, false).transform;
            foreach (Transform node in prop.GetComponentsInChildren<Transform>(true))
                node.gameObject.layer = PreviewLayer;
            return prop;
        }

        private GameObject Block(string name, Vector3 position, Vector3 scale, Color color, bool pickable = false, Transform parent = null) =>
            Primitive(PrimitiveType.Cube, name, position, scale, color, pickable, parent);

        private GameObject Primitive(PrimitiveType kind, string name, Vector3 position, Vector3 scale, Color color,
            bool pickable, Transform parent)
        {
            GameObject part = GameObject.CreatePrimitive(kind);
            part.layer = PreviewLayer;
            part.name = name;
            part.transform.SetParent(parent != null ? parent : model, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = surface;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            SetColor(renderer, color);
            if (!pickable)
            {
                Collider shape = part.GetComponent<Collider>();
                shape.enabled = false;
                Object.Destroy(shape);
            }
            return part;
        }

        private void SetColor(Renderer renderer, Color color)
        {
            colors.SetColor(BaseColor, color);
            renderer.SetPropertyBlock(colors);
        }

        private static Transform Group(string name, Transform parent, Vector3 position)
        {
            var obj = new GameObject(name) { layer = PreviewLayer };
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            return obj.transform;
        }

        public void Hide()
        {
            dragging = false;
            if (stage != null) stage.SetActive(false);
        }

        public void Dispose()
        {
            if (camera != null) camera.targetTexture = null;
            Texture.Release();
            Object.Destroy(Texture);
            Object.Destroy(stage);
        }
    }
}
