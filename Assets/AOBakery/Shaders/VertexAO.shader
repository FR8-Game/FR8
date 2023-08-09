Shader "Bakery/VertexAO" {
	
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_NormalMap("Normal", 2D) = "normal" {}
		_Color ("Occlusion Color", Color) = (0,0,0,1)
		_Intensity ("Intensity", Range(0, 1)) = 0.0
		_Blend ("Overflow", Range(1, 10)) = 1
	}
	
	SubShader {
		
		Tags {
			"RenderType" = "Opaque"
		}
		
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert 
		
		sampler2D _MainTex;
		sampler2D _NormalMap;
		half4 _Color;
		float _Intensity;
		float _Blend;
		
		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float2 _NormalMap;
			float4 color      : COLOR;
		};
		
		void vert(inout appdata_full v)
		{
			v.color.rgb = _Color;
			v.color.a   = pow((1-v.color.a)*(v.color.a * _Intensity / v.color.a), 10.001 - _Blend);
		}
		
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
			half4 c  = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = lerp(c.rgb, IN.color.rgb, IN.color.a);
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN._NormalMap));
			o.Alpha  = c.a;
		}
		
		ENDCG
	}
	
	FallBack "Diffuse"	
}
