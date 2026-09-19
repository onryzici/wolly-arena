Shader "Woolly/SelectionRing" {
Properties {_Color("Color",Color)=(.78,.94,.85,.85)}
SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent"}
Pass {Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Offset -1,-1
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
half4 _Color;
CBUFFER_END
struct A {float4 p:POSITION;float2 uv:TEXCOORD0;};
struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{
float r=length((i.uv-.5)*2);
float aa=max(fwidth(r),.003);
float rim=smoothstep(.79-aa,.79+aa,r)*(1-smoothstep(.865-aa,.865+aa,r));
return half4(_Color.rgb,_Color.a*rim);
}

ENDHLSL
}}}
