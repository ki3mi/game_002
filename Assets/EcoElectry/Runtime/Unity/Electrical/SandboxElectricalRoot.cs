using System;
using EcoElectry.Application.Electrical;
using EcoElectry.Domain.Electrical;
using EcoElectry.Unity.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Electrical
{
    [DisallowMultipleComponent, RequireComponent(typeof(LevelRoot))]
    public sealed partial class SandboxElectricalRoot : MonoBehaviour
    {
        [SerializeField] private Camera sandboxCamera;

        private SandboxDefinition definition;
        private SandboxWorld world;
        private CharacterController player;
        private GameObject avatar;
        private Vector3 focus;
        private float yaw;
        private float pitch;
        private float vertical;
        private float playbackStart;
        private int caseIndex;
        private CableTool? activeTool;
        private CableFamily family = CableFamily.Solar;
        private bool choosingOrigin;
        private bool intervention;
        private bool thirdPerson = true;

        public SandboxSession Session { get; private set; }
        public bool IsFirstPerson => intervention || !thirdPerson;
        public bool IsIntervening => intervention;
        public Vector3 PlayerPosition => player.transform.position;
        public Camera Camera => sandboxCamera;

        private void Awake()
        {
            definition = GetComponent<LevelRoot>().Definition as SandboxDefinition;
            if (definition == null || definition.Cases == null || definition.Cases.Count == 0 || sandboxCamera == null)
            {
                Debug.LogError("El sandbox requiere una definición con casos y una cámara local.", this);
                enabled = false;
                return;
            }

            Session = new SandboxSession();
            world = new SandboxWorld(definition, transform);
            InitializePlacement();
            var pawn = new GameObject("Jugador sandbox");
            pawn.transform.SetParent(transform, false);
            player = pawn.AddComponent<CharacterController>();
            player.height = 1.75f;
            player.radius = 0.28f;
            player.center = Vector3.up * 0.875f;

            avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Cuerpo del jugador";
            avatar.transform.SetParent(pawn.transform, false);
            avatar.transform.localPosition = Vector3.up * 0.82f;
            avatar.transform.localScale = new Vector3(0.45f, 0.8f, 0.45f);
            avatar.GetComponent<Collider>().enabled = false;

            LoadCase(0);
            UpdateCamera();
        }

        public void LoadCase(int index)
        {
            if (index < 0 || index >= definition.Cases.Count) throw new ArgumentOutOfRangeException(nameof(index));
            caseIndex = index;
            Session.Load(definition.Cases[index].CreateCase());
            world.Load(definition.Cases[index]);
            ResetPlacement();
            FinishIntervention();
            thirdPerson = true;
            Respawn();
            scroll = Vector2.zero;
            world.SetLamp(false);
            RefreshWorld();
        }

        public void ResetCase() => LoadCase(caseIndex);

        public void ToggleCamera()
        {
            if (intervention) return;
            thirdPerson = !thirdPerson;
            UpdateCamera();
        }

        public void SelectTool(CableTool tool)
        {
            Session.CancelRoute();
            choosingOrigin = false;
            activeTool = tool;
            EnterIntervention(world.At(Terminal.LightingOut));
            RefreshWorld();
        }

        public InteractionResult ApplyTool(Terminal terminal)
        {
            if (!activeTool.HasValue) return InteractionResult.Rejected;
            EnterIntervention(world.At(terminal));
            InteractionResult result = Session.Use(activeTool.Value, terminal);
            if (result == InteractionResult.Shock)
            {
                FinishIntervention();
                Respawn();
                world.SetLamp(false);
            }
            RefreshWorld();
            return result;
        }

        public bool StartRoute(Terminal origin, CableFamily selectedFamily)
        {
            if (!Session.StartRoute(origin, selectedFamily)) return false;
            activeTool = null;
            choosingOrigin = false;
            EnterIntervention(world.At(origin));
            RefreshWorld();
            return true;
        }

        public void FinishIntervention()
        {
            Session.CancelRoute();
            choosingOrigin = false;
            activeTool = null;
            intervention = false;
            RefreshWorld();
        }

        public void AddRoutePoint(RoutePoint point)
        {
            Session.AddPoint(point);
            RefreshWorld();
        }

        public bool ConfirmRoute()
        {
            if (!Session.ConfirmRoute()) return false;
            FinishIntervention();
            return true;
        }

        public void ChooseDestination(Terminal terminal)
        {
            Session.ChooseDestination(terminal);
            RefreshWorld();
        }

        public void Place(Equipment equipment)
        {
            CancelPlacement();
            Session.Place(equipment);
            RefreshWorld();
        }

        public SandboxReport RunTest()
        {
            CancelPlacement();
            FinishIntervention();
            playbackStart = Time.unscaledTime;
            SandboxReport report = Session.Test();
            RefreshWorld();
            return report;
        }

        public void CloseReport()
        {
            Session.CloseReport();
            world.SetLamp(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    if (Session.ReportOpen) CloseReport();
                    else if (showHelp) showHelp = false;
                    else if (categoryOpen) categoryOpen = false;
                    else if (HasSelectedModel) CancelPlacement();
                    else FinishIntervention();
                }

                if (!Session.ReportOpen && !showHelp)
                {
                    if (keyboard.cKey.wasPressedThisFrame) ToggleCamera();
                    if (keyboard.rKey.wasPressedThisFrame && !categoryOpen) placement?.Rotate();
                    if (!intervention && !categoryOpen) Move(keyboard);
                }
            }

            if (Session.ReportOpen)
                world.SetLamp(ClassMinutes < Session.Report.LitMinutes && ClassMinutes >= 0);
        }

        private int ClassMinutes
        {
            get
            {
                float elapsed = Time.unscaledTime - playbackStart;
                return elapsed < 0.6f ? -1 : Mathf.Min(119, (int)((elapsed - 0.6f) * 56));
            }
        }

        private void Move(Keyboard keyboard)
        {
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * 0.16f;
                pitch = Mathf.Clamp(pitch - delta.y * 0.13f, -60f, 66f);
            }

            float x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            float z = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
            Vector3 direction = Quaternion.Euler(0, yaw, 0) * Vector3.ClampMagnitude(new Vector3(x, 0, z), 1);
            vertical = player.isGrounded ? -1 : vertical - 19f * Time.deltaTime;
            player.Move((direction * 4.5f + Vector3.up * vertical) * Time.deltaTime);
            if (player.transform.position.y < -3.5f) Respawn();
        }

        private void Respawn()
        {
            player.enabled = false;
            player.transform.position = Session.SpawnId == SandboxSession.BoardSpawn ? SandboxWorld.BoardSpawn : SandboxWorld.Yard;
            player.enabled = true;
            vertical = 0;
            yaw = 20;
            pitch = 16;
            UpdateCamera();
        }

        private void EnterIntervention(Vector3 target)
        {
            CancelPlacement();
            focus = target;
            intervention = true;
            UpdateCamera();
        }

        private void LateUpdate()
        {
            UpdateCamera();
            UpdatePlacementPreview();
        }

        private void UpdateCamera()
        {
            if (player == null) return;
            avatar.SetActive(!IsFirstPerson);
            Transform rig = sandboxCamera.transform;
            sandboxCamera.fieldOfView = intervention ? 52f : thirdPerson ? 62f : 68f;
            if (intervention)
            {
                rig.position = focus + new Vector3(0, 0.38f, -2.0f);
                rig.LookAt(focus + Vector3.up * 0.12f);
                return;
            }

            Quaternion look = Quaternion.Euler(pitch, yaw, 0);
            Vector3 eye = player.transform.position + Vector3.up * 1.62f;
            rig.position = thirdPerson ? eye + look * new Vector3(1.03f, 2.05f, -7.25f) : eye;
            rig.rotation = look;
        }

        private void RefreshWorld() => world.UpdateInstallation(Session);

        private void OnDestroy()
        {
            placement?.Dispose();
            world?.Dispose();
            Session = null;
        }
    }
}
