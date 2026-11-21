Shader "WhackEnvironment/HeadHitNeon"
{
    Properties { _Tint("Tint",Color)=(0.2,1.6,2.2,1) _Ring("Ring",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            float _Ring;
            CBUFFER_END
            Varyings vert(Attributes v) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.uv=v.uv;o.color=v.color;return o;
            }
            half4 frag(Varyings i):SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float radius=length(i.uv-.5);
                float d=lerp(radius,abs(radius-.34),_Ring);
                float core=1-smoothstep(.015,.055,d);
                float glow=(1-smoothstep(.025,.15,d))*.3;
                return half4(_Tint.rgb*i.color.rgb,i.color.a*_Tint.a*saturate(core+glow));
            }
            ENDHLSL
        }
    }
}
