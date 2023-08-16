Shader "Unlit/Clouds"
{
    Properties
    {
        [MainTexture]
        [NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" {}
        _TextureScale("Texture Scale", float) = 1.0
        [MainColor]
        _ColorHigh("Cloud Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _ColorLow("Cloud Color", Color) = (0.2, 0.2, 0.2, 1.0)

        _CloudHeight("Cloud Height", float) = 20.0
        _CloudDepth("Cloud Depth", float) = 20.0
        _CloudDensity("Cloud Density", float) = 2.0
        _CloudSize("Cloud Size", Range(0.0, 1.0)) = 1.0
        _ScrollSpeed("Scroll Speed", Vector) = (0.0, 0.0, 0.0, 0.0)

        [NoScaleOffset]
        _Noise("Noise", 2D) = "gray" {}
        _NoiseInfluence("Noise Influence", Range(0.0, 1.0)) = 0.0
        _NoiseScale("Noise Scale", float) = 1.0

        _GeometrySize("Geometry Size", float) = 10000.0

        _ShadingContrast("Shading Contrast", float) = 0.0
        _ShadingStrength("Shading Strength", Range(0.0, 1.0)) = 1.0
        _ShadingDirection("Shading Direction [F2]", Vector) = (5.0, 0.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        //Blend One OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            const static int Resolution = 30;
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            //#define DEBUG;

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float3 normal : VAR_WORLDPOS;
                float4 vertex : SV_POSITION;
                float4 screenPos : VAR_SCREENPOS;

                #ifdef DEBUG
                float debug : VAR_DEBUG;
                #endif
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
            float _NoiseInfluence;
            float _NoiseScale;
            float2 _ScrollSpeed;
            float _CloudDepth;
            float _GeometrySize;
            float4 _ColorLow;
            float4 _ColorHigh;
            float _CloudHeight;
            float _CloudDensity;
            float _CloudSize;
            float _ShadingContrast;
            float _ShadingStrength;
            float2 _ShadingDirection;
            float _TextureScale;

            float sqr(float v) { return v * v; }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 vertex = input.vertex.xyz * _GeometrySize + _WorldSpaceCameraPos;
                output.vertex = TransformWorldToHClip(vertex);
                output.normal = input.vertex;
                output.screenPos = ComputeScreenPos(output.vertex);

                return output;
            }

            float sampleNoise(float2 uv)
            {
                float noise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uv / exp(_NoiseScale)).r;
                float sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + _ScrollSpeed * _Time.x).r;
                return (sample + noise * _NoiseInfluence) / (1.0 + _NoiseInfluence);
            }


            float2 getUV(float3 worldPosition) { return worldPosition.xz / _TextureScale * 0.01; }

            float3 worldPositionFromDepth(float4 screenPos)
            {
                float2 uv = screenPos.xy / screenPos.w;
                #if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(uv);
                #else
                // Adjust z to match NDC for OpenGL
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
            }

            half4 frag(Varyings input) : SV_Target
            {
                #ifdef DEBUG
                return float4(input.debug.xxx, 1.0);
                #endif

                float3 normal = normalize(input.normal);
                float3 start = normal * _CloudHeight / normal.y;
                float3 offset = normal * _CloudDepth / normal.y;

                float majorNoise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, getUV(start) * 0.1);

                float2 density = 0.0;
                for (int i = 0; i < Resolution; i++)
                {
                    float p = i / (Resolution - 1.0);

                    float3 worldPos = start + offset * (1.0 - p);
                    float2 uv = getUV(worldPos);

                    float sample = sampleNoise(uv);
                    float shading = sampleNoise(getUV(worldPos + float3(_ShadingDirection.x, 0.0, _ShadingDirection.y)));

                    float depth = 1.0 - sqr(2.0f * p - 1.0);
                    depth = pow(depth, 12.0);
                    sample *= depth;
                    sample -= 1.0 - _CloudSize;
                    sample /= _CloudSize;
                    sample = max(sample, 0.0);

                    float layer = sample * _CloudDensity;
                    density.x = lerp(density.x, layer, saturate(layer / Resolution));
                    density.y = lerp(density.y, shading, saturate(layer / Resolution));
                }

                float alpha = density.x * majorNoise;
                alpha = 1.0 - exp(-alpha);

                float3 lightColor = GetMainLight().color.rgb;
                
                float shading = exp(_ShadingContrast * -density.y) * _ShadingStrength + (1.0 - _ShadingStrength);
                float4 baseColor = lerp(_ColorLow, _ColorHigh, saturate(shading));

                return float4(baseColor.rgb * lightColor * saturate(shading), saturate(baseColor.a * alpha));
            }
            ENDHLSL
        }
    }
    CustomEditor "FR8Editor.Shader.CloudsGUI"
}