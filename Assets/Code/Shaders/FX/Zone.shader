Shader "Unlit/Zone"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Brightness("Brightness", float) = 0
        _Size("Size", float) = 1
        _Width("Width", float) = 0.1
        _Speed("Speed", float) = 1
        _Dots("Dots", float) = 1
        _FadeDistance("Fade Distance", float) = 10.0
        _FadeWidth("Fade Width", float) = 5.0
    }
    SubShader
    {

        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Blend One One
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : VAR_UV;
                float2 margin : VAR_SIZE;
                float3 worldPos : VAR_POSITION;
            };

            float2 triplanar(float3 position, float3 normal)
            {
                float3 weights = abs(normal);

                return
                    float2(position.z, position.y) * weights.x +
                    float2(position.x, position.z) * weights.y +
                    float2(position.x, position.y) * weights.z;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.worldPos = TransformObjectToWorld(input.vertex.xyz);
                output.vertex = TransformWorldToHClip(output.worldPos);

                float3 worldNormal = normalize(input.normal);
                float3 worldScale = abs(float3
                (
                    length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
                    length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
                    length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z)) // scale z axis
                ));

                output.uv = triplanar(input.vertex * worldScale, worldNormal);
                output.margin = triplanar(worldScale / 2, worldNormal);
                return output;
            }

            float4 _Color;
            float _Size, _Width, _Speed;
            float _Brightness;
            float _Dots;
            float _FadeDistance, _FadeWidth;

            float dottedLine(float uvx, float uvy)
            {
                uvx += _Time.x * _Speed;
                uvy += _Time.x * _Speed;

                float x1 = (uvx % _Size + _Size) % _Size;
                return (x1 < _Width) * ((uvy * _Dots % 1 + 1) % 1 > 0.5);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 col = float4(_Color.rgb * pow(2, _Brightness) * _Color.a, 1.0);

                float dist = length(input.worldPos - _WorldSpaceCameraPos);
                float fade = (dist - _FadeDistance) / _FadeWidth;

                float dither = fade - InterleavedGradientNoise(input.vertex, 0) > 0.0;
                if (dither)
                {
                    return col * _Width;
                }
                
                float bands = dottedLine(input.uv.x, input.uv.y) + dottedLine(input.uv.y, -input.uv.x);

                float2 margin = (abs(input.uv) - input.margin);
                bands += max(margin.x, margin.y) > -_Width;

                clip(bands - 0.5);

                return col;
            }
            ENDHLSL
        }
    }
}