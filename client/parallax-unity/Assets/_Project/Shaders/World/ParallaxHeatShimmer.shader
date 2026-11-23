// Ondas de calor e vapor sulfúrico subindo pelas grelhas do Fosso de Hefesto. Quad aditivo, sem sombra.
Shader "Parallax/World/HeatShimmer"
{
    Properties
    {
        _HeatColor ("Heat Color", Color) = (1.0, 0.42, 0.12, 0.35)
        _RiseSpeed ("Rise Speed", Range(0.1, 3)) = 0.65
        _ShimmerFreq ("Shimmer Frequency", Range(2, 24)) = 10
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "HeatShimmer"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _HeatColor;
                float _RiseSpeed;
                float _ShimmerFreq;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;
                float wave1 = sin(uv.x * _ShimmerFreq + time * 2.2) * 0.5 + 0.5;
                float wave2 = sin((uv.y - time * _RiseSpeed) * 14.0 + uv.x * 6.0) * 0.5 + 0.5;
                float verticalFade = smoothstep(0.0, 0.25, uv.y) * smoothstep(1.0, 0.15, uv.y);
                float edgeFade = smoothstep(0.0, 0.15, uv.x) * smoothstep(1.0, 0.85, uv.x);
                float alpha = wave1 * wave2 * verticalFade * edgeFade * _HeatColor.a;
                return half4(_HeatColor.rgb * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
