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
        [SerializeField, Tooltip("The saved hinge rotations are the open pose.")]
        private bool startsOpen = false;
        private Quaternion[] closedRotations;
        private Coroutine motion;

        public bool IsOpen { get; private set; }
        public string InteractionPrompt => IsOpen ? "[E] Cerrar" : "[E] Abrir";

        public void ConfigureHinges(Transform[] values)
        {
            hinges = values ?? new Transform[0];
            CachePose();
        }

        protected virtual void Awake()
        {
            CachePose();
            if (!startsOpen || hinges == null) return;

            IsOpen = true;
            for (int i = 0; i < hinges.Length; i++)
                if (hinges[i] != null) closedRotations[i] *= Quaternion.Inverse(OpenRotation(hinges[i]));
        }

        private void CachePose()
        {
            if (hinges == null) return;
            closedRotations = new Quaternion[hinges.Length];
            for (int i = 0; i < hinges.Length; i++)
                if (hinges[i] != null) closedRotations[i] = hinges[i].localRotation;
        }

        public void Interact() => SetOpen(!IsOpen);
        public void Interact(CharacterController explorer) => SetOpen(!IsOpen, explorer);
        public void Toggle() => Interact();

        public void SetOpen(bool open) => SetOpen(open, null);

        private void SetOpen(bool open, CharacterController explorer)
        {
            IsOpen = open;
            if (hinges == null || hinges.Length == 0) return;
            if (closedRotations == null || closedRotations.Length != hinges.Length) CachePose();
            Quaternion[] targets = new Quaternion[hinges.Length];
            for (int i = 0; i < hinges.Length; i++)
            {
                if (hinges[i] == null) continue;
                targets[i] = open
                    ? closedRotations[i] * OpenRotation(hinges[i])
                    : closedRotations[i];
            }

            if (motion != null) StopCoroutine(motion);
            if (!Application.isPlaying || transitionSeconds <= 0f)
            {
                for (int i = 0; i < hinges.Length; i++) if (hinges[i] != null) hinges[i].localRotation = targets[i];
                return;
            }
            motion = StartCoroutine(Animate(targets, explorer));
        }

        private Quaternion OpenRotation(Transform hinge)
        {
            float sign = hinge.name.IndexOf("derech", System.StringComparison.OrdinalIgnoreCase) >= 0 ? -1f : 1f;
            return Quaternion.AngleAxis(openAngle * sign, localAxis.normalized);
        }

        private IEnumerator Animate(Quaternion[] targets, CharacterController explorer)
        {
            Quaternion[] starts = new Quaternion[hinges.Length];
            Collider[][] leafColliders = explorer != null ? new Collider[hinges.Length][] : null;
            for (int i = 0; i < hinges.Length; i++) if (hinges[i] != null) starts[i] = hinges[i].localRotation;
            if (leafColliders != null)
                for (int i = 0; i < hinges.Length; i++)
                    if (hinges[i] != null) leafColliders[i] = hinges[i].GetComponentsInChildren<Collider>();

            float elapsed = 0f;
            while (elapsed < transitionSeconds)
            {
                float next = elapsed + (leafColliders != null ? Mathf.Min(Time.deltaTime, 1f / 60f) : Time.deltaTime);
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(next / transitionSeconds));
                if (explorer != null && WouldHitExplorer(starts, targets, t, leafColliders, explorer))
                {
                    yield return null;
                    continue;
                }

                elapsed = next;
                for (int i = 0; i < hinges.Length; i++)
                    if (hinges[i] != null) hinges[i].localRotation = Quaternion.Slerp(starts[i], targets[i], t);
                yield return null;
            }
            for (int i = 0; i < hinges.Length; i++) if (hinges[i] != null) hinges[i].localRotation = targets[i];
            motion = null;
        }

        private bool WouldHitExplorer(Quaternion[] starts, Quaternion[] targets, float t,
            Collider[][] leafColliders, CharacterController explorer)
        {
            for (int i = 0; i < hinges.Length; i++)
            {
                Transform hinge = hinges[i];
                if (hinge == null) continue;
                Quaternion next = Quaternion.Slerp(starts[i], targets[i], t);
                Quaternion delta = next * Quaternion.Inverse(hinge.localRotation);
                if (hinge.parent != null)
                    delta = hinge.parent.rotation * delta * Quaternion.Inverse(hinge.parent.rotation);

                foreach (Collider leaf in leafColliders[i])
                {
                    if (!leaf.enabled || leaf.isTrigger) continue;
                    Transform part = leaf.transform;
                    Vector3 position = hinge.position + delta * (part.position - hinge.position);
                    if (Physics.ComputePenetration(leaf, position, delta * part.rotation,
                        explorer, explorer.transform.position, explorer.transform.rotation, out _, out _))
                        return true;
                }
            }
            return false;
        }
    }
}
