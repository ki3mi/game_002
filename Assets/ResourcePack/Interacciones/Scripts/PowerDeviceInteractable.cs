using System;
using UnityEngine;

namespace AssetInteraction
{
    public abstract class PowerDeviceInteractable : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private Renderer[] stateRenderers;
        [SerializeField] private bool startPowered;
        [SerializeField] private Color onColor = new Color(0.12f, 0.78f, 0.36f, 1f);
        [SerializeField] private Color offColor = new Color(0.20f, 0.08f, 0.08f, 1f);
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private MaterialPropertyBlock block;

        public bool IsPowered { get; private set; }
        public event Action<bool> PowerChanged;
        public string InteractionPrompt => IsPowered ? "[E] Apagar" : "[E] Encender";

        protected virtual void Awake()
        {
            block = new MaterialPropertyBlock();
            IsPowered = startPowered;
            ApplyVisualState();
        }

        public void ConfigureStateRenderers(Renderer[] values, bool powered = false)
        {
            stateRenderers = values ?? new Renderer[0];
            startPowered = powered;
            IsPowered = powered;
            ApplyVisualState();
        }

        public virtual void Interact() => SetPowered(!IsPowered);

        public void SetPowered(bool powered)
        {
            if (IsPowered == powered) return;
            IsPowered = powered;
            ApplyVisualState();
            PowerChanged?.Invoke(IsPowered);
        }

        private void ApplyVisualState()
        {
            if (stateRenderers == null) return;
            if (block == null) block = new MaterialPropertyBlock();
            Color color = IsPowered ? onColor : offColor;
            foreach (Renderer r in stateRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                block.SetColor(ColorId, color);
                r.SetPropertyBlock(block);
            }
        }
    }
}