using System.Globalization;
using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    public sealed partial class SurvivalScene
    {
        private static readonly Color Bench = new(.062f, .093f, .110f);
        private static readonly Color Brass = new(.93f, .71f, .38f);
        private static readonly Color RedLead = new(.84f, .30f, .24f);
        private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es");
        private readonly MultimeterFace meterFace = new();
        private readonly BatteryReferenceSheet referenceSheet = new();
        private GUIStyle tagStyle;

        private void DrawCompartmentInspection()
        {
            BatteryInspection inspection = session.Inspection;
            bool interactive = !paused && Time.timeScale > 0;
            Paint(new Rect(0, 0, Screen.width, Screen.height), new Color(.012f, .023f, .030f, .91f));

            float width = Mathf.Min(1540, Screen.width - 42);
            float height = Mathf.Min(804, Screen.height - 104);
            var frame = new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
            MultimeterFace.Round(frame, Surface, 12);
            Paint(new Rect(frame.x + 22, frame.y + 54, width - 44, 1), Rule);
            GUI.Label(new Rect(frame.x + 21, frame.y + 10, width - 260, 36), "Compartimento de la linterna", titleStyle);
            GUI.Label(new Rect(frame.xMax - 280, frame.y + 17, 262, 22), "Arrastra para girar · Rueda para acercar", noteStyle);

            float leftWidth = Mathf.Clamp(width * .216f, 174, 326);
            float rightWidth = Mathf.Clamp(width * .184f, 154, 276);
            var center = new Rect(frame.x + leftWidth, frame.y + 63, width - leftWidth - rightWidth, height - 115);
            float aspect = (float)preview.Texture.width / preview.Texture.height;
            float pictureWidth = Mathf.Min(center.width, center.height * aspect);
            var picture = new Rect(center.center.x - pictureWidth / 2, center.center.y - pictureWidth / aspect / 2,
                pictureWidth, pictureWidth / aspect);
            MultimeterFace.Round(center, Bench, 7);
            GUI.DrawTexture(picture, preview.Texture);

            float instrumentHeight = Mathf.Min(395, center.height - 96);
            float instrumentWidth = Mathf.Min(leftWidth - 46, instrumentHeight * .68f);
            var instrument = new Rect(frame.x + (leftWidth - instrumentWidth) / 2, center.y + 23, instrumentWidth, instrumentHeight);
            MeterMode selectedMode = meterFace.Draw(instrument, inspection.Meter, interactive);
            if (selectedMode != inspection.Meter.Mode) inspection.Meter.SetMode(selectedMode);

            if (interactive && preview.HandlePointer(picture, out InspectionPreview.Hit selection, heldProbe.HasValue))
                InteractWithCompartment(selection);

            DrawLead(MultimeterFace.Port(instrument, 0), preview.ProjectCable(0, picture), new Color(.10f, .13f, .15f));
            DrawLead(MultimeterFace.Port(instrument, 1), preview.ProjectCable(1, picture), RedLead);
            DrawBatteryLabels(inspection, picture);
            if (inspection.Meter.GetContact(MeterProbe.Black).Cell == null)
                DrawTag(preview.ProjectDock(0, picture) + new Vector2(-4, 15), "COM −", heldProbe == MeterProbe.Black ? Brass : Ink, 52);
            if (inspection.Meter.GetContact(MeterProbe.Red).Cell == null)
                DrawTag(preview.ProjectDock(1, picture) + new Vector2(12, 15), "V/Ω +", heldProbe == MeterProbe.Red ? Brass : Ink, 56);

            var rail = new Rect(center.xMax + 11, center.y + 34, rightWidth - 25, center.height - 41);
            manualOpen = referenceSheet.Draw(rail, manualOpen, interactive);

            bool oldEnabled = GUI.enabled;
            GUI.enabled &= interactive;
            if (GUI.Button(new Rect(instrument.x, instrument.yMax + 13, instrument.width, 29), "Guardar puntas")) StoreProbes();
            DrawMeterHint(new Rect(frame.x + 19, instrument.yMax + 48, leftWidth - 30, 42), inspection);
            GUI.Label(new Rect(center.x + 8, center.yMax - 25, center.width - 16, 24), HandlingHint(inspection), centeredStyle);

            float buttons = frame.yMax - 38;
            if (GUI.Button(new Rect(center.x, buttons, 139, 25), "Restablecer vista")) preview.ResetView();
            if (GUI.Button(new Rect(frame.xMax - 309, buttons, 111, 25), "Cancelar · Esc"))
            {
                CloseInspection();
                GUI.enabled = oldEnabled;
                return;
            }
            GUI.enabled &= !preview.IsMovingCell;
            if (GUI.Button(new Rect(frame.xMax - 191, buttons, 172, 25), "Confirmar montaje")) ConfirmCompartment();
            GUI.enabled = oldEnabled;
        }

        private void DrawBatteryLabels(BatteryInspection inspection, Rect picture)
        {
            GUI.Label(new Rect(picture.x + 16, picture.y + 5, picture.width - 32, 18), "Bandeja de baterías", centeredStyle);
            for (int i = 0; i < inspection.Tray.Count; i++)
            {
                BatteryCell cell = inspection.Tray[i];
                Vector2 labelPosition = preview.ProjectTray(i, picture) + new Vector2(0, 27);
                string caption = i < inspection.ReserveCount ? $"Repuesto {i + 1}" : $"Retirada {(char)('A' + i - inspection.ReserveCount)}";
                DrawTag(labelPosition, caption, cell != null && cell == selectedCell ? Brass : Ink, 87);
                if (cell != null && inspection.Meter.TryGetReading(cell, out double volts))
                    DrawTag(labelPosition + new Vector2(0, -48), FormatVoltage(volts), Power, 56);
            }
            for (int i = 0; i < FlashlightState.SlotCount; i++)
            {
                BatteryCell cell = inspection.Slots[i];
                Vector2 position = preview.ProjectSlot(i, picture);
                string suffix = cell == null ? " · Libre" :
                    inspection.Meter.TryGetReading(cell, out double volts) ? $" · {FormatVoltage(volts)}" : "";
                float side = i == 0 ? 1 : -1;
                DrawTag(position + new Vector2(0, 29 * side), $"{(char)('A' + i)}{suffix}",
                    cell == null && selectedCell != null ? Brass : Ink, suffix.Length > 0 ? 90 : 24);
                if (!heldProbe.HasValue || cell == null || preview.IsMovingCell) continue;
                DrawTag(preview.ProjectTerminal(cell, BatteryTerminal.Negative, picture), "−", Ink, 16);
                DrawTag(preview.ProjectTerminal(cell, BatteryTerminal.Positive, picture), "+", Brass, 16);
            }
        }

        private void DrawTag(Vector2 point, string text, Color color, float width)
        {
            tagStyle ??= new GUIStyle(centeredStyle) { fontSize = 11, fontStyle = FontStyle.Bold, wordWrap = false, clipping = TextClipping.Clip };
            tagStyle.normal.textColor = color;
            var rect = new Rect(point.x - width / 2, point.y - 9, width, 18);
            MultimeterFace.Round(rect, new Color(.04f, .065f, .078f, .88f), 3);
            GUI.Label(rect, text, tagStyle);
        }

        private string HandlingHint(BatteryInspection inspection)
        {
            if (preview.IsMovingCell) return "Colocando batería…";
            if (heldProbe.HasValue) return "Pulsa un terminal visible de una batería instalada.";
            if (selectedCell != null)
                return inspection.Slots[0] != null && inspection.Slots[1] != null
                    ? "Retira una batería para liberar su ranura."
                    : "Elige una ranura libre.";
            return "Pulsa una batería para moverla o una punta para medir.";
        }

        private void DrawMeterHint(Rect rect, BatteryInspection inspection)
        {
            if (inspection.Meter.Display == MeterDisplay.WrongMode ||
                (inspection.Meter.Display == MeterDisplay.Off && inspection.CanMeasure))
            {
                GUI.Label(new Rect(rect.x, rect.y, 61, 18), "Selecciona", noteStyle);
                meterFace.DrawDc(new Rect(rect.x + 64, rect.y + 1, 23, 15), Brass, TextAnchor.MiddleLeft);
                GUI.Label(new Rect(rect.x + 89, rect.y, rect.width - 89, 18), "para comparar", noteStyle);
                GUI.Label(new Rect(rect.x, rect.y + 15, rect.width, 18), "baterías", noteStyle);
                return;
            }
            string hint = inspection.Meter.Display switch
            {
                MeterDisplay.ReversePolarity => "Corrige la polaridad para conservar la lectura.",
                MeterDisplay.SameTerminal => "Las dos puntas están en el mismo terminal.",
                MeterDisplay.Voltage => "La lectura queda junto a esta batería.",
                MeterDisplay.Stabilizing => "Mantén ambas puntas en contacto.",
                _ when !inspection.CanMeasure => "Instala dos baterías para comparar.",
                _ => "Pulsa una punta y después un terminal."
            };
            GUI.Label(rect, hint, noteStyle);
        }

        private static string FormatVoltage(double volts) => volts.ToString("0.00", Spanish) + " V";

        private static void DrawLead(Vector2 start, Vector2 end, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
            Vector2 a = start + new Vector2(9, 46);
            Vector2 b = end + new Vector2(-48, 34);
            Vector2 previous = start;
            for (int i = 1; i <= 23; i++)
            {
                float t = i / 23f;
                float u = 1 - t;
                Vector2 next = u * u * u * start + 3 * u * u * t * a + 3 * u * t * t * b + t * t * t * end;
                MultimeterFace.Segment(previous, next, new Color(.30f, .36f, .38f), 4.0f);
                MultimeterFace.Segment(previous, next, color, 2.65f);
                previous = next;
            }
        }
    }
}
