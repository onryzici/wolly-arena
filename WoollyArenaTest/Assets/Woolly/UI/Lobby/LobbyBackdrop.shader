Shader "Woolly/LobbyBackdrop"
{
    Properties
    {
        _Backdrop("Panoramic canyon", 2D) = "white" {}
        _SelectionMode("Character selection", Float) = 0
        _SelectionBackdrop("Cartoon character camp", 2D) = "white" {}
        _AmbientTime("Lobby wind clock", Float) = 0
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
            TEXTURE2D(_SelectionBackdrop); SAMPLER(sampler_SelectionBackdrop);
            TEXTURE2D(_SelectionPattern); SAMPLER(sampler_SelectionPattern);
            CBUFFER_START(UnityPerMaterial)
                float4 _Backdrop_TexelSize;
                float4 _SelectionBackdrop_TexelSize;
                float _SelectionMode;
                float _AmbientTime;
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
            half3 desertWind(half3 color, float2 uv)
            {
                // Thin ground haze: bounded opacity, entirely behind the hero and interface.
                float ground=smoothstep(.12,.19,uv.y)*(1-smoothstep(.35,.42,uv.y));
                float gust=pow(saturate(.5+.5*sin(uv.x*14-uv.y*35-_AmbientTime*.45)),6);
                float streak=pow(saturate(.5+.5*sin(uv.y*240+sin(uv.x*5-_AmbientTime*.24)*2)),12);
                color=lerp(color,half3(1,.83,.55),ground*gust*streak*.085);
                return color;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float2 screen = input.screen.xy / input.screen.w;
                float viewportAspect = _ScreenParams.x / _ScreenParams.y;
                if (_SelectionMode > .5)
                {
                    // A separate still cartoon camp, uniformly cropped to fill each screen.
                    float selectionAspect = _SelectionBackdrop_TexelSize.z / _SelectionBackdrop_TexelSize.w;
                    float2 selectionFit = float2(min(1, viewportAspect / selectionAspect), min(1, selectionAspect / viewportAspect));
                    float2 selectionUV = (screen - .5) * selectionFit + .5;
                    half3 camp = SAMPLE_TEXTURE2D(_SelectionBackdrop, sampler_SelectionBackdrop, selectionUV).rgb;
                    float selectionEdge = smoothstep(.18, .72, length((screen - .5) * float2(1, .75)));
                    return half4(camp * (1 - selectionEdge * .12), 1);
                }
                float imageAspect = _Backdrop_TexelSize.z / _Backdrop_TexelSize.w;
                // Uniform cover scaling: the panorama is never stretched on tablets or ultrawide screens.
                float2 fit = float2(min(1, viewportAspect / imageAspect), min(1, imageAspect / viewportAspect));
                float2 uv = (screen - .5) * fit + .5;
                // Follow the panorama's skyline: lower in the open centre, above the cliffs at the sides.
                // The old narrow mask missed most clouds and moved less than a pixel per second.
                float skyline=lerp(.54,.70,smoothstep(.18,.43,abs(uv.x-.5)));
                float sky=smoothstep(skyline,skyline+.07,uv.y);
                float2 movingUV=uv;
                movingUV.x+=sin(_AmbientTime*.18)*.032*sky;
                movingUV.x=clamp(movingUV.x,.001,.999);
                half3 color = SAMPLE_TEXTURE2D(_Backdrop, sampler_Backdrop, movingUV).rgb;
                color=desertWind(color,uv);
                float edge = smoothstep(.18, .72, length((screen - .5) * float2(1, .75)));
                return half4(color * (1 - edge * .16), 1);
            }
            ENDHLSL
        }
    }
}
