Shader "Woolly/LobbyBackdrop" {SubShader {Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass {Cull Off ZWrite On HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};V vert(A a){V o;o.pos=TransformObjectToHClip(a.positionOS.xyz);o.uv=a.uv;return o;}
half4 frag(V i):SV_Target{float2 p=(i.uv-.5)*float2(2,1);float glow=saturate(1-length(p)*1.7);float3 col=lerp(float3(.53,.16,.065),float3(1,.68,.25),glow);float2 cell=frac(i.uv*float2(22,11))-.5;float ring=1-smoothstep(.022,.036,abs(length(cell)-.26));col+=ring*.022;return half4(col,1);}
ENDHLSL}}}
