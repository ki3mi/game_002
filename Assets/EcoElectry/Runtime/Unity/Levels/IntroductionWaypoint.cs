using UnityEngine;
using UnityEngine.UI;

namespace EcoElectry.Unity.Levels
{
    internal sealed class IntroductionWaypoint
    {
        private readonly RectTransform canvas;
        private readonly RectTransform marker;
        private readonly Text label;
        private readonly Text instruction;
        private int objective;
        internal bool ReachedWorkshop => objective >= 2;

        internal IntroductionWaypoint(RectTransform canvas, Font font)
        {
            this.canvas = canvas;
            marker = IntroductionDrivingCall.Plate(canvas, "Objetivo temporal", 0, 0, 248, 44,
                new Color32(24, 28, 29, 247));
            marker.anchorMin = marker.anchorMax = marker.pivot = new Vector2(0.5f, 0.5f);
            Color amber = new Color32(237, 177, 96, 255);
            RectTransform diamond = IntroductionDrivingCall.Plate(marker, "Marcador", 24, 12, 14, 14, amber);
            diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
            label = IntroductionDrivingCall.Label(marker, "Destino", font, 47, 0, 195, 44, 18, Color.white);
            RectTransform banner = IntroductionDrivingCall.Plate(canvas, "Paso del tutorial", -310, 42, 620, 48,
                new Color32(24, 28, 29, 247));
            banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 0f);
            banner.anchoredPosition = new Vector2(-310f, 42f);
            instruction = IntroductionDrivingCall.Label(banner, "Instrucción", font, 12, 0, 596, 48, 22, Color.white);
            instruction.alignment = TextAnchor.MiddleCenter;
            Hide();
        }

        internal void Update(Camera view, Vector3 player, Transform booth, Transform plate, bool guardFinished)
        {
            if (view == null || objective >= 2) return;
            if (objective == 0 && guardFinished) objective = 1;
            Transform target = objective == 0 ? booth : plate;
            if (target == null)
            {
                objective++;
                Hide();
                return;
            }

            Vector3 approach = objective == 0
                ? GuardApproach(target)
                : target.position - target.parent.forward * 0.9f;
            float distance = Vector3.ProjectOnPlane(approach - player, Vector3.up).magnitude;
            if (distance <= 2.5f)
            {
                if (objective == 1) objective++;
                Hide();
                return;
            }

            Vector3 focus = objective == 0 ? approach + Vector3.up * 2.8f : target.position + Vector3.up * 0.55f;
            ShowMarker(view, focus, (objective == 0 ? "Caseta" : "Mantenimiento M-01") + $" · {Mathf.CeilToInt(distance)} m");
        }

        internal void ShowTutorial(Camera view, Vector3 position, string target, string step)
        {
            ShowMarker(view, position, target);
            if (instruction.text != step) instruction.text = step;
            instruction.transform.parent.gameObject.SetActive(true);
        }

        private void ShowMarker(Camera view, Vector3 positionInWorld, string text)
        {
            Vector3 projected = view.WorldToViewportPoint(positionInWorld);
            Vector2 size = canvas.rect.size;
            Vector2 position = new Vector2(projected.x - 0.5f, projected.y - 0.5f);
            if (projected.z <= 0f)
                position = new Vector2(projected.x <= 0.5f ? 0.5f : -0.5f, -0.26f);

            marker.anchoredPosition = new Vector2(
                Mathf.Clamp(position.x * size.x, -size.x * 0.5f + 144f, size.x * 0.5f - 144f),
                Mathf.Clamp(position.y * size.y, -size.y * 0.5f + 220f, size.y * 0.5f - 48f));
            if (label.text != text) label.text = text;
            marker.gameObject.SetActive(true);
        }

        internal static Vector3 GuardApproach(Transform booth) => booth.TransformPoint(0.3f, 0f, -3.25f);

        internal void Hide()
        {
            if (marker != null) marker.gameObject.SetActive(false);
            if (instruction != null) instruction.transform.parent.gameObject.SetActive(false);
        }

        internal void Complete()
        {
            objective = 2;
            Hide();
        }
    }
}
