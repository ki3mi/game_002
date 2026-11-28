using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using NUnit.Framework;

namespace EcoElectry.Tests.EditMode
{
    public sealed class FuseRepairTests
    {
        private static FuseRepair Create(SurvivalSession session) => session.GetFuseRepair("limpieza",
            new[] { "F-01", "F-02", "F-03" }, new[]
            {
                new FuseCartridge("a", "F-01", true),
                new FuseCartridge("b", "F-02", false),
                new FuseCartridge("c", "F-03", true),
                new FuseCartridge("spare", "F-02", true)
            });

        private static SurvivalSession NewSession() => new(new BatteryCell("installed", 1), null, true);

        private static void Open(FuseRepair repair)
        {
            foreach (int screw in new[] { 3, 0, 2, 1 })
            {
                Assert.That(repair.TryMoveCover(), Is.False);
                Assert.That(repair.TryTurnScrew(screw), Is.True);
            }
            Assert.That(repair.TryMoveCover(), Is.True);
        }

        private static void Measure(FuseRepair repair, int index, MeterDisplay expected)
        {
            Assert.That(repair.TryMoveToBench(index));
            repair.Meter.SetMode(MeterMode.Continuity);
            Assert.That(repair.Meter.TryContact(MeterProbe.Black, index, 0));
            Assert.That(repair.Meter.TryContact(MeterProbe.Red, index, 1));
            Assert.That(repair.Meter.Display, Is.EqualTo(expected));
        }

        [Test]
        public void CutSupplyIsRequiredAndInvalidContactsNeverRegisterAReading()
        {
            SurvivalSession session = NewSession();
            FuseRepair repair = Create(session);
            Assert.That(repair.TryTurnScrew(0), Is.False);
            Assert.That(repair.TryMoveToBench(0), Is.False);
            Assert.That(repair.Meter.TryContact(MeterProbe.Black, 0, 0), Is.False);
            session.CutSupply();
            Open(repair);
            Assert.That(repair.TryMoveToBench(0));
            repair.Meter.SetMode(MeterMode.DirectVoltage);
            Assert.That(repair.Meter.TryContact(MeterProbe.Black, 0, 0));
            Assert.That(repair.Meter.TryContact(MeterProbe.Red, 0, 1));
            Assert.That(repair.Meter.Display, Is.EqualTo(MeterDisplay.WrongMode));
            Assert.That(repair.TryGetReading(0, out _), Is.False);

            repair.Meter.Lift(MeterProbe.Red);
            repair.Meter.SetMode(MeterMode.Continuity);
            Assert.That(repair.Meter.TryContact(MeterProbe.Red, 1, 1), Is.False);
            Assert.That(repair.Meter.Display, Is.EqualTo(MeterDisplay.NoContact));
            Assert.That(repair.Meter.TryContact(MeterProbe.Red, 0, 0));
            Assert.That(repair.Meter.Display, Is.EqualTo(MeterDisplay.SameTerminal));
            Assert.That(repair.TryGetReading(0, out _), Is.False);
        }

        [Test]
        public void CompletingRepairRequiresACheckedCompatibleSpareAndFourScrews()
        {
            var session = NewSession();
            session.CutSupply();
            FuseRepair repair = Create(session);
            int tones = 0;
            repair.Meter.ContinuityReached += () => tones++;
            Open(repair);
            Measure(repair, 0, MeterDisplay.Continuity);
            Measure(repair, 1, MeterDisplay.OpenCircuit);
            Assert.That(tones, Is.EqualTo(1));
            Assert.That(repair.DiagnosedOpen, Is.EqualTo(1));
            Assert.That(repair.TryMount(1), Is.EqualTo(FuseMountResult.Open));
            Assert.That(repair.TryCollectSpare(), Is.False);
            Assert.That(repair.ReadClue(), Is.True);
            repair.Meter.StoreProbes();
            Assert.That(repair.Meter.Display, Is.EqualTo(MeterDisplay.NoContact));
            Assert.That(Create(session), Is.SameAs(repair));
            Assert.That(repair.Position(0), Is.EqualTo(FusePosition.Tray));
            Assert.That(repair.Position(1), Is.EqualTo(FusePosition.Bench));
            Assert.That(repair.TryCollectSpare(), Is.True);
            Assert.That(repair.TryCollectSpare(), Is.False);

            Assert.That(repair.TryMoveToBench(3));
            Assert.That(repair.TryMount(0), Is.EqualTo(FuseMountResult.Incompatible));
            Assert.That(repair.TryMount(1), Is.EqualTo(FuseMountResult.Unverified));
            Measure(repair, 2, MeterDisplay.Continuity);
            Assert.That(repair.TryMount(2), Is.EqualTo(FuseMountResult.Mounted));
            Assert.That(repair.TryMoveToBench(0));
            Assert.That(repair.TryMount(0), Is.EqualTo(FuseMountResult.Mounted));
            Measure(repair, 3, MeterDisplay.Continuity);
            Assert.That(repair.TryMount(1), Is.EqualTo(FuseMountResult.Mounted));
            Assert.That(repair.TryGetReading(1, out bool conducts), Is.True);
            Assert.That(conducts, Is.False);
            Assert.That(repair.Position(1), Is.EqualTo(FusePosition.Tray));
            Assert.That(tones, Is.EqualTo(3));
            Assert.That(repair.TryMoveCover());
            for (int screw = 0; screw < FuseRepair.ScrewCount; screw++)
            {
                Assert.That(repair.Completed, Is.False);
                Assert.That(repair.TryTurnScrew(screw));
            }
            Assert.That(repair.Completed, Is.True);
            Assert.That(repair.SupplyCut, Is.True);
        }
    }
}
