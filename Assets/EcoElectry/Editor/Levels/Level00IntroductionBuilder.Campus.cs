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
        private const string CampusDecorationName = "Decoración del campus";
        private const string FantasyTreePath =
            "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab";

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar decoración del campus")]
        internal static void RefreshCampusDecoration()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para decorar el campus.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            Transform school = level != null ? level.transform.Find("SchoolExterior") : null;
            Transform terrains = level != null ? level.transform.Find("Terrain") : null;
            if (school == null || terrains == null)
                throw new InvalidOperationException("El nivel 00 debe tener montado el exterior del colegio y sus terrenos.");

            BuildCampusDecoration(school, TerrainGrid.FromRoot(terrains));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudo guardar la decoración del campus.");
        }

        private static void BuildCampusDecoration(Transform school, TerrainGrid terrain)
        {
            Transform previous = school.Find(CampusDecorationName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Scene scene = school.gameObject.scene;
            Transform campus = Group(scene, school, CampusDecorationName);
            var assets = new CampusAssets();

            Transform entrance = Group(scene, campus, "Entrada y paseo");
            for (int i = 0; i < 3; i++)
            {
                float z = 3409f + i * 15f;
                foreach (int side in new[] { -1, 1 })
                {
                    float x = PedestrianGateX + side * 3.55f;
                    Transform bed = GardenBed(entrance, assets, x, z, ParkingSurface, 2.35f, 5.1f);
                    Plant(assets.Tree, bed, 0f, 0f, 0.29f + i * 0.025f, 30f + 74f * i + 35f * side);
                    Plant(assets.Flowers, bed, 0.1f, -1.48f, 0.78f, 40f);
                    Plant(assets.Shrub, bed, -0.35f, 1.46f, 0.9f, 120f);
                    Plant(assets.Flowers, bed, 0.41f, 1.86f, 0.60f, 82f);
                }
                PlacePrefab(assets.Lamp, entrance, new Vector3(222.15f, ParkingSurface, z + 4.6f), 0f, Vector3.one, "Luminaria del paseo");
            }
            Bench(entrance, assets, 217.55f, ParkingSurface, 3417f, 270f);
            Bench(entrance, assets, 222.45f, ParkingSurface, 3432.7f, 90f);
            Bins(entrance, assets, 222.15f, ParkingSurface, 3402.3f);
            Bins(entrance, assets, 216.9f, ParkingSurface, 3447.2f);
            foreach (float x in new[] { 180f, 229f, 243f, 260f })
            {
                const float z = 3385.5f;
                Transform bed = GardenBed(entrance, assets, x, z, GroundAt(terrain, x, z), 4.0f, 1.65f);
                Plant(assets.Flowers, bed, -1.22f, 0f, 0.76f, 35f);
                Plant(assets.Hedge, bed, 0f, 0.2f, 0.85f, 0f);
                Plant(assets.Flowers, bed, 1.23f, 0f, 0.76f, 85f);
            }

            Transform front = Group(scene, campus, "Plaza escolar");
            const float plazaY = CampusGrade + 0.105f;
            Paving(front, assets, "Pavimento de la plaza", 211.5f, plazaY, 3456.8f, 79f, 13.5f);
            foreach (float x in new[] { 177.5f, 195f, 234.5f, 245.6f })
            {
                Transform bed = GardenBed(front, assets, x, 3460f, plazaY, 4.25f, 3.05f);
                Plant(assets.Tree, bed, 0f, 0.25f, x < 200f ? 0.42f : 0.33f, x * 3f);
                Plant(assets.Flowers, bed, -1.26f, -0.75f, 0.75f, 25f);
                Plant(assets.Hedge, bed, 0.9f, -0.76f, 0.72f, 0f);
                Bench(front, assets, x, plazaY, 3456.9f, 0f);
            }
            NoticeBoard(front, assets, 228.7f, plazaY, 3459.7f);
            Bins(front, assets, 186.1f, plazaY, 3458.4f);
            Bins(front, assets, 241.2f, plazaY, 3455.6f);
            Flagpole(front, assets, 209f, plazaY, 3459.8f);
            PlacePrefab(assets.Lamp, front, new Vector3(187.7f, plazaY, 3461.5f), 0f, Vector3.one, "Luminaria plaza");
            PlacePrefab(assets.Lamp, front, new Vector3(240.2f, plazaY, 3461.5f), 0f, Vector3.one, "Luminaria plaza");

            Transform sides = Group(scene, campus, "Jardines laterales");
            BicycleRack(sides, assets);
            foreach (float z in new[] { 3404f, 3414f, 3440f, 3450f })
                BoundaryGarden(sides, assets, terrain, 164.3f, z, 90f);
            foreach (float z in new[] { 3404f, 3418f, 3432f, 3457f })
                BoundaryGarden(sides, assets, terrain, 274.2f, z, 90f);
            PlacePrefab(assets.Lamp, sides, new Vector3(268.5f, CampusGrade, 3440.1f), 0f, Vector3.one, "Luminaria acceso al bosque");
            Paving(sides, assets, "Patio lateral de descanso", 273f, plazaY, 3480.5f, 9.4f, 12f);
            Bench(sides, assets, 275.6f, plazaY, 3478f, 90f);
            Bench(sides, assets, 275.6f, plazaY, 3483f, 90f);
            BoundaryGarden(sides, assets, terrain, 275.8f, 3491f, 90f);
            Bins(sides, assets, 275.5f, plazaY, 3485.5f);

            Transform rear = Group(scene, campus, "Patio posterior");
            var rearPath = new PathData(new List<Vector3>
            {
                new Vector3(173.5f, plazaY + 0.015f, 3460f),
                new Vector3(166f, plazaY, 3465f),
                new Vector3(166f, plazaY, 3497f),
                new Vector3(170.3f, plazaY, 3506f),
                new Vector3(170.3f, plazaY, 3521.2f)
            });
            var pavement = new MeshBuilder();
            AddRibbon(pavement, rearPath, 1.65f);
            CreateMeshObject(rear, MeshFolder + "/Level00_CampusRearPath.asset",
                "Paseo al patio posterior", pavement, assets.Paving, true);
            Paving(rear, assets, "Patio de mesas", 182f, plazaY + 0.025f, 3527f, 32f, 15f);
            PicnicTable(rear, assets, 176f, plazaY + 0.025f, 3526.5f, 0f);
            PicnicTable(rear, assets, 188.5f, plazaY + 0.025f, 3526.5f, 15f);
            Bins(rear, assets, 170.2f, plazaY + 0.025f, 3531.4f);
            Bench(rear, assets, 194.2f, plazaY + 0.025f, 3526.8f, 90f);
            PlacePrefab(assets.Lamp, rear, new Vector3(192.8f, plazaY + 0.025f, 3532.5f), 0f, Vector3.one, "Luminaria patio");
            foreach (float x in new[] { 178f, 191f, 208f, 228f, 249f, 266f })
                BoundaryGarden(rear, assets, terrain, x, 3536.9f, 0f);

            CampusEdgeVegetation(Group(scene, campus, "Vegetación próxima al perímetro"), terrain, assets);
        }

        private static void BoundaryGarden(Transform parent, CampusAssets assets, TerrainGrid terrain, float x, float z, float yaw)
        {
            Transform bed = GardenBed(parent, assets, x, z, GroundAt(terrain, x, z), 4.3f, 2.1f, yaw);
            Plant(assets.Tree, bed, 0f, 0.1f, 0.37f, x + z);
            Plant(assets.Hedge, bed, -1.35f, 0.22f, 0.8f, 90f);
            Plant(assets.Flowers, bed, 1.1f, -0.45f, 0.84f, 38f);
            Plant(assets.Shrub, bed, 1.54f, 0.32f, 0.74f, 74f);
        }

        private static Transform GardenBed(Transform parent, CampusAssets assets, float x, float z, float y,
            float width, float depth, float yaw = 0f)
        {
            Transform bed = Group(parent.gameObject.scene, parent, "Jardinera", new Vector3(x, y, z));
            bed.rotation = Quaternion.Euler(0f, yaw, 0f);
            Block(bed, "Borde de piedra", new Vector3(0f, 0.15f, 0f), new Vector3(width, 0.30f, depth), assets.Trim);
            Block(bed, "Tierra", new Vector3(0f, 0.295f, 0f), new Vector3(width - 0.24f, 0.045f, depth - 0.24f), assets.Soil, false);
            return Group(parent.gameObject.scene, bed, "Plantación", Vector3.up * 0.315f);
        }

        private static GameObject Plant(GameObject prefab, Transform parent, float x, float z, float scale, float yaw)
        {
            GameObject item = PlacePrefab(prefab, parent, parent.TransformPoint(x, 0f, z),
                parent.eulerAngles.y + yaw, Vector3.one * scale, prefab.name);
            foreach (Collider collider in item.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
            }
            return item;
        }

        private static void Paving(Transform parent, CampusAssets assets, string name, float x, float y, float z, float width, float depth)
        {
            Block(parent, name, new Vector3(x, y - 0.075f, z), new Vector3(width, 0.15f, depth), assets.Paving);
        }

        private static void Bench(Transform parent, CampusAssets assets, float x, float y, float z, float yaw)
        {
            Transform bench = Group(parent.gameObject.scene, parent, "Banco", new Vector3(x, y, z));
            bench.rotation = Quaternion.Euler(0f, yaw, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                Block(bench, "Pata", new Vector3(side * 0.82f, 0.22f, 0f), new Vector3(0.105f, 0.44f, 0.50f), assets.Metal, false);
                Block(bench, "Soporte del respaldo", new Vector3(side * 0.82f, 0.66f, 0.235f),
                    new Vector3(0.055f, 0.52f, 0.065f), assets.Metal, false);
            }
            for (int i = -1; i <= 1; i++)
            {
                Block(bench, "Listón del asiento", new Vector3(0f, 0.46f, i * 0.155f),
                    new Vector3(2.0f, 0.065f, 0.14f), assets.Wood, false);
                Block(bench, "Listón del respaldo", new Vector3(0f, 0.72f + i * 0.115f, 0.25f),
                    new Vector3(2.0f, 0.105f, 0.06f), assets.Wood, false);
            }
            BoxCollider collider = bench.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.24f, 0f);
            collider.size = new Vector3(2f, 0.49f, 0.46f);
        }

        private static void Bins(Transform parent, CampusAssets assets, float x, float y, float z)
        {
            Transform bins = Group(parent.gameObject.scene, parent, "Punto de residuos", new Vector3(x, y, z));
            for (int i = 0; i < 2; i++)
            {
                float offset = i * 0.54f;
                Block(bins, "Contenedor", new Vector3(offset, 0.41f, 0f), new Vector3(0.41f, 0.82f, 0.40f),
                    i == 0 ? assets.Green : assets.Blue);
                Block(bins, "Boca", new Vector3(offset, 0.72f, -0.205f), new Vector3(0.28f, 0.12f, 0.015f), assets.Metal, false);
                Block(bins, "Tapa", new Vector3(offset, 0.86f, 0f), new Vector3(0.46f, 0.08f, 0.45f), assets.Metal, false);
            }
        }

        private static void NoticeBoard(Transform parent, CampusAssets assets, float x, float y, float z)
        {
            Transform board = Group(parent.gameObject.scene, parent, "Cartelera escolar", new Vector3(x, y, z));
            foreach (int side in new[] { -1, 1 })
                Block(board, "Pilar", new Vector3(side * 1.25f, 1.18f, 0f), new Vector3(0.10f, 2.36f, 0.10f), assets.Metal);
            Block(board, "Marco", new Vector3(0f, 1.57f, 0f), new Vector3(2.43f, 1.42f, 0.14f), assets.Green);
            Block(board, "Corcho", new Vector3(0f, 1.48f, -0.078f), new Vector3(2.28f, 0.99f, 0.025f), assets.Wood, false);
            Label(board, new Vector3(0f, 2.12f, -0.079f), "CARTELERA ESCOLAR", 2.14f, 0.18f, assets.Paving, 0.48f);
            for (int i = 0; i < 4; i++)
            {
                float letterX = -0.82f + i * 0.54f;
                float letterY = 1.55f + (i % 2 == 0 ? 0.13f : -0.11f);
                Block(board, "Aviso", new Vector3(letterX, letterY, -0.10f), new Vector3(0.36f, 0.45f, 0.015f),
                    i % 2 == 0 ? assets.Paving : assets.Trim, false);
                for (int line = 0; line < 3; line++)
                    Block(board, "Línea del aviso", new Vector3(letterX, letterY + 0.12f - line * 0.075f, -0.11f),
                        new Vector3(0.24f - line * 0.019f, 0.022f, 0.009f), assets.Green, false);
            }
        }

        private static void Flagpole(Transform parent, CampusAssets assets, float x, float y, float z)
        {
            Transform pole = Group(parent.gameObject.scene, parent, "Mástil del colegio", new Vector3(x, y, z));
            Block(pole, "Pedestal", new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.30f, 1.2f), assets.Trim);
            Block(pole, "Mástil", new Vector3(0f, 3.5f, 0f), new Vector3(0.095f, 7f, 0.095f), assets.Metal);
            for (int i = 0; i < 3; i++)
                Block(pole, "Franja del estandarte", new Vector3(0.95f, 6.66f - i * 0.31f, 0f),
                    new Vector3(1.84f, 0.31f, 0.028f), i == 1 ? assets.Paving : assets.Green, false);
        }

        private static void BicycleRack(Transform parent, CampusAssets assets)
        {
            const float z = 3427f;
            const float y = CampusGrade + 0.08f;
            Paving(parent, assets, "Base para bicicletas", 166.5f, y, z, 4.7f, 7f);
            Transform rack = Group(parent.gameObject.scene, parent, "Bicicletero", new Vector3(165.3f, y, z));
            for (int i = -2; i <= 2; i++)
            {
                float offset = i * 1.02f;
                foreach (int side in new[] { -1, 1 })
                    Block(rack, "Soporte", new Vector3(side * 0.33f, 0.38f, offset),
                        new Vector3(0.075f, 0.76f, 0.075f), assets.Metal);
                Block(rack, "Travesaño", new Vector3(0f, 0.76f, offset), new Vector3(0.735f, 0.075f, 0.075f), assets.Metal);
            }
        }

        private static void PicnicTable(Transform parent, CampusAssets assets, float x, float y, float z, float yaw)
        {
            Transform table = Group(parent.gameObject.scene, parent, "Mesa de patio", new Vector3(x, y, z));
            table.rotation = Quaternion.Euler(0f, yaw, 0f);
            Block(table, "Tablero", new Vector3(0f, 0.75f, 0f), new Vector3(2.5f, 0.085f, 0.88f), assets.Wood);
            foreach (int side in new[] { -1, 1 })
            {
                Block(table, "Pata", new Vector3(side * 1.03f, 0.355f, 0f), new Vector3(0.14f, 0.71f, 0.62f), assets.Metal);
                Block(table, "Banco", new Vector3(0f, 0.45f, side * 0.81f), new Vector3(2.5f, 0.08f, 0.32f), assets.Wood);
                Block(table, "Soporte de bancos", new Vector3(side * 1.03f, 0.37f, 0f),
                    new Vector3(0.12f, 0.08f, 1.96f), assets.Metal, false);
            }
        }

        private static void CampusEdgeVegetation(Transform parent, TerrainGrid terrain, CampusAssets assets)
        {
            var existingTrees = new List<Vector2>();
            foreach (Terrain tile in terrain.Tiles)
            {
                if (tile == null) continue;
                Vector3 origin = tile.transform.position;
                if (origin.x > 313f || origin.x + TileSize < 127f ||
                    origin.z > 3574f || origin.z + TileSize < 3364f) continue;
                foreach (TreeInstance instance in tile.terrainData.treeInstances)
                    existingTrees.Add(new Vector2(origin.x + instance.position.x * TileSize,
                        origin.z + instance.position.z * TileSize));
            }

            var candidates = new List<Vector2>();
            for (int i = 0; i < 12; i++)
            {
                float z = 3396f + i * 12.3f;
                candidates.Add(new Vector2(149f - i % 3 * 6f, z));
                if (z < 3432f || z > 3460f)
                    candidates.Add(new Vector2(290f + i % 3 * 7f, z + 3f));
                candidates.Add(new Vector2(160f + i * 10.8f, 3549f + i % 3 * 6f));
            }
            candidates.AddRange(new[] { new Vector2(162f, 3375f), new Vector2(177f, 3380f),
                new Vector2(252f, 3378f), new Vector2(264f, 3381f), new Vector2(278f, 3373f) });

            var random = new System.Random(180);
            PathData road = BuildPath(RoadControls, 4f);
            PathData trail = BuildPath(ForestTrailControls, 2f);
            foreach (Vector2 point in candidates)
            {
                if (!VegetationAllowed(point.x, point.y, road, trail, 5f)) continue;
                if (existingTrees.TrueForAll(other => (other - point).sqrMagnitude >= 42f))
                {
                    float scale = Mathf.Lerp(0.52f, 0.83f, (float)random.NextDouble());
                    PlacePrefab(assets.Tree, parent, new Vector3(point.x, GroundAt(terrain, point.x, point.y), point.y),
                        (float)random.NextDouble() * 360f, Vector3.one * scale, "Árbol del perímetro");
                    existingTrees.Add(point);
                }

                for (int i = 0; i < 3; i++)
                {
                    float x = point.x + Mathf.Lerp(-3.1f, 3.1f, (float)random.NextDouble());
                    float z = point.y + Mathf.Lerp(-2.5f, 2.5f, (float)random.NextDouble());
                    if (!VegetationAllowed(x, z, road, trail, 2.8f)) continue;
                    Vector3 position = new Vector3(x, GroundAt(terrain, x, z), z);
                    Transform clump = Group(parent.gameObject.scene, parent, "Sotobosque del perímetro", position);
                    bool grass = i == 1;
                    GameObject plant = Plant(grass ? assets.Grass : assets.Shrub, clump, 0f, 0f,
                        grass ? 1.4f : 1.15f, (float)random.NextDouble() * 360f);
                    if (grass) ApplyMaterialOverride(plant, assets.GrassMaterial);
                }
            }
        }

        private sealed class CampusAssets
        {
            public readonly Material Paving = CampusMaterial("Pavimento", new Color32(152, 143, 123, 255));
            public readonly Material Trim = CampusMaterial("Borde", new Color32(191, 179, 151, 255));
            public readonly Material Soil = CampusMaterial("Tierra", new Color32(77, 65, 49, 255));
            public readonly Material Wood = CampusMaterial("Madera", new Color32(136, 86, 51, 255));
            public readonly Material Metal = CampusMaterial("Metal", new Color32(51, 60, 59, 255));
            public readonly Material Green = CampusMaterial("Verde", new Color32(40, 80, 64, 255));
            public readonly Material Blue = CampusMaterial("Azul", new Color32(52, 91, 109, 255));
            public readonly GameObject Tree = LoadPrefab(FantasyTreePath);
            public readonly GameObject Grass = LoadPrefab("Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab");
            public readonly GameObject Lamp = LoadPrefab("Assets/ResourcePack/LuminariaExterior/Prefabs/LuminariaSolarPoste.prefab");
            public readonly GameObject Flowers = LoadPrefab("Assets/ResourcePack/Arbusto/Prefabs/ArbustoFlorido.prefab");
            public readonly GameObject Shrub = LoadPrefab("Assets/ResourcePack/Arbusto/Prefabs/ArbustoBajo.prefab");
            public readonly GameObject Hedge = LoadPrefab("Assets/ResourcePack/Arbusto/Prefabs/Seto.prefab");
            public readonly Material GrassMaterial;

            public CampusAssets()
            {
                Material original = Grass.GetComponentInChildren<Renderer>().sharedMaterial;
                GrassMaterial = CampusMaterial("Pasto", original.GetColor("_Color"));
                GrassMaterial.SetTexture("_BaseMap", original.GetTexture("_MainTex"));
                GrassMaterial.SetFloat("_Cull", 0f);
                GrassMaterial.SetFloat("_AlphaClip", 1f);
                GrassMaterial.SetFloat("_Cutoff", original.GetFloat("_Cutoff"));
                GrassMaterial.EnableKeyword("_ALPHATEST_ON");
                GrassMaterial.SetOverrideTag("RenderType", "TransparentCutout");
                GrassMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                EditorUtility.SetDirty(GrassMaterial);
                AssetDatabase.SaveAssetIfDirty(GrassMaterial);
            }

            private static Material CampusMaterial(string name, Color color) =>
                CreateMaterial("M_Level00_Campus_" + name, color, "Universal Render Pipeline/Lit", 0.16f);
        }
    }
}
