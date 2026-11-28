using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using NUnit.Framework;

namespace EcoElectry.Tests.EditMode
{
    public sealed class SurvivalSessionTests
    {
        private static readonly FearRules Rules = new(12, 6, 3, 5);

        [Test]
        public void ANewRunUsesOnlyTheFirstCellUntilItRunsOut()
        {
            var session = NewRun();
            Assert.That(session.TotalCharge * 2, Is.EqualTo(2));
            Assert.That(session.GetSlot(1), Is.Null);
            Assert.That(session.ShowFear, Is.False);

            Install(session, 1, new BatteryCell("spare", .75));
            session.Advance(7.5, 10, Rules);
            Assert.That(session.GetSlot(0).Charge, Is.EqualTo(.25));
            Assert.That(session.GetSlot(1).Charge, Is.EqualTo(.75));
            session.Advance(3, 10, Rules);
            Assert.That(session.GetSlot(0).Charge, Is.Zero);
            Assert.That(session.GetSlot(1).Charge, Is.EqualTo(.70).Within(1e-9));
            Assert.That(session.ActiveSlot, Is.EqualTo(1));
            Assert.That(session.HasBeam, Is.True);
        }

        [Test]
        public void ExhaustionAccountsForOnlyTheDarkPortionOfTheFrame()
        {
            var coarse = NewRun();
            var fine = NewRun();
            coarse.Advance(14, 10, Rules);
            for (int i = 0; i < 56; i++) fine.Advance(.25, 10, Rules);

            Assert.That(coarse.TotalCharge, Is.EqualTo(fine.TotalCharge).Within(1e-9));
            Assert.That(coarse.Fear, Is.EqualTo(fine.Fear).Within(1e-9));
            Assert.That(coarse.ShowFear, Is.EqualTo(fine.ShowFear));
            Assert.That(coarse.Fear, Is.EqualTo(12));
            Assert.That(coarse.HasBeam, Is.False);
            Assert.That(coarse.RequestedOn, Is.True);
            Assert.That(coarse.ShowFear, Is.True);
            Install(coarse, 1, new BatteryCell("restore", 1));
            Assert.That(coarse.HasBeam, Is.True);
        }

        [Test]
        public void FearUsesElapsedTimeAndSeparateShowAndHideThresholds()
        {
            var session = NewRun();
            for (int i = 0; i < 10; i++) session.ToggleFlashlight();
            Assert.That(session.Fear, Is.Zero);
            session.ToggleFlashlight();
            session.Advance(3, 30, Rules);
            Assert.That(session.ShowFear, Is.False);
            session.Advance(1, 30, Rules);
            Assert.That(session.ShowFear, Is.True);
            Assert.That(session.TotalCharge, Is.EqualTo(1));
            session.ToggleFlashlight();
            session.Advance(1, 30, Rules);
            Assert.That(session.Fear, Is.EqualTo(7));
            Assert.That(session.ShowFear, Is.True);
            session.Advance(.2, 30, Rules);
            Assert.That(session.Fear, Is.EqualTo(6));
            Assert.That(session.ShowFear, Is.False);
            session.ToggleFlashlight();
            session.Advance(2, 30, Rules);
            Assert.That(session.ShowFear, Is.True);
        }

        [Test]
        public void CancelPreservesInventorySwitchAndChargeButKeepsAccumulatedFear()
        {
            var session = NewRun();
            var spare = new BatteryCell("corridor", .81);
            Assert.That(session.BeginInspection(), Is.False);
            Assert.That(session.TryCollect(spare), Is.True);
            Assert.That(session.BeginInspection(), Is.True);
            Assert.That(session.HasBeam, Is.False);
            Assert.That(session.MeasureSlot(0), Is.Null);
            session.Advance(4, 30, Rules);
            session.CancelInspection();
            Assert.That(session.HasBeam, Is.True);
            Assert.That(session.GetSlot(0).Charge, Is.EqualTo(1));
            Assert.That(session.GetSlot(1), Is.Null);
            Assert.That(session.Inventory[0], Is.SameAs(spare));
            Assert.That(session.Fear, Is.EqualTo(12));
        }

        [Test]
        public void ReplacementKeepsOtherCellAndNeverResurrectsADiscardedBattery()
        {
            var session = NewRun();
            var left = session.GetSlot(0);
            Install(session, 1, new BatteryCell("first-spare", .52));
            session.Advance(2, 10, Rules);
            session.ToggleFlashlight();
            var right = session.GetSlot(1);
            Assert.That(session.TryCollect(new BatteryCell("second-spare", .91)), Is.True);
            Assert.That(session.BeginInspection(), Is.True);
            Assert.That(session.ConfirmReplacement(3, "second-spare"), Is.False);
            Assert.That(session.ConfirmReplacement(0, "wrong-id"), Is.False);
            Assert.That(session.MeasureSlot(0), Is.EqualTo(1.50).Within(1e-9));
            Assert.That(session.MeasureSlot(1), Is.EqualTo(1.35).Within(1e-9));
            session.Advance(1.5, 10, Rules);
            Assert.That(session.MeasureSlot(0), Is.EqualTo(1.50).Within(1e-9));
            Assert.That(session.ConfirmReplacement(0, "second-spare"), Is.True);
            Assert.That(session.GetSlot(0).Charge, Is.EqualTo(.91));
            Assert.That(session.GetSlot(1), Is.SameAs(right));
            Assert.That(session.Inventory, Is.Empty);
            Assert.That(session.HasBeam, Is.False);
            Assert.That(session.TryCollect(new BatteryCell(left.Id, 1)), Is.False);
        }

        [Test]
        public void PauseFreezesTheSimulationAndFaintingRaisesOneSignal()
        {
            var session = NewRun();
            int fainted = 0;
            session.Fainted += () => fainted++;
            session.Advance(2, 10, Rules, paused: true);
            Assert.That(session.TotalCharge, Is.EqualTo(1));
            session.ToggleFlashlight();
            session.Advance(12, 10, Rules, paused: true);
            Assert.That(session.Fear, Is.Zero);
            session.Advance(34, 10, Rules);
            session.Advance(100, 10, Rules);
            Assert.That(session.Fear, Is.EqualTo(100));
            Assert.That(session.IsDefeated, Is.True);
            Assert.That(session.HasBeam, Is.False);
            Assert.That(fainted, Is.EqualTo(1));
        }

        [Test]
        public void AContinuationCanUseDifferentRulesWithTheSameState()
        {
            var session = NewRun();
            var collected = new BatteryCell("scene-a/desk", .62);
            Assert.That(session.TryCollect(collected), Is.True);
            session.Advance(2, 10, Rules);
            session.ToggleFlashlight();
            session.Advance(3, 10, Rules);
            var nextRules = new FearRules(12, 6, 4, 1);
            session.Advance(1, 40, nextRules);
            session.ToggleFlashlight();
            session.Advance(1, 40, nextRules);

            Assert.That(session.Fear, Is.EqualTo(12));
            Assert.That(session.GetSlot(0).Charge, Is.EqualTo(.775).Within(1e-9));
            Assert.That(session.Inventory[0], Is.SameAs(collected));
            Assert.That(session.HasCollected("scene-a/desk"), Is.True);
        }

        private static SurvivalSession NewRun() => new(new BatteryCell("initial", 1), null, true);

        private static void Install(SurvivalSession session, int slot, BatteryCell cell)
        {
            Assert.That(session.TryCollect(cell), Is.True);
            Assert.That(session.BeginInspection(), Is.True);
            Assert.That(session.ConfirmReplacement(slot, cell.Id), Is.True);
        }
    }
}
