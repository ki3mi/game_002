Shader "Hidden/EcoElectry/CorridorSignal"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _Signal;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float phase = _Signal.z;
                float center = _Signal.x + floor(phase * 3.0) * 0.009;
                float ribbon = 1.0 - smoothstep(0.009, 0.022, abs(uv.y - center));
                float echo = 1.0 - smoothstep(0.001, 0.004, abs(uv.y - center + 0.047));
                float strength = (1.0 - phase * phase) * (phase < 0.57 ? 1.0 : -0.55);
                float shift = _Signal.y * strength * (ribbon - 0.65 * echo);
                float2 shifted = uv + float2(shift, 0.0);
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, shifted);
                color.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, shifted + float2(shift * 0.39, 0)).r;
                color.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, shifted - float2(shift * 0.25, 0)).b;
                color.rgb *= 1.0 - 0.09 * abs(strength) * echo;
                return color;
            }
            ENDHLSL
        }
    }
}
