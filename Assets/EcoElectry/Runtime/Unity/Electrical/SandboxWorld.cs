using System;
using System.Collections.Generic;
using EcoElectry.Application.Electrical;
using EcoElectry.Domain.Electrical;
using EcoElectry.Unity.Construction;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace EcoElectry.Unity.Electrical
{
    internal sealed class SandboxWorld : IDisposable
    {
        private readonly SandboxDefinition definition;
        private readonly Transform parent;
        private readonly Dictionary<Equipment, GameObject> models = new Dictionary<Equipment, GameObject>();
        private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        private readonly List<Bounds> fixedObstacles = new List<Bounds>();
        private readonly Material lineMaterial;
        private Transform wires;
        private Light classroomLight;
        private GameObject bladeIndicator;
        private GameObject protectionIndicator;
        private GameObject solarIndicator;
        private GameObject batteryIndicator;
        private SandboxCaseEntry layout;

        public static Vector3 Yard => new Vector3(-2.7f, 0.05f, -4.3f);
        public static Vector3 BoardSpawn => new Vector3(0.1f, 0.05f, -0.8f);
        public static readonly Color Amber = new Color32(248, 179, 78, 255);
        public static readonly Color Aqua = new Color32(76, 221, 200, 255);
        public static readonly Color Error = new Color32(239, 110, 112, 255);

        public SandboxWorld(SandboxDefinition definition, Transform parent)
        {
            this.definition = definition;
            this.parent = parent;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            lineMaterial = new Material(shader);
            BuildRoom();
        }

        public void Load(SandboxCaseEntry entry)
        {
            layout = entry;
            foreach (GameObject model in models.Values)
            {
                if (model == null) continue;
                model.SetActive(false);
                UObject.Destroy(model);
            }
            models.Clear();
        }

        public Vector3 At(Equipment equipment)
        {
            switch (equipment)
            {
                case Equipment.Panel: return new Vector3(-4.9f, 0.68f, 1.75f) + layout.PanelOffset;
                case Equipment.Battery: return new Vector3(0.55f, 0.1f, 0.15f) + layout.BatteryOffset;
                case Equipment.Board: return new Vector3(0.1f, 1.13f, 2.55f);
                case Equipment.Lamp: return new Vector3(5.55f, 2.33f, 2.47f);
                default: throw new ArgumentOutOfRangeException(nameof(equipment));
            }
        }

        public Vector3 At(Terminal terminal)
        {
            Vector3 position = At(SandboxCircuit.EquipmentFor(terminal));
            switch (terminal)
            {
                case Terminal.SolarOut: return position + new Vector3(0.72f, 0.46f, -0.25f);
                case Terminal.SolarIn: return position + new Vector3(-0.73f, 0.48f, -0.33f);
                case Terminal.BatterySocket: return position + new Vector3(0.48f, 0.5f, -0.12f);
                case Terminal.StorageIn: return position + new Vector3(-0.04f, 0.05f, -0.36f);
                case Terminal.LightingOut: return position + new Vector3(0.72f, 0.43f, -0.35f);
                case Terminal.LampIn: return position + new Vector3(0f, -0.12f, -0.34f);
                default: throw new ArgumentOutOfRangeException(nameof(terminal));
            }
        }

        public Vector3 At(RoutePoint point)
        {
            Vector3 position;
            switch (point)
            {
                case RoutePoint.WallLeft: position = new Vector3(2.43f, 1.67f, 2.38f); break;
                case RoutePoint.CeilingLeft: position = new Vector3(2.43f, 2.88f, 2.38f); break;
                case RoutePoint.CeilingRight: position = new Vector3(5.63f, 2.88f, 2.38f); break;
                case RoutePoint.WallRight: position = new Vector3(6.25f, 2.22f, 2.38f); break;
                case RoutePoint.FloorLeft: position = new Vector3(2.43f, 0.09f, 1.46f); break;
                case RoutePoint.FloorRight: position = new Vector3(6.25f, 0.09f, 1.46f); break;
                default: throw new ArgumentOutOfRangeException(nameof(point));
            }

            return position + layout.RouteOffset;
        }

        public void UpdateInstallation(SandboxSession session)
        {
            foreach (Equipment equipment in Enum.GetValues(typeof(Equipment)))
            {
                if (!session.Circuit.Installed(equipment) || models.ContainsKey(equipment)) continue;
                GameObject model = AddVisual(definition.VisualFor(equipment), At(equipment), VisualSize(equipment));
                if (equipment == Equipment.Board && model != null)
                {
                    AddVisual(definition.Isolator, At(equipment) + new Vector3(-0.29f, 0.29f, -0.34f), 0.20f, model.transform);
                    AddVisual(definition.Protection, At(equipment) + new Vector3(0.25f, 0.29f, -0.34f), 0.20f, model.transform);
                }
                models.Add(equipment, model);
            }

            bladeIndicator.GetComponent<Renderer>().sharedMaterial = Paint(session.Circuit.BladesClosed ? Aqua : Amber);
            protectionIndicator.GetComponent<Renderer>().sharedMaterial = Paint(session.Circuit.ProtectionTripped ? Error : Aqua);
            MoveIndicator(solarIndicator, At(Terminal.SolarOut), session.Circuit.SolarActive,
                session.Circuit.Installed(Equipment.Panel));
            MoveIndicator(batteryIndicator, At(Terminal.BatterySocket), session.Circuit.BatteryActive,
                session.Circuit.Installed(Equipment.Battery));

            if (wires != null)
            {
                wires.gameObject.SetActive(false);
                UObject.Destroy(wires.gameObject);
            }
            wires = new GameObject("Trazados").transform;
            wires.SetParent(parent, false);

            foreach (Cable cable in session.Circuit.Cables)
            {
                Color color = cable.Shorted ? Error : cable.Family == CableFamily.Solar ? Amber : Aqua;
                DrawRoute(cable.From, cable.To, cable.Route, cable.Family, color, 0.033f);
            }

            if (session.Draft != null)
                DrawRoute(session.Draft.From, session.Draft.To, session.Draft.Points, session.Draft.Family,
                    Color.white, 0.019f);
        }

        private void MoveIndicator(GameObject indicator, Vector3 position, bool enabled, bool installed)
        {
            indicator.transform.position = position + Vector3.up * 0.15f;
            indicator.GetComponent<Renderer>().sharedMaterial = Paint(enabled ? Amber : Aqua);
            indicator.SetActive(installed);
        }

        public void SetLamp(bool enabled) => classroomLight.enabled = enabled;

        public IEnumerable<Bounds> ConstructionObstacles()
        {
            foreach (Bounds obstacle in fixedObstacles) yield return obstacle;
            foreach (Equipment equipment in Enum.GetValues(typeof(Equipment)))
            {
                GameObject prefab = definition.VisualFor(equipment);
                if (prefab == null || !GridModelVisual.TryGetBounds(prefab, out Bounds bounds)) continue;

                Vector3 size = bounds.size;
                float factor = VisualSize(equipment) / Mathf.Max(0.001f, size.x, size.y, size.z);
                yield return new Bounds(At(equipment), size * factor);
            }

            yield return new Bounds(Yard, new Vector3(0.75f, 0.5f, 0.75f));
            yield return new Bounds(BoardSpawn, new Vector3(0.75f, 0.5f, 0.75f));
        }

        private void DrawRoute(Terminal from, Terminal? to, IReadOnlyList<RoutePoint> points,
            CableFamily family, Color color, float width)
        {
            Vector3 last = At(from);
            foreach (RoutePoint point in points)
            {
                Segment(last, At(point), family == CableFamily.Interior, color, width);
                last = At(point);
            }
            if (to.HasValue) Segment(last, At(to.Value), family == CableFamily.Interior, color, width);
        }

        private void Segment(Vector3 from, Vector3 to, bool dashed, Color color, float width)
        {
            if (!dashed)
            {
                Line(from, to, color, width);
                return;
            }

            Vector3 delta = to - from;
            float length = delta.magnitude;
            Vector3 direction = delta.normalized;
            for (float offset = 0; offset < length; offset += 0.235f)
                Line(from + direction * offset, from + direction * Mathf.Min(offset + 0.135f, length), color, width);
        }

        private void Line(Vector3 from, Vector3 to, Color color, float width)
        {
            var wire = new GameObject("Segmento");
            wire.transform.SetParent(wires, false);
            var line = wire.AddComponent<LineRenderer>();
            line.sharedMaterial = lineMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            line.SetPropertyBlock(properties);
            line.widthMultiplier = width;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private GameObject AddVisual(GameObject prefab, Vector3 floorPosition, float size, Transform owner = null)
        {
            if (prefab == null) return null;
            var container = new GameObject(prefab.name);
            container.SetActive(false);
            container.transform.SetParent(owner == null ? parent : owner, false);
            GameObject model = UObject.Instantiate(prefab, container.transform);
            foreach (MonoBehaviour script in model.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled = false;
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (Light light in model.GetComponentsInChildren<Light>(true)) light.enabled = false;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                float factor = size / Mathf.Max(0.001f, bounds.size.x, bounds.size.y, bounds.size.z);
                model.transform.localScale *= factor;
                Vector3 footprint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                model.transform.position = floorPosition + (model.transform.position - footprint) * factor;
            }
            else model.transform.position = floorPosition;

            container.SetActive(true);
            return container;
        }

        private static float VisualSize(Equipment equipment)
        {
            switch (equipment)
            {
                case Equipment.Panel: return 2.05f;
                case Equipment.Board: return 1.37f;
                case Equipment.Battery: return 1.05f;
                default: return 1.5f;
            }
        }

        private void BuildRoom()
        {
            var sand = new Color32(165, 155, 129, 255);
            var concrete = new Color32(105, 128, 132, 255);
            var wood = new Color32(175, 123, 81, 255);
            var plaster = new Color32(204, 201, 173, 255);
            var dark = new Color32(48, 75, 81, 255);

            Box("Plataforma", new Vector3(0, -0.38f, 0), new Vector3(16, 0.55f, 12), dark);
            Box("Patio", new Vector3(-4.9f, -0.115f, 0), new Vector3(5.8f, 0.23f, 11.6f), sand);
            Box("Zona técnica", new Vector3(0, -0.115f, 0), new Vector3(4f, 0.23f, 11.6f), concrete);
            Box("Aula", new Vector3(4.96f, -0.115f, 0), new Vector3(5.9f, 0.23f, 11.6f), wood);
            Box("Muro técnico", new Vector3(0, 1.52f, 2.95f), new Vector3(4.05f, 3.04f, 0.25f), dark);
            Box("Pared de aula", new Vector3(4.99f, 1.52f, 2.95f), new Vector3(5.92f, 3.04f, 0.25f), plaster);
            Box("Techo de aula", new Vector3(5, 3.08f, 2.62f), new Vector3(5.95f, 0.16f, 0.95f), dark);
            Box("Zócalo de aula", new Vector3(5, 0.18f, 2.78f), new Vector3(5.92f, 0.36f, 0.10f), wood);
            Box("Soporte solar", new Vector3(-4.85f, 0.31f, 1.28f), new Vector3(3.25f, 0.63f, 2.72f), concrete);
            Box("Banco técnico", new Vector3(0.6f, 0.05f, 0.2f), new Vector3(2.2f, 0.1f, 1.85f), dark);
            Box("Pizarra", new Vector3(5.03f, 1.38f, 2.78f), new Vector3(2.48f, 0.85f, 0.07f), dark);
            Box("Mesa de aula", new Vector3(5.1f, 0.64f, 0.65f), new Vector3(1.67f, 0.12f, 0.72f), dark);
            for (int sign = -1; sign <= 1; sign += 2)
                Box("Pata de mesa", new Vector3(5.1f + sign * 0.73f, 0.31f, 0.65f),
                    new Vector3(0.12f, 0.62f, 0.59f), dark);
            for (int i = 0; i < 9; i++)
                Box("Marca de tránsito", new Vector3(-7.35f + i * 1.62f, 0.006f, -2.9f),
                    new Vector3(0.77f, 0.01f, 0.055f), plaster, false);

            bladeIndicator = Box("Indicador de cuchillas", new Vector3(-0.3f, 1.72f, 2.1f),
                new Vector3(0.12f, 0.065f, 0.07f), Amber, false);
            protectionIndicator = Box("Indicador de protección", new Vector3(0.42f, 1.72f, 2.1f),
                new Vector3(0.12f, 0.065f, 0.07f), Aqua, false);
            solarIndicator = Box("Indicador solar", Vector3.zero, Vector3.one * 0.09f, Amber, false);
            batteryIndicator = Box("Indicador de batería", Vector3.zero, Vector3.one * 0.09f, Amber, false);

            var light = new GameObject("Luz del aula");
            light.transform.SetParent(parent, false);
            light.transform.position = new Vector3(5.55f, 2.38f, 1.97f);
            classroomLight = light.AddComponent<Light>();
            classroomLight.type = LightType.Point;
            classroomLight.color = new Color(1f, 0.92f, 0.68f);
            classroomLight.range = 5.3f;
            classroomLight.intensity = 3.6f;
            classroomLight.enabled = false;
        }

        private GameObject Box(string name, Vector3 point, Vector3 scale, Color color, bool collision = true)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = point;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = Paint(color);
            if (!collision) box.GetComponent<Collider>().enabled = false;
            else if (point.y + scale.y * 0.5f > 0.015f) fixedObstacles.Add(new Bounds(point, scale));
            return box;
        }

        private Material Paint(Color color)
        {
            if (materials.TryGetValue(color, out Material existing)) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            material.SetFloat("_Smoothness", 0.18f);
            materials.Add(color, material);
            return material;
        }

        public void Dispose()
        {
            foreach (Material material in materials.Values) UObject.Destroy(material);
            UObject.Destroy(lineMaterial);
        }
    }
}
