Shader "PSX/VertexLit SHADOW PLAYER" {
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv: TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv: TEXCOORD0;
                half3 lightAmount : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Get the VertexNormalInputs of the vertex, which contains the normal in world space
                VertexNormalInputs positions = GetVertexNormalInputs(IN.positionOS);

                // Get the properties of the main light
                Light light = GetMainLight();

                // Calculate the amount of light the vertex receives
                OUT.lightAmount = LightingLambert(light.color, light.direction, positions.normalWS.xyz);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Set the fragment color to the interpolated amount of light
                return float4(IN.lightAmount, 1);
            }
            ENDHLSL
        }
    }
}
