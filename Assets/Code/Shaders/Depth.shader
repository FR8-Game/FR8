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
				float4 normal : NORMAL;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float height : VAR_HEIGHT;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.height = TransformObjectToWorld(input.vertex.xyz).y;
				output.normal = TransformObjectToWorldNormal(input.normal.xyz);
				return output;
			}
			
			float4 frag (Varyings input) : SV_Target
			{
				float3 normal = normalize(input.normal);
				float heightScale = 1.0 / unity_OrthoParams.y;
				return float4(input.height * heightScale, normal.xz * 0.5 + 0.5, 1.0);
			}
			ENDHLSL
		}
	}
}