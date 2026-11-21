Shader "WhackEnvironment/CosmicSky"
{
 SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
 Pass { HLSLPROGRAM
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 positionOS:POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct V { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
 V vert(A i) { V o; UNITY_SETUP_INSTANCE_ID(i); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.pos=TransformObjectToHClip(i.positionOS.xyz); o.dir=i.positionOS.xyz; return o; }
 float hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
 half4 frag(V i):SV_Target {
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float3 d=normalize(i.dir);
 float band=pow(saturate(1-abs(d.y*.9+d.x*.24-.12)),9);
 float mist= .55+.45*sin(d.x*8+d.z*6)*sin(d.y*7-d.z*4);
 float3 c=float3(.0015,.003,.009)+band*mist*float3(.004,.013,.035);
 float3 cell=floor(d*180); float h=hash(cell);
 float star=pow(saturate(1-length(frac(d*180)-.5)*2),7)*step(.986,h);
 c+=star*float3(.16,.28,.42);
 return half4(c,1); }
 ENDHLSL }
 }
}
