// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grid" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainTex_1("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex_2("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex_3("Bugfix (Don't assign)", 2D) = "white" {}
		_NrmMap ("Abnormal", 2D) = "white" {}
		_PalletteMap ("Pallette", 2D) = "white" {}
		_EmissiveMap ("Emissive", 2D) = "white" {}
		
		_ChemLiquidGasPlasmaTex_0 ("ChemLiquidGasPlasmaTex_0", 2D) = "white" {}
		_ChemLiquidGasPlasmaTex_1 ("ChemLiquidGasPlasmaTex_1", 2D) = "white" {}
		_ChemLiquidGasPlasmaTex_2 ("ChemLiquidGasPlasmaTex_2", 2D) = "white" {}
		_ChemSolidTex ("ChemSolidTex", 2D) = "white" {}

//		_ChemInfo ("ChemInfo (assigned in code)", 2D) = "white" {}
		_Emission ("Emission (Lightmapper)", Float) = 1.0
		_ChemScale ("ChemScale", Float) = 0.25
		_TimeScale ("TimeScale", Float) = 1.0
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
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
			#pragma vertex vert 
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NrmMap;
			sampler2D _PalletteMap;
			sampler2D _EmissiveMap;
			sampler2D _ChemLiquidGasPlasmaTex_0;
			sampler2D _ChemLiquidGasPlasmaTex_1;
			sampler2D _ChemLiquidGasPlasmaTex_2;
			sampler2D _ChemSolidTex;
//			sampler2D _ChemInfo;

			fixed _Emission;
			fixed _ChemScale;
			fixed _TimeScale;

			uniform fixed TextureSizeX;
			uniform fixed TextureSizeY;
			uniform float4 allColors[128];
			uniform fixed colorIndices [10];
			
			static const int2 GRID_SIZE = int2(48, 48);
			static const int CHEMICAL_COUNT = 1;

			uniform int allChemicalsColorIndexSolid[CHEMICAL_COUNT];
			uniform int allChemicalsColorIndexLiquid[CHEMICAL_COUNT];
			uniform int allChemicalsColorIndexGas[CHEMICAL_COUNT];
			uniform int allChemicalsFreezingPoint[CHEMICAL_COUNT];
			uniform int allChemicalsBoilingPoint[CHEMICAL_COUNT];

			fixed4 tex;
			fixed4 nrmTex;
			fixed4 emTex;
			fixed4 palTex;

			struct appData {
				float4 Vertex : POSITION;
				float4 VColor : COLOR; // lighting
				float3 UVAndChemicalAmount : TEXCOORD0; // uvs for tiles and test-value for chemicals
				float3 ColorIndices : TEXCOORD1; // compressed color indices for coloring tool
                float4 ChemInfo : TANGENT; // compressed amount, state and color for top 3 chems on vertex, and local temperature
				// NORMAL
			};
			struct v2f {
				float4 Pos : POSITION;
				float4 WorldPos : NORMAL;
				fixed4 VColor : COLOR;
				float2 UV  : TEXCOORD0;
				int3 ColorIndices0to2 : TEXCOORD1;
				int3 ColorIndices3to5 : TEXCOORD2;
				int3 ColorIndices6to8 : TEXCOORD3;
				fixed ChemicalAmount : TEXCOORD4;
				fixed4 ChemInfo : TEXCOORD5;
			};
			
			//-- VERT --//
			// float2 DecompressAssetCoordChannelToUV(int _channel){
			// 	return float2(
			// 		// 0xFFFF == 65535 == 1111111111111111 (16 bits)
			// 		(_channel 		& 0xFFFF) / TextureSizeX,
			// 		(_channel >> 16 & 0xFFFF) / TextureSizeY 
			// 	);
			// }
			int3 IntToByte3(int _channel){
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

				o.UV.xy = v.UVAndChemicalAmount.xy;
				o.ChemicalAmount = v.UVAndChemicalAmount.z;
				o.ColorIndices0to2 = IntToByte3(v.ColorIndices.x).xyz;
				o.ColorIndices3to5 = IntToByte3(v.ColorIndices.y).xyz;
				o.ColorIndices6to8 = IntToByte3(v.ColorIndices.z).xyz;
				o.ChemInfo = v.ChemInfo;
				return o;
			}

			fixed4 AddOrOverwriteColors(fixed4 _oldColor, fixed3 _newColor, fixed _newAlpha){
				// is this still relevant?
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
			fixed3 GetColorToolColorToUse(int _indexToUse, int3 _indices012, int3 _indices345, int3 _indices678){
				colorIndices[0] = floor(_indices012.x);
				colorIndices[1] = floor(_indices012.y);
				colorIndices[2] = floor(_indices012.z);
				colorIndices[3] = floor(_indices345.x);
				colorIndices[4] = floor(_indices345.y);
				colorIndices[5] = floor(_indices345.z);
				colorIndices[6] = floor(_indices678.x);
				colorIndices[7] = floor(_indices678.y);
				colorIndices[8] = floor(_indices678.z);

				return allColors[colorIndices[_indexToUse]];
			}
			float2 GetRoundedGridUV(float2 _worldPos){
				return floor(_worldPos + (GRID_SIZE * 0.5)) / GRID_SIZE;
			}
			void GetChemAmountStateAndColor(fixed _pixelChannel, out int _amount, out int _state, out fixed4 _color){
				int3 _amountStateColor = IntToByte3(_pixelChannel);
				_amount = _amountStateColor.r;
				_state = _amountStateColor.g;
				_color = allColors[_amountStateColor.b];
			}
			fixed4 frag(v2f i) : COLOR {
				TryApplyTextures(i.UV, tex, nrmTex, emTex, palTex);

				

				float2 _gridUV = GetRoundedGridUV(i.WorldPos);
//				fixed4 _chemInfo = tex2D(_ChemInfo, _gridUV);

				int _chemAmount0, _chemAmount1, _chemAmount2; 
				int _chemState0, _chemState1, _chemState2; 
				fixed4 _chemColor0, _chemColor1, _chemColor2;
				GetChemAmountStateAndColor(i.ChemInfo.r, _chemAmount0, _chemState0, _chemColor0);
				GetChemAmountStateAndColor(i.ChemInfo.g, _chemAmount1, _chemState1, _chemColor1);
				GetChemAmountStateAndColor(i.ChemInfo.b, _chemAmount2, _chemState2, _chemColor2);

				float _temperature = i.ChemInfo.a;


				// _____________________________________________________________________________________
				half _time = (_Time * _TimeScale) % 1.0;
				float2 _tileUV = (i.WorldPos * _ChemScale + GRID_SIZE * 0.5) % 1.0;
				fixed _pixelLiquidGasPlasma_0 = tex2D(_ChemLiquidGasPlasmaTex_0, _tileUV);
				fixed _pixelLiquidGasPlasma_1 = tex2D(_ChemLiquidGasPlasmaTex_1, _tileUV);
				fixed _pixelLiquidGasPlasma_2 = tex2D(_ChemLiquidGasPlasmaTex_2, _tileUV);
				fixed _pixelLiquidGasPlasmaFinal = _pixelLiquidGasPlasma_2;
				_pixelLiquidGasPlasmaFinal = lerp(_pixelLiquidGasPlasmaFinal, _pixelLiquidGasPlasma_0, max(0.0, _time - 0.0) / 0.33);
				_pixelLiquidGasPlasmaFinal = lerp(_pixelLiquidGasPlasmaFinal, _pixelLiquidGasPlasma_1, max(0.0, _time - 0.33) / 0.33);
				_pixelLiquidGasPlasmaFinal = lerp(_pixelLiquidGasPlasmaFinal, _pixelLiquidGasPlasma_2, max(0.0, _time - 0.66) / 0.33);
				
				fixed _pixelSolid = tex2D(_ChemSolidTex, _tileUV);

				fixed4 _pixelChem = _chemColor0;
				float _a = _chemAmount0 / 255.0;

				if(_chemState0 == 0) {
					_pixelChem *= round(_a * _pixelSolid) - (0.25 * _pixelSolid * round(_a * _pixelSolid));
				}
				else if(_chemState0 == 1) {
					_pixelChem *= step(_pixelLiquidGasPlasmaFinal, _a);
				}
				else if(_chemState0 == 2) {
					_pixelChem *= _a * pow(_pixelLiquidGasPlasmaFinal * (_a + 2.0) * 0.5, 2);
				}
				else {
					_pixelChem *= _a * pow(_pixelLiquidGasPlasmaFinal * (_a + 2.0) * 0.5, 2);
				}

				//_____________________________________________________________________________________
				return _pixelChem;
				


				fixed3 _combinedColors = tex.rgb * i.VColor.rgb;
				_combinedColors.rgb *= GetColorToolColorToUse(floor(palTex.r * 10.0), i.ColorIndices0to2, i.ColorIndices3to5, i.ColorIndices6to8);
				_combinedColors.rgb *= i.VColor.a;

				return fixed4(
					lerp(_combinedColors, emTex, emTex.a),
					tex.a
				);
			}
			
			ENDCG
		}
	}
}