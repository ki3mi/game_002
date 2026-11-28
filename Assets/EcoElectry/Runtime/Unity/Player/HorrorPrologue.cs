using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EcoElectry.Unity.Player
{
    [DefaultExecutionOrder(10)]
    [DisallowMultipleComponent, RequireComponent(typeof(HorrorCorridorExplorer))]
    public sealed class HorrorPrologue : MonoBehaviour
    {
        private const float BeamCutTime = .23f;
        private const float CameraCutTime = .46f;
        private const float LungeDistance = 1.15f;
        private const float AnomalyScale = 1.8f;

        [SerializeField] private Camera view;
        [SerializeField] private GameObject anomalyPrefab;
        [SerializeField] private AudioClip sting;
        [SerializeField] private Vector3 apparitionPosition = new(.55f, .04f, 12.2f);
        [SerializeField] private Bounds classroom = new(new Vector3(11.17f, 1f, 12.2f), new Vector3(4.64f, 2.5f, 5.22f));
        [SerializeField] private string introductionScene;

        private HorrorCorridorExplorer explorer;
        private CorridorSignalGlitch signal;
        private FilmGrain grain;
        private AudioSource ambience;
        private Vector3 localGaze;
        private bool visitedClassroom;
        private bool cameraCut;
        private Transform apparition;
        private Light apparitionLight;
        private Vector3 lunge;
        private float elapsed;
        private int introductionIndex;

        private void Start()
        {
            explorer = GetComponent<HorrorCorridorExplorer>();
            introductionIndex = SceneUtility.GetBuildIndexByScenePath(introductionScene);
            if (view == null || anomalyPrefab == null || introductionIndex < 0)
            {
                Debug.LogError("El prólogo requiere cámara, prefab de anomalía y una introducción registrada en la compilación.", this);
                enabled = false;
                return;
            }

            Transform gaze = anomalyPrefab.transform.Find("Punto de mirada");
            localGaze = gaze != null ? gaze.localPosition * AnomalyScale : Vector3.up * 1.45f;
            signal = view.GetComponent<CorridorSignalGlitch>();
            ambience = view.GetComponent<AudioSource>();
            Volume volume = view.GetComponentInChildren<Volume>();
            if (volume != null) volume.profile.TryGet(out grain);
        }

        private void LateUpdate()
        {
            if (cameraCut) return;
            if (apparition != null)
            {
                AdvanceScare(Time.unscaledDeltaTime);
                return;
            }

            visitedClassroom |= classroom.Contains(transform.position);
            if (visitedClassroom && !explorer.IsImageMenuOpen && CanSeeApparition())
                ShowApparition();
        }

        private bool CanSeeApparition()
        {
            Vector3 target = apparitionPosition + localGaze;
            Vector3 point = view.WorldToViewportPoint(target);
            if (point.z < 1f || point.x < .23f || point.x > .77f || point.y < .20f || point.y > .80f)
                return false;

            return !Physics.Linecast(view.transform.position, target, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        private void ShowApparition()
        {
            Vector3 towardCamera = Vector3.ProjectOnPlane(view.transform.position - apparitionPosition, Vector3.up);
            lunge = Vector3.ClampMagnitude(towardCamera * .28f, LungeDistance);
            apparition = Instantiate(anomalyPrefab, apparitionPosition, Quaternion.LookRotation(towardCamera)).transform;
            apparition.localScale *= AnomalyScale;

            var light = new GameObject("Luz de la aparición");
            light.transform.SetParent(apparition, false);
            light.transform.localPosition = new Vector3(.22f, .95f, 1.02f);
            light.transform.localRotation = Quaternion.LookRotation(new Vector3(0f, .76f, 0f) - light.transform.localPosition);
            apparitionLight = light.AddComponent<Light>();
            apparitionLight.type = LightType.Spot;
            apparitionLight.color = new Color(.68f, .81f, 1f);
            apparitionLight.spotAngle = 72f;
            apparitionLight.innerSpotAngle = 42f;
            apparitionLight.range = 4.5f;
            apparitionLight.intensity = 7f;

            explorer.enabled = false;
            HorrorCorridorExplorer.SetCursorLocked(true);
            if (grain != null) grain.intensity.Override(1f);
            if (signal != null) signal.GrainInterference = Mathf.Max(signal.GrainInterference, .60f);
            if (sting != null) AudioSource.PlayClipAtPoint(sting, view.transform.position, .85f);
        }

        private void AdvanceScare(float seconds)
        {
            float previous = elapsed;
            elapsed += seconds;
            apparition.position = apparitionPosition + lunge * Mathf.SmoothStep(0f, 1f, elapsed / BeamCutTime);
            if (previous < BeamCutTime && elapsed >= BeamCutTime)
            {
                explorer.PresentBeam(false);
                apparitionLight.enabled = false;
                if (signal != null) signal.GrainInterference = 1f;
            }
            if (elapsed < CameraCutTime) return;

            cameraCut = true;
            if (ambience != null) ambience.Stop();
            SceneManager.LoadSceneAsync(introductionIndex, LoadSceneMode.Single);
        }

        private void OnGUI()
        {
            if (!cameraCut || Event.current.type != EventType.Repaint) return;
            int depth = GUI.depth;
            Color color = GUI.color;
            GUI.depth = -100;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.depth = depth;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(classroom.center, classroom.size);
            Gizmos.DrawWireSphere(apparitionPosition + Vector3.up, .45f);
        }
    }
}
