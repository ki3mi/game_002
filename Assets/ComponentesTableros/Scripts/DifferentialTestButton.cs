using UnityEngine;

/// <summary>Collider on the differential's separate test button.</summary>
[DisallowMultipleComponent]
public sealed class DifferentialTestButton : MonoBehaviour
{
    public bool TryTest()
    {
        var device = GetComponentInParent<DifferentialDevice>();
        return device != null && device.TryTest();
    }
}
