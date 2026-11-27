using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent, RequireComponent(typeof(Light))]
    public sealed class IntroductionSkyCycle : MonoBehaviour
    {
        private const float DriveMinutes = 120f;
        private const float FirstDuskMinutes = 80f;
        private const float SkyYaw = 253f;
        private const float ReflectionInterval = 10f;
        private const float RoadProximity = 10f;
        private const float DefaultChunkSize = 300f;
        private static readonly Color MoonColor = new Color32(153, 185, 238, 255);
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");
        private static readonly int FogSettings = Shader.PropertyToID("_FogSettings");

        [Serializable]
        public struct RouteSample
        {
            public Vector3 position;
            public float distance;

            public RouteSample(Vector3 position, float distance)
            {
                this.position = position;
                this.distance = distance;
            }
        }

        private readonly struct Lighting
        {
            public readonly float Minute, Sky, SunsetWarmth, Sun, Moon, Reflection, FogEndChunks;
            public readonly Color Ambient, Fog;

            public Lighting(float minute, float sky, float sunsetWarmth, float sun, float moon, float reflection,
                float fogEndChunks, Color32 ambient, Color32 fog)
            {
                Minute = minute;
                Sky = sky;
                SunsetWarmth = sunsetWarmth;
                Sun = sun;
                Moon = moon;
                Reflection = reflection;
                FogEndChunks = fogEndChunks;
                Ambient = ambient;
                Fog = fog;
            }
        }

        // Minutes since 16:00. The last driving state is also the first night state.
        private static readonly Lighting[] DriveLighting =
        {
            new(0f,   0.85f, 0.90f, 0.75f, 0f,     0.65f, 1.80f, new(149, 128, 105, 255), new(129, 91, 59, 255)),
            new(45f,  0.62f, 1.00f, 0.38f, 0f,     0.48f, 1.65f, new(111, 89, 79, 255), new(102, 64, 44, 255)),
            new(80f,  0.24f, 0.12f, 0.065f, 0.03f, 0.14f, 1.50f, new(39, 43, 55, 255), new(20, 22, 30, 255)),
            new(110f, 0.16f, 0.02f, 0.01f, 0.055f, 0.08f, 1.42f, new(24, 30, 43, 255), new(12, 15, 22, 255)),
            new(120f, 0.13f, 0f,    0f,    0.07f,  0.07f, 1.38f, new(17, 23, 35, 255), new(8, 11, 17, 255))
        };

        private static readonly Lighting DarkestNight = new(120f, 0.018f, 0f, 0f, 0.04f, 0.035f, 1.30f,
            new(9, 13, 21, 255), new(4, 6, 9, 255));

        [SerializeField] private Shader blendShader;
        [SerializeField] private Shader distanceFogShader;
        [SerializeField] private Cubemap[] skies;
        [SerializeField] private Rigidbody vehicle;
        [SerializeField] private RouteSample[] route;
        [SerializeField] private float startDistance;
        [SerializeField, Min(1f)] private float nightDuration = 120f;

        private Light moon;
        private Light sun;
        private Material sky;
        private Material fogMaterial;
        private DistanceFogPass fogPass;
        private ChunkRenderWindow chunks;
        private Material originalSky;
        private Light originalSun;
        private Color originalAmbient;
        private bool originalFogEnabled;
        private float originalReflection, nextReflectionUpdate, fogEndChunks;
        private float furthestDistance, nightElapsed;
        private float appliedMinute = -1f, appliedNight = -1f;
        private float environmentBrightness = 1f;
        private int skySegment = -1;
        private bool reflectionPending;

        public bool HasArrived { get; private set; }
        public float DrivingProgress => route != null && route.Length > 1
            ? Mathf.InverseLerp(startDistance, route[^1].distance, furthestDistance)
            : 0f;
        public float TimeOfDayHours => 16f + DrivingProgress * 2f;
        public float NightProgress => Mathf.Clamp01(nightElapsed / Mathf.Max(1f, nightDuration));

        public static Quaternion InitialMoonRotation => Quaternion.Euler(11f, 205f, 0f);

        internal bool ControlsLight(Light light) => light.gameObject == gameObject || light == sun;

        internal void SetEnvironmentBrightness(float multiplier)
        {
            if (environmentBrightness == multiplier) return;
            environmentBrightness = multiplier;
            appliedMinute = -1f;
            if (sky != null) ApplyLighting();
        }

        public void Configure(Shader shader, Shader fogShader, Cubemap[] cubemaps, Rigidbody truck, RouteSample[] samples,
            float origin)
        {
            blendShader = shader;
            distanceFogShader = fogShader;
            skies = cubemaps;
            vehicle = truck;
            route = samples;
            startDistance = origin;
        }

        public void BeginNight()
        {
            if (HasArrived || sky == null) return;
            HasArrived = true;
            furthestDistance = route[^1].distance;
            ApplyLighting();
        }

        private void Awake() => furthestDistance = startDistance;

        internal void SampleRoad(float distanceFromStart, out Vector3 point, out Vector3 tangent)
        {
            float distance = Mathf.Clamp(startDistance + distanceFromStart, route[0].distance, route[^1].distance);
            int low = 1;
            int high = route.Length - 1;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (route[mid].distance < distance) low = mid + 1;
                else high = mid;
            }
            RouteSample from = route[low - 1];
            RouteSample to = route[low];
            point = Vector3.Lerp(from.position, to.position, Mathf.InverseLerp(from.distance, to.distance, distance));
            tangent = (to.position - from.position).normalized;
        }

        private void OnEnable()
        {
            if (blendShader == null || skies == null || skies.Length != 3 || Array.Exists(skies, cube => cube == null) ||
                vehicle == null || route == null || route.Length < 2 || route[^1].distance <= startDistance)
            {
                Debug.LogError("El cielo del nivel 00 requiere tres cubemaps, la camioneta y el eje de la carretera.", this);
                return;
            }

            moon = GetComponent<Light>();
            originalSky = RenderSettings.skybox;
            originalSun = RenderSettings.sun;
            originalAmbient = RenderSettings.ambientLight;
            originalFogEnabled = RenderSettings.fog;
            originalReflection = RenderSettings.reflectionIntensity;

            if (distanceFogShader != null && distanceFogShader.isSupported)
            {
                chunks = GetComponentInParent<ChunkRenderWindow>();
                fogMaterial = CoreUtils.CreateEngineMaterial(distanceFogShader);
                fogPass = new DistanceFogPass(fogMaterial);
                RenderSettings.fog = false;
                RenderPipelineManager.beginCameraRendering += RenderDistanceFog;
            }
            else
            {
                Debug.LogWarning("No se pudo activar la niebla de distancia del nivel 00.", this);
            }

            var sunObject = new GameObject("Sol del atardecer");
            sunObject.transform.SetParent(transform.parent, false);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadowStrength = moon.shadowStrength;
            sky = new Material(blendShader) { name = "Cielo de la introducción" };
            float radians = SkyYaw * Mathf.Deg2Rad;
            var yaw = new Vector4(Mathf.Cos(radians), Mathf.Sin(radians), 0f, 0f);
            sky.SetVector("_YawA", yaw);
            sky.SetVector("_YawB", yaw);
            if (fogMaterial != null)
            {
                fogMaterial.SetVector("_YawA", yaw);
                fogMaterial.SetVector("_YawB", yaw);
            }
            RenderSettings.skybox = sky;
            skySegment = -1;
            appliedMinute = appliedNight = -1f;
            nextReflectionUpdate = 0f;
            ApplyLighting();
            UpdateReflection();
        }

        private void LateUpdate()
        {
            if (sky == null) return;
            if (HasArrived)
                nightElapsed = Mathf.Min(nightElapsed + Time.deltaTime, Mathf.Max(1f, nightDuration));
            else
                furthestDistance = Mathf.Max(furthestDistance, ProjectDistance(vehicle.position));

            ApplyLighting();
            UpdateReflection();
        }

        private float ProjectDistance(Vector3 position)
        {
            // The authored route is ordered by Z. Search only segments crossing the nearby Z band.
            int low = 1;
            int high = route.Length - 1;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (route[mid].position.z < position.z - RoadProximity) low = mid + 1;
                else high = mid;
            }

            float nearestSquared = RoadProximity * RoadProximity;
            float distance = furthestDistance;
            var point = new Vector2(position.x, position.z);
            for (int i = low; i < route.Length && route[i - 1].position.z <= position.z + RoadProximity; i++)
            {
                RouteSample from = route[i - 1];
                RouteSample to = route[i];
                var a = new Vector2(from.position.x, from.position.z);
                var segment = new Vector2(to.position.x, to.position.z) - a;
                float t = Mathf.Clamp01(Vector2.Dot(point - a, segment) / segment.sqrMagnitude);
                float squared = (point - (a + segment * t)).sqrMagnitude;
                if (squared >= nearestSquared) continue;
                nearestSquared = squared;
                distance = Mathf.Lerp(from.distance, to.distance, t);
            }
            return distance;
        }

        private void ApplyLighting()
        {
            float minute = DrivingProgress * DriveMinutes;
            float night = NightProgress;
            if (minute == appliedMinute && night == appliedNight) return;
            appliedMinute = minute;
            appliedNight = night;

            int segment = minute < FirstDuskMinutes ? 0 : 1;
            if (skySegment != segment)
            {
                sky.SetTexture("_SkyA", skies[segment]);
                sky.SetTexture("_SkyB", skies[segment + 1]);
                if (fogMaterial != null)
                {
                    fogMaterial.SetTexture("_SkyA", skies[segment]);
                    fogMaterial.SetTexture("_SkyB", skies[segment + 1]);
                }
                skySegment = segment;
            }
            float skyBlend = segment == 0
                ? Mathf.InverseLerp(0f, FirstDuskMinutes, minute)
                : Mathf.InverseLerp(FirstDuskMinutes, DriveMinutes, minute);
            skyBlend = Mathf.SmoothStep(0f, 1f, skyBlend);
            sky.SetFloat("_Blend", skyBlend);

            int next = 1;
            while (minute > DriveLighting[next].Minute) next++;
            Lighting from = DriveLighting[next - 1];
            Lighting to = DriveLighting[next];
            float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from.Minute, to.Minute, minute));
            if (HasArrived)
            {
                from = DriveLighting[^1];
                to = DarkestNight;
                blend = 1f - Mathf.Pow(1f - night, 2.5f);
            }

            float luminance = Mathf.Lerp(from.Sky, to.Sky, blend);
            float warmth = Mathf.Lerp(from.SunsetWarmth, to.SunsetWarmth, blend);
            sky.SetFloat("_Luminance", luminance);
            sky.SetFloat("_SunsetWarmth", warmth);
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(13f, 1f, DrivingProgress), SkyYaw + 180f, 0f);
            sun.color = Color.Lerp(new Color32(255, 199, 125, 255), new Color32(255, 140, 95, 255), DrivingProgress);
            sun.intensity = Mathf.Lerp(from.Sun, to.Sun, blend) * environmentBrightness;
            sun.enabled = sun.intensity > 0f;
            moon.transform.rotation = InitialMoonRotation;
            moon.color = MoonColor;
            moon.intensity = Mathf.Lerp(from.Moon, to.Moon, blend) * environmentBrightness;
            moon.enabled = moon.intensity > 0f;
            RenderSettings.sun = sun.intensity > moon.intensity ? sun : moon;
            sun.shadows = RenderSettings.sun == sun ? LightShadows.Soft : LightShadows.None;
            moon.shadows = RenderSettings.sun == moon ? LightShadows.Soft : LightShadows.None;

            Color ambient = Color.Lerp(from.Ambient, to.Ambient, blend);
            Color adjusted = (ambient.linear * environmentBrightness).gamma;
            adjusted.a = ambient.a;
            RenderSettings.ambientLight = adjusted;
            if (fogMaterial != null)
            {
                Color haze = Color.Lerp(from.Fog, to.Fog, blend);
                sky.SetColor(FogColor, haze);
                fogMaterial.SetColor(FogColor, haze);
                fogMaterial.SetFloat("_Blend", skyBlend);
                fogMaterial.SetFloat("_Luminance", luminance);
                fogMaterial.SetFloat("_SunsetWarmth", warmth);
                fogEndChunks = Mathf.Lerp(from.FogEndChunks, to.FogEndChunks, blend);
            }
            RenderSettings.reflectionIntensity = Mathf.Clamp01(
                Mathf.Lerp(from.Reflection, to.Reflection, blend) * environmentBrightness);
            reflectionPending = true;
        }

        private void UpdateReflection()
        {
            if (!reflectionPending || Time.unscaledTime < nextReflectionUpdate) return;
            DynamicGI.UpdateEnvironment();
            nextReflectionUpdate = Time.unscaledTime + ReflectionInterval;
            reflectionPending = false;
        }

        private void RenderDistanceFog(ScriptableRenderContext context, Camera camera)
        {
            if (camera.cameraType != CameraType.Game || camera.gameObject.scene != gameObject.scene ||
                !camera.TryGetComponent(out UniversalAdditionalCameraData cameraData) ||
                cameraData.renderType != CameraRenderType.Base)
                return;

            float chunkSize = chunks != null ? chunks.ChunkSize : DefaultChunkSize;
            Transform focus = chunks != null ? chunks.Focus : null;
            Vector3 center = focus != null ? focus.position : camera.transform.position;
            fogMaterial.SetVector(FogSettings, new Vector4(center.x, center.z, chunkSize, chunkSize * fogEndChunks));
            cameraData.scriptableRenderer.EnqueuePass(fogPass);
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= RenderDistanceFog;
            CoreUtils.Destroy(fogMaterial);
            fogMaterial = null;
            fogPass = null;
            if (sky == null) return;
            if (RenderSettings.skybox == sky)
            {
                RenderSettings.skybox = originalSky;
                RenderSettings.sun = originalSun;
                RenderSettings.ambientLight = originalAmbient;
                RenderSettings.fog = originalFogEnabled;
                RenderSettings.reflectionIntensity = originalReflection;
            }
            if (moon != null)
            {
                moon.transform.rotation = InitialMoonRotation;
                moon.intensity = 0.23f;
                moon.shadows = LightShadows.Soft;
                moon.enabled = true;
            }
            if (sun != null)
            {
                sun.enabled = false;
                Destroy(sun.gameObject);
            }
            Destroy(sky);
            sky = null;
        }

        private sealed class DistanceFogPass : ScriptableRenderPass
        {
            private readonly Material material;

            public DistanceFogPass(Material material)
            {
                this.material = material;
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing - 1;
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame)
            {
                var resources = frame.Get<UniversalResourceData>();
                using var builder = graph.AddRasterRenderPass<PassData>("Introduction distance fog", out var data);
                data.Material = material;
                builder.UseTexture(resources.cameraDepthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderFunc(static (PassData pass, RasterGraphContext context) =>
                    context.cmd.DrawProcedural(Matrix4x4.identity, pass.Material, 0, MeshTopology.Triangles, 3, 1));
            }

            private sealed class PassData
            {
                public Material Material;
            }
        }
    }
}
