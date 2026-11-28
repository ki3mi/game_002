using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal static class SurvivalRun
    {
        private static SurvivalSession current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void StartNew() => current = null;

        internal static SurvivalSession Enter(SurvivalSettings settings)
        {
            current ??= settings.CreateSession();
            return current;
        }
    }
}
