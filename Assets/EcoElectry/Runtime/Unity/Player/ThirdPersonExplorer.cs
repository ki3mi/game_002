using UnityEngine;
using UnityEngine.InputSystem;
using PlayerApplication = UnityEngine.Application;

namespace EcoElectry.Unity.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonExplorer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private Transform spawn;
        [SerializeField] private Transform avatar;
        [SerializeField] private Camera view;
        [SerializeField] private string cameraToggleBinding = "<Keyboard>/c";
        [SerializeField, Min(0.1f)] private float walkSpeed = 4f;
        [SerializeField, Min(0.5f)] private float followDistance = 3.35f;
        [SerializeField, Min(0.05f)] private float cameraRadius = 0.19f;

        private readonly RaycastHit[] cameraHits = new RaycastHit[16];
        private CharacterController body;
        private InputAction moveAction;
        private InputAction lookAction;
        private Renderer[] avatarRenderers;
        private float yaw;
        private float pitch;
        private float verticalSpeed;
        private float cameraDistance;
        private bool avatarHidden;
        private bool firstPerson;

        public Camera View => view;
        public InputAction CameraToggleAction { get; private set; }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            if (controls == null || spawn == null || avatar == null || view == null ||
                controls.FindAction("Player/Move") == null || controls.FindAction("Player/Look") == null)
            {
                Debug.LogError("La exploración requiere cámara, modelo, punto de aparición y acciones Move y Look.", this);
                enabled = false;
                return;
            }

            avatarRenderers = avatar.GetComponentsInChildren<Renderer>();
            moveAction = controls.FindAction("Player/Move").Clone();
            lookAction = controls.FindAction("Player/Look").Clone();
            CameraToggleAction = new InputAction("ToggleCamera", InputActionType.Button, cameraToggleBinding);
            Respawn();
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            CameraToggleAction?.Enable();
            GrabCursor();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            CameraToggleAction?.Disable();
            if (PlayerApplication.isPlaying) ReleaseCursor();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            lookAction?.Dispose();
            CameraToggleAction?.Dispose();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && PlayerApplication.isPlaying) ReleaseCursor();
        }

        private void Update()
        {
            if (CameraToggleAction.WasPressedThisFrame())
            {
                firstPerson = !firstPerson;
                cameraDistance = followDistance;
            }

            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true) ReleaseCursor();
            if (Cursor.lockState != CursorLockMode.Locked && Mouse.current?.leftButton.wasPressedThisFrame == true)
                GrabCursor();

            if (lookAction.activeControl?.device is Gamepad)
            {
                ApplyLook(lookAction.ReadValue<Vector2>() * (120f * Time.deltaTime));
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                ApplyLook(lookAction.ReadValue<Vector2>() * 0.14f);
            }

            Move(moveAction.ReadValue<Vector2>(), Time.deltaTime);
            if (transform.position.y < -9f) Respawn();
        }

        private void ApplyLook(Vector2 degrees)
        {
            yaw = Mathf.Repeat(yaw + degrees.x, 360f);
            pitch = Mathf.Clamp(pitch - degrees.y, -18f, 58f);
        }

        public void Move(Vector2 motion, float deltaTime)
        {
            Vector2 clamped = Vector2.ClampMagnitude(motion, 1f);
            Vector3 planar = Quaternion.Euler(0f, yaw, 0f) * new Vector3(clamped.x, 0f, clamped.y);
            if (planar.sqrMagnitude > 0.001f)
                avatar.rotation = Quaternion.RotateTowards(avatar.rotation, Quaternion.LookRotation(planar), 620f * deltaTime);

            verticalSpeed = body.isGrounded && verticalSpeed < 0f ? -3f : verticalSpeed - 22f * deltaTime;
            body.Move((planar * walkSpeed + Vector3.up * verticalSpeed) * deltaTime);
        }

        private void LateUpdate() => UpdateView(Time.deltaTime);

        private void UpdateView(float deltaTime)
        {
            float eyeHeight = firstPerson ? body.center.y + body.height * 0.5f - 0.14f : 1.42f;
            Vector3 pivot = transform.position + Vector3.up * eyeHeight;
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 backwards = orbit * Vector3.back;
            if (!firstPerson) UpdateFollowDistance(pivot, backwards, deltaTime);
            view.transform.SetPositionAndRotation(firstPerson ? pivot : pivot + backwards * cameraDistance, orbit);

            bool tooClose = firstPerson || cameraDistance < 0.82f;
            if (tooClose != avatarHidden)
            {
                avatarHidden = tooClose;
                foreach (Renderer renderer in avatarRenderers) renderer.forceRenderingOff = avatarHidden;
            }
        }

        private void UpdateFollowDistance(Vector3 pivot, Vector3 backwards, float deltaTime)
        {
            float allowedDistance = followDistance;
            int hits = Physics.SphereCastNonAlloc(pivot, cameraRadius, backwards, cameraHits, followDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            for (int index = 0; index < hits; index++)
            {
                RaycastHit hit = cameraHits[index];
                if (hit.transform.IsChildOf(transform)) continue;
                allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0.12f, hit.distance - 0.055f));
            }

            cameraDistance = allowedDistance < cameraDistance
                ? allowedDistance
                : Mathf.MoveTowards(cameraDistance, allowedDistance, 9f * deltaTime);
        }

        private void Respawn()
        {
            body.enabled = false;
            transform.position = spawn.position;
            body.enabled = true;
            avatar.rotation = spawn.rotation;
            yaw = spawn.eulerAngles.y;
            pitch = 9f;
            verticalSpeed = 0f;
            cameraDistance = followDistance;
            UpdateView(0f);
        }

        private static void GrabCursor()
        {
            if (!PlayerApplication.isFocused) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
