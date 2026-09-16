Shader "Woolly/LobbyBackdrop"
{
    Properties
    {
        _Backdrop("Panoramic canyon", 2D) = "white" {}
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
            CBUFFER_START(UnityPerMaterial)
                float4 _Backdrop_TexelSize;
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
