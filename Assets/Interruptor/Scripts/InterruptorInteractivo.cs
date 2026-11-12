using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interruptores
{
    public enum InterruptorMotion
    {
        WallTilt,
        VerticalSlide,
        QuarterTurn
    }

    /// <summary>
    /// Click-driven ON/OFF motion for proyecto 3D interruptor prefabs.
    /// Attach this component to the moving pivot and give that pivot a Collider.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterruptorInteractivo : MonoBehaviour
    {
        [Header("Estado")]
        [SerializeField] private bool isOn = true;
        [SerializeField] private InterruptorMotion motion = InterruptorMotion.WallTilt;
        [SerializeField, Min(0.1f)] private float transitionSpeed = 16f;

        [Header("Movimiento")]
        [SerializeField] private Vector3 localAxis = Vector3.right;
        [SerializeField] private float wallOnAngle = 8f;
        [SerializeField] private float wallOffAngle = -8f;
        [SerializeField, Min(0f)] private float verticalTravel = 0.008f;
        [SerializeField] private float quarterTurnDegrees = 90f;

        [Header("Indicador (opcional)")]
        [SerializeField] private Renderer stateIndicator;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;

        [Header("Evento")]
        public UnityEvent<bool> OnStateChanged = new UnityEvent<bool>();

        private Vector3 restLocalPosition;
        private Quaternion restLocalRotation;
        private Vector3 targetLocalPosition;
        private Quaternion targetLocalRotation;

        public bool IsOn => isOn;

        private void Awake()
        {
            restLocalPosition = transform.localPosition;
            restLocalRotation = transform.localRotation;
            SetTargets();
            ApplyStateImmediately();
        }

        private void Update()
        {
            float blend = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, blend);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRotation, blend);
        }

        /// <summary>Unity mouse message; requires a Collider on this moving pivot.</summary>
        public void OnMouseDown()
        {
            ToggleState();
        }

        public void ToggleState()
        {
            SetState(!isOn);
        }

        public void SetState(bool value)
        {
            if (isOn == value)
            {
                SetTargets();
                ApplyIndicator();
                return;
            }

            isOn = value;
            SetTargets();
            ApplyIndicator();
            OnStateChanged.Invoke(isOn);
        }

        private void SetTargets()
        {
            Vector3 axis = localAxis.sqrMagnitude > 0f ? localAxis.normalized : Vector3.right;

            switch (motion)
            {
                case InterruptorMotion.VerticalSlide:
                    targetLocalPosition = restLocalPosition +
                        (isOn ? Vector3.zero : -axis * verticalTravel);
                    targetLocalRotation = restLocalRotation;
                    break;

                case InterruptorMotion.QuarterTurn:
                    targetLocalPosition = restLocalPosition;
                    targetLocalRotation = restLocalRotation *
                        Quaternion.AngleAxis(isOn ? 0f : quarterTurnDegrees, axis);
                    break;

                default:
                    targetLocalPosition = restLocalPosition;
                    targetLocalRotation = restLocalRotation *
                        Quaternion.AngleAxis(isOn ? wallOnAngle : wallOffAngle, axis);
                    break;
            }
        }

        private void ApplyStateImmediately()
        {
            transform.localPosition = targetLocalPosition;
            transform.localRotation = targetLocalRotation;
            ApplyIndicator();
        }

        private void ApplyIndicator()
        {
            if (stateIndicator == null)
                return;

            Material desired = isOn ? onMaterial : offMaterial;
            if (desired != null)
                stateIndicator.sharedMaterial = desired;
        }
    }
}