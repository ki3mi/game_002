using UnityEngine;
using TablerosEnergia;
using System.Globalization;

/// <summary>Shows estimated active power and energized final circuits on a built-in TextMesh.</summary>
[DisallowMultipleComponent]
public sealed class EnergyMeterDisplay : MonoBehaviour
{
    [SerializeField] private TextMesh screenText;
    private EnergyGridRegistry registry;
    private ModularBoard board;
    private BoardSlot slot;

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        EnergyGridRegistry.EnsureRuntimeInstance();
        registry = EnergyGridRegistry.Instance;
        if (registry != null)
            registry.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (registry != null)
            registry.Changed -= Refresh;
        registry = null;
    }

    private void Start()
    {
        board = GetComponentInParent<ModularBoard>();
        slot = GetComponentInParent<BoardSlot>();
        if (screenText == null)
            screenText = GetComponentInChildren<TextMesh>(true);
        Refresh();
    }

    private void Refresh()
    {
        if (screenText == null)
            screenText = GetComponentInChildren<TextMesh>(true);
        if (screenText == null)
            return;

        if (registry == null)
            registry = EnergyGridRegistry.Instance;

        if (board == null) board = GetComponentInParent<ModularBoard>();
        if (slot == null) slot = GetComponentInParent<BoardSlot>();
        if (slot != null && !slot.IsEnergized)
        {
            screenText.text = "SIN RED";
            return;
        }

        float watts = registry != null ? registry.GetActiveLoadWatts(board) : 0f;
        int active = registry != null ? registry.GetActiveLoadCount(board) : 0;
        int total = registry != null ? registry.GetConfiguredLoadCount(board) : 0;
        screenText.text = (watts / 1000f).ToString("0.00", CultureInfo.InvariantCulture) +
            " kW\n" + active + "/" + total + " ON";
    }
}
