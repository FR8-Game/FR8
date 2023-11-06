Shader "Unlit/Map"
{
    Properties
    {
        _MapTexture("MapTexture", 2D) = "black" {}
        _MapColor("Base Color", Color) = (1, 1, 1, 1)
        _LineColor("Line Color", Color) = (1, 1, 1, 1)
        _Brightness("Brightness", float) = 0.0
        _BandFreq("Band Frequency", float) = 1.0
        _BandSize("Band Size", Range(0.0, 1.0)) = 0.2
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

            TEXTURE2D_X(_MapTexture);
            SAMPLER(sampler_MapTexture);
            float4 _MapTexture_TexelSize;
            float4 _MapColor;
            float4 _LineColor;
            float _Brightness;
            float _BandFreq;
            float _BandSize;

            Varyings vert(Attributes input)
            {
                return LitPassVertex(input);
            }

            half4 sampleMap(float2 uv)
            {
                half2 offsets[] =
                {
                    float2(-1.0, -1.0),
                    float2( 0.0, -1.0),
                    float2( 1.0, -1.0),
                    
                    float2(-1.0,  0.0),
                    float2( 0.0,  0.0),
                    float2( 1.0,  0.0),
                    
                    float2(-1.0,  1.0),
                    float2( 0.0,  1.0),
                    float2( 1.0,  1.0),
                };

                half4 res = 0.0;
                for (int i = 0; i < 9; i++)
                {
                    res += SAMPLE_TEXTURE2D(_MapTexture, sampler_MapTexture, uv + offsets[i] * _MapTexture_TexelSize.xy * 4.0);
                }
                return res / 9.0;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 base = LitPassFragment(input);

                float2 uv = input.uv;
                // uv = uv * 2.0 - 1.0;
                // float t = _Time.z;
                // uv *= 1 + sin(t) * 0.5;
                // uv = uv * 0.5 + 0.5;
                
                float4 sample = sampleMap(uv);
                float val = sin(sample.x * PI * _BandFreq / 100.0);
                val = val > _BandSize;

                float3 overlay = _MapColor.rgb * val * pow(2, _Brightness / 10.0);
                overlay += SAMPLE_TEXTURE2D(_MapTexture, sampler_MapTexture, uv).g * _LineColor;

                return base + float4(overlay, 1.0);
            }
            ENDHLSL
        }
    }
}