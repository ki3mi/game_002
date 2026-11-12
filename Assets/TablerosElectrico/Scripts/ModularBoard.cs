using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TablerosEnergia
{
    public enum ModularBoardKind { Secondary, General, Automation }

    /// <summary>A self-contained cabinet. Feeds are linked by instance reference and slot ID.</summary>
    [DisallowMultipleComponent]
    public sealed class ModularBoard : MonoBehaviour
    {
        [SerializeField] private ModularBoardKind kind;
        [SerializeField] private BoardSlot[] slots = Array.Empty<BoardSlot>();
        [SerializeField] private Transform[] incomingPorts = Array.Empty<Transform>();
        [SerializeField] private Material[] cableMaterials = Array.Empty<Material>();
        [SerializeField] private ModularBoard upstreamBoard;
        [SerializeField] private string upstreamOutputId;

        private bool externalConnected;
        private bool externalPowered;
        private readonly LineRenderer[] linkLines = new LineRenderer[3];
        private string instanceId;
        public event Action<ModularBoard> Changed;

        public ModularBoardKind Kind => kind;
        public string InstanceId => instanceId;
        public IReadOnlyList<BoardSlot> Slots => slots;
        public ModularBoard UpstreamBoard => upstreamBoard;
        public string UpstreamOutputId => upstreamOutputId;
        public bool HasSupplyLink => externalConnected || upstreamBoard != null;
        public bool HasTerminals => slots.Any(s => s != null && s.Role == BoardSlotRole.Terminals && s.IsOccupied);
        public bool HasIncomingPower => EvaluateInput(new HashSet<ModularBoard> { this });
        public bool IsBusEnergized => EvaluateBus(new HashSet<ModularBoard>());

        public void Configure(ModularBoardKind boardKind, BoardSlot[] positions,
            Transform[] inputPoints, Material[] lineMaterials)
        {
            kind = boardKind;
            slots = positions ?? Array.Empty<BoardSlot>();
            incomingPorts = inputPoints ?? Array.Empty<Transform>();
            cableMaterials = lineMaterials ?? Array.Empty<Material>();
        }

        private void Awake()
        {
            instanceId = Guid.NewGuid().ToString("N");
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<BoardSlot>(true);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            EnergyGridRegistry.EnsureRuntimeInstance();
            EnergyGridRegistry.Instance.RegisterBoard(this);
        }

        private void OnDisable()
        {
            if (Application.isPlaying && EnergyGridRegistry.Instance != null)
                EnergyGridRegistry.Instance.UnregisterBoard(this);
            foreach (var line in linkLines)
                if (line != null) line.enabled = false;
        }

        public BoardSlot FindSlot(string id)
        {
            return slots.FirstOrDefault(s => s != null && s.SlotId == id);
        }

        /// <summary>Connects this input to a feeder on one particular upstream board instance.</summary>
        public bool ConnectFeed(ModularBoard source, string outputSlotId)
        {
            if (source == null || source == this) return false;
            BoardSlot output = source.FindSlot(outputSlotId);
            if (output == null || output.Role != BoardSlotRole.Feeder || output.OutputPort == null)
                return false;
            var visited = new HashSet<ModularBoard>();
            for (var p = source; p != null; p = p.upstreamBoard)
                if (p == this || !visited.Add(p)) return false;
            upstreamBoard = source;
            upstreamOutputId = outputSlotId;
            externalConnected = false;
            externalPowered = false;
            NotifyChanged();
            UpdateLinkCable();
            return true;
        }

        /// <summary>Connects a game-provided source; powered can change without removing its cable.</summary>
        public void SetExternalSupply(bool connected, bool powered)
        {
            upstreamBoard = null;
            upstreamOutputId = string.Empty;
            externalConnected = connected;
            externalPowered = connected && powered;
            NotifyChanged();
            UpdateLinkCable();
        }

        public void DisconnectSupply() { SetExternalSupply(false, false); }

        public void NotifyChanged()
        {
            if (!Application.isPlaying) return;
            EnergyGridRegistry.EnsureRuntimeInstance();
            EnergyGridRegistry.Instance.Recalculate();
        }

        private bool EvaluateInput(HashSet<ModularBoard> visited)
        {
            if (!HasTerminals) return false;
            if (upstreamBoard != null)
            {
                if (!upstreamBoard.isActiveAndEnabled) return false;
                BoardSlot output = upstreamBoard.FindSlot(upstreamOutputId);
                return output != null && output.Role == BoardSlotRole.Feeder &&
                    output.RequestedOn && upstreamBoard.EvaluateBus(visited);
            }
            return externalConnected && externalPowered;
        }

        private bool EvaluateBus(HashSet<ModularBoard> visited)
        {
            if (!isActiveAndEnabled || !visited.Add(this)) return false;
            bool input = EvaluateInput(visited);
            BoardSlotRole gateRole = kind == ModularBoardKind.General ? BoardSlotRole.Main :
                kind == ModularBoardKind.Secondary ? BoardSlotRole.Differential : BoardSlotRole.Contactor;
            BoardSlot gate = slots.FirstOrDefault(s => s != null && s.Role == gateRole);
            bool result = input && gate != null && gate.RequestedOn;
            visited.Remove(this);
            return result;
        }

        internal void RefreshState()
        {
            bool input = HasIncomingPower;
            bool bus = IsBusEnergized;
            foreach (var slot in slots)
            {
                if (slot == null) continue;
                bool power;
                switch (slot.Role)
                {
                    case BoardSlotRole.Terminals: power = slot.IsOccupied && input; break;
                    case BoardSlotRole.Main:
                    case BoardSlotRole.Differential:
                    case BoardSlotRole.Contactor: power = input && slot.RequestedOn; break;
                    case BoardSlotRole.Meter: power = slot.IsOccupied && input; break;
                    default: power = bus && slot.RequestedOn; break;
                }
                slot.SetEnergized(power);
                slot.RefreshAppearance();
                if (slot.IsOccupied)
                {
                    var contactor = slot.InstalledObject.GetComponentInChildren<ContactorDevice>(true);
                    if (contactor != null) contactor.RefreshPower(input);
                }
            }
            UpdateLinkCable();
            Changed?.Invoke(this);
        }

        internal ModularBoard RootBoard()
        {
            var seen = new HashSet<ModularBoard>();
            ModularBoard current = this;
            while (current.upstreamBoard != null && seen.Add(current)) current = current.upstreamBoard;
            return current;
        }

        internal float ActiveWatts()
        {
            return slots.Where(s => s != null && s.Role == BoardSlotRole.Load && s.IsEnergized)
                .Sum(s => s.EstimatedWatts);
        }

        internal int ActiveLoads() => slots.Count(s => s != null && s.Role == BoardSlotRole.Load && s.IsEnergized);
        internal int ConfiguredLoads() => slots.Count(s => s != null && s.Role == BoardSlotRole.Load);

        private void LateUpdate()
        {
            if (upstreamBoard != null) UpdateLinkCable();
        }

        private void UpdateLinkCable()
        {
            if (!Application.isPlaying) return;
            BoardSlot output = upstreamBoard != null ? upstreamBoard.FindSlot(upstreamOutputId) : null;
            bool show = output != null && output.OutputPort != null && incomingPorts.Length >= 3 &&
                incomingPorts.All(p => p != null);
            for (int i = 0; i < linkLines.Length; i++)
            {
                if (!show)
                {
                    if (linkLines[i] != null) linkLines[i].enabled = false;
                    continue;
                }
                if (linkLines[i] == null)
                {
                    var cable = new GameObject("Cable_Alimentacion_" + new[] { "L", "N", "PE" }[i]);
                    cable.transform.SetParent(transform, false);
                    var line = cable.AddComponent<LineRenderer>();
                    line.useWorldSpace = true;
                    line.positionCount = 4;
                    line.startWidth = line.endWidth = .0035f;
                    line.numCornerVertices = 2;
                    line.numCapVertices = 2;
                    if (cableMaterials.Length > i) line.sharedMaterial = cableMaterials[i];
                    linkLines[i] = line;
                }
                Vector3 start = output.OutputPort.position + upstreamBoard.transform.right * ((i - 1) * .004f);
                Vector3 end = incomingPorts[i].position;
                float lower = Mathf.Min(start.y, end.y) - .12f - i * .006f;
                if (kind == ModularBoardKind.Automation)
                    lower = Mathf.Max(transform.position.y + .012f + i * .005f, lower);
                linkLines[i].SetPositions(new[] { start, new Vector3(start.x, lower, start.z),
                    new Vector3(end.x, lower, end.z), end });
                linkLines[i].enabled = true;
            }
        }
    }
}
