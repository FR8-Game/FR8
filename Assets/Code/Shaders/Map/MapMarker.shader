Shader "Unlit/MapMarker"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }

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

			half4 frag (float4 vertex : SV_POSITION) : SV_Target
			{
				return float4(0.0, 10.0, 0.0, 0.0);
			}
			ENDHLSL
		}
	}
}