Shader "Hidden/Volumetrics"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 worldPos : VAR_WORLDPOS;
            };

            static const float4 vertices[] =
            {
                float4(-1.0, -1.0, 0.0, 1.0),
                float4(3.0, -1.0, 0.0, 1.0),
                float4(-1.0, 3.0, 0.0, 1.0),
            };

            int _Volumetrics_Resolution = 1000;
            float _Volumetrics_Percent = 0.2;
            float _Volumetrics_Density = 0.2;

            Varyings vert(uint id : SV_VertexID)
            {
                Varyings o;

                int planeID = id / 3;
                int vertexID = id % 3;

                float3 cameraPos = vertices[vertexID];
                float p = (1.0f - (planeID + 1.0) / _Volumetrics_Resolution) * _Volumetrics_Percent;
                float far = _ProjectionParams.z;
                float near = _ProjectionParams.y;
                cameraPos.z -= p * (far - near) + near;

                o.worldPos.xyz = mul(UNITY_MATRIX_I_V, float4(cameraPos, 1.0));

                o.vertex = TransformWorldToHClip(o.worldPos.xyz);
                o.vertex.xy = cameraPos.xy * o.vertex.w;

                o.worldPos = mul(UNITY_MATRIX_I_VP, o.vertex);
                
                return o;
            }

            float3 _Color;
            float _Value;

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half3 col = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
                    Light light = GetAdditionalLight(lightIndex, input.worldPos);
                    col.rgb += light.color * light.distanceAttenuation * light.shadowAttenuation;
                LIGHT_LOOP_END

                return half4(col.rgb, _Volumetrics_Density / _Volumetrics_Resolution);
            }
            ENDHLSL
        }
    }
}