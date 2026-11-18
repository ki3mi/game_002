using System;
using AssetInteraction;
using UnityEngine;

namespace SensorAssets
{
    public sealed class SensorLuminosidad : PowerDeviceInteractable
    {
        [SerializeField, Min(0.5f)] private float sensingRadius = 12f;
        [SerializeField, Min(0f)] private float threshold = 0.35f;
        [SerializeField, Min(0.1f)] private float sampleInterval = 0.5f;
        private float nextSample;

        public float EstimatedIlluminance { get; private set; }
        public bool IsAboveThreshold => EstimatedIlluminance >= threshold;
        public event Action<float> ReadingChanged;

        private void Update()
        {
            if (!IsPowered || Time.unscaledTime < nextSample) return;
            nextSample = Time.unscaledTime + sampleInterval;
            float value = RenderSettings.ambientIntensity * 0.1f;
            foreach (Light source in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (source == null || !source.enabled) continue;
                float distance = Vector3.Distance(transform.position, source.transform.position);
                if (distance <= sensingRadius) value += source.intensity / (1f + distance * distance);
            }
            EstimatedIlluminance = value;
            ReadingChanged?.Invoke(value);
        }
    }
}