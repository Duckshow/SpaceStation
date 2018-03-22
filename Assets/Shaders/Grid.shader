// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grid" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainTex1("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex2("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex3("Bugfix (Don't assign)", 2D) = "white" {}
		_NrmMap ("Abnormal", 2D) = "white" {}
		_PalletteMap ("Pallette", 2D) = "white" {}
		_EmissiveMap ("Emissive", 2D) = "white" {}
		_Emission ("Emission (Lightmapper)", Float) = 1.0
	}

	SubShader {
		Tags {"IgnoreProjector"="True" "RenderType"="Transparent" "Queue"="Geometry" } 
		Cull Back
		Lighting Off
		ZWrite Off
		ColorMask RGBA
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NrmMap;
			sampler2D _PalletteMap;
			sampler2D _EmissiveMap;

			fixed _Emission;

			uniform fixed TextureSizeX;
			uniform fixed TextureSizeY;
			uniform fixed DotPrecision;
			uniform fixed4 allColors[128];
			uniform fixed colorIndices [10];

			fixed4 tex;
			fixed4 nrmTex;
			fixed4 emTex;
			fixed4 palTex;

			struct appData {
				float4 Vertex : POSITION;
				float4 VColor : COLOR; // vertex color
				float4 AssetCoord_0123 : TEXCOORD0;
				float4 AssetCoord_4 : TEXCOORD1;
				float4 ColorIndices : TEXCOORD2;
				float4 DoubleDots : TEXCOORD3;
			};
			struct v2f {
				float4 Pos : POSITION;
				float4 WorldPos : NORMAL;
				fixed4 VColor : COLOR;
				float4 UV01  : TEXCOORD0;
				float4 UV23  : TEXCOORD1;
				float2 UV4  : TEXCOORD2;
				float4 ColorIndices0to3 : TEXCOORD3;
				float4 ColorIndices4to7 : TEXCOORD4;
				float4 ColorIndices8to9 : TEXCOORD5;
				float4 DotXs : TEXCOORD6;
				float4 DotYs : TEXCOORD7;
			};
			
			//-- VERT --//
			float2 DecompressAssetCoordChannelToUV(int _channel){
				return float2(
					// 0xFFFF == 65535 == 1111111111111111 (16 bits)
					(_channel 		& 0xFFFF) / TextureSizeX,
					(_channel >> 16 & 0xFFFF) / TextureSizeY 
				);
			}
			float4 DecompressColorIndices(int _channel){
				return float4(
					// 0xFF == 255 == 11111111 (8 bits)
					_channel 		& 0xFF,
					_channel >> 8 	& 0xFF, 
					_channel >> 16 	& 0xFF, 
					_channel >> 24 	& 0xFF
				);
			}
			float4 DecompressDoubleDotsToDotXs(int4 _doubleDots){
				float _precision = 1 / DotPrecision;
				return float4(
					// 0xFFFF == 65535 == 1111111111111111 (16 bits)
					(_doubleDots.x & 0xFFFF) * _precision,
					(_doubleDots.y & 0xFFFF) * _precision, 
					(_doubleDots.z & 0xFFFF) * _precision, 
					(_doubleDots.w & 0xFFFF) * _precision
				);
			}
			float4 DecompressDoubleDotsToDotYs(int4 _doubleDots){
				float _precision = 1 / DotPrecision;
				return float4(
					// 0xFFFF == 65535 == 1111111111111111 (16 bits)
					(_doubleDots.x >> 16 & 0xFFFF) * _precision, 
					(_doubleDots.y >> 16 & 0xFFFF) * _precision, 
					(_doubleDots.z >> 16 & 0xFFFF) * _precision, 
					(_doubleDots.w >> 16 & 0xFFFF) * _precision
				);
			}
			v2f vert(appData v) {
				v2f o;
				o.Pos = UnityObjectToClipPos(v.Vertex);
				o.WorldPos = mul(unity_ObjectToWorld, v.Vertex);
				o.VColor = v.VColor;

				o.UV01.xy = DecompressAssetCoordChannelToUV(v.AssetCoord_0123.x);
				o.UV01.zw = DecompressAssetCoordChannelToUV(v.AssetCoord_0123.y);
				o.UV23.xy = DecompressAssetCoordChannelToUV(v.AssetCoord_0123.z);
				o.UV23.zw = DecompressAssetCoordChannelToUV(v.AssetCoord_0123.w);
				o.UV4.xy  = DecompressAssetCoordChannelToUV(v.AssetCoord_4.x);
				o.ColorIndices0to3 = DecompressColorIndices(v.ColorIndices.x);
				o.ColorIndices4to7 = DecompressColorIndices(v.ColorIndices.y);
				o.ColorIndices8to9 = DecompressColorIndices(v.ColorIndices.z);
				o.DotXs = DecompressDoubleDotsToDotXs(v.DoubleDots);
				o.DotYs = DecompressDoubleDotsToDotYs(v.DoubleDots);
				return o;
			}

			//-- FRAG --//
			fixed CalculateLighting(fixed4 _normals, fixed _dotX, fixed _dotY){
				fixed _mostlyY 		= round(abs(_normals.g * 2 - 1));						// 0 == x, 1 == y
				
				fixed _normal 		= lerp(_normals.r, _normals.g, _mostlyY);				// 0 == B/L, 1 == T/R
				fixed _dot 			= lerp(_dotX, _dotY, _mostlyY);							// 0 == heading B/L, 1 == heading T/R
				fixed _hitStrength 	= 1 - round(abs(_dot - _normal));						// 0 == not hit, 1 == hit
				
				fixed _lightExists 		= saturate(ceil(abs(_dotX + _dotY))); 				// 0 for both dots == not lit
				fixed _surfaceIsFlat 	= 1 - saturate(ceil(_normals.r + _normals.g));		// 0 == bumpy & lit, 1 == flat & unlit

				return max(_hitStrength, _surfaceIsFlat) * _lightExists;
			}
			// fixed LightExists(fixed _dotX, fixed _dotY){
			// 	return saturate(ceil(abs(_dotX + _dotY))); // 0 for both dots == not lit
			// }
			fixed IsUnlit(fixed4 _normals){
				return 1 - floor(_normals.a); // 0 == lit, 1 == unlit
			}
			// fixed IsFlatSurface(fixed4 _normals){
			// 	return 1 - saturate(ceil(_normals.r + _normals.g));	// 0 == bumpy & lit, 1 == flat & unlit
			// }
			fixed4 AddOrOverwriteColors(fixed4 _oldColor, fixed3 _newColor, fixed _newAlpha){
				return fixed4(
					_oldColor.rgb * (1 - _newAlpha) + _newColor.rgb * _newAlpha, 
					_oldColor.a + _newAlpha
				);
			}
			void TryApplyTextures(fixed2 _uv, inout fixed4 _tex, inout fixed4 _nrm, inout fixed4 _emi, inout fixed4 _pal){
				fixed4 _sampleMainTex = tex2D(_MainTex, _uv);
				fixed4 _samplePallette = tex2D(_PalletteMap, _uv);
				fixed4 _sampleNormal = tex2D(_NrmMap, _uv);
				fixed4 _sampleEmissive = tex2D(_EmissiveMap, _uv);
				_tex = AddOrOverwriteColors(_tex, _sampleMainTex, 				_sampleMainTex.a);
				_pal = AddOrOverwriteColors(_pal, _samplePallette, 				_sampleMainTex.a);
				_nrm = AddOrOverwriteColors(_nrm, _sampleNormal, 				_sampleNormal.a);
				_emi = AddOrOverwriteColors(_emi, _sampleEmissive, 				_sampleEmissive.a);
			}
			fixed4 frag(v2f i) : COLOR {
				TryApplyTextures(i.UV01.xy, tex, nrmTex, emTex, palTex); // floor
				TryApplyTextures(i.UV01.zw, tex, nrmTex, emTex, palTex); // floor corners
				TryApplyTextures(i.UV23.xy, tex, nrmTex, emTex, palTex); // wall
				TryApplyTextures(i.UV23.zw, tex, nrmTex, emTex, palTex); // top
				TryApplyTextures(i.UV4.xy, 	tex, nrmTex, emTex, palTex); // top corners

				colorIndices[0] = floor(i.ColorIndices0to3.x);
				colorIndices[1] = floor(i.ColorIndices0to3.y);
				colorIndices[2] = floor(i.ColorIndices0to3.z);
				colorIndices[3] = floor(i.ColorIndices0to3.w);
				colorIndices[4] = floor(i.ColorIndices4to7.x);
				colorIndices[5] = floor(i.ColorIndices4to7.y);
				colorIndices[6] = floor(i.ColorIndices4to7.z);
				colorIndices[7] = floor(i.ColorIndices4to7.w);
				colorIndices[8] = floor(i.ColorIndices8to9.x);
				colorIndices[9] = floor(i.ColorIndices8to9.y);

				fixed _indexToUse = 10 - ceil(palTex.r * 10);
				fixed4 _colorToUse = allColors[colorIndices[_indexToUse]];
				// fixed4 _hitMod = fixed4(//) saturate(
				// 	CalculateLighting(nrmTex, i.DotXs.r, i.DotYs.r),// + 
				// 	CalculateLighting(nrmTex, i.DotXs.g, i.DotYs.g),// + 
				// 	CalculateLighting(nrmTex, i.DotXs.b, i.DotYs.b),// + 
				// 	CalculateLighting(nrmTex, i.DotXs.a, i.DotYs.a)
				// );
				fixed4 _hitMod = saturate(
					CalculateLighting(nrmTex, i.DotXs.r, i.DotYs.r) + 
					CalculateLighting(nrmTex, i.DotXs.g, i.DotYs.g) + 
					CalculateLighting(nrmTex, i.DotXs.b, i.DotYs.b) + 
					CalculateLighting(nrmTex, i.DotXs.a, i.DotYs.a)
				);

				// _hitMod.r = CalculateLighting(nrmTex, i.DotXs.r, i.DotYs.r);
				// _hitMod.g = CalculateLighting(nrmTex, i.DotXs.g, i.DotYs.g);
				// _hitMod.b = CalculateLighting(nrmTex, i.DotXs.b, i.DotYs.b);


				// final apply
				// fixed3 _light = max(_hitMod, IsFlatSurface(nrmTex));
				// _light *= i.VColor;
				fixed3 _light = min(_hitMod, i.VColor);
				_light = max(_light, IsUnlit(nrmTex));

				fixed3 _litRGB = tex.rgb * _colorToUse.rgb * _light;
				//_litRGB = _hitMod;
				//_litRGB = i.VColor;
				return fixed4(
					lerp(_litRGB, emTex, emTex.a),
					tex.a
				);
			}
			
			ENDCG
		}
	}
}