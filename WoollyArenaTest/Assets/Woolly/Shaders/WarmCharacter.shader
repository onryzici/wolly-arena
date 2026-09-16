Shader "Woolly/WarmCharacter" {
Properties { _BaseColor("Tint",Color)=(1,1,1,1) _BaseMap("Character texture",2D)="white" {} }
SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
Pass {Name "Forward" Tags {"LightMode"="UniversalForward"}
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;float4 _BaseMap_ST;
CBUFFER_END
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float2 uv:TEXCOORD2;};
V vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(i.n);o.uv=TRANSFORM_TEX(i.uv,_BaseMap);return o;}
half4 frag(V i):SV_Target {
 half3 c=LinearToSRGB(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb);
 half mx=max(c.r,max(c.g,c.b)),mn=min(c.r,min(c.g,c.b));
 half cream=smoothstep(.5,.85,mn)*(1-smoothstep(.08,.24,mx-mn));
 half gold=smoothstep(.12,.35,c.r-c.b)*smoothstep(.12,.3,c.g-c.b);
 c*=lerp(half3(1,1,1),half3(1,.95,.84),cream);
 c*=lerp(half3(1,1,1),half3(1,.86,.72),gold);
 // Lift dark leather slightly into warm charcoal without losing silhouette contrast.
 half dark=1-smoothstep(.12,.32,mx);c=lerp(c,c*half3(1.12,1.04,.96)+half3(.025,.018,.012),dark*.65);
 Light l=GetMainLight(TransformWorldToShadowCoord(i.world));half diffuse=.8+.2*smoothstep(-.2,.8,dot(normalize(i.normal),l.direction));
 return half4(SRGBToLinear(saturate(c))*_BaseColor.rgb*diffuse*lerp(half3(.76,.65,.58),half3(1,1,1),l.shadowAttenuation),1);
}
ENDHLSL
}
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
UsePass "Universal Render Pipeline/Lit/DepthOnly"
}}
