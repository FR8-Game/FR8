Shader "Hidden/Mirror"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 screenPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4x4 _MirrorVP;
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                output.vertex = TransformObjectToHClip(input.vertex.xyz);
                output.screenPos = ComputeScreenPos(mul(_MirrorVP, mul(GetObjectToWorldMatrix(), float4(input.vertex.xyz, 1.0))));
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                screenUV.y = 1.0 - screenUV.y;

                //return half4(screenUV.xy, 0.0, 1.0);
                
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, screenUV);
                return col;
            }
            ENDHLSL
        }
    }
}
