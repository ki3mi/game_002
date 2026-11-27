Shader "Hidden/EcoElectry/Introduction Distance Fog"
{
    Properties
    {
        _FogColor ("Fog color", Color) = (0, 0, 0, 1)
        [NoScaleOffset] _SkyA ("Previous sky", Cube) = "" {}
        [NoScaleOffset] _SkyB ("Next sky", Cube) = "" {}
        _YawA ("Previous yaw", Vector) = (1, 0, 0, 0)
        _YawB ("Next yaw", Vector) = (1, 0, 0, 0)
        _Blend ("Blend", Range(0, 1)) = 0
        _Luminance ("Sky luminance", Range(0, 1)) = 1
        _SunsetWarmth ("Sunset warmth", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, Zero One

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "IntroductionSkyColor.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.positionCS.xy / _ScaledScreenParams.xy;
                float rawDepth = SampleSceneDepth(uv);
                #if UNITY_REVERSED_Z
                    bool skyPixel = rawDepth <= 0.0;
                    float depth = rawDepth;
                #else
                    bool skyPixel = rawDepth >= 1.0;
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, rawDepth);
                #endif
                if (skyPixel) return 0;
                float3 world = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
                float fog = smoothstep(_FogSettings.z, _FogSettings.w, distance(world.xz, _FogSettings.xy));
                if (fog <= 0.0) return 0;
                // Fully obscured geometry must match the sky behind it, even above the horizon.
                return half4(SampleIntroductionSky(normalize(world - _WorldSpaceCameraPos)), fog);
            }
            ENDHLSL
        }
    }
}
