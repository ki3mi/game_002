using System;
using System.Linq;

namespace EcoElectry.Domain.Electrical
{
    public sealed class SandboxCase
    {
        public SandboxCase(string id, double generation, double capacity, double demand, SandboxPreset preset)
        {
            if (string.IsNullOrWhiteSpace(id) || !Valid(generation) || !Valid(capacity) || !Valid(demand) || demand == 0)
                throw new ArgumentException("Datos de simulación inválidos.");

            Id = id;
            Generation = generation;
            Capacity = capacity;
            Demand = demand;
            Preset = preset;
        }

        public string Id { get; }
        public double Generation { get; }
        public double Capacity { get; }
        public double Demand { get; }
        public SandboxPreset Preset { get; }

        private static bool Valid(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }

    public sealed class SandboxReport
    {
        internal SandboxReport(Failure failure, Terminal? affectedTerminal, int litMinutes, double reserve, string service)
        {
            Failure = failure;
            AffectedTerminal = affectedTerminal;
            LitMinutes = litMinutes;
            Reserve = reserve;
            Service = service;
        }

        public Failure Failure { get; }
        public bool Passed => Failure == Failure.None;
        public Terminal? AffectedTerminal { get; }
        public int LitMinutes { get; }
        public int InterruptionMinute => 18 * 60 + LitMinutes;
        public double Reserve { get; }
        public string Service { get; }
    }

    public static class SandboxSimulation
    {
        public static SandboxReport Run(SandboxCircuit design, SandboxCase scenario)
        {
            SandboxCircuit circuit = design.Copy();
            double reserve = 0;
            Cable shorted = circuit.Cables.FirstOrDefault(cable => cable.Shorted && circuit.IsLive(cable));
            if (shorted != null)
                return new SandboxReport(Failure.ShortCircuit, shorted.From, 0, reserve, "Protección del tablero");

            if (circuit.ProtectionTripped)
                return new SandboxReport(Failure.OpenCircuit, Terminal.LightingOut, 0, reserve, "Protección abierta");

            if (!circuit.BladesClosed)
                return new SandboxReport(Failure.OpenCircuit, null, 0, reserve, "Cuchillas abiertas");

            Cable lamp = circuit.At(Terminal.LightingOut);
            if (lamp == null || !lamp.Continuous)
                return new SandboxReport(Failure.OpenCircuit, Terminal.LightingOut, 0, reserve, "Alumbrado del aula");

            Cable panel = circuit.At(Terminal.SolarOut);
            Cable battery = circuit.At(Terminal.BatterySocket);
            if (circuit.Installed(Equipment.Panel) && (panel == null || !panel.Continuous))
                return new SandboxReport(Failure.OpenCircuit, Terminal.SolarOut, 0, reserve, "Entrada fotovoltaica");
            if (circuit.Installed(Equipment.Battery) && (battery == null || !battery.Continuous))
                return new SandboxReport(Failure.OpenCircuit, Terminal.BatterySocket, 0, reserve, "Almacenamiento");

            // Two daylight intervals with no demand, then two one-hour class intervals without generation.
            if (panel != null && battery != null && circuit.SolarActive && circuit.BatteryActive)
            {
                for (int interval = 0; interval < 2; interval++)
                    reserve = Math.Min(scenario.Capacity, reserve + scenario.Generation / 2);
            }

            double hourlyDemand = scenario.Demand / 2;
            for (int interval = 0; interval < 2; interval++)
            {
                if (reserve < hourlyDemand)
                {
                    int minutes = Math.Min(59, (int)Math.Floor(reserve / hourlyDemand * 60));
                    return new SandboxReport(Failure.InsufficientEnergy, null, interval * 60 + minutes, 0, "Alumbrado del aula");
                }
                reserve -= hourlyDemand;
            }

            return new SandboxReport(Failure.None, null, 120, reserve, "Alumbrado del aula");
        }
    }
}
