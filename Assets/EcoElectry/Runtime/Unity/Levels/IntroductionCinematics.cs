using System.Collections.Generic;
using EcoElectry.Unity.Player;
using EcoElectry.Unity.Survival;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent]
    public sealed class IntroductionCinematics : MonoBehaviour
    {
        public const float ParkingMoveDuration = 2.4f;
        public const float ArrivalCabinDuration = 5f;
        public const float ArrivalParkedTime = 11f;
        public const float GuardShotStart = 13.65f;
        public const float GuardShotEnd = 16.25f;

        private const float DrivingGuideDuration = 3.5f;

        private enum Phase { Opening, DrivingGuide, Driving, Arrival, Walking }

        [SerializeField] private IntroductionTruckDrive truck;
        [SerializeField] private IntroductionSkyCycle sky;
        [SerializeField] private VehicleArrivalTransition arrival;
        [SerializeField] private PlayableDirector opening;
        [SerializeField] private PlayableDirector arrivalSequence;
        [SerializeField] private Camera openingView;
        [SerializeField] private Camera arrivalView;
        [SerializeField] private GameObject cockpit;
        [SerializeField] private AudioSource motor;
        [SerializeField] private AudioSource forest;
        [SerializeField] private AudioClip[] guilleCallClips;
        [SerializeField] private Transform guardWaypoint;
        [SerializeField] private Transform maintenanceWaypoint;
        [SerializeField] private SurvivalSettings tutorialSettings;
        [SerializeField] private InspectionModels tutorialModels;
        [SerializeField] private Material tutorialPreviewMaterial;
        [SerializeField, Min(1f)] private float openingSpeedKmh = 100f;

        private Phase phase;
        private Camera cabinCamera;
        private AudioListener cabinListener;
        private float laneOffset, roadHeightOffset;
        private bool exitShot;
        private bool settled;
        private bool highwayArrival;
        private IntroductionDrivingCall guilleCall;
        private IntroductionDashboard dashboard;
        private float guideStartTime;
        private float guideStartDistance;
        private Camera walkingView;
        private IntroductionGuard guard;
        private IntroductionVhs vhs;
        private HorrorCorridorExplorer walkingExplorer;
        private SurvivalScene workshopTutorial;
        private Transform workshopRoom;
        private bool cheatsOpen;
        private float cheatTimeScale;
        private CursorLockMode savedCursorLock;
        private bool savedCursorVisible;
        private BoxCollider maintenanceLock;
        private Transform maintenanceHinge;
        private Quaternion maintenanceOpenRotation;
        private readonly HashSet<AudioSource> vehicleAudio = new();
        private readonly List<AudioSource> parkedAudio = new();

        internal bool IsDriving => phase == Phase.Driving;
        private float OpeningSpeed => openingSpeedKmh / 3.6f;

        private void Start()
        {
            phase = Phase.Opening;
            cabinCamera = cockpit.GetComponent<Camera>();
            cabinListener = cockpit.GetComponent<AudioListener>();
            sky.SampleRoad(0f, out Vector3 center, out Vector3 tangent);
            Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;
            Vector3 offset = truck.transform.position - center;
            laneOffset = Vector3.Dot(offset, side);
            roadHeightOffset = offset.y;
            truck.BeginCinematicMotion();
            ShowView(openingView.gameObject);
            opening.stopped += OpeningStopped;
            arrivalSequence.stopped += ArrivalStopped;
            vehicleAudio.UnionWith(truck.GetComponentsInChildren<AudioSource>(true));
            vehicleAudio.Add(motor);
            // This loop is the road ambience heard from the cabin.
            vehicleAudio.Add(forest);
            if (motor.clip != null)
            {
                foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (source.gameObject.scene == gameObject.scene && source.clip == motor.clip) vehicleAudio.Add(source);
            }
            if (motor.clip != null) motor.Play();
            if (forest.clip != null) forest.Play();
            guilleCall = new IntroductionDrivingCall(transform, guilleCallClips);
            workshopRoom = maintenanceWaypoint != null ? maintenanceWaypoint.parent.parent : null;
            vhs = IntroductionVhs.Create(workshopRoom?.Find("Banco de trabajo y vídeo"));
            workshopTutorial = IntroductionWorkshop.Setup(workshopRoom, arrival.Walker, tutorialSettings,
                tutorialModels, tutorialPreviewMaterial);
            dashboard = IntroductionDashboard.Create(truck.transform);
            if (guardWaypoint != null)
            {
                guard = new IntroductionGuard(guardWaypoint);
                LockMaintenanceAccess();
            }
            opening.Play();
        }

        private void OnDisable()
        {
            if (cheatsOpen) SetCheatsOpen(false);
            guilleCall?.Stop();
            guilleCall?.ShowGuide(false);
            guilleCall?.Waypoint.Hide();
            guard?.Stop();
            vhs?.HidePrompt();
        }

        private void Update()
        {
            Keyboard keys = Keyboard.current;
            if (keys?.lKey.wasPressedThisFrame == true || (cheatsOpen && keys?.escapeKey.wasPressedThisFrame == true))
            {
                if (!cheatsOpen && (truck.ImageMenuOpen || walkingExplorer?.IsImageMenuOpen == true || Time.timeScale <= 0f))
                    return;
                SetCheatsOpen(!cheatsOpen);
            }
        }

        private void OnGUI()
        {
            if (!cheatsOpen)
            {
                vhs?.DrawPrompt();
                return;
            }

            float width = Mathf.Min(440f, Screen.width - 24f);
            GUILayout.BeginArea(new Rect((Screen.width - width) * 0.5f, (Screen.height - 210f) * 0.5f, width, 210f),
                GUI.skin.box);
            GUILayout.Label("Cheats · Nivel 00");
            int destination = -1;
            if (GUILayout.Button("Estacionamiento · llegada al colegio", GUILayout.Height(33))) destination = 0;
            if (GUILayout.Button("Mantenimiento · antes de reproducir el VHS", GUILayout.Height(33))) destination = 1;
            if (GUILayout.Button("Tutorial · después del VHS", GUILayout.Height(33))) destination = 2;
            if (GUILayout.Button("Volver · L / Esc", GUILayout.Height(28))) destination = 3;
            GUILayout.EndArea();
            if (destination < 0) return;
            SetCheatsOpen(false);
            if (destination < 3) JumpTo(destination);
        }

        private void SetCheatsOpen(bool value)
        {
            cheatsOpen = value;
            if (value)
            {
                cheatTimeScale = Time.timeScale;
                savedCursorLock = Cursor.lockState;
                savedCursorVisible = Cursor.visible;
                Time.timeScale = 0f;
            }
            else Time.timeScale = cheatTimeScale;
            vhs?.SetMenuPaused(value);
            if (workshopTutorial != null && workshopTutorial.IsReady) workshopTutorial.SetExternalMenu(value);
            else
            {
                Cursor.lockState = value ? CursorLockMode.None : savedCursorLock;
                Cursor.visible = value || savedCursorVisible;
            }
        }

        private void JumpTo(int destination)
        {
            if (phase != Phase.Walking)
            {
                if (phase != Phase.Driving)
                {
                    phase = Phase.Driving;
                    opening.Stop();
                    arrivalSequence.Stop();
                    guilleCall.ShowGuide(false);
                    ResumeAfterObstruction();
                }
                if (!arrival.GoToParking()) return;
                if (destination == 0) return;
                phase = Phase.Arrival;
                sky.BeginNight();
                DeliverWalking();
            }
            else if (destination == 0)
            {
                arrival.GoToParking();
                return;
            }

            if (phase != Phase.Walking || workshopRoom == null ||
                !arrival.MoveWalker(workshopRoom.TransformPoint(new Vector3(0f, 0.25f, -1.28f)), workshopRoom.forward)) return;
            workshopTutorial?.CloseWorkshopInspection();
            guilleCall.Stop();
            guard?.Skip();
            OpenMaintenanceAccess();
            guilleCall.Waypoint.Complete();
            vhs?.JumpToTutorial(destination == 2);
            workshopTutorial?.SetTutorialStarted(destination == 2);
        }

        private void OnDestroy()
        {
            if (opening != null) opening.stopped -= OpeningStopped;
            if (arrivalSequence != null) arrivalSequence.stopped -= ArrivalStopped;
            guilleCall?.Dispose();
            dashboard?.Dispose();
            vhs?.Dispose();
        }

        private void LateUpdate()
        {
            if (cheatsOpen) return;
            if (IsDriving)
            {
                dashboard?.UpdateReadouts(truck.transform.position, truck.Speed);
            }
            if (phase == Phase.Opening) ApplyOpeningPose((float)opening.time * OpeningSpeed);
            else if (phase == Phase.DrivingGuide)
            {
                float elapsed = Mathf.Clamp(Time.time - guideStartTime, 0f, DrivingGuideDuration);
                Vector3 direction = ApplyOpeningPose(guideStartDistance + elapsed * OpeningSpeed);
                if (elapsed >= DrivingGuideDuration) StartDriving(direction);
            }
            else if (phase == Phase.Arrival)
            {
                if (!settled)
                {
                    float time = (float)arrivalSequence.time;
                    float progress = Mathf.Clamp01((time - ArrivalCabinDuration) / ParkingMoveDuration);
                    bool clear = highwayArrival
                        ? arrival.AdvanceHighwayArrival(time, ArrivalCabinDuration, ArrivalParkedTime)
                        : arrival.MoveIntoParking(progress, ParkingMoveDuration);
                    if (!clear)
                    {
                        ResumeAfterObstruction();
                        return;
                    }
                    settled = highwayArrival ? time >= ArrivalParkedTime : progress >= 1f;
                    if (settled) MuteParkedVehicle();
                }
                float shotTime = (float)arrivalSequence.time;
                if (guardWaypoint != null && shotTime >= GuardShotStart && shotTime < GuardShotEnd)
                {
                    Pose pose = GuardShotPose(guardWaypoint, Mathf.InverseLerp(GuardShotStart, GuardShotEnd, shotTime));
                    arrivalView.transform.SetPositionAndRotation(pose.position, pose.rotation);
                }
                else if (exitShot && arrival.TryGetExitView(out Vector3 eyes, out Quaternion rotation))
                    arrivalView.transform.SetPositionAndRotation(eyes, rotation);
            }

            bool walking = phase == Phase.Walking;
            if ((IsDriving || phase == Phase.Arrival || walking) && guard?.Started != true)
                guilleCall.Tick(IsDriving ? sky.DrivingProgress : 0f, (IsDriving && !truck.ImageMenuOpen) || walking,
                    walking || (phase == Phase.Arrival && settled), walking);
            if (walking)
            {
                Vector3 position = arrival.Walker.position;
                guard?.Tick(position, guilleCall);
                if (guard?.Finished == true && maintenanceLock != null && maintenanceLock.enabled)
                    OpenMaintenanceAccess();
                guilleCall.Waypoint.Update(walkingView, position, guardWaypoint, maintenanceWaypoint,
                    guard == null || guard.Finished);
                vhs?.Tick(walkingView, walkingExplorer?.IsImageMenuOpen != true && (guard == null || guard.Finished));
                workshopTutorial?.SetTutorialStarted(vhs?.Finished == true);
                if (guilleCall.Waypoint.ReachedWorkshop) ShowWorkshopGuide();
            }

            if (motor.enabled)
            {
                float speed = truck.Speed;
                float movingVolume = Mathf.Lerp(0.07f, 0.28f, Mathf.InverseLerp(0f, 25f, speed));
                motor.volume = Mathf.MoveTowards(motor.volume, movingVolume, Time.deltaTime * 0.5f);
                motor.pitch = Mathf.Lerp(0.60f, 1.5f, Mathf.InverseLerp(0f, 28f, speed));
            }
        }

        private void ShowWorkshopGuide()
        {
            IntroductionWaypoint guide = guilleCall.Waypoint;
            if (walkingView == null || walkingExplorer?.IsImageMenuOpen == true ||
                workshopTutorial?.BlocksExplorerInput == true)
            {
                guide.Hide();
                return;
            }

            if (vhs != null && vhs.TryGetTutorialTarget(out Vector3 position, out string target, out string step))
                guide.ShowTutorial(walkingView, position, target, step);
            else if (vhs?.Finished == true && workshopTutorial?.UncollectedFlashlight is Transform flashlight)
                guide.ShowTutorial(walkingView, flashlight.position + Vector3.up * 0.20f, "Linterna",
                    "Mira la linterna de la mesa y pulsa E para equiparla.");
            else guide.Hide();
        }

        public static Pose GuardShotPose(Transform booth, float progress)
        {
            Vector3 from = new(0.04f, 1.83f, -4.95f);
            Vector3 to = new(0.18f, 1.81f, -4.15f);
            Vector3 point = booth.TransformPoint(Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress)));
            Vector3 face = booth.TransformPoint(0.26f, 1.80f, -1.75f);
            return new Pose(point, Quaternion.LookRotation(face - point, booth.up));
        }

        private void LockMaintenanceAccess()
        {
            if (maintenanceWaypoint == null) return;
            Transform access = maintenanceWaypoint.parent;
            maintenanceHinge = access.Find("PuertaExteriorMetalica/PuertaExteriorMetalica_Ensamble/PuertaExteriorMetalica_PivoteBisagra");
            if (maintenanceHinge != null)
            {
                maintenanceOpenRotation = maintenanceHinge.localRotation;
                maintenanceHinge.localRotation = Quaternion.identity;
            }

            var gate = new GameObject("Acceso pendiente de autorización");
            gate.transform.SetParent(access, false);
            gate.transform.localPosition = new Vector3(0f, 1.33f, -3.89f);
            maintenanceLock = gate.AddComponent<BoxCollider>();
            maintenanceLock.size = new Vector3(1.5f, 2.32f, 0.18f);
        }

        private void OpenMaintenanceAccess()
        {
            if (maintenanceLock != null) maintenanceLock.enabled = false;
            if (maintenanceHinge != null) maintenanceHinge.localRotation = maintenanceOpenRotation;
        }

        private Vector3 ApplyOpeningPose(float distance)
        {
            sky.SampleRoad(distance, out Vector3 point, out Vector3 tangent);
            sky.SampleRoad(distance + 2f, out _, out Vector3 ahead);
            Vector3 forward = (tangent + ahead).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            truck.SetCinematicPose(point + right * laneOffset + Vector3.up * roadHeightOffset,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, Vector3.up)), OpeningSpeed);
            return forward;
        }

        public void CutToCabin()
        {
            if (UnityEngine.Application.isPlaying && phase == Phase.Opening) ShowView(cockpit);
        }

        public void DeliverDriving()
        {
            if (!UnityEngine.Application.isPlaying || phase != Phase.Opening) return;
            guideStartDistance = (float)opening.time * OpeningSpeed;
            guideStartTime = Time.time;
            phase = Phase.DrivingGuide;
            ShowView(cockpit);
            guilleCall.ShowGuide(true);
        }

        private void StartDriving(Vector3 direction)
        {
            guilleCall.ShowGuide(false);
            phase = Phase.Driving;
            truck.ResumeDriving(direction * OpeningSpeed);
            dashboard?.Begin(truck.transform.position, truck.Speed);
            guilleCall.Ring();
        }

        internal void StartArrival(bool fromHighway)
        {
            if (!IsDriving) return;
            dashboard?.UpdateReadouts(truck.transform.position, truck.Speed);
            phase = Phase.Arrival;
            exitShot = false;
            settled = false;
            highwayArrival = fromHighway;
            truck.BeginCinematicMotion();
            ShowView(fromHighway ? cockpit : arrivalView.gameObject);
            arrivalSequence.initialTime = fromHighway ? 0f : ArrivalCabinDuration;
            arrivalSequence.time = arrivalSequence.initialTime;
            arrivalSequence.Play();
        }

        public void CutToFacade()
        {
            if (!UnityEngine.Application.isPlaying || phase != Phase.Arrival) return;
            sky.BeginNight();
            ShowView(arrivalView.gameObject);
        }

        public void CutToExit() => exitShot = UnityEngine.Application.isPlaying && phase == Phase.Arrival;

        public void DeliverWalking()
        {
            if (!UnityEngine.Application.isPlaying || phase != Phase.Arrival) return;
            ShowView(null);
            if (arrival.FinishArrival())
            {
                phase = Phase.Walking;
                MuteParkedVehicle();
                walkingView = arrival.Walker.GetComponentInChildren<Camera>();
                walkingExplorer = arrival.Walker.GetComponent<HorrorCorridorExplorer>();
                dashboard?.Hide();
            }
            else
                ResumeAfterObstruction();
        }

        private void MuteParkedVehicle()
        {
            vehicleAudio.UnionWith(truck.GetComponentsInChildren<AudioSource>(true));
            foreach (AudioSource source in vehicleAudio)
            {
                if (source == null || !source.enabled) continue;
                source.Stop();
                source.enabled = false;
                parkedAudio.Add(source);
            }
        }

        private void ResumeAfterObstruction()
        {
            phase = Phase.Driving;
            truck.ResumeDriving(Vector3.zero);
            foreach (AudioSource source in parkedAudio)
                if (source != null) source.enabled = true;
            parkedAudio.Clear();
            if (motor.clip != null && !motor.isPlaying) motor.Play();
            if (forest.clip != null && !forest.isPlaying) forest.Play();
            dashboard?.Begin(truck.transform.position, truck.Speed);
            guilleCall.Ring();
            ShowView(cockpit);
            arrivalSequence.Stop();
        }

        private void ShowView(GameObject view)
        {
            if (openingView.gameObject != view) openingView.gameObject.SetActive(false);
            if (arrivalView.gameObject != view) arrivalView.gameObject.SetActive(false);
            bool useCabin = view == cockpit;
            cabinCamera.enabled = useCabin;
            cabinListener.enabled = useCabin;
            if (view != null && !useCabin) view.SetActive(true);
        }

        private void OpeningStopped(PlayableDirector director)
        {
            if (phase == Phase.Opening)
            {
                director.time = director.duration;
                DeliverDriving();
            }
        }

        private void ArrivalStopped(PlayableDirector director)
        {
            if (phase == Phase.Arrival) DeliverWalking();
        }
    }
}
