// Olhar do Phantom: bordas Sobel em ciano-gelo (#00E5FF) para pontos altos, cabos e números no teto,
// com refração prismática de cristal de N2 nas bordas.
Shader "Parallax/Vision/PhantomOptic"
{
    Properties
    {
        _EdgeColor ("Optic Edge Color", Color) = (0.0, 0.9, 1.0, 1)
        _EdgeStrength ("Edge Strength", Range(0, 3)) = 1.65
        _PrismRefraction ("Prism Refraction", Range(0, 0.01)) = 0.0022
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "PhantomOptic"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _EdgeColor;
                float _EdgeStrength;
                float _PrismRefraction;
            CBUFFER_END

            half3 SampleScene(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 px = 1.0 / _ScreenParams.xy;
                float topDown = 1.0 - uv.y;

                float edgeDist = length(uv - 0.5);
                float2 prismShift = normalize(uv - 0.5 + 0.0001) * _PrismRefraction * smoothstep(0.28, 0.72, edgeDist);
                half3 color = half3(
                    SampleScene(uv + prismShift).r,
                    SampleScene(uv).g,
                    SampleScene(uv - prismShift).b);

                half3 n = SampleScene(uv + float2(0.0, px.y));
                half3 s = SampleScene(uv - float2(0.0, px.y));
                half3 e = SampleScene(uv + float2(px.x, 0.0));
                half3 w = SampleScene(uv - float2(px.x, 0.0));
                half edge = length((n - s) + (e - w)) * _EdgeStrength;

                // Foco ótico mais nítido na metade superior: teto, vigas e números das válvulas.
                half upperBoost = lerp(1.45, 0.72, topDown);
                half3 result = lerp(color * 0.84, _EdgeColor.rgb, saturate(edge * upperBoost));
                return half4(result, 1.0);
            }
            ENDHLSL
        }
    }
}
