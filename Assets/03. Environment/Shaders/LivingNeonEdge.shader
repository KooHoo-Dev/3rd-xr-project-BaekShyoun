Shader "WhackEnvironment/LivingNeonEdge"
{
 Properties { [HDR] _Color("Neon Color",Color)=(.08,.8,2,1) _Speed("Flow Speed",Float)=.12 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha One ZWrite Off Cull Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Color; float _Speed;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){ V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 float a=i.uv.y*6.2831853;float r=3.96+(i.uv.x-.5)*.5;
 o.pos=TransformObjectToHClip(float3(sin(a)*r,0,cos(a)*r));o.uv=i.uv;return o;}
 half4 frag(V i):SV_Target {
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float d=abs(i.uv.x-.5);
 float core=1-smoothstep(.012,.045,d);
 float glow=exp2(-d*d*42);
 float flow=pow(saturate(.5+.5*cos((i.uv.y*3-_Time.y*_Speed)*6.2831853)),10);
 float pulse=.85+.15*sin(_Time.y*1.8);
 float echo=(1-smoothstep(.015,.04,abs(i.uv.x-.22)))*.3;
 float sparks=pow(saturate(sin(i.uv.y*427-_Time.y*2)),30)*exp2(-pow((i.uv.x-.78)*15,2))*.3;
 half3 c=_Color.rgb*(glow*(.28+flow*.65)+echo+sparks)+half3(.2,.85,1.2)*core*(.7+flow*1.5);
 return half4(c*pulse,_Color.a*(1-smoothstep(.42,.5,d))); }
 ENDHLSL }
 }
}