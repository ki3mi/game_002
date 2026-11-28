using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [DisallowMultipleComponent]
    public sealed class BatteryPickup : MonoBehaviour
    {
        [SerializeField] private string pickupId = "";
        [SerializeField, Range(0, 1)] private float charge = 1f;

        private string Id => $"{gameObject.scene.path}/{pickupId}";
        public bool Available => !string.IsNullOrWhiteSpace(pickupId) && gameObject.activeInHierarchy;

        internal void ConfigureWorkshop(string id) => pickupId = id;

        internal void ApplySession(SurvivalSession session)
        {
            if (session.HasCollected(Id)) gameObject.SetActive(false);
        }

        internal bool TryCollect(SurvivalSession session)
        {
            if (!Available || !session.TryCollect(new BatteryCell(Id, Mathf.Clamp01(charge)))) return false;
            gameObject.SetActive(false);
            return true;
        }
    }
}
