using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace EcoElectry.Unity.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class CorridorSignalGlitch : MonoBehaviour
    {
        [SerializeField] private Shader signalShader;
        [Tooltip("Horizontal displacement as a fraction of the picture width.")]
        [SerializeField, Range(0f, 0.015f)] private float displacement = 0.0045f;
        [SerializeField, Min(0.1f)] private float minimumInterval = 3f;
        [SerializeField, Min(0.1f)] private float maximumInterval = 6f;
        [SerializeField, Range(0.05f, 0.4f)] private float pulseDuration = 0.17f;

        private static readonly int Signal = Shader.PropertyToID("_Signal");
        private Camera targetCamera;
        private UniversalAdditionalCameraData cameraData;
        private Material material;
        private SignalPass pass;
        private System.Random sequence;
        private float nextPulse;
        private float bandCenter;
        private float direction;

        private void OnEnable()
        {
            if (signalShader == null) return;
            targetCamera = GetComponent<Camera>();
            cameraData = targetCamera.GetUniversalAdditionalCameraData();
            material = CoreUtils.CreateEngineMaterial(signalShader);
            pass = new SignalPass(material);
            sequence = new System.Random(421);
            SchedulePulse(Time.time, 1.4f);
            RenderPipelineManager.beginCameraRendering += RenderSignal;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= RenderSignal;
            CoreUtils.Destroy(material);
        }

        private void SchedulePulse(float now, float delay)
        {
            nextPulse = now + delay;
            bandCenter = Mathf.Lerp(0.12f, 0.88f, (float)sequence.NextDouble());
            direction = sequence.Next(2) == 0 ? -1f : 1f;
        }

        private void RenderSignal(ScriptableRenderContext context, Camera camera)
        {
            if (camera != targetCamera || displacement <= 0f) return;
            float elapsed = Time.time - nextPulse;
            if (elapsed < 0f) return;
            if (elapsed >= pulseDuration)
            {
                float interval = Mathf.Lerp(minimumInterval, Mathf.Max(minimumInterval, maximumInterval),
                    (float)sequence.NextDouble());
                SchedulePulse(Time.time, interval);
                return;
            }

            material.SetVector(Signal, new Vector4(bandCenter, direction * displacement, elapsed / pulseDuration, 0f));
            cameraData.scriptableRenderer.EnqueuePass(pass);
        }

        private sealed class SignalPass : ScriptableRenderPass
        {
            private readonly Material material;

            public SignalPass(Material material)
            {
                this.material = material;
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                requiresIntermediateTexture = true;
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame)
            {
                UniversalResourceData resources = frame.Get<UniversalResourceData>();
                TextureHandle source = resources.activeColorTexture;
                TextureDesc desc = graph.GetTextureDesc(source);
                desc.name = "Corridor signal";
                desc.clearBuffer = false;
                TextureHandle output = graph.CreateTexture(desc);
                var blit = new RenderGraphUtils.BlitMaterialParameters(source, output, material, 0);
                graph.AddBlitPass(blit, passName: desc.name);
                resources.cameraColor = output;
            }
        }
    }
}
