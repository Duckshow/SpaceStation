// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grid" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainTex1("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex2("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex3("Bugfix (Don't assign)", 2D) = "white" {}
		////_Angles("Angles (Don't assign)", 2D) = "white" {}
		_DotXs("DotXs (Don't assign)", 2D) = "white" {}
		_DotYs("DotYs (Don't assign)", 2D) = "white" {}
		_Colors("Colors (Don't assign)", 2D) = "white" {}
		_Ranges("Ranges (Don't assign)", 2D) = "white" {}
		_Distances("Distances (Don't assign)", 2D) = "white" {}
		_Intensities("Intensities (Don't assign)", 2D) = "white" {}
		_PalletteMap ("Pallette", 2D) = "white" {}
		_NormalMap ("Normal", 2D) = "bump" {}
		_EmissiveMap ("Emissive", 2D) = "white" {}
		_Emission ("Emission (Lightmapper)", Float) = 1.0
	}

	SubShader {
		Tags {"IgnoreProjector"="True" "RenderType"="Transparent" "Queue"="Geometry" } 
		Cull Back
		Lighting Off
		//ZTest LEqual
		ZWrite Off
		ColorMask RGBA
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil {
			Ref 1
			Comp always
			Pass replace
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert 
			//#pragma alpha:fade
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NormalMap;
			sampler2D _PalletteMap;
			sampler2D _BumpMap;
			sampler2D _EmissiveMap;
			//sampler2D _Angles;
			sampler2D _DotXs;
			sampler2D _DotYs;
			sampler2D _Colors;
			sampler2D _Ranges;
			sampler2D _Distances;
			sampler2D _Intensities;
			fixed _Emission;

			uniform fixed4 _allColors[128];
			fixed4 colorToUse;

			fixed4 tex;
			fixed4 nrmTex;
			fixed4 emTex;
			fixed4 palTex;
			//fixed4 anglesTex;
			fixed4 dotXsTex;
			fixed4 dotYsTex;
			fixed4 colorsTex;
			fixed4 rangesTex;
			fixed4 distancesTex;
			fixed4 intensitiesTex;

			fixed4 finalColor;

			struct appData {
				float4 vertex : POSITION;
				float4 vColor : COLOR; // vertex color
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
				half2 texcoord2 : TEXCOORD2;
				half2 texcoord3 : TEXCOORD3;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 worldPos : NORMAL;
				fixed4 vColor : COLOR;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
				half2 uv2 : TEXCOORD2;
				half2 uv3 : TEXCOORD3;
			};

			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vColor = v.vColor;
				o.uv = half2(v.texcoord.x, v.texcoord.y);
				o.uv1 = half2(v.texcoord1.x, v.texcoord1.y);
				o.uv2 = half2(v.texcoord2.x, v.texcoord2.y);
				o.uv3 = half2(v.texcoord3.x, v.texcoord3.y);
				return o;
			}


			fixed4 frag(v2f i) : COLOR {
				//return tex2D(_MainTex, i.pos);

				tex = tex2D(_MainTex, i.uv);
				nrmTex = tex2D(_NormalMap, i.uv);
				emTex = tex2D(_EmissiveMap, i.uv);
				palTex = tex2D(_PalletteMap, i.uv);

				if(palTex.r > 0.9f)
					colorToUse = _allColors[floor(i.uv1.x)];
				else if(palTex.r > 0.8f)
					colorToUse = _allColors[floor(i.uv1.y)];
				else if(palTex.r > 0.7f)
					colorToUse = _allColors[floor(i.uv2.x)];
				else if(palTex.r > 0.6f)
					colorToUse = _allColors[floor(i.uv2.y)];
				else if (palTex.r > 0.5f)
					colorToUse = _allColors[floor(i.uv3.x)];
				else if (palTex.r > 0.4f)
					colorToUse = _allColors[floor(i.uv3.y)];
				else if (palTex.r > 0.3f)
					colorToUse = _allColors[floor(i.vColor.r * 255)];
				else if (palTex.r > 0.2f)
					colorToUse = _allColors[floor(i.vColor.g * 255)];
				else if (palTex.r > 0.1f)
					colorToUse = _allColors[floor(i.vColor.b * 255)];
				else if (palTex.r > 0.0f)
					colorToUse = _allColors[floor(i.vColor.a * 255)];

				// normals: R (0->1 == down->up), A (0->1 == left->right)

				half2 gridUV = (ceil(i.worldPos) + 23.5) / 47.5;
				//anglesTex = tex2D(_Angles, gridUV);
				dotXsTex = tex2D(_DotXs, gridUV);
				dotYsTex = tex2D(_DotYs, gridUV);
				colorsTex = tex2D(_Colors, gridUV);
				rangesTex = tex2D(_Ranges, gridUV);
				distancesTex = tex2D(_Distances, gridUV);
				intensitiesTex = tex2D(_Intensities, gridUV);

				finalColor.rgb = (tex.rgb * colorToUse.rgb);
				
				if(!any(intensitiesTex))
					finalColor.rgba = 0;


				fixed mod = 0;
				fixed nrmUpDown = nrmTex.r * 2 - 1;
				fixed nrmLeftRight = nrmTex.a * 2 - 1;
				fixed dotX = dotXsTex.r * 2 - 1;
				fixed dotY = dotYsTex.r * 2 - 1;

				
				mod += 1 - saturate(abs((nrmLeftRight - dotX) + (nrmUpDown - dotY)));
				mod = dotXsTex.r; dwao // the dot appears to be slightly off when diagonal. Find out why! It's messing with the lighting >:c
				// same as above
				// mod = saturate(mod + (1 - round(abs(((nrmTex.a * 2) - 1) - ((dotXsTex.g * 2) - 1)))));
				// mod = saturate(mod + (1 - round(abs(((nrmTex.a * 2) - 1) - ((dotXsTex.b * 2) - 1)))));
				// mod = saturate(mod + (1 - round(abs(((nrmTex.a * 2) - 1) - ((dotXsTex.a * 2) - 1)))));
				finalColor = mod;
				finalColor.a = tex.a;
				return finalColor;

				// fixed mod = 0;

				// // find the dominant axis and convert that value from 0->1 to -1->1
				// fixed nrmVal = abs(0.5 - nrmTex.a) > abs(0.5 - nrmTex.r) ? (nrmTex.a * 2 + 1) : (nrmTex.r * 2 + 1);
				// fixed dotVal = abs(0.5 - dotXsTex.r) > abs(0.5 - dotYsTex.r) ? (dotXsTex.r * 2 + 1) : (dotYsTex.r * 2 + 1);

				// finalColor.rgb = abs(
				// 				nrmVal - // convert normal and find diff between normal and dot
				// 				dotVal // convert Dot from 0->1 to -1->1
				// 			);
				// finalColor.a = tex.a;
				// return finalColor;

				// mod = saturate( // clamp between 0-1 to prevent overexposure
				// 	mod + (
				// 		1 - round( // round diff and invert. Angles within 0.5 diff return 1, else return 0
				// 			abs(
				// 				((nrmTex.a * 2) - 1) - // convert normal and find diff between normal and dot
				// 				((dotVal * 2) - 1) // convert Dot from 0->1 to -1->1
				// 			)
				// 		)
				// 	)
				// );
				// // same as above
				// // mod = saturate(mod + (1 - round(abs(((nrmTex.a * 2) - 1) - ((dotXsTex.g * 2) - 1)))));
				// // mod = saturate(mod + (1 - round(abs(((nrmTex.a * 2) - 1) - ((dotXsTex.b * 2) - 1)))));
				// // mod = saturate(mod + (1 - round(abs(((nrmTex.a * 2) - 1) - ((dotXsTex.a * 2) - 1)))));
				// finalColor *= mod;
				// finalColor.a = tex.a;
				// return finalColor;



				// fixed mod = 0;
				// if(intensitiesTex.r > 0)
				// 	mod += round(abs(dotYsTex.r - nrmTex.r)) + round(abs(dotXsTex.r - nrmTex.a));
				// if(intensitiesTex.g > 0)
				// 	mod += round(abs(dotYsTex.g - nrmTex.r)) + round(abs(dotXsTex.g - nrmTex.a));
				// if(intensitiesTex.b > 0)
				// 	mod += round(abs(dotYsTex.b - nrmTex.r)) + round(abs(dotXsTex.b - nrmTex.a));
				// if(intensitiesTex.a > 0)
				// 	mod += round(abs(dotYsTex.a - nrmTex.r)) + round(abs(dotXsTex.a - nrmTex.a));
				
				// finalColor *= 1 - mod;
				// finalColor.a = tex.a;
				// return finalColor;
			}

			// struct Input {
			// 	float2 uv_MainTex;
			// 	float2 uv_Angles;
			// 	float2 uv_Colors;
			// 	float2 uv_Ranges;
			// 	float2 uv_Distances;
			// 	float2 uv_Intensities;
			// 	float2 uv_PalletteMap;
			// 	float2 uv_BumpMap;
			// 	float2 uv_EmissiveMap;
			// 	float4 vColor : COLOR; // interpolated vertex color
			// 	float2 uv2_MainTex2;
			// 	float2 uv3_MainTex3;
			// 	float2 uv4_MainTex4;
			// };

			// // I think the way the color works is that I set vertex-color to say which color index I want
			// // but for details on a tile, I set different UVs to say the same thing!
			// void surf (Input IN, inout SurfaceOutput o) {
			// 	tex = tex2D(_MainTex, IN.uv_MainTex);
			// 	emTex = tex2D(_EmissiveMap, IN.uv_EmissiveMap);
			// 	palTex = tex2D(_PalletteMap, IN.uv_PalletteMap);

			// 	if(palTex.r > 0.9f){
			// 		colorToUse = _allColors[floor(IN.uv2_MainTex2.x)];
			// 	}
			// 	else if(palTex.r > 0.8f){
			// 		colorToUse = _allColors[floor(IN.uv2_MainTex2.y)];
			// 	}
			// 	else if(palTex.r > 0.7f){
			// 		colorToUse = _allColors[floor(IN.uv3_MainTex3.x)];
			// 	}
			// 	else if(palTex.r > 0.6f){
			// 		colorToUse = _allColors[floor(IN.uv3_MainTex3.y)];
			// 	}
			// 	else if (palTex.r > 0.5f) {
			// 		colorToUse = _allColors[floor(IN.uv4_MainTex4.x)];
			// 	}
			// 	else if (palTex.r > 0.4f) {
			// 		colorToUse = _allColors[floor(IN.uv4_MainTex4.y)];
			// 	}
			// 	else if (palTex.r > 0.3f) {
			// 		colorToUse = _allColors[floor(IN.vColor.r * 255)];
			// 	}
			// 	else if (palTex.r > 0.2f) {
			// 		colorToUse = _allColors[floor(IN.vColor.g * 255)];
			// 	}
			// 	else if (palTex.r > 0.1f) {
			// 		colorToUse = _allColors[floor(IN.vColor.b * 255)];
			// 	}
			// 	else if (palTex.r > 0.0f) {
			// 		colorToUse = _allColors[floor(IN.vColor.a * 255)];
			// 	}

			// 	o.Albedo = (tex.rgb * _Color.rgb * colorToUse.rgb);
			// 	o.Alpha = tex.a * _Color.a;
			// 	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			// 	o.Emission = emTex.rgb;
			// }
			ENDCG
		}
	}
	//FallBack "Legacy Shaders/Transparent/VertexLit"
}