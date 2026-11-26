using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody))]
    public sealed class IntroductionTruckDrive : MonoBehaviour
    {
        [Serializable]
        private struct Wheel
        {
            public WheelCollider collider;
            public Transform visual;
            public bool steers;
        }

        [SerializeField] private InputActionAsset controls;
        [SerializeField] private Wheel[] wheels;
        [SerializeField] private Transform steeringWheel;
        [SerializeField] private Light[] lowBeams;
        [SerializeField] private Renderer[] headlightLenses;
        [SerializeField] private Material unlitLens;
        [SerializeField, Min(1f)] private float forwardSpeedKmh = 120f;
        [SerializeField, Min(0.5f)] private float reverseSpeed = 4.4f;
        [SerializeField, Min(0f)] private float wheelTorque = 440f;
        [SerializeField, Min(0f)] private float brakeTorque = 1450f;
        [SerializeField, Min(0f)] private float antiRollForce = 4800f;

        private Rigidbody body;
        private InputAction drive;
        private Quaternion steeringWheelRest;
        private float steerAngle;
        private bool parked;
        internal bool ImageMenuOpen { get; set; }

        public float Speed => body != null ? body.linearVelocity.magnitude : 0f;
        public bool HasGroundContact { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            drive = controls?.FindAction("Player/Move")?.Clone();
            if (drive == null || wheels == null || wheels.Length != 4 || Array.Exists(wheels, w => w.collider == null))
            {
                Debug.LogError("La camioneta requiere la acción Player/Move y cuatro ruedas físicas.", this);
                enabled = false;
                return;
            }

            body.centerOfMass = new Vector3(0f, 0.53f, 0.02f);
            wheels[0].collider.ConfigureVehicleSubsteps(6f, 8, 15);
            if (steeringWheel != null) steeringWheelRest = steeringWheel.localRotation;
        }

        private void OnEnable() => drive?.Enable();
        private void OnDisable()
        {
            drive?.Disable();
            if (wheels == null) return;
            foreach (Wheel wheel in wheels)
                if (wheel.collider != null) wheel.collider.motorTorque = 0f;
        }
        private void OnDestroy() => drive?.Dispose();

        private void FixedUpdate()
        {
            if (parked) return;

            Vector2 input = ImageMenuOpen ? Vector2.zero : drive.ReadValue<Vector2>();
            float forwardSpeed = forwardSpeedKmh / 3.6f;
            float longitudinalSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
            if (HasGroundContact && longitudinalSpeed > forwardSpeed)
            {
                body.linearVelocity -= transform.forward * (longitudinalSpeed - forwardSpeed);
                longitudinalSpeed = forwardSpeed;
            }
            float speed = Mathf.Abs(longitudinalSpeed);
            float steerLimit = Mathf.Lerp(33f, 8.5f, Mathf.InverseLerp(2f, forwardSpeed, speed));
            float steerSpeed = Mathf.Abs(input.x) > 0.02f ? 64f : 100f;
            steerAngle = Mathf.MoveTowards(steerAngle, input.x * steerLimit, steerSpeed * Time.fixedDeltaTime);

            bool throttle = Mathf.Abs(input.y) > 0.05f;
            bool braking = throttle && speed > 0.36f && input.y * longitudinalSpeed < 0f;
            float limit = input.y < 0f ? reverseSpeed : forwardSpeed;
            float torque = throttle && !braking
                ? input.y * wheelTorque * Mathf.Clamp01((limit - speed) / (input.y < 0f ? 2.1f : 0.35f))
                : 0f;
            float brake = 0f;
            if (ImageMenuOpen) brake = brakeTorque;
            else if (braking) brake = brakeTorque * Mathf.Abs(input.y);
            else if (!throttle) brake = speed < 0.38f ? 420f : 110f;
            float overspeed = speed - (longitudinalSpeed < 0f ? reverseSpeed : forwardSpeed);
            if (overspeed > 0.12f) brake = Mathf.Max(brake, Mathf.Min(brakeTorque, overspeed * 240f));

            int grounded = 0;
            foreach (Wheel wheel in wheels)
            {
                WheelCollider collider = wheel.collider;
                collider.steerAngle = wheel.steers ? steerAngle : 0f;
                collider.motorTorque = torque;
                collider.brakeTorque = brake;
                if (collider.isGrounded) grounded++;
            }
            HasGroundContact = grounded >= 2;
            ApplyAntiRoll(wheels[0].collider, wheels[1].collider);
            ApplyAntiRoll(wheels[2].collider, wheels[3].collider);
        }

        private void ApplyAntiRoll(WheelCollider left, WheelCollider right)
        {
            bool leftGrounded = left.GetGroundHit(out WheelHit leftHit);
            bool rightGrounded = right.GetGroundHit(out WheelHit rightHit);
            if (!leftGrounded || !rightGrounded) return;

            float leftTravel = SuspensionTravel(left, leftHit);
            float rightTravel = SuspensionTravel(right, rightHit);
            float force = (leftTravel - rightTravel) * antiRollForce;
            body.AddForceAtPosition(-left.transform.up * force, left.transform.position);
            body.AddForceAtPosition(right.transform.up * force, right.transform.position);
        }

        private static float SuspensionTravel(WheelCollider wheel, WheelHit hit)
        {
            float contact = wheel.transform.InverseTransformPoint(hit.point).y;
            return Mathf.Clamp01((wheel.center.y - contact - wheel.radius) / wheel.suspensionDistance);
        }

        private void LateUpdate()
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.visual == null) continue;
                wheel.collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
                wheel.visual.SetPositionAndRotation(position, rotation);
            }
            if (steeringWheel != null)
                steeringWheel.localRotation = steeringWheelRest * Quaternion.Euler(0f, 0f, -3f * steerAngle);
        }

        public void Park()
        {
            if (parked) return;
            parked = true;
            foreach (Wheel wheel in wheels)
            {
                wheel.collider.motorTorque = 0f;
                wheel.collider.brakeTorque = brakeTorque * 2f;
            }
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            foreach (Light headlight in lowBeams) headlight.enabled = false;
            foreach (Renderer lens in headlightLenses) lens.sharedMaterial = unlitLens;
            enabled = false;
        }
    }
}
