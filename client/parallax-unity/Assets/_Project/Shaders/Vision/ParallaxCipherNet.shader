// Olhar do Cipher: desatura o mundo físico em 75%, realça a Pele de Rede em violeta (#B388FF),
// dispara o Tremor Vermelho da Diretriz Ômega com Contagem de Vazamento >= 3 e, no Override Cego,
// substitui o mundo pela moldura do terminal isolado.
Shader "Parallax/Vision/CipherNet"
{
    Properties
    {
        _GlitchNoise ("Glitch Noise", 2D) = "black" {}
        [Toggle] _UseGlitchTexture ("Use Glitch Texture", Float) = 1
        _OverrideFrame ("Override Terminal Frame", 2D) = "black" {}
        _NetColor ("Net Color", Color) = (0.7, 0.53, 1.0, 1)
        _OmegaErrorColor ("Omega Error Color", Color) = (1.0, 0.09, 0.27, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "CipherNet"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_GlitchNoise);
            SAMPLER(sampler_GlitchNoise);
            TEXTURE2D(_OverrideFrame);
            SAMPLER(sampler_OverrideFrame);

            CBUFFER_START(UnityPerMaterial)
                float _UseGlitchTexture;
                half4 _NetColor;
                half4 _OmegaErrorColor;
            CBUFFER_END

            float _Parallax_LeakCount;
            float _Parallax_OverrideBlind;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float topDown = 1.0 - uv.y;
                float time = _Time.y;

                float omegaActive = step(3.0, _Parallax_LeakCount);
                float severity = saturate((_Parallax_LeakCount - 2.0) / 3.0) * omegaActive;

                float blockGlitch = 0.0;
                float tearBand = 0.0;
                if (_UseGlitchTexture > 0.5 && omegaActive > 0.5)
                {
                    half3 gn = SAMPLE_TEXTURE2D(_GlitchNoise, sampler_GlitchNoise, float2(uv.x, frac(topDown + floor(time * 12.0) * 0.13))).rgb;
                    blockGlitch = step(0.78, gn.r) * severity;
                    tearBand = (gn.g - 0.5) * 0.018 * severity;
                }

                float glitchOffset = sin(time * 42.0 + topDown * 120.0) * 0.006 * omegaActive + tearBand;
                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(glitchOffset, 0.0)).r;
                half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(glitchOffset, 0.0)).b;
                half3 raw = half3(r, g, b);

                half luma = dot(raw, half3(0.299, 0.587, 0.114));
                half3 desaturated = lerp(luma.xxx, raw, 0.25);
                half3 netTinted = lerp(desaturated, _NetColor.rgb * luma * 1.38, 0.36);

                float scanline = step(0.92, frac(topDown * 180.0 + time * 4.0)) * omegaActive * 0.38;
                half3 color = lerp(netTinted, _OmegaErrorColor.rgb, saturate(min(scanline + blockGlitch * 0.45, 0.85)));

                // Override Cego: a interface de invasão é isolada do mundo físico.
                half3 terminal = SAMPLE_TEXTURE2D(_OverrideFrame, sampler_OverrideFrame, uv).rgb;
                terminal = lerp(terminal, _OmegaErrorColor.rgb, scanline);
                color = lerp(color, terminal, saturate(_Parallax_OverrideBlind));
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
