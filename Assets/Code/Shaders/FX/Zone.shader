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

            float Line(float uvx, float width = 1.0)
            {
                uvx += _Time.x * _Speed;

                float x1 = (uvx % _Size + _Size) % _Size;
                return x1 < (_Width * width);
            }

            float DottedLine(float uvx, float uvy)
            {
                uvy += _Time.x * _Speed;

                float x1 = Line(uvx);
                return x1 * ((uvy * _Dots % 1 + 1) % 1 > 0.5);
            }

            float Lod0(Varyings input)
            {
                float lines = DottedLine(input.uv.x, input.uv.y) + DottedLine(input.uv.y, -input.uv.x);

                float2 marginSS = (abs(input.uv) - input.margin);
                float margin = -max(marginSS.x, marginSS.y);

                float bands = lines + (margin < _Width);

                return saturate(bands);
            }

            float Lod1(Varyings input)
            {
                float width = 3.0;

                float lines = Line(input.uv.x, width) + Line(input.uv.y, width);

                float2 marginSS = (abs(input.uv) - input.margin);
                float margin = -max(marginSS.x, marginSS.y);

                float bands = lines + (margin < _Width * width);

                return saturate(bands) / width;
            }

            float Lod2(Varyings input)
            {
                float width = 9.0;

                float2 marginSS = (abs(input.uv) - input.margin);
                float margin = -max(marginSS.x, marginSS.y);

                return (margin < _Width * width) / width;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 col = float4(_Color.rgb * pow(2, _Brightness) * _Color.a, 1.0);

                float dist = length(input.worldPos - _WorldSpaceCameraPos);
                float fade[] =
                {
                    (dist - _FadeDistance) / _FadeWidth,
                    (dist - _FadeDistance * 4.0) / _FadeWidth,
                };

                float dither[] =
                {
                    fade[0] - InterleavedGradientNoise(input.vertex, 0) > 0.0,
                    fade[1] - InterleavedGradientNoise(input.vertex, 0) > 0.0,
                };

                float lods[] =
                {
                    Lod0(input),
                    Lod1(input),
                    Lod2(input),
                };

                float value = lerp(lerp(lods[0], lods[1], dither[0]), lods[2], dither[1]);

                clip(value - 0.1);

                return col * value;
            }
            ENDHLSL
        }
    }
}