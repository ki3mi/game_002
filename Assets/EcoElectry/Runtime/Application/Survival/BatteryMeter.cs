using System;
using System.Collections.Generic;
using EcoElectry.Domain.Survival;

namespace EcoElectry.Application.Survival
{
    public enum MeterMode { Off, DirectVoltage, Continuity }
    public enum MeterProbe { Black, Red }
    public enum BatteryTerminal { Negative, Positive }
    public enum MeterDisplay { Off, NoContact, WrongMode, Stabilizing, Voltage, ReversePolarity, SameTerminal }

    public readonly struct ProbeContact
    {
        public ProbeContact(BatteryCell cell, BatteryTerminal terminal)
        {
            Cell = cell;
            Terminal = terminal;
        }

        public BatteryCell Cell { get; }
        public BatteryTerminal Terminal { get; }
    }

    public sealed class BatteryMeter
    {
        public const double StabilizationSeconds = 1.5;
        private readonly BatteryInspection inspection;
        private readonly ProbeContact[] contacts = new ProbeContact[2];
        private readonly Dictionary<string, double> readings = new(StringComparer.Ordinal);

        internal BatteryMeter(BatteryInspection inspection) => this.inspection = inspection;
        public MeterMode Mode { get; private set; }
        public MeterDisplay Display { get; private set; }
        public double Remaining { get; private set; }
        public double DisplayVoltage { get; private set; }

        public ProbeContact GetContact(MeterProbe probe) => contacts[(int)probe];

        public bool TryGetReading(BatteryCell cell, out double volts) => readings.TryGetValue(cell.Id, out volts);

        public void SetMode(MeterMode mode)
        {
            if (Mode == mode) return;
            Mode = mode;
            RecheckContacts();
        }

        public bool TryContact(MeterProbe probe, BatteryCell cell, BatteryTerminal terminal)
        {
            if (inspection.FindInstalled(cell) < 0) return false;
            contacts[(int)probe] = new ProbeContact(cell, terminal);
            RecheckContacts();
            return true;
        }

        public void Lift(MeterProbe probe)
        {
            if (contacts[(int)probe].Cell == null) return;
            contacts[(int)probe] = default;
            RecheckContacts();
        }

        public void StoreProbes()
        {
            Array.Clear(contacts, 0, contacts.Length);
            RecheckContacts();
        }

        internal void ReleaseCell(BatteryCell cell)
        {
            if (contacts[0].Cell == cell) Lift(MeterProbe.Black);
            if (contacts[1].Cell == cell) Lift(MeterProbe.Red);
        }

        internal void Advance(double seconds, Func<int, double?> measureSlot)
        {
            if (Display != MeterDisplay.Stabilizing) return;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining > 1e-9) return;

            BatteryCell cell = contacts[0].Cell;
            int slot = inspection.FindInstalled(cell);
            double? voltage = slot < 0 ? null : measureSlot(slot);
            if (!voltage.HasValue)
            {
                Display = MeterDisplay.NoContact;
                return;
            }
            bool correctlyWired = contacts[0].Terminal == BatteryTerminal.Negative;
            DisplayVoltage = correctlyWired ? voltage.Value : -voltage.Value;
            Display = correctlyWired ? MeterDisplay.Voltage : MeterDisplay.ReversePolarity;
            if (correctlyWired) readings[cell.Id] = voltage.Value;
        }

        private void RecheckContacts()
        {
            Remaining = 0;
            DisplayVoltage = 0;
            ProbeContact black = contacts[0];
            ProbeContact red = contacts[1];
            if (Mode == MeterMode.Off) Display = MeterDisplay.Off;
            else if (Mode == MeterMode.Continuity && (black.Cell != null || red.Cell != null))
                Display = MeterDisplay.WrongMode;
            else if (Mode != MeterMode.DirectVoltage || black.Cell == null || black.Cell != red.Cell)
                Display = MeterDisplay.NoContact;
            else if (black.Terminal == red.Terminal) Display = MeterDisplay.SameTerminal;
            else if (inspection.CanMeasure)
            {
                Remaining = StabilizationSeconds;
                Display = MeterDisplay.Stabilizing;
            }
            else Display = MeterDisplay.NoContact;
        }
    }
}
