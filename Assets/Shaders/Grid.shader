// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Grid" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	//_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
	//_Gloss ("Gloss", Range (1, 50)) = 50
	//_Specularity ("Specularity", Range (1, 10)) = 10
	_MainTex ("Main Texture", 2D) = "white" {}
	_PalletteMap ("Pallette", 2D) = "white" {}
	_BumpMap ("Normal", 2D) = "bump" {}
	//_SpecMap ("Specular", 2D) = "white" {}
	_EmissiveMap ("Emissive", 2D) = "white" {}
	_Emission ("Emission (Lightmapper)", Float) = 1.0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 400
	
CGPROGRAM
#pragma surface surf BlinnPhong alpha:fade
#pragma target 3.0

sampler2D _MainTex;
sampler2D _PalletteMap;
sampler2D _BumpMap;
//sampler2D _SpecMap;
sampler2D _EmissiveMap;
fixed4 _Color;
//half _Gloss;
//half _Specularity;
fixed _Emission;

uniform fixed4 _allColors[32];
fixed4 colorToUse;

fixed4 tex;
fixed4 emTex;
fixed4 palTex;


struct Input {
	float2 uv_MainTex;
	float2 uv_PalletteMap;
	float2 uv_BumpMap;
	//float2 uv_SpecMap;
	float2 uv_EmissiveMap;
	float4 vColor : COLOR; // interpolated vertex color
};

void surf (Input IN, inout SurfaceOutput o) {
	tex = tex2D(_MainTex, IN.uv_MainTex);
	//fixed4 specTex = tex2D(_SpecMap, IN.uv_SpecMap);
	emTex = tex2D(_EmissiveMap, IN.uv_EmissiveMap);
	palTex = tex2D(_PalletteMap, IN.uv_PalletteMap);

	if(palTex.r > 0.75f){
		colorToUse = _allColors[floor(255 * IN.vColor.r)];
	}
	else if(palTex.r > 0.5f){
		colorToUse = _allColors[floor(255 * IN.vColor.g)];
	}
	else if(palTex.r > 0.25f){
		colorToUse = _allColors[floor(255 * IN.vColor.b)];
	}
	else if(palTex.r > 0){
		colorToUse = _allColors[floor(255 * IN.vColor.a)];
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