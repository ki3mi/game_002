using System;
using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    internal sealed partial class InspectionPreview
    {
        private static readonly Vector3 FuseBay = new(.08f, -.58f, -.22f);
        private static readonly Vector3 CoverRest = new(1.24f, .61f, .10f);
        private static readonly Vector3 BoxCenter = new(-.77f, .61f, .10f);
        private static readonly Vector3 ForwardOffBox = Vector3.back;
        private static readonly Quaternion DetachedCover = Quaternion.Euler(9, -15, -10);
        private readonly Transform[] fuseParts = new Transform[4];
        private readonly Transform[,] fuseTerminals = new Transform[4, 2];
        private readonly Transform[] screwParts = new Transform[FuseRepair.ScrewCount];
        private readonly Pose[] screwHomes = new Pose[FuseRepair.ScrewCount];
        private readonly Transform[] fuseSeats = new Transform[FuseRepair.SlotCount];
        private Transform repairCover;
        private Transform screwdriver;
        private Pose coverHome;
        private bool repairModel;
        private Action<float> repairMotion;
        private float repairMotionTime;
        private float repairMotionLength;

        public bool IsMovingRepairPart => repairMotion != null;

        public void ShowFuseBox(FuseRepairDefinition definition, FuseRepair repair)
        {
            BeginModel(false);
            repairModel = true;
            ResetView();
            workspace = Group("Banco de pruebas", model, Vector3.zero);

            Transform boxRoot = Spawn(definition.Box.gameObject, model);
            boxRoot.localPosition = BoxCenter;
            boxRoot.localScale = Vector3.one * (1.55f / definition.Box.Width);
            var box = boxRoot.GetComponent<FuseBoxModel>();
            foreach (var collider in box.GetComponentsInChildren<Collider>()) collider.enabled = false;
            var enclosure = box.GetComponent<BoxCollider>();
            Vector3 depth = Vector3.forward * enclosure.size.z;
            HitBox(boxRoot, enclosure.center + depth * .20f, enclosure.size - depth * .44f);

            repairCover = box.Cover;
            var lid = repairCover.GetComponentInChildren<MeshRenderer>();
            AddMeshHit(lid, new Hit(TargetKind.Cover));
            repairCover.SetParent(model, true);
            coverHome = LocalPose(repairCover);
            Pose cover = repair.CoverOn ? coverHome : new Pose(CoverRest, DetachedCover * coverHome.rotation);
            repairCover.SetLocalPositionAndRotation(cover.position, cover.rotation);

            for (int i = 0; i < screwParts.Length; i++)
            {
                Transform screw = box.Screws[i];
                screw.SetParent(model, true);
                screwParts[i] = screw;
                screwHomes[i] = LocalPose(screw);
                var renderer = screw.GetComponentInChildren<MeshRenderer>();
                Bounds bounds = renderer.bounds;
                Vector3 head = model.InverseTransformPoint(new Vector3(bounds.center.x, bounds.center.y, bounds.center.z));
                // The fixing point stays in place while its screw is on the tray.
                var fixing = Group($"Fijación {i + 1}", model, head + Vector3.back * .012f);
                HitBox(fixing, Vector3.zero, new Vector3(.15f, .15f, .16f), new Hit(TargetKind.Screw, i));
                if (!repair.IsFastened(i))
                    screw.SetLocalPositionAndRotation(ScrewRest(i), ScrewTrayRotation(i));
            }

            for (int i = 0; i < fuseSeats.Length; i++)
            {
                fuseSeats[i] = box.Slots[i];
                HitBox(fuseSeats[i], new Vector3(0, 0, .0055f), new Vector3(.065f, .022f, .007f),
                    new Hit(TargetKind.FuseSlot, i));
            }

            Block("Base de prueba", FuseBay + new Vector3(0, 0, .16f), new Vector3(1.25f, .44f, .10f), Rubber, parent: workspace);
            Block("Borde de prueba", FuseBay + new Vector3(0, -.213f, .095f), new Vector3(1.25f, .025f, .026f), Marking, parent: workspace);
            Block("Bandeja", new Vector3(.18f, -1.25f, .10f), new Vector3(3.55f, .48f, .09f), Graphite, parent: workspace);
            Block("Separador", new Vector3(1.29f, -1.23f, .04f), new Vector3(.024f, .42f, .037f), Steel, parent: workspace);

            for (int i = 0; i < fuseParts.Length; i++)
            {
                Transform piece = Spawn(definition.Fuse, model);
                fuseParts[i] = piece;
                foreach (TextMesh label in piece.GetComponentsInChildren<TextMesh>()) label.text = repair.Cartridges[i].Reference;
                HitBox(piece, Vector3.zero, new Vector3(.64f, .18f, .18f), new Hit(TargetKind.Fuse, i));
                for (int side = 0; side < 2; side++)
                {
                    Transform end = piece.Find(side == 0 ? "Terminal izquierdo" : "Terminal derecho");
                    fuseTerminals[i, side] = end;
                    HitBox(end, Vector3.zero, new Vector3(.15f, .195f, .195f), new Hit(TargetKind.FuseTerminal, i, terminal: side));
                }
                UpdateFusePose(i, repair.Position(i));
            }
            ShowFuseSeats(repair, !repair.CoverOn);

            for (int i = 0; i < probes.Length; i++)
                AddProbe(i, new Vector3(-2.21f + i * .44f, -.95f, -.19f), Quaternion.Euler(0, 0, i == 0 ? -17 : -23));
            screwdriver = Spawn(definition.Screwdriver, model);
            screwdriver.gameObject.SetActive(false);
        }

        public void ShowSpareFuse(FuseRepairDefinition definition, FuseCartridge fuse)
        {
            BeginModel(false);
            Transform part = Spawn(definition.Fuse, model);
            part.localScale = Vector3.one * 2.25f;
            foreach (TextMesh label in part.GetComponentsInChildren<TextMesh>()) label.text = fuse.Reference;
        }

        public void ShowEmptySpares(FuseRepairDefinition definition)
        {
            BeginModel(false);
            Transform caseModel = Spawn(definition.Spares, model);
            caseModel.localScale = Vector3.one * 5.35f;
            caseModel.localPosition = new Vector3(0, -.52f, 0);
            caseModel.Find("Tapa").localRotation = Quaternion.Euler(111, 0, 0);
            Rotate(new Vector2(-8, -21));
        }

        private void AddMeshHit(MeshRenderer renderer, Hit target)
        {
            Bounds bounds = renderer.GetComponent<MeshFilter>().sharedMesh.bounds;
            HitBox(renderer.transform, bounds.center, bounds.size, target);
        }

        private void UpdateFusePose(int index, FusePosition position)
        {
            bool visible = position is not (FusePosition.World or FusePosition.Inventory);
            Transform piece = fuseParts[index];
            piece.gameObject.SetActive(visible);
            if (!visible) return;
            var (point, scale) = FusePose(index, position);
            piece.SetLocalPositionAndRotation(point, Quaternion.identity);
            piece.localScale = Vector3.one * scale;
        }

        private (Vector3, float) FusePose(int index, FusePosition position)
        {
            if (position == FusePosition.Bench) return (FuseBay, 1.14f);
            if (position == FusePosition.Tray) return (new Vector3(-1.20f + index * .62f, -1.20f, -.07f), .66f);
            if (position == FusePosition.Inventory) return (new Vector3(1.38f, -.66f, -.12f), .66f);
            return (model.InverseTransformPoint(fuseSeats[(int)position].position), 1.07f);
        }

        public void MoveFuse(int index, FusePosition previous, int previousBench, FuseRepair repair)
        {
            Transform part = fuseParts[index];
            if (previous == FusePosition.Inventory)
            {
                var (entry, scale) = FusePose(index, previous);
                part.SetLocalPositionAndRotation(entry, Quaternion.identity);
                part.localScale = Vector3.one * scale;
                part.gameObject.SetActive(true);
            }
            Action<float> first = FuseMotion(index, repair.Position(index));
            Action<float> second = previousBench >= 0 && previousBench != index
                ? FuseMotion(previousBench, FusePosition.Tray) : null;
            StartRepairMotion(.49f, t =>
            {
                first(t);
                second?.Invoke(t);
            });
        }

        private Action<float> FuseMotion(int index, FusePosition destination)
        {
            Transform piece = fuseParts[index];
            Vector3 start = piece.localPosition;
            Vector3 scale = piece.localScale;
            var (end, endScale) = FusePose(index, destination);
            return t =>
            {
                piece.localPosition = Vector3.Lerp(start, end, t) + ForwardOffBox * (.32f * Mathf.Sin(Mathf.PI * t));
                piece.localScale = Vector3.Lerp(scale, Vector3.one * endScale, t);
            };
        }

        public void MoveCover(FuseRepair repair)
        {
            bool attach = repair.CoverOn;
            Pose start = LocalPose(repairCover);
            Pose end = attach ? coverHome : new Pose(CoverRest, DetachedCover * coverHome.rotation);
            StartRepairMotion(.67f, t =>
            {
                ShowFuseSeats(repair, attach ? t < .94f : t > .06f);
                repairCover.SetLocalPositionAndRotation(
                    Vector3.Lerp(start.position, end.position, t) + ForwardOffBox * (.67f * Mathf.Sin(Mathf.PI * t)),
                    Quaternion.Slerp(start.rotation, end.rotation, t));
            });
        }

        private void ShowFuseSeats(FuseRepair repair, bool visible)
        {
            for (int i = 0; i < fuseSeats.Length; i++) fuseSeats[i].gameObject.SetActive(visible);
            for (int i = 0; i < fuseParts.Length; i++)
                if (repair.Position(i) is FusePosition.SlotA or FusePosition.SlotB or FusePosition.SlotC)
                    fuseParts[i].gameObject.SetActive(visible);
        }

        public void MoveScrew(int index, bool attach)
        {
            Transform screw = screwParts[index];
            Pose home = screwHomes[index];
            Vector3 axial = home.position + ForwardOffBox * .22f;
            Vector3 trayPoint = ScrewRest(index);
            Quaternion trayRotation = ScrewTrayRotation(index);
            StartRepairMotion(.95f, progress =>
            {
                float phase = attach ? 1 - progress : progress;
                bool turning = phase <= .62f;
                float fraction = turning ? phase / .62f : (phase - .62f) / .38f;
                Quaternion spin = Quaternion.AngleAxis(fraction * 720f, Vector3.forward) * home.rotation;
                screw.SetLocalPositionAndRotation(
                    turning ? Vector3.Lerp(home.position, axial, fraction) :
                        Vector3.Lerp(axial, trayPoint, fraction) + ForwardOffBox * (.20f * Mathf.Sin(Mathf.PI * fraction)),
                    turning ? spin : Quaternion.Slerp(home.rotation, trayRotation, fraction));
                screwdriver.gameObject.SetActive(turning && progress < 1);
                if (turning)
                    screwdriver.SetLocalPositionAndRotation(screw.localPosition + ForwardOffBox * .098f,
                        Quaternion.Euler(0, 0, fraction * 720f));
            });
        }

        private static Pose LocalPose(Transform part) => new(part.localPosition, part.localRotation);
        private static Vector3 ScrewRest(int i) => new(1.43f + i % 2 * .24f, -1.11f - i / 2 * .20f, -.055f);
        private static Quaternion ScrewTrayRotation(int i) => Quaternion.Euler(59, -40, i * 21);

        private void StartRepairMotion(float seconds, Action<float> motion)
        {
            repairMotion = motion;
            repairMotionLength = seconds;
            repairMotionTime = 0;
            motion(0);
        }

        public void FinishRepairMotion()
        {
            repairMotion?.Invoke(1);
            repairMotion = null;
        }

        public void UpdateRepair(FuseRepair repair, float seconds)
        {
            if (repairMotion != null)
            {
                repairMotionTime += seconds;
                if (repairMotionTime >= repairMotionLength) FinishRepairMotion();
                else repairMotion(Mathf.SmoothStep(0, 1, repairMotionTime / repairMotionLength));
            }
            for (int i = 0; i < probes.Length; i++)
            {
                FuseContact? contact = repair.Meter.GetContact((MeterProbe)i);
                if (!contact.HasValue)
                {
                    PoseProbe(i, docks[i], Vector3.zero, Quaternion.identity, seconds);
                    continue;
                }
                FuseContact connection = contact.Value;
                Transform terminal = fuseTerminals[connection.Cartridge, connection.Terminal];
                float direction = connection.Terminal == 0 ? -1f : 1f;
                PoseProbe(i, terminal, new Vector3(0, i == 0 ? -.023f : .023f, -.070f),
                    Quaternion.FromToRotation(Vector3.up, new Vector3(direction * .70f, -.56f, -.34f)), seconds);
            }
        }

        public Vector2 ProjectFuse(int index, Rect area) => Project(fuseParts[index].position, area);
        public Vector2 ProjectFuseSeat(int index, Rect area) => Project(fuseSeats[index].position, area);
        public Vector2 ProjectFuseTerminal(int index, int side, Rect area) => Project(fuseTerminals[index, side].position, area);
        public Vector2 ProjectScrewRest(int index, Rect area) => Project(model.TransformPoint(ScrewRest(index)), area);
    }
}
