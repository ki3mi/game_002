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
            float _GrainInterference;

            float Noise(float2 cell)
            {
                float3 p = frac(cell.xyx * float3(0.1031, 0.11369, 0.13787));
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float phase = _Signal.z;
                float center = _Signal.x + floor(phase * 3.0) * 0.009;
                float ribbon = 1.0 - smoothstep(0.009, 0.022, abs(uv.y - center));
                float echo = (1.0 - smoothstep(0.001, 0.004, abs(uv.y - center + 0.047))) * step(0.00001, abs(_Signal.y));
                float strength = (1.0 - phase * phase) * (phase < 0.57 ? 1.0 : -0.55);
                float shift = _Signal.y * strength * (ribbon - 0.65 * echo);
                float noiseFrame = floor(_Time.y * 24.0);
                if (_GrainInterference > 0.0)
                {
                    float rowNoise = Noise(float2(floor(uv.y * 92.0), noiseFrame));
                    shift += _GrainInterference * 0.012 * step(0.83, rowNoise) * (rowNoise - 0.9);
                }
                float2 shifted = uv + float2(shift, 0.0);
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, shifted);
                color.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, shifted + float2(shift * 0.39, 0)).r;
                color.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, shifted - float2(shift * 0.25, 0)).b;
                if (_GrainInterference > 0.0)
                {
                    float2 pixel = input.positionCS.xy;
                    float fine = Noise(floor(pixel / 2.0) + float2(noiseFrame * 3.71, noiseFrame * 5.13)) - 0.5;
                    float rough = Noise(floor(pixel / 5.0) - float2(noiseFrame * 6.17, noiseFrame * 2.71)) - 0.5;
                    half luminance = dot(max(color.rgb, 0.0), half3(0.2126, 0.7152, 0.0722));
                    half amplitude = 0.028 + sqrt(luminance) * 0.7;
                    color.rgb = max(0.0, color.rgb + _GrainInterference * amplitude * (0.68 * fine + 0.32 * rough));
                }
                color.rgb *= (1.0 - 0.09 * abs(strength) * echo) * (1.0 - _Signal.w);
                return color;
            }
            ENDHLSL
        }
    }
}
