using System.Globalization;
using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal sealed class MultimeterFace
    {
        private static readonly Color Boot = new(.60f, .47f, .26f);
        private static readonly Color Panel = new(.13f, .17f, .18f);
        private static readonly Color Dark = new(.040f, .061f, .066f);
        private static readonly Color ScaleInk = new(.79f, .82f, .76f);
        private static readonly Color Highlight = new(.96f, .73f, .39f);
        private static readonly Color Lcd = new(.69f, .79f, .65f);
        private static readonly Color LcdInk = new(.093f, .17f, .13f);
        private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es");
        private static readonly int[] Segments = { 63, 6, 91, 79, 102, 109, 125, 7, 127, 111 };

        private GUIStyle label;
        private bool draggingKnob;
        private bool turnedKnob;

        public MeterMode Draw(Rect bounds, BatteryMeter meter, bool interact)
        {
            label ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, clipping = TextClipping.Clip };
            float w = bounds.width;
            float h = bounds.height;
            Round(new Rect(bounds.x + 2, bounds.y + 7, w, h), new Color(0, 0, 0, .42f), 21);
            Round(bounds, Boot, 20);
            Round(Inset(bounds, 9), Panel, 13);
            Text(new Rect(bounds.x, bounds.y + h * .045f, w, h * .065f), "MULTÍMETRO", ScaleInk, 14);

            var screen = new Rect(bounds.x + w * .123f, bounds.y + h * .142f, w * .754f, h * .275f);
            Round(new Rect(screen.x - 3, screen.y - 3, screen.width + 6, screen.height + 6), Dark, 7);
            Round(screen, meter.Mode == MeterMode.Off ? new Color(.24f, .29f, .25f) : Lcd, 4);
            if (meter.Mode != MeterMode.Off) DrawLcd(screen, meter);

            Vector2 center = new(bounds.center.x, bounds.y + h * .653f);
            float radius = Mathf.Min(w * .199f, h * .137f);
            MeterMode next = meter.Mode;
            Vector2 labelOffset = new(0, -radius * 1.61f);
            for (int i = 0; i < 3; i++)
            {
                var mode = (MeterMode)i;
                Vector2 position = center + Turn(labelOffset, i * 57f - 57f);
                var target = new Rect(position.x - 27, position.y - 11, 54, 22);
                Color ink = meter.Mode == mode ? Highlight : ScaleInk;
                if (i == 1) DrawDc(target, ink, TextAnchor.MiddleCenter);
                else Text(target, i == 0 ? "OFF" : "CONT.", ink, 12);
                if (interact && GUI.Button(target, GUIContent.none, GUIStyle.none)) next = mode;
            }
            Round(Around(center, radius + 5), Dark, radius + 5);
            Round(Around(center, radius), new Color(.25f, .30f, .31f), radius);
            Round(Around(center, radius - 4), new Color(.17f, .22f, .23f), radius - 4);
            Vector2 pointer = Turn(new Vector2(0, -radius * .85f), (int)next * 57f - 57f);
            Segment(center, center + pointer, new Color(.09f, .13f, .13f), 13);
            Segment(center + pointer * .54f, center + pointer, Highlight, 3);
            Round(Around(center, 4), ScaleInk, 4);
            if (interact) next = ReadKnob(center, radius, next);
            else draggingKnob = false;

            for (int i = 0; i < 2; i++)
            {
                Vector2 port = Port(bounds, i);
                Round(Around(port, 9), i == 0 ? new Color(.40f, .43f, .43f) : new Color(.78f, .28f, .22f), 9);
                Round(Around(port, 5f), Dark, 5f);
                Text(new Rect(port.x - 25, port.y - 30, 50, 18), i == 0 ? "COM" : "V/Ω", ScaleInk, 12);
            }
            return next;
        }

        private MeterMode ReadKnob(Vector2 center, float radius, MeterMode mode)
        {
            Event input = Event.current;
            Vector2 offset = input.mousePosition - center;
            if (input.type == EventType.MouseDown && input.button == 0 && offset.sqrMagnitude < radius * radius)
            {
                draggingKnob = true;
                turnedKnob = false;
                input.Use();
            }
            else if (draggingKnob && input.type == EventType.MouseDrag)
            {
                if (offset.sqrMagnitude > 36)
                {
                    turnedKnob = true;
                    float direction = Mathf.Atan2(offset.x, -offset.y) * Mathf.Rad2Deg;
                    mode = direction < -27f ? MeterMode.Off : direction > 27f ? MeterMode.Continuity : MeterMode.DirectVoltage;
                }
                input.Use();
            }
            else if (draggingKnob && input.type == EventType.MouseUp && input.button == 0)
            {
                draggingKnob = false;
                if (!turnedKnob && offset.sqrMagnitude < radius * radius)
                    mode = (MeterMode)(((int)mode + 1) % 3);
                input.Use();
            }
            return mode;
        }

        private void DrawLcd(Rect rect, BatteryMeter meter)
        {
            var annunciator = new Rect(rect.x + 5, rect.y + 4, rect.width - 10, 16);
            if (meter.Mode == MeterMode.DirectVoltage) DrawDc(annunciator, LcdInk, TextAnchor.MiddleLeft);
            else Text(annunciator, "CONTINUIDAD", LcdInk, 11, TextAnchor.MiddleLeft);
            bool validReading = meter.Display is MeterDisplay.Voltage or MeterDisplay.ReversePolarity or MeterDisplay.SameTerminal;
            string digits = validReading ? meter.DisplayVoltage.ToString("0.00", Spanish) : "-";
            var readout = new Rect(rect.x + 9, rect.y + 22, rect.width - 27, rect.height - 45);
            DrawDigits(readout, digits);
            if (validReading)
                Text(new Rect(rect.xMax - 22, readout.yMax - 18, 16, 18), "V", LcdInk, 14);

            string status = meter.Display switch
            {
                MeterDisplay.Stabilizing => "ESTABILIZANDO",
                MeterDisplay.Voltage => "LECTURA FIJADA",
                MeterDisplay.ReversePolarity => "POLARIDAD INVERTIDA",
                MeterDisplay.SameTerminal => "MISMO TERMINAL",
                MeterDisplay.WrongMode => "MODO DE PRUEBA",
                _ => "SIN CONTACTO"
            };
            Text(new Rect(rect.x + 5, rect.yMax - 20, rect.width - 10, 16), status, LcdInk, 10, TextAnchor.MiddleLeft);
            if (meter.Display == MeterDisplay.Stabilizing)
            {
                float progress = Mathf.Clamp01(1 - (float)(meter.Remaining / BatteryMeter.StabilizationSeconds));
                Round(new Rect(rect.x + 6, rect.yMax - 5, progress * (rect.width - 12), 2), LcdInk, 0);
            }
        }

        private static void DrawDigits(Rect rect, string value)
        {
            float digitWidth = Mathf.Min(rect.height * .56f, rect.width / 5.6f);
            float gap = digitWidth * .16f;
            float width = value.Length * (digitWidth + gap) - gap;
            for (int i = 0; i < value.Length; i++)
            {
                float x = rect.x + (rect.width - width) / 2 + i * (digitWidth + gap);
                float y = rect.y;
                float h = rect.height;
                float w = digitWidth;
                float stroke = Mathf.Max(2.3f, digitWidth * .13f);
                char digit = value[i];
                if (digit == ',')
                {
                    Segment(new Vector2(x + w * .42f, y + h * .9f), new Vector2(x + w * .28f, y + h), LcdInk, stroke);
                    continue;
                }
                if (digit == '-')
                {
                    Segment(new Vector2(x + .15f * w, y + .5f * h), new Vector2(x + .85f * w, y + .5f * h), LcdInk, stroke);
                    continue;
                }
                int lit = Segments[digit - '0'];
                if ((lit & 1) != 0) Segment(new Vector2(x + 3, y), new Vector2(x + w - 3, y), LcdInk, stroke);
                if ((lit & 2) != 0) Segment(new Vector2(x + w, y + 3), new Vector2(x + w, y + h / 2 - 3), LcdInk, stroke);
                if ((lit & 4) != 0) Segment(new Vector2(x + w, y + h / 2 + 3), new Vector2(x + w, y + h - 3), LcdInk, stroke);
                if ((lit & 8) != 0) Segment(new Vector2(x + 3, y + h), new Vector2(x + w - 3, y + h), LcdInk, stroke);
                if ((lit & 16) != 0) Segment(new Vector2(x, y + h / 2 + 3), new Vector2(x, y + h - 3), LcdInk, stroke);
                if ((lit & 32) != 0) Segment(new Vector2(x, y + 3), new Vector2(x, y + h / 2 - 3), LcdInk, stroke);
                if ((lit & 64) != 0) Segment(new Vector2(x + 3, y + h / 2), new Vector2(x + w - 3, y + h / 2), LcdInk, stroke);
            }
        }

        public static Vector2 Port(Rect rect, int index) =>
            new(rect.center.x + rect.width * (index == 0 ? -.18f : .18f), rect.yMax - rect.height * .088f);

        internal void DrawDc(Rect rect, Color color, TextAnchor anchor)
        {
            // The fallback Unity UI font lacks the DC symbol.
            float x = anchor == TextAnchor.MiddleLeft ? rect.x : rect.center.x - 10;
            Text(new Rect(x, rect.y, 10, rect.height), "V", color, 12);
            float y = rect.center.y;
            Segment(new Vector2(x + 11, y - 2), new Vector2(x + 20, y - 2), color, 1.25f);
            for (int i = 0; i < 3; i++)
                Segment(new Vector2(x + 11 + i * 3, y + 2), new Vector2(x + 12.7f + i * 3, y + 2), color, 1.2f);
        }

        private void Text(Rect rect, string content, Color color, int size, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            label.fontSize = size;
            label.alignment = anchor;
            label.normal.textColor = color;
            GUI.Label(rect, content, label);
        }

        private static Vector2 Turn(Vector2 vector, float degrees) => Quaternion.Euler(0, 0, degrees) * vector;
        private static Rect Around(Vector2 point, float radius) => new(point.x - radius, point.y - radius, radius * 2, radius * 2);
        private static Rect Inset(Rect rect, float margin) => new(rect.x + margin, rect.y + margin, rect.width - 2 * margin, rect.height - 2 * margin);

        internal static void Round(Rect rect, Color color, float radius)
        {
            if (Event.current.type != EventType.Repaint) return;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, radius);
        }

        internal static void Segment(Vector2 start, Vector2 end, Color color, float width)
        {
            if (Event.current.type != EventType.Repaint) return;
            Matrix4x4 previous = GUI.matrix;
            Vector2 delta = end - start;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, start);
            Round(new Rect(start.x, start.y - width * .5f, delta.magnitude + width * .35f, width), color, width * .35f);
            GUI.matrix = previous;
        }
    }
}
