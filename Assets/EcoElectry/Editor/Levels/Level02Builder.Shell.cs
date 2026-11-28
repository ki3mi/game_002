using System;
using System.Collections.Generic;
using EcoElectry.Unity.Player;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal sealed partial class Level02Builder
    {
        private enum OpeningKind
        {
            DoorClosed,
            DoorOpen,
            DoorLocked,
            FireDoorClosed,
            FireDoorOpen,
            FireDoorLocked,
            Ventanal,
            SlidingWindow,
            HighWindow
        }

        private readonly struct WallOpening
        {
            public readonly float Center;
            public readonly float Width;
            public readonly OpeningKind Kind;
            public readonly float FacingYaw;
            public readonly string Caption;

            public WallOpening(float center, OpeningKind kind, float facingYaw, string caption = null)
            {
                Center = center;
                Kind = kind;
                FacingYaw = facingYaw;
                Caption = caption;
                Width = kind switch
                {
                    OpeningKind.Ventanal => 2.64f,
                    OpeningKind.SlidingWindow => 1.32f,
                    OpeningKind.HighWindow => 1.24f,
                    _ => 1.18f
                };
            }
        }

        private void BuildStairwell()
        {
            const float totalH = YFloor2 + StoryHeight; // 8.55m above Floor 1

            // Floor 1 stairwell landing slab (West shaft x in [-1.72, 1.72], z in [24.80, 30.10])
            Slab(stairwellGroup, "Suelo descanso 1P", -1.72f, 1.72f, 24.80f, 30.10f, YFloor1, matFloor);
            Slab(stairwellGroup, "Repisa hueco escalera", -1.72f, 1.72f, 22.80f, 24.80f, YFloor2, matPlaster);

            // Solid foundation under East shaft so nothing is open beneath Floor 2
            Box(stairwellGroup, "Base estructural este", new Vector3(3.44f, YFloor2 * 0.5f, 24.45f),
                new Vector3(3.44f, YFloor2, 3.30f), matPlaster, solid: true);

            // Floor 2 upper landing (East shaft x in [1.72, 5.16], z in [22.80, 26.15])
            Slab(stairwellGroup, "Suelo llegada 2P", 1.72f, 5.16f, 22.80f, 26.15f, YFloor2, matFloor);

            // Full stairwell ceiling at Floor 2 roof height (y = 8.55)
            CeilingSlab(stairwellGroup, "Techo caja de escalera", -1.72f, 5.16f, 22.80f, 32.40f, totalH);

            // Outer stairwell perimeter walls: z in [21.70, 24.80] on West is open to Floor 1 side corridor (matching Level01!)
            TallWall(stairwellGroup, true, -1.72f, 24.80f, 32.40f, YFloor1, YFloor2);
            SingleSidedUpperWall(stairwellGroup, true, -1.72f, 22.80f, 32.40f, YFloor2, StoryHeight, trimSideSign: -1f);
            TallWall(stairwellGroup, true, 5.16f, 22.80f, 32.40f, YFloor1, YFloor2);
            SingleSidedUpperWall(stairwellGroup, true, 5.16f, 22.80f, 32.40f, YFloor2, StoryHeight, trimSideSign: 1f);

            // North stairwell wall (z = 32.40): landing wainscot inside stairwell at YLandingMid, corridor wainscot outside on +Z at YFloor2!
            TallWall(stairwellGroup, false, 32.40f, -1.72f, 5.16f, YFloor1, YLandingMid);
            SingleSidedUpperWall(stairwellGroup, false, 32.40f, -1.72f, 5.16f, YLandingMid, YFloor2 - YLandingMid, trimSideSign: -1f);
            SingleSidedUpperWall(stairwellGroup, false, 32.40f, -1.72f, 5.16f, YFloor2, StoryHeight, trimSideSign: 1f);

            // Side wainscot along intermediate landing (z in [30.00, 32.40] at y = YLandingMid)
            Box(stairwellGroup, "Zocalo descanso oeste", new Vector3(-1.61f, YLandingMid + 0.58f, 31.20f), new Vector3(0.02f, 1.16f, 2.40f), matDado);
            Box(stairwellGroup, "Guarda descanso oeste", new Vector3(-1.60f, YLandingMid + 1.17f, 31.20f), new Vector3(0.03f, 0.035f, 2.40f), matTrim);
            Box(stairwellGroup, "Zocalo descanso este", new Vector3(5.05f, YLandingMid + 0.58f, 31.20f), new Vector3(0.02f, 1.16f, 2.40f), matDado);
            Box(stairwellGroup, "Guarda descanso este", new Vector3(5.04f, YLandingMid + 1.17f, 31.20f), new Vector3(0.03f, 0.035f, 2.40f), matTrim);

            // Lintel & iconic "2.º PISO" plaque at z = 24.80 (seals cleanly up to YFloor2, exact match to Level01!)
            Box(stairwellGroup, "Dintel de la caja de escalera", new Vector3(0f, 3.50f, 24.80f),
                new Vector3(3.45f, 1.84f, 0.23f), matPlaster, solid: true);
            Sign(stairwellGroup, new Vector3(0f, 2.87f, 24.67f), 0f, "2.º PISO", 0.91f);

            // Central spine wall separating Flight 1 (West) and Flight 2 (East), with continuous Floor 1 dado on West face
            Box(stairwellGroup, "Muro central de escalera", new Vector3(1.72f, (YFloor2 + 1.12f) * 0.5f, 26.35f),
                new Vector3(0.20f, YFloor2 + 1.12f, 7.10f), matPlaster, solid: true);
            Box(stairwellGroup, "Remate muro central", new Vector3(1.72f, YFloor2 + 1.13f, 26.35f),
                new Vector3(0.26f, 0.05f, 7.14f), matTrim);
            Box(stairwellGroup, "Zocalo muro central 1P", new Vector3(1.615f, YFloor1 + 0.58f, 27.35f),
                new Vector3(0.02f, 1.16f, 5.10f), matDado);
            Box(stairwellGroup, "Guarda muro central 1P", new Vector3(1.608f, YFloor1 + 1.17f, 27.35f),
                new Vector3(0.03f, 0.035f, 5.10f), matTrim);
            Box(stairwellGroup, "Rodapie muro central 1P", new Vector3(1.604f, YFloor1 + 0.055f, 27.35f),
                new Vector3(0.035f, 0.11f, 5.10f), matTrim);

            // Build Floor 1 MainCorridor approach, left side turn (CafeteriaShortcut), and glazed Planta Baja doors matching Level01
            BuildLevel1CorridorConnection();

            // Floor 2 parapet overlooking Flight 1 void along z = 22.80, x in [-1.72, 1.72]
            Box(stairwellGroup, "Antepecho hueco escalera", new Vector3(0f, YFloor2 + 0.56f, 22.80f),
                new Vector3(3.44f, 1.12f, 0.20f), matPlaster, solid: true);
            Box(stairwellGroup, "Zocalo antepecho", new Vector3(0f, YFloor2 + 0.50f, 22.80f),
                new Vector3(3.44f, 1.00f, 0.21f), matDado);
            Box(stairwellGroup, "Pasamano mirador 2P", new Vector3(0f, YFloor2 + 1.15f, 22.80f),
                new Vector3(3.48f, 0.06f, 0.26f), matIron, solid: true);
            // Extra invisible safety collider up to 1.45m so player cannot hop over parapet
            InvisibleCollider(stairwellGroup, "GuardRail_Safety", new Vector3(0f, YFloor2 + 0.75f, 22.80f),
                new Vector3(3.44f, 1.50f, 0.24f));
            InvisibleCollider(stairwellGroup, "SpineRail_Safety", new Vector3(1.72f, YFloor2 + 0.75f, 26.35f),
                new Vector3(0.24f, 1.50f, 7.10f));

            // Intermediate Landing (y = 2.20, spanning full clear stairwell width x in [-1.62, 5.06], z in [30.00, 32.30])
            Box(stairwellGroup, "Descansillo intermedio", new Vector3(1.72f, YLandingMid * 0.5f, 31.15f),
                new Vector3(6.68f, YLandingMid, 2.30f), matPlaster, solid: true);
            Box(stairwellGroup, "Pavimento descansillo", new Vector3(1.72f, YLandingMid - 0.01f, 31.15f),
                new Vector3(6.68f, 0.03f, 2.30f), matFloor);

            // Flight 1: full shaft width 3.24m (flush with walls -1.62..+1.62, zero side gaps!)
            BuildStairFlight(stairwellGroup, "Tramo 1", centerX: 0f, zStart: 26.10f, zEnd: 30.00f,
                yStart: YFloor1, yEnd: YLandingMid, steps: 13, width: 3.24f);

            // Flight 2: full shaft width 3.24m (flush with walls +1.82..+5.06, zero side gaps!)
            BuildStairFlight(stairwellGroup, "Tramo 2", centerX: 3.44f, zStart: 30.00f, zEnd: 26.10f,
                yStart: YLandingMid, yEnd: YFloor2, steps: 13, width: 3.24f);

            // Stairwell lighting (exact Level01 spotlight position behind lintel!)
            EmergencyPilot(stairwellGroup, new Vector3(0f, YLandingMid + 1.65f, 32.26f), 0f);
            EmergencyPilot(stairwellGroup, new Vector3(3.44f, YLandingMid + 2.10f, 32.26f), 0f);
            Spotlight(stairwellGroup, "Luz descansillo inferior", new Vector3(0f, 3.40f, 25.45f),
                new Vector3(0f, -0.84f, 0.54f), new Color(0.68f, 0.78f, 0.87f, 1f), 4.6f, 8.2f, 98f, shadows: true);
            Spotlight(stairwellGroup, "Luz descansillo intermedio", new Vector3(2.40f, YLandingMid + 3.45f, 31.10f),
                Vector3.down, new Color(0.76f, 0.84f, 0.82f, 1f), 2.8f, 7.5f, 118f, shadows: false);
            FluorescentFixture(stairwellGroup, new Vector3(3.44f, totalH - 0.62f, 24.60f), 0f, lit: true, intensity: 2.5f, range: 8.5f);
            FluorescentFixture(stairwellGroup, new Vector3(1.72f, totalH - 0.62f, 31.10f), 0f, lit: false);
        }

        private void BuildLevel1CorridorConnection()
        {
            float ceilBottom = YFloor2 - 0.22f;
            float wallTop = YFloor2 - 0.20f;
            Transform lower = Group("Conexion Nivel 01", stairwellGroup);

            // 1. MainCorridor approach (x in [-1.72, 1.72], z in [15.60, 24.80]) — exact Level01 geometry!
            Slab(lower, "Suelo pasillo 1P", -1.72f, 1.72f, 15.60f, 24.80f, YFloor1, matFloor);
            CeilingSlab(lower, "Techo pasillo 1P", -1.72f, 1.72f, 15.60f, 24.80f, ceilBottom);
            TallWall(lower, true, 1.72f, 15.60f, 24.80f, YFloor1, wallTop);
            TallWall(lower, true, -1.72f, 15.60f, 21.70f, YFloor1, wallTop); // [21.70..24.80] open to West side turn!

            Beam(lower, new Vector3(0f, 4.04f, 19.63f), new Vector3(3.38f, 0.20f, 0.24f));
            Beam(lower, new Vector3(0f, 3.98f, 15.72f), new Vector3(3.38f, 0.30f, 0.24f));
            Box(lower, "Canaleta pasillo 1P", new Vector3(1.58f, YFloor1 + 2.90f, 19.49f),
                new Vector3(0.06f, 0.05f, 2.94f), matIron);

            // Iconic red-orange EmergencyPilot on right/East wall right before the stairs (exact Level01 pos!)
            EmergencyPilot(lower, new Vector3(1.60f, YFloor1 + 2.36f, 23.50f), 90f);
            FluorescentFixture(lower, new Vector3(0f, YFloor1 + 3.66f, 21.15f), 0f, lit: false);
            FluorescentFixture(lower, new Vector3(0f, YFloor1 + 3.66f, 17.90f), 0f, lit: true, intensity: 1.95f, range: 7.4f);

            // 2. West side turn (CafeteriaShortcut from Level01: [-5.40, -1.72] x [21.70, 24.80] + [-5.40, -3.40] x [24.80, 28.20])
            Slab(lower, "Suelo recodo oeste 1P", -5.40f, -1.72f, 21.70f, 24.80f, YFloor1, matFloor);
            CeilingSlab(lower, "Techo recodo oeste 1P", -5.40f, -1.72f, 21.70f, 24.80f, ceilBottom);
            Slab(lower, "Suelo paso lateral 1P", -5.40f, -3.40f, 24.80f, 28.20f, YFloor1, matFloor);
            CeilingSlab(lower, "Techo paso lateral 1P", -5.40f, -3.40f, 24.80f, 28.20f, ceilBottom);

            TallWall(lower, false, 21.70f, -5.50f, -1.62f, YFloor1, wallTop);
            TallWall(lower, true, -5.40f, 21.70f, 28.20f, YFloor1, wallTop);
            TallWall(lower, false, 24.80f, -3.50f, -1.62f, YFloor1, wallTop);
            TallWall(lower, true, -3.40f, 24.80f, 28.20f, YFloor1, wallTop);
            WallSpan(lower, false, 28.20f, -5.40f, -3.40f, YFloor1, wallTop,
                new WallOpening(-4.40f, OpeningKind.DoorLocked, 0f));

            FluorescentFixture(lower, new Vector3(-3.84f, YFloor1 + 3.66f, 23.18f), 90f, lit: true, intensity: 2.0f, range: 7.0f);
            FluorescentFixture(lower, new Vector3(-4.29f, YFloor1 + 3.66f, 26.08f), 90f, lit: false);

            // 3. Locked Double Glazed Doors at z = 15.60 (matching Level01 blue-framed double glass doors!)
            TallWall(lower, false, 15.60f, -1.72f, -1.30f, YFloor1, wallTop);
            TallWall(lower, false, 15.60f, 1.30f, 1.72f, YFloor1, wallTop);

            Transform dblDoors = Group("Puertas acristaladas Planta Baja", lower);
            dblDoors.localPosition = new Vector3(0f, YFloor1, 15.60f);
            float lintelH = wallTop - 2.38f;
            Box(dblDoors, "Dintel", new Vector3(0f, 2.38f + lintelH * 0.5f, 0f),
                new Vector3(2.64f, lintelH, 0.20f), matPlaster, solid: true);
            Box(dblDoors, "Umbral", new Vector3(0f, 0.006f, 0f), new Vector3(2.60f, 0.012f, 0.24f), matIron);
            InvisibleCollider(dblDoors, "LowerCorridor_Blocker", new Vector3(0f, 1.20f, 0f), new Vector3(2.64f, 2.40f, 0.22f));

            foreach (float sx in new[] { -0.64f, 0.64f })
            {
                Transform leaf = InstantiateResourcePrefab("Ventana", "VentanaFija", dblDoors, new Vector3(sx, 0.21f, 0f), 0f);
                leaf.localScale = new Vector3(1.091f, 1.808f, 1.300f);
                Box(dblDoors, "Zocalo de la hoja", new Vector3(sx, 0.108f, 0f), new Vector3(1.30f, 0.216f, 0.13f), matIron);
                foreach (float sz in new[] { -1f, 1f })
                {
                    Box(dblDoors, "Barra de empuje", new Vector3(sx, 1.02f, sz * 0.133f), new Vector3(0.84f, 0.044f, 0.048f), matIron);
                    Box(dblDoors, "Soporte de barra", new Vector3(sx - 0.39f, 1.02f, sz * 0.087f), new Vector3(0.043f, 0.05f, 0.09f), matIron);
                    Box(dblDoors, "Soporte de barra", new Vector3(sx + 0.39f, 1.02f, sz * 0.087f), new Vector3(0.043f, 0.05f, 0.09f), matIron);
                }
            }

            // 4. Visible Level01 Cross-Corridor beyond the locked glass doors (z in [12.00, 15.60], x in [-7.40, 7.40])
            Slab(lower, "Suelo cruce 1P", -7.40f, 7.40f, 12.00f, 15.60f, YFloor1, matFloor);
            CeilingSlab(lower, "Techo cruce 1P", -7.40f, 7.40f, 12.00f, 15.60f, ceilBottom);
            TallWall(lower, false, 12.00f, -7.40f, 7.40f, YFloor1, wallTop);
            TallWall(lower, false, 15.60f, -7.40f, -1.72f, YFloor1, wallTop);
            TallWall(lower, false, 15.60f, 1.72f, 7.40f, YFloor1, wallTop);
            TallWall(lower, true, -7.40f, 12.00f, 15.60f, YFloor1, wallTop);
            TallWall(lower, true, 7.40f, 12.00f, 15.60f, YFloor1, wallTop);

            WallClock(lower, new Vector3(0f, YFloor1 + 2.72f, 12.14f), 180f);
            NoticeBoard(lower, new Vector3(-4.40f, YFloor1 + 1.63f, 12.14f), 180f);
            FluorescentFixture(lower, new Vector3(0f, YFloor1 + 3.66f, 13.70f), 0f, lit: true, intensity: 1.75f, range: 7.5f);
            FluorescentFixture(lower, new Vector3(-4.80f, YFloor1 + 3.66f, 13.70f), 0f, lit: false);
            FluorescentFixture(lower, new Vector3(4.80f, YFloor1 + 3.66f, 13.70f), 0f, lit: false);
        }

        private void BuildStairFlight(Transform parent, string name, float centerX, float zStart, float zEnd,
            float yStart, float yEnd, int steps, float width)
        {
            Transform flight = Group(name, parent);
            float dir = Mathf.Sign(zEnd - zStart);
            float run = Mathf.Abs(zEnd - zStart);
            float rise = yEnd - yStart;
            float stepDepth = run / steps;
            float stepRise = rise / steps;

            for (int i = 0; i < steps; i++)
            {
                float topY = yStart + stepRise * (i + 1);
                float h = topY - yStart;
                float zCenter = zStart + dir * (stepDepth * (i + 0.5f));
                Box(flight, $"Escalon {i + 1:00}", new Vector3(centerX, yStart + h * 0.5f, zCenter),
                    new Vector3(width, h, stepDepth + 0.008f), matPlaster, solid: false);
                float nosingZ = zStart + dir * (stepDepth * i + 0.035f);
                Box(flight, "Cantonera", new Vector3(centerX, topY + 0.004f, nosingZ),
                    new Vector3(width, 0.014f, 0.070f), matRubber, solid: false);
            }

            // Smooth sloped BoxCollider aligned with the step nosings so CharacterController walks up & down cleanly
            float slopeLength = Mathf.Sqrt(run * run + rise * rise);
            float pitchDeg = -dir * Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            const float rampThickness = 0.16f;
            Vector3 normalOffset = new Vector3(0f, -Mathf.Cos(Mathf.Atan2(rise, run)), dir * Mathf.Sin(Mathf.Atan2(rise, run))) * (rampThickness * 0.5f);
            var ramp = new GameObject("Rampa de colision");
            ramp.transform.SetParent(flight, false);
            ramp.transform.localPosition = new Vector3(centerX, (yStart + yEnd) * 0.5f, (zStart + zEnd) * 0.5f) + normalOffset;
            ramp.transform.localRotation = Quaternion.Euler(pitchDeg, 0f, 0f);
            var rampCol = ramp.AddComponent<BoxCollider>();
            rampCol.size = new Vector3(width, rampThickness, slopeLength + 0.12f);

            // Iron balusters and angled handrails on both sides
            foreach (float side in new[] { -1f, 1f })
            {
                float railX = centerX + side * (width * 0.5f - 0.07f);
                for (int b = 0; b < 7; b++)
                {
                    float t = (b + 0.5f) / 7f;
                    float bz = Mathf.Lerp(zStart, zEnd, t);
                    float by = Mathf.Lerp(yStart, yEnd, t) + 0.48f;
                    Box(flight, "Montante de baranda", new Vector3(railX, by, bz),
                        new Vector3(0.055f, 0.96f, 0.055f), matIron);
                }

                var handrail = Box(flight, "Pasamano", new Vector3(railX, (yStart + yEnd) * 0.5f + 0.96f, (zStart + zEnd) * 0.5f),
                    new Vector3(0.095f, 0.068f, slopeLength + 0.25f), matIron, solid: true);
                handrail.localRotation = Quaternion.Euler(pitchDeg, 0f, 0f);
            }
        }

        private void BuildFloor2Shell()
        {
            // 1. ATRIUM GALLERY & UPPER LANDING ([-14.00, 14.00] x [19.40, 22.80])
            Slab(atriumGalleryGroup, "Suelo galeria", -14.00f, 14.00f, 19.40f, 22.80f, YFloor2, matFloor);
            CeilingSlab(atriumGalleryGroup, "Techo galeria", -14.00f, 14.00f, 19.40f, 22.80f, YFloor2 + StoryHeight);

            // South exterior courtyard-overlook facade (z = 19.40) with 5 large Ventanales
            WallSpan(atriumGalleryGroup, false, 19.40f, -14.00f, 14.00f, YFloor2, StoryHeight,
                new WallOpening(-10.80f, OpeningKind.Ventanal, 0f),
                new WallOpening(-5.40f, OpeningKind.Ventanal, 0f),
                new WallOpening(0.00f, OpeningKind.Ventanal, 0f),
                new WallOpening(5.40f, OpeningKind.Ventanal, 0f),
                new WallOpening(10.80f, OpeningKind.Ventanal, 0f));

            // North wall segments of AtriumGallery (z = 22.80):
            // [-14.00..-10.60] OPEN to WestWing
            // [-10.60..-5.20] Restrooms2F entrance
            WallSpan(atriumGalleryGroup, false, 22.80f, -10.60f, -5.20f, YFloor2, StoryHeight,
                new WallOpening(-7.90f, OpeningKind.DoorClosed, 0f, "BAÑOS"));
            // [-5.20..-1.72] Locked 3rd-floor stairwell entrance (with glass door so player sees stairs rising!)
            WallSpan(atriumGalleryGroup, false, 22.80f, -5.20f, -1.72f, YFloor2, StoryHeight,
                new WallOpening(-3.46f, OpeningKind.DoorLocked, 0f, "3.er PISO"));
            // [-1.72..+1.72] Stairwell parapet already built; add upper soffit beam at ceiling
            Beam(atriumGalleryGroup, new Vector3(0f, YFloor2 + StoryHeight - 0.14f, 22.80f), new Vector3(3.44f, 0.28f, 0.24f));
            // [+1.72..+5.16] OPEN to Stairwell 2nd-floor arrival! Add framing header beam
            Beam(atriumGalleryGroup, new Vector3(3.44f, YFloor2 + StoryHeight - 0.14f, 22.80f), new Vector3(3.44f, 0.28f, 0.24f));
            // [+5.16..+8.20] ServiceShortcut fire door (no sign clutter)
            WallSpan(atriumGalleryGroup, false, 22.80f, 5.16f, 8.20f, YFloor2, StoryHeight,
                new WallOpening(6.68f, OpeningKind.FireDoorClosed, 0f));
            // [+8.20..+10.60] South wall of ArchiveRoom
            WallSpan(atriumGalleryGroup, false, 22.80f, 8.20f, 10.60f, YFloor2, StoryHeight);
            // [+10.60..+14.00] OPEN to EastWing!

            // Structural ceiling beams across AtriumGallery (no floating directional signs on corridor corners!)
            foreach (float bx in new[] { -10.60f, -5.20f, 5.16f, 10.60f })
                Beam(atriumGalleryGroup, new Vector3(bx, YFloor2 + StoryHeight - 0.12f, 21.10f), new Vector3(0.24f, 0.24f, 3.36f));

            WallClock(atriumGalleryGroup, new Vector3(8.95f, YFloor2 + 2.68f, 22.68f), 0f);
            NoticeBoard(atriumGalleryGroup, new Vector3(-6.20f, YFloor2 + 1.65f, 22.68f), 0f);

            FluorescentFixture(atriumGalleryGroup, new Vector3(-11.80f, YFloor2 + 3.65f, 21.10f), 0f, lit: true);
            FluorescentFixture(atriumGalleryGroup, new Vector3(-6.80f, YFloor2 + 3.65f, 21.10f), 0f, lit: false);
            FluorescentFixture(atriumGalleryGroup, new Vector3(-1.40f, YFloor2 + 3.65f, 21.10f), 0f, lit: true);
            FluorescentFixture(atriumGalleryGroup, new Vector3(3.44f, YFloor2 + 3.65f, 21.10f), 0f, lit: true, intensity: 2.6f);
            FluorescentFixture(atriumGalleryGroup, new Vector3(8.20f, YFloor2 + 3.65f, 21.10f), 0f, lit: false);
            FluorescentFixture(atriumGalleryGroup, new Vector3(12.20f, YFloor2 + 3.65f, 21.10f), 0f, lit: true);

            // 2. BLOCKED 3RD FLOOR / ROOF STAIRCASE NICHE ([-5.20, -1.72] x [22.80, 29.00])
            Slab(stairwellGroup, "Suelo arranque 3P", -5.20f, -1.72f, 22.80f, 29.00f, YFloor2, matFloor);
            CeilingSlab(stairwellGroup, "Techo arranque 3P", -5.20f, -1.72f, 22.80f, 29.00f, YFloor2 + StoryHeight);
            WallSpan(stairwellGroup, true, -5.20f, 22.80f, 29.00f, YFloor2, StoryHeight);
            WallSpan(stairwellGroup, false, 29.00f, -5.20f, -1.72f, YFloor2, StoryHeight);
            BuildStairFlight(stairwellGroup, "Tramo bloqueado 3P", centerX: -3.46f, zStart: 24.40f, zEnd: 28.30f,
                yStart: YFloor2, yEnd: YFloor2 + 2.15f, steps: 11, width: 3.24f);
            InvisibleCollider(stairwellGroup, "ThirdFloor_Blocker", new Vector3(-3.46f, YFloor2 + 1.20f, 24.20f),
                new Vector3(3.40f, 2.40f, 0.12f));
            EmergencyPilot(stairwellGroup, new Vector3(-3.46f, YFloor2 + 3.15f, 28.86f), 0f);

            // 3. WEST CORRIDOR ([-14.00, -10.60] x [22.80, 39.20])
            Slab(westWingGroup, "Suelo pasillo oeste", -14.00f, -10.60f, 22.80f, 39.20f, YFloor2, matFloor);
            CeilingSlab(westWingGroup, "Techo pasillo oeste", -14.00f, -10.60f, 22.80f, 39.20f, YFloor2 + StoryHeight);
            // North end exterior wall of WestCorridor with Ventanal
            WallSpan(westWingGroup, false, 39.20f, -14.00f, -10.60f, YFloor2, StoryHeight,
                new WallOpening(-12.30f, OpeningKind.Ventanal, 180f));
            // West wall of WestCorridor (only primary room plaques, no secondary/arrow signs)
            WallSpan(westWingGroup, true, -14.00f, 22.80f, 26.20f, YFloor2, StoryHeight,
                new WallOpening(24.50f, OpeningKind.DoorOpen, 270f, "AULA 202"));
            WallSpan(westWingGroup, true, -14.00f, 26.20f, 34.40f, YFloor2, StoryHeight,
                new WallOpening(29.80f, OpeningKind.DoorClosed, 270f, "LAB. CIENCIAS"));
            WallSpan(westWingGroup, true, -14.00f, 34.40f, 39.20f, YFloor2, StoryHeight,
                new WallOpening(36.80f, OpeningKind.DoorClosed, 270f));
            // East wall of WestCorridor: [22.80..29.00] Restrooms wall, [29.00..32.40] Lightwell window, [32.40..35.80] OPEN to NorthBridge, [35.80..39.20] AudiovisualRoom west wall
            WallSpan(westWingGroup, true, -10.60f, 22.80f, 29.00f, YFloor2, StoryHeight);
            WallSpan(westWingGroup, true, -10.60f, 29.00f, 32.40f, YFloor2, StoryHeight,
                new WallOpening(30.70f, OpeningKind.Ventanal, 90f));
            WallSpan(westWingGroup, true, -10.60f, 35.80f, 39.20f, YFloor2, StoryHeight);

            FluorescentFixture(westWingGroup, new Vector3(-12.30f, YFloor2 + 3.65f, 25.00f), 90f, lit: true);
            FluorescentFixture(westWingGroup, new Vector3(-12.30f, YFloor2 + 3.65f, 29.80f), 90f, lit: false);
            FluorescentFixture(westWingGroup, new Vector3(-12.30f, YFloor2 + 3.65f, 34.10f), 90f, lit: true);
            FluorescentFixture(westWingGroup, new Vector3(-12.30f, YFloor2 + 3.65f, 37.80f), 90f, lit: false);
            EmergencyPilot(westWingGroup, new Vector3(-10.72f, YFloor2 + 2.35f, 32.15f), 90f);
            NoticeBoard(westWingGroup, new Vector3(-10.72f, YFloor2 + 1.65f, 26.80f), 90f);

            // 4. EAST CORRIDOR ([10.60, 14.00] x [22.80, 39.20])
            Slab(eastWingGroup, "Suelo pasillo este", 10.60f, 14.00f, 22.80f, 39.20f, YFloor2, matFloor);
            CeilingSlab(eastWingGroup, "Techo pasillo este", 10.60f, 14.00f, 22.80f, 39.20f, YFloor2 + StoryHeight);
            // North end exterior wall of EastCorridor with Ventanal
            WallSpan(eastWingGroup, false, 39.20f, 10.60f, 14.00f, YFloor2, StoryHeight,
                new WallOpening(12.30f, OpeningKind.Ventanal, 180f));
            // East wall of EastCorridor (minimal plaques on key rooms only)
            WallSpan(eastWingGroup, true, 14.00f, 22.80f, 25.40f, YFloor2, StoryHeight,
                new WallOpening(24.10f, OpeningKind.DoorOpen, 90f));
            WallSpan(eastWingGroup, true, 14.00f, 25.40f, 31.40f, YFloor2, StoryHeight,
                new WallOpening(28.40f, OpeningKind.DoorClosed, 90f, "DIRECCIÓN"));
            WallSpan(eastWingGroup, true, 14.00f, 31.40f, 39.20f, YFloor2, StoryHeight,
                new WallOpening(33.80f, OpeningKind.DoorOpen, 90f, "CÓMPUTO"));
            // West wall of EastCorridor: [22.80..32.40] ArchiveRoom w/ door, [32.40..35.80] OPEN to NorthBridge, [35.80..39.20] ElectricalRoom east wall
            WallSpan(eastWingGroup, true, 10.60f, 22.80f, 32.40f, YFloor2, StoryHeight,
                new WallOpening(27.60f, OpeningKind.DoorClosed, 270f));
            WallSpan(eastWingGroup, true, 10.60f, 35.80f, 39.20f, YFloor2, StoryHeight);

            FluorescentFixture(eastWingGroup, new Vector3(12.30f, YFloor2 + 3.65f, 24.80f), 90f, lit: true);
            FluorescentFixture(eastWingGroup, new Vector3(12.30f, YFloor2 + 3.65f, 29.40f), 90f, lit: true);
            FluorescentFixture(eastWingGroup, new Vector3(12.30f, YFloor2 + 3.65f, 34.10f), 90f, lit: false);
            FluorescentFixture(eastWingGroup, new Vector3(12.30f, YFloor2 + 3.65f, 37.60f), 90f, lit: true);
            NoticeBoard(eastWingGroup, new Vector3(10.72f, YFloor2 + 1.65f, 30.40f), 270f);

            // 5. NORTH SERVICE BRIDGE ([-10.60, 10.60] x [32.40, 35.80])
            Slab(northBridgeGroup, "Suelo pasillo norte", -10.60f, 10.60f, 32.40f, 35.80f, YFloor2, matFloor);
            CeilingSlab(northBridgeGroup, "Techo pasillo norte", -10.60f, 10.60f, 32.40f, 35.80f, YFloor2 + StoryHeight);

            // South wall of NorthServiceBridge (z = 32.40):
            // [-10.60..-5.20] Lightwell window
            WallSpan(northBridgeGroup, false, 32.40f, -10.60f, -5.20f, YFloor2, StoryHeight,
                new WallOpening(-7.90f, OpeningKind.Ventanal, 0f));
            // [-5.20..-1.72] StorageCloset2F door (unlabeled utility door)
            WallSpan(northBridgeGroup, false, 32.40f, -5.20f, -1.72f, YFloor2, StoryHeight,
                new WallOpening(-3.46f, OpeningKind.DoorClosed, 180f));
            // [-1.72..+5.16] Stairwell north wall already built with Floor 2 wainscot!
            // [+5.16..+8.20] ServiceShortcut north fire door (unlabeled)
            WallSpan(northBridgeGroup, false, 32.40f, 5.16f, 8.20f, YFloor2, StoryHeight,
                new WallOpening(6.68f, OpeningKind.FireDoorClosed, 180f));
            // [+8.20..+10.60] ArchiveRoom north wall
            WallSpan(northBridgeGroup, false, 32.40f, 8.20f, 10.60f, YFloor2, StoryHeight);

            // North wall of NorthServiceBridge (z = 35.80):
            // [-10.60..+1.80] AudiovisualRoom (only main entrance has plaque)
            WallSpan(northBridgeGroup, false, 35.80f, -10.60f, 1.80f, YFloor2, StoryHeight,
                new WallOpening(-6.80f, OpeningKind.DoorOpen, 0f, "AUDIOVISUAL"),
                new WallOpening(0.10f, OpeningKind.DoorClosed, 0f));
            // [+1.80..+10.60] ElectricalRoom (fire door)
            WallSpan(northBridgeGroup, false, 35.80f, 1.80f, 10.60f, YFloor2, StoryHeight,
                new WallOpening(4.80f, OpeningKind.FireDoorClosed, 0f, "CUARTO ELÉCTRICO"));

            FluorescentFixture(northBridgeGroup, new Vector3(-7.60f, YFloor2 + 3.65f, 34.10f), 0f, lit: true);
            FluorescentFixture(northBridgeGroup, new Vector3(-2.20f, YFloor2 + 3.65f, 34.10f), 0f, lit: false);
            FluorescentFixture(northBridgeGroup, new Vector3(2.80f, YFloor2 + 3.65f, 34.10f), 0f, lit: false);
            FluorescentFixture(northBridgeGroup, new Vector3(7.90f, YFloor2 + 3.65f, 34.10f), 0f, lit: true);
            EmergencyPilot(northBridgeGroup, new Vector3(4.80f, YFloor2 + 3.05f, 35.67f), 0f);

            // 6. SERVICE SHORTCUT ([5.16, 8.20] x [22.80, 32.40]) & ARCHIVE ROOM ([8.20, 10.60] x [22.80, 32.40])
            Slab(serviceShortcutGroup, "Suelo paso tecnico", 5.16f, 8.20f, 22.80f, 32.40f, YFloor2, matFloor);
            CeilingSlab(serviceShortcutGroup, "Techo paso tecnico", 5.16f, 8.20f, 22.80f, 32.40f, YFloor2 + StoryHeight);
            WallSpan(serviceShortcutGroup, true, 8.20f, 22.80f, 32.40f, YFloor2, StoryHeight);
            FluorescentFixture(serviceShortcutGroup, new Vector3(6.68f, YFloor2 + 3.65f, 25.60f), 90f, lit: true, intensity: 1.7f);
            FluorescentFixture(serviceShortcutGroup, new Vector3(6.68f, YFloor2 + 3.65f, 29.80f), 90f, lit: false);

            Slab(adminSuiteGroup, "Suelo archivo", 8.20f, 10.60f, 22.80f, 32.40f, YFloor2, matLaminate);
            CeilingSlab(adminSuiteGroup, "Techo archivo", 8.20f, 10.60f, 22.80f, 32.40f, YFloor2 + StoryHeight);
            FluorescentFixture(adminSuiteGroup, new Vector3(9.40f, YFloor2 + 3.65f, 25.40f), 90f, lit: true, intensity: 1.8f);
            FluorescentFixture(adminSuiteGroup, new Vector3(9.40f, YFloor2 + 3.65f, 29.80f), 90f, lit: false);

            // 7. RESTROOMS 2F ([-10.60, -5.20] x [22.80, 29.00]), LIGHTWELL ([-10.60, -5.20] x [29.00, 32.40]), STORAGE CLOSET ([-5.20, -1.72] x [29.00, 32.40])
            Slab(restroomsGroup, "Suelo banos 2P", -10.60f, -5.20f, 22.80f, 29.00f, YFloor2, matTileFloor);
            CeilingSlab(restroomsGroup, "Techo banos 2P", -10.60f, -5.20f, 22.80f, 29.00f, YFloor2 + StoryHeight);
            WallSpan(restroomsGroup, false, 29.00f, -10.60f, -5.20f, YFloor2, StoryHeight,
                new WallOpening(-9.10f, OpeningKind.HighWindow, 0f),
                new WallOpening(-6.70f, OpeningKind.HighWindow, 0f));

            // Interior lightwell walls (from Floor 1 up to Floor 2 roof so looking down through lightwell glass looks into a real 2-story shaft!)
            Slab(exteriorViewsGroup, "Fondo patio de luz", -10.60f, -5.20f, 29.00f, 32.40f, YFloor1, matRubber);
            TallWall(exteriorViewsGroup, true, -10.60f, 29.00f, 32.40f, YFloor1, YFloor2);
            TallWall(exteriorViewsGroup, true, -5.20f, 29.00f, 32.40f, YFloor1, YFloor2);
            WallSpan(northBridgeGroup, true, -5.20f, 29.00f, 32.40f, YFloor2, StoryHeight);
            TallWall(exteriorViewsGroup, false, 29.00f, -10.60f, -5.20f, YFloor1, YFloor2);
            TallWall(exteriorViewsGroup, false, 32.40f, -10.60f, -5.20f, YFloor1, YFloor2);

            Slab(northBridgeGroup, "Suelo deposito 2P", -5.20f, -1.72f, 29.00f, 32.40f, YFloor2, matFloor);
            CeilingSlab(northBridgeGroup, "Techo deposito 2P", -5.20f, -1.72f, 29.00f, 32.40f, YFloor2 + StoryHeight);
            FluorescentFixture(northBridgeGroup, new Vector3(-3.46f, YFloor2 + 3.65f, 30.70f), 0f, lit: true, intensity: 1.6f);

            // 8. WEST OUTER ROOMS: Classroom202, ScienceLab, PrepRoom (x in [-22.40, -14.00])
            // Classroom 202: [-22.40, -14.00] x [19.40, 26.20]
            Slab(classroomsAndLabsGroup, "Suelo Aula 202", -22.40f, -14.00f, 19.40f, 26.20f, YFloor2, matFloor);
            CeilingSlab(classroomsAndLabsGroup, "Techo Aula 202", -22.40f, -14.00f, 19.40f, 26.20f, YFloor2 + StoryHeight);
            WallSpan(classroomsAndLabsGroup, false, 19.40f, -22.40f, -14.00f, YFloor2, StoryHeight,
                new WallOpening(-18.20f, OpeningKind.Ventanal, 0f));
            WallSpan(classroomsAndLabsGroup, true, -22.40f, 19.40f, 26.20f, YFloor2, StoryHeight,
                new WallOpening(22.80f, OpeningKind.SlidingWindow, 270f));
            WallSpan(classroomsAndLabsGroup, true, -14.00f, 19.40f, 22.80f, YFloor2, StoryHeight);
            WallSpan(classroomsAndLabsGroup, false, 26.20f, -22.40f, -14.00f, YFloor2, StoryHeight);

            // Science Lab 201: [-22.40, -14.00] x [26.20, 34.40]
            Slab(classroomsAndLabsGroup, "Suelo Lab Ciencias", -22.40f, -14.00f, 26.20f, 34.40f, YFloor2, matTileFloor);
            CeilingSlab(classroomsAndLabsGroup, "Techo Lab Ciencias", -22.40f, -14.00f, 26.20f, 34.40f, YFloor2 + StoryHeight);
            WallSpan(classroomsAndLabsGroup, true, -22.40f, 26.20f, 34.40f, YFloor2, StoryHeight,
                new WallOpening(28.50f, OpeningKind.Ventanal, 270f),
                new WallOpening(32.10f, OpeningKind.Ventanal, 270f));
            WallSpan(classroomsAndLabsGroup, false, 34.40f, -22.40f, -14.00f, YFloor2, StoryHeight,
                new WallOpening(-18.20f, OpeningKind.DoorOpen, 0f));

            // Prep Room 201-B: [-22.40, -14.00] x [34.40, 39.20]
            Slab(classroomsAndLabsGroup, "Suelo Preparacion", -22.40f, -14.00f, 34.40f, 39.20f, YFloor2, matTileFloor);
            CeilingSlab(classroomsAndLabsGroup, "Techo Preparacion", -22.40f, -14.00f, 34.40f, 39.20f, YFloor2 + StoryHeight);
            WallSpan(classroomsAndLabsGroup, true, -22.40f, 34.40f, 39.20f, YFloor2, StoryHeight,
                new WallOpening(36.80f, OpeningKind.HighWindow, 270f));
            WallSpan(classroomsAndLabsGroup, false, 39.20f, -22.40f, -14.00f, YFloor2, StoryHeight);

            // 9. EAST OUTER ROOMS: Secretaria, PrincipalOffice, ComputerLab (x in [14.00, 22.40])
            // Secretaria: [14.00, 22.40] x [19.40, 25.40]
            Slab(adminSuiteGroup, "Suelo Secretaria", 14.00f, 22.40f, 19.40f, 25.40f, YFloor2, matLaminate);
            CeilingSlab(adminSuiteGroup, "Techo Secretaria", 14.00f, 22.40f, 19.40f, 25.40f, YFloor2 + StoryHeight);
            WallSpan(adminSuiteGroup, false, 19.40f, 14.00f, 22.40f, YFloor2, StoryHeight,
                new WallOpening(18.20f, OpeningKind.Ventanal, 0f));
            WallSpan(adminSuiteGroup, true, 22.40f, 19.40f, 25.40f, YFloor2, StoryHeight,
                new WallOpening(22.40f, OpeningKind.SlidingWindow, 90f));
            WallSpan(adminSuiteGroup, true, 14.00f, 19.40f, 22.80f, YFloor2, StoryHeight);
            WallSpan(adminSuiteGroup, false, 25.40f, 14.00f, 22.40f, YFloor2, StoryHeight,
                new WallOpening(19.20f, OpeningKind.DoorClosed, 0f));

            // Principal Office: [14.00, 22.40] x [25.40, 31.40]
            Slab(adminSuiteGroup, "Suelo Direccion", 14.00f, 22.40f, 25.40f, 31.40f, YFloor2, matWood);
            CeilingSlab(adminSuiteGroup, "Techo Direccion", 14.00f, 22.40f, 25.40f, 31.40f, YFloor2 + StoryHeight);
            WallSpan(adminSuiteGroup, true, 22.40f, 25.40f, 31.40f, YFloor2, StoryHeight,
                new WallOpening(28.40f, OpeningKind.Ventanal, 90f));
            WallSpan(adminSuiteGroup, false, 31.40f, 14.00f, 22.40f, YFloor2, StoryHeight);

            // Computer Lab 204: [14.00, 22.40] x [31.40, 39.20]
            Slab(adminSuiteGroup, "Suelo Lab Informatica", 14.00f, 22.40f, 31.40f, 39.20f, YFloor2, matLaminate);
            CeilingSlab(adminSuiteGroup, "Techo Lab Informatica", 14.00f, 22.40f, 31.40f, 39.20f, YFloor2 + StoryHeight);
            WallSpan(adminSuiteGroup, true, 22.40f, 31.40f, 39.20f, YFloor2, StoryHeight,
                new WallOpening(33.60f, OpeningKind.Ventanal, 90f),
                new WallOpening(37.10f, OpeningKind.Ventanal, 90f));
            WallSpan(adminSuiteGroup, false, 39.20f, 14.00f, 22.40f, YFloor2, StoryHeight);

            // 10. NORTH OUTER ROOMS: AudiovisualRoom ([-10.60, 1.80] x [35.80, 42.60]) & ElectricalRoom ([1.80, 10.60] x [35.80, 42.60])
            Slab(audiovisualGroup, "Suelo Sala Audiovisual", -10.60f, 1.80f, 35.80f, 42.60f, YFloor2, matWood);
            CeilingSlab(audiovisualGroup, "Techo Sala Audiovisual", -10.60f, 1.80f, 35.80f, 42.60f, YFloor2 + StoryHeight);
            WallSpan(audiovisualGroup, true, -10.60f, 39.20f, 42.60f, YFloor2, StoryHeight);
            WallSpan(audiovisualGroup, false, 42.60f, -10.60f, 1.80f, YFloor2, StoryHeight);
            WallSpan(audiovisualGroup, true, 1.80f, 35.80f, 42.60f, YFloor2, StoryHeight);
            // Internal booth partition inside AudiovisualRoom at x = -1.20 with glass observation window & door!
            WallSpan(audiovisualGroup, true, -1.20f, 35.80f, 42.60f, YFloor2, StoryHeight,
                new WallOpening(37.20f, OpeningKind.DoorOpen, 90f),
                new WallOpening(40.10f, OpeningKind.Ventanal, 90f));

            Slab(electricalRoomGroup, "Suelo Cuarto Electrico", 1.80f, 10.60f, 35.80f, 42.60f, YFloor2, matFloor);
            CeilingSlab(electricalRoomGroup, "Techo Cuarto Electrico", 1.80f, 10.60f, 35.80f, 42.60f, YFloor2 + StoryHeight);
            WallSpan(electricalRoomGroup, false, 42.60f, 1.80f, 10.60f, YFloor2, StoryHeight,
                new WallOpening(4.40f, OpeningKind.HighWindow, 180f),
                new WallOpening(8.20f, OpeningKind.HighWindow, 180f));
            WallSpan(electricalRoomGroup, true, 10.60f, 39.20f, 42.60f, YFloor2, StoryHeight);
        }

        private void WallSpan(Transform parent, bool alongZ, float fixedCoord, float from, float to,
            float baseY, float height, params WallOpening[] openings)
        {
            if (to < from) (from, to) = (to, from);
            if (openings != null && openings.Length > 1)
                Array.Sort(openings, (a, b) => a.Center.CompareTo(b.Center));

            float cursor = from;
            if (openings != null)
            {
                foreach (WallOpening op in openings)
                {
                    float start = op.Center - op.Width * 0.5f;
                    float end = op.Center + op.Width * 0.5f;
                    if (start > cursor + 0.02f)
                        WallSegment(parent, alongZ, fixedCoord, cursor, start, baseY, height);

                    Vector3 pos = alongZ
                        ? new Vector3(fixedCoord, baseY, op.Center)
                        : new Vector3(op.Center, baseY, fixedCoord);
                    PlaceOpening(parent, pos, op, height);
                    cursor = end;
                }
            }

            if (to > cursor + 0.02f)
                WallSegment(parent, alongZ, fixedCoord, cursor, to, baseY, height);
        }

        private void WallSegment(Transform parent, bool alongZ, float fixedCoord, float from, float to, float baseY, float height)
        {
            float length = to - from;
            if (length <= 0.01f) return;
            float mid = (from + to) * 0.5f;
            Transform wall = Group("Muro", parent);
            wall.localPosition = alongZ ? new Vector3(fixedCoord, baseY, mid) : new Vector3(mid, baseY, fixedCoord);
            wall.localRotation = Quaternion.Euler(0f, alongZ ? 90f : 0f, 0f);

            Box(wall, "Yeso", new Vector3(0f, height * 0.5f, 0f), new Vector3(length, height, 0.20f), matPlaster, solid: true);
            if (height >= 1.4f)
            {
                Box(wall, "Zocalo", new Vector3(0f, 0.58f, 0f), new Vector3(length, 1.16f, 0.209f), matDado);
                Box(wall, "Guarda", new Vector3(0f, 1.17f, 0f), new Vector3(length, 0.035f, 0.224f), matTrim);
                Box(wall, "Rodapie", new Vector3(0f, 0.055f, 0f), new Vector3(length, 0.11f, 0.232f), matTrim);
            }
        }

        private void TallWall(Transform parent, bool alongZ, float fixedCoord, float from, float to, float baseY, float topY)
        {
            WallSegment(parent, alongZ, fixedCoord, from, to, baseY, topY - baseY);
        }

        private void SingleSidedUpperWall(Transform parent, bool alongZ, float fixedCoord, float from, float to,
            float baseY, float height, float trimSideSign)
        {
            float length = Mathf.Abs(to - from);
            float mid = (from + to) * 0.5f;
            Transform wall = Group("Muro", parent);
            wall.localPosition = alongZ ? new Vector3(fixedCoord, baseY, mid) : new Vector3(mid, baseY, fixedCoord);
            wall.localRotation = Quaternion.Euler(0f, alongZ ? 90f : 0f, 0f);

            // For alongZ=true (rot Y=90), local +Z points in world +X; for alongZ=false (rot Y=0), local +Z points in world +Z
            float zOff = trimSideSign * 0.012f;
            Box(wall, "Yeso", new Vector3(0f, height * 0.5f, 0f), new Vector3(length, height, 0.20f), matPlaster, solid: true);
            Box(wall, "Zocalo", new Vector3(0f, 0.58f, zOff), new Vector3(length, 1.16f, 0.188f), matDado);
            Box(wall, "Guarda", new Vector3(0f, 1.17f, zOff * 1.35f), new Vector3(length, 0.035f, 0.194f), matTrim);
            Box(wall, "Rodapie", new Vector3(0f, 0.055f, zOff * 1.55f), new Vector3(length, 0.11f, 0.198f), matTrim);
        }

        private void PlaceOpening(Transform parent, Vector3 pos, WallOpening op, float wallHeight)
        {
            switch (op.Kind)
            {
                case OpeningKind.DoorClosed:
                case OpeningKind.DoorOpen:
                case OpeningKind.DoorLocked:
                    CreateGlazedSchoolDoor(parent, pos, op.FacingYaw, op.Caption, wallHeight,
                        startsOpen: op.Kind == OpeningKind.DoorOpen,
                        locked: op.Kind == OpeningKind.DoorLocked);
                    break;
                case OpeningKind.FireDoorClosed:
                case OpeningKind.FireDoorOpen:
                case OpeningKind.FireDoorLocked:
                    CreateFireDoor(parent, pos, op.FacingYaw, op.Caption, wallHeight,
                        startsOpen: op.Kind == OpeningKind.FireDoorOpen,
                        locked: op.Kind == OpeningKind.FireDoorLocked);
                    break;
                case OpeningKind.Ventanal:
                    CreateVentanal(parent, pos, op.FacingYaw, wallHeight);
                    break;
                case OpeningKind.SlidingWindow:
                    CreateSlidingWindow(parent, pos, op.FacingYaw, wallHeight);
                    break;
                case OpeningKind.HighWindow:
                    CreateHighWindow(parent, pos, op.FacingYaw, wallHeight);
                    break;
            }
        }

        private void CreateGlazedSchoolDoor(Transform parent, Vector3 localPos, float yaw, string caption,
            float wallHeight, bool startsOpen, bool locked)
        {
            string title = string.IsNullOrEmpty(caption) ? "PUERTA" : caption;
            Transform acceso = Group("Acceso " + title, parent);
            acceso.localPosition = localPos;
            acceso.localRotation = Quaternion.Euler(0f, yaw, 0f);

            if (!locked)
            {
                var trig = acceso.gameObject.AddComponent<BoxCollider>();
                trig.isTrigger = true;
                trig.center = new Vector3(0f, 1.15f, 0f);
                trig.size = new Vector3(1.14f, 2.29f, 0.18f);
            }

            float lintelHeight = Mathf.Max(0.20f, wallHeight - 2.43f);
            Box(acceso, "Dintel", new Vector3(0f, 2.43f + lintelHeight * 0.5f, 0f),
                new Vector3(1.185f, lintelHeight, 0.20f), matPlaster, solid: true);
            Box(acceso, "Umbral", new Vector3(0f, 0.006f, 0f), new Vector3(1.14f, 0.012f, 0.24f), matIron);

            Transform door = InstantiateResourcePrefab("Puerta", "PuertaInterior", acceso, Vector3.zero, 0f);
            door.localScale = new Vector3(1.26f, 1.12f, 1.18f);
            Transform ensamble = door.Find("PuertaInterior_Ensamble");
            if (ensamble != null) ensamble.localScale = new Vector3(1f, 1.04f, 1f);

            Transform hoja = door.Find("PuertaInterior_Ensamble/PuertaInterior_PivoteBisagra/PuertaInterior_Hoja");
            Transform paneles = door.Find("PuertaInterior_Ensamble/PuertaInterior_PivoteBisagra/PuertaInterior_Paneles");
            if (hoja != null && hoja.TryGetComponent<MeshRenderer>(out var mrHoja)) mrHoja.enabled = false;
            if (paneles != null && paneles.TryGetComponent<MeshRenderer>(out var mrPan)) mrPan.enabled = false;

            Transform hinge = door.Find("PuertaInterior_Ensamble/PuertaInterior_PivoteBisagra");
            if (hinge != null)
            {
                Box(hinge, "Larguero de hoja", new Vector3(-0.742f, 1.060f, 0f), new Vector3(0.135f, 2.090f, 0.055f), matWood);
                Box(hinge, "Larguero de hoja", new Vector3(-0.067f, 1.060f, 0f), new Vector3(0.135f, 2.090f, 0.055f), matWood);
                Box(hinge, "Panel inferior", new Vector3(-0.405f, 0.520f, 0f), new Vector3(0.560f, 1.020f, 0.055f), matWood);
                Box(hinge, "Travesano", new Vector3(-0.405f, 1.087f, 0f), new Vector3(0.560f, 0.125f, 0.055f), matWood);
                Box(hinge, "Travesano", new Vector3(-0.405f, 2.018f, 0f), new Vector3(0.560f, 0.170f, 0.055f), matWood);
                Box(hinge, "Vidrio de seguridad", new Vector3(-0.405f, 1.540f, 0f), new Vector3(0.560f, 0.800f, 0.014f), matDoorGlass);

                if (startsOpen)
                    hinge.localRotation = Quaternion.Euler(0f, 94f, 0f);
            }

            ConfigureDoorComponent(door, hinge, startsOpen, locked, openAngle: 94f);

            if (!string.IsNullOrEmpty(caption))
                Sign(acceso, new Vector3(0f, 2.63f, -0.125f), 0f, caption, Mathf.Clamp(0.42f + caption.Length * 0.045f, 0.72f, 1.24f));
        }

        private void CreateFireDoor(Transform parent, Vector3 localPos, float yaw, string caption,
            float wallHeight, bool startsOpen, bool locked)
        {
            string title = string.IsNullOrEmpty(caption) ? "CORTAFUEGO" : caption;
            Transform acceso = Group("Acceso " + title, parent);
            acceso.localPosition = localPos;
            acceso.localRotation = Quaternion.Euler(0f, yaw, 0f);

            if (!locked)
            {
                var trig = acceso.gameObject.AddComponent<BoxCollider>();
                trig.isTrigger = true;
                trig.center = new Vector3(0f, 1.15f, 0f);
                trig.size = new Vector3(1.14f, 2.29f, 0.18f);
            }

            float lintelHeight = Mathf.Max(0.20f, wallHeight - 2.43f);
            Box(acceso, "Dintel", new Vector3(0f, 2.43f + lintelHeight * 0.5f, 0f),
                new Vector3(1.185f, lintelHeight, 0.20f), matPlaster, solid: true);
            Box(acceso, "Umbral", new Vector3(0f, 0.006f, 0f), new Vector3(1.14f, 0.012f, 0.24f), matIron);

            Transform door = InstantiateResourcePrefab("Puerta", "PuertaCortafuego", acceso, Vector3.zero, 0f);
            door.localScale = new Vector3(1.16f, 1.125f, 1.10f);
            Transform hinge = door.Find("PuertaCortafuego_Root/PuertaCortafuego_PivoteBisagra");
            if (hinge != null && startsOpen)
                hinge.localRotation = Quaternion.Euler(0f, 92f, 0f);

            ConfigureDoorComponent(door, hinge, startsOpen, locked, openAngle: 92f);

            if (!string.IsNullOrEmpty(caption))
                Sign(acceso, new Vector3(0f, 2.63f, -0.125f), 0f, caption, Mathf.Clamp(0.45f + caption.Length * 0.045f, 0.76f, 1.28f));
        }

        private static void ConfigureDoorComponent(Transform door, Transform hinge, bool startsOpen, bool locked, float openAngle)
        {
            foreach (MonoBehaviour mb in door.GetComponents<MonoBehaviour>())
            {
                if (mb == null || mb.GetType().Name != "PuertaInteractable") continue;
                mb.enabled = !locked;
                var so = new SerializedObject(mb);
                var openAngleProp = so.FindProperty("openAngle");
                var startsOpenProp = so.FindProperty("startsOpen");
                var hingesProp = so.FindProperty("hinges");
                if (openAngleProp != null) openAngleProp.floatValue = openAngle;
                if (startsOpenProp != null) startsOpenProp.boolValue = startsOpen;
                if (hinge != null && hingesProp != null && hingesProp.arraySize == 0)
                {
                    hingesProp.arraySize = 1;
                    hingesProp.GetArrayElementAtIndex(0).objectReferenceValue = hinge;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void CreateVentanal(Transform parent, Vector3 localPos, float yaw, float wallHeight)
        {
            Transform vent = Group("Ventanal", parent);
            vent.localPosition = localPos;
            vent.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(vent, "Antepecho", new Vector3(0f, 0.525f, 0f), new Vector3(2.645f, 1.05f, 0.20f), matDado, solid: true);
            Box(vent, "Rodapie", new Vector3(0f, 0.055f, 0f), new Vector3(2.645f, 0.11f, 0.23f), matTrim);
            Box(vent, "Alfeizar", new Vector3(0f, 1.05f, 0f), new Vector3(2.68f, 0.05f, 0.32f), matPlaster);

            float lintelBottom = 2.52f;
            float lintelH = Mathf.Max(0.20f, wallHeight - lintelBottom);
            Box(vent, "Dintel", new Vector3(0f, lintelBottom + lintelH * 0.5f, 0f),
                new Vector3(2.645f, lintelH, 0.20f), matPlaster, solid: true);

            foreach (float wx in new[] { -0.65f, 0.65f })
            {
                Transform win = InstantiateResourcePrefab("Ventana", "VentanaFija", vent, new Vector3(wx, 1.05f, 0f), 0f);
                win.localScale = new Vector3(1.074f, 1.225f, 1.30f);
            }
        }

        private void CreateSlidingWindow(Transform parent, Vector3 localPos, float yaw, float wallHeight)
        {
            Transform vent = Group("Ventana corredera", parent);
            vent.localPosition = localPos;
            vent.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(vent, "Antepecho", new Vector3(0f, 0.525f, 0f), new Vector3(1.325f, 1.05f, 0.20f), matDado, solid: true);
            Box(vent, "Rodapie", new Vector3(0f, 0.055f, 0f), new Vector3(1.325f, 0.11f, 0.23f), matTrim);
            Box(vent, "Alfeizar", new Vector3(0f, 1.05f, 0f), new Vector3(1.36f, 0.05f, 0.30f), matPlaster);

            float lintelBottom = 2.38f;
            float lintelH = Mathf.Max(0.20f, wallHeight - lintelBottom);
            Box(vent, "Dintel", new Vector3(0f, lintelBottom + lintelH * 0.5f, 0f),
                new Vector3(1.325f, lintelH, 0.20f), matPlaster, solid: true);

            Transform win = InstantiateResourcePrefab("Ventana", "VentanaCorredera", vent, new Vector3(0f, 1.06f, 0f), 0f);
            win.localScale = new Vector3(1.01f, 1.20f, 1.35f);
        }

        private void CreateHighWindow(Transform parent, Vector3 localPos, float yaw, float wallHeight)
        {
            Transform vent = Group("Ventana alta", parent);
            vent.localPosition = localPos;
            vent.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(vent, "Muro inferior", new Vector3(0f, 1.12f, 0f), new Vector3(1.245f, 2.24f, 0.20f), matPlaster, solid: true);
            Box(vent, "Zocalo", new Vector3(0f, 0.58f, 0f), new Vector3(1.245f, 1.16f, 0.209f), matDado);
            Box(vent, "Guarda", new Vector3(0f, 1.17f, 0f), new Vector3(1.245f, 0.035f, 0.224f), matTrim);
            Box(vent, "Rodapie", new Vector3(0f, 0.055f, 0f), new Vector3(1.245f, 0.11f, 0.232f), matTrim);
            Box(vent, "Alfeizar", new Vector3(0f, 2.24f, 0f), new Vector3(1.28f, 0.045f, 0.28f), matPlaster);

            float lintelBottom = 2.96f;
            float lintelH = Mathf.Max(0.20f, wallHeight - lintelBottom);
            Box(vent, "Dintel", new Vector3(0f, lintelBottom + lintelH * 0.5f, 0f),
                new Vector3(1.245f, lintelH, 0.20f), matPlaster, solid: true);

            foreach (float wx in new[] { -0.31f, 0.31f })
            {
                Transform win = InstantiateResourcePrefab("Ventana", "VentanaProyectante", vent, new Vector3(wx, 2.26f, 0f), 0f);
                win.localScale = new Vector3(1.02f, 1.16f, 1.25f);
            }
        }

        private void Slab(Transform parent, string name, float x0, float x1, float z0, float z1, float surfaceY, Material mat)
        {
            Box(parent, name, new Vector3((x0 + x1) * 0.5f, surfaceY - 0.10f, (z0 + z1) * 0.5f),
                new Vector3(Mathf.Abs(x1 - x0), 0.20f, Mathf.Abs(z1 - z0)), mat, solid: true);
        }

        private void CeilingSlab(Transform parent, string name, float x0, float x1, float z0, float z1, float bottomY)
        {
            Box(parent, name, new Vector3((x0 + x1) * 0.5f, bottomY + 0.11f, (z0 + z1) * 0.5f),
                new Vector3(Mathf.Abs(x1 - x0), 0.22f, Mathf.Abs(z1 - z0)), matCeiling, solid: true);
        }

        private void Beam(Transform parent, Vector3 pos, Vector3 size)
        {
            Box(parent, "Viga", pos, size, matPlaster, solid: true);
        }

        private void Sign(Transform parent, Vector3 localPos, float yaw, string caption, float width)
        {
            Transform sign = Group("Placa " + caption, parent);
            sign.localPosition = localPos;
            sign.localRotation = Quaternion.identity;
            Box(sign, "Placa", Vector3.zero, new Vector3(width, 0.23f, 0.036f), matWayfinding);

            var face = Group("Rotulo", sign);
            face.localPosition = new Vector3(0f, -0.004f, -0.021f);
            var text = face.gameObject.AddComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            HorrorCorridorEnvironment.ConfigureSignText(text);
            text.fontSize = 60;
            text.characterSize = 0.032f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color32(172, 190, 160, 255);
            text.text = caption;

            // Fit rendered world width cleanly inside the green backing plate before rotating
            Quaternion savedParentRot = parent.rotation;
            parent.rotation = Quaternion.identity;
            var mr = text.GetComponent<MeshRenderer>();
            float currentWidth = mr != null ? mr.bounds.size.x : 0f;
            float maxAllowed = Mathf.Max(0.24f, width - 0.14f);
            if (currentWidth > maxAllowed && currentWidth > 0.001f)
                text.characterSize *= maxAllowed / currentWidth;
            parent.rotation = savedParentRot;

            sign.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void FluorescentFixture(Transform parent, Vector3 localPos, float yaw, bool lit,
            float intensity = 2.20f, float range = 7.20f)
        {
            Transform fluo = Group(lit ? "Fluorescente" : "Fluorescente apagado", parent);
            fluo.localPosition = localPos;
            fluo.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(fluo, "Carcasa", Vector3.zero, new Vector3(1.38f, 0.09f, 0.29f), matIron);
            Box(fluo, "Tubo", new Vector3(0f, -0.06f, -0.065f), new Vector3(1.24f, 0.033f, 0.039f), lit ? matFluorescent : matCeiling);
            Box(fluo, "Tubo", new Vector3(0f, -0.06f, 0.065f), new Vector3(1.24f, 0.033f, 0.039f), lit ? matFluorescent : matCeiling);
            Box(fluo, "Varilla de suspension", new Vector3(-0.48f, 0.268f, 0f), new Vector3(0.023f, 0.49f, 0.023f), matIron);
            Box(fluo, "Varilla de suspension", new Vector3(0.48f, 0.268f, 0f), new Vector3(0.023f, 0.49f, 0.023f), matIron);

            if (!lit) return;
            var lightGo = Group("Luz", fluo);
            lightGo.localPosition = new Vector3(0f, -0.118f, 0f);
            lightGo.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            var lt = lightGo.gameObject.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.intensity = intensity;
            lt.range = range;
            lt.innerSpotAngle = 83f;
            lt.spotAngle = 134f;
            lt.color = new Color(0.824f, 0.878f, 0.792f, 1f);
            lt.shadows = LightShadows.None;
        }

        private void EmergencyPilot(Transform parent, Vector3 localPos, float yaw)
        {
            Transform pilot = Group("Piloto de emergencia", parent);
            pilot.localPosition = localPos;
            pilot.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(pilot, "Soporte", Vector3.zero, new Vector3(0.24f, 0.11f, 0.08f), matIron);
            Box(pilot, "Lente", new Vector3(0f, 0f, -0.048f), new Vector3(0.15f, 0.055f, 0.03f), matEmergencyLens);
            var glowGo = Group("Resplandor", pilot);
            glowGo.localPosition = new Vector3(0f, 0f, -0.20f);
            var lt = glowGo.gameObject.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 2.6f;
            lt.intensity = 0.56f;
            lt.color = new Color32(255, 99, 66, 255);
            lt.shadows = LightShadows.None;
        }

        private Light Spotlight(Transform parent, string label, Vector3 localPos, Vector3 localDir,
            Color color, float intensity, float range, float angle, bool shadows)
        {
            Transform spot = Group(label, parent);
            spot.localPosition = localPos;
            spot.localRotation = Quaternion.LookRotation(localDir.normalized, Vector3.up);
            var lt = spot.gameObject.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            lt.spotAngle = angle;
            lt.innerSpotAngle = angle * 0.62f;
            lt.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            lt.shadowBias = 0.035f;
            lt.shadowNormalBias = 0.15f;
            return lt;
        }

        private void NoticeBoard(Transform parent, Vector3 localPos, float yaw)
        {
            Transform board = Group("Cartelera", parent);
            board.localPosition = localPos;
            board.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box(board, "Marco", Vector3.zero, new Vector3(1.68f, 0.88f, 0.06f), matTrim);
            Box(board, "Corcho", new Vector3(0f, 0f, -0.022f), new Vector3(1.58f, 0.78f, 0.025f), matCork);
            for (int i = 0; i < 5; i++)
            {
                var sheet = Box(board, "Circular", new Vector3(-0.56f + i * 0.28f, (i % 2 == 0 ? 0.07f : -0.06f), -0.038f),
                    new Vector3(0.22f, 0.31f, 0.008f), matPaper);
                sheet.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 4f : -5f);
            }
        }

        private void WallClock(Transform parent, Vector3 localPos, float yaw)
        {
            Transform clock = Group("Reloj del pasillo", parent);
            clock.localPosition = localPos;
            clock.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box(clock, "Caja", Vector3.zero, new Vector3(0.42f, 0.42f, 0.06f), matIron);
            Box(clock, "Esfera", new Vector3(0f, 0f, -0.032f), new Vector3(0.36f, 0.36f, 0.01f), matPaper);
            var hour = Box(clock, "Aguja horaria", new Vector3(-0.04f, -0.03f, -0.040f), new Vector3(0.018f, 0.11f, 0.008f), matTrim);
            hour.localRotation = Quaternion.Euler(0f, 0f, 145f);
            var min = Box(clock, "Minutero", new Vector3(0f, 0.05f, -0.042f), new Vector3(0.012f, 0.15f, 0.008f), matTrim);
            min.localRotation = Quaternion.Euler(0f, 0f, 12f);
        }

        private Transform InstantiateResourcePrefab(string family, string model, Transform parent, Vector3 localPos, float yaw)
        {
            string path = $"Assets/ResourcePack/{family}/Prefabs/{model}.prefab";
            return InstantiatePrefabAtPath(path, parent, localPos, yaw);
        }

        private static Transform InstantiatePrefabAtPath(string path, Transform parent, Vector3 localPos, float yaw)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new InvalidOperationException("Prefab ausente: " + path);
            var instance = ((GameObject)PrefabUtility.InstantiatePrefab(asset, parent)).transform;
            instance.localPosition = localPos;
            instance.localRotation = Quaternion.Euler(0f, yaw, 0f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
            return instance;
        }

        private static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform Box(Transform parent, string name, Vector3 localPos, Vector3 size, Material material, bool solid = false)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPos;
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(box.GetComponent<BoxCollider>());
            return box.transform;
        }

        private static void InvisibleCollider(Transform parent, string name, Vector3 localPos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
        }
    }
}
