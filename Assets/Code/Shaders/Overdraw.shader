Shader "Hidden/Overdraw"
{
	Properties { }
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			float4 vert (float4 vertex : POSITION) : SV_POSITION
			{
				return TransformObjectToHClip(vertex.xyz);
			}

			half4 frag () : SV_Target
			{
				float3 baseCol = float3(1.0, 0.4, 0.0);
				
				return float4(baseCol, 1.0);
			}
			ENDHLSL
		}
	}
}