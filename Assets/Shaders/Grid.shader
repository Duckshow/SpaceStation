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
			uniform float4 allColors[128];
			uniform fixed colorIndices [10];

			fixed4 tex;
			fixed4 nrmTex;
			fixed4 emTex;
			fixed4 palTex;

			struct appData {
				float4 Vertex : POSITION;
				float4 VColor : COLOR; // vertex color
				float2 UV : TEXCOORD0;
				float3 ColorIndices : TEXCOORD1;
			};
			struct v2f {
				float4 Pos : POSITION;
				float4 WorldPos : NORMAL;
				fixed4 VColor : COLOR;
				float4 UV  : TEXCOORD0;
				int3 ColorIndices0to2 : TEXCOORD1;
				int3 ColorIndices3to5 : TEXCOORD2;
				int3 ColorIndices6to8 : TEXCOORD3;
			};
			
			//-- VERT --//
			// float2 DecompressAssetCoordChannelToUV(int _channel){
			// 	return float2(
			// 		// 0xFFFF == 65535 == 1111111111111111 (16 bits)
			// 		(_channel 		& 0xFFFF) / TextureSizeX,
			// 		(_channel >> 16 & 0xFFFF) / TextureSizeY 
			// 	);
			// }
			int3 DecompressColorIndices(int _channel){
				return int3(
					// 0xFF == 255 == 11111111 (8 bits)
					_channel 		& 0xFF,
					_channel >> 8 	& 0xFF, 
					_channel >> 16 	& 0xFF
				);
			}
			v2f vert(appData v) {
				v2f o;
				o.Pos = UnityObjectToClipPos(v.Vertex);
				o.WorldPos = mul(unity_ObjectToWorld, v.Vertex);
				o.VColor = v.VColor;

				o.UV.xy = v.UV;
				o.ColorIndices0to2 = DecompressColorIndices(v.ColorIndices.x);
				o.ColorIndices3to5 = DecompressColorIndices(v.ColorIndices.y);
				o.ColorIndices6to8 = DecompressColorIndices(v.ColorIndices.z);
				return o;
			}

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
				_tex = AddOrOverwriteColors(_tex, _sampleMainTex, 	_sampleMainTex.a);
				_pal = AddOrOverwriteColors(_pal, _samplePallette, 	_sampleMainTex.a);
				_nrm = AddOrOverwriteColors(_nrm, _sampleNormal, 	_sampleNormal.a);
				_emi = AddOrOverwriteColors(_emi, _sampleEmissive, 	_sampleEmissive.a);
			}
			fixed4 frag(v2f i) : COLOR {
				TryApplyTextures(i.UV, tex, nrmTex, emTex, palTex);

				colorIndices[0] = floor(i.ColorIndices0to2.x);
				colorIndices[1] = floor(i.ColorIndices0to2.y);
				colorIndices[2] = floor(i.ColorIndices0to2.z);
				colorIndices[3] = floor(i.ColorIndices3to5.x);
				colorIndices[4] = floor(i.ColorIndices3to5.y);
				colorIndices[5] = floor(i.ColorIndices3to5.z);
				colorIndices[6] = floor(i.ColorIndices6to8.x);
				colorIndices[7] = floor(i.ColorIndices6to8.y);
				colorIndices[8] = floor(i.ColorIndices6to8.z);


				int _indexToUse = floor(palTex.r * 10.0);
				fixed4 _colorToUse = allColors[colorIndices[_indexToUse]];

				fixed3 _combinedColors = tex.rgb * _colorToUse.rgb * i.VColor.rgb;
				_combinedColors.rgb *= i.VColor.a;

				return fixed4(
					// _indexToUse, 0, 0,
					// (colorIndices[_indexToUse] * colorIndices[_indexToUse]) / 128.0, 
					// 0, 0,
					// (_indexToUse / 10.0), 0, 0,
					// allColors[colorIndices[1]].rgb,
					lerp(_combinedColors, emTex, emTex.a),
					tex.a
				);
			}
			
			ENDCG
		}
	}
}