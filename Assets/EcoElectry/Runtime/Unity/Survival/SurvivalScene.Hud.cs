using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    public sealed partial class SurvivalScene
    {
        private static readonly Color Surface = new(.046f, .077f, .091f, .98f);
        private static readonly Color Rule = new(.24f, .34f, .35f);
        private static readonly Color Ink = new(.89f, .92f, .85f);
        private static readonly Color Muted = new(.69f, .76f, .72f);
        private static readonly Color Power = new(.44f, .78f, .67f);
        private static readonly Color Alarm = new(.91f, .49f, .30f);
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle noteStyle;
        private GUIStyle centeredStyle;
        private Vector2 panelScroll;

        private void OnGUI()
        {
            if (!IsReady || externalMenu || (tutorialMode && !tutorialStarted) ||
                gameObject.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene()) return;
            PrepareStyles();
            if (session.IsDefeated || loading)
            {
                DrawResult();
                return;
            }
            if (session.IsInspecting) DrawCompartmentInspection();
            else if (openCase != null) DrawCaseInspection();
            else if (inspectedPart != null)
            {
                Rect panel = ModalRect(138);
                Paint(panel, Surface);
                GUI.Label(new Rect(panel.x + 15, panel.y + 9, panel.width - 30, 32), "Repuestos del taller", titleStyle);
                GUI.Label(new Rect(panel.x + 15, panel.y + 48, panel.width - 30, 53), inspectedPart, labelStyle);
                if (GUI.Button(new Rect(panel.xMax - 106, panel.yMax - 31, 90, 24), "Esc · Salir")) CloseInspection();
            }
            DrawStatus();
            if (paused) DrawPause();
        }

        private void PrepareStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Ink;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            labelStyle.normal.textColor = Ink;
            noteStyle = new GUIStyle(labelStyle) { fontSize = 12 };
            noteStyle.normal.textColor = Muted;
            centeredStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
        }

        private void DrawStatus()
        {
            if (tutorialMode && !equipped)
            {
                var guide = new Rect(18, Screen.height - 53, 330, 35);
                Paint(guide, Surface);
                GUI.Label(new Rect(28, guide.y + 6, 314, 22), "Tutorial · Recoge la linterna sobre la mesa.", labelStyle);
                if (aimedFlashlight) DrawAction("E  Recoger linterna");
                return;
            }

            var batteryPanel = new Rect(18, Screen.height - 84, 195, 66);
            Paint(batteryPanel, Surface);
            Paint(new Rect(batteryPanel.x, batteryPanel.y, 2, batteryPanel.height), Power);
            string batteryStatus = session.TotalCharge <= 0 ? "Linterna agotada" : session.IsInspecting
                ? "Compartimento abierto" : session.HasBeam ? "Linterna encendida" : "Linterna apagada";
            GUI.Label(new Rect(28, batteryPanel.y + 4, 177, 22), batteryStatus, labelStyle);

            float bars = (float)session.TotalCharge * 2f;
            for (int i = 0; i < 4; i++)
            {
                var bar = new Rect(29 + i * 29, batteryPanel.y + 29, 24, 12);
                Paint(bar, Rule);
                Paint(new Rect(bar.x + 1, bar.y + 1, (bar.width - 2) * Mathf.Clamp01(bars - i), bar.height - 2), Power);
            }
            GUI.Label(new Rect(28, batteryPanel.y + 44, 180, 18), $"F  Luz     I  Repuestos ({session.Inventory.Count})", noteStyle);
            if (tutorialMode)
            {
                string task = !tutorialMounted ? "I  Abre el compartimento y monta las dos baterías." :
                    !tutorialLightTried ? "F  Prueba la linterna." :
                    tutorialTasks ? "F: luz · I: baterías · E: inspecciona una caja del estante" : "J  Tareas del taller";
                var tasks = new Rect(18, batteryPanel.y - 33, !tutorialMounted || tutorialTasks ? 358 : 215, 29);
                Paint(tasks, Surface);
                GUI.Label(new Rect(28, tasks.y + 4, tasks.width - 18, 22), task, noteStyle);
            }

            if (session.ShowFear)
            {
                var fearBox = new Rect(Screen.width - 206, 18, 188, 52);
                Paint(fearBox, Surface);
                float fear = (float)session.Fear;
                GUI.Label(new Rect(fearBox.x + 11, fearBox.y + 3, 170, 21), $"Miedo   {Mathf.RoundToInt(fear)} / 100", labelStyle);
                Paint(new Rect(fearBox.x + 11, fearBox.y + 28, 166, 11), Rule);
                Paint(new Rect(fearBox.x + 12, fearBox.y + 29, 164 * fear / 100, 9), Alarm);
            }

            if (Time.unscaledTime < messageUntil)
            {
                var notice = new Rect((Screen.width - 300) * .5f, Screen.height - 64, 300, 36);
                Paint(notice, Surface);
                GUI.Label(notice, message, centeredStyle);
            }

            if (paused || Inspecting || explorer.IsImageMenuOpen) return;
            GUI.Label(new Rect(Screen.width - 79, Screen.height - 35, 71, 19), "P  Pausa", noteStyle);
            string prompt = aimedBattery != null && aimedBattery.Available ? "E  Recoger batería" :
                aimedCase != null ? "E  Inspeccionar caja" : aimedPart != null ? "E  Inspeccionar repuesto" : null;
            if (prompt == null) return;
            DrawAction(prompt);
        }

        private void DrawAction(string prompt)
        {
            var action = new Rect((Screen.width - 196) * .5f, Screen.height * .5f + 18, 196, 28);
            Paint(action, Surface);
            GUI.Label(action, prompt, centeredStyle);
        }

        private void DrawCaseInspection()
        {
            Paint(new Rect(0, 0, Screen.width, Screen.height), new Color(.011f, .021f, .027f, .89f));
            float width = Mathf.Min(1040, Screen.width - 32);
            float height = Mathf.Min(502, Screen.height - 118);
            var panel = new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
            Paint(panel, Surface);
            Paint(new Rect(panel.x, panel.y, width, 2), Power);
            GUI.Label(new Rect(panel.x + 17, panel.y + 10, width - 36, 33), openCase.Title, titleStyle);
            GUI.Label(new Rect(panel.x + 17, panel.y + 47, width - 36, 22), "Arrastra para girar · Rueda para acercar", noteStyle);

            float sideWidth = Mathf.Min(342, width * .46f);
            var modelArea = new Rect(panel.x + 16, panel.y + 69, width - sideWidth - 36, height - 116);
            float aspect = (float)preview.Texture.width / preview.Texture.height;
            float previewWidth = Mathf.Min(modelArea.width, modelArea.height * aspect);
            var picture = new Rect(modelArea.center.x - previewWidth / 2, modelArea.center.y - previewWidth / aspect / 2,
                previewWidth, previewWidth / aspect);
            Paint(modelArea, new Color(.062f, .093f, .11f));
            GUI.DrawTexture(picture, preview.Texture);
            if (!paused && preview.HandlePointer(picture, out InspectionPreview.Hit hit) &&
                hit.Kind == InspectionPreview.TargetKind.Latch && !openCase.IsOpen)
            {
                openCase.Open(session);
                preview.SetCaseOpen(true, openCase.Battery != null && openCase.Battery.Available);
            }

            bool wasEnabled = GUI.enabled;
            GUI.enabled &= !paused;
            if (GUI.Button(new Rect(panel.x + 16, panel.yMax - 37, 137, 26), "Restablecer vista")) preview.ResetView();
            if (GUI.Button(new Rect(panel.xMax - 149, panel.yMax - 37, 132, 26), "Volver · Esc")) CloseInspection();
            if (openCase != null)
            {
                GUILayout.BeginArea(new Rect(panel.xMax - sideWidth - 14, panel.y + 66, sideWidth, height - 111));
                panelScroll = GUILayout.BeginScrollView(panelScroll);
                DrawCaseOptions();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            GUI.enabled = wasEnabled;
        }

        private void DrawCaseOptions()
        {
            if (!string.IsNullOrEmpty(openCase.Description)) GUILayout.Label(openCase.Description, noteStyle);
            if (!openCase.IsOpen) GUILayout.Label("Gira la caja y pulsa el cierre metálico para abrirla.", labelStyle);
            else if (openCase.Battery != null && openCase.Battery.Available)
            {
                if (GUILayout.Button("Recoger batería", GUILayout.Height(30)) && Collect(openCase.Battery)) CloseInspection();
            }
            else GUILayout.Label("La caja está vacía.", labelStyle);
        }

        private void DrawPause()
        {
            Paint(new Rect(0, 0, Screen.width, Screen.height), new Color(.015f, .025f, .030f, .85f));
            GUILayout.BeginArea(ModalRect(172), GUI.skin.box);
            GUILayout.Label("Partida en pausa", titleStyle);
            if (GUILayout.Button("Continuar · P", GUILayout.Height(29))) SetPaused(false);
            if (continuationScene.Length > 0 && GUILayout.Button("Continuar al nivel 1", GUILayout.Height(27)))
                LoadScene(continuationScene, false);
            GUILayout.EndArea();
        }

        private void DrawResult()
        {
            Paint(new Rect(0, 0, Screen.width, Screen.height), new Color(.012f, .017f, .023f, .97f));
            GUILayout.BeginArea(ModalRect(148), GUI.skin.box);
            GUILayout.Label(loading ? "Cargando…" : "Te has desmayado", titleStyle);
            if (!loading && GUILayout.Button("Reintentar", GUILayout.Height(32))) LoadScene(gameObject.scene.path, true);
            if (Time.unscaledTime < messageUntil && !loading) GUILayout.Label(message, noteStyle);
            GUILayout.EndArea();
        }

        private static Rect ModalRect(float height) => new((Screen.width - 350) * .5f, (Screen.height - height) * .5f, 350, height);

        private static void Paint(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
