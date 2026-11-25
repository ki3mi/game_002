using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EcoElectry.Unity.Player
{
    internal sealed class CorridorBrightness
    {
        private const string DefaultBrightnessPreferenceKey = "EcoElectry.Corridor.Brightness";
        private const string GrainPreferenceKey = "EcoElectry.Corridor.FilmGrain";
        private const int DefaultLevel = 100;
        private const int MinLevel = 1;
        private const int MaxLevel = 220;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private readonly Camera view;
        private readonly string brightnessPreferenceKey;
        private readonly Color ambient;
        private readonly Color fog;
        private readonly Color background;
        private readonly float reflections;
        private readonly List<(Light Source, float Intensity)> lights = new();
        private readonly List<(Renderer Surface, int Slot, Vector4 Color)> emissions = new();
        private readonly MaterialPropertyBlock properties = new();
        private readonly FilmGrain grain;
        private readonly CorridorSignalGlitch cameraSignal;
        private readonly int defaultGrainLevel;
        private int level;
        private int grainLevel;
        private bool changed;

        internal bool IsOpen { get; private set; }

        internal CorridorBrightness(Camera view, Light flashlight, string brightnessPreferenceKey)
        {
            this.view = view;
            this.brightnessPreferenceKey = string.IsNullOrEmpty(brightnessPreferenceKey)
                ? DefaultBrightnessPreferenceKey
                : brightnessPreferenceKey;
            cameraSignal = view.GetComponent<CorridorSignalGlitch>();
            ambient = RenderSettings.ambientLight;
            fog = RenderSettings.fogColor;
            background = view.backgroundColor;
            reflections = RenderSettings.reflectionIntensity;

            foreach (GameObject root in view.gameObject.scene.GetRootGameObjects())
            {
                foreach (Light light in root.GetComponentsInChildren<Light>(true))
                {
                    if (light != flashlight) lights.Add((light, light.intensity));
                }

                foreach (Renderer surface in root.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] materials = surface.sharedMaterials;
                    for (int slot = 0; slot < materials.Length; slot++)
                    {
                        Material material = materials[slot];
                        if (material != null && material.HasProperty(EmissionColor) && material.IsKeywordEnabled("_EMISSION"))
                            emissions.Add((surface, slot, material.GetVector(EmissionColor)));
                    }
                }
            }

            level = Mathf.Clamp(PlayerPrefs.GetInt(this.brightnessPreferenceKey, DefaultLevel), MinLevel, MaxLevel);
            Apply();

            var volume = view.GetComponentInChildren<Volume>();
            if (volume != null && volume.profile.TryGet<FilmGrain>(out grain))
            {
                defaultGrainLevel = Mathf.RoundToInt(grain.intensity.value * 100f);
                grainLevel = Mathf.Clamp(PlayerPrefs.GetInt(GrainPreferenceKey, defaultGrainLevel), 0, 100);
                ApplyGrain();
            }
        }

        internal void Open() => IsOpen = true;

        internal void Close()
        {
            IsOpen = false;
            if (!changed) return;
            PlayerPrefs.SetInt(brightnessPreferenceKey, level);
            if (grain != null) PlayerPrefs.SetInt(GrainPreferenceKey, grainLevel);
            PlayerPrefs.Save();
            changed = false;
        }

        internal void Draw(string closeKey)
        {
            const float width = 360f;
            float height = grain != null ? 178f : 114f;
            float scale = Mathf.Min(Mathf.Clamp(Screen.height / 640f, 0.85f, 1.6f),
                Screen.width / (width + 24f), Screen.height / (height + 48f));
            var matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float x = (Screen.width / scale - width) * 0.5f;
            float y = Screen.height / scale - height - 24f;
            var color = GUI.color;
            GUI.color = new Color(0.06f, 0.08f, 0.10f, 0.97f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            GUI.color = new Color(0.88f, 0.72f, 0.48f);
            GUI.DrawTexture(new Rect(x, y, width, 2f), Texture2D.whiteTexture);
            GUI.color = color;

            int next = DrawSlider(x + 16f, y + 12f, width - 32f, "Brillo del entorno", level, MinLevel, MaxLevel);
            int nextGrain = grain != null
                ? DrawSlider(x + 16f, y + 76f, width - 32f, "Grano de la cámara", grainLevel, 0, 100)
                : grainLevel;
            GUI.Label(new Rect(x + 16f, y + height - 37f, 165f, 22f), $"{closeKey}  Cerrar");
            if (GUI.Button(new Rect(x + 240f, y + height - 40f, 104f, 26f), "Restablecer"))
            {
                next = DefaultLevel;
                nextGrain = defaultGrainLevel;
            }
            GUI.matrix = matrix;

            if (next == level && nextGrain == grainLevel) return;
            changed = true;
            if (next != level)
            {
                level = next;
                Apply();
            }
            if (grain != null && nextGrain != grainLevel)
            {
                grainLevel = nextGrain;
                ApplyGrain();
            }
        }

        private static int DrawSlider(float x, float y, float width, string label, int value, int min, int max)
        {
            GUI.Label(new Rect(x, y, 190f, 24f), label);
            GUI.Label(new Rect(x + width - 52f, y, 52f, 24f), $"{value} %");
            return Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(x, y + 28f, width, 22f), value, min, max));
        }

        private void ApplyGrain()
        {
            float intensity = grainLevel / 100f;
            grain.intensity.Override(intensity);
            if (cameraSignal != null)
            {
                float boost = Mathf.InverseLerp(0.65f, 1f, intensity);
                cameraSignal.GrainInterference = boost * boost;
            }
        }

        private void Apply()
        {
            float multiplier = level / 100f;
            if (view.gameObject.scene == SceneManager.GetActiveScene())
            {
                RenderSettings.ambientLight = ScaleColor(ambient, multiplier);
                RenderSettings.fogColor = ScaleColor(fog, multiplier);
                RenderSettings.reflectionIntensity = Mathf.Clamp01(reflections * multiplier);
            }
            view.backgroundColor = ScaleColor(background, multiplier);

            foreach (var light in lights)
            {
                if (light.Source != null) light.Source.intensity = light.Intensity * multiplier;
            }

            foreach (var emission in emissions)
            {
                if (emission.Surface == null) continue;
                emission.Surface.GetPropertyBlock(properties, emission.Slot);
                Vector4 color = emission.Color * multiplier;
                color.w = emission.Color.w;
                properties.SetVector(EmissionColor, color);
                emission.Surface.SetPropertyBlock(properties, emission.Slot);
            }
        }

        private static Color ScaleColor(Color source, float multiplier)
        {
            Color scaled = source.linear * multiplier;
            scaled.a = source.a;
            return scaled.gamma;
        }
    }
}
