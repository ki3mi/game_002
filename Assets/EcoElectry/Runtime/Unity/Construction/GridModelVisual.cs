using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace EcoElectry.Unity.Construction
{
    public static class GridModelVisual
    {
        public static bool TryGetBounds(GameObject source, out Bounds bounds)
        {
            Bounds combined = default;
            bool found = false;
            TakeBounds(source.transform, Matrix4x4.identity, true, (mesh, matrix) =>
            {
                Bounds local = mesh.bounds;
                Vector3 center = matrix.MultiplyPoint3x4(local.center);
                Vector3 axisX = matrix.MultiplyVector(Vector3.right * local.extents.x);
                Vector3 axisY = matrix.MultiplyVector(Vector3.up * local.extents.y);
                Vector3 axisZ = matrix.MultiplyVector(Vector3.forward * local.extents.z);
                var total = new Vector3(
                    Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                    Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                    Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
                if (found)
                {
                    combined.Encapsulate(center - total);
                    combined.Encapsulate(center + total);
                }
                else
                {
                    combined = new Bounds(center, total * 2f);
                    found = true;
                }
            });

            bounds = combined;
            return found;
        }

        internal static GameObject Create(GridModel model, Transform parent, Material previewMaterial = null)
        {
            var pivot = new GameObject(model.Label);
            pivot.SetActive(false);
            pivot.transform.SetParent(parent, false);

            var geometry = new GameObject("Geometría").transform;
            geometry.SetParent(pivot.transform, false);
            geometry.localScale = Vector3.one * model.Scale;
            Bounds bounds = model.VisualBounds;
            geometry.localPosition = -model.Scale * new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            CopyGeometry(model.Source.transform, geometry, previewMaterial, true);
            return pivot;
        }

        private static void TakeBounds(Transform source, Matrix4x4 parent, bool root, Action<Mesh, Matrix4x4> add)
        {
            if (!root && !source.gameObject.activeSelf) return;

            Matrix4x4 matrix = parent * Matrix4x4.TRS(root ? Vector3.zero : source.localPosition,
                source.localRotation, source.localScale);
            if (TryGetMesh(source, out Mesh mesh, out _))
                add(mesh, matrix);

            foreach (Transform child in source)
                TakeBounds(child, matrix, false, add);
        }

        private static void CopyGeometry(Transform source, Transform parent, Material preview, bool root)
        {
            if (!root && !source.gameObject.activeSelf) return;

            var node = new GameObject(source.name);
            Transform transform = node.transform;
            transform.SetParent(parent, false);
            transform.localPosition = root ? Vector3.zero : source.localPosition;
            transform.localRotation = source.localRotation;
            transform.localScale = source.localScale;

            if (TryGetMesh(source, out Mesh mesh, out Renderer sourceRenderer))
            {
                node.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = node.AddComponent<MeshRenderer>();
                if (preview == null)
                {
                    renderer.sharedMaterials = sourceRenderer.sharedMaterials;
                    renderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                    renderer.receiveShadows = sourceRenderer.receiveShadows;
                }
                else
                {
                    var materials = new Material[mesh.subMeshCount];
                    Array.Fill(materials, preview);
                    renderer.sharedMaterials = materials;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }

            foreach (Transform child in source)
                CopyGeometry(child, transform, preview, false);
        }

        private static bool TryGetMesh(Transform source, out Mesh mesh, out Renderer renderer)
        {
            if (source.TryGetComponent(out MeshRenderer staticRenderer) && staticRenderer.enabled &&
                source.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
            {
                mesh = filter.sharedMesh;
                renderer = staticRenderer;
                return true;
            }

            if (source.TryGetComponent(out SkinnedMeshRenderer skinned) && skinned.enabled && skinned.sharedMesh != null)
            {
                mesh = skinned.sharedMesh;
                renderer = skinned;
                return true;
            }

            mesh = null;
            renderer = null;
            return false;
        }
    }
}
