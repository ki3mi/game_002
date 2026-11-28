using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [DisallowMultipleComponent]
    public sealed class FuseBoxModel : MonoBehaviour
    {
        [SerializeField] private float width;
        [SerializeField] private Transform cover;
        [SerializeField] private Transform[] screws;
        [SerializeField] private Transform[] slots;

        internal float Width => width;
        internal Transform Cover => cover;
        internal Transform[] Screws => screws;
        internal Transform[] Slots => slots;
    }
}
