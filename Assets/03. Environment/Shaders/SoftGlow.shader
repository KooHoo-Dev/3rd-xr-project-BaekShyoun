Shader "WhackEnvironment/SoftGlow"
{
 Properties { _Color("Color",Color)=(0.02,0.45,1,1) _Round("Round Particle",Float)=0 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha One ZWrite Off Cull Off
 Pass { HLSLPROGRAM
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Color; float _Round;
 CBUFFER_END
 struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct V { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
 V vert(A i){ V o; UNITY_SETUP_INSTANCE_ID(i); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.pos=TransformObjectToHClip(i.positionOS.xyz); o.uv=i.uv; o.color=i.color; return o; }
 half4 frag(V i):SV_Target { UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); float d=lerp(abs(i.uv.x-.5)*2,length(i.uv-.5)*2,_Round); float g=pow(saturate(1-d),3); return half4(_Color.rgb*i.color.rgb,g*_Color.a*i.color.a); }
 ENDHLSL }
 }
}
