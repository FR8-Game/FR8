Shader "Unlit/OutlineObject"
{
    Properties
    {
        _Color("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(vertex.xyz);
            }

            half4 _Color;
            
            half4 frag () : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
