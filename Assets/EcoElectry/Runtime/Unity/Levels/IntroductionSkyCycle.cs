using UnityEngine;
using UnityEngine.Rendering;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent, RequireComponent(typeof(Light))]
    public sealed class IntroductionSkyCycle : MonoBehaviour
    {
        private static readonly Color MoonColor = new Color32(153, 185, 238, 255);
        private static readonly Color NightAmbient = new Color32(35, 45, 64, 255);
        private static readonly Color DayAmbient = new Color32(145, 157, 171, 255);
        private static readonly Color NightFog = new Color32(16, 24, 39, 255);
        private static readonly Color DayFog = new Color32(153, 174, 194, 255);

        private readonly struct Phase
        {
            public readonly float Hour, Yaw, Elevation, Sun, Moon;

            public Phase(float hour, float yaw, float elevation, float sun, float moon)
            {
                Hour = hour;
                Yaw = yaw;
                Elevation = elevation;
                Sun = sun;
                Moon = moon;
            }

            public Vector3 Direction => Quaternion.Euler(-Elevation, Yaw, 0f) * Vector3.forward;
        }

        // The visible discs are near +Z in the source cubemaps. Yaw rotates both the image and its light.
        private static readonly Phase[] Phases =
        {
            new Phase(0f,   0f,   9f, 0f,    0.23f), // Night MoonBurst at 22:00
            new Phase(2f,  24f,  14f, 0f,    0.16f), // Cold Night
            new Phase(5f,  48f,   4f, 0f,    0.03f), // Cartoon Base NightSky
            new Phase(8f,  90f,   3f, 0.26f, 0f),    // Epic Blue Sunset, used at dawn
            new Phase(11f, 125f, 15f, 0.92f, 0f),    // Cold Sunset
            new Phase(14f, 180f, 55f, 0.65f, 0f),    // Overcast Low, diffuse midday
            new Phase(19f, 253f,  3f, 0.36f, 0f),    // Epic Glorious Pink
            new Phase(21f, 270f,  6f, 0.06f, 0f),    // Deep Dusk
            new Phase(22.5f, 0f,  4f, 0f,    0.04f), // Space AnotherPlanet
        };

        [SerializeField] private Shader blendShader;
        [SerializeField] private Cubemap[] skies;
        [SerializeField, Min(1f)] private float cycleMinutes = 12f;

        private Light moon;
        private Light sun;
        private Material sky;
        private Material originalSky;
        private Light originalSun;
        private Color originalAmbient, originalFog;
        private float originalFogDensity, originalReflection, elapsedHours, nextReflectionUpdate;
        private int phaseIndex = -1;

        public static Quaternion InitialMoonRotation => Quaternion.LookRotation(-Phases[0].Direction);

        public void Configure(Shader shader, Cubemap[] cubemaps)
        {
            blendShader = shader;
            skies = cubemaps;
        }

        private void OnEnable()
        {
            if (blendShader == null || skies == null || skies.Length != Phases.Length ||
                System.Array.Exists(skies, cube => cube == null))
            {
                Debug.LogWarning("El ciclo de cielo requiere el shader y los nueve cubemaps de AllSkyFree.", this);
                return;
            }

            moon = GetComponent<Light>();
            originalSky = RenderSettings.skybox;
            originalSun = RenderSettings.sun;
            originalAmbient = RenderSettings.ambientLight;
            originalFog = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
            originalReflection = RenderSettings.reflectionIntensity;

            var sunObject = new GameObject("Sol del ciclo");
            sunObject.transform.SetParent(transform.parent, false);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadowStrength = moon.shadowStrength;
            sky = new Material(blendShader) { name = "Cielo del ciclo" };
            RenderSettings.skybox = sky;
            elapsedHours = 0f;
            phaseIndex = -1;
            nextReflectionUpdate = 0f;
            ApplyPhase();
        }

        private void Update()
        {
            if (sky == null) return;
            elapsedHours = Mathf.Repeat(elapsedHours + Time.deltaTime * 24f / (60f * cycleMinutes), 24f);
            ApplyPhase();
        }

        private void ApplyPhase()
        {
            int index = Phases.Length - 1;
            while (Phases[index].Hour > elapsedHours) index--;
            int next = (index + 1) % Phases.Length;
            Phase from = Phases[index];
            Phase to = Phases[next];
            float blend = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(from.Hour, next == 0 ? 24f : to.Hour, elapsedHours));
            if (index != phaseIndex)
            {
                sky.SetTexture("_SkyA", skies[index]);
                sky.SetTexture("_SkyB", skies[next]);
                sky.SetVector("_YawA", SkyYaw(from.Yaw));
                sky.SetVector("_YawB", SkyYaw(to.Yaw));
                phaseIndex = index;
            }
            sky.SetFloat("_Blend", blend);

            ApplyLight(sun, from, to, from.Sun, to.Sun, blend);
            ApplyLight(moon, from, to, from.Moon, to.Moon, blend);
            moon.color = MoonColor;
            float daylight = Mathf.Clamp01(sun.intensity / 0.75f);
            sun.color = Color.Lerp(new Color32(255, 156, 113, 255), new Color32(255, 242, 218, 255), daylight);
            RenderSettings.sun = sun.intensity > moon.intensity ? sun : moon;
            sun.shadows = RenderSettings.sun == sun ? LightShadows.Soft : LightShadows.None;
            moon.shadows = RenderSettings.sun == moon ? LightShadows.Soft : LightShadows.None;

            RenderSettings.ambientLight = Color.Lerp(NightAmbient, DayAmbient, daylight);
            RenderSettings.fogColor = Color.Lerp(NightFog, DayFog, daylight);
            RenderSettings.fogDensity = Mathf.Lerp(0.006f, 0.0017f, daylight);
            RenderSettings.reflectionIntensity = Mathf.Lerp(0.27f, 0.75f, daylight);
            if (Time.unscaledTime >= nextReflectionUpdate)
            {
                DynamicGI.UpdateEnvironment();
                nextReflectionUpdate = Time.unscaledTime + 12f;
            }
        }

        private static void ApplyLight(Light light, Phase from, Phase to, float start, float end, float blend)
        {
            light.intensity = Mathf.Lerp(start, end, blend);
            light.enabled = light.intensity > 0.001f;
            if (light.enabled)
            {
                float directionBlend = blend * end / light.intensity;
                light.transform.rotation = Quaternion.LookRotation(-Vector3.Slerp(from.Direction, to.Direction, directionBlend));
            }
        }

        private static Vector4 SkyYaw(float yaw)
        {
            float radians = yaw * Mathf.Deg2Rad;
            return new Vector4(Mathf.Cos(radians), Mathf.Sin(radians), 0f, 0f);
        }

        private void OnDisable()
        {
            if (sky == null) return;
            if (RenderSettings.skybox == sky)
            {
                RenderSettings.skybox = originalSky;
                RenderSettings.sun = originalSun;
                RenderSettings.ambientLight = originalAmbient;
                RenderSettings.fogColor = originalFog;
                RenderSettings.fogDensity = originalFogDensity;
                RenderSettings.reflectionIntensity = originalReflection;
            }
            if (moon != null)
            {
                moon.transform.rotation = InitialMoonRotation;
                moon.intensity = Phases[0].Moon;
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
    }
}
