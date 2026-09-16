Shader "Woolly/CartoonSpark" {
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
Pass{Blend SrcAlpha One ZWrite Off Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;half4 c:COLOR;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 c:COLOR;};
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.c=i.c;return o;}
half4 frag(V i):SV_Target{float2 p=i.uv*2-1;float r=length(p);float edge=.45+.45*pow(abs(cos(atan2(p.y,p.x)*3)),5);float a=1-smoothstep(edge-.09,edge,r);return half4(lerp(i.c.rgb,half3(1,1,.8),1-smoothstep(0,.35,r)),a*i.c.a);}
ENDHLSL
}}}
