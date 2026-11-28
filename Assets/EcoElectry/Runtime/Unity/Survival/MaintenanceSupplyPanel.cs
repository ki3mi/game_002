using EcoElectry.Unity.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EcoElectry.Unity.Survival
{
    [DisallowMultipleComponent]
    public sealed class MaintenanceSupplyPanel : MonoBehaviour
    {
        [SerializeField] private Collider panel;
        [SerializeField] private Transform lever;
        [SerializeField] private HorrorCorridorExplorer explorer;
        [SerializeField] private Camera view;
        [SerializeField] private string schoolScene;

        private bool aimed;
        private bool loading;
        private GUIStyle style;

        private void Start() => ShowSwitchPosition();

        private void Update()
        {
            aimed = !loading && explorer.isActiveAndEnabled && !explorer.IsImageMenuOpen &&
                Cursor.lockState == CursorLockMode.Locked && Physics.Raycast(
                    view.ViewportPointToRay(new Vector3(.5f, .5f)), out RaycastHit hit, 2.4f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider == panel;
            if (!aimed || Keyboard.current?.eKey.wasPressedThisFrame != true) return;
            Operate();
        }

        private void Operate()
        {
            if (!SurvivalRun.SupplyCut)
            {
                SurvivalRun.CutSupply();
                ShowSwitchPosition();
                return;
            }
            int scene = SceneUtility.GetBuildIndexByScenePath(schoolScene);
            if (scene < 0)
            {
                Debug.LogError("Registra la escena del colegio en la compilación.", this);
                return;
            }
            loading = SceneManager.LoadSceneAsync(scene) != null;
        }

        private void ShowSwitchPosition() => lever.localRotation = Quaternion.Euler(SurvivalRun.SupplyCut ? -32 : 32, 0, 0);

        private void OnGUI()
        {
            if (!aimed) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 15, wordWrap = true };
            var area = new Rect((Screen.width - 405) * .5f, Screen.height * .5f + 23, 405, 60);
            Color previous = GUI.color;
            GUI.color = new Color(.046f, .077f, .091f, .96f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;
            GUI.Label(area, SurvivalRun.SupplyCut
                ? "Suministro cortado. El colegio permanecerá apagado.\nE   Entrar al colegio"
                : "Aísla el circuito antes de desmontar o medir.\nE   Cortar suministro del colegio", style);
        }
    }
}
