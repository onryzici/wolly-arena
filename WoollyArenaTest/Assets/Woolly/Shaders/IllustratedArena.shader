Shader "Woolly/IllustratedArena" {
Properties { _BaseColor("Color",Color)=(1,1,1,1) _BaseMap("Painted ground",2D)="white" {} _WorldScale("World height",Float)=24 _WorldAspect("Map aspect",Float)=1.5 }
SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
Pass { Name "Forward" Tags { "LightMode"="UniversalForward" }
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;float _WorldScale;float _WorldAspect;
CBUFFER_END
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;};
V vert(A a){V o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(a.normalOS);return o;}
float tuft(float2 uv,float2 c){float2 d=abs(uv-c);return (1-smoothstep(.055,.08,d.x))*(1-smoothstep(.008,.022,d.y));}
half4 frag(V i):SV_Target {Light l=GetMainLight(TransformWorldToShadowCoord(i.positionWS));half shade=.84h+.16h*saturate(dot(normalize(i.normalWS),l.direction));half3 shadowTint=half3(.72,.52,.46);float2 uv=i.positionWS.xz/_WorldScale+0.5;float mask=tuft(uv,float2(.326,.78))+tuft(uv,float2(.647,.78))+tuft(uv,float2(.253,.562))+tuft(uv,float2(.716,.562))+tuft(uv,float2(.283,.323))+tuft(uv,float2(.666,.323));uv.x+=sin(_Time.y*1.8+uv.x*33)*.0012*saturate(mask);uv.x=(uv.x-.5)/_WorldAspect+.5;half3 painted=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;half3 c=painted*_BaseColor.rgb*lerp(shadowTint,half3(1,1,1),l.shadowAttenuation);return half4(c,1);}
ENDHLSL
}
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
UsePass "Universal Render Pipeline/Lit/DepthOnly"
}
}
