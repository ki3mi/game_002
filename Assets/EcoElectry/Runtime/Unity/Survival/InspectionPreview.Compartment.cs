using System.Collections.Generic;
using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal sealed partial class InspectionPreview
    {
        private const float TransferSeconds = .52f;

        private sealed class CellView
        {
            public CellView(Transform root)
            {
                Root = root;
                Negative = root.Find("Negative");
                Positive = root.Find("Positive");
            }

            public Transform Root { get; }
            public Transform Negative { get; }
            public Transform Positive { get; }
            public Transform Terminal(BatteryTerminal terminal) => terminal == BatteryTerminal.Positive ? Positive : Negative;
        }

        private readonly Dictionary<BatteryCell, CellView> compartmentCells = new();
        private readonly Transform[] seats = new Transform[FlashlightState.SlotCount];
        private readonly Renderer[] seatMarkers = new Renderer[FlashlightState.SlotCount];
        private readonly Transform[] probes = new Transform[2];
        private readonly Transform[] docks = new Transform[2];
        private readonly Transform[] cableEnds = new Transform[2];
        private Transform[] tray;
        private Transform workspace;
        private Transform movingCell;
        private Transform destination;
        private Vector3 startPosition;
        private Vector3 startDirection;
        private Vector3 startScale;
        private Quaternion startRotation;
        private float transferTime;

        public bool IsMovingCell => movingCell != null;

        public void ShowFlashlight(BatteryInspection inspection)
        {
            BeginModel(true);
            workspace = Group("Mesa de inspección", stage.transform, Vector3.zero);
            Transform flashlight = Spawn(assets.Flashlight, model);
            HitBox(flashlight, new Vector3(.23f, -.148f, 0), new Vector3(1.57f, .28f, .92f));
            for (int i = 0; i < seats.Length; i++)
            {
                seats[i] = flashlight.Find(i == 0 ? "SlotA" : "SlotB");
                HitBox(seats[i], new Vector3(0, -.096f, 0), new Vector3(.98f, .042f, .34f), new Hit(TargetKind.Slot, i));
                seatMarkers[i] = Block("Marca de ranura", new Vector3(0, -.091f, .169f), new Vector3(.9f, .023f, .022f),
                    Marking, false, seats[i]).GetComponent<Renderer>();
                AddCell(inspection.Slots[i], seats[i]);
            }

            tray = new Transform[inspection.Tray.Count];
            float step = Mathf.Min(1.03f, 4.50f / tray.Length);
            float trayWidth = tray.Length * step + .15f;
            Block("Bandeja", new Vector3(0, 1.19f, .155f), new Vector3(trayWidth, .30f, .13f), Graphite, false, workspace);
            Block("Borde inferior", new Vector3(0, 1.043f, .099f), new Vector3(trayWidth, .026f, .108f), Steel, false, workspace);
            for (int i = 0; i < tray.Length; i++)
            {
                float x = (i - (tray.Length - 1f) / 2f) * step;
                tray[i] = Group($"Bandeja {i + 1}", workspace, new Vector3(x, 1.194f, -.016f));
                tray[i].localRotation = Quaternion.Euler(-62f, 0, 0);
                tray[i].localScale = Vector3.one * Mathf.Min(.75f, step * .82f);
                Block("Fondo del alojamiento", new Vector3(x, 1.194f, .077f), new Vector3(step - .031f, .239f, .019f),
                    i < inspection.ReserveCount ? Rubber : new Color(.15f, .19f, .21f), false, workspace);
                AddCell(inspection.Tray[i], tray[i]);
            }

            for (int i = 0; i < probes.Length; i++)
            {
                docks[i] = Group("Soporte de punta", workspace, new Vector3(-2.14f + i * .39f, -.98f + i * .08f, -.28f));
                docks[i].localRotation = Quaternion.Euler(-16, 0, i == 0 ? -23 : -16);
                probes[i] = Spawn(i == 0 ? assets.BlackProbe : assets.RedProbe, workspace);
                cableEnds[i] = probes[i].Find("Cable");
                HitBox(probes[i], new Vector3(0, .405f, 0), new Vector3(.20f, .43f, .19f), new Hit(TargetKind.Probe, i));
                probes[i].SetPositionAndRotation(docks[i].position, docks[i].rotation);
            }
        }

        private void AddCell(BatteryCell cell, Transform seat)
        {
            if (cell == null) return;
            var instance = new CellView(Spawn(assets.Cell, seat));
            instance.Root.localPosition = Vector3.zero;
            HitBox(instance.Root, Vector3.zero, new Vector3(.93f, .214f, .347f), new Hit(TargetKind.Cell, cell: cell));
            HitBox(instance.Negative, new Vector3(0, .017f, 0), new Vector3(.22f, .083f, .20f),
                new Hit(TargetKind.Terminal, (int)BatteryTerminal.Negative, cell));
            HitBox(instance.Positive, new Vector3(0, .017f, 0), new Vector3(.22f, .083f, .20f),
                new Hit(TargetKind.Terminal, (int)BatteryTerminal.Positive, cell));
            compartmentCells.Add(cell, instance);
        }

        private void HitBox(Transform parent, Vector3 center, Vector3 dimensions, Hit target = default)
        {
            var box = parent.gameObject.AddComponent<BoxCollider>();
            box.center = center;
            box.size = dimensions;
            if (target.Kind != TargetKind.None) targets.Add(box, target);
        }

        public void TransferCell(BatteryCell cell, int index, bool toTray)
        {
            movingCell = compartmentCells[cell].Root;
            destination = toTray ? tray[index] : seats[index];
            startPosition = movingCell.position;
            startRotation = movingCell.rotation;
            startScale = movingCell.lossyScale;
            startDirection = toTray ? movingCell.up : Vector3.down;
            movingCell.SetParent(workspace, true);
            transferTime = 0f;
        }

        public void UpdateCompartment(BatteryInspection inspection, BatteryCell selected, float seconds)
        {
            if (movingCell != null)
            {
                transferTime += seconds;
                float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(transferTime / TransferSeconds));
                float inv = 1 - t;
                Vector3 end = destination.position;
                Vector3 lift = startPosition + startDirection * .49f;
                Vector3 approach = end + destination.up * .42f;
                movingCell.SetPositionAndRotation(inv * inv * inv * startPosition + 3 * inv * inv * t * lift +
                    3 * inv * t * t * approach + t * t * t * end, Quaternion.Slerp(startRotation, destination.rotation, t));
                movingCell.localScale = Vector3.Lerp(startScale, destination.lossyScale, t);
                if (transferTime >= TransferSeconds)
                {
                    movingCell.SetParent(destination, false);
                    movingCell.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    movingCell.localScale = Vector3.one;
                    movingCell = null;
                }
            }

            for (int i = 0; i < seats.Length; i++)
                SetColor(seatMarkers[i], selected != null && inspection.Slots[i] == null ? Selected : Marking);
            for (int i = 0; i < probes.Length; i++)
                FollowContact(inspection.Meter, i, seconds);
        }

        private void FollowContact(BatteryMeter meter, int index, float seconds)
        {
            ProbeContact contact = meter.GetContact((MeterProbe)index);
            Transform anchor = docks[index];
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            if (contact.Cell != null)
            {
                CellView cell = compartmentCells[contact.Cell];
                anchor = cell.Terminal(contact.Terminal);
                float direction = contact.Terminal == BatteryTerminal.Positive ? -1 : 1;
                float offset = index == 0 ? -.031f : .031f;
                position = new Vector3(0, .016f, offset);
                rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(.49f * direction, .87f, index == 0 ? -.21f : .21f));
            }
            float speed = 1 - Mathf.Exp(-19 * seconds);
            Transform probe = probes[index];
            if (probe.parent != anchor) probe.SetParent(anchor, true);
            probe.SetLocalPositionAndRotation(Vector3.Lerp(probe.localPosition, position, speed),
                Quaternion.Slerp(probe.localRotation, rotation, speed));
            probe.localScale = Vector3.Lerp(probe.localScale, Vector3.one, speed);
        }

        public Vector2 ProjectCable(int index, Rect area) => Project(cableEnds[index].position, area);
        public Vector2 ProjectDock(int index, Rect area) => Project(docks[index].position, area);
        public Vector2 ProjectCell(BatteryCell cell, Rect area) => Project(compartmentCells[cell].Root.position, area);
        public Vector2 ProjectTray(int index, Rect area) => Project(tray[index].position, area);
        public Vector2 ProjectSlot(int index, Rect area) => Project(seats[index].position, area);
        public Vector2 ProjectTerminal(BatteryCell cell, BatteryTerminal terminal, Rect area) =>
            Project(compartmentCells[cell].Terminal(terminal).position, area);

        private Vector2 Project(Vector3 position, Rect area)
        {
            Vector3 point = camera.WorldToViewportPoint(position);
            return new Vector2(area.x + point.x * area.width, area.y + (1 - point.y) * area.height);
        }
    }
}
