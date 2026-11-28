using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [DisallowMultipleComponent]
    public sealed class FuseRepairInteractable : MonoBehaviour
    {
        internal enum Kind { Box, Spares, Spare }

        [SerializeField] private Kind kind;
        [SerializeField] private FuseRepairDefinition definition;

        internal Kind Type => kind;
        internal FuseRepairDefinition Definition => definition;
    }
}
