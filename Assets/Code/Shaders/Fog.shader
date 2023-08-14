Shader "Hidden/Fog"
{
    Properties
    {
        _Value ("Fog Density", float) = 1.0
        _Color ("Fog Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _FarPlane ("Far Plane", float) = 0.9
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"
        }
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            struct Attributes
            {
                uint id : SV_VertexID;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 direction : VAR_DIRECTION;
            };

            static const float4 vertices[] =
            {
                float4(-1.0, -1.0, 0.0, 1.0),
                float4(-1.0, 3.0, 0.0, 1.0),
                float4(3.0, -1.0, 0.0, 1.0),
            };

            static const float2 uv[] =
            {
                float2(0.0, 0.0),
                float2(0.0, 1.0),
                float2(1.0, 1.0),
            };

            Varyings vert(Attributes input)
            {
                Varyings o;

                o.vertex = vertices[input.id];
                o.uv = (o.vertex.xy + 1) / 2;
                o.uv.y = 1 - o.uv.y;
                o.direction = mul(UNITY_MATRIX_I_VP, o.vertex);
                return o;
            }

            float3 _Color;
            float _Value;

            TEXTURECUBE(_Noise);
            SAMPLER(sampler_Noise);

            float4 frag(Varyings input) : SV_Target
            {
                float depth = SampleSceneDepth(input.uv);
                float3 pos = ComputeWorldSpacePosition(input.uv, depth, UNITY_MATRIX_I_VP);
                float dist = length(pos - _WorldSpaceCameraPos) / 100.0f;

                float fog = pow(2.71, -(dist * dist) * _Value);
                fog = clamp(1.0f - fog, 0.0, 1.0);
                clip(fog);

                return float4(pow(_Color, 1), fog);
            }
            ENDHLSL
        }
    }
}