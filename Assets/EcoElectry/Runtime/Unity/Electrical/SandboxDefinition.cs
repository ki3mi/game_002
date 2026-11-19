using System;
using EcoElectry.Domain.Electrical;
using EcoElectry.Unity.Construction;
using EcoElectry.Unity.Levels;
using UnityEngine;

namespace EcoElectry.Unity.Electrical
{
    [Serializable]
    public sealed class SandboxCaseEntry
    {
        [SerializeField] private string id;
        [SerializeField] private string label;
        [SerializeField] private SandboxPreset preset;
        [SerializeField] private float generation = 6;
        [SerializeField] private float capacity = 6;
        [SerializeField] private float demand = 4;
        [SerializeField] private Vector3 panelOffset;
        [SerializeField] private Vector3 batteryOffset;
        [SerializeField] private Vector3 routeOffset;

        public string Label => label;
        public Vector3 PanelOffset => panelOffset;
        public Vector3 BatteryOffset => batteryOffset;
        public Vector3 RouteOffset => routeOffset;
        public SandboxCase CreateCase() => new SandboxCase(id, generation, capacity, demand, preset);
    }

    [CreateAssetMenu(menuName = "EcoElectry/Sandbox eléctrico")]
    public sealed class SandboxDefinition : LevelDefinition
    {
        [SerializeField] private SandboxCaseEntry[] cases;
        [SerializeField] private GameObject solarPanel;
        [SerializeField] private GameObject battery;
        [SerializeField] private GameObject board;
        [SerializeField] private GameObject lamp;
        [SerializeField] private GameObject isolator;
        [SerializeField] private GameObject protection;

        [Header("Colocación en grilla")]
        [SerializeField] private GridModelCatalog placementCatalog;
        [SerializeField] private Vector3 gridOrigin = new Vector3(-7.5f, 0f, -5.5f);
        [SerializeField] private Vector2Int gridSize = new Vector2Int(30, 16);
        [SerializeField, Min(0.1f)] private float gridCellSize = 0.5f;

        public System.Collections.Generic.IReadOnlyList<SandboxCaseEntry> Cases => cases;
        public GameObject Isolator => isolator;
        public GameObject Protection => protection;
        public GridModelCatalog PlacementCatalog => placementCatalog;
        public Vector3 GridOrigin => gridOrigin;
        public Vector2Int GridSize => gridSize;
        public float GridCellSize => gridCellSize;

        internal GameObject VisualFor(Equipment equipment)
        {
            switch (equipment)
            {
                case Equipment.Panel: return solarPanel;
                case Equipment.Battery: return battery;
                case Equipment.Board: return board;
                case Equipment.Lamp: return lamp;
                default: throw new ArgumentOutOfRangeException(nameof(equipment));
            }
        }
    }
}
