Shader "Woolly/SelectionRing" {
Properties {_Color("Color",Color)=(.2,1,.25,.75)}
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
float2 p=(i.uv-.5)*2;float r=length(p);float angle=atan2(p.y,p.x);float phase=_Time.y*.5;float pulse=.5+.5*sin(_Time.y*2.2);
float disc=1-smoothstep(.735,.77,r);float rim=smoothstep(.64,.70,r)*disc;
float arcs=smoothstep(-.70,-.58,cos(angle*3-phase))*smoothstep(.83,.855,r)*(1-smoothstep(.97,.995,r));
float2 orbitPoint=float2(cos(phase),sin(phase))*.91;float orbitDot=1-smoothstep(.047,.065,length(p-orbitPoint));
float3 green=lerp(float3(.14,.57,.15),float3(.30,1,.10),rim);
float alpha=disc*(.26+rim*.48+.025*pulse);float3 c=green;
c=lerp(c,float3(.12,.77,1),arcs);alpha=max(alpha,arcs*.86);c=lerp(c,float3(.92,1,1),orbitDot);alpha=max(alpha,orbitDot);return half4(c,alpha);
}

ENDHLSL
}}}
