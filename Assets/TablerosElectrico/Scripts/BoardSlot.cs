using System;
using Interruptores;
using UnityEngine;

namespace TablerosEnergia
{
    public enum BoardPartKind { Breaker, Disconnector, Terminals, Differential, Contactor, Meter }
    public enum BoardSlotRole { Load, Feeder, Main, Terminals, Differential, Contactor, Meter }
    public enum InstallResult { Installed, Occupied, Incompatible, MissingPrefab, RequiresPlayMode }

    /// <summary>An individually addressable mounting position on a modular board.</summary>
    [DisallowMultipleComponent]
    public sealed class BoardSlot : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private string displayName;
        [SerializeField] private BoardPartKind acceptedPart;
        [SerializeField] private BoardSlotRole role;
        [SerializeField, Min(0f)] private float estimatedWatts;
        [SerializeField] private GameObject componentPrefab;
        [SerializeField] private GameObject blankCover;
        [SerializeField] private GameObject[] wires = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] feedWires = Array.Empty<GameObject>();
        [SerializeField] private Transform outputPort;

        private GameObject installed;
        private InterruptorInteractivo switchControl;
        private ModularBoard owner;
        private bool energized;

        public string SlotId => slotId;
        public string DisplayName => displayName;
        public BoardPartKind AcceptedPart => acceptedPart;
        public BoardSlotRole Role => role;
        public float EstimatedWatts => estimatedWatts;
        public bool IsOccupied => installed != null;
        public bool IsEnergized => energized;
        public GameObject InstalledObject => installed;
        public InterruptorInteractivo SwitchControl => switchControl;
        public ModularBoard Board => owner != null ? owner : GetComponentInParent<ModularBoard>();
        public Transform OutputPort => outputPort;
        public event Action<BoardSlot> Changed;

        public bool RequestedOn
        {
            get
            {
                if (!IsOccupied) return false;
                var differential = installed.GetComponentInChildren<DifferentialDevice>(true);
                if (differential != null && differential.IsTripped) return false;
                var contactor = installed.GetComponentInChildren<ContactorDevice>(true);
                if (contactor != null) return contactor.ControlSignal;
                return switchControl == null || switchControl.IsOn;
            }
        }

        public void Configure(string id, string label, BoardPartKind kind, BoardSlotRole slotRole,
            float watts, GameObject prefab, GameObject cover, GameObject[] slotWires,
            GameObject[] incomingWires, Transform outgoingPort)
        {
            slotId = id;
            displayName = label;
            acceptedPart = kind;
            role = slotRole;
            estimatedWatts = Mathf.Max(0f, watts);
            componentPrefab = prefab;
            blankCover = cover;
            wires = slotWires ?? Array.Empty<GameObject>();
            feedWires = incomingWires ?? Array.Empty<GameObject>();
            outputPort = outgoingPort;
        }

        private void Awake()
        {
            owner = GetComponentInParent<ModularBoard>();
            RefreshAppearance();
        }

        public InstallResult TryInstall(BoardPartKind part)
        {
            if (!Application.isPlaying) return InstallResult.RequiresPlayMode;
            if (IsOccupied) return InstallResult.Occupied;
            if (part != acceptedPart) return InstallResult.Incompatible;
            if (componentPrefab == null) return InstallResult.MissingPrefab;

            installed = Instantiate(componentPrefab, transform, false);
            installed.name = componentPrefab.name;
            switchControl = installed.GetComponentInChildren<InterruptorInteractivo>(true);
            if (switchControl != null)
            {
                switchControl.OnStateChanged.AddListener(OnSwitchChanged);
                switchControl.SetState(false);
            }
            var differential = installed.GetComponentInChildren<DifferentialDevice>(true);
            if (differential != null) differential.Bind(this);
            var contactor = installed.GetComponentInChildren<ContactorDevice>(true);
            if (contactor != null) contactor.Bind(this);
            NotifyChanged();
            return InstallResult.Installed;
        }

        public bool TryRemove(out BoardPartKind part)
        {
            part = acceptedPart;
            if (!Application.isPlaying || !IsOccupied) return false;
            if (switchControl != null)
                switchControl.OnStateChanged.RemoveListener(OnSwitchChanged);
            installed.SetActive(false);
            Destroy(installed);
            installed = null;
            switchControl = null;
            energized = false;
            NotifyChanged();
            return true;
        }

        public void SetEstimatedWatts(float watts)
        {
            estimatedWatts = Mathf.Max(0f, watts);
            NotifyChanged();
        }

        private void OnSwitchChanged(bool unused) { NotifyChanged(); }

        public void NotifyChanged()
        {
            RefreshAppearance();
            Changed?.Invoke(this);
            Board?.NotifyChanged();
        }

        internal void SetEnergized(bool value) { energized = value; }

        internal void RefreshAppearance()
        {
            if (blankCover != null) blankCover.SetActive(!IsOccupied);
            bool installedWires = IsOccupied;
            if (role == BoardSlotRole.Main || role == BoardSlotRole.Differential)
                installedWires &= Board != null && Board.HasTerminals;
            foreach (var item in wires)
                if (item != null) item.SetActive(installedWires);
            bool incoming = IsOccupied && Board != null && Board.HasSupplyLink;
            foreach (var item in feedWires)
                if (item != null) item.SetActive(incoming);
        }
    }
}
