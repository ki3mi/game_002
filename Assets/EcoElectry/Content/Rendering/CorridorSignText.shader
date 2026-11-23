Shader "EcoElectry/Corridor Sign Text"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font atlas", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Tags { "LightMode" = "UniversalForwardOnly" }
            Cull Back
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Vertex
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Interpolators
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half3 vertexLight : TEXCOORD3;
                half4 color : COLOR;
            };

            Interpolators Vert(Vertex vertex)
            {
                Interpolators output;
                output.positionWS = TransformObjectToWorld(vertex.position.xyz);
                output.position = TransformWorldToHClip(output.positionWS);
                // TextMesh draws the front of the glyphs along local -Z.
                output.normalWS = TransformObjectToWorldNormal(float3(0, 0, -1));
                output.vertexLight = VertexLighting(output.positionWS, output.normalWS);
                output.uv = vertex.uv;
                output.color = vertex.color;
                return output;
            }

            half4 Frag(Interpolators input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - 0.01h);

                InputData lighting = (InputData)0;
                lighting.positionWS = input.positionWS;
                lighting.normalWS = NormalizeNormalPerPixel(input.normalWS);
                lighting.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lighting.vertexLighting = input.vertexLight;
                lighting.bakedGI = SampleSH(lighting.normalWS);
                lighting.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.position);
                lighting.shadowMask = half4(1, 1, 1, 1);

                SurfaceData ink = (SurfaceData)0;
                ink.albedo = input.color.rgb;
                ink.alpha = input.color.a * alpha;
                ink.occlusion = 1.0h;
                return UniversalFragmentBlinnPhong(lighting, ink);
            }
            ENDHLSL
        }
    }
}
