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
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float height : VAR_HEIGHT;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.height = TransformObjectToWorld(input.vertex.xyz).y;
				return output;
			}
			
			half4 frag (Varyings input) : SV_Target
			{
				return input.height;
			}
			ENDHLSL
		}
	}
}