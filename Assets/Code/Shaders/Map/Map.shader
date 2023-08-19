Shader "Unlit/Map"
{
    Properties
    {
        _MainTex ("Map Texture", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Color("Line Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Brightness("Color Emissivity", float) = 1.0
        _LineWidth("Line Width", Range(0.0, 1.0)) = 0.1
        _HeightScale("Height Scale", float) = 3.0

        _Fill("Fill Brightness", Range(0.0, 1.0)) = 0.1
        _NoiseColorS("Noise Strength Small", Range(0.0, 1.0)) = 0.2
        _NoiseColorL("Noise Strength Large", Range(0.0, 1.0)) = 0.2
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        ZWrite Off
        Blend One One
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
            float4 _Noise_TexelSize;

            float4 _Color;
            float _Brightness;
            float _LineWidth;

            float _Fill;
            float _NoiseColorS;
            float _NoiseColorL;
            
            float _HeightScale;
            
            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float height : VAR_HEIGHT;
                float2 uv : TEXCOORD0;
                float warp : VAR_WARP;
            };

            float sqr(float v) { return v * v; }

            bool sampleHeight(Varyings input, float2 uvOffset)
            {
                float2 uv = input.uv + uvOffset;
                if (input.warp > 0.5) uv = floor(uv * 10.0 / input.warp) / (10 / input.warp);
                
                return (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r * _HeightScale + 0.001 - input.height) > 0.0;
            }

            float rand(int i)
            {
                int w = _Noise_TexelSize.z;
                i %= w * _Noise_TexelSize.w;
                return SAMPLE_TEXTURE2D_LOD(_Noise, sampler_Noise, float2(i / w, i % w) / w, 0).x;
            }
            
            float noiseColor(Varyings input, float scale)
            {
                int layer = floor(input.height * 2598.0) + 1;

                float2 noiseUV = input.uv * _Noise_TexelSize.zw;
                noiseUV += floor(float2(rand(layer), rand(layer + 10)) * _Noise_TexelSize.zw);
                noiseUV *= _Noise_TexelSize.xy;
                float noise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV * scale).r;
                noise = (noise + _Time.x) * 4.0 % 1.0;
                noise = 1.0 - sqr(2.0 * noise - 1.0);
                return noise * noise * noise;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;

                float4 vertex = input.vertex;

                output.vertex = TransformObjectToHClip(vertex.xyz);

                float r = rand(_Time.x * 50 + rand(vertex.y * 1000.0) * 1000.0);
                output.warp = floor(r * r * r * 2.0);
                
                output.height = vertex.y;
                output.uv = vertex.xz * 0.5 + 0.5;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float sampleDistance = _LineWidth / 100.0;
                bool height = sampleHeight(input, 0.0);

                bool4 heights = float4
                (
                    sampleHeight(input, float2(-1.0, 1.0) * sampleDistance),
                    sampleHeight(input, float2(-1.0, -1.0) * sampleDistance),
                    sampleHeight(input, float2(1.0, 1.0) * sampleDistance),
                    sampleHeight(input, float2(1.0, -1.0) * sampleDistance)
                );

                float total = heights.r == heights.g && heights.r == heights.b && heights.r == heights.a;

                float val = 0.0;
                val += (1.0 - total);
                val += _Fill * height;
                clip(val);

                float noise = 1.0;
                noise *= lerp(1.0, noiseColor(input, 1.0), _NoiseColorS);
                noise *= lerp(1.0, noiseColor(input, 0.2), _NoiseColorL);

                return _Color * noise * val * _Brightness;
            }
            ENDHLSL
        }
    }
}