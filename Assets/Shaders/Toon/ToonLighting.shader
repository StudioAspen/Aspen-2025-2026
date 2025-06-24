Shader "Hidden/ToonPost"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);

                Varyings o;
                o.uv = uv;
                o.positionCS = float4(uv * 2.0f - 1.0f, 0.0f, 1.0f);
                return o;
            }

            float3 ApplyToonLighting(float3 color)
            {
                float brightness = dot(color, float3(0.3, 0.59, 0.11));
                float band = step(0.5, brightness);
                return color * band;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgba;
                col = ApplyToonLighting(col);
                return float4(col, 1);
            }

            ENDHLSL
        }
    }
}