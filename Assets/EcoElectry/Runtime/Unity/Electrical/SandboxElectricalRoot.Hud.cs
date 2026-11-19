using System;
using EcoElectry.Application.Electrical;
using EcoElectry.Domain.Electrical;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Electrical
{
    public sealed partial class SandboxElectricalRoot
    {
        private static readonly Color Ink = new Color32(13, 25, 31, 255);
        private static readonly Color Panel = new Color32(21, 37, 43, 255);
        private static readonly Color Tile = new Color32(34, 55, 61, 255);
        private static readonly Color Hover = new Color32(52, 74, 78, 255);
        private static readonly Color Text = new Color32(236, 244, 239, 255);
        private static readonly Color Muted = new Color32(174, 194, 192, 255);
        private const float Sidebar = 346;
        private const float Header = 72;
        private const float Footer = 50;
        private const float SidebarNavigation = 37;

        private Vector2 scroll;
        private Vector2 pointer;
        private Vector2 panelPointer;
        private bool showHelp;
        private bool uiClick;
        private bool clickConsumed;
        private bool inputBlocked;
        private bool inSidebar;
        private int pointerFrame = -1;
        private float scrollContent;
        private float installationTop;
        private float routingTop;
        private float toolsTop;
        [NonSerialized] private GUIStyle regular;
        [NonSerialized] private GUIStyle strong;
        [NonSerialized] private GUIStyle large;
        [NonSerialized] private GUIStyle tiny;

        private void OnGUI()
        {
            if (Session == null) return;
            float scale = Mathf.Min(Screen.width / 1200f, Screen.height / 720f);
            if (scale <= 0) return;
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            if (regular == null) InitStyles();
            Matrix4x4 original = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));

            var viewport = new Rect(Sidebar, Header, width - Sidebar, height - Header - Footer - PlacementBarHeight);
            sandboxCamera.rect = new Rect(viewport.x / width, (height - viewport.yMax) / height,
                viewport.width / width, viewport.height / height);

            var left = new Rect(0, Header + SidebarNavigation, Sidebar, height - Header - Footer - SidebarNavigation);
            ReadPointer(scale, left);
            bool overlay = showHelp || Session.ReportOpen;
            inputBlocked = overlay;
            DrawHeader(width);
            DrawSidebar(left);
            if (!overlay && !categoryOpen)
            {
                if (!HasSelectedModel) DrawWorldLabels(viewport, scale);
                else Label(new Rect(viewport.x + 18, viewport.y + 6, 310, 24),
                    "COLOCACIÓN EN GRILLA", tiny, Text);
            }
            DrawPlacementBar(new Rect(Sidebar, viewport.yMax, width - Sidebar, PlacementBarHeight));
            DrawFooter(width, height);
            Mouse mouse = Mouse.current;
            if (!overlay && !categoryOpen && HasSelectedModel && uiClick && !clickConsumed &&
                viewport.Contains(pointer) && mouse != null && !mouse.rightButton.isPressed)
            {
                clickConsumed = true;
                TryPlaceModel(sandboxCamera.ScreenPointToRay(mouse.position.ReadValue()));
            }
            inputBlocked = false;
            if (Session.ReportOpen) DrawReport(width, height);
            else if (showHelp) DrawHelp(width, height);
            GUI.matrix = original;
        }

        private void ReadPointer(float scale, Rect sidebar)
        {
            Mouse mouse = Mouse.current;
            uiClick = Event.current.type == EventType.Repaint && mouse != null && mouse.leftButton.wasPressedThisFrame;
            if (mouse == null) return;
            Vector2 screen = mouse.position.ReadValue();
            pointer = new Vector2(screen.x, Screen.height - screen.y) / scale;
            if (Event.current.type != EventType.Repaint || pointerFrame == Time.frameCount) return;
            pointerFrame = Time.frameCount;
            clickConsumed = false;
            if (sidebar.Contains(pointer) && !showHelp && !Session.ReportOpen)
            {
                float delta = mouse.scroll.ReadValue().y;
                scroll.y = Mathf.Clamp(scroll.y - delta * 0.46f, 0, Mathf.Max(0, scrollContent - sidebar.height));
            }
        }

        private void DrawHeader(float width)
        {
            Fill(new Rect(0, 0, width, Header), Ink);
            Fill(new Rect(22, 17, 5, 39), SandboxWorld.Aqua);
            Label(new Rect(40, 9, 450, 31), "Sandbox eléctrico", large, Text);
            Label(new Rect(41, 41, 430, 17), "ECOELECTRY  /  TALLER DE INSTALACIÓN", tiny, SandboxWorld.Aqua);
            Label(new Rect(width - 587, 18, 250, 32), definition.Cases[caseIndex].Label, strong, Text);
            string cameraLabel = IsFirstPerson ? "Cámara · Primera persona" : "Cámara · Tercera persona";
            if (Button(new Rect(width - 326, 20, 202, 32), cameraLabel, Tile, !intervention, tiny)) ToggleCamera();
            if (Button(new Rect(width - 113, 20, 93, 32), "Controles", Tile)) showHelp = true;
        }

        private void DrawSidebar(Rect bounds)
        {
            Fill(new Rect(0, Header, Sidebar, SidebarNavigation), Ink);
            if (Button(new Rect(10, Header + 5, 62, 26), "Casos", Tile, true, tiny)) scroll.y = 0;
            if (Button(new Rect(77, Header + 5, 72, 26), "Montaje", Tile, true, tiny)) scroll.y = installationTop;
            if (Button(new Rect(154, Header + 5, 61, 26), "Cables", Tile, true, tiny)) scroll.y = routingTop;
            if (Button(new Rect(220, Header + 5, 117, 26), "Herramientas", Tile, true, tiny)) scroll.y = toolsTop;
            scroll.y = Mathf.Clamp(scroll.y, 0, Mathf.Max(0, scrollContent - bounds.height));
            Fill(bounds, Panel);
            GUI.BeginGroup(bounds);
            panelPointer = pointer;
            inSidebar = true;
            pointer -= bounds.position;
            pointer.y += scroll.y;
            GUI.BeginGroup(new Rect(0, -scroll.y, bounds.width, Mathf.Max(bounds.height + scroll.y, scrollContent + 100)));

            float y = 12;
            Heading(ref y, "CASOS DE PRUEBA");
            for (int i = 0; i < definition.Cases.Count; i++)
            {
                Color tint = i == caseIndex ? new Color32(38, 74, 74, 255) : Tile;
                if (Button(Row(y), definition.Cases[i].Label, tint)) LoadCase(i);
                y += 31;
            }

            y += 6;
            installationTop = y;
            Heading(ref y, "INSTALACIÓN");
            foreach (Equipment equipment in new[] { Equipment.Panel, Equipment.Battery, Equipment.Lamp })
            {
                bool installed = Session.Circuit.Installed(equipment);
                if (Button(Row(y), (installed ? "✓  " : "+  ") + EquipmentName(equipment), Tile, !installed))
                    Place(equipment);
                y += 31;
            }

            bool efficient = Session.EfficientLamp;
            if (Button(Row(y), efficient ? "Luminaria eficiente · ½ demanda" : "Luminaria estándar · cambiar", Tile,
                Session.Circuit.Installed(Equipment.Lamp))) Session.SelectEfficientLamp(!efficient);
            y += 40;

            Heading(ref y, "TABLERO Y FUENTES");
            SandboxCircuit circuit = Session.Circuit;
            if (Button(Row(y), circuit.BladesClosed ? "Cuchillas cerradas · abrir" : "Cuchillas abiertas · cerrar", Tile))
            {
                Session.SetBlades(!circuit.BladesClosed);
                RefreshWorld();
            }
            y += 31;
            if (Button(Row(y), circuit.SolarActive ? "Panel activo · aislar" : "Panel aislado · activar", Tile,
                circuit.Installed(Equipment.Panel)))
            {
                Session.SetSolar(!circuit.SolarActive);
                RefreshWorld();
            }
            y += 31;
            if (Button(Row(y), circuit.BatteryActive ? "Batería activa · aislar" : "Batería aislada · activar", Tile,
                circuit.Installed(Equipment.Battery)))
            {
                Session.SetBattery(!circuit.BatteryActive);
                RefreshWorld();
            }
            y += 31;
            if (Button(Row(y), circuit.ProtectionTripped ? "Protección disparada · rearmar" : "Protección armada",
                circuit.ProtectionTripped ? new Color32(107, 47, 54, 255) : Tile, circuit.ProtectionTripped))
            {
                Session.ResetProtection();
                RefreshWorld();
            }
            y += 40;

            routingTop = y;
            Heading(ref y, "CABLEADO");
            if (Button(Row(y), "Familia · " + (family == CableFamily.Solar ? "Fotovoltaico" : "Interior"), Tile, Session.Draft == null))
                family = family == CableFamily.Solar ? CableFamily.Interior : CableFamily.Solar;

            y += 31;
            if (Session.Draft == null)
            {
                if (Button(Row(y), choosingOrigin ? "Cerrar selección" : "Trazar un tramo", Tile))
                {
                    if (choosingOrigin) FinishIntervention();
                    else
                    {
                        CancelPlacement();
                        activeTool = null;
                        choosingOrigin = true;
                    }
                }
                y += 31;
            }

            if (choosingOrigin || Session.Draft != null)
                DrawRouting(ref y);
            y += 8;

            toolsTop = y;
            Heading(ref y, "HERRAMIENTAS");
            foreach (CableTool tool in Enum.GetValues(typeof(CableTool)))
            {
                if (Button(Row(y), ToolName(tool), activeTool == tool ? new Color32(38, 74, 74, 255) : Tile))
                    SelectTool(tool);
                y += 31;
            }

            if (activeTool.HasValue)
            {
                foreach (Terminal terminal in Enum.GetValues(typeof(Terminal)))
                {
                    Cable cable = circuit.At(terminal);
                    string end = cable == null ? "libre" : EndName(SandboxCircuit.EndAt(cable, terminal));
                    if (Button(Row(y), TerminalName(terminal) + "  ·  " + end, Tile,
                        circuit.Installed(SandboxCircuit.EquipmentFor(terminal)))) ApplyTool(terminal);
                    y += 31;
                }
            }

            if (intervention || choosingOrigin)
            {
                if (Button(Row(y), "Salir de la intervención  [Esc]", Tile)) FinishIntervention();
                y += 31;
            }

            scrollContent = y + 10;
            GUI.EndGroup();
            GUI.EndGroup();
            inSidebar = false;
            pointer = panelPointer;
            if (scrollContent > bounds.height)
            {
                float ratio = bounds.height / scrollContent;
                Fill(new Rect(bounds.xMax - 4, bounds.y + scroll.y * ratio, 3, bounds.height * ratio), Hover);
            }
        }

        private void DrawRouting(ref float y)
        {
            if (Session.Draft == null)
            {
                foreach (Terminal terminal in new[] { Terminal.SolarOut, Terminal.BatterySocket, Terminal.LightingOut })
                {
                    if (Button(Row(y), TerminalName(terminal), Tile,
                        Session.Circuit.Installed(SandboxCircuit.EquipmentFor(terminal)) &&
                        Session.Circuit.At(terminal) == null)) StartRoute(terminal, family);
                    y += 31;
                }
                return;
            }

            if (Session.Draft.From == Terminal.LightingOut)
            {
                Label(new Rect(12, y, 322, 42),
                    "Ruta completa: Suelo izq. → Muro izq. → Techo izq. → Techo der. → Muro der.", tiny, Muted);
                y += 46;
                foreach (RoutePoint point in Enum.GetValues(typeof(RoutePoint)))
                {
                    if (Button(Row(y), RouteName(point), Tile)) AddRoutePoint(point);
                    y += 31;
                }
            }

            foreach (Terminal target in new[] { Terminal.SolarIn, Terminal.StorageIn, Terminal.LampIn })
            {
                if (Button(Row(y), "Destino · " + TerminalName(target),
                    Session.Draft.To == target ? new Color32(38, 74, 74, 255) : Tile,
                    Session.Circuit.Installed(SandboxCircuit.EquipmentFor(target)))) ChooseDestination(target);
                y += 31;
            }

            Label(new Rect(12, y, 322, 32), Session.DraftError ?? "Ruta compatible", tiny, Muted);
            y += 34;
            if (Button(new Rect(12, y, 104, 26), "Deshacer punto", Tile))
            {
                Session.UndoPoint();
                RefreshWorld();
            }
            if (Button(new Rect(121, y, 104, 26), "Cancelar", Tile)) FinishIntervention();
            if (Button(new Rect(230, y, 104, 26), "Confirmar", new Color32(38, 74, 74, 255))) ConfirmRoute();
            y += 31;
        }

        private void DrawWorldLabels(Rect viewport, float scale)
        {
            var view = new Rect(viewport.x + 6, viewport.y + 35, viewport.width - 12, viewport.height - 42);
            Label(new Rect(Sidebar + 18, Header + 6, 380, 24),
                intervention ? "INTERVENCIÓN  /  PRIMERA PERSONA" :
                IsFirstPerson ? "EXPLORACIÓN  /  PRIMERA PERSONA" : "EXPLORACIÓN  /  TERCERA PERSONA", tiny, Text);

            if (activeTool == null && Session.Draft == null && !choosingOrigin)
            {
                foreach (Equipment equipment in new[] { Equipment.Panel, Equipment.Battery, Equipment.Lamp })
                {
                    bool installed = Session.Circuit.Installed(equipment);
                    string label = installed ? EquipmentName(equipment) : "+ " + EquipmentName(equipment);
                    if (Marker(world.At(equipment) + new Vector3(0, 0.78f, -0.4f), label, view, scale) && !installed)
                        Place(equipment);
                }

                if (Marker(world.At(Equipment.Board) + new Vector3(0, 1.1f, -0.2f), "Inspeccionar tablero", view, scale))
                {
                    EnterIntervention(world.At(Terminal.LightingOut));
                }
                return;
            }

            foreach (Terminal terminal in Enum.GetValues(typeof(Terminal)))
            {
                if (!Session.Circuit.Installed(SandboxCircuit.EquipmentFor(terminal))) continue;
                if (choosingOrigin && !SandboxCircuit.IsOrigin(terminal)) continue;
                if (Session.Draft != null && SandboxCircuit.IsOrigin(terminal)) continue;
                Cable cable = Session.Circuit.At(terminal);
                string label = TerminalName(terminal);
                if (activeTool != null && cable != null) label += " · " + EndName(SandboxCircuit.EndAt(cable, terminal));
                if (!Marker(world.At(terminal), label, view, scale)) continue;
                if (activeTool != null) ApplyTool(terminal);
                else if (choosingOrigin) StartRoute(terminal, family);
                else ChooseDestination(terminal);
            }

            if (Session.Draft?.From != Terminal.LightingOut) return;
            foreach (RoutePoint point in Enum.GetValues(typeof(RoutePoint)))
            {
                if (Marker(world.At(point), RouteName(point), view, scale)) AddRoutePoint(point);
            }
        }

        private bool Marker(Vector3 position, string label, Rect view, float scale)
        {
            Vector3 point = sandboxCamera.WorldToScreenPoint(position);
            if (point.z < 0.3f) return false;
            Vector2 center = new Vector2(point.x, Screen.height - point.y) / scale;
            float size = Mathf.Max(70, tiny.CalcSize(new GUIContent(label)).x + 18);
            Rect rect = new Rect(center.x - size / 2, center.y - 13, size, 26);
            if (!view.Contains(center) || !view.Contains(rect.min) || !view.Contains(rect.max)) return false;
            return Button(rect, label, Ink, true, tiny);
        }

        private void DrawFooter(float width, float height)
        {
            Fill(new Rect(0, height - Footer, width, Footer), Ink);
            if (Button(new Rect(16, height - 41, 152, 31), "Probar  18:00–20:00", new Color32(36, 100, 94, 255))) RunTest();
            if (Button(new Rect(177, height - 41, 151, 31), "Reiniciar caso", Tile)) ResetCase();
            Label(new Rect(Sidebar + 16, height - 48, width - Sidebar - 30, 21), Session.Task, tiny, SandboxWorld.Aqua);
            string status = Session.Message ?? (Session.Report == null ? "" : "Última prueba · " + FailureName(Session.Report.Failure));
            Label(new Rect(Sidebar + 16, height - 27, width - Sidebar - 30, 25), status, regular, Text);
        }

        private void DrawReport(float width, float height)
        {
            SandboxReport report = Session.Report;
            Rect box = Overlay(width, height, 470, 264);
            Label(new Rect(box.x + 20, box.y + 12, 420, 36), FailureName(report.Failure), large,
                report.Passed ? SandboxWorld.Aqua : SandboxWorld.Amber);
            string hour = ClassMinutes < 0 ? "Antes de la clase · carga solar" :
                "Clase · " + Clock(1080 + ClassMinutes);
            Label(new Rect(box.x + 20, box.y + 54, 420, 24), hour, strong, Text);
            string summary = report.Passed ? "Servicio continuo de 18:00 a 20:00." :
                "Interrupción a las " + Clock(report.InterruptionMinute) + " · " + report.Service;
            Label(new Rect(box.x + 20, box.y + 85, 424, 42), summary, regular, Text);
            string affected = report.AffectedTerminal.HasValue ? "Tramo: " + TerminalName(report.AffectedTerminal.Value) : "";
            Label(new Rect(box.x + 20, box.y + 130, 424, 24), affected, regular, Muted);
            Label(new Rect(box.x + 20, box.y + 161, 424, 26),
                $"Luz: {report.LitMinutes}/120 min     Reserva: {report.Reserve:0.##} u.", strong, Text);
            if (Button(new Rect(box.x + 20, box.yMax - 51, 245, 32),
                report.Passed ? "Cerrar informe" : "Cerrar y continuar corrección", Tile)) CloseReport();
        }

        private void DrawHelp(float width, float height)
        {
            Rect box = Overlay(width, height, 565, 416);
            Label(new Rect(box.x + 20, box.y + 14, 480, 35), "Controles del taller", large, SandboxWorld.Aqua);
            Label(new Rect(box.x + 20, box.y + 59, 520, 296),
                "WASD  ·  Caminar\nArrastra con el botón derecho  ·  Mirar\nBotón Cámara o C  ·  Cambiar de vista\nRueda  ·  Desplazar el panel lateral\nEsc  ·  Cancelar la acción o cerrar el informe\n\nPara colocar una pieza, elígela en la barra Modelos y apunta a la grilla del suelo. Pulsa R para girar y haz clic con la huella en verde. Usa Deshacer para retirar la última pieza.\n\nPara cablear, elige la familia y los bornes. Pela, fija y mide con las fuentes aisladas. Activa las fuentes y cierra las cuchillas antes de probar.",
                regular, Text);
            if (Button(new Rect(box.x + 20, box.yMax - 49, 135, 30), "Volver", Tile)) showHelp = false;
        }

        private static string Clock(int minute) => $"{minute / 60:00}:{minute % 60:00}";
        private Rect Overlay(float width, float height, float w, float h)
        {
            Rect box = new Rect(Sidebar + (width - Sidebar - w) / 2, (height - h) / 2, w, h);
            Fill(box, Ink);
            Fill(new Rect(box.x, box.y, w, 3), SandboxWorld.Aqua);
            return box;
        }

        private static Rect Row(float y) => new Rect(12, y, 322, 26);

        private void Heading(ref float y, string label)
        {
            Label(new Rect(12, y, 322, 21), label, tiny, SandboxWorld.Aqua);
            y += 25;
        }

        private bool Button(Rect rect, string label, Color color, bool available = true, GUIStyle style = null)
        {
            bool hover = available && !inputBlocked && rect.Contains(pointer);
            if (inSidebar && (panelPointer.x < 0 || panelPointer.x > Sidebar || panelPointer.y < Header + SidebarNavigation ||
                    panelPointer.y > Screen.height / GUI.matrix.m00 - Footer))
                hover = false;
            Fill(rect, hover ? Hover : color);
            Label(new Rect(rect.x + 7, rect.y + 1, rect.width - 14, rect.height - 2), label, style ?? regular,
                available ? Text : Muted);
            if (!hover || !uiClick || clickConsumed) return false;
            clickConsumed = true;
            return true;
        }

        private void InitStyles()
        {
            regular = MakeStyle(15, FontStyle.Normal);
            strong = MakeStyle(16, FontStyle.Bold);
            large = MakeStyle(25, FontStyle.Bold);
            tiny = MakeStyle(13, FontStyle.Bold);
        }

        private static GUIStyle MakeStyle(int size, FontStyle fontStyle) => new GUIStyle(GUI.skin.label)
        {
            fontSize = size, fontStyle = fontStyle, wordWrap = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset()
        };

        private static void Label(Rect rect, string content, GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            GUI.Label(rect, content, style);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color saved = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        }

        private static string EquipmentName(Equipment equipment)
        {
            switch (equipment)
            {
                case Equipment.Panel: return "Panel solar";
                case Equipment.Battery: return "Batería";
                case Equipment.Lamp: return "Luminaria del aula";
                default: return "Tablero";
            }
        }

        private static string TerminalName(Terminal terminal)
        {
            switch (terminal)
            {
                case Terminal.SolarOut: return "Salida solar";
                case Terminal.SolarIn: return "Entrada FV";
                case Terminal.StorageIn: return "Entrada batería";
                case Terminal.BatterySocket: return "Borne de batería";
                case Terminal.LightingOut: return "Salida aula";
                default: return "Borne de luminaria";
            }
        }

        private static string RouteName(RoutePoint route)
        {
            switch (route)
            {
                case RoutePoint.WallLeft: return "Muro izq.";
                case RoutePoint.CeilingLeft: return "Techo izq.";
                case RoutePoint.CeilingRight: return "Techo der.";
                case RoutePoint.WallRight: return "Muro der.";
                case RoutePoint.FloorLeft: return "Suelo izq.";
                default: return "Suelo der.";
            }
        }

        private static string EndName(EndState end)
        {
            switch (end)
            {
                case EndState.Raw: return "sin pelar";
                case EndState.Stripped: return "suelto";
                default: return "fijado";
            }
        }

        private static string ToolName(CableTool tool)
        {
            switch (tool)
            {
                case CableTool.Strip: return "Pelacables · preparar extremo";
                case CableTool.Secure: return "Destornillador · fijar extremo";
                case CableTool.Measure: return "Multímetro · continuidad";
                case CableTool.Repair: return "Corregir contacto indebido";
                default: return "Retirar tramo";
            }
        }

        private static string FailureName(Failure failure)
        {
            switch (failure)
            {
                case Failure.OpenCircuit: return "Conexión abierta";
                case Failure.ShortCircuit: return "Cortocircuito";
                case Failure.InsufficientEnergy: return "Reserva insuficiente";
                default: return "Prueba superada";
            }
        }
    }
}
