using System;

namespace EcoElectry.Domain.Survival
{
    public sealed class BatteryCell
    {
        public BatteryCell(string id, double charge)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("La batería requiere un identificador.", nameof(id));
            if (double.IsNaN(charge) || charge < 0 || charge > 1) throw new ArgumentOutOfRangeException(nameof(charge));
            Id = id;
            Charge = charge;
        }

        public string Id { get; }
        public double Charge { get; private set; }
        public double Voltage => Math.Round(1.08 + 0.52 * Charge, 2, MidpointRounding.AwayFromZero);

        internal void Discharge(double amount)
        {
            Charge = Math.Max(0, Charge - amount);
        }
    }

    public sealed class FlashlightState
    {
        public const int SlotCount = 2;
        private const double TimeTolerance = 1e-9;
        private readonly BatteryCell[] slots = new BatteryCell[SlotCount];

        public FlashlightState(BatteryCell first, BatteryCell second, bool requestedOn)
        {
            slots[0] = first;
            slots[1] = second;
            RequestedOn = requestedOn;
            SelectChargedSlot();
        }

        public bool RequestedOn { get; private set; }
        public int ActiveSlot { get; private set; } = -1;
        public bool HasBeam => RequestedOn && ActiveSlot >= 0;
        public double TotalCharge => (slots[0]?.Charge ?? 0) + (slots[1]?.Charge ?? 0);
        public BatteryCell GetSlot(int index) => slots[index];

        public void Toggle() => RequestedOn = !RequestedOn;

        public void SetSlots(BatteryCell first, BatteryCell second)
        {
            BatteryCell active = ActiveSlot >= 0 ? slots[ActiveSlot] : null;
            slots[0] = first;
            slots[1] = second;
            ActiveSlot = active == null ? -1 : Array.IndexOf(slots, active);
            SelectChargedSlot();
        }

        // Return the lit portion of the frame so fear can account for the moment the last cell runs out.
        public double Advance(double seconds, double secondsPerCell)
        {
            double remaining = seconds;
            while (HasBeam && remaining > 0)
            {
                BatteryCell cell = slots[ActiveSlot];
                double available = cell.Charge * secondsPerCell;
                if (available - remaining > TimeTolerance)
                {
                    cell.Discharge(remaining / secondsPerCell);
                    SelectChargedSlot();
                    return seconds;
                }

                cell.Discharge(cell.Charge);
                remaining = Math.Max(0, remaining - available);
                SelectChargedSlot();
            }
            return seconds - remaining;
        }

        private void SelectChargedSlot()
        {
            if (ActiveSlot >= 0 && slots[ActiveSlot]?.Charge > 0) return;
            ActiveSlot = slots[0]?.Charge > 0 ? 0 : slots[1]?.Charge > 0 ? 1 : -1;
        }
    }
}
