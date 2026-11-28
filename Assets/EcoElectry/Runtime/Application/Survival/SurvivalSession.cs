using System;
using System.Collections.Generic;
using EcoElectry.Domain.Survival;

namespace EcoElectry.Application.Survival
{
    public sealed class SurvivalSession
    {
        private readonly FlashlightState flashlight;
        private readonly FearState fear = new();
        private readonly List<BatteryCell> inventory = new();
        private readonly HashSet<string> obtained = new(StringComparer.Ordinal);

        public SurvivalSession(BatteryCell first, BatteryCell second, bool requestedOn, IEnumerable<BatteryCell> spares = null)
        {
            flashlight = new FlashlightState(first, second, requestedOn);
            Remember(first);
            Remember(second);
            Inventory = inventory.AsReadOnly();
            if (spares != null)
                foreach (BatteryCell spare in spares)
                    if (!TryCollect(spare)) throw new ArgumentException("El inventario inicial contiene una batería duplicada.", nameof(spares));
        }

        public event Action Fainted;
        public bool IsDefeated { get; private set; }
        public BatteryInspection Inspection { get; private set; }
        public bool IsInspecting => Inspection != null;
        public bool HasBeam => !IsInspecting && !IsDefeated && flashlight.HasBeam;
        public bool RequestedOn => flashlight.RequestedOn;
        public int ActiveSlot => flashlight.ActiveSlot;
        public double TotalCharge => flashlight.TotalCharge;
        public double Fear => fear.Value;
        public bool ShowFear => fear.Visible;
        public IReadOnlyList<BatteryCell> Inventory { get; }

        public BatteryCell GetSlot(int index) => flashlight.GetSlot(index);
        public bool HasCollected(string id) => obtained.Contains(id);

        public void ToggleFlashlight()
        {
            if (!IsInspecting && !IsDefeated) flashlight.Toggle();
        }

        public bool TryCollect(BatteryCell cell)
        {
            if (IsInspecting || IsDefeated || cell == null || !obtained.Add(cell.Id)) return false;
            inventory.Add(cell);
            return true;
        }

        public bool BeginInspection()
        {
            if (IsDefeated || (Inventory.Count == 0 && (GetSlot(0) == null || GetSlot(1) == null))) return false;
            Inspection ??= new BatteryInspection(GetSlot(0), GetSlot(1), Inventory);
            return true;
        }

        public void CancelInspection() => Inspection = null;

        public double? MeasureSlot(int slot)
        {
            if (Inspection?.CanMeasure != true || (uint)slot >= FlashlightState.SlotCount) return null;
            return Inspection.Slots[slot]?.Voltage;
        }

        public bool ConfirmReplacement(int slot, string spareId)
        {
            if (!IsInspecting || slot < 0 || slot >= FlashlightState.SlotCount) return false;
            BatteryCell spare = inventory.Find(cell => cell.Id == spareId);
            int trayIndex = Inspection.FindOnTray(spare);
            if (trayIndex < 0) return false;
            Inspection.TryExtract(slot, out _);
            return Inspection.TryInsert(trayIndex, slot) && ConfirmInspection();
        }

        public bool ConfirmInspection()
        {
            if (Inspection == null) return false;
            BatteryCell first = Inspection.Slots[0];
            BatteryCell second = Inspection.Slots[1];
            flashlight.SetSlots(first, second);
            inventory.Remove(first);
            inventory.Remove(second);
            Inspection = null;
            return true;
        }

        public void Advance(double seconds, double secondsPerCell, FearRules rules, bool paused = false)
        {
            if (paused || IsDefeated || seconds == 0) return;
            if (!(seconds > 0) || double.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!(secondsPerCell > 0) || double.IsInfinity(secondsPerCell)) throw new ArgumentOutOfRangeException(nameof(secondsPerCell));
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            double lit = IsInspecting ? 0 : flashlight.Advance(seconds, secondsPerCell);
            fear.Advance(lit, true, rules);
            if (!fear.Advance(seconds - lit, false, rules))
            {
                Inspection?.Meter.Advance(seconds, MeasureSlot);
                return;
            }

            Inspection = null;
            IsDefeated = true;
            Fainted?.Invoke();
        }

        private void Remember(BatteryCell cell)
        {
            if (cell != null && !obtained.Add(cell.Id))
                throw new ArgumentException("Las ranuras iniciales deben tener baterías distintas.");
        }
    }
}
