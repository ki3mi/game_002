using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterController))]
    public sealed class HorrorCorridorExplorer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private Camera view;
        [SerializeField] private Light flashlight;
        [SerializeField] private string flashlightBinding = "<Keyboard>/f";
        [SerializeField] private bool flashlightStartsOn = true;
        [SerializeField, Range(0f, 1f)] private float flashlightFlickerDepth = 0.88f;
        [SerializeField] private Vector2 flashlightFlickerInterval = new Vector2(4f, 9f);
        [SerializeField] private Key brightnessMenuKey = Key.B;
        [SerializeField] private string brightnessPreferenceKey = "";
        [SerializeField] private string grainPreferenceKey = "";
        [SerializeField, Min(0.1f)] private float walkSpeed = 1.7f;
        [SerializeField, Min(0.01f)] private float accelerationTime = 0.16f;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Min(0.1f)] private float eyeHeight = 1.65f;
        [SerializeField, Range(0f, 1f)] private float bobIntensity = 1f;
        [SerializeField, Min(0.1f)] private float stepLength = 0.72f;
        [SerializeField, Range(0f, 0.04f)] private float verticalBob = 0.015f;
        [SerializeField, Range(0f, 1f)] private float rollDegrees = 0.28f;

        private CharacterController body;
        private InputAction move;
        private InputAction look;
        private InputAction toggleLight;
        private CorridorBrightness brightness;
        private CorridorFlicker flashlightFlicker;
        private float flashlightIntensity;
        private Vector3 velocity;
        private float fallSpeed;
        private float pitch;
        private float phase;
        private float bobWeight;
        internal bool IsImageMenuOpen => brightness?.IsOpen == true;

        internal void ResetMovement()
        {
            velocity = Vector3.zero;
            fallSpeed = 0f;
            pitch = 0f;
            phase = 0f;
            bobWeight = 0f;
            ApplyView();
        }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            move = controls?.FindAction("Player/Move")?.Clone();
            look = controls?.FindAction("Player/Look")?.Clone();
            if (move == null || look == null || view == null || flashlight == null)
            {
                Debug.LogError("El prototipo de pasillos requiere cámara, linterna y las acciones Player/Move y Player/Look.", this);
                enabled = false;
                return;
            }

            toggleLight = new InputAction("Flashlight", InputActionType.Button, flashlightBinding);
            brightness = new CorridorBrightness(view, flashlight, brightnessPreferenceKey, grainPreferenceKey);
            flashlightIntensity = flashlight.intensity;
            flashlightFlicker = new CorridorFlicker(flashlightFlickerInterval, Time.time);
            flashlight.enabled = flashlightStartsOn;
            ApplyView();
        }

        private void OnEnable()
        {
            move?.Enable();
            look?.Enable();
            toggleLight?.Enable();
            flashlightFlicker?.Restart(Time.time);
            if (UnityEngine.Application.isFocused) SetCursorLocked(true);
        }

        private void OnDisable()
        {
            move?.Disable();
            look?.Disable();
            toggleLight?.Disable();
            brightness?.Close();
            if (flashlightFlicker != null && flashlight != null) flashlight.intensity = flashlightIntensity;
            velocity = Vector3.zero;
            fallSpeed = 0f;
            bobWeight = 0f;
            SetCursorLocked(false);
        }

        private void OnDestroy()
        {
            move?.Dispose();
            look?.Dispose();
            toggleLight?.Dispose();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetCursorLocked(false);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            bool escapePressed = keyboard?.escapeKey.wasPressedThisFrame == true;
            bool brightnessPressed = brightness != null && brightnessMenuKey != Key.None &&
                keyboard?[brightnessMenuKey].wasPressedThisFrame == true;

            if (brightness?.IsOpen == true)
            {
                if (brightnessPressed || escapePressed)
                {
                    brightness.Close();
                    SetCursorLocked(true);
                }
                return;
            }

            if (brightnessPressed)
            {
                brightness.Open();
                velocity = Vector3.zero;
                bobWeight = 0f;
                SetCursorLocked(false);
                return;
            }

            if (escapePressed) SetCursorLocked(false);
            else if (Mouse.current?.leftButton.wasPressedThisFrame == true) SetCursorLocked(true);
            if (Time.deltaTime <= 0f) return;

            if (toggleLight.WasPressedThisFrame())
            {
                flashlight.enabled = !flashlight.enabled;
                flashlight.intensity = flashlightIntensity;
                flashlightFlicker.Restart(Time.time);
            }

            Vector2 delta = look.ReadValue<Vector2>();
            if (look.activeControl?.device is Gamepad) delta *= 110f * Time.deltaTime;
            else delta *= Cursor.lockState == CursorLockMode.Locked ? mouseSensitivity : 0f;
            transform.Rotate(0f, delta.x, 0f);
            pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
            Walk(move.ReadValue<Vector2>(), Time.deltaTime);
        }

        private void Walk(Vector2 input, float dt)
        {
            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 target = transform.TransformDirection(new Vector3(input.x, 0f, input.y)) * walkSpeed;
            Vector3 previousVelocity = velocity;
            velocity = Vector3.MoveTowards(velocity, target, walkSpeed * dt / accelerationTime);
            fallSpeed = body.isGrounded ? -2f : fallSpeed + Physics.gravity.y * dt;
            Vector3 before = transform.position;
            CollisionFlags contacts = body.Move(((previousVelocity + velocity) * 0.5f + Vector3.up * fallSpeed) * dt);

            Vector3 actual = transform.position - before;
            actual.y = 0f;
            float distance = (contacts & CollisionFlags.Below) != 0 ? actual.magnitude : 0f;
            phase = Mathf.Repeat(phase + distance * Mathf.PI / stepLength, Mathf.PI * 2f);
            float speedWeight = Mathf.Clamp01(distance / (dt * walkSpeed));
            bobWeight = Mathf.Lerp(bobWeight, speedWeight, 1f - Mathf.Exp(-14f * dt));
        }

        private void LateUpdate()
        {
            ApplyView();
            if (flashlight.enabled)
                flashlight.intensity = flashlightIntensity * (1f - flashlightFlickerDepth * flashlightFlicker.Sample(Time.time));
        }

        private void OnGUI()
        {
            if (brightness?.IsOpen == true) brightness.Draw(brightnessMenuKey.ToString());
        }

        private void ApplyView()
        {
            float weight = bobWeight * bobIntensity;
            float side = Mathf.Sin(phase) * weight;
            Vector3 offset = new Vector3(0.006f * side, Mathf.Sin(2f * phase) * verticalBob * weight, 0f);

            // Keep the entire near plane inside the collision capsule, even facing a jamb side-on.
            float halfSegment = Mathf.Max(0f, body.height * 0.5f - body.radius);
            float capDistance = Mathf.Max(0f, Mathf.Abs(eyeHeight - body.center.y) - halfSegment);
            float halfPlane = Mathf.Tan(view.fieldOfView * Mathf.Deg2Rad * 0.5f);
            float planeRadius = view.nearClipPlane * Mathf.Sqrt(1f + halfPlane * halfPlane * (1f + view.aspect * view.aspect));
            float clearance = Mathf.Max(0f, body.radius - body.skinWidth - capDistance - planeRadius);
            view.transform.localPosition = Vector3.up * eyeHeight + Vector3.ClampMagnitude(offset, clearance);
            view.transform.localRotation = Quaternion.Euler(pitch, 0f, rollDegrees * side);
        }
    }
}
