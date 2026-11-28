using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal sealed class BatteryReferenceSheet
    {
        private static readonly Color Shadow = new(.026f, .041f, .047f);
        private static readonly Color Paper = new(.87f, .85f, .75f);
        private static readonly Color Card = new(.65f, .60f, .47f);
        private static readonly Color Ink = new(.17f, .25f, .25f);
        private static readonly (string Range, string Capacity)[] Rows =
        {
            ("1,60–1,48 V", "77–100 %"),
            ("1,47–1,34 V", "50–75 %"),
            ("1,33–1,21 V", "25–49 %"),
            ("1,20–1,13 V", "10–24 %"),
            ("1,12–1,08 V", "0–9 %")
        };
        private GUIStyle writing;

        public bool Draw(Rect rail, bool open, bool interact)
        {
            writing ??= new GUIStyle(GUI.skin.label) { clipping = TextClipping.Clip };
            float width = rail.width - 14;
            float height = Mathf.Min(rail.height - 34, 290);
            var stack = new Rect(rail.x + 9, rail.y + 21, width - 4, Mathf.Min(rail.height - 20, 186));
            MultimeterFace.Round(new Rect(stack.x + 4, stack.y + 6, stack.width, stack.height), Shadow, 3);
            MultimeterFace.Round(stack, Card, 2);
            MultimeterFace.Round(new Rect(stack.x + 4, stack.y - 5, stack.width - 11, stack.height - 2), new Color(.73f, .73f, .63f), 2);

            if (!open)
            {
                var tab = new Rect(rail.x + 11, rail.y + 3, width * .85f, 52);
                MultimeterFace.Round(tab, Paper, 2);
                Text(new Rect(tab.x + 8, tab.y + 8, tab.width - 16, 21), "Manual de baterías", 14, true);
                Text(new Rect(tab.x + 8, tab.y + 29, tab.width - 16, 16), "Abrir hoja", 11, false);
                for (int i = 0; i < 3; i++)
                    MultimeterFace.Round(new Rect(stack.x + 12, stack.y + 51 + i * 13, stack.width * (.66f - .09f * i), 1), Ink * .72f, 0);
                return interact && GUI.Button(tab, GUIContent.none, GUIStyle.none);
            }

            var sheet = new Rect(rail.x + 2, rail.y + 7, width + 7, height);
            MultimeterFace.Round(new Rect(sheet.x + 3, sheet.y + 5, sheet.width, sheet.height), Shadow, 2);
            MultimeterFace.Round(sheet, Paper, 1);
            float rowHeight = Mathf.Min(34, (height - 34) / 6);
            float column = (sheet.width - 18) * .56f;
            for (int row = 0; row <= Rows.Length; row++)
            {
                float y = sheet.y + 17 + row * rowHeight;
                if (row % 2 == 1)
                    MultimeterFace.Round(new Rect(sheet.x + 8, y, sheet.width - 16, rowHeight), new Color(.79f, .80f, .71f), 0);
                string voltage = row == 0 ? "Voltaje" : Rows[row - 1].Range;
                string charge = row == 0 ? "Carga aprox." : Rows[row - 1].Capacity;
                Text(new Rect(sheet.x + 13, y, column, rowHeight), voltage, 13, row == 0);
                Text(new Rect(sheet.x + 13 + column, y, sheet.width - 19 - column, rowHeight), charge, 13, row == 0);
            }
            return !(interact && GUI.Button(sheet, GUIContent.none, GUIStyle.none));
        }

        private void Text(Rect rect, string value, int size, bool bold)
        {
            writing.normal.textColor = Ink;
            writing.fontSize = size;
            writing.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            writing.alignment = TextAnchor.MiddleLeft;
            GUI.Label(rect, value, writing);
        }
    }
}
