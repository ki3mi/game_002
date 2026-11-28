using System.Collections.Generic;
using EcoElectry.Unity.Player;
using UnityEngine;

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
        [SerializeField] private IntroductionSkyCycle skyCycle;
        [SerializeField] private IntroductionCinematics cinematics;
        [SerializeField] private Transform highwayArrivalStart;
        [SerializeField] private Vector3[] highwayArrivalRoute;
        [SerializeField] private int highwayGateSample;
        [SerializeField, Min(0.05f)] private float exitSpeed = 0.5f;
        [SerializeField, Min(1f)] private float fallbackDistance = 7f;
        [SerializeField, Min(1f)] private float parkingAdjustment = 5f;

        private static readonly Vector3 VehicleClearance = new(1.08f, 0.88f, 2.63f);
        private static readonly Vector3 ClearanceCenter = new(0f, 1.02f, -0.05f);

        private readonly HashSet<Collider> contacts = new();
        private readonly RaycastHit[] hits = new RaycastHit[48];
        private readonly Collider[] parkingOverlaps = new Collider[24];
        private BoxCollider zone;
        private IntroductionCabinImage cabinImage;
        private HorrorCorridorExplorer walkingExplorer;
        private GUIStyle promptStyle;
        private float blockedUntil;
        private float parkingBlockedUntil;
        private bool walking;
        private Vector3 parkingStart;
        private Quaternion parkingStartRotation;
        private Vector3 previousRoadPosition;
        private Vector3[] approachPath;
        private float[] approachDistances;
        private float approachSpeed;
        internal Transform Walker => explorer.transform;

        private bool IsParked => !walking && contacts.Count > 0 && InsideZone(vehicleBody.worldCenterOfMass) &&
                                 vehicle.Speed <= exitSpeed && vehicle.HasGroundContact;

        private void Awake()
        {
            zone = GetComponent<BoxCollider>();
            if (vehicle == null || vehicleBody == null || cockpitView == null || explorer == null ||
                driverExit == null || fallbackExit == null || parking == null || schoolEntrance == null ||
                cinematics == null || !zone.isTrigger)
            {
                Debug.LogError("La llegada requiere un trigger, la camioneta y los anclajes de ambas vistas.", this);
                enabled = false;
                return;
            }

            cabinImage = cockpitView.GetComponent<IntroductionCabinImage>();
            walkingExplorer = explorer.GetComponent<HorrorCorridorExplorer>();
            previousRoadPosition = vehicleBody.position;
        }

        private void OnDisable() => contacts.Clear();

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
            Vector3 previous = previousRoadPosition;
            previousRoadPosition = vehicleBody.position;
            if (Time.timeScale <= 0f || (!walking && !cinematics.IsDriving)) return;

            if (walking || cabinImage?.IsOpen == true) return;
            if (PassedFourthLight(previous) && PrepareHighwayArrival())
            {
                cinematics.StartArrival(true);
                return;
            }
            if (!IsParked) return;
            parkingStart = vehicleBody.position;
            parkingStartRotation = vehicleBody.rotation;
            if (Vector3.Distance(parkingStart, parking.position) > parkingAdjustment ||
                Quaternion.Angle(parkingStartRotation, parking.rotation) > 65f ||
                !IsManoeuvreClear() || !TryFindExit(out _))
            {
                blockedUntil = Time.unscaledTime + 2f;
                return;
            }

            skyCycle?.BeginNight();
            cinematics.StartArrival(false);
        }

        private bool PassedFourthLight(Vector3 previous)
        {
            if (highwayArrivalStart == null || highwayArrivalRoute == null || highwayArrivalRoute.Length < 3 ||
                highwayGateSample < 1 || highwayGateSample >= highwayArrivalRoute.Length - 1)
                return false;

            Vector3 last = highwayArrivalStart.InverseTransformPoint(previous);
            Vector3 current = highwayArrivalStart.InverseTransformPoint(vehicleBody.position);
            if (last.z >= 0f || current.z < 0f || Vector3.Dot(vehicleBody.transform.forward, highwayArrivalStart.forward) < 0.6f)
                return false;
            Vector3 crossing = Vector3.Lerp(last, current, Mathf.InverseLerp(last.z, current.z, 0f));
            return Mathf.Abs(crossing.x) < 4.1f && Mathf.Abs(crossing.y) < 1.5f;
        }

        private bool PrepareHighwayArrival()
        {
            if (!TryFindExit(out _))
            {
                parkingBlockedUntil = Time.unscaledTime + 4f;
                return false;
            }

            parkingStartRotation = vehicleBody.rotation;
            approachPath = new Vector3[highwayArrivalRoute.Length];
            approachDistances = new float[approachPath.Length];
            Vector3 first = highwayArrivalRoute[0];
            Vector3 offset = vehicleBody.position - first;
            for (int i = 0; i < approachPath.Length; i++)
            {
                Vector3 point = highwayArrivalRoute[i];
                float merge = 1f - Mathf.SmoothStep(0f, 1f, Vector3.Distance(first, point) / 20f);
                approachPath[i] = point + offset * merge;
                if (i > 0)
                    approachDistances[i] = approachDistances[i - 1] + Vector3.Distance(approachPath[i - 1], approachPath[i]);
            }

            approachSpeed = vehicle.Speed;
            for (int i = 0; i < approachDistances.Length; i++)
            {
                SampleApproach(approachDistances[i], out Vector3 position, out Quaternion rotation);
                if (VehiclePoseClear(position, rotation)) continue;
                parkingBlockedUntil = Time.unscaledTime + 4f;
                return false;
            }
            return true;
        }

        internal bool AdvanceHighwayArrival(float time, float gateTime, float parkedTime)
        {
            const float gateSpeed = 6f;
            float gateDistance = approachDistances[highwayGateSample];
            float total = approachDistances[^1];
            float distance, speed;
            if (time < gateTime)
            {
                float startSpeed = Mathf.Min(approachSpeed, 2.3f * gateDistance / gateTime);
                distance = RoadTravel(time, gateTime, gateDistance, startSpeed, gateSpeed, out speed);
            }
            else
                distance = gateDistance + RoadTravel(time - gateTime, parkedTime - gateTime,
                    total - gateDistance, gateSpeed, 0f, out speed);

            SampleApproach(distance, out Vector3 position, out Quaternion rotation);
            if (!VehiclePoseClear(position, rotation)) return false;
            vehicle.SetCinematicPose(position, rotation, speed);
            return true;
        }

        private void SampleApproach(float distance, out Vector3 position, out Quaternion rotation)
        {
            position = PointAlongApproach(distance);
            Vector3 ahead = PointAlongApproach(distance + 1.3f);
            Vector3 behind = PointAlongApproach(distance - 1.3f);
            Quaternion alongRoad = Quaternion.LookRotation(Vector3.ProjectOnPlane(ahead - behind, Vector3.up));
            rotation = Quaternion.Slerp(parkingStartRotation, alongRoad, Mathf.Clamp01(distance / 12f));
            rotation = Quaternion.Slerp(rotation, parking.rotation,
                1f - Mathf.Clamp01((approachDistances[^1] - distance) / 2f));
        }

        private Vector3 PointAlongApproach(float distance)
        {
            int low = 0;
            int high = approachDistances.Length - 1;
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (approachDistances[mid] < distance) low = mid;
                else high = mid;
            }
            return Vector3.Lerp(approachPath[low], approachPath[high],
                Mathf.InverseLerp(approachDistances[low], approachDistances[high], distance));
        }

        private static float RoadTravel(float time, float duration, float length, float fromSpeed, float toSpeed,
            out float speed)
        {
            float t = Mathf.Clamp01(time / duration);
            float t2 = t * t;
            float t3 = t2 * t;
            float lengthRate = length / duration;
            speed = (6f * t - 6f * t2) * lengthRate +
                    (3f * t2 - 4f * t + 1f) * fromSpeed + (3f * t2 - 2f * t) * toSpeed;
            return length * (3f * t2 - 2f * t3) +
                   duration * ((t3 - 2f * t2 + t) * fromSpeed + (t3 - t2) * toSpeed);
        }

        internal bool GoToParking()
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
                return clear;
            }

            if (!VehiclePoseClear(parking.position, parking.rotation))
            {
                parkingBlockedUntil = Time.unscaledTime + 4f;
                return false;
            }

            contacts.Clear();
            blockedUntil = 0f;
            parkingBlockedUntil = 0f;
            vehicleBody.position = parking.position;
            previousRoadPosition = parking.position;
            vehicleBody.rotation = parking.rotation;
            vehicleBody.linearVelocity = Vector3.zero;
            vehicleBody.angularVelocity = Vector3.zero;
            return true;
        }

        internal bool MoveWalker(Vector3 position, Vector3 facing)
        {
            if (!walking) return false;
            explorer.enabled = false;
            bool clear = TryPlaceCapsule(position, out Vector3 feet, groundReference: position.y);
            if (clear)
            {
                explorer.transform.SetPositionAndRotation(feet, Quaternion.LookRotation(facing));
                walkingExplorer?.ResetMovement();
            }
            explorer.enabled = true;
            return clear;
        }

        private bool IsManoeuvreClear()
        {
            float travel = Vector3.Distance(parkingStart, parking.position);
            float turn = Quaternion.Angle(parkingStartRotation, parking.rotation);
            int samples = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(travel / 0.16f, turn / 3f)));
            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                if (!VehiclePoseClear(Vector3.Lerp(parkingStart, parking.position, t),
                    Quaternion.Slerp(parkingStartRotation, parking.rotation, t))) return false;
            }
            return true;
        }

        private bool VehiclePoseClear(Vector3 position, Quaternion rotation)
        {
            int count = Physics.OverlapBoxNonAlloc(position + rotation * ClearanceCenter, VehicleClearance,
                parkingOverlaps, rotation, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == parkingOverlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (parkingOverlaps[i].attachedRigidbody != vehicleBody) return false;
            return true;
        }

        internal bool MoveIntoParking(float progress, float duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, progress);
            Vector3 position = Vector3.Lerp(parkingStart, parking.position, t);
            Quaternion rotation = Quaternion.Slerp(parkingStartRotation, parking.rotation, t);
            if (!VehiclePoseClear(position, rotation)) return false;
            float speed = Vector3.Distance(parkingStart, parking.position) * (6f * progress * (1f - progress)) / duration;
            vehicle.SetCinematicPose(position, rotation, speed);
            return true;
        }

        private bool TryFindExit(out Vector3 feet)
        {
            Vector3 door = parking.TransformPoint(driverExit.localPosition);
            if (TryPlaceCapsule(door, out feet, true) && OutsideParkingFootprint(feet)) return true;

            Vector3 alternative = fallbackExit.position;
            Vector3 delta = Vector3.ProjectOnPlane(alternative - parking.position, Vector3.up);
            if (delta.sqrMagnitude > fallbackDistance * fallbackDistance ||
                !TryPlaceCapsule(alternative, out feet, true) || !OutsideParkingFootprint(feet) ||
                !InsideZone(feet))
                return false;

            Vector3 from = new Vector3(parking.position.x, feet.y, parking.position.z);
            if (IsClearWalk(from, feet)) return true;

            // The door may be blocked from one side. Check access around both ends of the truck.
            Vector3 along = Vector3.ProjectOnPlane(parking.forward, Vector3.up).normalized * 3f;
            return CanWalkFrom(from + along, feet) || CanWalkFrom(from - along, feet);
        }

        private bool CanWalkFrom(Vector3 start, Vector3 destination) =>
            TryPlaceCapsule(start, out Vector3 ground, true) && InsideZone(ground) && IsClearWalk(ground, destination);

        private bool OutsideParkingFootprint(Vector3 feet)
        {
            Vector3 local = parking.InverseTransformPoint(feet) - ClearanceCenter;
            return Mathf.Abs(local.x) >= VehicleClearance.x + explorer.radius ||
                   Mathf.Abs(local.z) >= VehicleClearance.z + explorer.radius;
        }

        private bool TryPlaceCapsule(Vector3 target, out Vector3 feet, bool ignoreTruck = false, float? groundReference = null)
        {
            float groundHeight = groundReference ?? parking.position.y;
            var origin = new Vector3(target.x, groundHeight + 2f, target.z);
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, hits, 3f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            float minSlope = Mathf.Cos(explorer.slopeLimit * Mathf.Deg2Rad);
            feet = default;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.attachedRigidbody == vehicleBody || hit.distance >= nearest ||
                    hit.normal.y < minSlope || Mathf.Abs(hit.point.y - groundHeight) > 0.42f)
                    continue;

                feet = hit.point + Vector3.up * (explorer.skinWidth + 0.025f);
                nearest = hit.distance;
            }

            if (float.IsPositiveInfinity(nearest)) return false;
            CapsuleAt(feet, out Vector3 lower, out Vector3 upper);
            int overlaps = Physics.OverlapCapsuleNonAlloc(lower, upper, explorer.radius, parkingOverlaps,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (overlaps == parkingOverlaps.Length) return false;
            for (int i = 0; i < overlaps; i++)
                if (!ignoreTruck || parkingOverlaps[i].attachedRigidbody != vehicleBody) return false;
            return true;
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

        internal bool TryGetExitView(out Vector3 eyes, out Quaternion rotation)
        {
            bool valid = TryFindExit(out Vector3 feet);
            eyes = feet + Vector3.up * 1.65f;
            rotation = FacingEntrance(feet);
            return valid;
        }

        internal bool FinishArrival()
        {
            if (!TryFindExit(out Vector3 feet))
            {
                blockedUntil = Time.unscaledTime + 3f;
                return false;
            }
            walking = true;
            vehicle.Park();
            cockpitView.SetActive(false);
            explorer.transform.SetPositionAndRotation(feet, FacingEntrance(feet));
            explorer.gameObject.SetActive(true);
            if (chunks != null)
            {
                chunks.Follow(explorer.transform);
                chunks.RegisterStaticScenery(vehicle.transform);
            }
            return true;
        }

        private Quaternion FacingEntrance(Vector3 feet)
        {
            Vector3 direction = Vector3.ProjectOnPlane(schoolEntrance.position - feet, Vector3.up);
            return direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction) : fallbackExit.rotation;
        }

        private void OnGUI()
        {
            if ((!walking && !cinematics.IsDriving) || cabinImage?.IsOpen == true ||
                walkingExplorer?.IsImageMenuOpen == true) return;
            bool parkingBlocked = parkingBlockedUntil > Time.unscaledTime;
            bool exitBlocked = !walking && IsParked && blockedUntil > Time.unscaledTime;
            if (!parkingBlocked && !exitBlocked) return;
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                promptStyle.normal.textColor = Color.white;
            }

            if (parkingBlocked)
            {
                DrawLabel("El punto de llegada al estacionamiento está ocupado.", 18f, 375f);
                return;
            }
            DrawLabel("Acércate a la plaza y deja espacio junto a la puerta.", 18f, 455f);
        }

        private void DrawLabel(string text, float margin, float maxWidth)
        {
            float scale = Mathf.Clamp(Screen.height / 720f, 0.8f, 1.5f);
            float width = Mathf.Min(maxWidth * scale, Screen.width - margin * 2f);
            float height = 44f * scale;
            Rect area = new Rect((Screen.width - width) * 0.5f,
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
