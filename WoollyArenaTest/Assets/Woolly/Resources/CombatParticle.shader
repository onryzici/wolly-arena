Shader "Woolly/CombatParticle" {
 Properties { _BaseMap("Particle",2D)="white" {} _Tint("Tint",Color)=(1,1,1,1) }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint;
 CBUFFER_END
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color*_Tint;return o;}
 half4 frag(V i):SV_Target{return SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*i.color;}
 ENDHLSL
 } }
}
