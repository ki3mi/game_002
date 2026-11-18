using UnityEngine;

namespace TablerosEnergia
{
    /// <summary>
    /// Hinged enclosure door. Pressing E through the player's center reticle toggles it.
    /// Place this component and a fitted BoxCollider on the exported hinge pivot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoardDoorInteractable : MonoBehaviour
    {
        [SerializeField] private string displayName = "Puerta del tablero";
        [SerializeField] private bool startOpen = true;
        [SerializeField] private Vector3 closedEuler;
        [SerializeField] private Vector3 openEuler;
        [SerializeField, Min(0.1f)] private float degreesPerSecond = 300f;

        private Quaternion targetRotation;
        private bool isOpen;

        public string DisplayName => displayName;
        public bool IsOpen => isOpen;

        public void Configure(string label, Vector3 closed, Vector3 opened, bool initiallyOpen = true)
        {
            displayName = label;
            closedEuler = closed;
            openEuler = opened;
            startOpen = initiallyOpen;
            isOpen = initiallyOpen;
            targetRotation = Quaternion.Euler(initiallyOpen ? openEuler : closedEuler);
            transform.localRotation = targetRotation;
        }

        private void Awake()
        {
            isOpen = startOpen;
            targetRotation = Quaternion.Euler(isOpen ? openEuler : closedEuler);
            transform.localRotation = targetRotation;
        }

        private void Update()
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                targetRotation,
                degreesPerSecond * Time.deltaTime);
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            targetRotation = Quaternion.Euler(isOpen ? openEuler : closedEuler);
        }
    }
}
