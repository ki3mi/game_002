using System.Collections;
using UnityEngine;

namespace AssetInteraction
{
    public abstract class HingedInteractable : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private Transform[] hinges;
        [SerializeField, Range(45f, 140f)] private float openAngle = 95f;
        [SerializeField, Min(0f)] private float transitionSeconds = 0.25f;
        [SerializeField] private Vector3 localAxis = Vector3.up;
        private Quaternion[] closedRotations;
        private Coroutine motion;

        public bool IsOpen { get; private set; }
        public string InteractionPrompt => IsOpen ? "[E] Cerrar" : "[E] Abrir";

        public void ConfigureHinges(Transform[] values)
        {
            hinges = values ?? new Transform[0];
            CachePose();
        }

        protected virtual void Awake() => CachePose();

        private void CachePose()
        {
            if (hinges == null) return;
            closedRotations = new Quaternion[hinges.Length];
            for (int i = 0; i < hinges.Length; i++)
                if (hinges[i] != null) closedRotations[i] = hinges[i].localRotation;
        }

        public void Interact() => SetOpen(!IsOpen);
        public void Toggle() => Interact();

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (hinges == null || hinges.Length == 0) return;
            if (closedRotations == null || closedRotations.Length != hinges.Length) CachePose();
            Quaternion[] targets = new Quaternion[hinges.Length];
            for (int i = 0; i < hinges.Length; i++)
            {
                if (hinges[i] == null) continue;
                float sign = hinges[i].name.ToLowerInvariant().Contains("derech") ? -1f : 1f;
                targets[i] = open
                    ? closedRotations[i] * Quaternion.AngleAxis(openAngle * sign, localAxis.normalized)
                    : closedRotations[i];
            }

            if (motion != null) StopCoroutine(motion);
            if (!Application.isPlaying || transitionSeconds <= 0f)
            {
                for (int i = 0; i < hinges.Length; i++) if (hinges[i] != null) hinges[i].localRotation = targets[i];
                return;
            }
            motion = StartCoroutine(Animate(targets));
        }

        private IEnumerator Animate(Quaternion[] targets)
        {
            Quaternion[] starts = new Quaternion[hinges.Length];
            for (int i = 0; i < hinges.Length; i++) if (hinges[i] != null) starts[i] = hinges[i].localRotation;
            float elapsed = 0f;
            while (elapsed < transitionSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionSeconds));
                for (int i = 0; i < hinges.Length; i++)
                    if (hinges[i] != null) hinges[i].localRotation = Quaternion.Slerp(starts[i], targets[i], t);
                yield return null;
            }
            for (int i = 0; i < hinges.Length; i++) if (hinges[i] != null) hinges[i].localRotation = targets[i];
            motion = null;
        }
    }
}