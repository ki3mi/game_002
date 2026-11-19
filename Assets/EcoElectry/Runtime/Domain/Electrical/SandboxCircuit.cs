using System;
using System.Collections.Generic;
using System.Linq;

namespace EcoElectry.Domain.Electrical
{
    public enum Equipment { Board, Panel, Battery, Lamp }
    public enum Terminal { SolarOut, SolarIn, BatterySocket, StorageIn, LightingOut, LampIn }
    public enum CableFamily { Solar, Interior }
    public enum RoutePoint { WallLeft, CeilingLeft, CeilingRight, WallRight, FloorLeft, FloorRight }
    public enum EndState { Raw, Stripped, Secured }
    public enum Failure { None, OpenCircuit, ShortCircuit, InsufficientEnergy }
    public enum SandboxPreset { Empty, Open, Short, LowEnergy }

    public sealed class Cable
    {
        internal Cable(Terminal from, Terminal to, CableFamily family, IEnumerable<RoutePoint> route)
        {
            From = from;
            To = to;
            Family = family;
            Route = Array.AsReadOnly(route.ToArray());
        }

        public Terminal From { get; }
        public Terminal To { get; }
        public CableFamily Family { get; }
        public IReadOnlyList<RoutePoint> Route { get; }
        public EndState FromState { get; internal set; }
        public EndState ToState { get; internal set; }
        public bool Shorted { get; internal set; }
        public bool Continuous => FromState == EndState.Secured && ToState == EndState.Secured;

        internal Cable Copy()
        {
            return new Cable(From, To, Family, Route)
            {
                FromState = FromState, ToState = ToState, Shorted = Shorted
            };
        }
    }

    public sealed class SandboxCircuit
    {
        private readonly HashSet<Equipment> installed = new HashSet<Equipment> { Equipment.Board };
        private readonly List<Cable> cables = new List<Cable>();

        public IReadOnlyList<Cable> Cables => cables.AsReadOnly();
        public bool SolarActive { get; set; }
        public bool BatteryActive { get; set; }
        public bool BladesClosed { get; set; }
        public bool ProtectionTripped { get; private set; }

        public bool Installed(Equipment equipment) => installed.Contains(equipment);
        public bool Place(Equipment equipment) => installed.Add(equipment);
        public Cable At(Terminal terminal) => cables.Find(cable => cable.From == terminal || cable.To == terminal);
        public void ResetProtection() => ProtectionTripped = false;

        public static Equipment EquipmentFor(Terminal terminal)
        {
            switch (terminal)
            {
                case Terminal.SolarOut: return Equipment.Panel;
                case Terminal.BatterySocket: return Equipment.Battery;
                case Terminal.LampIn: return Equipment.Lamp;
                default: return Equipment.Board;
            }
        }

        public static bool IsOrigin(Terminal terminal) =>
            terminal == Terminal.SolarOut || terminal == Terminal.BatterySocket || terminal == Terminal.LightingOut;

        public string Validate(Terminal from, Terminal to, CableFamily family, IReadOnlyList<RoutePoint> points)
        {
            if (!Installed(EquipmentFor(from)) || !Installed(EquipmentFor(to)))
                return "Primero instala los equipos de ambos bornes.";
            if (At(from) != null || At(to) != null)
                return "Uno de los bornes ya está ocupado.";

            if (from == Terminal.SolarOut && to == Terminal.SolarIn && family == CableFamily.Solar ||
                from == Terminal.BatterySocket && to == Terminal.StorageIn && family == CableFamily.Interior)
                return points.Count == 0 ? null : "Este enlace va directo al tablero.";

            if (from != Terminal.LightingOut || to != Terminal.LampIn || family != CableFamily.Interior)
                return "Los bornes o la familia de cable son incompatibles.";

            bool ceilingRoute = points.SequenceEqual(new[] {
                RoutePoint.WallLeft, RoutePoint.CeilingLeft, RoutePoint.CeilingRight, RoutePoint.WallRight
            });
            bool floorRoute = points.SequenceEqual(new[] {
                RoutePoint.FloorLeft, RoutePoint.FloorRight, RoutePoint.WallRight
            });
            bool mixedRoute = points.SequenceEqual(new[] {
                RoutePoint.FloorLeft, RoutePoint.WallLeft, RoutePoint.CeilingLeft,
                RoutePoint.CeilingRight, RoutePoint.WallRight
            });

            return ceilingRoute || floorRoute || mixedRoute ? null :
                "Sigue los anclajes en orden hasta el muro del aula.";
        }

        public Cable Add(Terminal from, Terminal to, CableFamily family, IReadOnlyList<RoutePoint> points)
        {
            string error = Validate(from, to, family, points);
            if (error != null)
                throw new InvalidOperationException(error);
            var cable = new Cable(from, to, family, points);
            cables.Add(cable);
            return cable;
        }

        public void Remove(Cable cable) => cables.Remove(cable);

        public bool IsLive(Terminal terminal) => EnergizedTerminals().Contains(terminal);

        public bool IsLive(Cable cable)
        {
            var live = EnergizedTerminals();
            return cable.FromState == EndState.Secured && live.Contains(cable.From) ||
                   cable.ToState == EndState.Secured && live.Contains(cable.To);
        }

        private HashSet<Terminal> EnergizedTerminals()
        {
            var live = new HashSet<Terminal>();
            if (Installed(Equipment.Panel) && SolarActive) live.Add(Terminal.SolarOut);
            if (Installed(Equipment.Battery) && BatteryActive) live.Add(Terminal.BatterySocket);

            int previousCount;
            do
            {
                previousCount = live.Count;
                foreach (Cable cable in cables)
                {
                    if (!cable.Continuous) continue;
                    Link(live, cable.From, cable.To);
                }

                if (BladesClosed)
                {
                    Link(live, Terminal.SolarIn, Terminal.StorageIn);
                    if (!ProtectionTripped)
                        Link(live, Terminal.SolarIn, Terminal.LightingOut);
                }
            } while (live.Count != previousCount);

            return live;
        }

        private static void Link(HashSet<Terminal> live, Terminal a, Terminal b)
        {
            if (live.Contains(a) || live.Contains(b))
            {
                live.Add(a);
                live.Add(b);
            }
        }

        public bool Prepare(Cable cable, Terminal terminal)
        {
            if (EndAt(cable, terminal) != EndState.Raw) return false;
            SetEnd(cable, terminal, EndState.Stripped);
            return true;
        }

        public bool Secure(Cable cable, Terminal terminal)
        {
            if (EndAt(cable, terminal) != EndState.Stripped) return false;
            SetEnd(cable, terminal, EndState.Secured);
            return true;
        }

        public static EndState EndAt(Cable cable, Terminal terminal)
        {
            if (cable.From == terminal) return cable.FromState;
            if (cable.To == terminal) return cable.ToState;
            throw new ArgumentException("El borne no pertenece al tramo.", nameof(terminal));
        }

        private static void SetEnd(Cable cable, Terminal terminal, EndState state)
        {
            if (cable.From == terminal) cable.FromState = state;
            else cable.ToState = state;
        }

        public void SetContactFault(Cable cable, bool enabled)
        {
            if (!cables.Contains(cable)) throw new ArgumentException("Tramo ajeno al circuito.", nameof(cable));
            cable.Shorted = enabled;
        }

        public void TripProtection() => ProtectionTripped = true;

        public SandboxCircuit Copy()
        {
            var copy = new SandboxCircuit
            {
                SolarActive = SolarActive,
                BatteryActive = BatteryActive,
                BladesClosed = BladesClosed,
                ProtectionTripped = ProtectionTripped
            };
            copy.installed.UnionWith(installed);
            copy.cables.AddRange(cables.Select(cable => cable.Copy()));
            return copy;
        }
    }
}
