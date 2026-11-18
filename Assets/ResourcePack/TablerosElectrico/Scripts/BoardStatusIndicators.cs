using System.Linq;
using UnityEngine;

namespace TablerosEnergia
{
    /// <summary>Red: no bus power; amber: some outputs off; green: all outputs powered.</summary>
    [DisallowMultipleComponent]
    public sealed class BoardStatusIndicators : MonoBehaviour
    {
        [SerializeField] private Renderer red;
        [SerializeField] private Renderer amber;
        [SerializeField] private Renderer green;
        private ModularBoard board;
        private EnergyGridRegistry registry;
        private MaterialPropertyBlock block;

        public void Configure(Renderer redLight, Renderer amberLight, Renderer greenLight)
        {
            red = redLight; amber = amberLight; green = greenLight;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            board = GetComponent<ModularBoard>();
            block = new MaterialPropertyBlock();
            EnergyGridRegistry.EnsureRuntimeInstance();
            registry = EnergyGridRegistry.Instance;
            registry.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (registry != null) registry.Changed -= Refresh;
            registry = null;
        }

        private void Refresh()
        {
            if (board == null) return;
            bool powered = board.IsBusEnergized;
            var outputs = board.Slots.Where(s => s != null &&
                (s.Role == BoardSlotRole.Load || s.Role == BoardSlotRole.Feeder)).ToArray();
            bool allOn = outputs.Length > 0 && outputs.All(s => s.IsEnergized);
            Set(red, new Color(.90f, .22f, .14f), !powered);
            Set(amber, new Color(.95f, .65f, .16f), powered && !allOn);
            Set(green, new Color(.37f, .75f, .18f), powered && allOn);
        }

        private void Set(Renderer target, Color color, bool on)
        {
            if (target == null) return;
            if (block == null) block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            block.SetColor("_BaseColor", on ? color : color * .16f);
            block.SetColor("_EmissionColor", on ? color * .4f : Color.black);
            target.SetPropertyBlock(block);
        }
    }
}
