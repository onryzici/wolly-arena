Shader "Woolly/LobbyBackdrop"
{
    Properties
    {
        _Backdrop("Panoramic canyon", 2D) = "white" {}
        _SelectionMode("Character selection", Float) = 0
        _SelectionPattern("Collection pattern", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Cull Off ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_Backdrop); SAMPLER(sampler_Backdrop);
            TEXTURE2D(_SelectionPattern); SAMPLER(sampler_SelectionPattern);
            CBUFFER_START(UnityPerMaterial)
                float4 _Backdrop_TexelSize;
                float _SelectionMode;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 screen : TEXCOORD0; };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screen = ComputeScreenPos(output.positionCS);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float2 screen = input.screen.xy / input.screen.w;
                float viewportAspect = _ScreenParams.x / _ScreenParams.y;
                if (_SelectionMode > .5)
                {
                    half3 baseColor=lerp(half3(.34,.18,.66),half3(.24,.31,.70),screen.y);
                    half4 pattern=SAMPLE_TEXTURE2D(_SelectionPattern,sampler_SelectionPattern,frac(screen*float2(viewportAspect*3,3)));
                    return half4(baseColor+pattern.rgb*pattern.a*.055,1);
                }
                float imageAspect = _Backdrop_TexelSize.z / _Backdrop_TexelSize.w;
                // Uniform cover scaling: the panorama is never stretched on tablets or ultrawide screens.
                float2 fit = float2(min(1, viewportAspect / imageAspect), min(1, imageAspect / viewportAspect));
                float2 uv = (screen - .5) * fit + .5;
                half3 color = SAMPLE_TEXTURE2D(_Backdrop, sampler_Backdrop, uv).rgb;
                float edge = smoothstep(.18, .72, length((screen - .5) * float2(1, .75)));
                return half4(color * (1 - edge * .16), 1);
            }
            ENDHLSL
        }
    }
}
