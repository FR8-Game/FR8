Shader "Hidden/Volumetrics"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"
        }
        Blend One One
        ZWrite Off
        Cull Front

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/surfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : VAR_WORLDPOS;
                float3 lightPos : VAR_LIGHTPOS;
                float3 spotDirection : VAR_SPOT_DIRECTION;
            };

            static const float3 vertices[] =
            {
                float3(-1.0, -1.0, 0.0),
                float3(1.0, -1.0, 0.0),
                float3(1.0, 1.0, 0.0),

                float3(1.0, 1.0, 0.0),
                float3(-1.0, 1.0, 0.0),
                float3(-1.0, -1.0, 0.0),
            };

            float4 _LightData;
            int _Resolution;

            Varyings vert(uint id : SV_VertexID)
            {
                Varyings output;

                int vertexID = id % 6;
                int planeID = id / 6;

                output.lightPos = TransformObjectToWorld(0.0);
                
                float d = (planeID / (float)_Resolution * -2.0 + 1.0) * _LightData.x;
                float3 planeOffset = normalize(_WorldSpaceCameraPos - output.lightPos) * d;
                float4 vertexOffset = float4(vertices[vertexID] * _LightData.x, 0.0);
                
                output.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(output.lightPos + planeOffset, 1.0)) + vertexOffset * 1.414);
                output.worldPos = mul(UNITY_MATRIX_I_VP, output.vertex);
                output.spotDirection = TransformObjectToWorldDir(float3(0, 0, 1));

                return output;
            }

            float3 _LightColor;
            float _Density;

            float4 frag(Varyings input) : SV_Target
            {
                float3 vec = input.worldPos - input.lightPos;
                float dist2 = dot(vec, vec);

                float attenuation = DistanceAttenuation(dist2, 1.0 / (_LightData.x * _LightData.x)) * AngleAttenuation(input.spotDirection, normalize(vec), _LightData.yz);
                return float4(_LightColor * attenuation / _Resolution * 5.0 * _Density, 1.0);
            }
            ENDHLSL
        }
    }
}