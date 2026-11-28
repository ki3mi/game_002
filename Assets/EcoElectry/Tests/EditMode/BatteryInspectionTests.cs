using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using NUnit.Framework;

namespace EcoElectry.Tests.EditMode
{
    public sealed class BatteryInspectionTests
    {
        private static readonly FearRules Fear = new(12, 6, 3, 5);

        [Test]
        public void ValidContactsMeasureIndependentlyAndKeepBothReadings()
        {
            var session = OpenInspection();
            var meter = session.Inspection.Meter;
            var first = session.GetSlot(0);
            var second = session.GetSlot(1);
            meter.SetMode(MeterMode.DirectVoltage);
            meter.TryContact(MeterProbe.Black, first, BatteryTerminal.Negative);
            session.Advance(3, 30, Fear);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.NoContact));
            meter.TryContact(MeterProbe.Red, first, BatteryTerminal.Positive);
            session.Advance(1.4, 30, Fear);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.Stabilizing));
            session.Advance(.1, 30, Fear);
            Assert.That(meter.DisplayVoltage, Is.EqualTo(1.47).Within(1e-9));
            Assert.That(meter.TryGetReading(first, out double firstVolts), Is.True);

            meter.TryContact(MeterProbe.Black, second, BatteryTerminal.Negative);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.NoContact));
            meter.TryContact(MeterProbe.Red, second, BatteryTerminal.Positive);
            session.Advance(1.5, 30, Fear);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.Voltage));
            Assert.That(meter.TryGetReading(second, out double secondVolts), Is.True);
            Assert.That(secondVolts, Is.EqualTo(1.18).Within(1e-9));
            Assert.That(firstVolts, Is.EqualTo(1.47).Within(1e-9));
            Assert.That(first.Charge, Is.EqualTo(.75));
            Assert.That(second.Charge, Is.EqualTo(.20));
        }

        [Test]
        public void ContinuityReversePolarityAndSameTerminalDoNotSaveAComparison()
        {
            var session = OpenInspection();
            BatteryCell cell = session.GetSlot(0);
            var meter = session.Inspection.Meter;
            meter.SetMode(MeterMode.Continuity);
            meter.TryContact(MeterProbe.Black, cell, BatteryTerminal.Positive);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.WrongMode));
            meter.TryContact(MeterProbe.Red, cell, BatteryTerminal.Positive);
            meter.SetMode(MeterMode.DirectVoltage);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.SameTerminal));
            Assert.That(meter.DisplayVoltage, Is.Zero);
            Assert.That(meter.TryGetReading(cell, out _), Is.False);

            meter.TryContact(MeterProbe.Red, cell, BatteryTerminal.Negative);
            session.Advance(1.5, 30, Fear);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.ReversePolarity));
            Assert.That(meter.DisplayVoltage, Is.EqualTo(-1.47).Within(1e-9));
            Assert.That(meter.TryGetReading(cell, out _), Is.False);
        }

        [Test]
        public void PauseResumesTheSameReadingButLiftingAMovingProbeOrChangingModeRestartsIt()
        {
            var session = OpenInspection();
            BatteryCell cell = session.GetSlot(0);
            var meter = session.Inspection.Meter;
            meter.SetMode(MeterMode.DirectVoltage);
            Connect(meter, cell);
            session.Advance(.5, 30, Fear);
            session.Advance(5, 30, Fear, paused: true);
            Assert.That(meter.Remaining, Is.EqualTo(1));
            Assert.That(session.Fear, Is.EqualTo(1.5));

            meter.Lift(MeterProbe.Red);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.NoContact));
            Assert.That(meter.Remaining, Is.Zero);
            meter.TryContact(MeterProbe.Red, cell, BatteryTerminal.Positive);
            session.Advance(.6, 30, Fear);
            meter.SetMode(MeterMode.Off);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.Off));
            meter.SetMode(MeterMode.DirectVoltage);
            Assert.That(meter.Remaining, Is.EqualTo(1.5));
            session.Advance(1.5, 30, Fear, paused: true);
            session.Advance(1.5, 30, Fear);
            Assert.That(meter.Display, Is.EqualTo(MeterDisplay.Voltage));
            meter.StoreProbes();
            Assert.That(meter.GetContact(MeterProbe.Black).Cell, Is.Null);
            session.CancelInspection();
            Assert.That(session.Inspection, Is.Null);
            Assert.That(session.HasBeam, Is.True);
            Assert.That(session.Fear, Is.GreaterThan(1.5));
        }

        [Test]
        public void SwappingCellsKeepsTheirIdentityChargeAndMeasurement()
        {
            var session = OpenInspection();
            BatteryInspection inspection = session.Inspection;
            BatteryCell first = session.GetSlot(0);
            BatteryCell second = session.GetSlot(1);
            inspection.Meter.SetMode(MeterMode.DirectVoltage);
            Connect(inspection.Meter, first);
            session.Advance(1.5, 30, Fear);
            Assert.That(inspection.TryExtract(0, out int firstHome), Is.True);
            Assert.That(inspection.Meter.Display, Is.EqualTo(MeterDisplay.NoContact));
            Assert.That(inspection.Meter.TryContact(MeterProbe.Red, first, BatteryTerminal.Positive), Is.False);
            Assert.That(inspection.TryExtract(1, out int secondHome), Is.True);
            Assert.That(firstHome, Is.Not.EqualTo(secondHome));
            Assert.That(session.GetSlot(0), Is.SameAs(first));
            Assert.That(inspection.TryInsert(firstHome, 1), Is.True);
            Assert.That(inspection.TryInsert(firstHome, 0), Is.False);
            Assert.That(inspection.TryInsert(secondHome, 0), Is.True);
            Assert.That(inspection.Meter.TryGetReading(first, out double reading), Is.True);
            Assert.That(reading, Is.EqualTo(session.MeasureSlot(1)));
            Assert.That(session.ConfirmInspection(), Is.True);
            Assert.That(session.GetSlot(0), Is.SameAs(second));
            Assert.That(session.GetSlot(1), Is.SameAs(first));
            Assert.That(session.ActiveSlot, Is.EqualTo(1));
            Assert.That(session.Inventory.Count, Is.EqualTo(1));
        }

        [Test]
        public void CancelRestoresStagedMovesAndConfirmConsumesOnlyInstalledSpares()
        {
            var session = OpenInspection();
            BatteryCell first = session.GetSlot(0);
            BatteryCell spare = session.Inventory[0];
            session.Inspection.TryExtract(0, out _);
            session.Inspection.TryInsert(0, 0);
            session.Advance(4, 30, Fear);
            session.CancelInspection();
            Assert.That(session.GetSlot(0), Is.SameAs(first));
            Assert.That(session.Inventory[0], Is.SameAs(spare));
            Assert.That(session.Fear, Is.EqualTo(12));

            session.BeginInspection();
            session.Inspection.TryExtract(0, out _);
            session.Inspection.TryInsert(0, 0);
            Assert.That(session.ConfirmInspection(), Is.True);
            Assert.That(session.GetSlot(0), Is.SameAs(spare));
            Assert.That(session.Inventory, Is.Empty);
            Assert.That(session.TryCollect(first), Is.False);
            Assert.That(session.BeginInspection(), Is.True, "Se pueden comparar dos pilas aun sin repuestos.");
        }

        private static SurvivalSession OpenInspection()
        {
            var session = new SurvivalSession(new BatteryCell("a", .75), new BatteryCell("b", .20), true,
                new[] { new BatteryCell("spare", .87) });
            Assert.That(session.BeginInspection(), Is.True);
            return session;
        }

        private static void Connect(BatteryMeter meter, BatteryCell cell)
        {
            Assert.That(meter.TryContact(MeterProbe.Black, cell, BatteryTerminal.Negative), Is.True);
            Assert.That(meter.TryContact(MeterProbe.Red, cell, BatteryTerminal.Positive), Is.True);
        }
    }
}
