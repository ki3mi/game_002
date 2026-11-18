using Interruptores;
using UnityEngine;
using UnityEngine.Events;
using TablerosEnergia;

/// <summary>Game-level residual-current device with a powered test and manual reset.</summary>
[DisallowMultipleComponent]
public sealed class DifferentialDevice : MonoBehaviour
{
    [SerializeField] private InterruptorInteractivo lever;
    public UnityEvent OnTripped = new UnityEvent();
    private BoardSlot slot;
    public bool IsTripped { get; private set; }

    public void Configure(InterruptorInteractivo control) { lever = control; }

    public void Bind(BoardSlot mountingSlot)
    {
        slot = mountingSlot;
        if (lever == null) lever = GetComponentInChildren<InterruptorInteractivo>(true);
        if (lever != null)
        {
            lever.OnStateChanged.RemoveListener(OnLeverChanged);
            lever.OnStateChanged.AddListener(OnLeverChanged);
        }
    }

    private void OnDisable()
    {
        if (lever != null) lever.OnStateChanged.RemoveListener(OnLeverChanged);
    }

    public bool TryTest()
    {
        if (slot == null || slot.Board == null || !slot.Board.HasIncomingPower ||
            lever == null || !lever.IsOn) return false;
        IsTripped = true;
        lever.SetState(false);
        OnTripped.Invoke();
        slot.NotifyChanged();
        return true;
    }

    private void OnLeverChanged(bool isOn)
    {
        if (isOn && IsTripped)
        {
            IsTripped = false;
            if (slot != null) slot.NotifyChanged();
        }
    }
}
