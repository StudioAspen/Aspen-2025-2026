Shader "Custom/ProximityRevealTransparent"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {} // The regular texture
        _RevealPosition ("Reveal Position", Vector) = (0, 0, 0, 0) // Position of the reveal field
        _RevealRadius ("Reveal Radius", Float) = 1.0 // Radius of the reveal effect
        _FadeWidth ("Fade Width", Float) = 0.5 // Width of the fade transition
        _ProximityEnabled ("Proximity Enabled", Float) = 1.0 // Toggle proximity effect (0 = off, 1 = on)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Enable transparency
            ZWrite Off // Disable depth writing for transparency
            Cull Off // Render both sides of the object
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST; // Tiling and offset for the base map
            float4 _RevealPosition;
            float _RevealRadius;
            float _FadeWidth;
            float _ProximityEnabled;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap); // Apply tiling and offset
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the base texture with tiling and offset
                fixed4 baseColor = tex2D(_BaseMap, i.uv);

                // Check if proximity transparency is enabled
                if (_ProximityEnabled > 0.5)
                {
                    // Calculate the distance from the fragment to the reveal position
                    float dist = distance(i.worldPos, _RevealPosition.xyz);

                    // Calculate the alpha value based on the distance
                    float alpha = saturate(1.0 - (dist - _RevealRadius) / _FadeWidth);

                    // Ensure the object is fully invisible outside the reveal radius
                    if (dist > _RevealRadius + _FadeWidth)
                    {
                        alpha = 0.0;
                    }

                    // Apply the calculated alpha
                    baseColor.a *= alpha;
                }
                else
                {
                    // If proximity transparency is disabled, make the object fully visible
                    baseColor.a = 1.0;
                }

                return baseColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}