// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VD Vertex Color/Vertex color overlay (multiply)"
{
	Properties
	{
		_VCIntensity ("Vertex color intensity", Range(0.0,2.0)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		blend DstColor Zero
		zwrite off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			struct Varyings
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
			};
			half _VCIntensity;
			Varyings vert (appdata_full v)
			{
				Varyings o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			fixed4 frag (Varyings input) : SV_Target
			{
				return lerp(1.0,input.color,_VCIntensity);
			}
			ENDCG
		}
	}
}
