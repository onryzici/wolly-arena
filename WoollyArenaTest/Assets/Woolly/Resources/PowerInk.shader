Shader "Woolly/PowerInk" {
Properties { _Tint("Tint", Color) = (1,1,1,1) }
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A { float4 p:POSITION; half4 c:COLOR; };
struct V { float4 p:SV_POSITION; half4 c:COLOR; };
CBUFFER_START(UnityPerMaterial)
half4 _Tint;
CBUFFER_END
V vert(A i) { V o; o.p=TransformObjectToHClip(i.p.xyz); o.c=i.c*_Tint; return o; }
half4 frag(V i):SV_Target { return i.c; }
ENDHLSL
} } }
