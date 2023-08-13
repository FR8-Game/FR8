Shader "Unlit/Distortion"
{
    Properties
    {
        _Intensity("Intensity", float) = 0.2
        _Speed("Speed", float) = 1.0
        _Rings("Rings", int) = 3
        _EdgeTint("Edge Tint", Color) = (1.0, 1.0, 1.0, 0.1)
        _EdgeExp("Edge Exponent", float) = 6
        _Noise("Noise [Grey]", 2D) = "white"{}
        _NoiseStrength("Noise Strength", float) = 0.1
        _NoiseSpeed("Noise ScrollSpeed", Vector) = (0.0, 0.0, 0.0, 0.0)
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
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionCS : VAR_POSITION_CS;
                float4 centerCS : VAR_CENTER_CS;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = mul(UNITY_MATRIX_M, float4(0.0, 0.0, 0.0, 1.0));

                output.vertex = mul(UNITY_MATRIX_V, output.vertex) + mul(UNITY_MATRIX_M, float4(input.vertex.xyz, 0.0));
                output.vertex = mul(UNITY_MATRIX_P, output.vertex);

                output.positionCS = ComputeScreenPos(output.vertex);
                output.centerCS = ComputeScreenPos(TransformObjectToHClip(0.0));
                output.uv = input.uv;
                return output;
            }

            float2 slope(float2 val, float exp)
            {
                return pow(abs(val), exp) * sign(val);
            }

            float _Intensity;
            float _Speed;
            float _Rings;

            float4 _EdgeTint;
            float _EdgeExp;

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
            float _NoiseStrength;
            float4 _NoiseSpeed;

            float getNoise(float2 uv, float rotation)
            {
                float s = sin(rotation);
                float c = cos(rotation);

                uv = 2.0 * uv - 1.0;
                uv /= 1.414;
                uv = float2(uv.x * c + uv.y * s, uv.y * c - uv.x * s);
                uv = uv * 0.5 + 0.5;
                
                return SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uv);
            }

            float getScrollingNoise(float2 uv)
            {
                float t = _Time.y * _NoiseSpeed.x;

                float noise1 = getNoise(uv, t);
                float noise2 = getNoise(uv, -t);
                
                return lerp(noise1, noise2, 0.5);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float noise = getScrollingNoise(input.uv);

                half4 col;
                half2 uv = input.positionCS.xy / input.positionCS.w;
                half2 center = input.centerCS.xy / input.centerCS.w;

                float size = _Time.x * _Speed;
                float distance = length(input.uv * 2.0 - 1.0);
                distance = distance + noise * 0.1 * _NoiseStrength;

                if (distance > 1.0) discard;
                float rings = 1.0 - (-(pow(distance, 4.0) - size) * _Rings % 1.0);

                //return rings;

                float edge = 1.0 - distance;
                float distortion = (1.0 - rings) * edge * edge * _Intensity;

                uv = (uv - center) * (1.0 + distortion) + center;

                col.rgb = SampleSceneColor(uv);
                col.a = 1.0;

                float4 edgeColor = _EdgeTint * pow(rings, _EdgeExp);
                col.rgb = lerp(col.rgb, edgeColor.rgb, edgeColor.a);

                return col;
            }
            ENDHLSL
        }
    }
}