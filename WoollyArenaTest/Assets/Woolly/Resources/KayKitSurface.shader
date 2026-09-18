Shader "Woolly/KayKitSurface" {
 Properties { [MainTexture] _BaseMap("Authored palette",2D)="white" {} [MainColor] _BaseColor("Tint",Color)=(1,1,1,1) _HitFlash("Hit",Range(0,1))=0 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass { Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;half _HitFlash;
 CBUFFER_END
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 struct A {float4 position:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
 struct V {float4 position:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;};
 V vert(A i){V o;o.position=TransformObjectToHClip(i.position.xyz);o.normal=TransformObjectToWorldNormal(i.normal);o.uv=i.uv;return o;}
 half4 frag(V i):SV_Target{half3 base=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*_BaseColor.rgb;Light light=GetMainLight();half shade=.72h+.28h*smoothstep(-.1h,.5h,dot(normalize(i.normal),light.direction));return half4(lerp(base*shade,half3(1,.8,.4),saturate(_HitFlash)*.35h),1);}
 ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
