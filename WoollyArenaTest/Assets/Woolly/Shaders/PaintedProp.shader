Shader "Woolly/PaintedProp" {
Properties {_BaseMap("Prop sheet",2D)="white" {} _Hit("Hit",Float)=0}
SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
Pass {Cull Off ZWrite On
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
float _Hit;
CBUFFER_END
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{half3 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;half key=min(c.r,c.b)-c.g;clip(.22-key);return half4(lerp(c,half3(1,.85,.55),_Hit),1);}
ENDHLSL
}}}
