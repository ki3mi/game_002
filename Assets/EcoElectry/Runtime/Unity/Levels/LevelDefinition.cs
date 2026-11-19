using EcoElectry.Domain.Levels;
using UnityEngine;

namespace EcoElectry.Unity.Levels
{
    [CreateAssetMenu(menuName = "EcoElectry/Definición de nivel")]
    public class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId;
        [SerializeField, Min(0)] private int budgetCredits;
        [SerializeField, Range(0, 100)] private int minimumBatteryReservePercent;

        public LevelRules CreateRules()
        {
            return new LevelRules(levelId, budgetCredits, minimumBatteryReservePercent);
        }
    }
}
