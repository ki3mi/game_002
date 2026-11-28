using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    public sealed partial class SurvivalScene
    {
        private void DrawRepair()
        {
            GUI.enabled = !paused;
            if (repairView == RepairView.Box) DrawFuseWorkbench();
            else DrawRepairNotes();
            GUI.enabled = true;
        }

        private void DrawFuseWorkbench()
        {
            const float pad = 14;
            float width = Mathf.Min(Screen.width - 28, 1500);
            float height = Mathf.Min(Screen.height - 82, 790);
            var panel = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
            if (!DrawRepairHeader(panel, "Caja de fusibles · Limpieza")) return;
            GUI.Label(new Rect(panel.x + pad, panel.y + 43, width - 30, 22), RepairStatus(), noteStyle);

            float sidebarWidth = Mathf.Clamp(width * .213f, 158, 272);
            float instrumentWidth = Mathf.Clamp(width * .22f, 132, 285);
            var sidebar = new Rect(panel.xMax - pad - sidebarWidth, panel.y + 73, sidebarWidth, height - 91);
            var center = new Rect(panel.x + pad + instrumentWidth + 10, panel.y + 73,
                sidebar.x - (panel.x + pad + instrumentWidth + 10) - 12, height - 86);
            float viewHeight = Mathf.Min(center.height - 86, center.width * 660 / 960);
            var scene = new Rect(center.x, center.y + Mathf.Max(0, (center.height - 86 - viewHeight) * .5f), center.width, viewHeight);
            Paint(new Rect(center.x, center.y, center.width, center.height - 80), Bench);
            GUI.DrawTexture(scene, preview.Texture, ScaleMode.StretchToFill, false);

            float meterWidth = Mathf.Min(instrumentWidth, (height - 195) / 1.42f);
            var meter = new Rect(panel.x + pad, scene.y + 2, meterWidth, meterWidth * 1.42f);
            MeterMode selectedMode = meterFace.Draw(meter, repair.Meter, !paused && repair.SupplyCut);
            if (!paused && repair.SupplyCut) repair.Meter.SetMode(selectedMode);
            DrawLead(MultimeterFace.Port(meter, 0), preview.ProjectCable(0, scene), new Color(.10f, .13f, .15f));
            DrawLead(MultimeterFace.Port(meter, 1), preview.ProjectCable(1, scene), RedLead);

            GUI.enabled = !paused && repair.SupplyCut && !repair.Completed && !preview.IsMovingRepairPart;
            if (GUI.Button(new Rect(meter.x, meter.yMax + 13, instrumentWidth, 30),
                driverEquipped ? "Destornillador equipado" : "Equipar destornillador"))
            {
                driverEquipped = !driverEquipped;
                StowRepairProbes();
                repairFeedback = "";
            }
            if (GUI.Button(new Rect(meter.x, meter.yMax + 49, instrumentWidth, 27), "Guardar puntas")) StowRepairProbes();
            if (repair.Position(FuseRepair.SlotCount) == FusePosition.Inventory && !repair.CoverOn &&
                GUI.Button(new Rect(meter.x, meter.yMax + 82, instrumentWidth, 30), "Repuesto al banco"))
                SelectFuse(FuseRepair.SlotCount);

            GUI.enabled = !paused;
            if (!paused && preview.HandlePointer(scene, out var hit, heldProbe.HasValue)) InteractWithFuseBox(hit);
            if (Event.current.type == EventType.Repaint) DrawFuseTags(scene);
            GUI.Label(new Rect(center.x + 5, center.yMax - 65, center.width - 10, 39),
                string.IsNullOrEmpty(repairFeedback) ? WorkbenchHint() : repairFeedback, labelStyle);
            GUI.Label(new Rect(center.x + 5, center.yMax - 27, center.width - 110, 21),
                "Clic para manipular · Arrastra para girar · Rueda para acercar", noteStyle);
            if (GUI.Button(new Rect(center.xMax - 103, center.yMax - 29, 103, 24), "Centrar vista"))
                preview.ResetView();
            DrawFuseSheet(sidebar);
        }

        private void DrawFuseTags(Rect scene)
        {
            if (!repair.CoverOn)
                for (int slot = 0; slot < FuseRepair.SlotCount; slot++)
                    if (repair.At((FusePosition)slot) < 0)
                        DrawRepairTag(preview.ProjectFuseSeat(slot, scene) + new Vector2(0, -15),
                            $"Ranura {(char)('A' + slot)}", Ink, 67, scene);

            for (int i = 0; i < repair.Cartridges.Count; i++)
            {
                FusePosition position = repair.Position(i);
                if (position is not (FusePosition.Bench or FusePosition.Tray)) continue;
                bool measured = repair.TryGetReading(i, out bool conducts);
                string name = PartName(i);
                string tag = measured ? $"{name} · {(conducts ? "OK" : "OL")}" : name;
                DrawRepairTag(preview.ProjectFuse(i, scene) + new Vector2(0, 25), tag, measured && !conducts ? Alarm : Power,
                    i == FuseRepair.SlotCount ? 94 : 57, scene);
                if (position != FusePosition.Bench) continue;
                for (int end = 0; end < 2; end++)
                    DrawRepairTag(preview.ProjectFuseTerminal(i, end, scene) + new Vector2(0, -18),
                        $"{end + 1}", Brass, 17, scene);
            }
            if (repair.Meter.GetContact(MeterProbe.Black) == null)
                DrawRepairTag(preview.ProjectDock(0, scene) + new Vector2(-2, 20), "COM", heldProbe == MeterProbe.Black ? Brass : Ink, 38, scene);
            if (repair.Meter.GetContact(MeterProbe.Red) == null)
                DrawRepairTag(preview.ProjectDock(1, scene) + new Vector2(9, 20), "V/Ω", heldProbe == MeterProbe.Red ? Brass : Ink, 38, scene);
            for (int i = 0; i < FuseRepair.ScrewCount; i++)
                if (!repair.IsFastened(i))
                    DrawRepairTag(preview.ProjectScrewRest(i, scene) + new Vector2(0, 14),
                        $"T{i + 1}", Muted, 21, scene);
        }

        private void DrawRepairTag(Vector2 point, string text, Color color, float width, Rect scene)
        {
            if (scene.Contains(point)) DrawTag(point, text, color, width);
        }

        private void DrawFuseSheet(Rect area)
        {
            Paint(new Rect(area.x + 6, area.y + 8, area.width - 18, 29), Rule);
            Paint(new Rect(area.x + 12, area.y + 4, area.width - 21, 30), new Color(.60f, .60f, .49f));
            if (GUI.Button(new Rect(area.x + 17, area.y + 1, area.width - 17, 30),
                taskSheetOpen ? "Referencias  ▸" : "Referencias  ▾"))
                taskSheetOpen = !taskSheetOpen;
            if (!taskSheetOpen)
            {
                DrawRepairLog(area.x, area.y + 42, area.width);
                return;
            }
            var manual = new Rect(area.x, area.y + 38, area.width, 224);
            Paint(manual, new Color(.83f, .81f, .69f));
            GUIStyle document = new(labelStyle);
            document.normal.textColor = new Color(.138f, .177f, .149f);
            float x = manual.x + 12;
            float w = manual.width - 24;
            GUI.Label(new Rect(x, manual.y + 10, w, 32), "Ficha de mantenimiento", document);
            for (int i = 0; i < FuseRepair.SlotCount; i++)
                GUI.Label(new Rect(x, manual.y + 40 + i * 24, w, 23),
                    $"{(char)('A' + i)}     {repair.RequiredReference(i)}", document);
            Paint(new Rect(x, manual.y + 117, w, 1), new Color(.43f, .48f, .37f));
            GUI.Label(new Rect(x, manual.y + 129, w, 83),
                "Selector en continuidad\nPitido = continuidad\nOL = circuito abierto", document);
        }

        private bool DrawRepairHeader(Rect panel, string title)
        {
            Paint(panel, Surface);
            Paint(new Rect(panel.x, panel.y, panel.width, 2), Power);
            GUI.Label(new Rect(panel.x + 14, panel.y + 10, panel.width - 122, 34), title, titleStyle);
            if (!GUI.Button(new Rect(panel.xMax - 102, panel.y + 15, 85, 24), "Esc  Salir")) return true;
            CloseInspection();
            return false;
        }

        private void DrawRepairNotes()
        {
            bool journal = repairView == RepairView.Journal;
            float width = Mathf.Min(Screen.width - 36, journal ? 466 : 700);
            float height = Mathf.Min(Screen.height - 105, journal ? 394 : 509);
            var panel = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
            string title = repairView switch
            {
                RepairView.Journal => "Tareas de mantenimiento",
                RepairView.Inventory => "Inventario · fusible de repuesto",
                _ => "Caja de repuestos · Limpieza"
            };
            if (!DrawRepairHeader(panel, title)) return;
            if (journal)
            {
                GUI.Label(new Rect(panel.x + 17, panel.y + 46, width - 34, 23),
                    repair.Completed ? RepairStatus() : "Cuarto de Limpieza · Revisar la caja mural de fusibles", labelStyle);
                DrawRepairLog(panel.x + 17, panel.y + 76, width - 34);
                if (repair.Position(FuseRepair.SlotCount) == FusePosition.Inventory &&
                    GUI.Button(new Rect(panel.x + 17, panel.yMax - 52, width - 34, 34), "Inspeccionar fusible del inventario"))
                    ShowRepairView(RepairView.Inventory);
                return;
            }

            float available = Mathf.Min(height - 132, (width - 32) * 660 / 960);
            float pictureWidth = available * 960 / 660;
            var picture = new Rect(panel.center.x - pictureWidth / 2, panel.y + 49, pictureWidth, available);
            GUI.DrawTexture(picture, preview.Texture, ScaleMode.StretchToFill, false);
            if (!paused) preview.HandlePointer(picture, out _);
            var note = new Rect(panel.x + 17, picture.yMax + 8, width - 34, 62);
            if (repairView == RepairView.Spares)
                GUI.Label(note, repair.ClueRead ? $"La caja está vacía. {repairDefinition.Clue}" :
                    "La caja está vacía. Diagnostica el fusible abierto para registrar la nota de reposición.", labelStyle);
            else
            {
                GUI.Label(new Rect(note.x, note.y, note.width, 26), "Gira la pieza para leer la referencia impresa.", noteStyle);
                if (GUI.Button(new Rect(note.x, note.y + 28, 134, 27), "Volver a tareas"))
                    ShowRepairView(RepairView.Journal);
            }
        }

        private void DrawRepairLog(float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, width, 25), "Lecturas guardadas", labelStyle);
            for (int i = 0; i < repair.Cartridges.Count; i++)
            {
                bool tested = repair.TryGetReading(i, out bool conducts);
                string value = tested ? conducts ? "0 Ω · Continuidad" : "OL · Abierto" : "Pendiente";
                var row = new Rect(x, y + 32 + i * 27, width, 24);
                Paint(row, Bench);
                GUI.Label(new Rect(x + 5, row.y + 2, width - 10, 20), $"{PartName(i)}  ·  {value}", noteStyle);
            }
            if (repair.ClueRead)
                GUI.Label(new Rect(x, y + 149, width, 85), repair.Completed ? "Caja cerrada. El suministro sigue cortado." :
                    repairDefinition.Clue, noteStyle);
        }

        private string RepairStatus() => repair.Completed ? "Completada · suministro cortado" :
            repair.SupplyCut ? "Suministro cortado · tablero de Mantenimiento" : "Corte de suministro pendiente";

        private string WorkbenchHint()
        {
            if (repair.Completed) return "Encargo terminado. La caja quedó cerrada y el colegio sigue sin suministro.";
            if (!repair.SupplyCut) return "Comprueba el corte en el tablero de Mantenimiento.";
            if (heldProbe.HasValue) return "Haz clic en un extremo metálico del fusible situado en el banco.";
            if (repair.CoverOn)
            {
                if (repair.ReadyToClose) return "Equipa el destornillador y haz clic en las cuatro fijaciones.";
                for (int i = 0; i < FuseRepair.ScrewCount; i++)
                    if (repair.IsFastened(i)) return "Equipa el destornillador y haz clic en cada tornillo visible.";
                return "Haz clic en la tapa para retirarla.";
            }
            if (repair.ReadyToClose) return "Haz clic en la tapa retirada para cerrar la caja.";
            int open = repair.DiagnosedOpen;
            if (open >= 0 && repair.Position(FuseRepair.SlotCount) == FusePosition.World)
                return $"Fusible abierto · {repair.Cartridges[open].Reference}. Revisa la caja de repuestos de Limpieza.";
            if (repair.At(FusePosition.Bench) >= 0)
                return "En continuidad, haz clic en cada punta y luego en un extremo distinto. Para montar, haz clic en su ranura vacía.";
            return "Haz clic en un fusible para llevarlo al banco de pruebas.";
        }

        private static string PartName(int index) => index == FuseRepair.SlotCount ? "Repuesto" : $"{(char)('A' + index)}";
    }
}
