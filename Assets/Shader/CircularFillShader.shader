Shader "Custom/RadialFillShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FillColor;
            float _FillAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert UV to polar coordinates
                float2 centeredUV = i.uv - float2(0.5, 0.5);
                float angle = atan2(centeredUV.y, centeredUV.x) + 3.14159; // Adjust starting angle here
                float radius = length(centeredUV) * 2.0; // Normalize radius

                // Normalize angle (0 to 1) and compare with _FillAmount
                float normalizedAngle = angle / (2.0 * 3.14159);
                if(normalizedAngle > _FillAmount || radius > 1.0) // Check if angle is outside fill amount
                    discard; // Discard pixel

                // Sample the texture color
                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply fill color
                return col * _FillColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
