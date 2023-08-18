Shader "Unlit/Map"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GridSize("Grid Size", float) = 100.0
		_GridWidth("Grid Width", Range(0.0, 1.0)) = 0.1
		_Color("Line Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Brightness("Color Emissivity", float) = 1.0
		_HeightScale("Height Scale", float) = 3.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		ZWrite Off
		Blend One One
		
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
			
			TEXTURE2D_X(_MainTex);
			SAMPLER(sampler_MainTex);

			float _HeightScale;
			
			Varyings vert (Attributes input)
			{
				Varyings output;
				
				float2 uv = input.vertex.xz * 0.5 + 0.5;
				float3 vertex = input.vertex.xyz;

				float height = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uv, 0).r * _HeightScale + 0.001;
				vertex.y *= height;
				
				output.vertex = TransformObjectToHClip(vertex);
				output.height = (1.0 - input.vertex.y) * height;
				return output;
			}

			float _GridSize;
			float _GridWidth;

			float4 _Color;
			float _Brightness;
			
			half4 frag (Varyings input) : SV_Target
			{	
				// float2 grid = abs((input.uv * _GridSize % 1.0) * 2.0 - 1.0) < _GridWidth;
				// return half4(saturate(grid.r * grid.g) * _Color.rgb * _Color.a * exp(_Brightness) * _Brightness, 1.0);

				return input.height < 0.001 ? 1.0 : 0.05;
			}
			ENDHLSL
		}
	}
}