using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level01MapBuilder
    {
        private static GameObject CreateSchoolSign(CampusPalette palette)
        {
            Transform sign = Group(null, "PF_SchoolSign");
            try
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * 1.51f;
                    Block(sign, "Zapata", new Vector3(x, 0.16f, 0f), new Vector3(0.55f, 0.32f, 0.52f), palette.Cement);
                    Block(sign, "Pilar", new Vector3(x, 1.30f, 0.01f), new Vector3(0.14f, 2.44f, 0.14f), palette.Steel);
                }
                Block(sign, "Marco", new Vector3(0f, 2.33f, 0f), new Vector3(3.90f, 1.12f, 0.15f), palette.Steel);
                Block(sign, "Placa", new Vector3(0f, 2.33f, -0.089f), new Vector3(3.79f, 1.01f, 0.035f), palette.Cream, false);
                Block(sign, "Franja de color", new Vector3(0f, 1.92f, -0.111f),
                    new Vector3(3.79f, 0.19f, 0.012f), palette.Accent, false);
                Label(sign, new Vector3(0f, 2.59f, -0.115f), "ESCUELA RURAL", 1.65f, 0.15f, palette.Ink);
                Label(sign, new Vector3(0f, 2.28f, -0.115f), "LA PRIMERA AULA", 3.35f, 0.33f, palette.Ink);
                return PrefabUtility.SaveAsPrefabAsset(sign.gameObject, LevelPrefabs + "/PF_SchoolSign.prefab");
            }
            finally
            {
                Object.DestroyImmediate(sign.gameObject);
            }
        }

        private static void BuildDirectory(Transform parent, CampusPalette palette)
        {
            Transform sign = Group(parent, "DirectorioPatio", new Vector3(1.32f, 0f, 6.05f), -12f);
            Block(sign, "Poste", Vector3.up * 0.96f, new Vector3(0.13f, 1.92f, 0.13f), palette.Steel);
            Block(sign, "Piedra de apoyo", Vector3.up * 0.12f, new Vector3(0.45f, 0.24f, 0.41f), palette.Cement);
            DirectionPlate(sign, "AULAS", 1.93f, 90f, palette);
            DirectionPlate(sign, "CANCHA", 1.60f, 0f, palette);
            DirectionPlate(sign, "SALA ELÉCTRICA", 1.27f, 0f, palette);
        }

        private static void DirectionPlate(Transform parent, string title, float height, float arrowAngle, CampusPalette palette)
        {
            Vector3 point = new Vector3(0f, height, -0.07f);
            Block(parent, title, point, new Vector3(1.88f, 0.27f, 0.08f), palette.Steel, false);
            Label(parent, point + new Vector3(-0.10f, 0f, -0.045f), title, 1.29f, 0.155f, Color.white);
            Transform arrow = Group(parent, "Flecha", point + new Vector3(0.715f, 0f, -0.052f));
            arrow.localRotation = Quaternion.Euler(0f, 0f, arrowAngle);
            for (int side = -1; side <= 1; side += 2)
                Block(arrow, "Punta", Vector3.up * (side * 0.037f), new Vector3(0.125f, 0.030f, 0.024f), palette.Accent, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, -side * 35f);
        }

        private static void SmallSign(Transform parent, Vector3 position, string title, CampusPalette palette)
        {
            Transform stake = Group(parent, title, position);
            Block(stake, "Poste", Vector3.up * 0.42f, new Vector3(0.075f, 0.84f, 0.075f), palette.Steel, false);
            WallLabel(stake, Vector3.up * 0.76f, 0f, title, palette, 1.12f);
        }

        private static void SiteOutline(Transform parent, Vector3 center, Material marking)
        {
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = center + new Vector3(x * 0.96f, 0f, z * 0.61f);
                Block(parent, "Marca de emplazamiento", corner - Vector3.right * (x * 0.14f),
                    new Vector3(0.31f, 0.014f, 0.052f), marking, false);
                Block(parent, "Marca de emplazamiento", corner - Vector3.forward * (z * 0.14f),
                    new Vector3(0.052f, 0.014f, 0.30f), marking, false);
            }
        }

        private static void WallLabel(Transform parent, Vector3 position, float yaw, string title, CampusPalette palette, float width = 1.18f)
        {
            Transform plaque = Group(parent, title, position, yaw);
            Block(plaque, "Placa", Vector3.zero, new Vector3(width, 0.30f, 0.06f), palette.Steel, false);
            Label(plaque, Vector3.back * 0.035f, title, width - 0.13f, 0.17f, Color.white);
        }

        private static void Label(Transform parent, Vector3 position, string text, float width, float height, Color color)
        {
            Transform node = Group(parent, text, position);
            var label = node.gameObject.AddComponent<TextMesh>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 96;
            label.fontStyle = FontStyle.Bold;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.10f;
            label.color = color;
            label.text = text;

            var renderer = node.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = label.font.material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Bounds bounds = renderer.localBounds;
            if (bounds.size.x > 0f && bounds.size.y > 0f)
                node.localScale = Vector3.one * Mathf.Min(width / bounds.size.x, height / bounds.size.y);
        }

        private static GameObject Block(Transform parent, string name, Vector3 center, Vector3 size, Material material, bool solid = true)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = center;
            block.transform.localScale = size;
            block.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(block.GetComponent<BoxCollider>());
            return block;
        }

        private static void Ramp(Transform parent, string name, Vector3 start, float width, float length, float rise, Material material)
        {
            string path = CampusAssets + "/" + name + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = name };
                AssetDatabase.CreateAsset(mesh, path);
            }

            float x = width * 0.5f;
            const float bottom = -0.09f;
            mesh.Clear();
            mesh.vertices = new[]
            {
                new Vector3(-x, 0f, 0f), new Vector3(x, 0f, 0f), new Vector3(-x, rise, length), new Vector3(x, rise, length),
                new Vector3(-x, bottom, 0f), new Vector3(x, bottom, 0f), new Vector3(-x, bottom, length), new Vector3(x, bottom, length)
            };
            mesh.triangles = new[]
            {
                0, 2, 3, 0, 3, 1, 4, 5, 7, 4, 7, 6, 0, 1, 5, 0, 5, 4,
                2, 6, 7, 2, 7, 3, 0, 4, 6, 0, 6, 2, 1, 3, 7, 1, 7, 5
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssetIfDirty(mesh);
            Transform node = Group(parent, name, start);
            node.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            node.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            node.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int separator = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, separator));
            AssetDatabase.CreateFolder(path.Substring(0, separator), path.Substring(separator + 1));
        }

        private sealed class CampusPalette
        {
            public CampusPalette()
            {
                Steel = LoadMaterial("M_Campus_Azul", new Color32(35, 69, 77, 255));
                Cement = LoadMaterial("M_Campus_Cemento", new Color32(152, 150, 130, 255));
                Cream = LoadMaterial("M_Campus_Crema", new Color32(239, 223, 177, 255));
                Marking = LoadMaterial("M_Campus_Marcas", new Color32(249, 224, 123, 255));
                Accent = LoadMaterial("M_Campus_Terracota", new Color32(188, 104, 66, 255));
                Sky = LoadMaterial("M_Campus_Cielo", Color.white, "Skybox/Procedural");
                Sky.SetColor("_SkyTint", new Color32(142, 178, 194, 255));
                Sky.SetColor("_GroundColor", new Color32(160, 148, 122, 255));
                Sky.SetFloat("_AtmosphereThickness", 0.86f);
                Sky.SetFloat("_Exposure", 1.12f);
                EditorUtility.SetDirty(Sky);
                AssetDatabase.SaveAssetIfDirty(Sky);
            }

            public Material Steel { get; }
            public Material Cement { get; }
            public Material Cream { get; }
            public Material Marking { get; }
            public Material Accent { get; }
            public Material Sky { get; }
            public Color Ink => new Color32(28, 59, 66, 255);

            private static Material LoadMaterial(string name, Color color, string shaderName = "Universal Render Pipeline/Lit")
            {
                string path = CampusAssets + "/" + name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find(shaderName)) { name = name };
                    AssetDatabase.CreateAsset(material, path);
                }
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                    material.SetFloat("_Smoothness", 0.14f);
                }
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
                return material;
            }
        }
    }
}
