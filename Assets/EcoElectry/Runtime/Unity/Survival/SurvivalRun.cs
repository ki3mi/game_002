using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal static class SurvivalRun
    {
        private static SurvivalSession current;
        private static bool supplyCut;
        internal static bool SupplyCut => supplyCut || current?.SupplyCut == true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void StartNew()
        {
            current = null;
            supplyCut = false;
        }

        internal static void CutSupply()
        {
            supplyCut = true;
            current?.CutSupply();
        }

        internal static SurvivalSession Enter(SurvivalSettings settings, bool initiallyIsolated = false)
        {
            if (current == null)
            {
                current = settings.CreateSession();
                if (initiallyIsolated || supplyCut) current.CutSupply();
            }
            return current;
        }
    }
}
