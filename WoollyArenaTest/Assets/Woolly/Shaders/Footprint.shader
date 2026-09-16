Shader "Woolly/Footprint" {
Properties {_Color("Color",Color)=(.37,.19,.105,.24)}
SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent"}
Pass {Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Offset -1,-1
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
half4 _Color;
CBUFFER_END
float4 vert(float4 p:POSITION):SV_POSITION{return TransformObjectToHClip(p.xyz);}
half4 frag():SV_Target{return _Color;}
ENDHLSL
}}
}
