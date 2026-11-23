using UnityEngine;

namespace EcoElectry.Unity.Environment
{
    /// <summary>
    /// Spawns and moves pooled low-poly clouds across the level sky, recycling any cloud
    /// that drifts outside the configured area so distant clouds never waste rendering cost.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LowPolyCloudSystem : MonoBehaviour
    {
        [Header("Cloud Prefab & Pool")]
        [SerializeField] private GameObject cloudPrefab;
        [SerializeField, Min(1)] private int cloudCount = 18;

        [Header("Sky Bounds (Centered on Area Center)")]
        [SerializeField] private Vector3 areaCenter = new Vector3(10f, 38f, 0f);
        [SerializeField] private Vector2 areaSize = new Vector2(180f, 180f);
        [SerializeField, Min(0f)] private float heightVariation = 6f;

        [Header("Movement & Scale")]
        [SerializeField] private Vector3 windDirection = new Vector3(1f, 0f, 0.25f);
        [SerializeField, Min(0.1f)] private float minSpeed = 1.6f;
        [SerializeField, Min(0.1f)] private float maxSpeed = 3.2f;
        [SerializeField, Min(0.2f)] private float minScale = 0.85f;
        [SerializeField, Min(0.2f)] private float maxScale = 1.65f;

        private struct CloudInstance
        {
            public Transform Transform;
            public float Speed;
        }

        private CloudInstance[] pool;
        private Vector3 windDirNormalized;
        private Vector3 windPerpNormalized;
        private float halfLength;
        private float halfWidth;

        private void Start()
        {
            if (cloudPrefab == null)
            {
                Debug.LogWarning("Asigna cloudPrefab en LowPolyCloudSystem para generar nubes.", this);
                enabled = false;
                return;
            }

            Vector3 flatWind = new Vector3(windDirection.x, 0f, windDirection.z);
            windDirNormalized = flatWind.sqrMagnitude > 0.001f ? flatWind.normalized : Vector3.right;
            windPerpNormalized = new Vector3(-windDirNormalized.z, 0f, windDirNormalized.x);

            halfLength = areaSize.x * 0.5f;
            halfWidth = areaSize.y * 0.5f;

            pool = new CloudInstance[cloudCount];
            for (int i = 0; i < cloudCount; i++)
            {
                GameObject go = Instantiate(cloudPrefab, transform);
                go.name = "Cloud_" + (i + 1).ToString("00");

                float along = Random.Range(-halfLength, halfLength);
                float across = Random.Range(-halfWidth, halfWidth);
                PlaceCloud(go.transform, along, across);

                pool[i] = new CloudInstance
                {
                    Transform = go.transform,
                    Speed = Random.Range(minSpeed, maxSpeed)
                };
            }
        }

        private void Update()
        {
            if (pool == null) return;

            float dt = Time.deltaTime;
            for (int i = 0; i < pool.Length; i++)
            {
                Transform t = pool[i].Transform;
                if (t == null) continue;

                t.position += windDirNormalized * (pool[i].Speed * dt);

                Vector3 offset = t.position - areaCenter;
                float along = Vector3.Dot(offset, windDirNormalized);
                float across = Vector3.Dot(offset, windPerpNormalized);

                // Recycle cloud as soon as it leaves the active sky area
                if (along > halfLength || along < -halfLength || Mathf.Abs(across) > halfWidth)
                {
                    float newAcross = Random.Range(-halfWidth, halfWidth);
                    PlaceCloud(t, -halfLength, newAcross);
                    pool[i].Speed = Random.Range(minSpeed, maxSpeed);
                }
            }
        }

        private void PlaceCloud(Transform t, float along, float across)
        {
            float yOffset = Random.Range(-heightVariation, heightVariation);
            t.position = areaCenter + windDirNormalized * along + windPerpNormalized * across + Vector3.up * yOffset;
            t.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float s = Random.Range(minScale, maxScale);
            t.localScale = new Vector3(s * Random.Range(0.9f, 1.25f), s * Random.Range(0.75f, 1.0f), s * Random.Range(0.9f, 1.25f));
        }
    }
}
