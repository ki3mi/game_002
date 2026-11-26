Shader "EcoElectry/Introduction Sky Blend"
{
    Properties
    {
        [NoScaleOffset] _SkyA ("Previous sky", Cube) = "" {}
        [NoScaleOffset] _SkyB ("Next sky", Cube) = "" {}
        _YawA ("Previous yaw (cos, sin)", Vector) = (1, 0, 0, 0)
        _YawB ("Next yaw (cos, sin)", Vector) = (1, 0, 0, 0)
        _Blend ("Blend", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            samplerCUBE _SkyA, _SkyB;
            half4 _SkyA_HDR, _SkyB_HDR;
            float4 _YawA, _YawB;
            half _Blend;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(appdata_base vertex)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(vertex);
                UNITY_INITIALIZE_OUTPUT(Varyings, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = UnityObjectToClipPos(vertex.vertex);
                output.direction = vertex.vertex.xyz;
                return output;
            }

            float3 UndoYaw(float3 direction, float2 yaw)
            {
                return float3(yaw.x * direction.x - yaw.y * direction.z, direction.y,
                              yaw.y * direction.x + yaw.x * direction.z);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 a = DecodeHDR(texCUBE(_SkyA, UndoYaw(input.direction, _YawA.xy)), _SkyA_HDR);
                half3 b = DecodeHDR(texCUBE(_SkyB, UndoYaw(input.direction, _YawB.xy)), _SkyB_HDR);
                return half4(lerp(a, b, _Blend), 1);
            }
            ENDCG
        }
    }
}
