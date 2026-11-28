using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using EcoElectry.Unity.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EcoElectry.Unity.Survival
{
    [DefaultExecutionOrder(5)]
    [DisallowMultipleComponent, RequireComponent(typeof(HorrorCorridorExplorer))]
    public sealed partial class SurvivalScene : MonoBehaviour
    {
        [SerializeField] private SurvivalSettings settings = null;
        [SerializeField] private Camera view = null;
        [SerializeField] private Material previewMaterial = null;
        [SerializeField] private InspectionModels inspectionModels = null;
        [SerializeField] private BatteryPickup[] pickups = System.Array.Empty<BatteryPickup>();
        [SerializeField] private string continuationScene = "";
        [SerializeField] private bool directEntrySupplyCut;
        [SerializeField, Min(.1f)] private float reach = 2.3f;

        private HorrorCorridorExplorer explorer;
        private SurvivalSession session;
        private FearRules fearRules;
        private InspectionPreview preview;
        private BatteryCase openCase;
        private BatteryCase aimedCase;
        private BatteryPickup aimedBattery;
        private BatteryCell selectedCell;
        private MeterProbe? heldProbe;
        private bool manualOpen;
        private bool paused;
        private float previousTimeScale;
        private bool loading;
        private string message = "";
        private float messageUntil;

        internal bool IsReady => isActiveAndEnabled && session != null;
        internal bool BlocksExplorerInput => IsReady && (paused || loading || session.IsDefeated || Inspecting);
        private bool Inspecting => session.IsInspecting || openCase != null || repairView != RepairView.None;

        private void Awake()
        {
            explorer = GetComponent<HorrorCorridorExplorer>();
            if (settings == null || view == null || previewMaterial == null || inspectionModels == null || !inspectionModels.Assigned)
            {
                Debug.LogError("Asigna la configuración, cámara, material y modelos del visor de linterna.", this);
                enabled = false;
                return;
            }

            session = SurvivalRun.Enter(settings, directEntrySupplyCut);
            PrepareRepair();
            fearRules = settings.CreateFearRules();
            foreach (BatteryPickup pickup in pickups)
                if (pickup != null) pickup.ApplySession(session);
            explorer.PresentBeam(session.HasBeam);
        }

        private void OnEnable()
        {
            if (session != null) session.Fainted += OnFainted;
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.Fainted -= OnFainted;
                if (session.IsInspecting && preview != null) session.CancelInspection();
            }
            openCase = null;
            CloseRepair();
            meterFace.CancelInput();
            preview?.Hide();
            if (paused) ReturnTimeToGame();
        }

        private void OnDestroy()
        {
            preview?.Dispose();
            if (continuityClip != null) Destroy(continuityClip);
        }

        internal void ToggleFlashlight() => session.ToggleFlashlight();

        private void Update()
        {
            if (loading || session.IsDefeated || explorer.IsImageMenuOpen || gameObject.scene != SceneManager.GetActiveScene())
                return;

            var keyboard = Keyboard.current;
            if (keyboard?.pKey.wasPressedThisFrame == true)
            {
                SetPaused(!paused);
                return;
            }

            if (paused || Time.timeScale <= 0) return;
            if (keyboard?.escapeKey.wasPressedThisFrame == true && Inspecting)
            {
                CloseInspection();
                return;
            }

            if (repair != null && keyboard?.jKey.wasPressedThisFrame == true && !session.IsInspecting && openCase == null)
            {
                if (repairView == RepairView.None) ShowRepairView(RepairView.Journal);
                else if (repairView is RepairView.Journal or RepairView.Inventory) CloseInspection();
                else taskSheetOpen = !taskSheetOpen;
                return;
            }

            if (openCase != null || repairView != RepairView.None || keyboard?.iKey.wasPressedThisFrame != true) return;
            if (session.IsInspecting) CloseInspection();
            else if (session.BeginInspection()) ShowCompartment();
            else Notify("Busca una batería de repuesto.");
        }

        private void LateUpdate()
        {
            if (gameObject.scene != SceneManager.GetActiveScene()) return;
            session.Advance(Time.deltaTime, settings.SecondsPerBattery, fearRules, paused || loading);
            explorer.PresentBeam(session.HasBeam);
            aimedBattery = null;
            aimedCase = null;
            aimedRepair = null;
            if (paused || loading || session.IsDefeated || Time.timeScale <= 0) return;

            if (session.IsInspecting)
            {
                preview.UpdateCompartment(session.Inspection, selectedCell, Time.deltaTime);
                return;
            }

            if (repairView != RepairView.None)
            {
                if (repairView == RepairView.Box) preview.UpdateRepair(repair, Time.deltaTime);
                return;
            }

            if (openCase != null || explorer.IsImageMenuOpen || Cursor.lockState != CursorLockMode.Locked) return;
            Ray aim = view.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if (!Physics.Raycast(aim, out RaycastHit hit, reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                return;

            aimedBattery = hit.collider.GetComponentInParent<BatteryPickup>();
            aimedCase = hit.collider.GetComponentInParent<BatteryCase>();
            aimedRepair = hit.collider.GetComponentInParent<FuseRepairInteractable>();
            if (Keyboard.current?.eKey.wasPressedThisFrame != true) return;
            if (aimedRepair != null && aimedRepair.Definition == repairDefinition && repair != null)
                InteractRepairWorld(aimedRepair);
            else if (aimedBattery != null && Collect(aimedBattery))
            {
                aimedBattery = null;
                aimedCase = null;
            }
            else if (aimedCase != null)
            {
                openCase = aimedCase;
                panelScroll = Vector2.zero;
                GetPreview().ShowCase(openCase.IsOpen, openCase.Battery != null && openCase.Battery.Available);
                HorrorCorridorExplorer.SetCursorLocked(false);
            }
        }

        private InspectionPreview GetPreview() => preview ??= new InspectionPreview(previewMaterial, inspectionModels);

        private bool Collect(BatteryPickup battery)
        {
            if (!battery.TryCollect(session)) return false;
            Notify("Batería guardada como repuesto.");
            return true;
        }

        private void ShowCompartment()
        {
            selectedCell = null;
            heldProbe = null;
            manualOpen = false;
            GetPreview().ShowFlashlight(session.Inspection);
            explorer.PresentBeam(false);
            HorrorCorridorExplorer.SetCursorLocked(false);
        }

        private void InteractWithCompartment(InspectionPreview.Hit target)
        {
            if (preview.IsMovingCell) return;
            BatteryInspection inspection = session.Inspection;
            var meter = inspection.Meter;
            if (target.Kind == InspectionPreview.TargetKind.Probe)
            {
                var probe = (MeterProbe)target.Index;
                meter.Lift(probe);
                heldProbe = heldProbe == probe ? null : probe;
                selectedCell = null;
                return;
            }

            if (heldProbe.HasValue)
            {
                if (target.Kind == InspectionPreview.TargetKind.Terminal &&
                    meter.TryContact(heldProbe.Value, target.Cell, (BatteryTerminal)target.Index))
                    heldProbe = null;
                return;
            }

            if (target.Kind == InspectionPreview.TargetKind.Slot)
            {
                PlaceSelectedCell(target.Index);
                return;
            }
            if (target.Kind != InspectionPreview.TargetKind.Cell && target.Kind != InspectionPreview.TargetKind.Terminal) return;

            BatteryCell cell = target.Cell;
            int slot = inspection.FindInstalled(cell);
            if (slot >= 0 && inspection.TryExtract(slot, out int home))
            {
                preview.TransferCell(cell, home, true);
                return;
            }

            if (inspection.FindOnTray(cell) < 0) return;
            selectedCell = selectedCell == cell ? null : cell;
            if ((inspection.Slots[0] == null) != (inspection.Slots[1] == null))
                PlaceSelectedCell(inspection.Slots[0] == null ? 0 : 1);
        }

        private void PlaceSelectedCell(int slot)
        {
            if (preview.IsMovingCell || selectedCell == null) return;
            BatteryInspection inspection = session.Inspection;
            if (!inspection.TryInsert(inspection.FindOnTray(selectedCell), slot)) return;
            preview.TransferCell(selectedCell, slot, false);
            selectedCell = null;
        }

        private void StoreProbes()
        {
            session.Inspection.Meter.StoreProbes();
            heldProbe = null;
        }

        private void ConfirmCompartment()
        {
            if (preview.IsMovingCell || !session.ConfirmInspection()) return;
            CloseInspection();
            Notify("Montaje de la linterna guardado.");
        }

        private void CloseInspection()
        {
            session.CancelInspection();
            heldProbe = null;
            selectedCell = null;
            openCase = null;
            CloseRepair();
            meterFace.CancelInput();
            preview?.Hide();
            explorer.PresentBeam(session.HasBeam);
            HorrorCorridorExplorer.SetCursorLocked(!paused && !session.IsDefeated);
        }

        private void OnFainted()
        {
            CloseInspection();
            HorrorCorridorExplorer.SetCursorLocked(false);
        }

        private void SetPaused(bool value)
        {
            if (value == paused) return;
            if (value)
            {
                paused = true;
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
                meterFace.CancelInput();
                preview?.CancelPointer();
                if (repairView == RepairView.Box)
                {
                    preview.FinishRepairMotion();
                    StowRepairProbes();
                    preview.UpdateRepair(repair, 1f);
                }
            }
            else ReturnTimeToGame();
            HorrorCorridorExplorer.SetCursorLocked(!paused && !Inspecting && !session.IsDefeated);
        }

        private void ReturnTimeToGame()
        {
            Time.timeScale = previousTimeScale;
            paused = false;
        }

        private void LoadScene(string path, bool restart)
        {
            if (loading) return;
            int index = SceneUtility.GetBuildIndexByScenePath(path);
            if (index < 0)
            {
                Notify("La escena de destino no está registrada.");
                return;
            }
            if (paused) ReturnTimeToGame();
            CloseInspection();
            if (restart) SurvivalRun.StartNew();
            else if (path == continuationScene) SurvivalRun.CutSupply();
            loading = true;
            SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        }

        private void Notify(string text)
        {
            message = text;
            messageUntil = Time.unscaledTime + 2.8f;
        }
    }
}
