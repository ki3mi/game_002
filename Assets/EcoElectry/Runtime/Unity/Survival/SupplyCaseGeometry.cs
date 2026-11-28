using System;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal static class SupplyCaseGeometry
    {
        internal const float OpenAngle = -105f;
        internal const float Bottom = -0.305f;
        internal const float Width = 1.82f;
        internal const float Depth = 1.13f;

        internal static (Transform hinge, Collider latch) Build(Transform root,
            Func<string, Vector3, Vector3, Color, bool, Transform, GameObject> block)
        {
            Color graphite = new(.09f, .13f, .14f);
            Color steel = new(.41f, .46f, .47f);
            Color copper = new(.72f, .40f, .18f);
            block("Fondo", new Vector3(0, -.23f, 0), new Vector3(1.81f, .15f, 1.13f), graphite, true, root);
            foreach (float x in new[] { -.85f, .85f })
                block("Costado", new Vector3(x, -.075f, 0), new Vector3(.102f, .31f, 1.13f), graphite, true, root);
            foreach (float z in new[] { -.518f, .518f })
                block("Frente", new Vector3(0, -.075f, z), new Vector3(1.62f, .31f, .099f), graphite, true, root);
            var hinge = new GameObject("Bisagra") { layer = root.gameObject.layer }.transform;
            hinge.SetParent(root, false);
            hinge.localPosition = new Vector3(0, .09f, -.54f);
            block("Tapa", new Vector3(0, .016f, .54f), new Vector3(1.82f, .103f, 1.13f), graphite, true, hinge);
            foreach (float x in new[] { -.66f, .66f })
                block("Refuerzo", new Vector3(x, .077f, .54f), new Vector3(.061f, .031f, .99f), steel, false, hinge);
            Collider latch = block("Cierre", new Vector3(0, -.032f, 1.128f), new Vector3(.28f, .18f, .075f),
                copper, true, hinge).GetComponent<Collider>();
            return (hinge, latch);
        }
    }
}
