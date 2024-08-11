Shader "Custom/VertexLightingShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            // Declare light properties
            uniform float4 _LightColor0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Transform the normal to world space
                float3 worldNormal = normalize(mul(v.normal, (float3x3)unity_ObjectToWorld));

                // Transform the light direction to object space
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float NdotL = max(0, dot(worldNormal, lightDir));
                float3 lightColor = _LightColor0.rgb;

                // Apply ambient light
                float3 ambient = unity_AmbientSky.rgb * _Color.rgb;
                float3 diffuse = lightColor * _Color.rgb * NdotL;

                o.color = float4(diffuse + ambient, _Color.a);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * i.color;
            }
            ENDCG
        }
    }
}
