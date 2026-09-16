Shader "Woolly/ArenaToon" {
Properties { _BaseColor("Color",Color)=(1,1,1,1) }
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
half4 _BaseColor;
CBUFFER_END
struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;};
V vert(A a){V o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(a.normalOS);return o;}
half4 frag(V i):SV_Target {Light l=GetMainLight(TransformWorldToShadowCoord(i.positionWS));half shade=.84h+.16h*saturate(dot(normalize(i.normalWS),l.direction));half3 shadowTint=half3(.72,.52,.46);half3 c=_BaseColor.rgb*shade*lerp(shadowTint,half3(1,1,1),l.shadowAttenuation);return half4(c,1);}
ENDHLSL
}
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
UsePass "Universal Render Pipeline/Lit/DepthOnly"
}
}
