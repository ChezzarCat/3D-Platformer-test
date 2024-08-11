Shader "Unlit/PixelateShader"
{
     Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _PixelationFactor ("Pixelation Factor", Float) = 100
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _PixelationFactor;

            half4 frag (v2f_img i) : SV_Target
            {
                // Calculate pixelation
                float2 pixelatedUV = floor(i.uv * _PixelationFactor) / _PixelationFactor;

                // Sample the texture at the pixelated UV coordinates
                half4 col = tex2D(_MainTex, pixelatedUV);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
