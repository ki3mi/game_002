using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [DisallowMultipleComponent]
    public sealed class BatteryCase : MonoBehaviour
    {
        [SerializeField] private Transform lid = null;
        [SerializeField] private BatteryPickup battery = null;
        [SerializeField] private float openAngle = -106f;

        private Quaternion closedRotation;
        public bool IsOpen { get; private set; }
        internal BatteryPickup Battery => battery;
        internal string Title { get; private set; } = "Caja de suministros";
        internal string Description { get; private set; }

        internal void ConfigureWorkshop(string title, string description, Transform hinge, BatteryPickup pickup)
        {
            Title = title;
            Description = description;
            lid = hinge;
            closedRotation = hinge.localRotation;
            openAngle = SupplyCaseGeometry.OpenAngle;
            battery = pickup;
            if (battery != null) battery.gameObject.SetActive(false);
        }

        private void Awake()
        {
            if (lid != null) closedRotation = lid.localRotation;
            if (battery != null) battery.gameObject.SetActive(false);
        }

        internal void Open(SurvivalSession session)
        {
            if (IsOpen) return;
            IsOpen = true;
            if (lid != null) lid.localRotation = closedRotation * Quaternion.Euler(openAngle, 0, 0);
            if (battery == null) return;
            battery.gameObject.SetActive(true);
            battery.ApplySession(session);
        }
    }
}
