// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Grid" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		//_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
		//_Gloss ("Gloss", Range (1, 50)) = 50
		//_Specularity ("Specularity", Range (1, 10)) = 10
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainTex2("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex3("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex4("Bugfix (Don't assign)", 2D) = "white" {}
		_Angles("Angles (Don't assign)", 2D) = "white" {}
		_Colors("Colors (Don't assign)", 2D) = "white" {}
		_Ranges("Ranges (Don't assign)", 2D) = "white" {}
		_Distances("Distances (Don't assign)", 2D) = "white" {}
		_Intensities("Intensities (Don't assign)", 2D) = "white" {}
		_PalletteMap ("Pallette", 2D) = "white" {}
		_BumpMap ("Normal", 2D) = "bump" {}
		//_SpecMap ("Specular", 2D) = "white" {}
		_EmissiveMap ("Emissive", 2D) = "white" {}
		_Emission ("Emission (Lightmapper)", Float) = 1.0
	}

	SubShader {
		Tags {"IgnoreProjector"="True" "RenderType"="Transparent" "Queue"="Geometry" } 
		LOD 400
		ZTest LEqual
		ZWrite On

		Stencil {
			Ref 1
			Comp always
			Pass replace
		}

		CGPROGRAM
		#pragma surface surf BlinnPhong alpha:fade
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Angles;
		sampler2D _Colors;
		sampler2D _Ranges;
		sampler2D _Distances;
		sampler2D _Intensities;
		sampler2D _PalletteMap;
		sampler2D _BumpMap;
		//sampler2D _SpecMap;
		sampler2D _EmissiveMap;
		fixed4 _Color;
		//half _Gloss;
		//half _Specularity;
		fixed _Emission;

		uniform fixed4 _allColors[128];
		fixed4 colorToUse;

		fixed4 tex;
		fixed4 emTex;
		fixed4 palTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Angles;
			float2 uv_Colors;
			float2 uv_Ranges;
			float2 uv_Distances;
			float2 uv_Intensities;
			float2 uv_PalletteMap;
			float2 uv_BumpMap;
			//float2 uv_SpecMap;
			float2 uv_EmissiveMap;
			float4 vColor : COLOR; // interpolated vertex color
			float2 uv2_MainTex2;
			float2 uv3_MainTex3;
			float2 uv4_MainTex4;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			tex = tex2D(_MainTex, IN.uv_MainTex);
			//fixed4 specTex = tex2D(_SpecMap, IN.uv_SpecMap);
			emTex = tex2D(_EmissiveMap, IN.uv_EmissiveMap);
			palTex = tex2D(_PalletteMap, IN.uv_PalletteMap);

			if(palTex.r > 0.9f){
				colorToUse = _allColors[floor(IN.uv2_MainTex2.x)];
			}
			else if(palTex.r > 0.8f){
				colorToUse = _allColors[floor(IN.uv2_MainTex2.y)];
			}
			else if(palTex.r > 0.7f){
				colorToUse = _allColors[floor(IN.uv3_MainTex3.x)];
			}
			else if(palTex.r > 0.6f){
				colorToUse = _allColors[floor(IN.uv3_MainTex3.y)];
			}
			else if (palTex.r > 0.5f) {
				colorToUse = _allColors[floor(IN.uv4_MainTex4.x)];
			}
			else if (palTex.r > 0.4f) {
				colorToUse = _allColors[floor(IN.uv4_MainTex4.y)];
			}
			else if (palTex.r > 0.3f) {
				colorToUse = _allColors[floor(IN.vColor.r * 255)];
			}
			else if (palTex.r > 0.2f) {
				colorToUse = _allColors[floor(IN.vColor.g * 255)];
			}
			else if (palTex.r > 0.1f) {
				colorToUse = _allColors[floor(IN.vColor.b * 255)];
			}
			else if (palTex.r > 0.0f) {
				colorToUse = _allColors[floor(IN.vColor.a * 255)];
			}

			o.Albedo = (tex.rgb * _Color.rgb * colorToUse.rgb);
			o.Alpha = tex.a * _Color.a;
				//o.Gloss = specTex.a * _Specularity; // for some reason, these seem to be inverted - I have no idea.
			//o.Specular = specTex.a * _Gloss;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Emission = emTex.rgb;
		}
		ENDCG
	}

	FallBack "Legacy Shaders/Transparent/VertexLit"
}