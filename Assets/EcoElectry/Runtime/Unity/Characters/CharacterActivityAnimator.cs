using System;
using UnityEngine;

namespace EcoElectry.Unity.Characters
{
    public enum CharacterActivityState
    {
        Reposo = 0,
        Desplazamiento = 1,
        Estudio = 2,
        UsoEquipos = 3
    }

    /// <summary>
    /// Controla las acciones básicas de docentes, estudiantes y personal técnico
    /// según la continuidad o interrupción del suministro educativo en la zona.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterActivityAnimator : MonoBehaviour
    {
        private static readonly int HashEstadoActividad = Animator.StringToHash("EstadoActividad");
        private static readonly int HashClaseActiva = Animator.StringToHash("ClaseActiva");
        private static readonly int HashEnMovimiento = Animator.StringToHash("EnMovimiento");
        private static readonly int HashInterrumpido = Animator.StringToHash("Interrumpido");

        [Header("Estado Inicial")]
        [SerializeField] private CharacterActivityState initialState = CharacterActivityState.Estudio;
        [SerializeField] private bool startWithPowerActive = true;

        private Animator animator;
        private CharacterActivityState currentState;
        private bool isClassActive;

        public CharacterActivityState CurrentState => currentState;
        public bool IsClassActive => isClassActive;

        public event Action<CharacterActivityState, bool> OnActivityUpdated;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            transform.localScale = Vector3.one;
            currentState = initialState;
            isClassActive = startWithPowerActive;
            ApplyAnimatorParameters(triggerInterruption: false);
        }

        /// <summary>
        /// Llamado por el sistema educativo/eléctrico cuando la zona tiene energía (true)
        /// o sufre un corte/sobrecarga que interrumpe la clase (false).
        /// </summary>
        public void SetClassActive(bool active)
        {
            if (isClassActive == active)
                return;

            bool wasInterrupted = isClassActive && !active;
            isClassActive = active;
            ApplyAnimatorParameters(triggerInterruption: wasInterrupted);
            OnActivityUpdated?.Invoke(currentState, isClassActive);
        }

        /// <summary>
        /// Cambia la acción principal del personaje sin simulación social compleja.
        /// </summary>
        public void SetActivityState(CharacterActivityState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;
            ApplyAnimatorParameters(triggerInterruption: false);
            OnActivityUpdated?.Invoke(currentState, isClassActive);
        }

        public void SetMoving(bool moving)
        {
            SetActivityState(moving ? CharacterActivityState.Desplazamiento : CharacterActivityState.Reposo);
        }

        private void ApplyAnimatorParameters(bool triggerInterruption)
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            animator.SetInteger(HashEstadoActividad, (int)currentState);
            animator.SetBool(HashClaseActiva, isClassActive);
            animator.SetBool(HashEnMovimiento, currentState == CharacterActivityState.Desplazamiento);

            if (triggerInterruption)
                animator.SetTrigger(HashInterrumpido);
        }
    }
}
