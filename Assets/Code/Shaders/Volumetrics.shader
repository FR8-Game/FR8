Shader "Hidden/Volumetrics"
{
    Properties
    {

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"
        }
        Blend One One
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            float4 _VolumetricColors[100];
            float4 _VolumetricPositions[100];
            float _VolumetricRanges[100];
            float _VolumetricDensities[100];
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            
            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 color : VAR_COLOR;
                float2 uv : VAR_UV;
            };

            static const float3 vertices[] =
            {
                float3( 0.5,  0.5, 0.0),
                float3( 0.5, -0.5, 0.0),
                float3(-0.5, -0.5, 0.0),
                
                float3(-0.5, -0.5, 0.0),
                float3(-0.5,  0.5, 0.0),
                float3( 0.5,  0.5, 0.0),
            };

            float noise(int position, uint seed)
            {
                static const int bitNoise1 = 0xB5297A4D;
                static const int bitNoise2 = 0x68E31DA4;
                static const int bitNoise3 = 0x1b56C4E9;
                
                uint res = position;
                res *= bitNoise1;
                res += seed;
                res ^= (res >> 8);
                res += bitNoise2;
                res ^= (res << 8);
                res *= bitNoise3;
                res ^= (res >> 8);
                
                return (float)res / ~0u;
            }
            
            const static float particleSize = 0.4;
            const static int particleCount = 100000;

            Varyings vert(uint id : SV_VertexID)
            {
                Varyings o;

                int particleID = id / 6;

                float3 offset = float3(noise(particleID, 0), noise(particleID, 5), noise(particleID, 10)) * 2.0 - 1.0;
                float3 drift = float3(noise(particleID, 15), noise(particleID, 20), noise(particleID, 25)) * 2.0 - 1.0;
                offset = (offset + drift * _Time[0]) % 1.0;
                
                float lightID = particleID / particleCount;
                float range = _VolumetricRanges[lightID].x;
                float3 color = _VolumetricColors[lightID];
                
                float3 position = _VolumetricPositions[lightID].xyz;

                o.vertex.xyz = mul(UNITY_MATRIX_V, float4(position + offset * range, 1.0)).xyz + vertices[id % 6] * particleSize;
                o.vertex = mul(UNITY_MATRIX_P, float4(o.vertex.xyz, 1.0));
                
                float attenuation = DistanceAttenuation(dot(offset * range, offset * range), 1.0f / (range * range));
                float density = range * range * range / particleCount;
                
                o.color.rgb = color;
                o.color.a = attenuation * density * _VolumetricDensities[lightID];

                o.uv = vertices[id % 6].xy;
                return o;
            }

            float3 _Color;
            float _Value;

            TEXTURECUBE(_Noise);
            SAMPLER(sampler_Noise);

            float4 frag(Varyings input) : SV_Target
            {
                return float4(input.color.rgb * input.color.a, 1.0f);
            }
            ENDHLSL
        }
    }
}