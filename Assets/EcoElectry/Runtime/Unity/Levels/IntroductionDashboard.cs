using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace EcoElectry.Unity.Levels
{
    internal sealed class IntroductionDashboard
    {
        private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("es-PE");
        private readonly RectTransform screen;
        private readonly Text readout;
        private readonly Text speedReadout;
        private Vector3 lastPosition;
        private float metres;
        private int displayedHundredths = -1;
        private int displayedSpeed = -1;

        internal static IntroductionDashboard Create(Transform truck)
        {
            // The truck prefab already mounts this world canvas on the dashboard.
            Transform cluster = truck.Find("Canvas");
            return cluster != null ? new IntroductionDashboard(cluster) : null;
        }

        private IntroductionDashboard(Transform cluster)
        {
            screen = IntroductionDrivingCall.Plate(cluster, "Indicadores del tablero", 0, 0, 112, 61,
                new Color32(9, 31, 31, 245));
            screen.anchorMin = screen.anchorMax = new Vector2(0.5f, 0.5f);
            screen.localScale = Vector3.one * 0.001f;
            screen.anchoredPosition3D = new Vector3(-0.056f, -0.006f, -0.010f);

            var mint = new Color32(168, 255, 225, 255);
            IntroductionDrivingCall.Plate(screen, "Luz", 2, 4, 2, 53, mint);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            readout = IntroductionDrivingCall.Label(screen, "Distancia recorrida", font, 10, 35, 98, 26, 19, mint);
            speedReadout = IntroductionDrivingCall.Label(screen, "Velocidad", font, 10, 2, 98, 33, 23, mint);
            LightDigits(readout);
            LightDigits(speedReadout);
            Hide();
        }

        private static void LightDigits(Text digits)
        {
            digits.fontStyle = FontStyle.Bold;
            var glow = digits.gameObject.AddComponent<Outline>();
            glow.effectColor = new Color32(46, 205, 171, 150);
            glow.effectDistance = new Vector2(1.5f, 1.5f);
        }

        internal void Begin(Vector3 position, float speed)
        {
            lastPosition = position;
            screen.gameObject.SetActive(true);
            UpdateReadouts(position, speed);
        }

        internal void UpdateReadouts(Vector3 position, float speed)
        {
            int kmPerHour = Mathf.RoundToInt(speed * 3.6f);
            if (kmPerHour != displayedSpeed)
            {
                displayedSpeed = kmPerHour;
                speedReadout.text = kmPerHour.ToString(DisplayCulture) + " km/h";
            }

            Vector3 displacement = position - lastPosition;
            displacement.y = 0f;
            metres += displacement.magnitude;
            lastPosition = position;
            int hundredths = Mathf.FloorToInt(metres / 10f);
            if (hundredths == displayedHundredths) return;
            displayedHundredths = hundredths;
            readout.text = (hundredths / 100f).ToString("0.00", DisplayCulture) + " km";
        }

        internal void Hide() => screen.gameObject.SetActive(false);

        internal void Dispose()
        {
            if (screen != null) Object.Destroy(screen.gameObject);
        }
    }
}
