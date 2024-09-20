Shader "Custom/SilhouetteWithLayerExclusion" {
    Properties {
        _Color("Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _PlayerLayerMask("Player", Int) = 6
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        // First pass: Render objects and write to the stencil buffer
        Pass {
            Name "StencilWrite"
            ZTest Greater
            ZWrite Off
            Cull Front
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _OutlineColor;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }

        // Second pass: Render only objects where stencil equals 1 (exclude player)
        Pass {
            Name "StencilCheck"
            ZTest LEqual
            Stencil {
                Ref 1
                Comp Equal
                Pass Keep
            }
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

    // Optional third pass for the player layer exclusion
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        // Define player layer stencil reference
        Pass {
            Name "PlayerLayerStencil"
            Stencil {
                Ref 0 // Using reference 0 for player layer, indicating no silhouette
                Comp Equal
                Pass Replace
            }
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
