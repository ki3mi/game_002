using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const float WorkshopFloor = 0.20f;
        private const float WorkshopCeiling = 3.27f;
        private const float WorkshopX = 260f;
        private const float WorkshopLandingWidth = 2.37f;
        private static readonly Vector3 VhsAnchor = new Vector3(0.58f, 1.22f, 2.78f);

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar sala de mantenimiento")]
        internal static void RefreshMaintenance()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para actualizar mantenimiento.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            Transform maintenance = level != null ? level.transform.Find("Maintenance") : null;
            Transform room = maintenance != null ? maintenance.Find("SalaDeMantenimiento") : null;
            if (room == null) throw new InvalidOperationException("No se encontró la sala de mantenimiento.");

            for (int i = room.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(room.GetChild(i).gameObject);
            Transform oldLink = maintenance.Find("EnlaceCubierto");
            if (oldLink != null) Object.DestroyImmediate(oldLink.gameObject);

            BuildMaintenanceRoom(room, new WorkshopMaterials());
            Transform anchor = level.transform.Find("Markers/VHSPlaybackPoint");
            if (anchor != null)
                anchor.SetPositionAndRotation(room.TransformPoint(VhsAnchor), Quaternion.LookRotation(-room.forward));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudo guardar la sala de mantenimiento.");
        }

        private static void BuildMaintenance(Transform level, TerrainGrid terrain)
        {
            Scene scene = level.gameObject.scene;
            Transform maintenance = Group(scene, level, "Maintenance");
            Transform room = Group(scene, maintenance, "SalaDeMantenimiento",
                new Vector3(WorkshopX, GroundAt(terrain, WorkshopX, 3470f), 3470f));
            BuildMaintenanceRoom(room, new WorkshopMaterials());
        }

        private static void BuildMaintenanceRoom(Transform room, WorkshopMaterials materials)
        {
            Scene scene = room.gameObject.scene;
            BuildWorkshopShell(Group(scene, room, "Envolvente"), materials);
            BuildWorkshopEntrance(Group(scene, room, "Acceso"), materials);
            BuildVideoBench(Group(scene, room, "Banco de trabajo y vídeo"), materials);
            BuildWorkshopStorage(Group(scene, room, "Almacenamiento y electricidad"), materials);
            BuildWorkshopLights(Group(scene, room, "Iluminación"), materials);
        }

        private static void BuildWorkshopShell(Transform shell, WorkshopMaterials m)
        {
            Block(shell, "Losa", new Vector3(0f, 0.08f, 0f), new Vector3(10f, 0.24f, 8f), m.Floor);
            Block(shell, "Cubierta", new Vector3(0f, WorkshopCeiling + 0.11f, 0f),
                new Vector3(10.34f, 0.22f, 8.36f), m.Metal);
            Block(shell, "Cielo raso", new Vector3(0f, WorkshopCeiling - 0.022f, 0f),
                new Vector3(9.54f, 0.044f, 7.54f), m.Wall, false);
            for (int i = -3; i <= 3; i++)
                Block(shell, "Junta del cielo raso", new Vector3(0f, WorkshopCeiling - 0.045f, i * 0.95f),
                    new Vector3(9.51f, 0.006f, 0.014f), m.Floor, false);

            WallPanel(shell, m, 0f, 3.88f, 10f, 0.24f);
            WallPanel(shell, m, -4.88f, 0f, 0.24f, 7.52f);
            WallPanel(shell, m, 4.88f, 0f, 0.24f, 7.52f);
            const float front = -3.88f;
            WallPanel(shell, m, -2.875f, front, 4.25f, 0.24f);
            WallPanel(shell, m, 1.55f, front, 1.60f, 0.24f);
            WallPanel(shell, m, 4.525f, front, 0.95f, 0.24f);
            Block(shell, "Dintel", new Vector3(0f, 2.86f, front), new Vector3(1.50f, 0.82f, 0.24f), m.Wall);
            Block(shell, "Antepecho de ventana", new Vector3(3.20f, 0.613f, front),
                new Vector3(1.70f, 1.226f, 0.24f), m.Wall);
            Block(shell, "Franja baja de ventana", new Vector3(3.20f, 0.51f, -4.005f),
                new Vector3(1.70f, 1.02f, 0.015f), m.Green, false);
            Block(shell, "Dintel de ventana", new Vector3(3.20f, 2.785f, front),
                new Vector3(1.70f, 0.97f, 0.24f), m.Wall);
            PlaceWorkshopProp(shell, "Ventana", "VentanaFija", new Vector3(3.20f, 1.22f, front),
                0f, new Vector3(1.40f, 0.91f, 2f));
            Block(shell, "Alféizar", new Vector3(3.20f, 1.209f, front), new Vector3(1.76f, 0.07f, 0.39f), m.Metal);

            foreach (float z in new[] { -2.15f, 0f, 2.15f })
                Block(shell, "Junta del piso", new Vector3(0f, WorkshopFloor + 0.005f, z),
                    new Vector3(9.5f, 0.006f, 0.019f), m.Seam, false);
        }

        private static void WallPanel(Transform shell, WorkshopMaterials m, float x, float z, float width, float depth)
        {
            const float split = 1.02f;
            Block(shell, "Zócalo pintado", new Vector3(x, split * 0.5f, z), new Vector3(width, split, depth), m.Green);
            Block(shell, "Pared clara", new Vector3(x, (split + WorkshopCeiling) * 0.5f, z),
                new Vector3(width, WorkshopCeiling - split, depth), m.Wall);
        }

        private static void BuildWorkshopEntrance(Transform entry, WorkshopMaterials m)
        {
            GameObject door = PlaceWorkshopProp(entry, "Puerta", "PuertaExteriorMetalica",
                new Vector3(0f, WorkshopFloor, -3.88f), 0f, new Vector3(1.75f, 1.08f, 1.37f));
            Transform hinge = door.transform.Find("PuertaExteriorMetalica_Ensamble/PuertaExteriorMetalica_PivoteBisagra");
            hinge.localRotation = Quaternion.Euler(0f, 103f, 0f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hinge);

            Block(entry, "Descanso de acceso", new Vector3(0f, 0.145f, -5.10f),
                new Vector3(WorkshopLandingWidth, 0.098f, 2.21f), m.Floor);
            Block(entry, "Marquesina", new Vector3(0f, 2.73f, -4.48f), new Vector3(3.48f, 0.14f, 1.10f), m.Green);
            foreach (float x in new[] { -1.48f, 1.48f })
                Block(entry, "Soporte marquesina", new Vector3(x, 2.53f, -4.13f),
                    new Vector3(0.08f, 0.34f, 0.51f), m.Metal, false);
            Block(entry, "Placa mantenimiento", new Vector3(0f, 2.97f, -4.035f),
                new Vector3(3.56f, 0.40f, 0.055f), m.Green, false);
            Label(entry, new Vector3(0f, 2.97f, -4.069f), "MANTENIMIENTO", 3.33f, 0.25f, m.Paper, 0.7f);
            Block(entry, "Placa de sala", new Vector3(-1.54f, 1.66f, -4.025f),
                new Vector3(0.55f, 0.31f, 0.04f), m.Green, false);
            Label(entry, new Vector3(-1.54f, 1.66f, -4.053f), "M-01", 0.43f, 0.17f, m.Paper, 0.56f);
            PlaceWorkshopProp(entry, "LuminariaExterior", "LuminariaSolarMural", new Vector3(1.23f, 2.43f, -4.018f),
                180f, Vector3.one * 1.25f);
            PlaceWorkshopProp(entry, "LuminariaInterior", "LuminariaEmergencia", new Vector3(0f, 2.55f, -3.742f),
                0f, Vector3.one * 1.4f);
            PlaceWorkshopProp(entry, "Interruptor", "InterruptorPared", new Vector3(-1.06f, 1.31f, -3.747f),
                0f, Vector3.one);
        }

        private static void BuildVideoBench(Transform work, WorkshopMaterials m)
        {
            const float benchZ = 2.96f;
            const float top = 1.115f;
            Block(work, "Tablero de madera", new Vector3(-0.31f, top - 0.055f, benchZ),
                new Vector3(4.32f, 0.11f, 0.92f), m.Wood);
            foreach (float x in new[] { -2.28f, 1.65f })
            foreach (float z in new[] { 2.59f, 3.34f })
                Block(work, "Pata de acero", new Vector3(x, 0.63f, z), new Vector3(0.09f, 0.86f, 0.09f), m.Metal);
            foreach (float z in new[] { 2.59f, 3.34f })
                Block(work, "Travesaño del banco", new Vector3(-0.31f, 0.55f, z),
                    new Vector3(3.96f, 0.055f, 0.045f), m.Metal, false);
            Block(work, "Cajonera", new Vector3(-1.91f, 0.61f, benchZ), new Vector3(0.64f, 0.79f, 0.68f), m.Green);
            for (int i = 0; i < 3; i++)
            {
                float y = 0.38f + i * 0.23f;
                Block(work, "Frente de cajón", new Vector3(-1.91f, y, 2.607f), new Vector3(0.57f, 0.19f, 0.026f), m.Metal, false);
                Block(work, "Tirador de cajón", new Vector3(-1.91f, y, 2.572f), new Vector3(0.19f, 0.035f, 0.044f), m.Silver, false);
            }
            PlaceWorkshopProp(work, "Silla", "SillaApilable", new Vector3(-1.38f, WorkshopFloor, 1.92f),
                158f, Vector3.one);
            BuildVideoEquipment(work, m, top);
            BuildToolBoard(work, m);

            Block(work, "Base del tornillo de banco", new Vector3(-1.85f, top + 0.035f, 2.76f),
                new Vector3(0.29f, 0.07f, 0.27f), m.Metal, false);
            Block(work, "Cuerpo del tornillo de banco", new Vector3(-1.85f, top + 0.135f, 2.78f),
                new Vector3(0.22f, 0.16f, 0.21f), m.Blue, false);
            foreach (float z in new[] { 2.68f, 2.84f })
                Block(work, "Mordaza", new Vector3(-1.85f, top + 0.21f, z), new Vector3(0.25f, 0.06f, 0.045f), m.Silver, false);
            Block(work, "Maneral", new Vector3(-1.85f, top + 0.115f, 2.628f),
                new Vector3(0.32f, 0.023f, 0.025f), m.Silver, false);
        }

        private static void BuildVideoEquipment(Transform work, WorkshopMaterials m, float top)
        {
            Transform crt = Group(work.gameObject.scene, work, "Televisor CRT", new Vector3(-0.29f, top, 2.98f));
            Block(crt, "Carcasa", new Vector3(0f, 0.35f, 0.031f), new Vector3(0.88f, 0.66f, 0.66f), m.Metal);
            Block(crt, "Marco frontal", new Vector3(0f, 0.355f, -0.329f), new Vector3(0.91f, 0.65f, 0.080f), m.Rubber, false);
            Block(crt, "Bisel", new Vector3(-0.046f, 0.383f, -0.374f), new Vector3(0.72f, 0.51f, 0.035f), m.Silver, false);
            Block(crt, "Vidrio oscuro", new Vector3(-0.046f, 0.385f, -0.401f), new Vector3(0.65f, 0.44f, 0.038f), m.Screen, false);
            for (int i = 0; i < 5; i++)
                Block(crt, "Rejilla de audio", new Vector3(0.362f, 0.28f + i * 0.05f, -0.372f),
                    new Vector3(0.082f, 0.014f, 0.007f), m.Silver, false);
            Block(crt, "Piloto", new Vector3(0.36f, 0.145f, -0.373f), new Vector3(0.024f, 0.014f, 0.009f), m.Orange, false);
            Label(crt, new Vector3(-0.045f, 0.087f, -0.373f), "MONITOR", 0.25f, 0.035f, m.Paper, 0.25f);

            Transform vcr = Group(work.gameObject.scene, work, "Videocasetera VHS", new Vector3(0.58f, top, 2.97f));
            Block(vcr, "Chasis", new Vector3(0f, 0.067f, 0f), new Vector3(0.63f, 0.134f, 0.43f), m.Metal);
            Block(vcr, "Bandeja de cinta", new Vector3(-0.098f, 0.088f, -0.219f),
                new Vector3(0.28f, 0.049f, 0.011f), m.Rubber, false);
            Block(vcr, "Display", new Vector3(0.168f, 0.090f, -0.221f), new Vector3(0.19f, 0.044f, 0.013f), m.Screen, false);
            for (int i = 0; i < 4; i++)
                Block(vcr, "Botón de reproducción", new Vector3(0.088f + i * 0.050f, 0.033f, -0.222f),
                    new Vector3(0.034f, 0.019f, 0.014f), m.Silver, false);
            Label(vcr, new Vector3(0.17f, 0.092f, -0.230f), "00:00", 0.15f, 0.027f, m.Amber, 0.32f);

            Transform cassette = Group(work.gameObject.scene, work, "Cinta VHS del encargo", new Vector3(1.27f, top, 2.74f));
            cassette.localRotation = Quaternion.Euler(0f, -12f, 0f);
            Block(cassette, "Estuche", new Vector3(0f, 0.024f, 0f), new Vector3(0.34f, 0.048f, 0.22f), m.Orange, false);
            Block(cassette, "Carcasa de cinta", new Vector3(0f, 0.070f, -0.012f), new Vector3(0.24f, 0.044f, 0.155f), m.Rubber, false);
            Block(cassette, "Etiqueta", new Vector3(0f, 0.093f, -0.015f), new Vector3(0.19f, 0.005f, 0.073f), m.Paper, false);
            Transform title = Group(work.gameObject.scene, cassette, "Identificación de la cinta", new Vector3(0f, 0.097f, -0.015f));
            title.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Label(title, Vector3.zero, "ENCARGO 01", 0.17f, 0.030f, m.Rubber, 0.25f);
            Label(cassette, new Vector3(0f, 0.027f, -0.113f), "ENCARGO 01", 0.30f, 0.028f, m.Paper, 0.24f);
        }

        private static void BuildToolBoard(Transform work, WorkshopMaterials m)
        {
            Transform board = Group(work.gameObject.scene, work, "Panel de herramientas", new Vector3(-0.83f, 2.05f, 3.777f));
            Block(board, "Base perforada", Vector3.zero, new Vector3(3.18f, 1.27f, 0.075f), m.Wood, false);
            Block(board, "Encabezado", new Vector3(0f, 0.49f, -0.048f), new Vector3(3.18f, 0.25f, 0.032f), m.Green, false);
            Label(board, new Vector3(0f, 0.49f, -0.067f), "HERRAMIENTAS", 2.08f, 0.14f, m.Paper, 0.46f);
            for (int i = 0; i < 13; i++)
            for (int row = 0; row < 3; row++)
                Block(board, "Perforación", new Vector3(-1.32f + i * 0.22f, -0.41f + row * 0.25f, -0.040f),
                    new Vector3(0.028f, 0.028f, 0.007f), m.Rubber, false);
            for (int i = 0; i < 4; i++)
            {
                float x = -1.18f + i * 0.23f;
                float length = 0.43f - i * 0.045f;
                Block(board, "Llave", new Vector3(x, 0.25f - length * 0.5f, -0.105f),
                    new Vector3(0.051f, length, 0.043f), m.Silver, false);
                Block(board, "Cabeza de llave", new Vector3(x, 0.22f, -0.107f), new Vector3(0.115f, 0.10f, 0.044f), m.Silver, false);
            }
            for (int i = 0; i < 3; i++)
            {
                float x = 0.18f + i * 0.27f;
                Block(board, "Vástago", new Vector3(x, 0.07f, -0.11f), new Vector3(0.028f, 0.33f, 0.030f), m.Silver, false);
                Block(board, "Mango", new Vector3(x, -0.14f, -0.11f), new Vector3(0.080f, 0.18f, 0.074f),
                    i % 2 == 0 ? m.Orange : m.Blue, false);
            }
            Transform hammer = Group(work.gameObject.scene, board, "Martillo", new Vector3(1.17f, -0.06f, -0.11f));
            hammer.localRotation = Quaternion.Euler(0f, 0f, -19f);
            Block(hammer, "Mango", Vector3.zero, new Vector3(0.065f, 0.47f, 0.07f), m.Metal, false);
            Block(hammer, "Cabeza", new Vector3(0f, 0.22f, 0f), new Vector3(0.24f, 0.105f, 0.095f), m.Silver, false);
        }

        private static void BuildWorkshopStorage(Transform storage, WorkshopMaterials m)
        {
            Block(storage, "Armario metálico", new Vector3(-3.80f, 1.26f, 3.03f), new Vector3(1.29f, 2.12f, 0.65f), m.Green);
            foreach (float x in new[] { -4.11f, -3.49f })
            {
                Block(storage, "Puerta de armario", new Vector3(x, 1.27f, 2.695f), new Vector3(0.58f, 1.97f, 0.026f), m.Metal, false);
                Block(storage, "Tirador", new Vector3(x + 0.17f, 1.30f, 2.647f), new Vector3(0.035f, 0.16f, 0.065f), m.Silver, false);
                for (int i = 0; i < 4; i++)
                    Block(storage, "Ventilación de armario", new Vector3(x, 2.03f - i * 0.063f, 2.680f),
                        new Vector3(0.32f, 0.024f, 0.009f), m.Rubber, false);
            }

            PlaceWorkshopProp(storage, "TablerosElectrico", "TableroDistribucion", new Vector3(-4.75f, 1.35f, -0.92f),
                90f, Vector3.one);
            Transform panelSign = Group(storage.gameObject.scene, storage, "Identificación del cuadro",
                new Vector3(-4.73f, 1.955f, -0.92f));
            panelSign.localRotation = Quaternion.Euler(0f, -90f, 0f);
            Block(panelSign, "Placa", Vector3.zero, new Vector3(0.64f, 0.19f, 0.03f), m.Green, false);
            Label(panelSign, new Vector3(0f, 0f, -0.022f), "CUADRO ELÉCTRICO", 0.58f, 0.084f, m.Paper, 0.38f);
            Transform conduit = PlaceWorkshopProp(storage, "Canaleta", "CanaletaIndustrial",
                new Vector3(-4.70f, 2.28f, -0.92f), 0f, Vector3.one).transform;
            conduit.localRotation = Quaternion.Euler(0f, 0f, 90f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(conduit);
            PlaceWorkshopProp(storage, "Tomacorriente", "TomacorrienteIndustrial", new Vector3(1.77f, 1.27f, 3.747f),
                180f, Vector3.one);
            PlaceWorkshopProp(storage, "Ventilador", "VentiladorPared", new Vector3(3.60f, 2.71f, 3.746f),
                180f, Vector3.one);

            Transform rack = Group(storage.gameObject.scene, storage, "Estantería de repuestos", new Vector3(4.13f, WorkshopFloor, 1.18f));
            rack.localRotation = Quaternion.Euler(0f, 90f, 0f);
            foreach (float x in new[] { -1.15f, 1.15f })
            foreach (float z in new[] { -0.31f, 0.31f })
                Block(rack, "Montante", new Vector3(x, 1.13f, z), new Vector3(0.06f, 2.26f, 0.06f), m.Metal);
            for (int i = 0; i < 4; i++)
                Block(rack, "Bandeja", new Vector3(0f, 0.27f + i * 0.56f, 0f), new Vector3(2.35f, 0.055f, 0.69f), m.Metal);
            WorkshopBox(rack, m, -0.72f, 0.30f, 0.02f, new Vector3(0.67f, 0.39f, 0.49f), "CABLES");
            WorkshopBox(rack, m, 0.52f, 0.30f, 0.03f, new Vector3(0.76f, 0.45f, 0.49f), "REPUESTOS");
            WorkshopBox(rack, m, -0.45f, 0.86f, -0.04f, new Vector3(0.92f, 0.34f, 0.45f), "FIJACIONES");
            WorkshopBox(rack, m, 0.57f, 1.42f, 0.02f, new Vector3(0.75f, 0.37f, 0.47f), "CONECTORES");
            WorkshopBox(rack, m, -0.60f, 1.98f, 0.04f, new Vector3(0.69f, 0.28f, 0.42f), "TUBOS");
            Transform reel = Group(storage.gameObject.scene, storage, "Carrete de cable", new Vector3(-3.98f, WorkshopFloor + 0.39f, 0.94f));
            WorkshopCylinder(reel, "Cable bobinado", Vector3.zero, 0.58f, 0.33f, m.Rubber);
            foreach (float offset in new[] { -0.195f, 0.195f })
                WorkshopCylinder(reel, "Disco del carrete", new Vector3(0f, 0f, offset), 0.78f, 0.055f, m.Wood);
            WorkshopCylinder(reel, "Eje", new Vector3(0f, 0f, -0.236f), 0.19f, 0.047f, m.Metal);
            BoxCollider reelCollider = reel.gameObject.AddComponent<BoxCollider>();
            reelCollider.size = new Vector3(0.74f, 0.76f, 0.48f);
        }

        private static void WorkshopBox(Transform parent, WorkshopMaterials m, float x, float y, float z, Vector3 size, string name)
        {
            Block(parent, "Caja de " + name, new Vector3(x, y + size.y * 0.5f, z), size, m.Cardboard);
            Block(parent, "Cinta adhesiva", new Vector3(x, y + size.y + 0.002f, z),
                new Vector3(0.09f, 0.005f, size.z), m.Paper, false);
            Vector3 label = new Vector3(x, y + size.y * 0.59f, z - size.z * 0.5f - 0.006f);
            Block(parent, "Etiqueta de caja", label, new Vector3(size.x * 0.70f, 0.15f, 0.015f), m.Paper, false);
            Label(parent, label + Vector3.back * 0.010f, name, size.x * 0.64f, 0.068f, m.Rubber, 0.31f);
        }

        private static void WorkshopCylinder(Transform parent, string name, Vector3 position, float diameter, float depth, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = position;
            cylinder.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cylinder.transform.localScale = new Vector3(diameter, depth * 0.5f, diameter);
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(cylinder.GetComponent<Collider>());
            cylinder.isStatic = true;
        }

        private static void BuildWorkshopLights(Transform lights, WorkshopMaterials m)
        {
            foreach (float x in new[] { -2.13f, 2.13f })
            {
                Block(lights, "Carcasa del fluorescente", new Vector3(x, 3.19f, -0.32f),
                    new Vector3(1.56f, 0.12f, 0.27f), m.Metal);
                PlaceWorkshopProp(lights, "LuminariaInterior", "TuboLED", new Vector3(x, 3.105f, -0.32f),
                    0f, new Vector3(1.16f, 1f, 2.6f));
                AddWorkshopSpot(lights, "Fluorescente frío", new Vector3(x, 3.075f, -0.32f),
                    new Vector3(x * 0.67f, WorkshopFloor, 0.66f), new Color32(191, 220, 255, 255), 5.25f, 6.2f, 106f);
            }
            Block(lights, "Luz del banco", new Vector3(0.07f, 2.50f, 3.08f),
                new Vector3(2.40f, 0.14f, 0.27f), m.Metal, false);
            Block(lights, "Difusor del banco", new Vector3(0.07f, 2.424f, 3.08f),
                new Vector3(2.23f, 0.032f, 0.16f), m.Amber, false);
            AddWorkshopSpot(lights, "Luz cálida del vídeo", new Vector3(0.18f, 2.40f, 2.91f),
                new Vector3(0.18f, 1.06f, 2.83f), new Color32(255, 202, 130, 255), 3.35f, 3.4f, 114f);
        }

        private static void AddWorkshopSpot(Transform parent, string name, Vector3 position, Vector3 target,
            Color color, float intensity, float range, float angle)
        {
            Transform fixture = Group(parent.gameObject.scene, parent, name, position);
            fixture.localRotation = Quaternion.LookRotation(target - position, Vector3.forward);
            Light light = fixture.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.innerSpotAngle = angle * 0.71f;
            light.shadows = LightShadows.Soft;
        }

        private static GameObject PlaceWorkshopProp(Transform parent, string family, string name,
            Vector3 position, float yaw, Vector3 scale)
        {
            GameObject prefab = LoadPrefab($"Assets/ResourcePack/{family}/Prefabs/{name}.prefab");
            GameObject instance = PlacePrefab(prefab, parent, parent.TransformPoint(position), yaw + parent.eulerAngles.y, scale, name);
            foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || !behaviour.enabled) continue;
                behaviour.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            }
            return instance;
        }

        private sealed class WorkshopMaterials
        {
            public readonly Material Wall = Finish("Yeso", 208, 202, 179);
            public readonly Material Green = Finish("VerdeServicio", 63, 98, 87);
            public readonly Material Floor = Finish("Hormigon", 121, 123, 115);
            public readonly Material Seam = Finish("Junta", 104, 107, 99);
            public readonly Material Wood = Finish("BancoMadera", 143, 104, 66);
            public readonly Material Metal = Finish("Acero", 54, 64, 66, 0.30f);
            public readonly Material Rubber = Finish("Grafito", 26, 31, 32);
            public readonly Material Silver = Finish("Aluminio", 161, 163, 151, 0.43f);
            public readonly Material Paper = Finish("Papel", 238, 223, 180);
            public readonly Material Screen = Finish("VidrioCRT", 35, 57, 53, 0.80f);
            public readonly Material Amber = Finish("Ambar", 240, 175, 86, 0.32f);
            public readonly Material Orange = Finish("CajaRoja", 153, 71, 44);
            public readonly Material Blue = Finish("HerramientasAzul", 57, 113, 122);
            public readonly Material Cardboard = Finish("Carton", 151, 117, 75);

            private static Material Finish(string name, byte r, byte g, byte b, float smoothness = 0.14f) =>
                CreateMaterial("M_Level00_Mantenimiento_" + name, new Color32(r, g, b, 255),
                    "Universal Render Pipeline/Lit", smoothness);
        }
    }
}
