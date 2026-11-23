using UnityEngine;

namespace EcoElectry.Unity.Player
{
    internal sealed class CorridorFlicker
    {
        private readonly float minDelay;
        private readonly float maxDelay;
        private float start;
        private float duration;
        private float depth;

        internal CorridorFlicker(Vector2 interval, float now)
        {
            minDelay = Mathf.Max(1f, interval.x);
            maxDelay = Mathf.Max(minDelay, interval.y);
            Restart(now);
        }

        internal void Restart(float now)
        {
            start = now + Random.Range(minDelay, maxDelay);
            duration = Random.Range(0.13f, 0.24f);
            depth = Random.Range(0.7f, 1f);
        }

        internal float Sample(float now)
        {
            float elapsed = now - start;
            if (elapsed <= 0f) return 0f;
            if (elapsed >= duration)
            {
                Restart(now);
                return 0f;
            }

            return depth * Mathf.Sin(Mathf.PI * elapsed / duration);
        }
    }
}
