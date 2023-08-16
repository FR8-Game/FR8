Shader "Unlit/Clouds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextureScale("Texture Scale", float) = 1.0
        _Color("Cloud Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _CloudDepth("Cloud Depth", float) = 20.0
        _CloudDensity("Cloud Density", float) = 2.0
        _CloudSize("Cloud Size", Range(0.0, 1.0)) = 1.0
        _Exponent("Exponent", float) = 5.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float depth : VAR_DEPTH;
                float4 vertex : SV_POSITION;
            };

            float _TextureScale;
            float _CloudDepth;
            float _Exponent;

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldOffset;
                worldOffset.xz = _WorldSpaceCameraPos.xz;
                worldOffset.y = TransformObjectToWorld(0.0).y;

                float3 vertex = input.vertex.xyz;
                vertex.xz *= 100000.0f;
                vertex.y *= _CloudDepth;

                float3 worldPos = worldOffset + vertex;

                output.vertex = TransformWorldToHClip(worldPos);
                output.uv = worldPos.xz / (100.0 * _TextureScale);
                output.depth = pow(abs(input.vertex.y * 2.0), 1.0 / _Exponent);
                return output;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float _CloudDensity;
            float _CloudSize;

            half4 frag(Varyings input) : SV_Target
            {
                float sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).r;
                sample -= 1.0 - _CloudSize;
                sample /= _CloudSize;
                sample -= input.depth;
                sample = max(sample, 0.0);

                half4 col = _Color;
                col.a *= sample;
                return half4(col.rgb, col.a / 400 * _CloudDensity);
            }
            ENDHLSL
        }
    }
}