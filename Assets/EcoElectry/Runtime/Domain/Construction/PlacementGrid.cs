using System;
using System.Collections.Generic;

namespace EcoElectry.Domain.Construction
{
    public readonly struct GridCell
    {
        public GridCell(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }
        public int Row { get; }
    }

    public readonly struct GridFootprint
    {
        public GridFootprint(int width, int depth)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));
            Width = width;
            Depth = depth;
        }

        public int Width { get; }
        public int Depth { get; }

        public GridFootprint Rotated(int quarterTurns)
        {
            return (quarterTurns & 1) == 0 ? this : new GridFootprint(Depth, Width);
        }
    }

    public enum GridAvailability
    {
        Available,
        Outside,
        Reserved,
        Occupied
    }

    public sealed class GridPlacement
    {
        internal GridPlacement(string modelId, GridCell anchor, GridFootprint footprint, int quarterTurns)
        {
            ModelId = modelId;
            Anchor = anchor;
            Footprint = footprint;
            QuarterTurns = quarterTurns;
        }

        public string ModelId { get; }
        public GridCell Anchor { get; }
        public GridFootprint Footprint { get; }
        public int QuarterTurns { get; }
    }

    public sealed class PlacementGrid
    {
        private readonly bool[,] reserved;
        private readonly bool[,] occupied;
        private readonly Stack<GridPlacement> placements = new Stack<GridPlacement>();

        public PlacementGrid(GridFootprint size)
        {
            if (size.Width <= 0 || size.Depth <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            reserved = new bool[size.Width, size.Depth];
            occupied = new bool[size.Width, size.Depth];
        }

        public GridFootprint Size { get; }
        public int Count => placements.Count;

        public void Reserve(GridCell anchor, GridFootprint footprint)
        {
            if (!Contains(anchor, footprint)) throw new ArgumentOutOfRangeException(nameof(anchor));
            if (HasAny(occupied, anchor, footprint)) throw new InvalidOperationException("La región contiene una pieza.");
            SetRegion(reserved, anchor, footprint, true);
        }

        public GridAvailability Check(GridCell anchor, GridFootprint footprint)
        {
            if (!Contains(anchor, footprint)) return GridAvailability.Outside;
            if (HasAny(reserved, anchor, footprint)) return GridAvailability.Reserved;
            return HasAny(occupied, anchor, footprint) ? GridAvailability.Occupied : GridAvailability.Available;
        }

        public GridPlacement TryPlace(string modelId, GridCell anchor, GridFootprint footprint, int quarterTurns)
        {
            GridFootprint oriented = footprint.Rotated(quarterTurns);
            if (Check(anchor, oriented) != GridAvailability.Available) return null;

            var placement = new GridPlacement(modelId, anchor, oriented, quarterTurns & 3);
            placements.Push(placement);
            SetRegion(occupied, anchor, oriented, true);
            return placement;
        }

        public GridPlacement Undo()
        {
            if (placements.Count == 0) return null;

            GridPlacement placement = placements.Pop();
            SetRegion(occupied, placement.Anchor, placement.Footprint, false);
            return placement;
        }

        private bool Contains(GridCell anchor, GridFootprint footprint)
        {
            return footprint.Width > 0 && footprint.Depth > 0 &&
                anchor.Column >= 0 && anchor.Row >= 0 &&
                anchor.Column <= Size.Width - footprint.Width &&
                anchor.Row <= Size.Depth - footprint.Depth;
        }

        private static bool HasAny(bool[,] cells, GridCell anchor, GridFootprint size)
        {
            for (int column = anchor.Column; column < anchor.Column + size.Width; column++)
            for (int row = anchor.Row; row < anchor.Row + size.Depth; row++)
            {
                if (cells[column, row]) return true;
            }

            return false;
        }

        private static void SetRegion(bool[,] cells, GridCell anchor, GridFootprint size, bool value)
        {
            for (int column = anchor.Column; column < anchor.Column + size.Width; column++)
            for (int row = anchor.Row; row < anchor.Row + size.Depth; row++)
                cells[column, row] = value;
        }
    }
}
