using System.Collections;
using AssetInteraction;
using UnityEngine;

namespace VentanaAssets
{
    public enum WindowMotion { Sliding, TopHinged }

    public sealed class VentanaInteractable : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private WindowMotion motionType;
        [SerializeField] private Transform[] slidingSashes;
        [SerializeField] private Transform hingePivot;
        [SerializeField, Min(0.05f)] private float slideDistance = 0.35f;
        [SerializeField, Range(10f, 65f)] private float openAngle = 38f;
        [SerializeField, Min(0f)] private float transitionSeconds = 0.25f;
        private Vector3[] closedPositions;
        private Quaternion closedRotation;
        private bool isOpen;
        private Coroutine motion;

        public bool IsOpen => isOpen;
        public string InteractionPrompt => isOpen ? "[E] Cerrar ventana" : "[E] Abrir ventana";

        public void ConfigureSliding(Transform[] values)
        {
            motionType = WindowMotion.Sliding;
            slidingSashes = values ?? new Transform[0];
            CachePose();
        }

        public void ConfigureTopHinge(Transform pivot)
        {
            motionType = WindowMotion.TopHinged;
            hingePivot = pivot;
            CachePose();
        }

        private void Awake() => CachePose();

        private void CachePose()
        {
            if (slidingSashes != null)
            {
                closedPositions = new Vector3[slidingSashes.Length];
                for (int i = 0; i < slidingSashes.Length; i++)
                    if (slidingSashes[i] != null) closedPositions[i] = slidingSashes[i].localPosition;
            }
            if (hingePivot != null) closedRotation = hingePivot.localRotation;
        }

        public void Interact() => SetOpen(!isOpen);

        public void SetOpen(bool open)
        {
            isOpen = open;
            if (motion != null) StopCoroutine(motion);
            if (!Application.isPlaying || transitionSeconds <= 0f) { Apply(open ? 1f : 0f); return; }
            motion = StartCoroutine(Animate(open));
        }

        private IEnumerator Animate(bool open)
        {
            float elapsed = 0f;
            while (elapsed < transitionSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionSeconds));
                Apply(open ? t : 1f - t);
                yield return null;
            }
            Apply(open ? 1f : 0f);
            motion = null;
        }

        private void Apply(float t)
        {
            if (motionType == WindowMotion.Sliding && slidingSashes != null)
            {
                for (int i = 0; i < slidingSashes.Length; i++)
                {
                    Transform sash = slidingSashes[i];
                    if (sash == null) continue;
                    float sign = sash.name.ToLowerInvariant().Contains("izquierda") ? -1f : 1f;
                    Vector3 target = closedPositions[i] + Vector3.right * slideDistance * sign;
                    sash.localPosition = Vector3.Lerp(closedPositions[i], target, t);
                }
            }
            else if (hingePivot != null)
            {
                Quaternion target = closedRotation * Quaternion.AngleAxis(-openAngle, Vector3.right);
                hingePivot.localRotation = Quaternion.Slerp(closedRotation, target, t);
            }
        }
    }
}