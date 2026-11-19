using System;

namespace EcoElectry.Domain.Levels
{
    public sealed class LevelRules
    {
        public LevelRules(string id, int budgetCredits, int minimumBatteryReservePercent)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El nivel requiere un identificador.", nameof(id));

            if (budgetCredits < 0)
                throw new ArgumentOutOfRangeException(nameof(budgetCredits), "El presupuesto debe ser mayor o igual a cero.");

            if (minimumBatteryReservePercent < 0 || minimumBatteryReservePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(minimumBatteryReservePercent), "La reserva debe estar entre 0 y 100 %.");

            Id = id;
            BudgetCredits = budgetCredits;
            MinimumBatteryReservePercent = minimumBatteryReservePercent;
        }

        public string Id { get; }
        public int BudgetCredits { get; }
        public int MinimumBatteryReservePercent { get; }
    }
}
