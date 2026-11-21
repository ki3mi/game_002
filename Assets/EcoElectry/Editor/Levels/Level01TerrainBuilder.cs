using System;
using UnityEditor;
using UnityEngine;

namespace EcoElectry.Editor.Levels
{
    internal static class Level01TerrainBuilder
    {
        private const string AssetFolder = "Assets/EcoElectry/Content/Terrain/Level01";
        private const string TerrainObjectName = "RuralTerrain";
        private const float RoadHalfWidth = 2.3f;
        private const int HeightResolution = 513;
        private const int PaintResolution = 512;
        private const int TextureSize = 64;

        private static readonly Vector3 Origin = new Vector3(-80f, -2f, -56f);
        private static readonly Vector3 Size = new Vector3(160f, 24f, 152f);

        // Coordinates are stored as (world Z, world X), from the southern edge to the gate.
        private static readonly Vector2[] Road =
        {
            new Vector2(-56f, -25f), new Vector2(-50f, -25f), new Vector2(-42f, -24.3f),
            new Vector2(-35f, -20f), new Vector2(-29f, -12f), new Vector2(-23f, -8.1f),
            new Vector2(-17f, -8f), new Vector2(-9f, -8f), new Vector2(0f, -8f)
        };

        internal static Terrain Create(Transform parent)
        {
            EnsureAssetFolder();
            var meadow = CreateLayer("Meadow", new Color32(106, 117, 71, 255), new Color32(135, 145, 92, 255), 6.5f, 0.08f);
            var dryGrass = CreateLayer("DryGrass", new Color32(127, 130, 78, 255), new Color32(157, 151, 98, 255), 7f, 0.08f);
            var shoulder = CreateLayer("Shoulder", new Color32(127, 117, 72, 255), new Color32(158, 143, 93, 255), 5f, 0.12f);
            var gravel = CreateLayer("RoadGravel", new Color32(149, 112, 69, 255), new Color32(187, 152, 104, 255), 4.2f, 0.23f);

            TerrainData data = LoadOrCreate("RuralTerrainData.asset", () => new TerrainData());
            if (data.heightmapResolution != HeightResolution) data.heightmapResolution = HeightResolution;
            if (data.alphamapResolution != PaintResolution) data.alphamapResolution = PaintResolution;
            data.size = Size;
            data.terrainLayers = new[] { meadow, dryGrass, shoulder, gravel };
            data.SetHeights(0, 0, BuildHeights());
            data.SetAlphamaps(0, 0, BuildPaint());
            Save(data);

            Transform existing = parent.Find(TerrainObjectName);
            GameObject terrainObject = existing != null ? existing.gameObject : Terrain.CreateTerrainGameObject(data);
            terrainObject.name = TerrainObjectName;
            terrainObject.transform.SetParent(parent, false);
            terrainObject.transform.SetPositionAndRotation(Origin, Quaternion.identity);
            terrainObject.transform.localScale = Vector3.one;

            var terrain = terrainObject.GetComponent<Terrain>();
            var collider = terrainObject.GetComponent<TerrainCollider>();
            terrain.terrainData = data;
            terrain.materialTemplate = CreateMaterial();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 3f;
            collider.terrainData = data;
            return terrain;
        }

        internal static float GroundAt(Terrain terrain, float x, float z)
        {
            return terrain.transform.position.y + terrain.SampleHeight(new Vector3(x, 0f, z));
        }

        internal static float RoadX(float z)
        {
            if (z <= Road[0].x) return Road[0].y;
            int last = Road.Length - 1;
            if (z >= Road[last].x) return Road[last].y;

            int index = 0;
            while (z >= Road[index + 1].x) index++;
            Vector2 start = Road[index];
            Vector2 end = Road[index + 1];
            float interval = end.x - start.x;
            float t = (z - start.x) / interval;
            float t2 = t * t;
            float t3 = t2 * t;
            return (2f * t3 - 3f * t2 + 1f) * start.y
                + (t3 - 2f * t2 + t) * interval * RoadTangent(index)
                + (-2f * t3 + 3f * t2) * end.y
                + (t3 - t2) * interval * RoadTangent(index + 1);
        }

        private static float RoadTangent(int index)
        {
            if (index <= 0 || index >= Road.Length - 1) return 0f;
            Vector2 before = Road[index] - Road[index - 1];
            Vector2 after = Road[index + 1] - Road[index];
            float a = before.y / before.x;
            float b = after.y / after.x;
            return a * b <= 0f ? 0f : 2f * a * b / (a + b);
        }

        private static float[,] BuildHeights()
        {
            var heights = new float[HeightResolution, HeightResolution];
            for (int row = 0; row < HeightResolution; row++)
            {
                float z = Origin.z + Size.z * row / (HeightResolution - 1);
                float roadX = RoadX(z);
                float perpendicular = RoadWidthCorrection(z);
                float roadGrade = 1.35f * (1f - Blend(-56f, -9f, z));
                float approach = 1f - Blend(-1f, 4f, z);
                for (int column = 0; column < HeightResolution; column++)
                {
                    float x = Origin.x + Size.x * column / (HeightResolution - 1);
                    float distanceToRoad = Mathf.Abs(x - roadX) * perpendicular;
                    float corridor = approach * (1f - Blend(3.15f, 8.5f, distanceToRoad));
                    float height = Mathf.Lerp(NaturalHeight(x, z), roadGrade, corridor);
                    heights[row, column] = Mathf.Clamp01((height - Origin.y) / Size.y);
                }
            }

            return heights;
        }

        private static float NaturalHeight(float x, float z)
        {
            // Leave one extra sample on every side so bilinear sampling also keeps the campus level.
            float dx = Mathf.Max(0f, Mathf.Max(-23.65f - x, x - 35.65f));
            float dz = Mathf.Max(0f, Mathf.Max(-9.65f - z, z - 42.65f));
            float outskirts = Blend(0f, 14f, Mathf.Sqrt(dx * dx + dz * dz));
            float ground = 0.22f + Mathf.PerlinNoise(x * 0.027f + 18.4f, z * 0.026f + 29.8f) * 2.1f;
            ground += (Mathf.PerlinNoise(x * 0.07f + 2.3f, z * 0.065f + 0.62f) - 0.5f) * 0.64f;
            ground += Hill(x, z, -43f, -30f, 24f, 22f, 3.6f);
            ground += Hill(x, z, 14f, -39f, 25f, 19f, 3.2f);
            ground += Hill(x, z, -34f, 48f, 24f, 25f, 4.4f);
            ground += Hill(x, z, 51f, 63f, 30f, 29f, 5.3f);
            return ground * outskirts;
        }

        private static float Hill(float x, float z, float centerX, float centerZ, float spreadX, float spreadZ, float height)
        {
            float dx = (x - centerX) / spreadX;
            float dz = (z - centerZ) / spreadZ;
            return height * Mathf.Exp(-2f * (dx * dx + dz * dz));
        }

        private static float[,,] BuildPaint()
        {
            var paint = new float[PaintResolution, PaintResolution, 4];
            for (int row = 0; row < PaintResolution; row++)
            {
                float z = Origin.z + Size.z * (row + 0.5f) / PaintResolution;
                float roadX = RoadX(z);
                float perpendicular = RoadWidthCorrection(z);
                float end = 1f - Blend(-0.67f, 0f, z);
                for (int column = 0; column < PaintResolution; column++)
                {
                    float x = Origin.x + Size.x * (column + 0.5f) / PaintResolution;
                    float roughEdge = (Mathf.PerlinNoise(x * 0.42f + 13.4f, z * 0.49f + 14f) - 0.5f) * 0.24f;
                    float d = Mathf.Abs(x - roadX) * perpendicular + roughEdge;
                    float gravel = end * (1f - Blend(RoadHalfWidth - 0.11f, RoadHalfWidth + 0.14f, d));
                    float verge = end * (1f - Blend(RoadHalfWidth + 0.12f, RoadHalfWidth + 0.82f, d));
                    float soil = Mathf.Max(0f, verge - gravel);
                    float meadow = 1f - verge;
                    float fieldNoise = Mathf.PerlinNoise(x * 0.076f + 7.9f, z * 0.081f + 12.6f);
                    float bare = Mathf.Lerp(0.12f, 0.68f, Blend(0.23f, 0.79f, fieldNoise));

                    paint[row, column, 0] = meadow * (1f - bare);
                    paint[row, column, 1] = meadow * bare;
                    paint[row, column, 2] = soil;
                    paint[row, column, 3] = gravel;
                }
            }

            return paint;
        }

        private static float RoadWidthCorrection(float z)
        {
            float slope = (RoadX(z + 0.2f) - RoadX(z - 0.2f)) / 0.4f;
            return 1f / Mathf.Sqrt(1f + slope * slope);
        }

        private static float Blend(float from, float to, float value)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
        }

        private static TerrainLayer CreateLayer(string name, Color32 baseColor, Color32 fleckColor, float meters, float grit)
        {
            Texture2D texture = LoadOrCreate(name + "Albedo.asset",
                () => new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, true));
            if (texture.width != TextureSize || texture.height != TextureSize || texture.mipmapCount < 2)
                texture.Reinitialize(TextureSize, TextureSize, TextureFormat.RGBA32, true);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 4;

            var pixels = new Color32[TextureSize * TextureSize];
            for (int z = 0; z < TextureSize; z++)
            for (int x = 0; x < TextureSize; x++)
            {
                float u = (x + 0.5f) / TextureSize;
                float v = (z + 0.5f) / TextureSize;
                float broad = TileNoise(u, v, 4f, 3.73f);
                float speckled = TileNoise(u, v, 22f, 7.31f);
                float amount = broad * 0.56f + speckled * 0.28f;
                amount += Mathf.Max(0f, speckled - 0.48f) * grit * 4.5f;
                pixels[z * TextureSize + x] = Color32.Lerp(baseColor, fleckColor, Mathf.Clamp01(amount));
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            Save(texture);

            var layer = LoadOrCreate(name + ".terrainlayer", () => new TerrainLayer());
            layer.diffuseTexture = texture;
            layer.tileSize = new Vector2(meters, meters);
            layer.metallic = 0f;
            layer.smoothness = 0.03f;
            Save(layer);
            return layer;
        }

        private static float TileNoise(float u, float v, float frequency, float seed)
        {
            float x = u * frequency + seed;
            float z = v * frequency + seed * 1.13f;
            float blendX = Mathf.SmoothStep(0f, 1f, u);
            float blendZ = Mathf.SmoothStep(0f, 1f, v);
            float a = Mathf.Lerp(Mathf.PerlinNoise(x, z), Mathf.PerlinNoise(x - frequency, z), blendX);
            float b = Mathf.Lerp(Mathf.PerlinNoise(x, z - frequency), Mathf.PerlinNoise(x - frequency, z - frequency), blendX);
            return Mathf.Lerp(a, b, blendZ);
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
            if (shader == null) throw new InvalidOperationException("Falta el shader URP Terrain Lit para el nivel 01.");
            Material material = LoadOrCreate("RuralTerrain.mat", () => new Material(shader));
            material.shader = shader;
            Save(material);
            return material;
        }

        private static void EnsureAssetFolder()
        {
            const string content = "Assets/EcoElectry/Content";
            const string terrain = content + "/Terrain";
            if (!AssetDatabase.IsValidFolder(terrain)) AssetDatabase.CreateFolder(content, "Terrain");
            if (!AssetDatabase.IsValidFolder(AssetFolder)) AssetDatabase.CreateFolder(terrain, "Level01");
        }

        private static T LoadOrCreate<T>(string fileName, Func<T> create) where T : UnityEngine.Object
        {
            string path = AssetFolder + "/" + fileName;
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = create();
            asset.name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Save(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}
