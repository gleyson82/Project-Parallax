// Olhar do Goliath: calor e pressão por LUT térmico (38 K -> 140% Therma-Tek), ondas sísmicas da Batida
// no terço inferior e vinheta superior do capacete pesado.
// Uso: material do Full Screen Pass Renderer Feature do renderer Parallax_Renderer_Goliath.
Shader "Parallax/Vision/GoliathThermal"
{
    Properties
    {
        _ThermalLut ("Thermal LUT", 2D) = "white" {}
        [Toggle] _UseThermalLut ("Use Thermal LUT", Float) = 1
        _ThermalContrast ("Thermal Contrast", Range(0.5, 2.5)) = 1.45
        _HotColor ("Hot Color", Color) = (1.0, 0.34, 0.13, 1)
        _ColdColor ("Cold Color", Color) = (0.07, 0.11, 0.18, 1)
        _ThermalMix ("Thermal Mix", Range(0, 1)) = 0.52
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "GoliathThermal"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_ThermalLut);
            SAMPLER(sampler_ThermalLut);

            CBUFFER_START(UnityPerMaterial)
                float _UseThermalLut;
                float _ThermalContrast;
                half4 _HotColor;
                half4 _ColdColor;
                float _ThermalMix;
            CBUFFER_END

            float _Parallax_SeismicPulse;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                // Coordenadas com origem no topo, como no shader original.
                float2 g = float2(uv.x, 1.0 - uv.y);
                float time = _Time.y;

                float floorMask = smoothstep(0.52, 1.0, g.y);
                float distBottom = length(float2((g.x - 0.5) * 1.77, g.y - 1.0));
                float wave = sin(distBottom * 48.0 - time * 10.0);
                float ring = wave * 0.0035 * _Parallax_SeismicPulse * floorMask;
                half3 baseColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(ring, -ring * 0.5)).rgb;

                half luma = dot(baseColor, half3(0.299, 0.587, 0.114));
                half heat = pow(saturate(luma * _ThermalContrast), 1.25);
                half3 thermal = _UseThermalLut > 0.5
                    ? SAMPLE_TEXTURE2D(_ThermalLut, sampler_ThermalLut, float2(heat, 0.5)).rgb
                    : lerp(_ColdColor.rgb, _HotColor.rgb, heat);

                half3 color = lerp(baseColor, thermal, _ThermalMix);
                color += half3(1.0, 0.55, 0.12) * max(0.0, wave) * floorMask * _Parallax_SeismicPulse * 0.22;
                color *= lerp(0.32, 1.0, smoothstep(0.0, 0.28, g.y));
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
