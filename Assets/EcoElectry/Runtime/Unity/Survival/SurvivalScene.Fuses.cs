using EcoElectry.Application.Survival;
using EcoElectry.Unity.Player;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    public sealed partial class SurvivalScene
    {
        private enum RepairView { None, Box, Spares, Journal, Inventory }

        [SerializeField] private FuseRepairDefinition repairDefinition;
        [SerializeField] private FuseRepairInteractable fuseSpare;

        private FuseRepair repair;
        private FuseRepairInteractable aimedRepair;
        private RepairView repairView;
        private bool driverEquipped;
        private bool taskSheetOpen;
        private string repairFeedback;
        private AudioClip continuityClip;
        private AudioSource continuityAudio;

        private void PrepareRepair()
        {
            if (repairDefinition == null) return;
            repair = repairDefinition.Enter(session);
            SyncSpareVisibility();
        }

        private void SyncSpareVisibility()
        {
            if (fuseSpare != null) fuseSpare.gameObject.SetActive(repair.Position(FuseRepair.SlotCount) == FusePosition.World);
        }

        private static string RepairPrompt(FuseRepairInteractable.Kind kind) => kind switch
        {
            FuseRepairInteractable.Kind.Box => "E  Inspeccionar caja de fusibles",
            FuseRepairInteractable.Kind.Spares => "E  Abrir caja de repuestos",
            _ => "E  Recoger fusible de repuesto"
        };

        private void InteractRepairWorld(FuseRepairInteractable target)
        {
            switch (target.Type)
            {
                case FuseRepairInteractable.Kind.Box:
                    ShowRepairView(RepairView.Box);
                    break;
                case FuseRepairInteractable.Kind.Spares:
                    repair.ReadClue();
                    ShowRepairView(RepairView.Spares);
                    break;
                case FuseRepairInteractable.Kind.Spare:
                    if (!repair.TryCollectSpare())
                    {
                        Notify("Consulta la caja de repuestos en Limpieza.");
                        return;
                    }
                    SyncSpareVisibility();
                    Notify("Fusible guardado. Pulsa J para verlo.");
                    break;
            }
        }

        private void ShowRepairView(RepairView next)
        {
            if (repairView == RepairView.Box) StowRepairProbes();
            repairView = next;
            repairFeedback = "";
            taskSheetOpen = false;
            switch (next)
            {
                case RepairView.Box:
                    GetPreview().ShowFuseBox(repairDefinition, repair);
                    repair.Meter.ContinuityReached -= PlayContinuityTone;
                    repair.Meter.ContinuityReached += PlayContinuityTone;
                    break;
                case RepairView.Spares:
                    GetPreview().ShowEmptySpares(repairDefinition);
                    break;
                case RepairView.Inventory:
                    GetPreview().ShowSpareFuse(repairDefinition, repair.Spare);
                    break;
                default:
                    preview?.Hide();
                    break;
            }
            HorrorCorridorExplorer.SetCursorLocked(false);
        }

        private void CloseRepair()
        {
            if (repairView == RepairView.None) return;
            repair.Meter.ContinuityReached -= PlayContinuityTone;
            StowRepairProbes();
            repairView = RepairView.None;
        }

        private void StowRepairProbes()
        {
            repair.Meter.StoreProbes();
            heldProbe = null;
            if (continuityAudio != null) continuityAudio.Stop();
        }

        private void PlayContinuityTone()
        {
            if (continuityClip == null)
            {
                const int rate = 24000;
                var samples = new float[rate / 6];
                for (int i = 0; i < samples.Length; i++)
                {
                    float envelope = Mathf.Clamp01(Mathf.Min(i, samples.Length - i - 1) / 188f);
                    samples[i] = .35f * envelope * Mathf.Sin(2f * Mathf.PI * 1780f * i / rate);
                }
                continuityClip = AudioClip.Create("Continuidad", samples.Length, 1, rate, false);
                continuityClip.SetData(samples, 0);
                continuityAudio = gameObject.AddComponent<AudioSource>();
                continuityAudio.playOnAwake = false;
                continuityAudio.spatialBlend = 0;
            }
            continuityAudio.PlayOneShot(continuityClip);
        }

        private void InteractWithFuseBox(InspectionPreview.Hit hit)
        {
            if (preview.IsMovingRepairPart || repair.Completed || hit.Kind == InspectionPreview.TargetKind.None) return;
            repairFeedback = "";
            if (!repair.SupplyCut)
            {
                repairFeedback = "Comprueba el corte en el tablero de Mantenimiento antes de intervenir.";
                return;
            }

            if (hit.Kind == InspectionPreview.TargetKind.Probe)
            {
                var probe = (MeterProbe)hit.Index;
                repair.Meter.Lift(probe);
                heldProbe = heldProbe == probe ? null : probe;
                return;
            }

            if (heldProbe.HasValue)
            {
                if (hit.Kind == InspectionPreview.TargetKind.FuseTerminal &&
                    repair.Meter.TryContact(heldProbe.Value, hit.Index, hit.Terminal))
                    heldProbe = null;
                else repairFeedback = "Apoya la punta en un extremo metálico del fusible del banco.";
                return;
            }

            switch (hit.Kind)
            {
                case InspectionPreview.TargetKind.Screw:
                    if (!repair.CoverOn) repairFeedback = "Coloca la tapa antes de fijarla.";
                    else if (!driverEquipped) repairFeedback = "Equipa el destornillador para girar el tornillo.";
                    else if (repair.TryTurnScrew(hit.Index))
                        preview.MoveScrew(hit.Index, repair.IsFastened(hit.Index));
                    else repairFeedback = "Retira la tapa y revisa los fusibles.";
                    break;
                case InspectionPreview.TargetKind.Cover:
                    if (repair.TryMoveCover()) preview.MoveCover(repair);
                    else repairFeedback = repair.CoverOn ? "Retira los cuatro tornillos para liberar la tapa." :
                        "Monta los tres fusibles compatibles y comprobados antes de cerrar.";
                    break;
                case InspectionPreview.TargetKind.FuseSlot:
                    MountFuse(hit.Index);
                    break;
                case InspectionPreview.TargetKind.Fuse:
                case InspectionPreview.TargetKind.FuseTerminal:
                    SelectFuse(hit.Index);
                    break;
            }
        }

        private void SelectFuse(int index)
        {
            if (preview.IsMovingRepairPart || repair.Completed || !repair.SupplyCut) return;
            var previous = repair.Position(index);
            if (previous == FusePosition.Bench) return;
            int bench = repair.At(FusePosition.Bench);
            if (repair.TryMoveToBench(index))
            {
                StowRepairProbes();
                preview.MoveFuse(index, previous, bench, repair);
                repairFeedback = "";
            }
            else repairFeedback = "Retira la tapa para acceder al banco de pruebas.";
        }

        private void MountFuse(int slot)
        {
            int index = repair.At(FusePosition.Bench);
            FuseMountResult result = repair.TryMount(slot);
            repairFeedback = result switch
            {
                FuseMountResult.Incompatible => $"La ranura {(char)('A' + slot)} requiere {repair.RequiredReference(slot)}.",
                FuseMountResult.Unverified => "Mide la continuidad de este fusible antes de instalarlo.",
                FuseMountResult.Open => "El fusible está abierto. Déjalo en la bandeja y monta un repuesto sano.",
                FuseMountResult.Blocked => "Coloca primero un fusible en el banco de pruebas.",
                _ => ""
            };
            if (result != FuseMountResult.Mounted) return;
            StowRepairProbes();
            preview.MoveFuse(index, FusePosition.Bench, -1, repair);
        }
    }
}
