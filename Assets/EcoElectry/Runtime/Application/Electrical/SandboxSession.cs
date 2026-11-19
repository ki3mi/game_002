using System;
using System.Collections.Generic;
using System.Linq;
using EcoElectry.Domain.Electrical;

namespace EcoElectry.Application.Electrical
{
    public enum CableTool { Strip, Secure, Measure, Repair, Remove }
    public enum InteractionResult { Applied, Rejected, Shock }

    public sealed class CableDraft
    {
        private readonly List<RoutePoint> points = new List<RoutePoint>();

        internal CableDraft(Terminal from, CableFamily family)
        {
            From = from;
            Family = family;
        }

        public Terminal From { get; }
        public CableFamily Family { get; }
        public Terminal? To { get; internal set; }
        public IReadOnlyList<RoutePoint> Points => points.AsReadOnly();

        internal void Add(RoutePoint point) => points.Add(point);
        internal void Undo() { if (points.Count > 0) points.RemoveAt(points.Count - 1); }
    }

    public sealed class SandboxSession
    {
        private sealed class Checkpoint
        {
            public SandboxCircuit Circuit;
            public bool EfficientLamp;
            public string Task;
            public string SpawnId;
        }

        public const string YardSpawn = "yard";
        public const string BoardSpawn = "board";

        private Checkpoint checkpoint;
        public SandboxCase Scenario { get; private set; }
        public SandboxCircuit Circuit { get; private set; }
        public CableDraft Draft { get; private set; }
        public SandboxReport Report { get; private set; }
        public string LastMeasurement { get; private set; }
        public string Message { get; private set; }
        public string Task { get; private set; }
        public string SpawnId { get; private set; }
        public bool EfficientLamp { get; private set; }
        public bool ReportOpen { get; private set; }

        public void Load(SandboxCase scenario)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            Circuit = new SandboxCircuit();
            Draft = null;
            Report = null;
            LastMeasurement = null;
            ReportOpen = false;
            EfficientLamp = false;
            Message = null;
            Task = "Instala los equipos en sus anclajes.";
            SpawnId = YardSpawn;

            if (scenario.Preset != SandboxPreset.Empty)
            {
                Circuit.Place(Equipment.Panel);
                Circuit.Place(Equipment.Battery);
                Circuit.Place(Equipment.Lamp);
                Seed(Terminal.SolarOut, Terminal.SolarIn, CableFamily.Solar, Array.Empty<RoutePoint>());
                Seed(Terminal.BatterySocket, Terminal.StorageIn, CableFamily.Interior, Array.Empty<RoutePoint>());
                Cable lighting = Seed(Terminal.LightingOut, Terminal.LampIn, CableFamily.Interior, new[]
                {
                    RoutePoint.FloorLeft, RoutePoint.WallLeft, RoutePoint.CeilingLeft,
                    RoutePoint.CeilingRight, RoutePoint.WallRight
                }, scenario.Preset != SandboxPreset.Open);
                Circuit.SetContactFault(lighting, scenario.Preset == SandboxPreset.Short);
                Circuit.BladesClosed = Circuit.SolarActive = Circuit.BatteryActive = true;
                Task = "Prueba el montaje para diagnosticar el caso.";
            }

            SaveCheckpoint();
        }

        private Cable Seed(Terminal from, Terminal to, CableFamily family, RoutePoint[] points, bool closed = true)
        {
            Cable cable = Circuit.Add(from, to, family, points);
            Circuit.Prepare(cable, from);
            Circuit.Secure(cable, from);
            Circuit.Prepare(cable, to);
            if (closed) Circuit.Secure(cable, to);
            return cable;
        }

        public void Reset() => Load(Scenario);

        public void Place(Equipment equipment)
        {
            if (!Circuit.Place(equipment)) return;
            Task = Circuit.Installed(Equipment.Panel) && Circuit.Installed(Equipment.Battery) &&
                Circuit.Installed(Equipment.Lamp)
                ? "Traza los tres enlaces de la instalación." : "Instala los equipos restantes.";
        }

        public void SelectEfficientLamp(bool enabled)
        {
            EfficientLamp = enabled;
            Message = enabled ? "Luminaria eficiente seleccionada." : "Luminaria estándar seleccionada.";
        }

        public void SetSolar(bool enabled) => Circuit.SolarActive = enabled;
        public void SetBattery(bool enabled) => Circuit.BatteryActive = enabled;
        public void SetBlades(bool closed) => Circuit.BladesClosed = closed;

        public void ResetProtection()
        {
            if (Circuit.BladesClosed || Circuit.SolarActive || Circuit.BatteryActive)
                Message = "Abre las cuchillas y aísla ambas fuentes para rearmar.";
            else
            {
                Circuit.ResetProtection();
                Message = "Protección rearmada.";
            }
        }

        public bool StartRoute(Terminal from, CableFamily family)
        {
            if (!SandboxCircuit.IsOrigin(from) || Circuit.At(from) != null ||
                !Circuit.Installed(SandboxCircuit.EquipmentFor(from)))
            {
                Message = "Elige un borne de origen libre en un equipo instalado.";
                return false;
            }

            Draft = new CableDraft(from, family);
            Message = "Elige los anclajes y el borne de destino.";
            return true;
        }

        public void AddPoint(RoutePoint point)
        {
            if (Draft == null || Draft.From != Terminal.LightingOut) return;
            if (Draft.Points.Contains(point))
            {
                Message = "Ese anclaje ya forma parte del tramo.";
                return;
            }

            Draft.Add(point);
        }

        public void UndoPoint() => Draft?.Undo();
        public void ChooseDestination(Terminal terminal) { if (Draft != null) Draft.To = terminal; }

        public string DraftError => Draft?.To == null ? "Selecciona el borne de destino." :
            Circuit.Validate(Draft.From, Draft.To.Value, Draft.Family, Draft.Points);

        public bool ConfirmRoute()
        {
            string error = DraftError;
            if (error != null)
            {
                Message = error;
                return false;
            }

            Circuit.Add(Draft.From, Draft.To.Value, Draft.Family, Draft.Points);
            Draft = null;
            Message = "Tramo trazado. Prepara y fija ambos extremos.";
            Task = "Prepara, fija y verifica los extremos.";
            return true;
        }

        public void CancelRoute() => Draft = null;

        public InteractionResult Use(CableTool tool, Terminal terminal)
        {
            Cable cable = Circuit.At(terminal);
            if (tool == CableTool.Measure)
            {
                if (Circuit.IsLive(terminal) || cable != null && (Circuit.IsLive(cable) ||
                    Circuit.IsLive(cable.From) || Circuit.IsLive(cable.To)))
                    return Reject("Aísla el tramo antes de medir continuidad.");

                LastMeasurement = cable == null || !cable.Continuous
                    ? "Circuito abierto · sin continuidad."
                    : cable.Shorted ? "Continuidad · revisa el contacto indebido." : "Continuidad confirmada.";
                Message = LastMeasurement;
                if (cable != null && cable.Continuous && !cable.Shorted)
                    Task = "Pon el tablero en servicio y pulsa Probar.";
                return InteractionResult.Applied;
            }

            if (Circuit.IsLive(terminal) || cable != null && Circuit.IsLive(cable))
            {
                RestoreAfterShock();
                return InteractionResult.Shock;
            }

            if (cable == null) return Reject("Este borne no tiene un tramo.");

            switch (tool)
            {
                case CableTool.Strip:
                    if (!Circuit.Prepare(cable, terminal)) return Reject("Este extremo ya está preparado.");
                    Message = "Extremo preparado. Usa el destornillador.";
                    break;
                case CableTool.Secure:
                    if (!Circuit.Secure(cable, terminal)) return Reject("Prepara primero el extremo suelto.");
                    Message = "Extremo fijado al borne.";
                    break;
                case CableTool.Repair:
                    if (!cable.Shorted) return Reject("El tramo no tiene un contacto indebido.");
                    Circuit.SetContactFault(cable, false);
                    Message = "Contacto corregido. Verifica continuidad y rearma la protección.";
                    Task = Message;
                    break;
                case CableTool.Remove:
                    Circuit.Remove(cable);
                    LastMeasurement = null;
                    Message = "Tramo retirado y bornes liberados.";
                    Task = "Revisa los enlaces de la instalación.";
                    break;
            }

            return InteractionResult.Applied;
        }

        private InteractionResult Reject(string message)
        {
            Message = message;
            return InteractionResult.Rejected;
        }

        public SandboxReport Test()
        {
            Draft = null;
            var testCase = new SandboxCase(Scenario.Id, Scenario.Generation, Scenario.Capacity,
                EfficientLamp ? Scenario.Demand / 2 : Scenario.Demand, Scenario.Preset);
            Report = SandboxSimulation.Run(Circuit, testCase);
            if (Report.Failure == Failure.ShortCircuit) Circuit.TripProtection();
            ReportOpen = true;
            Message = null;
            return Report;
        }

        public void CloseReport()
        {
            if (!ReportOpen) return;
            ReportOpen = false;
            switch (Report.Failure)
            {
                case Failure.None: Task = "Prueba superada. La clase recibió luz continua."; return;
                case Failure.OpenCircuit: Task = "Aísla las fuentes y verifica la conexión abierta."; break;
                case Failure.ShortCircuit: Task = "Aísla las fuentes, corrige el contacto y rearma."; break;
                case Failure.InsufficientEnergy: Task = "Reduce la demanda con la luminaria eficiente."; break;
            }
            SpawnId = BoardSpawn;
            SaveCheckpoint();
        }

        private void SaveCheckpoint()
        {
            checkpoint = new Checkpoint
            {
                Circuit = Circuit.Copy(), EfficientLamp = EfficientLamp, Task = Task, SpawnId = SpawnId
            };
        }

        private void RestoreAfterShock()
        {
            Circuit = checkpoint.Circuit.Copy();
            Task = checkpoint.Task;
            EfficientLamp = checkpoint.EfficientLamp;
            SpawnId = checkpoint.SpawnId;
            Draft = null;
            ReportOpen = false;
            Report = null;
            LastMeasurement = null;
            Message = "Descarga simulada: el tramo estaba bajo tensión. Punto de control restaurado.";
        }
    }
}
