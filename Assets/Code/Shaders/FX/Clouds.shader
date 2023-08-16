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
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            //#define DEBUG;

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float3 worldPosition : VAR_WORLDPOS;
                float4 vertex : SV_POSITION;

                #ifdef DEBUG
                float debug : VAR_DEBUG;
                #endif
            };

            float _CloudDepth;
            float _GeometrySize;

            float sqr(float v) { return v * v; }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldOffset;
                worldOffset.xz = _WorldSpaceCameraPos.xz;
                worldOffset.y = TransformObjectToWorld(0.0).y;

                float3 vertex = input.vertex.xyz / 10.0;
                vertex.x *= -1.0;
                vertex.xz *= _GeometrySize;

                float heightScale = 1.0 - length(input.vertex.xz / 10.0) * 2.0;
                output.worldPosition = worldOffset + vertex;
                
                worldOffset.y *= heightScale;
                output.vertex = TransformWorldToHClip(worldOffset + vertex);

                return output;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            float _NoiseInfluence;
            float _NoiseScale;

            float2 _ScrollSpeed;

            float sampleNoise(float2 uv)
            {
                float noise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uv / exp(_NoiseScale)).r;
                float sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + _ScrollSpeed * _Time.x).r;
                return (sample + noise * _NoiseInfluence) / (1.0 + _NoiseInfluence);
            }

            float _TextureScale;

            float2 getUV(float3 worldPosition) { return worldPosition.xz / _TextureScale * 0.01; }

            float4 _ColorLow;
            float4 _ColorHigh;
            float _CloudDensity;
            float _CloudSize;

            float _ShadingContrast;
            float _ShadingStrength;
            float2 _ShadingDirection;

            const static int Resolution = 100;

            half4 frag(Varyings input) : SV_Target
            {
                #ifdef DEBUG
                return float4(input.debug.xxx, 1.0);
                #endif

                float3 start = input.worldPosition;
                float3 offset = normalize(_WorldSpaceCameraPos - start);
                offset *= _CloudDepth / offset.y;

                float2 density = 0.0;
                for (int i = 0; i < Resolution; i++)
                {
                    float p = i / (Resolution - 1.0);

                    float3 worldPos = start + offset * (1.0 - p);
                    float2 uv = getUV(worldPos);

                    float sample = sampleNoise(uv);
                    float shading = sampleNoise(getUV(worldPos + float3(_ShadingDirection.x, 0.0, _ShadingDirection.y)));

                    float depth = 1.0 - sqr(2.0f * p - 1.0);
                    depth = depth * depth * depth * depth;
                    sample *= depth;
                    sample -= 1.0 - _CloudSize;
                    sample /= _CloudSize;
                    sample = max(sample, 0.0);

                    float layer = sample * _CloudDensity;
                    density.x = lerp(density.x, layer, layer / Resolution);
                    density.y = lerp(density.y, shading, layer / Resolution);
                }

                float alpha = density.x;
                alpha = 1.0 - exp(-alpha);

                float3 lightColor = GetMainLight().color.rgb;
                float shading = exp(_ShadingContrast * -density.y) * _ShadingStrength + (1.0 - _ShadingStrength);
                float4 baseColor = lerp(_ColorLow, _ColorHigh, saturate(shading));
                
                return float4(baseColor.rgb * lightColor * saturate(shading), baseColor.a * alpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "FR8Editor.Shader.CloudsGUI"
}