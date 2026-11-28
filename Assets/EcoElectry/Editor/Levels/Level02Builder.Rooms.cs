using UnityEditor;
using UnityEngine;

namespace EcoElectry.Editor.Levels
{
    internal sealed partial class Level02Builder
    {
        private void BuildRoomsAndProps()
        {
            FurnishAtriumAndCorridors();
            FurnishClassroom202();
            FurnishScienceLabAndPrep();
            FurnishSecretariaAndPrincipal();
            FurnishComputerLab();
            FurnishAudiovisualRoom();
            FurnishElectricalRoom();
            FurnishRestrooms2F();
            FurnishArchiveAndStorage();
        }

        private void FurnishAtriumAndCorridors()
        {
            // Benches along AtriumGallery overlooking the courtyard
            Bench(atriumGalleryGroup, new Vector3(-8.10f, YFloor2, 19.82f), 0f);
            Bench(atriumGalleryGroup, new Vector3(-2.60f, YFloor2, 19.82f), 0f);
            Bench(atriumGalleryGroup, new Vector3(8.10f, YFloor2, 19.82f), 0f);

            // Upper landing wall sub-panel + emergency luminaire + switch + outlets
            Transform subpanel = InstantiateResourcePrefab("TablerosElectrico", "TableroDistribucion",
                atriumGalleryGroup, new Vector3(5.85f, YFloor2 + 1.25f, 22.68f), 180f);
            subpanel.name = "Subtablero Galeria 2P";

            InstantiateResourcePrefab("LuminariaInterior", "LuminariaEmergencia",
                atriumGalleryGroup, new Vector3(3.44f, YFloor2 + 2.85f, 22.68f), 180f);
            InstantiateResourcePrefab("Interruptor", "InterruptorPared",
                atriumGalleryGroup, new Vector3(5.32f, YFloor2 + 1.22f, 22.69f), 180f);
            InstantiateResourcePrefab("Tomacorriente", "TomacorrienteDoble",
                atriumGalleryGroup, new Vector3(-4.20f, YFloor2 + 0.35f, 19.51f), 0f);
            InstantiateResourcePrefab("Tomacorriente", "TomacorrienteDoble",
                atriumGalleryGroup, new Vector3(7.40f, YFloor2 + 0.35f, 19.51f), 0f);

            // Presence sensors in main corridor intersections
            InstantiateResourcePrefab("Sensor", "SensorPresencia",
                westWingGroup, new Vector3(-12.30f, YFloor2 + StoryHeight - 0.02f, 32.40f), 0f);
            InstantiateResourcePrefab("Sensor", "SensorPresencia",
                eastWingGroup, new Vector3(12.30f, YFloor2 + StoryHeight - 0.02f, 32.40f), 0f);

            // ServiceShortcut wall conduits, junction boxes, and emergency light
            for (int i = 0; i < 4; i++)
            {
                Transform tray = InstantiateResourcePrefab("Canaleta", "CanaletaIndustrial",
                    serviceShortcutGroup, new Vector3(5.30f, YFloor2 + 2.65f, 24.10f + i * 2.15f), 90f);
                tray.localScale = new Vector3(1.05f, 1f, 1f);
            }
            InstantiateResourcePrefab("CajaConexion", "CajaDerivacion",
                serviceShortcutGroup, new Vector3(5.28f, YFloor2 + 2.40f, 26.20f), 90f);
            InstantiateResourcePrefab("CajaConexion", "CajaEmpalme",
                serviceShortcutGroup, new Vector3(5.28f, YFloor2 + 2.40f, 29.50f), 90f);
            InstantiateResourcePrefab("LuminariaInterior", "LuminariaEmergencia",
                serviceShortcutGroup, new Vector3(8.08f, YFloor2 + 2.75f, 27.60f), 270f);
        }

        private void FurnishClassroom202()
        {
            Transform room = Group("Aula202", classroomsAndLabsGroup);
            FluorescentFixture(room, new Vector3(-17.20f, YFloor2 + 3.65f, 22.80f), 90f, lit: true);
            FluorescentFixture(room, new Vector3(-20.20f, YFloor2 + 3.65f, 22.80f), 90f, lit: false);

            InstantiateResourcePrefab("Pizarra", "PizarraBlanca", room, new Vector3(-22.26f, YFloor2 + 1.05f, 24.45f), 90f);
            InstantiateResourcePrefab("EscritorioDocente", "EscritorioBasico", room, new Vector3(-20.80f, YFloor2, 20.85f), 85f);
            InstantiateResourcePrefab("Silla", "SillaDocente", room, new Vector3(-21.48f, YFloor2, 20.85f), 90f);

            float[] cols = { -19.20f, -17.65f, -16.10f };
            float[] rows = { 21.10f, 22.80f, 24.50f };
            for (int c = 0; c < cols.Length; c++)
            {
                for (int r = 0; r < rows.Length; r++)
                {
                    float angle = (c == 2 && r == 0) ? -76f : -90f;
                    InstantiateResourcePrefab("CarpetaEscolar", "CarpetaConBandeja",
                        room, new Vector3(cols[c], YFloor2, rows[r]), angle);
                }
            }

            InstantiateResourcePrefab("Ventilador", "VentiladorPared", room, new Vector3(-18.20f, YFloor2 + 2.55f, 26.08f), 180f);
            InstantiateResourcePrefab("Interruptor", "InterruptorPared", room, new Vector3(-14.11f, YFloor2 + 1.22f, 23.55f), 270f);
            InstantiateResourcePrefab("Tomacorriente", "TomacorrienteDoble", room, new Vector3(-22.29f, YFloor2 + 0.35f, 20.60f), 90f);
            NoticeBoard(room, new Vector3(-17.40f, YFloor2 + 1.62f, 26.08f), 180f);
            LowCabinet(room, "Armario Aula 202", new Vector3(-15.20f, YFloor2, 25.75f), 0f);
        }

        private void FurnishScienceLabAndPrep()
        {
            Transform lab = Group("LabCiencias201", classroomsAndLabsGroup);
            FluorescentFixture(lab, new Vector3(-16.80f, YFloor2 + 3.65f, 28.60f), 90f, lit: true);
            FluorescentFixture(lab, new Vector3(-20.20f, YFloor2 + 3.65f, 28.60f), 90f, lit: false);
            FluorescentFixture(lab, new Vector3(-16.80f, YFloor2 + 3.65f, 32.10f), 90f, lit: true);
            FluorescentFixture(lab, new Vector3(-20.20f, YFloor2 + 3.65f, 32.10f), 90f, lit: true, intensity: 1.85f);

            // Slowly spinning ceiling fan in the Science Lab
            Transform fan = InstantiateResourcePrefab("Ventilador", "VentiladorTecho",
                lab, new Vector3(-18.40f, YFloor2 + StoryHeight - 0.02f, 30.30f), 0f);
            ConfigureCeilingFan(fan, startOn: true, rpm: 28f);

            // Chalkboard + short-throw projector + teacher demonstration bench
            InstantiateResourcePrefab("Pizarra", "PizarraTiza", lab, new Vector3(-18.40f, YFloor2 + 1.02f, 26.33f), 0f);
            InstantiateResourcePrefab("Proyector", "ProyectorCortoAlcance", lab, new Vector3(-18.40f, YFloor2 + 2.48f, 26.65f), 180f);
            LabBench(lab, new Vector3(-18.40f, YFloor2, 27.65f), width: 2.80f, hasSink: true);

            // 4 student laboratory island stations with stools and industrial outlets
            foreach (float lx in new[] { -20.15f, -16.65f })
            {
                foreach (float lz in new[] { 29.85f, 32.15f })
                {
                    LabBench(lab, new Vector3(lx, YFloor2, lz), width: 2.10f, hasSink: true);
                    InstantiateResourcePrefab("Silla", "SillaApilable", lab, new Vector3(lx - 0.55f, YFloor2, lz + 0.72f), 180f);
                    InstantiateResourcePrefab("Silla", "SillaApilable", lab, new Vector3(lx + 0.55f, YFloor2, lz + 0.72f), 172f);
                }
            }

            // Lab safety breaker & subpanel near corridor wall
            InstantiateResourcePrefab("TablerosElectrico", "TableroDistribucion",
                lab, new Vector3(-14.12f, YFloor2 + 1.30f, 32.80f), 270f);
            InstantiateResourcePrefab("Interruptor", "InterruptorSeccionador",
                lab, new Vector3(-14.13f, YFloor2 + 1.42f, 31.85f), 270f);
            InstantiateResourcePrefab("Tomacorriente", "TomacorrienteIndustrial",
                lab, new Vector3(-22.29f, YFloor2 + 1.12f, 30.30f), 90f);

            // Prep Room 201-B ([-22.40, -14.00] x [34.40, 39.20])
            Transform prep = Group("Preparacion201B", classroomsAndLabsGroup);
            FluorescentFixture(prep, new Vector3(-18.20f, YFloor2 + 3.65f, 36.80f), 0f, lit: true, intensity: 1.65f);
            EmergencyPilot(prep, new Vector3(-14.12f, YFloor2 + 2.45f, 35.40f), 90f);

            Bookcase(prep, "Estanteria reactivos 1", new Vector3(-21.95f, YFloor2, 35.60f), 90f);
            Bookcase(prep, "Estanteria reactivos 2", new Vector3(-21.95f, YFloor2, 38.00f), 90f);
            Bookcase(prep, "Estanteria instrumental", new Vector3(-16.20f, YFloor2, 38.82f), 180f);

            // Workbench with spare electrical components and portable projector
            LabBench(prep, new Vector3(-19.50f, YFloor2, 38.45f), width: 2.40f, hasSink: false);
            InstantiateResourcePrefab("Proyector", "ProyectorPortatil", prep, new Vector3(-20.15f, YFloor2 + 0.89f, 38.45f), 18f);
            InstantiateResourcePrefab("Cable", "CableFase", prep, new Vector3(-19.25f, YFloor2 + 0.89f, 38.42f), 35f);
            InstantiateResourcePrefab("ComponentesTableros", "Contactor", prep, new Vector3(-18.65f, YFloor2 + 0.94f, 38.45f), 0f);
            InstantiateResourcePrefab("ComponentesTableros", "InterruptorDiferencial", prep, new Vector3(-18.48f, YFloor2 + 0.94f, 38.45f), 0f);
            InstantiateResourcePrefab("CajaConexion", "CajaDerivacion", prep, new Vector3(-18.85f, YFloor2 + 0.89f, 38.55f), 12f);
        }

        private void FurnishSecretariaAndPrincipal()
        {
            // Secretaria ([14.00, 22.40] x [19.40, 25.40])
            Transform sec = Group("Secretaria", adminSuiteGroup);
            InstantiateResourcePrefab("LuminariaInterior", "PanelLED", sec, new Vector3(16.80f, YFloor2 + StoryHeight - 0.01f, 22.40f), 0f);
            InstantiateResourcePrefab("LuminariaInterior", "PanelLED", sec, new Vector3(20.20f, YFloor2 + StoryHeight - 0.01f, 22.40f), 0f);
            FluorescentFixture(sec, new Vector3(18.20f, YFloor2 + 3.65f, 22.40f), 0f, lit: true, intensity: 2.1f);

            // Reception counter facing the entrance door
            Transform counter = Group("Mostrador de atencion", sec);
            counter.localPosition = new Vector3(16.90f, YFloor2, 22.20f);
            Box(counter, "Cuerpo", new Vector3(0f, 0.54f, 0f), new Vector3(0.56f, 1.08f, 3.10f), matWood, solid: true);
            Box(counter, "Cubierta", new Vector3(0f, 1.10f, 0f), new Vector3(0.66f, 0.05f, 3.18f), matLaminate, solid: true);

            InstantiateResourcePrefab("EscritorioDocente", "EscritorioConCajones", sec, new Vector3(19.60f, YFloor2, 21.30f), 270f);
            InstantiateResourcePrefab("Silla", "SillaDocente", sec, new Vector3(20.32f, YFloor2, 21.30f), 270f);
            InstantiateResourcePrefab("Computadora", "ComputadoraCompacta", sec, new Vector3(19.58f, YFloor2 + 0.75f, 21.30f), 270f);
            InstantiateResourcePrefab("Monitor", "MonitorBasico", sec, new Vector3(19.52f, YFloor2 + 0.75f, 21.82f), 250f);

            Bench(sec, new Vector3(15.20f, YFloor2, 20.05f), 0f);
            Bookcase(sec, "Estanteria expedientes", new Vector3(21.95f, YFloor2, 24.10f), 270f);
            InstantiateResourcePrefab("Ventilador", "VentiladorPared", sec, new Vector3(18.20f, YFloor2 + 2.55f, 25.28f), 180f);
            InstantiateResourcePrefab("Interruptor", "InterruptorPared", sec, new Vector3(14.11f, YFloor2 + 1.22f, 23.25f), 90f);

            // Principal Office ([14.00, 22.40] x [25.40, 31.40])
            Transform dir = Group("Direccion", adminSuiteGroup);
            FluorescentFixture(dir, new Vector3(18.40f, YFloor2 + 3.65f, 28.40f), 0f, lit: true, intensity: 2.0f);

            InstantiateResourcePrefab("EscritorioDocente", "EscritorioModular", dir, new Vector3(20.30f, YFloor2, 29.20f), 270f);
            InstantiateResourcePrefab("Silla", "SillaDocente", dir, new Vector3(21.15f, YFloor2, 29.20f), 270f);
            InstantiateResourcePrefab("Computadora", "ComputadoraAllInOne", dir, new Vector3(20.28f, YFloor2 + 0.75f, 29.20f), 270f);

            // Meeting table + 4 chairs
            MeetingTable(dir, new Vector3(16.75f, YFloor2, 28.40f), 1.90f, 1.05f);
            InstantiateResourcePrefab("Silla", "SillaDocente", dir, new Vector3(16.25f, YFloor2, 27.55f), 0f);
            InstantiateResourcePrefab("Silla", "SillaDocente", dir, new Vector3(17.25f, YFloor2, 27.55f), 0f);
            InstantiateResourcePrefab("Silla", "SillaDocente", dir, new Vector3(16.25f, YFloor2, 29.25f), 180f);
            InstantiateResourcePrefab("Silla", "SillaDocente", dir, new Vector3(17.25f, YFloor2, 29.25f), 180f);

            Bookcase(dir, "Vitrina Direccion 1", new Vector3(16.80f, YFloor2, 31.02f), 180f);
            Bookcase(dir, "Vitrina Direccion 2", new Vector3(19.60f, YFloor2, 31.02f), 180f);
            LowCabinet(dir, "Credenza Direccion", new Vector3(21.75f, YFloor2, 26.55f), 270f);
            NoticeBoard(dir, new Vector3(14.12f, YFloor2 + 1.65f, 26.80f), 270f);
        }

        private void FurnishComputerLab()
        {
            Transform comp = Group("LabInformatica204", adminSuiteGroup);
            FluorescentFixture(comp, new Vector3(16.80f, YFloor2 + 3.65f, 33.60f), 0f, lit: true);
            FluorescentFixture(comp, new Vector3(20.10f, YFloor2 + 3.65f, 33.60f), 0f, lit: false);
            FluorescentFixture(comp, new Vector3(16.80f, YFloor2 + 3.65f, 36.90f), 0f, lit: true, intensity: 1.9f);
            FluorescentFixture(comp, new Vector3(20.10f, YFloor2 + 3.65f, 36.90f), 0f, lit: false);

            // Smartboard + ceiling projector at south wall of ComputerLab (z = 31.54)
            InstantiateResourcePrefab("Pizarra", "PizarraDigital", comp, new Vector3(18.20f, YFloor2 + 1.12f, 31.54f), 0f);
            InstantiateResourcePrefab("Proyector", "ProyectorTecho", comp, new Vector3(18.20f, YFloor2 + StoryHeight - 0.02f, 33.80f), 180f);
            InstantiateResourcePrefab("Ventilador", "VentiladorTecho", comp, new Vector3(18.20f, YFloor2 + StoryHeight - 0.02f, 36.40f), 0f);

            // Instructor workstation
            InstantiateResourcePrefab("EscritorioDocente", "EscritorioConCajones", comp, new Vector3(15.65f, YFloor2, 32.55f), 0f);
            InstantiateResourcePrefab("Silla", "SillaDocente", comp, new Vector3(15.65f, YFloor2, 31.92f), 0f);
            InstantiateResourcePrefab("Monitor", "MonitorUltrawide", comp, new Vector3(15.65f, YFloor2 + 0.75f, 32.52f), 180f);

            // 6 double computer desks (12 PCs!) + floor cable raceways
            float[] cols = { 16.50f, 19.85f };
            float[] rows = { 34.10f, 35.75f, 37.40f };
            for (int r = 0; r < rows.Length; r++)
            {
                InstantiateResourcePrefab("Canaleta", "CanaletaBajoPiso",
                    comp, new Vector3(18.20f, YFloor2, rows[r] - 0.35f), 0f).localScale = new Vector3(2.90f, 1f, 1f);

                for (int c = 0; c < cols.Length; c++)
                {
                    if (c == 1 && r == 2) continue; // Reserve NE corner for UPS & Server rack!
                    float x = cols[c];
                    float z = rows[r];
                    InstantiateResourcePrefab("CarpetaEscolar", "CarpetaDoble", comp, new Vector3(x, YFloor2, z), 180f);

                    string pcModel = (r + c) % 2 == 0 ? "ComputadoraAllInOne" : "ComputadoraEscritorio";
                    Transform pc1 = InstantiateResourcePrefab("Computadora", pcModel, comp, new Vector3(x - 0.30f, YFloor2 + 0.80f, z - 0.05f), 180f);
                    pc1.localScale = Vector3.one * 0.72f;
                    Transform pc2 = InstantiateResourcePrefab("Computadora", "ComputadoraAllInOne", comp, new Vector3(x + 0.30f, YFloor2 + 0.80f, z - 0.05f), 180f);
                    pc2.localScale = Vector3.one * 0.72f;
                }
            }

            // Server & UPS Power Backup Corner (NE corner: x ~ 20.6..22.0, z ~ 37.2..39.0)
            Box(comp, "Alfombra aislante UPS", new Vector3(20.90f, YFloor2 + 0.01f, 38.25f),
                new Vector3(2.30f, 0.02f, 1.40f), matRubber);
            InstantiateResourcePrefab("TablerosElectrico", "TableroGeneralDistrib",
                comp, new Vector3(20.85f, YFloor2 + 1.15f, 39.04f), 180f);
            InstantiateResourcePrefab("Inversor", "InversorHibrido",
                comp, new Vector3(19.95f, YFloor2 + 1.18f, 38.98f), 180f);
            InstantiateResourcePrefab("Bateria", "Bateria3", comp, new Vector3(20.85f, YFloor2 + 0.02f, 38.55f), 180f);
            InstantiateResourcePrefab("Bateria", "Bateria2", comp, new Vector3(21.85f, YFloor2 + 0.02f, 38.55f), 180f);
            EmergencyPilot(comp, new Vector3(20.85f, YFloor2 + 2.45f, 39.06f), 180f);
        }

        private void FurnishAudiovisualRoom()
        {
            // Main Projection Hall (x in [-10.60, -1.20], z in [35.80, 42.60])
            FluorescentFixture(audiovisualGroup, new Vector3(-7.80f, YFloor2 + 3.65f, 39.20f), 90f, lit: false);
            FluorescentFixture(audiovisualGroup, new Vector3(-4.00f, YFloor2 + 3.65f, 39.20f), 90f, lit: true, intensity: 1.35f);

            // Projection screen + smartboard on West wall (x = -10.48) + ceiling projector with faint cool projection beam!
            Box(audiovisualGroup, "Marco pantalla proyeccion", new Vector3(-10.46f, YFloor2 + 2.10f, 39.20f),
                new Vector3(0.05f, 2.05f, 3.40f), matIron);
            Box(audiovisualGroup, "Tela proyeccion", new Vector3(-10.43f, YFloor2 + 2.10f, 39.20f),
                new Vector3(0.02f, 1.90f, 3.24f), matPaper);
            InstantiateResourcePrefab("Proyector", "ProyectorTecho",
                audiovisualGroup, new Vector3(-5.20f, YFloor2 + StoryHeight - 0.02f, 39.20f), 270f);
            Spotlight(audiovisualGroup, "Haz standby proyector", new Vector3(-5.32f, YFloor2 + StoryHeight - 0.38f, 39.20f),
                new Vector3(-1f, -0.28f, 0f), new Color(0.55f, 0.70f, 0.95f, 1f), 2.1f, 6.8f, 44f, shadows: false);

            // Acoustic cork panels along North wall
            for (int i = 0; i < 4; i++)
            {
                Box(audiovisualGroup, "Panel acustico", new Vector3(-9.10f + i * 2.25f, YFloor2 + 1.85f, 42.47f),
                    new Vector3(1.65f, 1.25f, 0.04f), matCork);
            }

            // Rows of seating facing West screen
            float[] seatCols = { -7.90f, -6.10f, -4.30f, -2.65f };
            float[] seatRows = { 37.60f, 39.20f, 40.80f };
            foreach (float sx in seatCols)
            {
                foreach (float sz in seatRows)
                {
                    InstantiateResourcePrefab("Silla", "SillaApilable",
                        audiovisualGroup, new Vector3(sx, YFloor2, sz), -90f);
                }
            }

            // Audio / Projection Control Booth (x in [-1.20, 1.80], z in [35.80, 42.60])
            Transform booth = Group("Cabina de proyeccion", audiovisualGroup);
            FluorescentFixture(booth, new Vector3(0.30f, YFloor2 + 3.65f, 39.20f), 90f, lit: true, intensity: 1.55f);
            InstantiateResourcePrefab("EscritorioDocente", "EscritorioBasico", booth, new Vector3(-0.35f, YFloor2, 40.10f), -90f);
            InstantiateResourcePrefab("Silla", "SillaDocente", booth, new Vector3(0.32f, YFloor2, 40.10f), -90f);
            InstantiateResourcePrefab("Monitor", "MonitorUltrawide", booth, new Vector3(-0.38f, YFloor2 + 0.73f, 40.10f), 90f);
            InstantiateResourcePrefab("ControladorCarga", "ControladorMPPT", booth, new Vector3(1.66f, YFloor2 + 1.38f, 40.90f), 270f);
            InstantiateResourcePrefab("TablerosElectrico", "TableroDistribucion", booth, new Vector3(1.66f, YFloor2 + 1.25f, 39.50f), 270f);
            InstantiateResourcePrefab("Cable", "CableNeutro", booth, new Vector3(0.95f, YFloor2, 41.80f), 20f);
            EmergencyPilot(booth, new Vector3(0.30f, YFloor2 + 2.55f, 42.46f), 180f);
        }

        private void FurnishElectricalRoom()
        {
            // Main 2nd-Floor Electrical Substation ([1.80, 10.60] x [35.80, 42.60])
            FluorescentFixture(electricalRoomGroup, new Vector3(4.60f, YFloor2 + 3.65f, 38.80f), 0f, lit: true, intensity: 1.95f);
            FluorescentFixture(electricalRoomGroup, new Vector3(8.10f, YFloor2 + 3.65f, 38.80f), 0f, lit: false);
            EmergencyPilot(electricalRoomGroup, new Vector3(6.20f, YFloor2 + 2.85f, 42.46f), 180f);
            InstantiateResourcePrefab("LuminariaInterior", "LuminariaEmergencia",
                electricalRoomGroup, new Vector3(1.92f, YFloor2 + 2.65f, 39.20f), 90f);

            // Insulating floor mat + Main Automation Cabinet (TableroControlAutom)
            Box(electricalRoomGroup, "Alfombra dielectrica", new Vector3(6.20f, YFloor2 + 0.01f, 41.15f),
                new Vector3(4.80f, 0.02f, 2.20f), matRubber);
            Transform mainCab = InstantiateResourcePrefab("TablerosElectrico", "TableroControlAutom",
                electricalRoomGroup, new Vector3(6.20f, YFloor2 + 0.05f, 41.75f), 180f);
            mainCab.name = "Tablero Principal 2P";

            // Secondary & General distribution boards + inverters + MPPT controllers on East & North walls
            InstantiateResourcePrefab("TablerosElectrico", "TableroGeneralDistrib",
                electricalRoomGroup, new Vector3(8.55f, YFloor2 + 1.15f, 42.38f), 180f);
            InstantiateResourcePrefab("TablerosElectrico", "TableroDistribucion",
                electricalRoomGroup, new Vector3(10.42f, YFloor2 + 1.25f, 38.40f), 270f);
            InstantiateResourcePrefab("Inversor", "InversorString",
                electricalRoomGroup, new Vector3(10.40f, YFloor2 + 1.28f, 40.20f), 270f);
            InstantiateResourcePrefab("ControladorCarga", "ControladorMPPTDoble",
                electricalRoomGroup, new Vector3(10.42f, YFloor2 + 1.28f, 41.35f), 270f);
            InstantiateResourcePrefab("Interruptor", "InterruptorSeccionador",
                electricalRoomGroup, new Vector3(4.10f, YFloor2 + 1.42f, 42.44f), 180f);

            // Battery bank + industrial floor fan
            InstantiateResourcePrefab("Bateria", "Bateria3", electricalRoomGroup, new Vector3(9.45f, YFloor2 + 0.02f, 41.85f), 180f);
            InstantiateResourcePrefab("Bateria", "Bateria3", electricalRoomGroup, new Vector3(3.25f, YFloor2 + 0.02f, 41.85f), 180f);
            InstantiateResourcePrefab("Ventilador", "VentiladorIndustrial",
                electricalRoomGroup, new Vector3(2.90f, YFloor2, 37.20f), 42f);

            // Overhead cable trays feeding into the main cabinet
            for (int i = 0; i < 4; i++)
            {
                InstantiateResourcePrefab("Canaleta", "CanaletaIndustrial",
                    electricalRoomGroup, new Vector3(3.20f + i * 2.0f, YFloor2 + 2.85f, 42.35f), 0f);
            }
            InstantiateResourcePrefab("CajaConexion", "CajaEstancaExterior",
                electricalRoomGroup, new Vector3(4.10f, YFloor2 + 2.25f, 42.44f), 180f);
            InstantiateResourcePrefab("Tomacorriente", "TomacorrienteIndustrial",
                electricalRoomGroup, new Vector3(1.92f, YFloor2 + 1.10f, 37.80f), 90f);

            // Electrician workbench with terminal blocks, breakers, and cable coils
            LabBench(electricalRoomGroup, new Vector3(7.90f, YFloor2, 36.85f), width: 2.30f, hasSink: false);
            InstantiateResourcePrefab("ComponentesTableros", "BornerasCableado",
                electricalRoomGroup, new Vector3(7.35f, YFloor2 + 0.93f, 36.85f), 0f);
            InstantiateResourcePrefab("Interruptor", "InterruptorTermomagnetico",
                electricalRoomGroup, new Vector3(7.85f, YFloor2 + 0.94f, 36.85f), 0f);
            InstantiateResourcePrefab("Cable", "CableTierra",
                electricalRoomGroup, new Vector3(8.55f, YFloor2 + 0.89f, 36.85f), -25f);
            Spotlight(electricalRoomGroup, "Luz banco tecnico", new Vector3(7.90f, YFloor2 + 2.45f, 36.85f),
                Vector3.down, new Color(1.0f, 0.84f, 0.62f, 1f), 2.9f, 4.8f, 92f, shadows: false);
        }

        private void FurnishRestrooms2F()
        {
            FluorescentFixture(restroomsGroup, new Vector3(-7.90f, YFloor2 + 3.65f, 24.50f), 0f, lit: true, intensity: 1.85f);
            FluorescentFixture(restroomsGroup, new Vector3(-7.90f, YFloor2 + 3.65f, 27.40f), 0f, lit: false);

            // Privacy screen & ceramic washbasins
            Box(restroomsGroup, "Mampara de entrada", new Vector3(-7.90f, YFloor2 + 1.00f, 24.25f),
                new Vector3(1.55f, 2.00f, 0.12f), matCeramic, solid: true);
            for (int i = 0; i < 3; i++)
            {
                float z = 24.40f + i * 1.15f;
                Transform sink = Group($"Lavamanos {i + 1}", restroomsGroup);
                sink.localPosition = new Vector3(-10.22f, YFloor2, z);
                Box(sink, "Pedestal", new Vector3(0f, 0.42f, 0f), new Vector3(0.22f, 0.84f, 0.22f), matCeramic, solid: true);
                Box(sink, "Bacha", new Vector3(0.05f, 0.86f, 0f), new Vector3(0.46f, 0.14f, 0.54f), matCeramic, solid: true);
                Box(sink, "Grifo", new Vector3(-0.10f, 0.98f, 0f), new Vector3(0.12f, 0.12f, 0.04f), matIron);
                Box(sink, "Espejo", new Vector3(-0.26f, 1.48f, 0f), new Vector3(0.03f, 0.78f, 0.52f), matDoorGlass);
            }

            // 3 toilet stalls along East side of Restrooms2F
            for (int s = 0; s < 3; s++)
            {
                float sz = 25.00f + s * 1.30f;
                Transform stall = Group($"Cabina {s + 1}", restroomsGroup);
                stall.localPosition = new Vector3(-6.15f, YFloor2, sz);
                Box(stall, "Division", new Vector3(0f, 1.02f, -0.64f), new Vector3(1.65f, 1.92f, 0.06f), matLaminate, solid: true);
                Box(stall, "Frente", new Vector3(-0.80f, 1.02f, -0.42f), new Vector3(0.06f, 1.92f, 0.44f), matLaminate, solid: true);
                var leaf = Box(stall, "Puerta cabina", new Vector3(-0.80f, 1.02f, 0.15f), new Vector3(0.05f, 1.82f, 0.68f), matWood, solid: true);
                if (s == 1)
                {
                    leaf.localPosition = new Vector3(-1.02f, 1.02f, -0.08f);
                    leaf.localRotation = Quaternion.Euler(0f, 42f, 0f);
                }
                Box(stall, "Inodoro base", new Vector3(0.28f, 0.22f, 0f), new Vector3(0.52f, 0.42f, 0.38f), matCeramic, solid: true);
                Box(stall, "Tanque", new Vector3(0.55f, 0.62f, 0f), new Vector3(0.20f, 0.42f, 0.40f), matCeramic, solid: true);
            }
        }

        private void FurnishArchiveAndStorage()
        {
            // ArchiveRoom ([8.20, 10.60] x [22.80, 32.40])
            for (int i = 0; i < 4; i++)
                Bookcase(adminSuiteGroup, $"Estanteria archivo {i + 1}", new Vector3(8.62f, YFloor2, 24.00f + i * 2.25f), 90f);

            InstantiateResourcePrefab("EscritorioDocente", "EscritorioBasico",
                adminSuiteGroup, new Vector3(9.65f, YFloor2, 31.35f), 180f);
            InstantiateResourcePrefab("Cable", "CableFase",
                adminSuiteGroup, new Vector3(9.65f, YFloor2 + 0.74f, 31.35f), 15f);

            // StorageCloset2F ([-5.20, -1.72] x [29.00, 32.40])
            Bookcase(northBridgeGroup, "Estante suministros 1", new Vector3(-4.78f, YFloor2, 30.20f), 90f);
            Bookcase(northBridgeGroup, "Estante suministros 2", new Vector3(-2.14f, YFloor2, 30.20f), 270f);
            for (int i = 0; i < 3; i++)
                InstantiateResourcePrefab("Silla", "SillaApilable",
                    northBridgeGroup, new Vector3(-3.46f, YFloor2 + i * 0.14f, 29.65f), 8f * i);
            InstantiateResourcePrefab("Cable", "CableNeutro", northBridgeGroup, new Vector3(-4.55f, YFloor2, 31.55f), 0f);
            InstantiateResourcePrefab("CajaConexion", "CajaDerivacion", northBridgeGroup, new Vector3(-2.35f, YFloor2 + 0.92f, 30.20f), 0f);
        }

        private void BuildVentilationSystem()
        {
            float ductY = YFloor2 + StoryHeight - 0.36f;
            DuctRun(new Vector3(-12.30f, ductY, 21.10f), new Vector3(12.30f, ductY, 21.10f), grilleEvery: 7.5f);
            DuctRun(new Vector3(-12.30f, ductY, 21.10f), new Vector3(-12.30f, ductY, 38.00f), grilleEvery: 6.0f);
            DuctRun(new Vector3(12.30f, ductY, 21.10f), new Vector3(12.30f, ductY, 38.00f), grilleEvery: 6.0f);
            DuctRun(new Vector3(-12.30f, ductY, 34.10f), new Vector3(12.30f, ductY, 34.10f), grilleEvery: 6.5f);
            DuctRun(new Vector3(6.20f, ductY, 34.10f), new Vector3(6.20f, ductY, 41.60f), grilleEvery: 4.0f);
            DuctRun(new Vector3(-18.20f, ductY, 27.40f), new Vector3(-18.20f, ductY, 37.60f), grilleEvery: 5.0f);
            DuctRun(new Vector3(18.20f, ductY, 22.00f), new Vector3(18.20f, ductY, 37.80f), grilleEvery: 5.5f);
        }

        private void DuctRun(Vector3 from, Vector3 to, float grilleEvery)
        {
            Vector3 delta = to - from;
            float len = delta.magnitude;
            bool alongZ = Mathf.Abs(delta.z) > Mathf.Abs(delta.x);
            Transform seg = Group("Tramo ventilacion", ventilationGroup);
            seg.localPosition = (from + to) * 0.5f;
            Box(seg, "Conducto", Vector3.zero, alongZ ? new Vector3(0.56f, 0.34f, len) : new Vector3(len, 0.34f, 0.56f), matDuctMetal);

            int count = Mathf.Max(1, Mathf.FloorToInt(len / grilleEvery));
            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                Vector3 gPos = Vector3.Lerp(from, to, t);
                Transform grille = Group("Rejilla ventilacion", ventilationGroup);
                grille.localPosition = gPos + Vector3.down * 0.18f;
                Box(grille, "Marco", Vector3.zero, new Vector3(0.62f, 0.05f, 0.48f), matIron);
                for (int l = -3; l <= 3; l++)
                    Box(grille, "Lama", new Vector3(l * 0.075f, -0.02f, 0f), new Vector3(0.02f, 0.025f, 0.42f), matDuctMetal);
            }
        }

        private void BuildExteriorCourtyardAndMoonlight()
        {
            // 1st-floor roof cornice right below the 2nd-floor South & West/East windows (y = 4.22)
            Box(exteriorViewsGroup, "Cornisa primer piso sur", new Vector3(0f, YFloor2 - 0.18f, 19.12f),
                new Vector3(45.20f, 0.20f, 0.44f), matPlaster);
            Box(exteriorViewsGroup, "Cornisa primer piso oeste", new Vector3(-22.68f, YFloor2 - 0.18f, 29.30f),
                new Vector3(0.44f, 0.20f, 20.20f), matPlaster);
            Box(exteriorViewsGroup, "Cornisa primer piso este", new Vector3(22.68f, YFloor2 - 0.18f, 29.30f),
                new Vector3(0.44f, 0.20f, 20.20f), matPlaster);

            // Ground-level courtyard 4.40 m BELOW the 2nd floor (visible when looking down from AtriumGallery windows!)
            Box(exteriorViewsGroup, "Patio escolar inferior", new Vector3(0f, YFloor1 - 0.12f, 8.50f),
                new Vector3(58f, 0.24f, 24f), matRubber);
            InstantiatePrefabAtPath("Assets/EcoElectry/Content/Prefabs/Level01/PF_BasketballCourt.prefab",
                exteriorViewsGroup, new Vector3(0f, YFloor1, 10.60f), 0f);
            InstantiateResourcePrefab("LuminariaExterior", "LuminariaSolarPoste",
                exteriorViewsGroup, new Vector3(-13.20f, YFloor1, 10.60f), 90f);
            InstantiateResourcePrefab("LuminariaExterior", "LuminariaSolarPoste",
                exteriorViewsGroup, new Vector3(13.20f, YFloor1, 10.60f), 270f);

            // Walkways, hedges, and tall trees strictly outside the building envelope
            for (int i = -2; i <= 2; i++)
                InstantiateResourcePrefab("Arbusto", "Seto", exteriorViewsGroup, new Vector3(i * 4.2f, YFloor1, 18.20f), 0f);

            InstantiateResourcePrefab("Arbol", "ArbolGrande", exteriorViewsGroup, new Vector3(-18.50f, YFloor1, 9.80f), 25f);
            InstantiateResourcePrefab("Arbol", "ArbolGrande", exteriorViewsGroup, new Vector3(18.80f, YFloor1, 10.20f), 140f);
            InstantiateResourcePrefab("Arbol", "ArbolMediano", exteriorViewsGroup, new Vector3(-27.20f, YFloor1, 30.20f), 60f);
            InstantiateResourcePrefab("Arbol", "ArbolGrande", exteriorViewsGroup, new Vector3(29.00f, YFloor1, 29.50f), 215f);
            InstantiateResourcePrefab("Arbol", "ArbolMediano", exteriorViewsGroup, new Vector3(3.60f, YFloor1, 48.20f), 110f);
            InstantiateResourcePrefab("Montana", "LomaFondo", exteriorViewsGroup, new Vector3(0f, YFloor1 - 2.0f, -44.0f), 0f);

            // Cold blue moonlight spots outside key 2nd-floor windows (casting window frame shadows across corridors & labs!)
            Color moonColor = new Color(0.60f, 0.75f, 0.95f, 1f);
            Spotlight(exteriorViewsGroup, "Luna patio central", new Vector3(0f, YFloor2 + 3.60f, 15.20f),
                new Vector3(0f, -0.45f, 1f), moonColor, 5.4f, 16f, 88f, shadows: true);
            Spotlight(exteriorViewsGroup, "Luna patio cancha", new Vector3(0f, YFloor2 + 7.20f, 6.50f),
                new Vector3(0f, -0.85f, 0.32f), moonColor, 5.2f, 24f, 114f, shadows: false);
            Spotlight(exteriorViewsGroup, "Luna lab ciencias", new Vector3(-26.20f, YFloor2 + 3.40f, 30.30f),
                new Vector3(1f, -0.44f, 0.12f), moonColor, 5.0f, 14f, 86f, shadows: true);
            Spotlight(exteriorViewsGroup, "Luna lab informatica", new Vector3(26.20f, YFloor2 + 3.40f, 35.30f),
                new Vector3(-1f, -0.44f, -0.10f), moonColor, 5.0f, 14f, 86f, shadows: true);
            Spotlight(exteriorViewsGroup, "Luna patio de luz", new Vector3(-7.90f, YFloor2 + 4.80f, 30.70f),
                new Vector3(0.15f, -0.82f, 0.35f), moonColor, 3.6f, 12f, 92f, shadows: false);
        }

        private void Bench(Transform parent, Vector3 localPos, float yaw)
        {
            Transform bench = Group("Banco", parent);
            bench.localPosition = localPos;
            bench.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box(bench, "Asiento", new Vector3(0f, 0.45f, 0f), new Vector3(2.10f, 0.06f, 0.36f), matWood, solid: true);
            Box(bench, "Pata", new Vector3(-0.78f, 0.22f, 0f), new Vector3(0.07f, 0.44f, 0.30f), matIron);
            Box(bench, "Pata", new Vector3(0.78f, 0.22f, 0f), new Vector3(0.07f, 0.44f, 0.30f), matIron);
        }

        private void Bookcase(Transform parent, string name, Vector3 localPos, float yaw)
        {
            Transform shelf = Group(name, parent);
            shelf.localPosition = localPos;
            shelf.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // Collider volume + open-front wooden frame so shelves and binders are clearly visible
            InvisibleCollider(shelf, "Colision estanteria", new Vector3(0f, 1.02f, 0f), new Vector3(1.52f, 2.04f, 0.38f));
            Box(shelf, "Fondo", new Vector3(0f, 1.02f, -0.17f), new Vector3(1.52f, 2.04f, 0.04f), matWood);
            Box(shelf, "Lateral izq", new Vector3(-0.74f, 1.02f, 0f), new Vector3(0.04f, 2.04f, 0.38f), matWood);
            Box(shelf, "Lateral der", new Vector3(0.74f, 1.02f, 0f), new Vector3(0.04f, 2.04f, 0.38f), matWood);
            Box(shelf, "Base", new Vector3(0f, 0.04f, 0f), new Vector3(1.44f, 0.08f, 0.36f), matWood);
            Box(shelf, "Tapa superior", new Vector3(0f, 2.02f, 0f), new Vector3(1.52f, 0.04f, 0.38f), matWood);

            Material[] bookMats = { matBookBlue, matBookBrick, matBookGreen, matPaper };
            for (int level = 0; level < 4; level++)
            {
                float sy = 0.46f + level * 0.45f;
                Box(shelf, "Balda", new Vector3(0f, sy, 0.01f), new Vector3(1.44f, 0.03f, 0.34f), matWood);
                for (int b = 0; b < 5; b++)
                {
                    float bx = -0.52f + b * 0.26f;
                    Box(shelf, "Carpeta", new Vector3(bx, sy + 0.15f, 0.02f),
                        new Vector3(0.16f, 0.27f, 0.24f), bookMats[(level + b) % bookMats.Length]);
                }
            }
        }

        private void LowCabinet(Transform parent, string name, Vector3 localPos, float yaw)
        {
            Transform cab = Group(name, parent);
            cab.localPosition = localPos;
            cab.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box(cab, "Mueble", new Vector3(0f, 0.46f, 0f), new Vector3(1.24f, 0.92f, 0.48f), matWood, solid: true);
            Box(cab, "Tapa", new Vector3(0f, 0.93f, 0f), new Vector3(1.28f, 0.03f, 0.52f), matLaminate);
        }

        private void LabBench(Transform parent, Vector3 localPos, float width, bool hasSink)
        {
            Transform bench = Group("Mesa de laboratorio", parent);
            bench.localPosition = localPos;
            Box(bench, "Estructura", new Vector3(0f, 0.43f, 0f), new Vector3(width - 0.12f, 0.86f, 0.78f), matIron, solid: true);
            Box(bench, "Mesada quimica", new Vector3(0f, 0.875f, 0f), new Vector3(width, 0.05f, 0.88f), matRubber, solid: true);
            if (!hasSink) return;
            float sx = width * 0.34f;
            Box(bench, "Borde pileta", new Vector3(sx, 0.903f, 0f), new Vector3(0.38f, 0.018f, 0.40f), matCeramic);
            Box(bench, "Fondo pileta", new Vector3(sx, 0.913f, 0f), new Vector3(0.30f, 0.005f, 0.32f), matTrim);
            Box(bench, "Columna grifo", new Vector3(sx, 1.01f, -0.16f), new Vector3(0.024f, 0.21f, 0.024f), matIron);
            Box(bench, "Pico grifo", new Vector3(sx, 1.11f, -0.10f), new Vector3(0.022f, 0.022f, 0.12f), matIron);
        }

        private void MeetingTable(Transform parent, Vector3 localPos, float width, float depth)
        {
            Transform tbl = Group("Mesa de reuniones", parent);
            tbl.localPosition = localPos;
            Box(tbl, "Tablero", new Vector3(0f, 0.74f, 0f), new Vector3(width, 0.05f, depth), matWood, solid: true);
            foreach (float sx in new[] { -width * 0.40f, width * 0.40f })
            foreach (float sz in new[] { -depth * 0.38f, depth * 0.38f })
                Box(tbl, "Pata", new Vector3(sx, 0.36f, sz), new Vector3(0.06f, 0.72f, 0.06f), matIron, solid: true);
        }

        private static void ConfigureCeilingFan(Transform fan, bool startOn, float rpm)
        {
            foreach (MonoBehaviour mb in fan.GetComponents<MonoBehaviour>())
            {
                if (mb == null || mb.GetType().Name != "VentiladorInteractable") continue;
                var so = new SerializedObject(mb);
                var startProp = so.FindProperty("startOn");
                var rpmProp = so.FindProperty("revolutionsPerMinute");
                if (startProp != null) startProp.boolValue = startOn;
                if (rpmProp != null) rpmProp.floatValue = rpm;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
