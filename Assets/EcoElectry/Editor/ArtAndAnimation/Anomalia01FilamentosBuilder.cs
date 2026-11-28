using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace EcoElectry.Editor.ArtAndAnimation
{
    /// <summary>
    /// Construye la Anomalía 01 ("Entidad de Filamentos") como un busto 3D low poly a partir de su
    /// ilustración: la silueta se infla en relieve, las cuencas y la boca se hunden según la
    /// oscuridad de la pintura y se añaden púas 3D que sobresalen del cráneo.
    /// </summary>
    public static class Anomalia01FilamentosBuilder
    {
        private const string Folder = "Assets/EcoElectry/Content/Models/Anomalias/Anomalia01_Filamentos";
        private const string TexturePath = Folder + "/anomalia_01_filamentos.png";
        private const string MeshPath = Folder + "/Anomalia01_Filamentos_Mesh.asset";
        private const string SkinMaterialPath = Folder + "/Anomalia01_Piel.mat";
        private const string BackMaterialPath = Folder + "/Anomalia01_Sombra.mat";
        private const string SpikeMaterialPath = Folder + "/Anomalia01_Puas.mat";
        private const string PrefabFolder = "Assets/EcoElectry/Content/Prefabs/Anomalias";
        public const string PrefabPath = PrefabFolder + "/Anomalia01_Filamentos.prefab";

        private const string Level02ScenePath = "Assets/EcoElectry/Content/Scenes/Level02.unity";
        private const string DoorPath = "Level02/Stairs/Conexion Nivel 01/Acceso PUERTA";
        private const string PlacementRootName = "Anomalias";
        private const string PlacementName = "Anomalia01 tras puerta 1er PISO";

        private const float BustHeight = 1.15f;   // metros, de la base de las hebras a la coronilla
        private const int CellPixels = 10;        // resolución de la malla (px de la ilustración por celda)
        private const float CellAlpha = 0.08f;    // una celda entra en la malla si algún píxel supera este alfa
        private const float SolidAlpha = 0.5f;    // píxeles que cuentan como volumen para el relieve
        private const float CutoutAlpha = 0.35f;

        [MenuItem("Tools/EcoElectry/Anomalias/Construir Anomalia 01 (Filamentos)")]
        public static void Build()
        {
            EnsureFolder(Folder);
            EnsureFolder(PrefabFolder);
            Texture2D texture = ImportTexture();
            Mesh mesh = BuildMesh(out Vector3 headCenter);
            SaveAsset(mesh, MeshPath);

            Material skin = SaveMaterial(SkinMaterialPath, texture, Color.white, 0.18f, 0f);
            Material back = SaveMaterial(BackMaterialPath, texture, new Color(0.2f, 0.13f, 0.12f), 0.1f, 0f);
            Material spikes = SaveMaterial(SpikeMaterialPath, null, new Color(0.16f, 0.07f, 0.05f), 0.4f, 0.35f);

            var root = new GameObject("Anomalia01_Filamentos");
            var model = new GameObject("Modelo");
            model.transform.SetParent(root.transform, false);
            model.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = model.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { skin, back, spikes };
            renderer.shadowCastingMode = ShadowCastingMode.On;

            var eyes = new GameObject("Punto de mirada");
            eyes.transform.SetParent(root.transform, false);
            eyes.transform.localPosition = headCenter;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log("[Anomalia01] Prefab generado en " + PrefabPath + " (" + mesh.vertexCount + " vértices, " +
                      (mesh.triangles.Length / 3) + " triángulos).");
        }

        [MenuItem("Tools/EcoElectry/Anomalias/Colocar Anomalia 01 tras la puerta del 1er PISO (Level02)")]
        public static void PlaceInLevel02()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) Build();
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != Level02ScenePath)
                scene = EditorSceneManager.OpenScene(Level02ScenePath, OpenSceneMode.Single);

            GameObject door = GameObject.Find(DoorPath);
            if (door == null) throw new InvalidOperationException("No se encontró la puerta " + DoorPath);

            GameObject container = GameObject.Find(PlacementRootName) ?? new GameObject(PlacementRootName);
            Transform existing = container.transform.Find(PlacementName);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

            var placement = new GameObject(PlacementName);
            placement.transform.SetParent(container.transform, false);
            Transform doorT = door.transform;
            // La puerta mira hacia el paso lateral (-Z). Detrás de ella queda +Z de la puerta.
            Vector3 behind = doorT.forward;
            placement.transform.position = doorT.position + behind * 0.55f;
            placement.transform.rotation = Quaternion.LookRotation(-behind, Vector3.up);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetParent(placement.transform, false);
            // Base de las hebras escondida tras el panel inferior; los ojos quedan a la altura del vidrio.
            instance.transform.localPosition = new Vector3(0.02f, 0.86f, 0f);
            instance.transform.localRotation = Quaternion.Euler(0f, -8f, 0f);

            BuildCloset(placement.transform, doorT);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = instance;
            Debug.Log("[Anomalia01] Colocada detrás de " + DoorPath + " en " + instance.transform.position);
        }

        // ---------------------------------------------------------------- recinto tras la puerta

        private static void BuildCloset(Transform parent, Transform door)
        {
            // Pequeño recinto oscuro para que tras el vidrio no se vea el vacío.
            Material wall = FindSharedMaterial("Level02/Stairs/Conexion Nivel 01/Muro/Yeso");
            Material floor = FindSharedMaterial("Level02/Stairs/Conexion Nivel 01/Suelo paso lateral 1P");
            var closet = new GameObject("Recinto oscuro");
            closet.transform.SetParent(parent, false);
            closet.transform.position = door.position;
            closet.transform.rotation = door.rotation;

            const float width = 1.36f, depth = 1.7f, height = 3.4f, thick = 0.08f, start = 0.13f;
            AddBlock(closet.transform, "Suelo", new Vector3(0f, -0.05f, start + depth * 0.5f), new Vector3(width, 0.1f, depth), floor);
            AddBlock(closet.transform, "Techo", new Vector3(0f, height, start + depth * 0.5f), new Vector3(width, thick, depth), wall);
            AddBlock(closet.transform, "Fondo", new Vector3(0f, height * 0.5f, start + depth), new Vector3(width, height, thick), wall);
            AddBlock(closet.transform, "Lateral izquierdo", new Vector3(-width * 0.5f, height * 0.5f, start + depth * 0.5f), new Vector3(thick, height, depth), wall);
            AddBlock(closet.transform, "Lateral derecho", new Vector3(width * 0.5f, height * 0.5f, start + depth * 0.5f), new Vector3(thick, height, depth), wall);

            var lightGo = new GameObject("Luz residual fria");
            lightGo.transform.SetParent(closet.transform, false);
            lightGo.transform.localPosition = new Vector3(0.25f, 2.05f, 0.22f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.55f, 0.66f, 0.8f);
            light.range = 2.2f;
            light.intensity = 0.75f;
            light.shadows = LightShadows.Soft;
        }

        private static void AddBlock(Transform parent, string name, Vector3 localPos, Vector3 size, Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPos;
            block.transform.localScale = size;
            if (material != null) block.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Material FindSharedMaterial(string path)
        {
            GameObject go = GameObject.Find(path);
            var r = go != null ? go.GetComponent<Renderer>() : null;
            return r != null ? r.sharedMaterial : null;
        }

        // ---------------------------------------------------------------- malla

        private static Mesh BuildMesh(out Vector3 headCenter)
        {
            var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            src.LoadImage(File.ReadAllBytes(Path.GetFullPath(TexturePath)));
            int w = src.width, h = src.height;
            Color32[] px = src.GetPixels32();
            UnityEngine.Object.DestroyImmediate(src);
            float scale = BustHeight / h;

            var alpha = new float[w * h];
            var lum = new float[w * h];
            var solid = new bool[w * h];
            for (int i = 0; i < px.Length; i++)
            {
                alpha[i] = px[i].a / 255f;
                lum[i] = (0.299f * px[i].r + 0.587f * px[i].g + 0.114f * px[i].b) / 255f;
                solid[i] = alpha[i] >= SolidAlpha;
            }

            float[] dist = DistanceToEdge(solid, w, h);
            float[] lumNear = BlurredLuminance(lum, solid, w, h, 5);
            float[] lumWide = BlurredLuminance(lum, solid, w, h, 26);

            // Centro de la cabeza: centroide de los píxeles sólidos de la mitad superior.
            int headLimitTop = Mathf.RoundToInt(h * 0.55f);
            double cx = 0, cy = 0; int count = 0;
            for (int y = h - headLimitTop; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (solid[y * w + x]) { cx += x; cy += y; count++; }
            Vector2 headPx = count > 0 ? new Vector2((float)(cx / count), (float)(cy / count)) : new Vector2(w * 0.5f, h * 0.7f);

            int nx = Mathf.CeilToInt((float)w / CellPixels), ny = Mathf.CeilToInt((float)h / CellPixels);
            var cellOn = new bool[nx * ny];
            for (int j = 0; j < ny; j++)
                for (int i = 0; i < nx; i++)
                {
                    bool on = false;
                    for (int y = j * CellPixels; y < Mathf.Min(h, (j + 1) * CellPixels) && !on; y++)
                        for (int x = i * CellPixels; x < Mathf.Min(w, (i + 1) * CellPixels); x++)
                            if (alpha[y * w + x] > CellAlpha) { on = true; break; }
                    cellOn[j * nx + i] = on;
                }

            int vx = nx + 1, vy = ny + 1;
            var front = new float[vx * vy];
            var backDepth = new float[vx * vy];
            var used = new bool[vx * vy];
            var border = new bool[vx * vy];
            for (int j = 0; j < vy; j++)
                for (int i = 0; i < vx; i++)
                {
                    int on = 0, total = 0;
                    for (int dj = -1; dj <= 0; dj++)
                        for (int di = -1; di <= 0; di++)
                        {
                            int ci = i + di, cj = j + dj;
                            total++;
                            if (ci >= 0 && cj >= 0 && ci < nx && cj < ny && cellOn[cj * nx + ci]) on++;
                        }
                    int v = j * vx + i;
                    used[v] = on > 0;
                    border[v] = on > 0 && on < total;
                    if (!used[v] || border[v]) continue;

                    int sx = Mathf.Clamp(i * CellPixels, 0, w - 1), sy = Mathf.Clamp(j * CellPixels, 0, h - 1);
                    int p = sy * w + sx;
                    float topPx = h - 1 - sy;
                    float head = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(h * 0.52f, h * 0.62f, topPx));
                    float d = dist[p] * scale;
                    float amp = Mathf.Lerp(0.11f, 0.17f, head);
                    float reach = Mathf.Lerp(0.16f, 0.13f, head);
                    float bulge = amp * Mathf.Sqrt(Mathf.Clamp01(d / reach));
                    float cavity = Mathf.Clamp01((lumWide[p] - lumNear[p]) * 3.2f) * head * 0.085f;
                    front[v] = Mathf.Max(0.006f, bulge - cavity);
                    backDepth[v] = -bulge * Mathf.Lerp(0.55f, 0.8f, head);
                }
            Smooth(front, used, border, vx, vy);
            Smooth(backDepth, used, border, vx, vy);

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var frontIndex = new int[vx * vy];
            var backIndex = new int[vx * vy];
            Func<int, int, Vector3> basePos = (i, j) =>
            {
                float x = Mathf.Min(i * CellPixels, w), y = Mathf.Min(j * CellPixels, h);
                // X invertida para que la ilustración se lea igual mirando al frente (+Z local).
                return new Vector3(-(x - w * 0.5f) * scale, y * scale, 0f);
            };
            for (int j = 0; j < vy; j++)
                for (int i = 0; i < vx; i++)
                {
                    int v = j * vx + i;
                    frontIndex[v] = backIndex[v] = -1;
                    if (!used[v]) continue;
                    Vector3 pos = basePos(i, j);
                    var uv = new Vector2(Mathf.Min(i * CellPixels, w) / (float)w, Mathf.Min(j * CellPixels, h) / (float)h);
                    frontIndex[v] = vertices.Count;
                    vertices.Add(new Vector3(pos.x, pos.y, front[v]));
                    uvs.Add(uv);
                    if (border[v]) { backIndex[v] = frontIndex[v]; continue; }
                    backIndex[v] = vertices.Count;
                    vertices.Add(new Vector3(pos.x, pos.y, backDepth[v]));
                    uvs.Add(uv);
                }

            var frontTris = new List<int>();
            var backTris = new List<int>();
            for (int j = 0; j < ny; j++)
                for (int i = 0; i < nx; i++)
                {
                    if (!cellOn[j * nx + i]) continue;
                    int a = j * vx + i, b = a + 1, c = a + vx, d = c + 1;
                    AddQuad(frontTris, vertices, frontIndex[a], frontIndex[b], frontIndex[c], frontIndex[d], Vector3.forward);
                    AddQuad(backTris, vertices, backIndex[a], backIndex[b], backIndex[c], backIndex[d], Vector3.back);
                }

            headCenter = new Vector3(-(headPx.x - w * 0.5f) * scale, headPx.y * scale, 0.02f);
            float headRadius = 0.2f;
            var spikeTris = new List<int>();
            AddSpikes(vertices, uvs, spikeTris, headCenter, headRadius);

            var mesh = new Mesh { name = "Anomalia01_Filamentos", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 3;
            mesh.SetTriangles(frontTris, 0);
            mesh.SetTriangles(backTris, 1);
            mesh.SetTriangles(spikeTris, 2);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(List<int> tris, List<Vector3> verts, int a, int b, int c, int d, Vector3 facing)
        {
            if (a < 0 || b < 0 || c < 0 || d < 0) return;
            AddTri(tris, verts, a, b, c, facing);
            AddTri(tris, verts, b, d, c, facing);
        }

        private static void AddTri(List<int> tris, List<Vector3> verts, int a, int b, int c, Vector3 facing)
        {
            Vector3 n = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]);
            if (Vector3.Dot(n, facing) < 0f) { int t = b; b = c; c = t; }
            tris.Add(a); tris.Add(b); tris.Add(c);
        }

        private static void AddSpikes(List<Vector3> verts, List<Vector2> uvs, List<int> tris, Vector3 head, float radius)
        {
            var rng = new System.Random(1795857);
            const int count = 11;
            for (int k = 0; k < count; k++)
            {
                // Abanico desde el lateral derecho, por la coronilla, hasta el lateral izquierdo.
                float angle = Mathf.Lerp(-115f, 115f, (k + 0.5f) / count) + (float)(rng.NextDouble() - 0.5) * 16f;
                float rad = angle * Mathf.Deg2Rad;
                var radial = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f);
                Vector3 dir = (radial + Vector3.back * (0.45f + (float)rng.NextDouble() * 0.5f)).normalized;
                Vector3 basePoint = head + radial * radius * 0.55f + Vector3.back * 0.04f;
                float length = 0.24f + (float)rng.NextDouble() * 0.2f;
                float thickness = 0.006f + (float)rng.NextDouble() * 0.004f;
                // Leve curva: punta desviada.
                Vector3 bend = Vector3.Cross(dir, Vector3.forward).normalized * ((float)(rng.NextDouble() - 0.5) * 0.06f);
                AddSpike(verts, uvs, tris, basePoint, basePoint + dir * length + bend, thickness);
            }
        }

        private static void AddSpike(List<Vector3> verts, List<Vector2> uvs, List<int> tris, Vector3 from, Vector3 to, float r)
        {
            Vector3 axis = (to - from).normalized;
            Vector3 u = Vector3.Cross(axis, Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right).normalized * r;
            Vector3 v = Vector3.Cross(axis, u).normalized * r;
            Vector3 mid = Vector3.Lerp(from, to, 0.55f);
            var ring0 = new[] { from + u, from + v, from - u, from - v };
            var ring1 = new[] { mid + u * 0.55f, mid + v * 0.55f, mid - u * 0.55f, mid - v * 0.55f };
            int start = verts.Count;
            foreach (Vector3 p in ring0) { verts.Add(p); uvs.Add(Vector2.zero); }
            foreach (Vector3 p in ring1) { verts.Add(p); uvs.Add(Vector2.zero); }
            verts.Add(to); uvs.Add(Vector2.zero);
            int tip = start + 8;
            for (int s = 0; s < 4; s++)
            {
                int a0 = start + s, a1 = start + (s + 1) % 4, b0 = start + 4 + s, b1 = start + 4 + (s + 1) % 4;
                Vector3 outward = ((verts[a0] + verts[a1]) * 0.5f - from).normalized;
                AddTri(tris, verts, a0, a1, b0, outward);
                AddTri(tris, verts, a1, b1, b0, outward);
                Vector3 outward2 = ((verts[b0] + verts[b1]) * 0.5f - mid).normalized;
                AddTri(tris, verts, b0, b1, tip, outward2);
            }
        }

        private static void Smooth(float[] values, bool[] used, bool[] border, int vx, int vy)
        {
            var copy = (float[])values.Clone();
            for (int j = 1; j < vy - 1; j++)
                for (int i = 1; i < vx - 1; i++)
                {
                    int v = j * vx + i;
                    if (!used[v] || border[v]) continue;
                    float sum = copy[v] * 2f, weight = 2f;
                    int[] n = { v - 1, v + 1, v - vx, v + vx };
                    foreach (int k in n)
                        if (used[k]) { sum += copy[k]; weight += 1f; }
                    values[v] = sum / weight;
                }
        }

        private static float[] DistanceToEdge(bool[] solid, int w, int h)
        {
            const int inf = int.MaxValue / 4;
            var d = new int[w * h];
            for (int i = 0; i < d.Length; i++) d[i] = solid[i] ? inf : 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (d[i] == 0) continue;
                    int best = d[i];
                    if (x == 0 || y == 0 || x == w - 1) best = 3;
                    else
                    {
                        best = Mathf.Min(best, d[i - 1] + 3);
                        best = Mathf.Min(best, d[i - w] + 3);
                        best = Mathf.Min(best, d[i - w - 1] + 4);
                        best = Mathf.Min(best, d[i - w + 1] + 4);
                    }
                    d[i] = best;
                }
            for (int y = h - 1; y >= 0; y--)
                for (int x = w - 1; x >= 0; x--)
                {
                    int i = y * w + x;
                    if (d[i] == 0) continue;
                    int best = d[i];
                    if (x == w - 1 || y == h - 1 || x == 0) best = Mathf.Min(best, 3);
                    else
                    {
                        best = Mathf.Min(best, d[i + 1] + 3);
                        best = Mathf.Min(best, d[i + w] + 3);
                        best = Mathf.Min(best, d[i + w + 1] + 4);
                        best = Mathf.Min(best, d[i + w - 1] + 4);
                    }
                    d[i] = best;
                }
            var result = new float[w * h];
            for (int i = 0; i < d.Length; i++) result[i] = d[i] / 3f;
            return result;
        }

        private static float[] BlurredLuminance(float[] lum, bool[] solid, int w, int h, int radius)
        {
            var sum = new double[(w + 1) * (h + 1)];
            var cnt = new double[(w + 1) * (h + 1)];
            for (int y = 0; y < h; y++)
            {
                double rowSum = 0, rowCnt = 0;
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (solid[i]) { rowSum += lum[i]; rowCnt += 1; }
                    int o = (y + 1) * (w + 1) + (x + 1);
                    sum[o] = sum[o - (w + 1)] + rowSum;
                    cnt[o] = cnt[o - (w + 1)] + rowCnt;
                }
            }
            var result = new float[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int x0 = Mathf.Max(0, x - radius), x1 = Mathf.Min(w, x + radius + 1);
                    int y0 = Mathf.Max(0, y - radius), y1 = Mathf.Min(h, y + radius + 1);
                    int a = y0 * (w + 1) + x0, b = y0 * (w + 1) + x1, c = y1 * (w + 1) + x0, d = y1 * (w + 1) + x1;
                    double n = cnt[d] - cnt[b] - cnt[c] + cnt[a];
                    result[y * w + x] = n > 0 ? (float)((sum[d] - sum[b] - sum[c] + sum[a]) / n) : 0f;
                }
            return result;
        }

        // ---------------------------------------------------------------- assets

        private static Texture2D ImportTexture()
        {
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            if (importer == null) throw new InvalidOperationException("Falta la textura " + TexturePath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        }

        private static Material SaveMaterial(string path, Texture2D texture, Color color, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            bool clip = texture != null;
            material.SetFloat("_AlphaClip", clip ? 1f : 0f);
            material.SetFloat("_Cutoff", CutoutAlpha);
            if (clip) material.EnableKeyword("_ALPHATEST_ON"); else material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = clip ? (int)RenderQueue.AlphaTest : (int)RenderQueue.Geometry;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SaveAsset(UnityEngine.Object asset, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
