// Geada nas bordas do Quadro de Válvulas: quanto maior _Parallax_FrostProgress (0–1), mais o frio de
// -235 °C avança das bordas para o centro. Recua com qualquer input.
Shader "Parallax/Vision/FrostBorder"
{
    Properties
    {
        _FrostColor ("Frost Color", Color) = (0.65, 0.92, 1.0, 0.85)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "FrostBorder"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FrostColor;
            CBUFFER_END

            float _Parallax_FrostProgress;

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half3 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                float distEdge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float crystalNoise = Hash(floor(uv * 160.0)) * 0.08;
                float threshold = _Parallax_FrostProgress * 0.35 + crystalNoise;
                float mask = smoothstep(threshold, threshold - 0.06, distEdge);
                float alpha = mask * _FrostColor.a * _Parallax_FrostProgress;
                return half4(lerp(scene, _FrostColor.rgb, alpha), 1.0);
            }
            ENDHLSL
        }
    }
}
