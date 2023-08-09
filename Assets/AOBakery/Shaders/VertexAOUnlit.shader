Shader "Bakery/VertexAOUnlitColor" {

	Properties{
		_ObjColor("Object Color", Color) = (0,0,0,1)
		_Color("Occlusion Color", Color) = (0,0,0,1)
		_Intensity("Intensity", Range(0, 1)) = 0.0
		_Blend("Overflow", Range(1, 10)) = 1
	}

		SubShader{

			Lighting Off
			AlphaTest Greater 0.5

			Tags {
				"RenderType" = "Opaque"
			}

			LOD 200

			CGPROGRAM
			#pragma surface surf NoLighting vertex:vert 
			#include "UnityCG.cginc" 

			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
				return fixed4(0, 0, 0, 0);//half4(s.Albedo, s.Alpha);
			}

			half4 _ObjColor;
			half4 _Color;
			float _Intensity;
			float _Blend;

			struct Input {
				float4 color      : COLOR;
			};


			void vert(inout appdata_full v)
			{
				v.color.rgb = _Color;
				v.color.a = pow((1 - v.color.a)*(_Intensity), 10.001 - _Blend);
			}


			void surf(Input IN, inout SurfaceOutput o) {
				half4 c = { 1,1,1,1 };
				o.Emission = _ObjColor * lerp(c.rgb, IN.color.rgb, IN.color.a);
			}

			ENDCG
	}

		FallBack "Diffuse"
}
