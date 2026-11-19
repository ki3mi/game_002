using System.Linq;
using EcoElectry.Domain.Construction;
using EcoElectry.Unity.Electrical;
using NUnit.Framework;
using UnityEditor;

namespace EcoElectry.Tests.EditMode
{
    public sealed class PlacementGridTests
    {
        [Test]
        public void RotationReservesTheEntireOrientedFootprintUntilUndo()
        {
            var grid = new PlacementGrid(new GridFootprint(6, 5));
            var anchor = new GridCell(1, 1);
            var footprint = new GridFootprint(3, 2);
            var singleCell = new GridFootprint(1, 1);

            GridPlacement placement = grid.TryPlace("panel", anchor, footprint, 1);

            Assert.That(placement.Footprint.Width, Is.EqualTo(2));
            Assert.That(placement.Footprint.Depth, Is.EqualTo(3));
            Assert.That(grid.Check(new GridCell(2, 3), singleCell), Is.EqualTo(GridAvailability.Occupied));
            Assert.That(grid.Check(new GridCell(3, 3), singleCell), Is.EqualTo(GridAvailability.Available));
            Assert.That(grid.TryPlace("chair", new GridCell(2, 3), singleCell, 0), Is.Null);
            Assert.That(grid.Undo(), Is.SameAs(placement));
            Assert.That(grid.Check(new GridCell(2, 3), singleCell), Is.EqualTo(GridAvailability.Available));
        }

        [Test]
        public void RejectsReservedCellsAndOverflowWithoutOccupyingAnyCells()
        {
            var grid = new PlacementGrid(new GridFootprint(6, 5));
            var singleCell = new GridFootprint(1, 1);
            var footprint = new GridFootprint(3, 2);
            grid.Reserve(new GridCell(2, 2), singleCell);

            Assert.That(grid.Check(new GridCell(0, 2), footprint), Is.EqualTo(GridAvailability.Reserved));
            Assert.That(grid.TryPlace("soporte", new GridCell(0, 2), footprint, 0), Is.Null);
            Assert.That(grid.TryPlace("batería", new GridCell(5, 0), footprint, 0), Is.Null);
            Assert.That(grid.TryPlace("batería", new GridCell(-1, 0), footprint, 0), Is.Null);
            Assert.That(grid.Count, Is.Zero);
            Assert.That(grid.Check(new GridCell(0, 2), singleCell), Is.EqualTo(GridAvailability.Available));
        }

        [Test]
        public void SandboxCatalogReferencesAllResourcePackPrefabs()
        {
            const string path = "Assets/EcoElectry/Content/Definitions/Levels/SandboxElectrical.asset";
            var sandbox = AssetDatabase.LoadAssetAtPath<SandboxDefinition>(path);
            Assert.That(sandbox.PlacementCatalog.PreviewShader, Is.Not.Null);

            var models = sandbox.PlacementCatalog.Models;
            var required = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ResourcePack" })
                .Where(guid => AssetDatabase.GUIDToAssetPath(guid).EndsWith(".prefab"));
            Assert.That(models.Select(model => model.Id), Is.SupersetOf(required));
            Assert.That(models.Select(model => model.Id), Is.Unique);
            foreach (var model in models)
            {
                Assert.That(model.Source, Is.Not.Null, model.Label);
                GridFootprint footprint = model.Footprint(sandbox.GridCellSize);
                Assert.That(footprint.Width, Is.InRange(1, sandbox.GridSize.x), model.Label);
                Assert.That(footprint.Depth, Is.InRange(1, sandbox.GridSize.y), model.Label);
            }
        }
    }
}
