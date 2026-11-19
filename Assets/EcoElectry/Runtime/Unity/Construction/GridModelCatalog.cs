using System;
using System.Collections.Generic;
using EcoElectry.Domain.Construction;
using UnityEngine;

namespace EcoElectry.Unity.Construction
{
    [Serializable]
    public sealed class GridModel
    {
        [SerializeField] private string id;
        [SerializeField] private string label;
        [SerializeField] private string family;
        [SerializeField] private GameObject source;
        [SerializeField] private Bounds visualBounds;
        [SerializeField, Min(0.001f)] private float scale = 1f;

        public string Id => id;
        public string Label => label;
        public string Family => family;
        public GameObject Source => source;
        public Bounds VisualBounds => visualBounds;
        public float Scale => scale;

        public GridFootprint Footprint(float cellSize)
        {
            Vector3 size = visualBounds.size * scale / cellSize;
            return new GridFootprint(Cells(size.x), Cells(size.z));
        }

        private static int Cells(float length)
        {
            return Mathf.Max(1, Mathf.CeilToInt(length - 0.0001f));
        }
    }

    public sealed class GridModelCatalog : ScriptableObject
    {
        [SerializeField] private Shader previewShader;
        [SerializeField] private GridModel[] models = Array.Empty<GridModel>();

        public Shader PreviewShader => previewShader;
        public IReadOnlyList<GridModel> Models => models;
    }
}
