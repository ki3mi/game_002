using System;
using UnityEngine;

namespace EcoElectry.Unity.Devices
{
    public enum DevicePowerState
    {
        Apagado = 0,
        Encendido = 1,
        SinSuministro = 2
    }

    public enum DeviceMotionType
    {
        RotacionContinua = 0,
        IndicadorEmisivo = 1
    }

    /// <summary>
    /// Coordina movimiento mecánico, Animator, emisión de materiales URP y audio
    /// según el estado real de encendido y suministro eléctrico del dispositivo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeviceOperationalAnimator : MonoBehaviour
    {
        private static readonly int HashEstadoEnergia = Animator.StringToHash("EstadoEnergia");
        private static readonly int HashEncendido = Animator.StringToHash("Encendido");
        private static readonly int HashSinSuministro = Animator.StringToHash("SinSuministro");
        private static readonly int HashTriggerEncendido = Animator.StringToHash("TriggerEncendido");
        private static readonly int HashTriggerCorte = Animator.StringToHash("TriggerCorte");

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Estado Inicial")]
        [SerializeField] private DevicePowerState initialState = DevicePowerState.Apagado;
        [SerializeField] private DeviceMotionType motionType = DeviceMotionType.RotacionContinua;

        [Header("Movimiento Mecánico (Ventiladores / Rotores)")]
        [SerializeField] private Transform[] movingParts;
        [SerializeField] private Vector3 localSpinAxis = Vector3.up;
        [SerializeField, Min(1f)] private float targetRpm = 210f;
        [SerializeField, Min(0.05f)] private float spinUpSeconds = 0.65f;
        [SerializeField, Min(0.05f)] private float spinDownSeconds = 1.1f;

        [Header("Señales Visuales (MaterialPropertyBlock URP)")]
        [SerializeField] private Renderer[] indicatorRenderers;
        [SerializeField] private Color onColor = new Color(0.18f, 0.82f, 0.42f, 1f);
        [SerializeField] private Color offColor = new Color(0.20f, 0.22f, 0.25f, 1f);
        [SerializeField] private Color outageAlertColor = new Color(0.92f, 0.26f, 0.18f, 1f);
        [SerializeField, Min(0.1f)] private float outageAlertDuration = 1.6f;

        [Header("Audio Coordinado (Opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip powerOnClip;
        [SerializeField] private AudioClip powerOffClip;
        [SerializeField] private AudioClip powerOutageClip;
        [SerializeField] private AudioClip runningLoopClip;

        private Animator animator;
        private MaterialPropertyBlock propertyBlock;
        private DevicePowerState currentState;
        private float currentRpm;
        private float outageTimer;

        public DevicePowerState CurrentState => currentState;
        public float CurrentRpm => currentRpm;
        public bool IsVisuallyActive => currentState == DevicePowerState.Encendido || currentRpm > 0.5f;

        public event Action<DevicePowerState> OnDeviceStateChanged;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            propertyBlock = new MaterialPropertyBlock();

            if (motionType == DeviceMotionType.RotacionContinua && (movingParts == null || movingParts.Length == 0))
                movingParts = AutoDetectRotors();

            currentState = initialState;
            currentRpm = currentState == DevicePowerState.Encendido ? targetRpm : 0f;
            outageTimer = 0f;

            SyncAnimator(triggerOn: false, triggerOutage: false);
            ApplyIndicatorVisuals(Color.black, 0f);
            SyncAudio(playTransitionOneShot: false, previousState: currentState);
        }

        private void Update()
        {
            UpdateMechanicalMotion();
            UpdateVisualSignals();
        }

        /// <summary>
        /// Método principal para conectar con la simulación eléctrica:
        /// isSwitchedOn = interruptor del usuario; hasPowerSupply = energía disponible en circuito.
        /// </summary>
        public void SetOperationalState(bool isSwitchedOn, bool hasPowerSupply)
        {
            if (!hasPowerSupply)
            {
                SetDeviceState(DevicePowerState.SinSuministro);
                return;
            }

            SetDeviceState(isSwitchedOn ? DevicePowerState.Encendido : DevicePowerState.Apagado);
        }

        public void SetDeviceState(DevicePowerState newState)
        {
            if (currentState == newState)
                return;

            DevicePowerState previous = currentState;
            currentState = newState;

            bool turnedOn = newState == DevicePowerState.Encendido;
            bool powerLost = newState == DevicePowerState.SinSuministro && previous == DevicePowerState.Encendido;

            if (newState == DevicePowerState.SinSuministro)
                outageTimer = outageAlertDuration;
            else
                outageTimer = 0f;

            SyncAnimator(triggerOn: turnedOn, triggerOutage: powerLost);
            SyncAudio(playTransitionOneShot: true, previousState: previous);
            OnDeviceStateChanged?.Invoke(currentState);
        }

        public void ConfigureParts(Transform[] rotors, Vector3 axis, float rpm, Renderer[] indicators)
        {
            movingParts = rotors ?? Array.Empty<Transform>();
            localSpinAxis = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
            targetRpm = Mathf.Max(1f, rpm);
            indicatorRenderers = indicators ?? Array.Empty<Renderer>();
        }

        private void UpdateMechanicalMotion()
        {
            if (motionType != DeviceMotionType.RotacionContinua || movingParts == null || movingParts.Length == 0)
            {
                currentRpm = 0f;
                return;
            }

            float desiredRpm = currentState == DevicePowerState.Encendido ? targetRpm : 0f;
            float rampTime = desiredRpm > currentRpm ? spinUpSeconds : spinDownSeconds;
            float rate = targetRpm / Mathf.Max(0.05f, rampTime);

            currentRpm = Mathf.MoveTowards(currentRpm, desiredRpm, rate * Time.deltaTime);

            if (currentRpm <= 0.01f)
            {
                currentRpm = 0f;
                return;
            }

            float degrees = currentRpm * 6f * Time.deltaTime;
            Vector3 axis = localSpinAxis.sqrMagnitude > 0.001f ? localSpinAxis.normalized : Vector3.up;

            for (int i = 0; i < movingParts.Length; i++)
            {
                if (movingParts[i] != null)
                    movingParts[i].Rotate(axis, degrees, Space.Self);
            }
        }

        private void UpdateVisualSignals()
        {
            if (indicatorRenderers == null || indicatorRenderers.Length == 0)
                return;

            if (currentState == DevicePowerState.Encendido)
            {
                float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 4f);
                ApplyIndicatorVisuals(onColor, 0.65f * pulse);
            }
            else if (currentState == DevicePowerState.SinSuministro)
            {
                if (outageTimer > 0f)
                {
                    outageTimer -= Time.deltaTime;
                    float blink = Mathf.PingPong(Time.time * 8f, 1f);
                    Color c = Color.Lerp(offColor, outageAlertColor, blink);
                    ApplyIndicatorVisuals(c, 0.7f * blink);
                }
                else
                {
                    ApplyIndicatorVisuals(offColor, 0f);
                }
            }
            else
            {
                ApplyIndicatorVisuals(offColor, 0f);
            }
        }

        private void ApplyIndicatorVisuals(Color baseColor, float emissionStrength)
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color emission = emissionStrength > 0.001f ? baseColor * emissionStrength : Color.black;

            for (int i = 0; i < indicatorRenderers.Length; i++)
            {
                Renderer r = indicatorRenderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, baseColor);
                propertyBlock.SetColor(EmissionColorId, emission);
                r.SetPropertyBlock(propertyBlock);
            }
        }

        private void SyncAnimator(bool triggerOn, bool triggerOutage)
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            animator.SetInteger(HashEstadoEnergia, (int)currentState);
            animator.SetBool(HashEncendido, currentState == DevicePowerState.Encendido);
            animator.SetBool(HashSinSuministro, currentState == DevicePowerState.SinSuministro);

            if (triggerOn)
                animator.SetTrigger(HashTriggerEncendido);
            if (triggerOutage)
                animator.SetTrigger(HashTriggerCorte);
        }

        private void SyncAudio(bool playTransitionOneShot, DevicePowerState previousState)
        {
            if (audioSource == null)
                return;

            if (currentState == DevicePowerState.Encendido)
            {
                if (playTransitionOneShot && powerOnClip != null)
                    audioSource.PlayOneShot(powerOnClip);

                if (runningLoopClip != null)
                {
                    audioSource.clip = runningLoopClip;
                    audioSource.loop = true;
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.loop)
                    audioSource.Stop();

                if (!playTransitionOneShot)
                    return;

                if (currentState == DevicePowerState.SinSuministro && previousState == DevicePowerState.Encendido && powerOutageClip != null)
                    audioSource.PlayOneShot(powerOutageClip);
                else if (currentState == DevicePowerState.Apagado && previousState == DevicePowerState.Encendido && powerOffClip != null)
                    audioSource.PlayOneShot(powerOffClip);
            }
        }

        private Transform[] AutoDetectRotors()
        {
            var found = new System.Collections.Generic.List<Transform>();
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("rotor") || n.Contains("aspa") || n.Contains("brazo") || n.Contains("helice") || n.Contains("buje"))
                    found.Add(t);
            }
            return found.ToArray();
        }
    }
}
