using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EcoElectry.Domain.Construction;
using EcoElectry.Unity.Construction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Electrical
{
    public sealed partial class SandboxElectricalRoot
    {
        private const float PaletteHeight = 128f;
        private const float PaletteInset = 13f;
        private const float CardGap = 7f;
        private static readonly Color SelectedTile = new Color32(38, 74, 74, 255);
        private ModelPlacement placement;
        private string[] modelFamilies = Array.Empty<string>();
        private IReadOnlyList<GridModel> visibleModels = Array.Empty<GridModel>();
        private string selectedFamily = string.Empty;
        private int palettePage;
        private bool categoryOpen;

        public IReadOnlyList<GridModel> PlaceableModels => definition?.PlacementCatalog?.Models ?? Array.Empty<GridModel>();
        public int PlacedModelCount => placement?.Count ?? 0;
        private bool HasSelectedModel => placement?.Selected != null;
        private float PlacementBarHeight => placement != null ? PaletteHeight : 0f;

        private void InitializePlacement()
        {
            GridModelCatalog catalog = definition.PlacementCatalog;
            if (catalog == null || catalog.Models.Count == 0 || catalog.PreviewShader == null) return;

            placement = new ModelPlacement(transform, catalog.PreviewShader, definition.GridOrigin,
                definition.GridSize, definition.GridCellSize);
            visibleModels = catalog.Models;
            modelFamilies = catalog.Models.Select(model => model.Family).Distinct().OrderBy(familyName => familyName).ToArray();
        }

        private void ResetPlacement()
        {
            categoryOpen = false;
            placement?.Reset(world.ConstructionObstacles());
        }

        public bool SelectPlacementModel(string modelId)
        {
            if (placement == null || Session.ReportOpen || showHelp) return false;

            GridModel model = PlaceableModels.FirstOrDefault(item => item.Id == modelId);
            if (model == null) return false;

            if (intervention || choosingOrigin) FinishIntervention();
            categoryOpen = false;
            placement.Select(model);
            return true;
        }

        public bool TryPlaceModel(Ray ray)
        {
            return !intervention && !Session.ReportOpen && !showHelp && !categoryOpen &&
                placement != null && placement.TryPlace(ray, player);
        }

        public void CancelPlacement()
        {
            categoryOpen = false;
            placement?.Cancel();
        }

        public void UndoPlacement()
        {
            placement?.Undo();
        }

        private void UpdatePlacementPreview()
        {
            if (!HasSelectedModel) return;

            if (intervention || Session.ReportOpen || showHelp || categoryOpen)
            {
                placement.HidePreview();
                return;
            }

            Mouse mouse = Mouse.current;
            Vector2 cursor = mouse?.position.ReadValue() ?? new Vector2(-1, -1);
            Ray? ray = sandboxCamera.pixelRect.Contains(cursor) ? sandboxCamera.ScreenPointToRay(cursor) : null;
            placement.Preview(ray, player);
        }

        private void DrawPlacementBar(Rect bar)
        {
            if (placement == null) return;

            Fill(bar, Ink);
            Fill(new Rect(bar.x, bar.y, bar.width, 1f), SelectedTile);
            float left = bar.x + PaletteInset;
            float row = bar.y + 9f;
            float right = bar.xMax - PaletteInset;
            Label(new Rect(left, row, 76, 29), "Modelos", strong, Text);

            string familyLabel = string.IsNullOrEmpty(selectedFamily) ? "Todas las familias" : selectedFamily;
            if (Button(new Rect(left + 78f, row, 174f, 29f), familyLabel + "  ▾", Tile, true, tiny))
                categoryOpen = !categoryOpen;

            int cards = Mathf.Max(1, Mathf.FloorToInt((bar.width - PaletteInset * 2 + CardGap) / 133f));
            int pages = Mathf.Max(1, Mathf.CeilToInt(visibleModels.Count / (float)cards));
            palettePage = Mathf.Clamp(palettePage, 0, pages - 1);
            if (Button(new Rect(right - 90f, row, 26f, 29f), "‹", Tile, palettePage > 0, strong)) palettePage--;
            Label(new Rect(right - 60f, row, 36f, 29f), $"{palettePage + 1}/{pages}", tiny, Muted);
            if (Button(new Rect(right - 26f, row, 26f, 29f), "›", Tile, palettePage + 1 < pages, strong)) palettePage++;

            if (Button(new Rect(right - 191f, row, 90f, 29f), "Deshacer", Tile, placement.Count > 0, tiny)) UndoPlacement();

            if (HasSelectedModel)
            {
                float cancelX = right - 289f;
                float hintX = left + 263f;
                if (Button(new Rect(cancelX, row, 90f, 29f), "Cancelar", Tile, true, tiny)) CancelPlacement();
                Label(new Rect(hintX, row, Mathf.Max(0f, cancelX - hintX - 9f), 29f),
                    placement.Hint ?? "Apunta a la grilla del suelo.", tiny, SandboxWorld.Aqua);
            }
            else
            {
                string cell = placement.CellSize.ToString("0.##", CultureInfo.GetCultureInfo("es-CO"));
                Label(new Rect(left + 263f, row, 210, 29f), $"{visibleModels.Count} modelos  ·  Celda de {cell} m", tiny, Muted);
            }

            float cardWidth = (bar.width - PaletteInset * 2 - CardGap * (cards - 1)) / cards;
            bool blockedByCategory = categoryOpen;
            bool blockedByModal = inputBlocked;
            inputBlocked = blockedByModal || blockedByCategory;
            int first = palettePage * cards;
            int limit = Mathf.Min(visibleModels.Count, first + cards);
            for (int index = first; index < limit; index++)
            {
                GridModel model = visibleModels[index];
                Rect card = new Rect(left + (index - first) * (cardWidth + CardGap), row + 38f, cardWidth, 69f);
                DrawModelCard(card, model);
            }

            inputBlocked = blockedByModal;
            if (categoryOpen && !inputBlocked) DrawCategories(bar);
        }

        private void DrawModelCard(Rect card, GridModel model)
        {
            bool selected = model == placement.Selected;
            if (Button(card, "", selected ? SelectedTile : Tile))
            {
                if (selected) CancelPlacement();
                else SelectPlacementModel(model.Id);
            }

            if (selected) Fill(new Rect(card.x, card.y, card.width, 2f), SandboxWorld.Aqua);
            GridFootprint footprint = placement.Footprint(model);
            Label(new Rect(card.x + 7f, card.y + 6f, card.width - 14f, 29f), model.Label, tiny, Text);
            PickTileGraphic(new Rect(card.x + 8f, card.y + 42f, 26f, 18f), footprint, selected ? SandboxWorld.Aqua : Muted);
            Label(new Rect(card.x + 38f, card.y + 37f, card.width - 44f, 28f),
                $"{footprint.Width} × {footprint.Depth} celdas", tiny, selected ? SandboxWorld.Aqua : Muted);
        }

        private static void PickTileGraphic(Rect area, GridFootprint footprint, Color tint)
        {
            float step = Mathf.Min(area.width / footprint.Width, area.height / footprint.Depth, 8f);
            float size = Mathf.Max(0.7f, step - 1f);
            for (int row = 0; row < footprint.Depth; row++)
            for (int column = 0; column < footprint.Width; column++)
            {
                Fill(new Rect(area.x + column * step,
                    area.y + area.height - (row + 1) * step, size, size), tint);
            }
        }

        private void DrawCategories(Rect bar)
        {
            const int columns = 3;
            int count = modelFamilies.Length + 1;
            int rows = Mathf.CeilToInt(count / (float)columns);
            float menuHeight = 42 + rows * 25f;
            Rect menu = new Rect(bar.x + PaletteInset, bar.y - menuHeight - 8f,
                Mathf.Min(735f, bar.width - PaletteInset * 2f), menuHeight);

            Fill(menu, Ink);
            Fill(new Rect(menu.x, menu.y, menu.width, 2f), SandboxWorld.Aqua);
            Label(new Rect(menu.x + 12f, menu.y + 3f, menu.width - 95f, 33f), "Familias de modelos", strong, Text);
            if (Button(new Rect(menu.xMax - 74f, menu.y + 6f, 62f, 24f), "Cerrar", Tile, true, tiny))
                categoryOpen = false;

            float width = (menu.width - 36f) / columns;
            for (int index = 0; index < count; index++)
            {
                string familyName = index == 0 ? string.Empty : modelFamilies[index - 1];
                Rect option = new Rect(menu.x + 12f + index / rows * (width + 6f),
                    menu.y + 36f + index % rows * 25f, width, 22f);
                string label = familyName.Length == 0 ? "Todas las familias" : familyName;
                if (!Button(option, label, familyName == selectedFamily ? SelectedTile : Tile, true, tiny))
                    continue;

                selectedFamily = familyName;
                visibleModels = selectedFamily.Length == 0 ? PlaceableModels :
                    PlaceableModels.Where(model => model.Family == selectedFamily).ToArray();
                palettePage = 0;
                CancelPlacement();
                break;
            }
        }
    }
}
