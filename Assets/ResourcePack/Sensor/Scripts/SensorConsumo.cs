using System;
using AssetInteraction;
using TablerosEnergia;
using UnityEngine;

namespace SensorAssets
{
    public sealed class SensorConsumo : PowerDeviceInteractable
    {
        private EnergyGridRegistry registry;
        public float ActiveLoadWatts { get; private set; }
        public int ActiveLoadCount { get; private set; }
        public event Action<float, int> ReadingChanged;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            EnergyGridRegistry.EnsureRuntimeInstance();
            BindRegistry();
            Refresh();
        }

        private void OnDisable()
        {
            if (registry != null) registry.Changed -= Refresh;
            registry = null;
        }

        private void Update()
        {
            if (registry == null) BindRegistry();
        }

        private void BindRegistry()
        {
            EnergyGridRegistry current = EnergyGridRegistry.Instance;
            if (registry == current) return;
            if (registry != null) registry.Changed -= Refresh;
            registry = current;
            if (registry != null) registry.Changed += Refresh;
        }

        private void Refresh()
        {
            if (!IsPowered || registry == null) return;
            ActiveLoadWatts = registry.GetActiveLoadWatts();
            ActiveLoadCount = registry.GetActiveLoadCount();
            ReadingChanged?.Invoke(ActiveLoadWatts, ActiveLoadCount);
        }
    }
}