using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal sealed class HorrorCorridorEnvironment
    {
        private const string Models = "Assets/EcoElectry/Content/Models";
        private const string Materials = Models + "/HorrorCorridor";
        private const float Ceiling = 3.05f;
        private const float DoorWidth = 1.01f;
        private const float DoorHeight = 2.39f;

        private Transform shell, details, lighting;
        private Material plaster, paint, trim, metal, cork, paper, floor, ceiling, tube, signal;

        public static void Build()
        {
            new HorrorCorridorEnvironment().BuildInterior();
        }

        private void BuildInterior()
        {
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Models, "HorrorCorridor");
            plaster = Surface("Plaster", new Color32(176, 172, 149, 255));
            paint = Surface("Dado", new Color32(76, 100, 94, 255));
            trim = Surface("Trim", new Color32(43, 58, 56, 255));
            metal = Surface("Iron", new Color32(67, 72, 72, 255));
            cork = Surface("Cork", new Color32(132, 96, 66, 255));
            paper = Surface("Paper", new Color32(197, 191, 155, 255));
            floor = Surface("Floor", Color.white);
            floor.SetTexture("_BaseMap", FloorTexture());
            floor.SetTextureScale("_BaseMap", new Vector2(21f, 21f));
            EditorUtility.SetDirty(floor);
            AssetDatabase.SaveAssetIfDirty(floor);
            ceiling = Surface("Ceiling", new Color32(144, 146, 135, 255));
            tube = Surface("Fluorescent", new Color32(222, 228, 205, 255), new Color(1.4f, 1.55f, 1.16f));
            signal = Surface("Wayfinding", new Color32(48, 106, 94, 255), new Color(.035f, .11f, .075f));

            Transform building = Group("Pabellon de aulas", null);
            shell = Group("Arquitectura", building);
            details = Group("Carpinteria y mobiliario", building);
            lighting = Group("Iluminacion nocturna", building);
            // One continuous slab and flush thresholds keep the controller grounded across the turn.
            Box(shell, "Terrazo", new Vector3(4.3f, -.1f, 7), new Vector3(21, .2f, 21), floor, true);
            Box(shell, "Techo pasillo", new Vector3(0, Ceiling + .08f, 6.2f), new Vector3(3.3f,.16f,15.25f), ceiling, true);
            Box(shell, "Techo retorno", new Vector3(4.95f, Ceiling + .08f,12.2f), new Vector3(6.6f,.16f,3.12f), ceiling, true);
            Box(shell, "Techo aula", new Vector3(11.05f, Ceiling + .08f,12.2f), new Vector3(5.85f,.16f,5.8f), ceiling, true);

            Wall(true, -1.55f, -1.40f, 2.695f);
            Door(-1.55f, 3.2f, -90f, 0f, "Puerta cerrada");
            Wall(true, -1.55f, 3.705f, 7.595f);
            Door(-1.55f, 8.1f, -90f, 25f, "Puerta entreabierta");
            Wall(true, -1.55f, 8.605f, 13.79f);
            Wall(false, -1.30f, -1.64f, 1.64f);
            Wall(true, 1.55f, -1.4f, 4.6f);
            Window();
            Wall(true, 1.55f, 7.6f, 10.79f);
            Wall(false, 13.7f, -1.64f, 8.29f);
            Wall(false, 10.7f, 1.46f, 8.29f);

            Wall(true, 8.2f, 9.35f, 11.695f);
            Door(8.2f, 12.2f, 90f, 93f, "Puerta abierta");
            Wall(true, 8.2f, 12.705f, 15.05f);
            Wall(true, 13.9f, 9.35f, 15.05f);
            Wall(false, 9.45f, 8.2f, 13.9f);
            Wall(false, 14.95f, 8.2f, 13.9f);

            // A shallow secondary classroom is visible through the partly open leaf.
            Wall(true, -5.0f, 5.9f, 10.35f);
            Wall(false, 6.0f, -5f, -1.55f);
            Wall(false, 10.25f, -5f, -1.55f);
            Box(shell, "Techo anexo", new Vector3(-3.27f, Ceiling+.08f,8.12f), new Vector3(3.5f,.16f,4.45f), ceiling);
            Prefab("CarpetaEscolar", "CarpetaIndividual", new Vector3(-3.35f, 0, 8.55f), -80f);
            FurnishClassroom();
            CorridorDetails();
            LightScene();
        }

        private void Door(float x, float z, float yaw, float angle, string label)
        {
            Transform instance = Prefab("Puerta", "PuertaInterior", new Vector3(x, 0, z), yaw);
            instance.name = label;
            instance.localScale = Vector3.one * 1.12f;
            // The original hinge carries both the wooden mesh and its BoxCollider.
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>())
            {
                behaviour.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            }
            Transform hinge = instance.Find("PuertaInterior_Ensamble/PuertaInterior_PivoteBisagra");
            hinge.localRotation = Quaternion.Euler(0, angle, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hinge);
            Box(shell, "Dintel", new Vector3(x, (Ceiling+DoorHeight)/2, z),
                new Vector3(.18f, Ceiling-DoorHeight, DoorWidth), plaster, true);
        }

        private void Wall(bool alongZ, float fixedAxis, float from, float to)
        {
            float middle = (from + to) / 2;
            Transform wall = Group("Muro", shell);
            wall.position = alongZ ? new Vector3(fixedAxis,0,middle) : new Vector3(middle,0,fixedAxis);
            wall.rotation = Quaternion.Euler(0, alongZ ? 90 : 0, 0);
            Box(wall, "Enlucido", new Vector3(0, Ceiling/2, 0), new Vector3(to-from,Ceiling,.18f), plaster, true);
            Box(wall, "Pintura inferior", new Vector3(0,.54f,0), new Vector3(to-from,1.08f,.184f), paint);
            Box(wall, "Guarda", new Vector3(0,1.10f,0), new Vector3(to-from,.037f,.196f), trim);
            Box(wall, "Rodapie", new Vector3(0,.065f,0), new Vector3(to-from,.13f,.205f), trim);
        }

        private void Window()
        {
            const float x=1.55f, z=6.1f;
            Box(shell, "Antepecho", new Vector3(x,.49f,z), new Vector3(.18f,.98f,3f), paint, true);
            Box(shell, "Dintel ventana", new Vector3(x,2.775f,z), new Vector3(.18f,.55f,3f), plaster, true);
            Box(details, "Alfeizar", new Vector3(x-.04f,.97f,z), new Vector3(.33f,.065f,3.1f), plaster);
            for (int i=0;i<=4;i++)
                Box(details, "Montante", new Vector3(x,1.735f,4.63f + i*.735f), new Vector3(.12f,1.53f,.05f), metal);
            foreach (float y in new[]{1.02f,2.08f,2.47f})
                Box(details, "Travesano", new Vector3(x,y,z), new Vector3(.10f,.045f,3f), metal);
            var glazing = Group("Vidrio", details);
            glazing.position = new Vector3(x,1.74f,z);
            glazing.gameObject.AddComponent<BoxCollider>().size = new Vector3(.02f,1.52f,3f);
            Prefab("Arbol", "ArbolMediano", new Vector3(7f,-1.4f,5.15f), 50f);
            Prefab("Arbol", "ArbolPequeno", new Vector3(9f,-1.1f,7.2f), 140f);
        }

        private void FurnishClassroom()
        {
            Prefab("Pizarra", "PizarraTiza", new Vector3(13.78f, .88f,12.2f), -90);
            Prefab("EscritorioDocente", "EscritorioBasico", new Vector3(12.84f,0,13.86f), -92);
            foreach (float x in new[] {9.9f,11.5f})
            foreach (float z in new[] {10.6f,13.85f})
                Prefab("CarpetaEscolar", "CarpetaIndividual", new Vector3(x,0,z), x < 10 && z < 11 ? 72f : 90f);
        }

        private void CorridorDetails()
        {
            for (int i=0;i<4;i++)
            {
                float z=.8f + 3.8f*i;
                Box(details, "Viga", new Vector3(0,2.99f,z), new Vector3(3.12f,.13f,.19f), plaster);
                Box(details, "Canaleta electrica", new Vector3(-1.42f,2.83f,z+1.1f), new Vector3(.055f,.051f,2.2f), metal);
            }
            Box(details, "Cartelera",new Vector3(1.426f,1.68f,2.38f),new Vector3(.065f,.89f,1.72f),trim);
            Box(details, "Corcho",new Vector3(1.388f,1.68f,2.38f),new Vector3(.024f,.80f,1.64f),cork);
            for (int i=0;i<4;i++)
            {
                var sheet = Box(details,"Circular",new Vector3(1.367f,1.69f+(i%3)*.056f,1.83f+i*.36f),
                    new Vector3(.008f,.34f,.25f),paper);
                sheet.Rotate(i%2==0 ? -4 : 7,0,0);
            }
            Sign(new Vector3(-1.447f,1.62f,2.42f),-90f, "A - 101", .36f);
            Sign(new Vector3(-1.447f,1.62f,7.32f),-90f, "A - 102", .36f);
            Sign(new Vector3(8.097f,1.62f,11.42f),90f, "A - 103", .36f);
            Sign(new Vector3(0,2.09f,13.592f),0f, "AULAS   >", .88f);
        }

        private void Sign(Vector3 position, float yaw, string caption, float width)
        {
            Transform sign = Group("Placa " + caption, details);
            sign.SetPositionAndRotation(position, Quaternion.Euler(0,yaw,0));
            Box(sign, "Placa",Vector3.zero,new Vector3(width,width*.32f,.028f), signal);
            var face = Group("Rotulo",sign);
            face.localPosition = new Vector3(0,0,-.017f);
            var text = face.gameObject.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = font;
            text.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            text.fontSize = 64;
            text.characterSize = width * .041f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color32(166,184,153,255);
            text.text = caption;
        }

        private void LightScene()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color32(40,49,62,255);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color32(30,37,46,255);
            RenderSettings.fogDensity = .032f;
            Fixture(new Vector3(0,2.93f,1.6f), true);
            Fixture(new Vector3(0,2.93f,9.0f), false);
            Fixture(new Vector3(5.25f,2.93f,12.2f), false);
            var pilot = Group("Piloto de emergencia", lighting);
            pilot.position = new Vector3(0f, 2.44f, 13.57f);
            Box(pilot, "Soporte", Vector3.zero, new Vector3(.29f, .15f, .10f), metal);
            var lens = Surface("EmergencyLens", new Color32(155, 51, 32, 255), new Color(2.0f, .13f, .045f));
            Box(pilot, "Lente", new Vector3(0f, 0f, -.06f), new Vector3(.19f, .072f, .04f), lens);
            Light glow = Group("Resplandor", pilot).gameObject.AddComponent<Light>();
            glow.transform.localPosition = new Vector3(0f, 0f, -.21f);
            glow.type = LightType.Point;
            glow.range = 2.2f;
            glow.intensity = .46f;
            glow.color = new Color32(255, 99, 66, 255);
            Light spot = Spotlight("Fluorescente de guardia", new Vector3(0,2.83f,1.6f), Vector3.down,
                new Color32(208,224,200,255), 2.6f, 9, 132);
            spot.innerSpotAngle = 85f;
            // A real shadow-casting source outside the opening projects the window bars across the floor.
            Spotlight("Luna en el patio", new Vector3(3.1f,3.10f,5.15f), new Vector3(-1,-.58f,.33f),
                new Color32(154,192,241,255), 5.3f, 12, 86);
        }

        private void Fixture(Vector3 position, bool lit)
        {
            Transform fixture = Group(lit ? "Luminaria encendida" : "Luminaria apagada", lighting);
            fixture.position = position;
            Box(fixture,"Carcasa",Vector3.zero,new Vector3(1.18f,.08f,.25f),metal);
            foreach (float z in new[] {-.064f,.064f})
                Box(fixture,"Tubo",new Vector3(0,-.05f,z),new Vector3(1.08f,.031f,.038f),lit ? tube : ceiling);
        }

        private Light Spotlight(string label, Vector3 position, Vector3 direction, Color color, float power, float reach, float cone)
        {
            var fixture = Group(label,lighting);
            fixture.SetPositionAndRotation(position, Quaternion.LookRotation(direction, Vector3.forward));
            var source = fixture.gameObject.AddComponent<Light>();
            source.type = LightType.Spot;
            source.range = reach;
            source.spotAngle = cone;
            source.innerSpotAngle = cone * .6f;
            source.color = color;
            source.intensity = power;
            source.shadows = LightShadows.Soft;
            source.shadowBias = .035f;
            source.shadowNormalBias = .15f;
            return source;
        }

        private Transform Prefab(string family, string model, Vector3 position, float yaw)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/ResourcePack/{family}/Prefabs/{model}.prefab");
            if (!asset) throw new InvalidOperationException("Prefab ausente: " + model);
            var instance = ((GameObject)PrefabUtility.InstantiatePrefab(asset, details)).transform;
            instance.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
            return instance;
        }

        private static Transform Group(string name, Transform parent)
        {
            var result = new GameObject(name).transform;
            result.SetParent(parent, false);
            return result;
        }

        private static Transform Box(Transform parent, string name, Vector3 position, Vector3 dimensions, Material material, bool solid = false)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent,false);
            box.transform.localPosition = position;
            box.transform.localScale = dimensions;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(box.GetComponent<BoxCollider>());
            return box.transform;
        }

        private static Material Surface(string name, Color color, Color emission = default)
        {
            string path = $"{Materials}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) {name = name};
                AssetDatabase.CreateAsset(mat,path);
            }
            mat.SetColor("_BaseColor",color);
            mat.SetFloat("_Smoothness",.16f);
            if (emission.maxColorComponent > 0)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor",emission);
            }
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssetIfDirty(mat);
            return mat;
        }

        private static Texture2D FloorTexture()
        {
            const string path = Materials + "/Terrazzo.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture) return texture;
            const int size = 128;
            texture = new Texture2D(size,size,TextureFormat.RGB24,true) {name = "Terrazzo", wrapMode = TextureWrapMode.Repeat, anisoLevel = 4};
            var pixels = new Color32[size*size];
            var noise = new System.Random(617);
            for (int y=0;y<size;y++)
            for (int x=0;x<size;x++)
            {
                int grain = noise.Next(-6,7) + (noise.Next(10)==0 ? noise.Next(-20,21) : 0);
                int tone = (((x/64)+(y/64))%2==0 ? 121 : 115) + grain;
                if (x%64==0 || y%64==0) tone -= 27;
                pixels[y*size+x] = new Color32((byte)(tone+1),(byte)(tone+1),(byte)(tone-3),255);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture,path);
            return texture;
        }
    }
}
