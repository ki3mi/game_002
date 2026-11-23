using UnityEngine;

namespace EcoElectry.Unity.Environment
{
    /// <summary>
    /// Controls a continuous day/night cycle with independent Sun and Moon directional lights,
    /// optional sky visuals, and smooth ambient lighting transitions.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("0.0 = Midnight, 0.25 = Sunrise (6:00), 0.5 = Noon (12:00), 0.75 = Sunset (18:00)")]
        private float timeOfDay = 0.42f;

        [SerializeField, Min(5f)]
        [Tooltip("Real-time duration in seconds for one full 24-hour cycle.")]
        private float dayDurationSeconds = 120f;

        [SerializeField]
        private bool advanceTimeInPlayMode = true;

        [Header("Celestial Lights")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;
        [SerializeField] private float orbitYaw = 328f;

        [Header("Sky Visuals (Optional)")]
        [SerializeField] private Transform sunVisual;
        [SerializeField] private Transform moonVisual;
        [SerializeField, Min(50f)] private float visualDistance = 300f;

        [Header("Sun Settings")]
        [SerializeField, Min(0f)] private float maxSunIntensity = 1.6f;
        [SerializeField] private Color sunNoonColor = new Color(1f, 0.96f, 0.88f);
        [SerializeField] private Color sunHorizonColor = new Color(1f, 0.58f, 0.28f);

        [Header("Moon Settings")]
        [SerializeField, Min(0f)] private float maxMoonIntensity = 0.4f;
        [SerializeField] private Color moonColor = new Color(0.62f, 0.76f, 1f);

        [Header("Ambient Colors (Trilight)")]
        [SerializeField] private Color daySkyAmbient = new Color(0.56f, 0.72f, 0.90f);
        [SerializeField] private Color dayEquatorAmbient = new Color(0.48f, 0.52f, 0.56f);
        [SerializeField] private Color dayGroundAmbient = new Color(0.28f, 0.25f, 0.22f);

        [SerializeField] private Color nightSkyAmbient = new Color(0.08f, 0.11f, 0.20f);
        [SerializeField] private Color nightEquatorAmbient = new Color(0.06f, 0.08f, 0.14f);
        [SerializeField] private Color nightGroundAmbient = new Color(0.03f, 0.03f, 0.05f);

        public float TimeOfDay
        {
            get => timeOfDay;
            set
            {
                timeOfDay = Mathf.Repeat(value, 1f);
                ApplyCycle();
            }
        }

        private void OnEnable()
        {
            ApplyCycle();
        }

        private void OnValidate()
        {
            ApplyCycle();
        }

        private void Update()
        {
            if (UnityEngine.Application.isPlaying && advanceTimeInPlayMode && dayDurationSeconds > 0f)
            {
                timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime / dayDurationSeconds, 1f);
            }

            ApplyCycle();
        }

        private void LateUpdate()
        {
            UpdateVisualPositions();
        }

        private void ApplyCycle()
        {
            float sunPitch = (timeOfDay * 360f) - 90f;
            float moonPitch = sunPitch + 180f;

            float sunHeight = Mathf.Sin(timeOfDay * Mathf.PI * 2f - Mathf.PI * 0.5f);

            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.Euler(sunPitch, orbitYaw, 0f);
                float sunIntensity = maxSunIntensity * Mathf.Clamp01(sunHeight * 1.4f);
                sunLight.intensity = sunIntensity;
                sunLight.color = Color.Lerp(sunHorizonColor, sunNoonColor, Mathf.Clamp01(sunHeight * 1.8f));
                sunLight.enabled = sunIntensity > 0.005f;
                RenderSettings.sun = sunLight;
            }

            if (moonLight != null)
            {
                moonLight.transform.rotation = Quaternion.Euler(moonPitch, orbitYaw, 0f);
                float moonIntensity = maxMoonIntensity * Mathf.Clamp01(-sunHeight * 1.4f);
                moonLight.intensity = moonIntensity;
                moonLight.color = moonColor;
                moonLight.enabled = moonIntensity > 0.005f;
            }

            if (sunVisual != null)
            {
                sunVisual.gameObject.SetActive(sunHeight > -0.15f);
            }

            if (moonVisual != null)
            {
                moonVisual.gameObject.SetActive(sunHeight < 0.15f);
            }

            float ambientBlend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((sunHeight + 0.25f) * 2f));
            RenderSettings.ambientSkyColor = Color.Lerp(nightSkyAmbient, daySkyAmbient, ambientBlend);
            RenderSettings.ambientEquatorColor = Color.Lerp(nightEquatorAmbient, dayEquatorAmbient, ambientBlend);
            RenderSettings.ambientGroundColor = Color.Lerp(nightGroundAmbient, dayGroundAmbient, ambientBlend);

            UpdateVisualPositions();
        }

        private void UpdateVisualPositions()
        {
            Vector3 anchor = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

            if (sunVisual != null && sunLight != null)
            {
                sunVisual.position = anchor - sunLight.transform.forward * visualDistance;
            }

            if (moonVisual != null && moonLight != null)
            {
                moonVisual.position = anchor - moonLight.transform.forward * visualDistance;
            }
        }
    }
}
