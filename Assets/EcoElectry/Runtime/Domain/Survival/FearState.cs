using System;

namespace EcoElectry.Domain.Survival
{
    public sealed class FearRules
    {
        public FearRules(double alert, double safe, double increasePerSecond, double recoveryPerSecond)
        {
            if (!(0 <= safe && safe < alert && alert <= FearState.Maximum)) throw new ArgumentOutOfRangeException(nameof(alert));
            if (!(increasePerSecond >= 0) || double.IsInfinity(increasePerSecond)) throw new ArgumentOutOfRangeException(nameof(increasePerSecond));
            if (!(recoveryPerSecond >= 0) || double.IsInfinity(recoveryPerSecond)) throw new ArgumentOutOfRangeException(nameof(recoveryPerSecond));
            Alert = alert;
            Safe = safe;
            IncreasePerSecond = increasePerSecond;
            RecoveryPerSecond = recoveryPerSecond;
        }

        public double Alert { get; }
        public double Safe { get; }
        public double IncreasePerSecond { get; }
        public double RecoveryPerSecond { get; }
    }

    public sealed class FearState
    {
        public const double Maximum = 100;
        private const double BoundaryTolerance = 1e-9;
        public double Value { get; private set; }
        public bool Visible { get; private set; }

        public bool Advance(double seconds, bool hasBeam, FearRules rules)
        {
            if (Value >= Maximum || seconds <= 0) return false;
            double rate = hasBeam ? -rules.RecoveryPerSecond : rules.IncreasePerSecond;
            Value = Math.Clamp(Value + seconds * rate, 0, Maximum);
            if (Maximum - Value <= BoundaryTolerance) Value = Maximum;
            Visible = Visible ? Value - rules.Safe > BoundaryTolerance : rules.Alert - Value <= BoundaryTolerance;
            return Value >= Maximum;
        }
    }
}
