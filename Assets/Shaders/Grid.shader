// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grid" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_NrmMap ("Abnormal", 2D) = "white" {}
		_PalletteMap ("Pallette", 2D) = "white" {}
		_EmissiveMap ("Emissive", 2D) = "white" {}
		
		_PerlinTex_0 ("PerlinTex_0", 2D) = "white" {}
		_PerlinTex_1 ("PerlinTex_1", 2D) = "white" {}
		_PerlinTex_2 ("PerlinTex_2", 2D) = "white" {}
		_ChemSolidTex ("ChemSolidTex", 2D) = "white" {}

		_Emission ("Emission (Lightmapper)", Float) = 1.0
		_ChemScale ("ChemScale", Float) = 0.25
		_TimeScale ("TimeScale", Float) = 1.0

		_AlphaSolid ("AlphaSolid ", Float) = 1.0
		_AlphaLiquid ("AlphaLiquid ", Float) = 1.0
		_AlphaGas ("AlphaGas ", Float) = 1.0
		_AlphaPlasma ("AlphaPlasma ", Float) = 1.0

		_ChemAmountsAndTemperatureTex ("ChemAmountsAndTemperature (assigned in code)", 2D) = "white" {}
		_ChemColorsTex_0 ("ChemColorsTex_0 (assigned in code)", 2D) = "white" {}
		_ChemColorsTex_1 ("ChemColorsTex_1 (assigned in code)", 2D) = "white" {}
		_ChemColorsTex_2 ("ChemColorsTex_2 (assigned in code)", 2D) = "white" {}
		_DebugTex ("DebugTex (assigned in code)", 2D) = "white" {}
		_ChemStatesTex ("ChemStatesTex (assigned in code)", 2D) = "white" {}

        _MainTex_1("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex_2("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex_3("Bugfix (Don't assign)", 2D) = "white" {}
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
			sampler2D _PerlinTex_0;
			sampler2D _PerlinTex_1;
			sampler2D _PerlinTex_2;
			sampler2D _ChemSolidTex;

			sampler2D _ChemAmountsAndTemperatureTex;
			sampler2D _ChemColorsTex_0;
			sampler2D _ChemColorsTex_1;
			sampler2D _ChemColorsTex_2;
			sampler2D _DebugTex;
			sampler2D _ChemStatesTex;

			fixed _Emission;
			fixed _ChemScale;
			fixed _TimeScale;

			fixed _AlphaSolid;
			fixed _AlphaLiquid;
			fixed _AlphaGas;
			fixed _AlphaPlasma;

			uniform fixed TextureSizeX;
			uniform fixed TextureSizeY;
			uniform float4 allColors[128];
			uniform fixed colorIndices [10];
			
			static const int2 GRID_SIZE = int2(48, 48);
			static const int CHEMICAL_COUNT = 1;
			static const fixed STATE_TRANSITION_START = 0.9;

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
				float3 UV : TEXCOORD0; // uvs for tiles and test-value for chemicals
				float3 ColorIndices : TEXCOORD1; // compressed color indices for coloring tool
                // fixed4 ChemAmountsAndTemperature : TEXCOORD2; // amount for top 3 chems on vertex and local temperature
				// fixed3 ChemColorIndices : TEXCOORD3; // color indices for top 3 chems on vertex
				// fixed3 ChemStates : NORMAL; // states for top 3 chems on vertex
			};
			
			struct v2f {
				float4 Pos : POSITION;
				float4 WorldPos : NORMAL;
				fixed4 VColor : COLOR;
				float2 UV  : TEXCOORD0;
				int3 ColorIndices0to2 : TEXCOORD1;
				int3 ColorIndices3to5 : TEXCOORD2;
				int3 ColorIndices6to8 : TEXCOORD3;
				// fixed4 ChemAmountsAndTemperature : TEXCOORD4;
				// fixed3 ChemColor_0 : TEXCOORD5;
				// fixed3 ChemColor_1 : TEXCOORD6;
				// fixed3 ChemColor_2 : TEXCOORD7;
				// fixed3 ChemStates : TEXCOORD8;
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
			
			int4 Int16ToFourHalfByte4(int _i) {
				// 0xF == 15 == 1111 (4 bits)
				return int4(
				    _i >> 0  & 0xF,
                    _i >> 4  & 0xF,
                    _i >> 8  & 0xF,
                    _i >> 12 & 0xF
				);
            }
			
			v2f vert(appData v) {
				v2f o;
				o.Pos = UnityObjectToClipPos(v.Vertex);
				o.WorldPos = mul(unity_ObjectToWorld, v.Vertex);
				o.VColor = v.VColor;

				o.UV.xy = v.UV.xy;
				o.ColorIndices0to2 = IntToByte3(v.ColorIndices.x).xyz;
				o.ColorIndices3to5 = IntToByte3(v.ColorIndices.y).xyz;
				o.ColorIndices6to8 = IntToByte3(v.ColorIndices.z).xyz;
				
				// o.ChemAmountsAndTemperature = v.ChemAmountsAndTemperature;
                // o.ChemColor_0 = allColors[v.ChemColorIndices.r * 127.0];
                // o.ChemColor_1 = allColors[v.ChemColorIndices.g * 127.0];
                // o.ChemColor_2 = allColors[v.ChemColorIndices.b * 127.0];
				// o.ChemStates = v.ChemStates;

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
			
			void GetMorphingPerlinSample(float4 _worldPos, out fixed _morphingPerlin, out fixed _staticPerlin){
			    half _time = (_Time * _TimeScale) % 1.0;
			    
				float2 _tileUV = _worldPos * _ChemScale;
				fixed _perlinSample_0 = tex2D(_PerlinTex_0, _tileUV);
				fixed _perlinSample_1 = tex2D(_PerlinTex_1, _tileUV);
				fixed _perlinSample_2 = tex2D(_PerlinTex_2, _tileUV);
				
				_morphingPerlin = _perlinSample_2;
				_staticPerlin = _perlinSample_2;
				
				_morphingPerlin = lerp(_morphingPerlin, _perlinSample_0, max(0.0, _time - 0.0) / 0.33);
				_morphingPerlin = lerp(_morphingPerlin, _perlinSample_1, max(0.0, _time - 0.33) / 0.33);
				_morphingPerlin = lerp(_morphingPerlin, _perlinSample_2, max(0.0, _time - 0.66) / 0.33);
			}
			
			fixed GetStateAlpha(fixed _state){
			    if(_state < 1.0) { 
			        return _AlphaSolid;
			    }
			    else if(_state < 2.0) { 
			        return _AlphaLiquid;
			    }
			    else if(_state < 3.0) { 
			        return _AlphaGas;
			    }
			    else { 
			        return _AlphaPlasma;
			    }
			}
			
			fixed4 GetPixelForChem(fixed _chemAmount, fixed3 _chemColor, fixed _chemState, fixed4 _pixelSolid, fixed _morphingPerlin, fixed _staticPerlin){
			    fixed _solidMod = floor((_pixelSolid + _chemAmount) * 0.95);
			    fixed _liquidMod = floor(_staticPerlin + _chemAmount) * _chemAmount;
			    fixed _gasMod = lerp(_morphingPerlin * _chemAmount, 1.0, _chemAmount) * _chemAmount;
			    fixed _plasmaMod = lerp(floor(_morphingPerlin + _chemAmount), 1.0, _chemAmount) * _chemAmount;
			    
				float _isStateAbove1 = step(1.0, _chemState);
				float _isStateBelow1 = 1.0 - _isStateAbove1;

				float _isStateAbove2 = step(2.0, _chemState);
				float _isStateBelow2 = 1.0 - _isStateAbove2;

				float _isStateAbove3 = step(3.0, _chemState);
				float _isStateBelow3 = 1.0 - _isStateAbove3;

				fixed _chemStateFloored = floor(_chemState);

				float _alpha = 0.0;
                _alpha += lerp(_solidMod, _liquidMod, _chemStateFloored) * _isStateBelow1;
                _alpha += lerp(_liquidMod, _gasMod, _chemStateFloored - 1.0) * _isStateAbove1 * _isStateBelow2;
                _alpha += lerp(_gasMod, _plasmaMod, _chemStateFloored - 2.0) * _isStateAbove2;
                _alpha *= GetStateAlpha(_chemStateFloored);

				return fixed4(_chemColor.r, _chemColor.g, _chemColor.b, _alpha);
			}
			
			fixed3 GetProperlyFormattedChemStates(fixed3 _chemStates){
				return _chemStates; 
				const fixed _tolerance = 1 - STATE_TRANSITION_START;

			    fixed3 _newChemStates = floor(_chemStates);
			    fixed3 _diff = _chemStates - _newChemStates;
			    
			    _newChemStates.x += max(0.0, (_diff.x - STATE_TRANSITION_START) / _tolerance);
			    _newChemStates.y += max(0.0, (_diff.y - STATE_TRANSITION_START) / _tolerance);
			    _newChemStates.z += max(0.0, (_diff.z - STATE_TRANSITION_START) / _tolerance);
			    
			    return _newChemStates;
			}

			fixed4 frag(v2f i) : COLOR {
				TryApplyTextures(i.UV, tex, nrmTex, emTex, palTex);

				float2 _nodeGridPosExact = i.WorldPos + GRID_SIZE * 0.5 + 0.5;
				
				int2 _nodeGridPos = floor(_nodeGridPosExact);
				float2 _nodeGridPosDecimals = _nodeGridPosExact - _nodeGridPos;

				float2 _gridMax = GRID_SIZE + float2(1.0, 1.0);
				float2 _nodeGridUVBL = (_nodeGridPos) / _gridMax;
				float2 _nodeGridUVTL = (_nodeGridPos + int2(0, 1)) / _gridMax;
				float2 _nodeGridUVTR = (_nodeGridPos + int2(1, 1)) / _gridMax;
				float2 _nodeGridUVBR = (_nodeGridPos + int2(1, 0)) / _gridMax;
				// // return fixed4((_nodeGridUVTR.x - _nodeGridUVBL.x), (_nodeGridUVTR.y - _nodeGridUVBL.y), 0, 1);

				// return fixed4(_nodeGridPosDecimals.x, _nodeGridPosDecimals.y, 0, 1);

				// fixed _chemAmount_0 = i.ChemAmountsAndTemperature.r;
				// fixed _chemAmount_1 = i.ChemAmountsAndTemperature.g;
				// fixed _chemAmount_2 = i.ChemAmountsAndTemperature.b;
				// fixed _temperature = i.ChemAmountsAndTemperature.a;

				fixed4 _chemAmountsAndTemperatureBL = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVBL);
				fixed4 _chemAmountsAndTemperatureTL = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVTL);
				fixed4 _chemAmountsAndTemperatureTR = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVTR);
				fixed4 _chemAmountsAndTemperatureBR = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVBR);

				fixed4 _lerpAmountsAndTemperatureBLToBR = lerp(_chemAmountsAndTemperatureBL, _chemAmountsAndTemperatureBR, _nodeGridPosDecimals.x);
				fixed4 _lerpAmountsAndTemperatureTLToTR = lerp(_chemAmountsAndTemperatureTL, _chemAmountsAndTemperatureTR, _nodeGridPosDecimals.x);

				fixed4 _chemAmountsAndTemperature = lerp(_lerpAmountsAndTemperatureBLToBR, _lerpAmountsAndTemperatureTLToTR, _nodeGridPosDecimals.y);
				
				
//				return fixed4(_nodeGridUVBL.x, 0, _nodeGridUVBL.y, 1);
                return tex2D(_DebugTex, _nodeGridUVBL) + fixed4(_chemAmountsAndTemperature.r, 0, 0, 1);
//				return fixed4(_chemAmountsAndTemperature.r * 1.0, 0, 0.2, 1);

				
				// _chemAmountsAndTemperatureBL = lerp(_chemAmountsAndTemperatureBL, 0.0, _nodeGridPosDecimals.x);
				// _chemAmountsAndTemperatureTL = lerp(_chemAmountsAndTemperatureTL, 0.0, _nodeGridPosDecimals.x);
				// _chemAmountsAndTemperatureTR = lerp(0.0, _chemAmountsAndTemperatureTR, _nodeGridPosDecimals.x);
				// _chemAmountsAndTemperatureBR = lerp(0.0, _chemAmountsAndTemperatureBR, _nodeGridPosDecimals.x);
				// fixed4 _chemAmountsAndTemperatureL = lerp(_chemAmountsAndTemperatureBL, _chemAmountsAndTemperatureTL, _nodeGridPosDecimals.y);
				// fixed4 _chemAmountsAndTemperatureR = lerp(_chemAmountsAndTemperatureBR, _chemAmountsAndTemperatureTR, _nodeGridPosDecimals.y);
				// fixed4 _chemAmountsAndTemperature = (_chemAmountsAndTemperatureL + _chemAmountsAndTemperatureR) * 0.5;



                // fixed3 _chemStates = GetProperlyFormattedChemStates(i.ChemStates * 3);
				fixed3 _chemStatesBL = tex2D(_ChemStatesTex, _nodeGridUVBL) * 3.0;
				fixed3 _chemStatesTL = tex2D(_ChemStatesTex, _nodeGridUVTL) * 3.0;
				fixed3 _chemStatesTR = tex2D(_ChemStatesTex, _nodeGridUVTR) * 3.0;
				fixed3 _chemStatesBR = tex2D(_ChemStatesTex, _nodeGridUVBR) * 3.0;

				_chemStatesBL = lerp(_chemStatesBL, 0.0, _nodeGridPosDecimals.x);
				_chemStatesTL = lerp(_chemStatesTL, 0.0, _nodeGridPosDecimals.x);
				_chemStatesTR = lerp(0.0, _chemStatesTR, _nodeGridPosDecimals.x);
				_chemStatesBR = lerp(0.0, _chemStatesBR, _nodeGridPosDecimals.x);

				fixed3 _chemStatesL = lerp(_chemStatesBL, _chemStatesTL, _nodeGridPosDecimals.y);
				fixed3 _chemStatesR = lerp(_chemStatesBR, _chemStatesTR, _nodeGridPosDecimals.y);
				fixed3 _chemStates = GetProperlyFormattedChemStates((_chemStatesL + _chemStatesR) * 0.5);

				// return fixed4(lerp(_lerpStatesBLToBR, _lerpStatesTLToTR, _nodeGridPosDecimals.y).r, 0.0, 0.0, 1.0);

				// fixed3 _chemColor_0 = i.ChemColor_0; 
				// fixed3 _chemColor_1 = i.ChemColor_1; 
				// fixed3 _chemColor_2 = i.ChemColor_2;

				fixed3 _chemColorBL_0 = tex2D(_ChemColorsTex_0, _nodeGridUVBL);
				fixed3 _chemColorTL_0 = tex2D(_ChemColorsTex_0, _nodeGridUVTL);
				fixed3 _chemColorTR_0 = tex2D(_ChemColorsTex_0, _nodeGridUVTR);
				fixed3 _chemColorBR_0 = tex2D(_ChemColorsTex_0, _nodeGridUVBR);

				fixed3 _chemColorBL_1 = tex2D(_ChemColorsTex_1, _nodeGridUVBL);
				fixed3 _chemColorTL_1 = tex2D(_ChemColorsTex_1, _nodeGridUVTL);
				fixed3 _chemColorTR_1 = tex2D(_ChemColorsTex_1, _nodeGridUVTR);
				fixed3 _chemColorBR_1 = tex2D(_ChemColorsTex_1, _nodeGridUVBR);

				fixed3 _chemColorBL_2 = tex2D(_ChemColorsTex_2, _nodeGridUVBL);
				fixed3 _chemColorTL_2 = tex2D(_ChemColorsTex_2, _nodeGridUVTL);
				fixed3 _chemColorTR_2 = tex2D(_ChemColorsTex_2, _nodeGridUVTR);
				fixed3 _chemColorBR_2 = tex2D(_ChemColorsTex_2, _nodeGridUVBR);

				fixed3 _lerpColorBLToBR_0 = lerp(_chemColorBL_0, _chemColorBR_0, _nodeGridPosDecimals.x);
				fixed3 _lerpColorBLToBR_1 = lerp(_chemColorBL_1, _chemColorBR_1, _nodeGridPosDecimals.x);
				fixed3 _lerpColorBLToBR_2 = lerp(_chemColorBL_2, _chemColorBR_2, _nodeGridPosDecimals.x);
				
				fixed3 _lerpColorTLToTR_0 = lerp(_chemColorTL_0, _chemColorTR_0, _nodeGridPosDecimals.x);
				fixed3 _lerpColorTLToTR_1 = lerp(_chemColorTL_1, _chemColorTR_1, _nodeGridPosDecimals.x);
				fixed3 _lerpColorTLToTR_2 = lerp(_chemColorTL_2, _chemColorTR_2, _nodeGridPosDecimals.x);
				
				fixed3 _chemColor_0 = lerp(_lerpColorBLToBR_0, _lerpColorTLToTR_0, _nodeGridPosDecimals.y);
				fixed3 _chemColor_1 = lerp(_lerpColorBLToBR_1, _lerpColorTLToTR_1, _nodeGridPosDecimals.y);
				fixed3 _chemColor_2 = lerp(_lerpColorBLToBR_2, _lerpColorTLToTR_2, _nodeGridPosDecimals.y);

				// return fixed4(_chemStates.r / 3.0, 0, 0, 1);
				// return fixed4(_chemAmountsAndTemperature.a * 10.0, _, 1);
				// return fixed4(_chemColor_0.r, _chemColor_0.g, _chemColor_0.b, 1);

				fixed _morphingPerlin;
				fixed _staticPerlin;
				GetMorphingPerlinSample(i.WorldPos, _morphingPerlin, _staticPerlin);
				
				fixed _pixelSolid = tex2D(_ChemSolidTex, i.WorldPos);

				fixed4 _pixelChem_0 = GetPixelForChem(_chemAmountsAndTemperature.r, _chemColor_0, _chemStates.r, _pixelSolid, _morphingPerlin, _staticPerlin);
//				fixed4 _pixelChem_1 = GetPixelForChem(_chemAmount_1, _chemColor_1, _chemStates.g, _pixelSolid, _morphingPerlin, _staticPerlin);
//				fixed4 _pixelChem_2 = GetPixelForChem(_chemAmount_2, _chemColor_2, _chemStates.b, _pixelSolid, _morphingPerlin, _staticPerlin);

				// return fixed4(_chemAmountsAndTemperature.a * 100.0, 0, _chemStates.r / 3.0, _pixelChem_0.r);
//                return fixed4(round(_chemStates.r) / 3.0, 0, 0, 1);
//                return fixed4(_chemAmountsAndTemperature.a * 10.0, 0, 0, 1);
//                return fixed4(_chemAmountsAndTemperature.a * 10.0, 0, 0, 1);
                return _pixelChem_0;

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