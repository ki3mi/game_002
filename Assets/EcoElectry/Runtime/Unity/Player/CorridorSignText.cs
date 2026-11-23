using UnityEngine;

namespace EcoElectry.Unity.Player
{
    [ExecuteAlways, RequireComponent(typeof(TextMesh), typeof(MeshRenderer))]
    public sealed class CorridorSignText : MonoBehaviour
    {
        private static readonly int FontAtlas = Shader.PropertyToID("_MainTex");
        private TextMesh label;
        private MeshRenderer surface;
        private MaterialPropertyBlock properties;

        private void OnEnable()
        {
            label = GetComponent<TextMesh>();
            surface = GetComponent<MeshRenderer>();
            properties = new MaterialPropertyBlock();
            Font.textureRebuilt += RefreshAtlas;
            RefreshAtlas(label.font);
        }

        private void OnDisable() => Font.textureRebuilt -= RefreshAtlas;

        private void RefreshAtlas(Font font)
        {
            if (font == null || font != label.font || font.material.mainTexture == null) return;
            surface.GetPropertyBlock(properties);
            properties.SetTexture(FontAtlas, font.material.mainTexture);
            surface.SetPropertyBlock(properties);
        }
    }
}
