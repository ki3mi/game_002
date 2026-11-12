using TablerosEnergia;
using UnityEngine;

/// <summary>Control signal and visual state of the contactor used by the game.</summary>
[DisallowMultipleComponent]
public sealed class ContactorDevice : MonoBehaviour
{
    [SerializeField] private Transform actuator;
    [SerializeField] private Renderer indicator;
    [SerializeField, Min(0f)] private float travel = .003f;
    private BoardSlot slot;
    private Vector3 restPosition;
    private bool closed;
    private MaterialPropertyBlock properties;

    public bool ControlSignal { get; private set; }
    public bool IsClosed => closed;

    public void Configure(Transform movingPart, Renderer lamp)
    {
        actuator = movingPart;
        indicator = lamp;
    }

    private void Awake()
    {
        if (actuator != null) restPosition = actuator.localPosition;
        properties = new MaterialPropertyBlock();
    }

    public void Bind(BoardSlot mountingSlot) { slot = mountingSlot; }

    public void SetControlSignal(bool active)
    {
        if (ControlSignal == active) return;
        ControlSignal = active;
        if (slot != null) slot.NotifyChanged();
    }

    public void ToggleControlSignal() { SetControlSignal(!ControlSignal); }

    public void RefreshPower(bool hasIncomingPower)
    {
        closed = hasIncomingPower && ControlSignal;
        if (indicator == null) return;
        if (properties == null) properties = new MaterialPropertyBlock();
        indicator.GetPropertyBlock(properties);
        Color color = closed ? new Color(.48f, .68f, .22f) : new Color(.28f, .31f, .28f);
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_EmissionColor", closed ? color * .35f : Color.black);
        indicator.SetPropertyBlock(properties);
    }

    private void Update()
    {
        if (actuator == null) return;
        Vector3 target = restPosition + (closed ? Vector3.down * travel : Vector3.zero);
        actuator.localPosition = Vector3.Lerp(actuator.localPosition, target, 1f - Mathf.Exp(-16f * Time.deltaTime));
    }
}
