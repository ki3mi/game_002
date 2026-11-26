using System.Collections.Generic;
using EcoElectry.Unity.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class VehicleArrivalTransition : MonoBehaviour
    {
        [SerializeField] private IntroductionTruckDrive vehicle;
        [SerializeField] private Rigidbody vehicleBody;
        [SerializeField] private GameObject cockpitView;
        [SerializeField] private CharacterController explorer;
        [SerializeField] private Transform driverExit;
        [SerializeField] private Transform fallbackExit;
        [SerializeField] private Transform parking;
        [SerializeField] private Transform schoolEntrance;
        [SerializeField] private ChunkRenderWindow chunks;
        [SerializeField, Min(0.05f)] private float exitSpeed = 0.5f;
        [SerializeField, Min(1f)] private float fallbackDistance = 7f;

        private readonly HashSet<Collider> contacts = new();
        private readonly RaycastHit[] hits = new RaycastHit[48];
        private readonly Collider[] parkingOverlaps = new Collider[24];
        private BoxCollider zone;
        private IntroductionCabinImage cabinImage;
        private HorrorCorridorExplorer walkingExplorer;
        private InputAction exit;
        private GUIStyle promptStyle;
        private float blockedUntil;
        private float parkingBlockedUntil;
        private bool walking;

        private bool CanExit => !walking && cabinImage?.IsOpen != true &&
                                contacts.Count > 0 && InsideZone(vehicleBody.worldCenterOfMass) &&
                                vehicle.Speed <= exitSpeed && vehicle.HasGroundContact;

        private void Awake()
        {
            zone = GetComponent<BoxCollider>();
            if (vehicle == null || vehicleBody == null || cockpitView == null || explorer == null ||
                driverExit == null || fallbackExit == null || parking == null || schoolEntrance == null || !zone.isTrigger)
            {
                Debug.LogError("La llegada requiere un trigger, la camioneta y los anclajes de ambas vistas.", this);
                enabled = false;
                return;
            }

            cabinImage = cockpitView.GetComponent<IntroductionCabinImage>();
            walkingExplorer = explorer.GetComponent<HorrorCorridorExplorer>();
            exit = new InputAction("Salir de la camioneta", InputActionType.Button, "<Keyboard>/e");
        }

        private void OnDisable()
        {
            exit?.Disable();
            contacts.Clear();
        }

        private void OnDestroy() => exit?.Dispose();

        private void OnTriggerEnter(Collider other) => RegisterContact(other);
        private void OnTriggerStay(Collider other) => RegisterContact(other);
        private void OnTriggerExit(Collider other) => contacts.Remove(other);

        private void RegisterContact(Collider other)
        {
            if (!walking && enabled && other.attachedRigidbody == vehicleBody)
                contacts.Add(other);
        }

        private void Update()
        {
            if (Keyboard.current?.lKey.wasPressedThisFrame == true &&
                cabinImage?.IsOpen != true && walkingExplorer?.IsImageMenuOpen != true)
            {
                GoToParking();
                return;
            }

            if (walking) return;
            if (!CanExit)
            {
                if (exit.enabled) exit.Disable();
                blockedUntil = 0f;
                return;
            }

            // An E held while arriving cannot turn into a new exit press.
            if (!exit.enabled && Keyboard.current?.eKey.isPressed != true) exit.Enable();
            if (!exit.WasPressedThisFrame()) return;

            if (TryFindExit(out Vector3 feet))
                BeginWalking(feet);
            else
                blockedUntil = Time.unscaledTime + 4f;
        }

        private void GoToParking()
        {
            if (walking)
            {
                explorer.enabled = false;
                bool clear = TryPlaceCapsule(fallbackExit.position, out Vector3 feet);
                if (clear)
                {
                    explorer.transform.SetPositionAndRotation(feet, FacingEntrance(feet));
                    walkingExplorer?.ResetMovement();
                    parkingBlockedUntil = 0f;
                }
                explorer.enabled = true;
                if (!clear) parkingBlockedUntil = Time.unscaledTime + 4f;
                return;
            }

            // Check the parking space at the dimensions of the truck, above the pavement.
            Vector3 center = parking.TransformPoint(0f, 1.02f, -0.05f);
            int count = Physics.OverlapBoxNonAlloc(center, new Vector3(1.08f, 0.88f, 2.63f), parkingOverlaps,
                parking.rotation, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (parkingOverlaps[i].attachedRigidbody == vehicleBody) continue;
                parkingBlockedUntil = Time.unscaledTime + 4f;
                return;
            }
            if (count == parkingOverlaps.Length)
            {
                parkingBlockedUntil = Time.unscaledTime + 4f;
                return;
            }

            contacts.Clear();
            exit.Disable();
            blockedUntil = 0f;
            parkingBlockedUntil = 0f;
            vehicleBody.position = parking.position;
            vehicleBody.rotation = parking.rotation;
            vehicleBody.linearVelocity = Vector3.zero;
            vehicleBody.angularVelocity = Vector3.zero;
        }

        private bool TryFindExit(out Vector3 feet)
        {
            Vector3 door = driverExit.position;
            if (TryPlaceCapsule(door, out feet)) return true;

            Vector3 alternative = fallbackExit.position;
            Vector3 delta = Vector3.ProjectOnPlane(alternative - vehicleBody.position, Vector3.up);
            if (delta.sqrMagnitude > fallbackDistance * fallbackDistance ||
                !TryPlaceCapsule(alternative, out feet) ||
                !InsideZone(feet))
                return false;

            Vector3 from = new Vector3(vehicleBody.position.x, feet.y, vehicleBody.position.z);
            if (IsClearWalk(from, feet)) return true;

            // The door may be blocked from one side. Check access around both ends of the truck.
            Vector3 along = Vector3.ProjectOnPlane(vehicleBody.transform.forward, Vector3.up).normalized * 3f;
            return CanWalkFrom(from + along, feet) || CanWalkFrom(from - along, feet);
        }

        private bool CanWalkFrom(Vector3 start, Vector3 destination) =>
            TryPlaceCapsule(start, out Vector3 ground) && InsideZone(ground) && IsClearWalk(ground, destination);

        private bool TryPlaceCapsule(Vector3 target, out Vector3 feet)
        {
            var origin = new Vector3(target.x, parking.position.y + 2f, target.z);
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, hits, 3f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            float minSlope = Mathf.Cos(explorer.slopeLimit * Mathf.Deg2Rad);
            feet = default;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.attachedRigidbody == vehicleBody || hit.distance >= nearest ||
                    hit.normal.y < minSlope || Mathf.Abs(hit.point.y - parking.position.y) > 0.42f)
                    continue;

                feet = hit.point + Vector3.up * (explorer.skinWidth + 0.025f);
                nearest = hit.distance;
            }

            if (float.IsPositiveInfinity(nearest)) return false;
            CapsuleAt(feet, out Vector3 lower, out Vector3 upper);
            return !Physics.CheckCapsule(lower, upper, explorer.radius,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        private bool IsClearWalk(Vector3 start, Vector3 destination)
        {
            Vector3 delta = destination - start;
            if (delta.sqrMagnitude < 0.0001f) return true;
            CapsuleAt(start, out Vector3 lower, out Vector3 upper);
            int count = Physics.CapsuleCastNonAlloc(lower, upper, explorer.radius, delta.normalized, hits, delta.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == hits.Length) return false;

            for (int i = 0; i < count; i++)
                if (hits[i].collider.attachedRigidbody != vehicleBody) return false;
            return true;
        }

        private void CapsuleAt(Vector3 feet, out Vector3 lower, out Vector3 upper)
        {
            Vector3 center = feet + explorer.center;
            float halfCylinder = Mathf.Max(0f, explorer.height * 0.5f - explorer.radius);
            lower = center - Vector3.up * halfCylinder;
            upper = center + Vector3.up * halfCylinder;
        }

        private bool InsideZone(Vector3 world)
        {
            Vector3 local = zone.transform.InverseTransformPoint(world) - zone.center;
            Vector3 half = zone.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        private void BeginWalking(Vector3 feet)
        {
            walking = true;
            exit.Disable();
            vehicle.Park();
            cockpitView.SetActive(false);
            explorer.transform.SetPositionAndRotation(feet, FacingEntrance(feet));
            explorer.gameObject.SetActive(true);
            if (chunks != null)
            {
                chunks.Follow(explorer.transform);
                chunks.RegisterStaticScenery(vehicle.transform);
            }
        }

        private Quaternion FacingEntrance(Vector3 feet)
        {
            Vector3 direction = Vector3.ProjectOnPlane(schoolEntrance.position - feet, Vector3.up);
            return direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction) : fallbackExit.rotation;
        }

        private void OnGUI()
        {
            if (exit == null || cabinImage?.IsOpen == true || walkingExplorer?.IsImageMenuOpen == true) return;
            bool parkingBlocked = parkingBlockedUntil > Time.unscaledTime;
            if (walking && !parkingBlocked) return;
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                promptStyle.normal.textColor = Color.white;
            }

            if (!walking) DrawLabel($"{Mathf.RoundToInt(vehicle.Speed * 3.6f):0} km/h", 18f, 116f, false);
            if (parkingBlocked)
            {
                DrawLabel("El punto de llegada al estacionamiento está ocupado.", 18f, 375f, true);
                return;
            }
            if (!CanExit) return;
            string message = blockedUntil > Time.unscaledTime
                ? "No hay espacio para bajar. Vuelve a maniobrar."
                : "E · Bajar del vehículo";
            DrawLabel(message, 18f, 375f, true);
        }

        private void DrawLabel(string text, float margin, float maxWidth, bool centered)
        {
            float scale = Mathf.Clamp(Screen.height / 720f, 0.8f, 1.5f);
            float width = Mathf.Min(maxWidth * scale, Screen.width - margin * 2f);
            float height = 44f * scale;
            Rect area = new Rect(centered ? (Screen.width - width) * 0.5f : margin,
                Screen.height - height - margin, width, height);
            Color previous = GUI.color;
            GUI.color = new Color(0.035f, 0.055f, 0.075f, 0.88f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            Color previousContent = GUI.contentColor;
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            promptStyle.fontSize = Mathf.RoundToInt(17f * scale);
            GUI.Label(area, text, promptStyle);
            GUI.contentColor = previousContent;
            GUI.color = previous;
        }
    }
}
