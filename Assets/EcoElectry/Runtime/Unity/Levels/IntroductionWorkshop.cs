using EcoElectry.Unity.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Levels
{
    internal static class IntroductionWorkshop
    {
        internal static SurvivalScene Setup(Transform room, Transform player, SurvivalSettings settings,
            InspectionModels models, Material previewMaterial)
        {
            if (room == null || settings == null || models == null || !models.Assigned || previewMaterial == null) return null;
            Transform bench = room.Find("Banco de trabajo y vídeo");
            Camera view = player.GetComponentInChildren<Camera>(true);
            if (bench == null || view == null || player.gameObject.activeInHierarchy) return null;

            GameObject flashlight = DisplayModel("Linterna del tutorial", models.Flashlight, bench,
                new Vector3(-1.03f, 1.115f, 2.57f), 0.33f, 18f);
            ImproveLighting(room);
            PrepareStock(room, models, previewMaterial);
            var interfaceOwner = player.gameObject.AddComponent<SurvivalScene>();
            interfaceOwner.ConfigureTutorial(settings, view, previewMaterial, models, flashlight);
            return interfaceOwner;
        }

        private static void PrepareStock(Transform room, InspectionModels models, Material material)
        {
            Transform storage = room.Find("Almacenamiento y electricidad");
            Transform rack = storage?.Find("Estantería de repuestos");
            if (rack == null) return;

            AddCase(rack, material, "REPUESTOS", "Caja de suministros",
                "Baterías de reserva. Abre la caja para recoger un repuesto de linterna.", models.Cell);
            AddCase(rack, material, "FIJACIONES", "Caja de fijaciones",
                "Tornillos, anclajes y abrazaderas para sujetar equipos y canaletas.");
            AddCase(rack, material, "CONECTORES", "Caja de conectores",
                "Bornes y conectores de repuesto. Se usan para unir y organizar conductores.");
            AddCase(rack, material, "CABLES", "Caja de cables",
                "Conductores de repuesto para sustituir tramos dañados.");
            AddCase(rack, material, "TUBOS", "Caja de tubos",
                "Tubos y difusores para luminarias del colegio.");

            var stock = new GameObject("Caja de fusibles");
            stock.transform.SetParent(rack, false);
            stock.transform.localPosition = new Vector3(0.52f, 0.858f, -0.12f);
            Material body = rack.Find("Caja de REPUESTOS")?.GetComponent<Renderer>()?.sharedMaterial;
            Material metal = rack.Find("Bandeja")?.GetComponent<Renderer>()?.sharedMaterial;
            StockPart(stock.transform, "Caja", PrimitiveType.Cube, new Vector3(0f, 0.055f, 0f),
                new Vector3(0.52f, 0.11f, 0.29f), body);
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 0.15f;
                StockPart(stock.transform, "Cartucho de fusible", PrimitiveType.Cylinder,
                    new Vector3(x, 0.15f, 0f), new Vector3(0.042f, 0.065f, 0.042f), body);
                foreach (float end in new[] { -0.07f, 0.07f })
                    StockPart(stock.transform, "Terminal", PrimitiveType.Cylinder, new Vector3(x, 0.15f, end),
                        new Vector3(0.047f, 0.017f, 0.047f), metal);
            }
            Inspect(stock.transform, "Caja de fusibles · Los cartuchos de repuesto protegen circuitos. Usa el multímetro para comprobar continuidad.");
            Inspect(storage.Find("TableroDistribucion"),
                "Cuadro eléctrico · Distribuye la alimentación hacia los circuitos del colegio.");
            Inspect(storage.Find("Carrete de cable"), "Carrete · Reserva de conductor para reparaciones de cableado.");
            Inspect(storage.Find("TomacorrienteIndustrial"), "Tomacorriente industrial · Punto de conexión para equipos del taller.");
        }

        private static void AddCase(Transform rack, Material material, string label, string title, string description,
            GameObject part = null)
        {
            Transform box = rack.Find("Caja de " + label);
            if (box == null) return;
            var physical = new GameObject(title);
            Transform root = physical.transform;
            root.SetParent(rack, false);
            Vector3 size = box.localScale;
            float scale = Mathf.Min(size.x / SupplyCaseGeometry.Width, size.z / SupplyCaseGeometry.Depth, .32f);
            root.localScale = Vector3.one * scale;
            root.localRotation = Quaternion.Euler(0f, 180f, 0f);
            root.localPosition = box.localPosition + Vector3.up * (-size.y * .5f - SupplyCaseGeometry.Bottom * scale);
            int index = box.GetSiblingIndex();
            string[] decorations = { "Cinta adhesiva", "Etiqueta de caja", label };
            for (int i = 0; i < decorations.Length && index + i + 1 < rack.childCount; i++)
            {
                Transform candidate = rack.GetChild(index + i + 1);
                if (candidate.name == decorations[i]) candidate.gameObject.SetActive(false);
            }
            box.gameObject.SetActive(false);

            var colors = new MaterialPropertyBlock();
            var (hinge, _) = SupplyCaseGeometry.Build(root, (name, position, dimensions, color, pickable, parent) =>
            {
                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.name = name;
                piece.transform.SetParent(parent, false);
                piece.transform.localPosition = position;
                piece.transform.localScale = dimensions;
                piece.GetComponent<Collider>().enabled = pickable;
                Renderer renderer = piece.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
                colors.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(colors);
                return piece;
            });

            BatteryPickup battery = null;
            if (part != null)
            {
                Vector3 position = rack.InverseTransformPoint(root.TransformPoint(new Vector3(.11f, -.15f, -.09f)));
                GameObject model = DisplayModel("Batería de la caja", part, rack, position, scale * .86f, 0f);
                battery = model.AddComponent<BatteryPickup>();
                battery.ConfigureWorkshop("level00-taller-caja-bateria");
                model.SetActive(false);
            }

            physical.AddComponent<BatteryCase>().ConfigureWorkshop(title, description, hinge, battery);
        }

        private static void Inspect(Transform item, string text)
        {
            if (item != null) item.gameObject.AddComponent<WorkshopInspection>().Description = text;
        }

        private static void StockPart(Transform root, string name, PrimitiveType type, Vector3 position,
            Vector3 size, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(root, false);
            part.transform.localPosition = position;
            part.transform.localScale = size;
            if (type == PrimitiveType.Cylinder) part.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            if (material != null) part.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static GameObject DisplayModel(string name, GameObject prefab, Transform parent, Vector3 position, float width, float yaw)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var model = Object.Instantiate(prefab, root.transform);
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (Light light in model.GetComponentsInChildren<Light>(true)) light.enabled = false;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return root;
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.sqrMagnitude <= 0.000001f) return root;
            model.transform.localScale *= width / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            Vector3 baseCenter = new(bounds.center.x, bounds.min.y, bounds.center.z);
            model.transform.position += root.transform.position - baseCenter;
            var target = root.AddComponent<BoxCollider>();
            target.center = root.transform.InverseTransformPoint(bounds.center + root.transform.position - baseCenter);
            target.size = Vector3.one * Mathf.Max(0.12f, width);
            return root;
        }

        private static void ImproveLighting(Transform room)
        {
            Transform fixtures = room.Find("Iluminación");
            if (fixtures != null)
                foreach (Light light in fixtures.GetComponentsInChildren<Light>())
                    if (light.name.StartsWith("Fluorescente")) light.intensity *= 1.45f;

            var fill = new GameObject("Relleno · banco y estantes").AddComponent<Light>();
            fill.transform.SetParent(room, false);
            fill.transform.localPosition = new Vector3(1.1f, 2.65f, 1.32f);
            fill.type = LightType.Point;
            fill.color = new Color32(236, 226, 203, 255);
            fill.range = 4.65f;
            fill.intensity = 2.4f;
        }
    }
}
