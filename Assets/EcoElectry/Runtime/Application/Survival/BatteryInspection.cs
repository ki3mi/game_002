using System;
using System.Collections.Generic;
using EcoElectry.Domain.Survival;

namespace EcoElectry.Application.Survival
{
    public sealed class BatteryInspection
    {
        private readonly BatteryCell[] slots;
        private readonly BatteryCell[] tray;
        private readonly Dictionary<string, int> trayHomes = new(StringComparer.Ordinal);

        internal BatteryInspection(BatteryCell first, BatteryCell second, IReadOnlyList<BatteryCell> inventory)
        {
            slots = new[] { first, second };
            tray = new BatteryCell[inventory.Count + FlashlightState.SlotCount];
            for (int i = 0; i < inventory.Count; i++)
            {
                tray[i] = inventory[i];
                trayHomes.Add(tray[i].Id, i);
            }
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) trayHomes.Add(slots[i].Id, inventory.Count + i);
            Slots = Array.AsReadOnly(slots);
            Tray = Array.AsReadOnly(tray);
            ReserveCount = inventory.Count;
            CanMeasure = first != null && second != null;
            Meter = new BatteryMeter(this);
        }

        public IReadOnlyList<BatteryCell> Slots { get; }
        public IReadOnlyList<BatteryCell> Tray { get; }
        public int ReserveCount { get; }
        public bool CanMeasure { get; private set; }
        public BatteryMeter Meter { get; }

        public int FindInstalled(BatteryCell cell) => cell == null ? -1 : Array.IndexOf(slots, cell);
        public int FindOnTray(BatteryCell cell) => cell == null ? -1 : Array.IndexOf(tray, cell);

        public bool TryExtract(int slot, out int home)
        {
            home = -1;
            if ((uint)slot >= slots.Length || slots[slot] == null) return false;
            BatteryCell cell = slots[slot];
            home = trayHomes[cell.Id];
            tray[home] = cell;
            slots[slot] = null;
            Meter.ReleaseCell(cell);
            return true;
        }

        public bool TryInsert(int trayIndex, int slot)
        {
            if ((uint)trayIndex >= tray.Length || tray[trayIndex] == null ||
                (uint)slot >= slots.Length || slots[slot] != null) return false;

            slots[slot] = tray[trayIndex];
            tray[trayIndex] = null;
            CanMeasure |= slots[0] != null && slots[1] != null;
            return true;
        }
    }
}
