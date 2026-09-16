Shader "Woolly/ContactShadow" {SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-30" "RenderType"="Transparent"}Pass{Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Offset -1,-1
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}half4 frag(V i):SV_Target{float r=length((i.uv-.5)*2);return half4(.24,.12,.08,.30*(1-smoothstep(.65,1,r)));}
ENDHLSL
}}}
