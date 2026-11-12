using UnityEngine;
using Interruptores;

namespace Interruptores.Pruebas
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FPSPlayer : MonoBehaviour
    {
        [Header("Cámara")]
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(0.1f, 5f)] private float mouseSensitivity = 2.2f;
        [SerializeField, Range(-85f, 85f)] private float startingPitch = 4f;

        [Header("Movimiento")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float sprintSpeed = 5.2f;
        [SerializeField] private float jumpHeight = 0.85f;
        [SerializeField] private float gravity = -18f;

        [Header("Inspección")]
        [SerializeField] private float interactionDistance = 2.4f;
        [SerializeField] private LayerMask inspectionMask = Physics.DefaultRaycastLayers;

        private CharacterController character;
        private float yaw;
        private float pitch;
        private float verticalSpeed;
        private bool cursorLocked = true;
        private InterruptorInteractivo currentSwitch;
        private Collider currentCollider;
        private string currentObjectName;

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            pitch = startingPitch;
            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLocked(false);
                return;
            }

            if (!cursorLocked)
            {
                if (Input.GetMouseButtonDown(0))
                    SetCursorLocked(true);
                return;
            }

            UpdateLook();
            UpdateMovement();
            RefreshLookTarget();

            if (Input.GetKeyDown(KeyCode.E))
                TryInteractAtCrosshair();
        }

        private void UpdateLook()
        {
            if (playerCamera == null)
                return;

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -85f, 85f);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateMovement()
        {
            if (character == null)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 planar = transform.right * horizontal + transform.forward * vertical;
            if (planar.sqrMagnitude > 1f)
                planar.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            if (character.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;

            if (character.isGrounded && Input.GetKeyDown(KeyCode.Space))
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalSpeed += gravity * Time.deltaTime;
            Vector3 movement = planar * speed + Vector3.up * verticalSpeed;
            character.Move(movement * Time.deltaTime);
        }

        public void RefreshLookTarget()
        {
            currentSwitch = null;
            currentCollider = null;
            currentObjectName = null;

            if (playerCamera == null)
                return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, inspectionMask, QueryTriggerInteraction.Ignore))
                return;

            currentCollider = hit.collider;
            currentSwitch = hit.collider.GetComponentInParent<InterruptorInteractivo>();

            Transform root = hit.collider.transform.root;
            currentObjectName = root.name.Replace('_', ' ');
        }

        /// <summary>
        /// Recasts from the center reticle and toggles the prefab switch it hits.
        /// Public so the exact player-to-prefab path can be smoke-checked in Unity.
        /// </summary>
        public bool TryInteractAtCrosshair()
        {
            RefreshLookTarget();
            if (currentSwitch == null)
                return false;

            currentSwitch.ToggleState();
            return true;
        }

        private void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnGUI()
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            Color reticle = currentSwitch != null
                ? new Color(0.643f, 0.788f, 0.306f, 1f)
                : currentCollider != null
                    ? new Color(0.961f, 0.820f, 0.553f, 1f)
                    : Color.white;

            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.DrawTexture(new Rect(cx - 1f, cy - 10f, 2f, 20f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx - 10f, cy - 1f, 20f, 2f), Texture2D.whiteTexture);
            GUI.color = reticle;
            GUI.DrawTexture(new Rect(cx - 1f, cy - 7f, 2f, 14f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx - 7f, cy - 1f, 14f, 2f), Texture2D.whiteTexture);
            GUI.color = previous;

            string message;
            if (currentSwitch != null)
            {
                string state = currentSwitch.IsOn
                    ? "<color=#A4C94E><b>ON · ENCENDIDO</b></color>"
                    : "<color=#E66B56><b>OFF · APAGADO</b></color>";
                message = currentObjectName + "\n" + state + "    <color=#FFFFFF>[E] Alternar</color>";
            }
            else if (currentCollider != null)
            {
                message = "<b>" + currentObjectName + "</b>\nObjeto identificado · solo revisión";
            }
            else
            {
                message = "Centro: identificar objeto     <color=#FFFFFF>WASD · ratón · ESPACIO · SHIFT</color>";
            }

            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.richText = true;
            box.alignment = TextAnchor.MiddleCenter;
            box.fontSize = 15;
            box.normal.textColor = Color.white;
            GUI.Box(new Rect(cx - 205f, Screen.height - 82f, 410f, 60f), message, box);

            GUIStyle hint = new GUIStyle(GUI.skin.label);
            hint.fontSize = 12;
            hint.normal.textColor = new Color(1f, 1f, 1f, 0.8f);
            GUI.Label(new Rect(16f, Screen.height - 28f, 280f, 20f), cursorLocked ? "ESC: liberar cursor" : "Clic: volver a controlar · ESC: liberar", hint);
        }
    }
}