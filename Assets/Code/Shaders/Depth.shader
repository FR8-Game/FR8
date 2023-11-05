Shader "Unlit/Depth"
{
	Properties{ }
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float3 positionWS : POSITION_WS;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.positionWS = TransformObjectToWorld(input.vertex.xyz);
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				return output;
			}
			
			float4 frag (Varyings input) : SV_Target
			{
				return float4(input.positionWS.y, 0.0, 0.0, 0.0);
			}
			ENDHLSL
		}
	}
}