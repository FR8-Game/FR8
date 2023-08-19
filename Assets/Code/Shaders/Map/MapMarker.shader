Shader "Unlit/MapMarker"
{
	Properties
	{
		_BaseColor ("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Brightness ("Brightness", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend One One
		ZWrite Off
		Cull Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : VAR_WORLDPOS;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.worldPos = TransformObjectToWorld(input.vertex.xyz);
				output.vertex = TransformWorldToHClip(output.worldPos);
				output.uv = input.uv;
				return output;
			}

			float4 _BaseColor;
			float4 _MarkerColor;
			float _Brightness;

			float4x4 _CullMatrix;
			
			half4 frag (Varyings input) : SV_Target
			{
				float3 cullPos = mul(_CullMatrix, float4(input.worldPos, 1.0));
				if (cullPos.x > 0.5) discard;
				if (cullPos.y > 0.5) discard;
				if (cullPos.z > 0.5) discard;
				
				if (cullPos.x < -0.5) discard;
				if (cullPos.y < -0.5) discard;
				if (cullPos.z < -0.5) discard;
				
				half3 color =_BaseColor.rgb * _MarkerColor.rgb * max(0.0, _Brightness);
				color *= _BaseColor.a * _MarkerColor.a;
				return half4(color, 1.0);
			}
			ENDHLSL
		}
	}
}