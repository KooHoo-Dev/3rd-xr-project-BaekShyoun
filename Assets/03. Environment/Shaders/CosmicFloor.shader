Shader "WhackEnvironment/CosmicFloor"
{
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 positionOS:POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct V { float4 pos:SV_POSITION; float3 world:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
 V vert(A i){ V o; UNITY_SETUP_INSTANCE_ID(i); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.pos=TransformObjectToHClip(i.positionOS.xyz); o.world=TransformObjectToWorld(i.positionOS.xyz); return o; }
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p), f=frac(p); f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 half4 frag(V i):SV_Target {
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 p=i.world.xz-float2(0,1.5);
 float n=noise(p*.75)*.6+noise(p*1.8+9)*.3+noise(p*4.2+31)*.1;
 float band=exp2(-pow(p.y*.42+p.x*.3+sin(p.x*.8)*.6,2)*1.8);
 float cloud=smoothstep(.25,.85,n)*band;
 half3 c=half3(.0015,.003,.009)+cloud*half3(.005,.014,.032);
 float2 cell=p*16;float h=hash(floor(cell));
 float d=length(frac(cell)-.5);
 float stars=(1-smoothstep(.015,.12,d))*step(.985,h);
 c+=stars*half3(.06,.12,.2);
 // 옆면은 어둡게 해서 발판의 윤곽을 유지.
 c*=lerp(.24,1,step(-.02,i.world.y));
 return half4(c,1); }
 ENDHLSL }
 }
}