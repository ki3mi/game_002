using System;
using System.Collections.Generic;

namespace EcoElectry.Application.Survival
{
    public sealed class FuseCartridge
    {
        public FuseCartridge(string id, string reference, bool conducts)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Cada fusible necesita una identidad y una referencia.");
            Id = id;
            Reference = reference;
            Conducts = conducts;
        }

        public string Id { get; }
        public string Reference { get; }
        internal bool Conducts { get; }
    }

    public enum FusePosition { SlotA, SlotB, SlotC, Bench, Tray, Inventory, World }
    public enum FuseMountResult { Blocked, Incompatible, Unverified, Open, Mounted }

    public sealed class FuseRepair
    {
        public const int SlotCount = 3;
        public const int ScrewCount = 4;

        private readonly string[] required;
        private readonly FusePosition[] positions = { FusePosition.SlotA, FusePosition.SlotB, FusePosition.SlotC, FusePosition.World };
        private readonly bool[] fastened = { true, true, true, true };
        private readonly Dictionary<string, bool> readings = new(StringComparer.Ordinal);
        private readonly SurvivalSession session;

        internal FuseRepair(SurvivalSession session, string[] required, FuseCartridge[] cartridges)
        {
            if (required.Length != SlotCount || cartridges.Length != positions.Length)
                throw new ArgumentException("La caja requiere tres ranuras y un repuesto.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (FuseCartridge fuse in cartridges)
                if (!ids.Add(fuse.Id)) throw new ArgumentException("Los fusibles deben tener identidades distintas.");
            this.session = session;
            this.required = (string[])required.Clone();
            Cartridges = Array.AsReadOnly((FuseCartridge[])cartridges.Clone());
            Meter = new FuseMeter(this);
        }

        public bool SupplyCut => session.SupplyCut;
        public IReadOnlyList<FuseCartridge> Cartridges { get; }
        public FuseCartridge Spare => Cartridges[SlotCount];
        public FusePosition Position(int index) => positions[index];
        public int At(FusePosition position) => Array.IndexOf(positions, position);
        public string RequiredReference(int slot) => required[slot];
        public bool CoverOn { get; private set; } = true;
        public bool IsFastened(int screw) => fastened[screw];
        public bool Completed { get; private set; }
        public bool ClueRead { get; private set; }
        public FuseMeter Meter { get; }

        public bool TryGetReading(int index, out bool conducts) => readings.TryGetValue(Cartridges[index].Id, out conducts);

        public int DiagnosedOpen
        {
            get
            {
                for (int i = 0; i < SlotCount; i++)
                    if (TryGetReading(i, out bool conducts) && !conducts) return i;
                return -1;
            }
        }

        public bool ReadyToClose
        {
            get
            {
                for (int i = 0; i < SlotCount; i++)
                {
                    int index = At((FusePosition)i);
                    if (index < 0 || Cartridges[index].Reference != required[i] || !TryGetReading(index, out bool conducts) || !conducts)
                        return false;
                }
                return true;
            }
        }

        public bool TryTurnScrew(int index)
        {
            if ((uint)index >= ScrewCount || !SupplyCut || Completed || !CoverOn || (!fastened[index] && !ReadyToClose))
                return false;
            fastened[index] = !fastened[index];
            Completed = fastened[index] && ReadyToClose && Array.TrueForAll(fastened, value => value);
            return true;
        }

        public bool TryMoveCover()
        {
            if (!SupplyCut || Completed || Array.Exists(fastened, value => value) || (!CoverOn && !ReadyToClose))
                return false;
            CoverOn = !CoverOn;
            return true;
        }

        public bool TryMoveToBench(int index)
        {
            if ((uint)index >= positions.Length || !SupplyCut || Completed || CoverOn ||
                positions[index] is FusePosition.Bench or FusePosition.World)
                return false;
            int previous = At(FusePosition.Bench);
            if (previous >= 0) positions[previous] = FusePosition.Tray;
            Meter.StoreProbes();
            positions[index] = FusePosition.Bench;
            return true;
        }

        public FuseMountResult TryMount(int slot)
        {
            int index = At(FusePosition.Bench);
            if ((uint)slot >= SlotCount || !SupplyCut || Completed || CoverOn || index < 0 || At((FusePosition)slot) >= 0)
                return FuseMountResult.Blocked;
            if (Cartridges[index].Reference != required[slot]) return FuseMountResult.Incompatible;
            if (!TryGetReading(index, out bool conducts)) return FuseMountResult.Unverified;
            if (!conducts) return FuseMountResult.Open;
            Meter.StoreProbes();
            positions[index] = (FusePosition)slot;
            return FuseMountResult.Mounted;
        }

        public bool ReadClue()
        {
            if (DiagnosedOpen < 0) return false;
            ClueRead = true;
            return true;
        }

        public bool TryCollectSpare()
        {
            if (!ClueRead || positions[SlotCount] != FusePosition.World) return false;
            positions[SlotCount] = FusePosition.Inventory;
            return true;
        }

        internal void Record(int index, bool conducts) => readings[Cartridges[index].Id] = conducts;
    }

    public readonly struct FuseContact
    {
        public FuseContact(int cartridge, int terminal)
        {
            Cartridge = cartridge;
            Terminal = terminal;
        }

        public int Cartridge { get; }
        public int Terminal { get; }
    }

    public sealed class FuseMeter : IMeterReadout
    {
        private readonly FuseRepair repair;
        private readonly FuseContact?[] contacts = new FuseContact?[2];

        internal FuseMeter(FuseRepair repair) => this.repair = repair;
        public MeterMode Mode { get; private set; }
        public MeterDisplay Display { get; private set; }
        public double Reading => 0;
        public double Progress => 0;
        public event Action ContinuityReached;

        public FuseContact? GetContact(MeterProbe probe) => contacts[(int)probe];

        public void SetMode(MeterMode mode)
        {
            if (Mode == mode) return;
            Mode = mode;
            Recheck();
        }

        public bool TryContact(MeterProbe probe, int cartridge, int terminal)
        {
            if ((uint)cartridge >= repair.Cartridges.Count || (uint)terminal > 1 ||
                !repair.SupplyCut || repair.CoverOn || repair.Position(cartridge) != FusePosition.Bench)
                return false;
            contacts[(int)probe] = new FuseContact(cartridge, terminal);
            Recheck();
            return true;
        }

        public void Lift(MeterProbe probe)
        {
            contacts[(int)probe] = null;
            Recheck();
        }

        public void StoreProbes()
        {
            Array.Clear(contacts, 0, contacts.Length);
            Recheck();
        }

        private void Recheck()
        {
            MeterDisplay previous = Display;
            var black = contacts[0];
            var red = contacts[1];
            if (Mode == MeterMode.Off) Display = MeterDisplay.Off;
            else if (Mode != MeterMode.Continuity) Display = MeterDisplay.WrongMode;
            else if (!repair.SupplyCut || !black.HasValue || !red.HasValue || black.Value.Cartridge != red.Value.Cartridge)
                Display = MeterDisplay.NoContact;
            else if (black.Value.Terminal == red.Value.Terminal) Display = MeterDisplay.SameTerminal;
            else
            {
                int index = black.Value.Cartridge;
                bool conducts = repair.Cartridges[index].Conducts;
                repair.Record(index, conducts);
                Display = conducts ? MeterDisplay.Continuity : MeterDisplay.OpenCircuit;
                if (conducts && previous != Display) ContinuityReached?.Invoke();
            }
        }
    }
}
