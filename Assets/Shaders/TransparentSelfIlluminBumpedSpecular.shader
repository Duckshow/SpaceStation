Shader "Custom/TransparentSelfIlluminBumpedSpecular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	//_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_Illum("Emissive", 2D) = "white" {}
	_SpecMap("Specular", 2D) = "white" {}
	_Emission ("Emission (Lightmapper)", Float) = 1.0
}
SubShader {
	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
	LOD 400
CGPROGRAM
#pragma surface surf BlinnPhong alpha:fade
#pragma target 3.0

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Illum;
sampler2D _SpecMap;
fixed4 _Color;
fixed4 c;
half _Shininess;
fixed _Emission;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float2 uv_Illum;
	float2 uv_SpecMap;
	//float4 interpolatedVertexColor : COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 specTex = tex2D(_SpecMap, IN.uv_SpecMap);
	c = tex * _Color;
	o.Albedo = c.rgb;
	o.Emission = /*c.rgb * */tex2D(_Illum, IN.uv_Illum)/*.a*/;
#if defined (UNITY_PASS_META)
	o.Emission *= _Emission.rrr;
#endif
	o.Alpha = tex.a * _Color.a;
	o.Gloss = 1; //specTex.a * _Shininess;
	o.Specular = 1; //specTex.a * _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
}
FallBack "Legacy Shaders/Self-Illumin/Specular"
CustomEditor "LegacyIlluminShaderGUI"
}
