Shader "WhackEnvironment/OuterShockwave"
{
 Properties { [HDR] _Color("Neon Color",Color)=(0.12,1.6,4,1) _Progress("Progress",Range(0,1))=0 _Seed("Seed",Float)=0 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha One ZWrite Off Cull Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Color; float _Progress; float _Seed;
 CBUFFER_END
 struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct V { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
 V vert(A i) {
 V o; UNITY_SETUP_INSTANCE_ID(i); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 float angle=i.uv.y*6.2831853;
 float jag=.12*sin(angle*9+_Seed+_Progress*3)+.045*sin(angle*23-_Seed)+.025*sin(angle*41+_Seed);
 float radius=4.65+jag+(i.uv.x-.5)*.85;
 float3 p=float3(sin(angle)*radius,.06*sin(angle*9+_Seed)*sin(_Progress*3.14159),cos(angle)*radius);
 o.pos=TransformObjectToHClip(p); o.uv=i.uv; return o; }
 half4 frag(V i):SV_Target {
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float u=i.uv.x;
 float center=.59+.025*sin(i.uv.y*150+_Seed+_Progress*12);
 float d=abs(u-center);
 float core=1-smoothstep(.012,.035,d);
 float halo=exp2(-d*d*65);
 float echo=(1-smoothstep(.012,.04,abs(u-.24)))*.45;
 float flecks=pow(saturate(sin(i.uv.y*290+_Seed)),18)*pow(saturate(1-abs(u-.82)*5),3);
 float edge=smoothstep(0,.08,u)*(1-smoothstep(.92,1,u));
 half3 col=_Color.rgb*(halo*.65+echo+flecks*1.3)+half3(.65,1.5,2)*core*2;
 return half4(col,_Color.a*edge); }
 ENDHLSL }
 }
}