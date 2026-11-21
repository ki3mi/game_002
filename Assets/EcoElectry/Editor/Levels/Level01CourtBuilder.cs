using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static class Level01CourtBuilder
    {
        private const string PrefabRoot = "Assets/EcoElectry/Content/Prefabs";
        private const string LevelFolder = PrefabRoot + "/Level01";
        private const string CourtFolder = LevelFolder + "/Court";
        private const string CourtPath = LevelFolder + "/PF_BasketballCourt.prefab";
        private const string HoopPath = LevelFolder + "/PF_BasketballHoop.prefab";

        private const float SurfaceHeight = 0.025f;
        private const float Baseline = 11.30f;
        private const float Sideline = 6.25f;
        private const float PoleX = 11.62f;
        private const float RimOffset = 1.54f;
        private const float RimHeight = 3.05f;
        private const float LineWidth = 0.07f;

        public static GameObject CreatePrefab()
        {
            EnsureFolder(PrefabRoot, "Level01");
            EnsureFolder(LevelFolder, "Court");

            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject hoop = CreateHoopPrefab(preview);
                return CreateCourtPrefab(preview, hoop);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static GameObject CreateCourtPrefab(Scene preview, GameObject hoopPrefab)
        {
            Material concrete = SaveMaterial("M_CourtConcrete", new Color32(147, 139, 115, 255));
            Material terracotta = SaveMaterial("M_CourtTerracotta", new Color32(165, 95, 75, 255));
            Material petrol = SaveMaterial("M_CourtPaintPetrol", new Color32(57, 93, 90, 255));
            Material paint = SaveMaterial("M_CourtPaintIvory", new Color32(248, 229, 192, 255));

            GameObject court = NewRoot("PF_BasketballCourt", preview);
            AddBox(court.transform, "Losa", new Vector3(0f, SurfaceHeight - 0.10f, 0f),
                new Vector3(24f, 0.20f, 14f), concrete, solid: true);

            var finish = new Geometry();
            finish.Rectangle(-11.92f, -6.92f, 11.92f, 6.92f, SurfaceHeight + 0.001f);
            AddMesh(court.transform, "PavimentoTerracota", SaveMesh("SM_CourtSurfacePaint", finish), terracotta);

            var lanes = new Geometry();
            lanes.Rectangle(-Baseline, -1.82f, -6.34f, 1.82f, SurfaceHeight + 0.003f);
            lanes.Rectangle(6.34f, -1.82f, Baseline, 1.82f, SurfaceHeight + 0.003f);
            lanes.Arc(Vector2.zero, 1.48f, 0f, 360f, 48, 0.17f, SurfaceHeight + 0.003f);
            AddMesh(court.transform, "PinturaZonas", SaveMesh("SM_CourtKeyPaint", lanes), petrol);

            Geometry marking = CreateMarkings();
            AddMesh(court.transform, "Demarcacion", SaveMesh("SM_CourtMarkings", marking), paint);

            PlaceHoop(hoopPrefab, court.transform, "CanastaOeste", -PoleX, 90f);
            PlaceHoop(hoopPrefab, court.transform, "CanastaEste", PoleX, -90f);
            return SavePrefab(court, CourtPath);
        }

        private static Geometry CreateMarkings()
        {
            const float y = SurfaceHeight + 0.006f;
            var lines = new Geometry();
            lines.Stroke(-Baseline, -Sideline, Baseline, -Sideline, LineWidth, y);
            lines.Stroke(-Baseline, Sideline, Baseline, Sideline, LineWidth, y);
            lines.Stroke(-Baseline, -Sideline, -Baseline, Sideline, LineWidth, y);
            lines.Stroke(Baseline, -Sideline, Baseline, Sideline, LineWidth, y);
            lines.Stroke(0f, -Sideline, 0f, Sideline, LineWidth, y);
            lines.Arc(Vector2.zero, 1.60f, 0f, 360f, 48, LineWidth, y);

            AddHalfCourtMarkings(lines, -1f, y);
            AddHalfCourtMarkings(lines, 1f, y);
            return lines;
        }

        private static void AddHalfCourtMarkings(Geometry lines, float side, float y)
        {
            const float laneHalf = 1.82f;
            float end = side * Baseline;
            float freeThrow = side * 6.34f;
            float rim = side * (PoleX - RimOffset);
            lines.Stroke(freeThrow, -laneHalf, end, -laneHalf, LineWidth, y);
            lines.Stroke(freeThrow, laneHalf, end, laneHalf, LineWidth, y);
            lines.Stroke(freeThrow, -laneHalf, freeThrow, laneHalf, LineWidth, y);

            var freeThrowCenter = new Vector2(freeThrow, 0f);
            float towardCenter = side > 0f ? 180f : 0f;
            lines.Arc(freeThrowCenter, 1.58f, towardCenter - 90f, towardCenter + 90f, 24, LineWidth, y);
            for (int dash = 0; dash < 6; dash++)
            {
                float start = towardCenter + 96f + dash * 29f;
                lines.Arc(freeThrowCenter, 1.58f, start, start + 13f, 3, LineWidth, y);
            }

            for (int tick = 0; tick < 3; tick++)
            {
                float x = side * (7.55f + tick * 0.88f);
                lines.Stroke(x, laneHalf, x, laneHalf + 0.20f, LineWidth, y);
                lines.Stroke(x, -laneHalf, x, -laneHalf - 0.20f, LineWidth, y);
            }

            const float corner = 5.70f;
            const float radius = 5.98f;
            float halfAngle = Mathf.Asin(corner / radius) * Mathf.Rad2Deg;
            float arcJoin = rim - side * Mathf.Sqrt(radius * radius - corner * corner);
            lines.Stroke(end, -corner, arcJoin, -corner, LineWidth, y);
            lines.Stroke(end, corner, arcJoin, corner, LineWidth, y);
            var rimCenter = new Vector2(rim, 0f);
            lines.Arc(rimCenter, radius, towardCenter - halfAngle, towardCenter + halfAngle, 40, LineWidth, y);
            lines.Arc(rimCenter, 1.11f, towardCenter - 90f, towardCenter + 90f, 20, LineWidth, y);
        }

        private static GameObject CreateHoopPrefab(Scene preview)
        {
            Material steel = SaveMaterial("M_HoopSteel", new Color32(46, 66, 77, 255), 0.14f);
            Material pads = SaveMaterial("M_HoopGuard", new Color32(46, 103, 99, 255));
            Material board = SaveMaterial("M_HoopBackboard", new Color32(237, 221, 185, 255));
            Material orange = SaveMaterial("M_HoopRim", new Color32(212, 103, 45, 255), 0.08f);
            Material footing = SaveMaterial("M_HoopFooting", new Color32(128, 122, 103, 255));
            Material net = SaveMaterial("M_HoopNet", new Color32(246, 236, 213, 255));

            GameObject hoop = NewRoot("PF_BasketballHoop", preview);
            Transform root = hoop.transform;
            AddBox(root, "AnclajeConcreto", new Vector3(0f, 0.04f, 0f),
                new Vector3(0.57f, 0.08f, 0.55f), footing, solid: true);
            AddBox(root, "Soporte", new Vector3(0f, 1.70f, 0f), new Vector3(0.17f, 3.30f, 0.17f), steel);
            AddBox(root, "Protector", new Vector3(0f, 0.74f, 0f), new Vector3(0.34f, 1.32f, 0.32f), pads, solid: true);
            AddBox(root, "RemateProtector", new Vector3(0f, 1.38f, 0f), new Vector3(0.36f, 0.13f, 0.34f), steel);

            AddBeam(root, "BrazoInclinado", new Vector3(0f, 3.27f, 0f), new Vector3(0f, 3.62f, 0.50f), 0.16f, steel);
            AddBeam(root, "BrazoSuperior", new Vector3(0f, 3.62f, 0.46f), new Vector3(0f, 3.62f, 1.19f), 0.15f, steel);
            AddBeam(root, "Refuerzo", new Vector3(0f, 3.15f, 0f), new Vector3(0f, 3.54f, 0.86f), 0.08f, steel);
            AddBox(root, "MarcoTablero", new Vector3(0f, 3.51f, 1.19f), new Vector3(1.77f, 1.04f, 0.095f), steel);
            AddBox(root, "FrenteTablero", new Vector3(0f, 3.51f, 1.241f), new Vector3(1.68f, 0.95f, 0.013f), board);

            const float face = 1.251f;
            AddBox(root, "MarcaInferior", new Vector3(0f, 3.105f, face), new Vector3(0.56f, 0.035f, 0.012f), orange);
            AddBox(root, "MarcaSuperior", new Vector3(0f, 3.447f, face), new Vector3(0.56f, 0.035f, 0.012f), orange);
            AddBox(root, "MarcaIzquierda", new Vector3(-0.266f, 3.276f, face), new Vector3(0.035f, 0.342f, 0.012f), orange);
            AddBox(root, "MarcaDerecha", new Vector3(0.266f, 3.276f, face), new Vector3(0.035f, 0.342f, 0.012f), orange);
            AddBox(root, "PlacaDelAro", new Vector3(0f, RimHeight + 0.01f, 1.274f),
                new Vector3(0.17f, 0.135f, 0.07f), steel);
            AddBox(root, "UnionDelAro", new Vector3(0f, RimHeight, 1.29f),
                new Vector3(0.14f, 0.055f, 0.108f), orange);

            Vector3 rimCenter = new Vector3(0f, RimHeight, RimOffset);
            var ring = new Geometry();
            ring.Torus(rimCenter, 0.233f, 0.018f);
            AddMesh(root, "Aro", SaveMesh("SM_HoopRim", ring), orange);

            Geometry strands = CreateNet(rimCenter);
            AddMesh(root, "Red", SaveMesh("SM_HoopNet", strands), net);
            return SavePrefab(hoop, HoopPath);
        }

        private static Geometry CreateNet(Vector3 center)
        {
            var net = new Geometry();
            const int knots = 12;
            const float angleStep = 360f / knots;
            float[] radii = { 0.221f, 0.202f, 0.175f, 0.150f };
            float[] drops = { 0.022f, 0.148f, 0.282f, 0.427f };
            for (int level = 0; level < radii.Length - 1; level++)
            {
                for (int knot = 0; knot < knots; knot++)
                {
                    float angle = (knot + (level % 2) * 0.5f) * angleStep;
                    Vector3 knotPosition = OnCircle(center, radii[level], angle, drops[level]);
                    net.Tube(knotPosition, OnCircle(center, radii[level + 1], angle + angleStep * 0.5f, drops[level + 1]), 0.007f);
                    net.Tube(knotPosition, OnCircle(center, radii[level + 1], angle - angleStep * 0.5f, drops[level + 1]), 0.007f);
                }
            }

            for (int knot = 0; knot < knots; knot++)
            {
                float angle = (knot + 0.5f) * angleStep;
                net.Tube(OnCircle(center, radii[3], angle, drops[3]),
                    OnCircle(center, radii[3], angle + angleStep, drops[3]), 0.007f);
            }
            return net;
        }

        private static Vector3 OnCircle(Vector3 center, float radius, float degrees, float drop)
        {
            float angle = degrees * Mathf.Deg2Rad;
            return center + new Vector3(radius * Mathf.Cos(angle), -drop, radius * Mathf.Sin(angle));
        }

        private static void PlaceHoop(GameObject prefab, Transform parent, string name, float x, float heading)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = new Vector3(x, SurfaceHeight, 0f);
            instance.transform.localRotation = Quaternion.Euler(0f, heading, 0f);
        }

        private static GameObject NewRoot(string name, Scene scene)
        {
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static GameObject AddBox(Transform parent, string name, Vector3 center, Vector3 size,
            Material material, bool solid = false)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(box.GetComponent<BoxCollider>());
            return box;
        }

        private static void AddBeam(Transform parent, string name, Vector3 from, Vector3 to, float width, Material material)
        {
            Vector3 direction = to - from;
            GameObject beam = AddBox(parent, name, (from + to) * 0.5f,
                new Vector3(width, direction.magnitude, width), material);
            beam.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
        }

        private static void AddMesh(Transform parent, string name, Mesh mesh, Material material)
        {
            var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            part.transform.SetParent(parent, false);
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Material SaveMaterial(string name, Color color, float metallic = 0f)
        {
            string path = CourtFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null) throw new InvalidOperationException("La cancha requiere un shader de superficie.");
                material = new Material(shader) { name = name };
            }

            material.color = color;
            material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.25f);
            if (create) AssetDatabase.CreateAsset(material, path);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static Mesh SaveMesh(string name, Geometry geometry)
        {
            string path = CourtFolder + "/" + name + ".asset";
            Mesh generated = geometry.ToMesh(name);
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, path);
                return generated;
            }

            EditorUtility.CopySerialized(generated, existing);
            Object.DestroyImmediate(generated);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssetIfDirty(existing);
            return existing;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            Object.DestroyImmediate(root);
            if (!success || prefab == null) throw new InvalidOperationException($"No se pudo guardar {path}.");
            return prefab;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        private sealed class Geometry
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();

            public void Rectangle(float x0, float z0, float x1, float z1, float y)
            {
                Quad(new Vector3(x0, y, z0), new Vector3(x0, y, z1),
                    new Vector3(x1, y, z1), new Vector3(x1, y, z0));
            }

            public void Stroke(float x0, float z0, float x1, float z1, float width, float y)
            {
                var start = new Vector3(x0, y, z0);
                var end = new Vector3(x1, y, z1);
                Vector3 offset = Vector3.Cross(end - start, Vector3.up).normalized * (width * 0.5f);
                Quad(start + offset, end + offset, end - offset, start - offset);
            }

            public void Arc(Vector2 center, float radius, float start, float end, int steps, float width, float y)
            {
                var origin = new Vector3(center.x, y, center.y);
                float inner = radius - width * 0.5f;
                float outer = radius + width * 0.5f;
                for (int segment = 0; segment < steps; segment++)
                {
                    float a = Mathf.Lerp(start, end, segment / (float)steps);
                    float b = Mathf.Lerp(start, end, (segment + 1) / (float)steps);
                    Quad(OnCircle(origin, outer, a, 0f), OnCircle(origin, inner, a, 0f),
                        OnCircle(origin, inner, b, 0f), OnCircle(origin, outer, b, 0f));
                }
            }

            public void Torus(Vector3 center, float radius, float section)
            {
                const int segments = 36;
                const int sides = 8;
                const float angularStep = 360f / segments;
                const float tubeStep = 360f / sides;
                for (int segment = 0; segment < segments; segment++)
                {
                    float a = segment * angularStep;
                    float b = a + angularStep;
                    for (int side = 0; side < sides; side++)
                    {
                        float v0 = side * tubeStep;
                        float v1 = v0 + tubeStep;
                        Quad(TorusPoint(center, radius, section, a, v0), TorusPoint(center, radius, section, a, v1),
                            TorusPoint(center, radius, section, b, v1), TorusPoint(center, radius, section, b, v0));
                    }
                }
            }

            private static Vector3 TorusPoint(Vector3 center, float radius, float section, float heading, float sectionAngle)
            {
                float radians = sectionAngle * Mathf.Deg2Rad;
                return OnCircle(center, radius + section * Mathf.Cos(radians), heading, -section * Mathf.Sin(radians));
            }

            public void Tube(Vector3 start, Vector3 end, float radius)
            {
                const int sides = 5;
                Vector3 axis = (end - start).normalized;
                Vector3 reference = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
                Vector3 major = Vector3.Cross(reference, axis).normalized * radius;
                Vector3 minor = Vector3.Cross(axis, major);
                for (int side = 0; side < sides; side++)
                {
                    float a = 2f * Mathf.PI * side / sides;
                    float b = 2f * Mathf.PI * (side + 1) / sides;
                    Vector3 offsetA = major * Mathf.Cos(a) + minor * Mathf.Sin(a);
                    Vector3 offsetB = major * Mathf.Cos(b) + minor * Mathf.Sin(b);
                    Quad(start + offsetA, start + offsetB, end + offsetB, end + offsetA);
                }
            }

            public Mesh ToMesh(string name)
            {
                var mesh = new Mesh { name = name };
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }

            private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int index = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                vertices.Add(d);
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }
        }
    }
}
