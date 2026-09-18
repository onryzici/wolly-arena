Shader "Woolly/PlayableOutline" {
Properties {_OutlineColor("Outline Color",Color)=(.1,.06,.1,1) _Width("Width",Float)=.02}
SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry-1"}
Pass {Name "Outline" Tags {"LightMode"="SRPDefaultUnlit"} Cull Front ZWrite On
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
half4 _OutlineColor;float _Width;
CBUFFER_END
struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;};
float4 vert(A a):SV_POSITION {float3 p=TransformObjectToWorld(a.positionOS.xyz);float3 n=normalize(TransformObjectToWorldNormal(a.normalOS));return TransformWorldToHClip(p+n*_Width);}
half4 frag():SV_Target{return _OutlineColor;}
ENDHLSL
}}
}
