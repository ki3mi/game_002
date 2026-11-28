using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [CreateAssetMenu(menuName = "EcoElectry/Modelos de inspección")]
    public sealed class InspectionModels : ScriptableObject
    {
        [SerializeField] private GameObject flashlight = null;
        [SerializeField] private GameObject cell = null;
        [SerializeField] private GameObject blackProbe = null;
        [SerializeField] private GameObject redProbe = null;

        internal GameObject Flashlight => flashlight;
        internal GameObject Cell => cell;
        internal GameObject BlackProbe => blackProbe;
        internal GameObject RedProbe => redProbe;
        internal bool Assigned => flashlight != null && cell != null && blackProbe != null && redProbe != null;
    }
}
