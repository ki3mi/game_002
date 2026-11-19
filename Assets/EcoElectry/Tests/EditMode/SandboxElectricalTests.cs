using EcoElectry.Application.Electrical;
using EcoElectry.Domain.Electrical;
using NUnit.Framework;

namespace EcoElectry.Tests.EditMode
{
    public sealed class SandboxElectricalTests
    {
        private static SandboxSession Load(SandboxPreset preset, double generation = 6)
        {
            var session = new SandboxSession();
            session.Load(new SandboxCase("test", generation, 6, 4, preset));
            return session;
        }

        [Test]
        public void ValidatesFamilyOccupiedTerminalsAndOrderedTransitions()
        {
            SandboxSession session = Load(SandboxPreset.Empty);
            foreach (Equipment equipment in new[] { Equipment.Panel, Equipment.Battery, Equipment.Lamp })
                session.Place(equipment);

            Assert.That(session.StartRoute(Terminal.SolarOut, CableFamily.Interior), Is.True);
            session.ChooseDestination(Terminal.SolarIn);
            Assert.That(session.ConfirmRoute(), Is.False);
            session.CancelRoute();
            Assert.That(session.Circuit.Cables, Is.Empty);

            session.StartRoute(Terminal.SolarOut, CableFamily.Solar);
            session.ChooseDestination(Terminal.SolarIn);
            Assert.That(session.ConfirmRoute(), Is.True);
            Assert.That(session.StartRoute(Terminal.SolarOut, CableFamily.Solar), Is.False);
            Assert.That(session.Circuit.At(Terminal.SolarOut).Continuous, Is.False);

            session.StartRoute(Terminal.LightingOut, CableFamily.Interior);
            session.AddPoint(RoutePoint.WallRight);
            session.ChooseDestination(Terminal.LampIn);
            Assert.That(session.ConfirmRoute(), Is.False);
            session.UndoPoint();
            session.AddPoint(RoutePoint.FloorLeft);
            session.AddPoint(RoutePoint.FloorRight);
            session.AddPoint(RoutePoint.WallRight);
            Assert.That(session.ConfirmRoute(), Is.True);
            Assert.That(session.Use(CableTool.Remove, Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            Assert.That(session.Circuit.At(Terminal.LightingOut), Is.Null);
        }

        [Test]
        public void MeasuresOpenCircuitAndPassesAfterSecuringTheEnd()
        {
            SandboxSession session = Load(SandboxPreset.Open);
            Assert.That(session.Test().Failure, Is.EqualTo(Failure.OpenCircuit));
            Assert.That(session.Report.AffectedTerminal, Is.EqualTo(Terminal.LightingOut));
            session.CloseReport();

            Assert.That(session.Use(CableTool.Measure, Terminal.LampIn), Is.EqualTo(InteractionResult.Rejected));
            Isolate(session);
            Assert.That(session.Use(CableTool.Measure, Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            Assert.That(session.LastMeasurement, Does.Contain("sin continuidad"));
            Assert.That(session.Use(CableTool.Secure, Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            Assert.That(session.Use(CableTool.Measure, Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            Assert.That(session.LastMeasurement, Is.EqualTo("Continuidad confirmada."));
            Energize(session);
            Assert.That(session.Test().Passed, Is.True);
        }

        [Test]
        public void TripsOnlyOnElectricalFaultAndRestoresProtectionAfterRepair()
        {
            SandboxSession session = Load(SandboxPreset.Short);
            Assert.That(session.Test().Failure, Is.EqualTo(Failure.ShortCircuit));
            Assert.That(session.Circuit.ProtectionTripped, Is.True);
            Assert.That(session.Circuit.IsLive(Terminal.LightingOut), Is.False);
            Assert.That(session.Circuit.IsLive(Terminal.StorageIn), Is.True);
            session.CloseReport();

            Isolate(session);
            Assert.That(session.Use(CableTool.Repair, Terminal.LightingOut), Is.EqualTo(InteractionResult.Applied));
            session.ResetProtection();
            Energize(session);
            Assert.That(session.Test().Passed, Is.True);
            Assert.That(session.Circuit.ProtectionTripped, Is.False);
        }

        [Test]
        public void ReportsDeterministicDepletionAndSupportsReducingDemand()
        {
            SandboxSession session = Load(SandboxPreset.LowEnergy, 2);
            SandboxReport first = session.Test();
            SandboxReport second = SandboxSimulation.Run(session.Circuit, session.Scenario);
            Assert.That(first.Failure, Is.EqualTo(Failure.InsufficientEnergy));
            Assert.That(first.InterruptionMinute, Is.EqualTo(19 * 60));
            Assert.That(second.InterruptionMinute, Is.EqualTo(first.InterruptionMinute));
            Assert.That(second.Reserve, Is.Zero);
            session.CloseReport();

            session.SelectEfficientLamp(true);
            Assert.That(session.Test().Passed, Is.True);
            Assert.That(session.Circuit.ProtectionTripped, Is.False);
        }

        [Test]
        public void BatteryBackfeedRestoresIndependentCheckpointBeforeApplyingChanges()
        {
            SandboxSession session = Load(SandboxPreset.Open);
            session.Test();
            session.CloseReport();
            string task = session.Task;
            session.SetSolar(false);
            Assert.That(session.Circuit.IsLive(Terminal.LightingOut), Is.True);
            Assert.That(session.Circuit.IsLive(Terminal.LampIn), Is.False);

            session.SelectEfficientLamp(true);
            Assert.That(session.Use(CableTool.Secure, Terminal.LampIn), Is.EqualTo(InteractionResult.Shock));
            Assert.That(session.Circuit.At(Terminal.LampIn).ToState, Is.EqualTo(EndState.Stripped));
            Assert.That(session.Circuit.SolarActive, Is.True);
            Assert.That(session.Circuit.BatteryActive, Is.True);
            Assert.That(session.EfficientLamp, Is.False);
            Assert.That(session.Task, Is.EqualTo(task));
            Assert.That(session.SpawnId, Is.EqualTo(SandboxSession.BoardSpawn));

            Isolate(session);
            Assert.That(session.Use(CableTool.Secure, Terminal.LampIn), Is.EqualTo(InteractionResult.Applied));
            session.Reset();
            Assert.That(session.Circuit.At(Terminal.LampIn).Continuous, Is.False);
            Assert.That(session.Report, Is.Null);
            Assert.That(session.LastMeasurement, Is.Null);
        }

        private static void Isolate(SandboxSession session)
        {
            session.SetBlades(false);
            session.SetSolar(false);
            session.SetBattery(false);
        }

        private static void Energize(SandboxSession session)
        {
            session.SetSolar(true);
            session.SetBattery(true);
            session.SetBlades(true);
        }
    }
}
