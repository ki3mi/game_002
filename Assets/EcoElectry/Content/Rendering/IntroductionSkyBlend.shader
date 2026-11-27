Shader "EcoElectry/Introduction Sky Blend"
{
    Properties
    {
        [NoScaleOffset] _SkyA ("Previous sky", Cube) = "" {}
        [NoScaleOffset] _SkyB ("Next sky", Cube) = "" {}
        _YawA ("Previous yaw (cos, sin)", Vector) = (1, 0, 0, 0)
        _YawB ("Next yaw (cos, sin)", Vector) = (1, 0, 0, 0)
        _Blend ("Blend", Range(0, 1)) = 0
        _Luminance ("Sky luminance", Range(0, 1)) = 1
        _SunsetWarmth ("Sunset warmth", Range(0, 1)) = 0
        _FogColor ("Horizon fog", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "IntroductionSkyColor.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = TransformObjectToHClip(input.positionOS);
                output.direction = input.positionOS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return half4(SampleIntroductionSky(normalize(input.direction)), 1);
            }
            ENDHLSL
        }
    }
}
