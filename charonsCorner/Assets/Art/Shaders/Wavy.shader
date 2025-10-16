Shader "UI/WavyOverlay"
{
    Properties
    {
        _Color ("Color", Color) = (0,0.5,1,0.4)
        _MainTex ("Texture", 2D) = "white" {}
        _WaveStrength ("Wave Strength", Float) = 0.03
        _WaveSpeed ("Wave Speed", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _WaveStrength;
            float _WaveSpeed;

            v2f vert (appdata_t v)
            {
                v2f o;
                float wave = sin(_Time.y * _WaveSpeed + v.vertex.x * 10) * _WaveStrength;
                v.vertex.y += wave;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // Ensure alpha is preserved for transparency
                col.a *= _Color.a;
                return col;
            }
            ENDCG
        }
    }
}