using AssetInteraction;
using UnityEngine;

namespace VentiladorAssets
{
    public sealed class VentiladorInteractable : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private Transform[] rotors;
        [SerializeField] private Vector3 localSpinAxis = Vector3.up;
        [SerializeField, Min(1f)] private float revolutionsPerMinute = 180f;
        [SerializeField] private bool startOn;

        public bool IsRunning { get; private set; }
        public string InteractionPrompt => IsRunning ? "[E] Apagar ventilador" : "[E] Encender ventilador";

        private void Awake()
        {
            IsRunning = startOn;
            if (rotors == null || rotors.Length == 0) rotors = FindRotors();
        }

        public void ConfigureRotors(Transform[] targets, Vector3 axis)
        {
            rotors = targets ?? new Transform[0];
            localSpinAxis = axis.sqrMagnitude > 0.01f ? axis.normalized : Vector3.up;
        }

        private Transform[] FindRotors()
        {
            var result = new System.Collections.Generic.List<Transform>();
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("rotor") || n.Contains("helice") || n.Contains("aspas")) result.Add(t);
            }
            return result.ToArray();
        }

        public void Interact() => IsRunning = !IsRunning;
        public void SetRunning(bool value) => IsRunning = value;

        private void Update()
        {
            if (!IsRunning || rotors == null) return;
            float degrees = revolutionsPerMinute * 6f * Time.deltaTime;
            foreach (Transform rotor in rotors)
                if (rotor != null) rotor.Rotate(localSpinAxis, degrees, Space.Self);
        }
    }
}