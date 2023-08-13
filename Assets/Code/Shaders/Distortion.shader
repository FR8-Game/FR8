Shader "Unlit/Distortion"
{
    Properties
    {
        _Slope("Slope", float) = 1.1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float3 positionWS : VAR_POSITION_WS;
                float4 centerCS : VAR_CENTER_CS;
                float4 positionCS : VAR_POSITION_CS;
                float3 normal : VAR_NORMAL;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = TransformObjectToHClip(input.vertex.xyz);
                output.positionCS = ComputeScreenPos(output.vertex);
                output.centerCS = ComputeScreenPos(TransformObjectToHClip(0.0));
                output.normal = TransformObjectToWorldNormal(input.normal.xyz);
                output.positionWS = TransformObjectToWorld(input.vertex.xyz);
                return output;
            }

            float2 slope(float2 val, float exp)
            {
                return pow(abs(val), exp) * sign(val);
            }

            float _Slope;

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = 0.0;
                half2 uv = input.positionCS.xy / input.positionCS.w;
                half2 center = input.centerCS.xy / input.centerCS.w;

                float3 normal = normalize(input.normal.xyz);
                float ndv = 1 - dot(normal, normalize(_WorldSpaceCameraPos - input.positionWS));
                ndv = 1.0 + ndv * ndv * _Slope;

                uv = (uv - center) * ndv + center;

                col.rgb = SampleSceneColor(uv);
                col.a = 1.0;

                return col;
            }
            ENDHLSL
        }
    }
}