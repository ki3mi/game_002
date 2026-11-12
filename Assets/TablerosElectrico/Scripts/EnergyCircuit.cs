using System;
using Interruptores;
using UnityEngine;

namespace TablerosEnergia
{
    public enum EnergyCircuitRole
    {
        Source,
        Feeder,
        Load
    }

    /// <summary>
    /// Gives one existing interactive switch a place in the game's estimated power tree.
    /// Attach it to the same pivot GameObject as InterruptorInteractivo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnergyCircuit : MonoBehaviour
    {
        [SerializeField] private string circuitId = "Circuit";
        [SerializeField] private string displayName = "Circuit";
        [SerializeField] private string parentCircuitId = "";
        [SerializeField, Min(0f)] private float estimatedWatts;
        [SerializeField] private EnergyCircuitRole role = EnergyCircuitRole.Load;

        private InterruptorInteractivo interactiveSwitch;
        private bool manuallyRequestedOn = true;

        public string CircuitId => circuitId;
        public string DisplayName => displayName;
        public string ParentCircuitId => parentCircuitId;
        public float EstimatedWatts => estimatedWatts;
        public EnergyCircuitRole Role => role;
        public bool RequestedOn => interactiveSwitch != null ? interactiveSwitch.IsOn : manuallyRequestedOn;
        public bool IsEnergized { get; internal set; }

        public void Configure(string id, string label, string parentId, float watts, EnergyCircuitRole circuitRole)
        {
            circuitId = id;
            displayName = label;
            parentCircuitId = parentId ?? "";
            estimatedWatts = Mathf.Max(0f, watts);
            role = circuitRole;
            BindSwitch();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            BindSwitch();
            EnergyGridRegistry.EnsureRuntimeInstance();
            EnergyGridRegistry.Instance?.Register(this);
        }

        private void OnDisable()
        {
            if (interactiveSwitch != null)
                interactiveSwitch.OnStateChanged.RemoveListener(OnSwitchStateChanged);

            if (Application.isPlaying)
                EnergyGridRegistry.Instance?.Unregister(this);
        }

        private void BindSwitch()
        {
            if (interactiveSwitch != null)
                interactiveSwitch.OnStateChanged.RemoveListener(OnSwitchStateChanged);

            interactiveSwitch = GetComponent<InterruptorInteractivo>();
            if (interactiveSwitch == null)
                interactiveSwitch = GetComponentInChildren<InterruptorInteractivo>(true);

            if (interactiveSwitch != null)
            {
                manuallyRequestedOn = interactiveSwitch.IsOn;
                interactiveSwitch.OnStateChanged.AddListener(OnSwitchStateChanged);
            }
        }

        private void OnSwitchStateChanged(bool isOn)
        {
            manuallyRequestedOn = isOn;
            EnergyGridRegistry.Instance?.Recalculate();
        }

        /// <summary>Allows automated gameplay or a test harness to set the request state.</summary>
        public void SetRequestedOn(bool isOn)
        {
            if (interactiveSwitch != null)
                interactiveSwitch.SetState(isOn);
            else
            {
                manuallyRequestedOn = isOn;
                EnergyGridRegistry.Instance?.Recalculate();
            }
        }
    }
}
