using PuertaAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Interaction
{
    [DefaultExecutionOrder(10)]
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterController))]
    public sealed class SchoolDoorInteractor : MonoBehaviour
    {
        [SerializeField] private Camera view = null;
        [SerializeField, Min(.1f)] private float reach = 2.25f;

        private CharacterController explorer;
        private PuertaInteractable door;

        private void Awake() => explorer = GetComponent<CharacterController>();

        private void LateUpdate()
        {
            door = null;
            if (view == null || Cursor.lockState != CursorLockMode.Locked || Time.timeScale <= 0f)
                return;

            Ray aim = view.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            if (!Physics.Raycast(aim, out RaycastHit hit, reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                return;

            door = hit.collider.GetComponentInParent<PuertaInteractable>();
            // The trigger across the opening also lets the player close a door from either side.
            if (door == null && hit.collider.isTrigger)
                door = hit.collider.GetComponentInChildren<PuertaInteractable>();

            if (door != null && door.isActiveAndEnabled && Keyboard.current?.eKey.wasPressedThisFrame == true)
                door.Interact(explorer);
        }

        private void OnGUI()
        {
            if (door == null || !door.isActiveAndEnabled)
                return;

            var label = new Rect((Screen.width - 136f) * .5f, Screen.height * .5f + 18f, 136f, 25f);
            GUI.Box(label, door.InteractionPrompt);
        }
    }
}
