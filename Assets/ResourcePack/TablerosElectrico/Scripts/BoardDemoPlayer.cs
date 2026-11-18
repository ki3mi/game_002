using Interruptores;
using UnityEngine;
using AssetInteraction;

namespace TablerosEnergia
{
    /// <summary>Small first-person test adapter; replace its inventory with the game's inventory.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class BoardDemoPlayer : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float reach = 2.5f;
        [SerializeField] private int[] inventory = { 40, 2, 4, 2, 2, 2 };
        private CharacterController character;
        private BoardPartKind selected = BoardPartKind.Breaker;
        private float pitch;
        private float vertical;
        private BoardSlot lookedAt;
        private string prompt = string.Empty;

        public BoardPartKind SelectedPart => selected;
        public int Count(BoardPartKind kind) => inventory[(int)kind];
        public void Select(BoardPartKind kind) { selected = kind; }

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                return;
            }

            for (int i = 0; i < 6; i++)
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) selected = (BoardPartKind)i;
            transform.Rotate(0f, Input.GetAxis("Mouse X") * lookSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * lookSensitivity, -85f, 85f);
            if (playerCamera != null) playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 motion = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
            if (motion.sqrMagnitude > 1f) motion.Normalize();
            if (character.isGrounded && vertical < 0f) vertical = -2f;
            if (character.isGrounded && Input.GetKeyDown(KeyCode.Space)) vertical = 5f;
            vertical += Physics.gravity.y * Time.deltaTime;
            character.Move((motion * moveSpeed + Vector3.up * vertical) * Time.deltaTime);

            UpdateTarget();
            if (Input.GetKeyDown(KeyCode.E)) TryUse();
            if (Input.GetKeyDown(KeyCode.R)) TryRemove();
        }

        private bool Raycast(out RaycastHit hit)
        {
            hit = default;
            return playerCamera != null && Physics.Raycast(playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0)),
                out hit, reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        private void UpdateTarget()
        {
            lookedAt = null;
            prompt = string.Empty;
            if (!Raycast(out RaycastHit hit)) return;
            lookedAt = hit.collider.GetComponentInParent<BoardSlot>();
            if (hit.collider.GetComponentInParent<DifferentialTestButton>() != null)
                prompt = "E: probar diferencial";
            else if (lookedAt != null)
            {
                if (!lookedAt.IsOccupied)
                    prompt = lookedAt.DisplayName + " · VACIO   E: instalar " + selected;
                else if (lookedAt.Role == BoardSlotRole.Meter)
                {
                    var display = lookedAt.InstalledObject.GetComponentInChildren<TextMesh>(true);
                    prompt = lookedAt.DisplayName + " · " + (display != null ? display.text.Replace('\n', ' ') : "SIN LECTURA") + "   R: retirar";
                }
                else if (lookedAt.Role == BoardSlotRole.Terminals)
                    prompt = lookedAt.DisplayName + " · INSTALADOS   R: retirar";
                else
                    prompt = lookedAt.DisplayName + (lookedAt.IsEnergized ? " · ENERGIZADO" : " · SIN ENERGIA") +
                        "   E: accionar   R: retirar";
            }
            else if (hit.collider.GetComponentInParent<BoardDoorInteractable>() != null)
                prompt = "E: abrir / cerrar";
            else
            {
                IPlayerInteractable interactable = FindInteractable(hit.collider);
                if (interactable != null) prompt = interactable.InteractionPrompt;
            }
        }

        public bool TryUse()
        {
            if (!Raycast(out RaycastHit hit)) return false;
            var testButton = hit.collider.GetComponentInParent<DifferentialTestButton>();
            if (testButton != null) return testButton.TryTest();
            var slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (slot != null)
            {
                if (!slot.IsOccupied)
                {
                    if (inventory[(int)selected] <= 0) return false;
                    InstallResult result = slot.TryInstall(selected);
                    if (result != InstallResult.Installed) return false;
                    inventory[(int)selected]--;
                    return true;
                }
                var contactor = slot.InstalledObject.GetComponentInChildren<ContactorDevice>(true);
                if (contactor != null) { contactor.ToggleControlSignal(); return true; }
                if (slot.SwitchControl != null) { slot.SwitchControl.ToggleState(); return true; }
                return false;
            }
            var sw = hit.collider.GetComponentInParent<InterruptorInteractivo>();
            if (sw != null) { sw.ToggleState(); return true; }
            var door = hit.collider.GetComponentInParent<BoardDoorInteractable>();
            if (door != null) { door.Toggle(); return true; }
            IPlayerInteractable generic = FindInteractable(hit.collider);
            if (generic != null) { generic.Interact(); return true; }
            return false;
        }
        private static IPlayerInteractable FindInteractable(Collider collider)
        {
            foreach (MonoBehaviour behaviour in collider.GetComponentsInParent<MonoBehaviour>(true))
                if (behaviour is IPlayerInteractable interactable) return interactable;
            return null;
        }

        public bool TryRemove()
        {
            if (!Raycast(out RaycastHit hit)) return false;
            var slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (slot == null || !slot.TryRemove(out BoardPartKind removed)) return false;
            inventory[(int)removed]++;
            return true;
        }

        private void OnGUI()
        {
            float x = Screen.width * .5f;
            float y = Screen.height * .5f;
            GUI.DrawTexture(new Rect(x - 1, y - 7, 2, 14), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x - 7, y - 1, 14, 2), Texture2D.whiteTexture);
            GUI.Box(new Rect(x - 250, Screen.height - 100, 500, 75),
                prompt + "\n1 Termico  2 Seccionador  3 Bornes  4 Diferencial  5 Contactor  6 Medidor" +
                "\nSeleccion: " + selected + " (" + Count(selected) + ")");
        }
    }
}
