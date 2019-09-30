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

		_Emission ("Emission (Lightmapper)", Float) = 1.0
		_ChemScale ("ChemScale", Float) = 0.25
		_TimeScale ("TimeScale", Float) = 1.0

		_AlphaSolid ("AlphaSolid ", Float) = 1.0
		_AlphaLiquid ("AlphaLiquid ", Float) = 1.0
		_AlphaGas ("AlphaGas ", Float) = 1.0
		_AlphaPlasma ("AlphaPlasma ", Float) = 1.0

		_ChemAmountsAndTemperatureTex ("ChemAmountsAndTemperature (assigned in code)", 2D) = "white" {}
		_ChemColorsTex ("ChemColorsTex (assigned in code)", 2D) = "white" {}
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

			sampler2D _ChemAmountsAndTemperatureTex;
			sampler2D _ChemColorsTex;
			sampler2D _ChemStatesTex;

			fixed _Emission;
			fixed _ChemScale;
			fixed _TimeScale;

			fixed _AlphaSolid;
			fixed _AlphaLiquid;
			fixed _AlphaGas;
			fixed _AlphaPlasma;

			static const int2 GRID_SIZE = int2(48, 48);
			static const int COLOR_COUNT = 128;
			static const int CHEMICAL_COUNT = 1;
			static const half STATE_TRANSITION_UP = 0.8;
			static const half STATE_TRANSITION_DOWN = 1.0 - STATE_TRANSITION_UP;
			static const half STATE_TRANSITION_LENGTH = STATE_TRANSITION_DOWN;
			
			uniform fixed TextureSizeX;
			uniform fixed TextureSizeY;
			uniform float4 allColors[COLOR_COUNT];
			uniform fixed colorIndices [10];

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
			
			fixed TryGetValueInterpolatedTowardsLowerState(fixed _valueLowerState, fixed _valueHigherState, fixed _stateDecimals){
			    fixed _progress = _stateDecimals * 2.0;
			    return lerp(_valueLowerState, _valueHigherState, _progress) * step(0.0, _progress) * step(_progress, 1.0);
			}
			
			fixed TryGetValueInterpolatedTowardsHigherState(fixed _valueLowerState, fixed _valueHigherState, fixed _stateDecimals){
			    fixed _progress = _stateDecimals * 2.0 - 1.0;
			    return lerp(_valueLowerState, _valueHigherState, _progress) * step(0.0, _progress) * step(_progress, 1.0);
			}
			
			fixed3 TryGetValueInterpolatedTowardsLowerState(fixed3 _valueLowerState, fixed3 _valueHigherState, fixed _stateDecimals){
			    return fixed3(
			        TryGetValueInterpolatedTowardsLowerState(_valueLowerState.x, _valueHigherState.x, _stateDecimals),
                    TryGetValueInterpolatedTowardsLowerState(_valueLowerState.y, _valueHigherState.y, _stateDecimals),
                    TryGetValueInterpolatedTowardsLowerState(_valueLowerState.z, _valueHigherState.z, _stateDecimals)
			    );
			}
			
			fixed3 TryGetValueInterpolatedTowardsHigherState(fixed3 _valueLowerState, fixed3 _valueHigherState, fixed _stateDecimals){
			    return fixed3(
			        TryGetValueInterpolatedTowardsHigherState(_valueLowerState.x, _valueHigherState.x, _stateDecimals),
                    TryGetValueInterpolatedTowardsHigherState(_valueLowerState.y, _valueHigherState.y, _stateDecimals),
                    TryGetValueInterpolatedTowardsHigherState(_valueLowerState.z, _valueHigherState.z, _stateDecimals)
			    );
			}
			
			fixed GetPixelIncadescense(fixed _temperature){
			    const half _tempMax = 10000.0;
			    const half _tempDraperPoint = 798.0; // 525C == 798K == Draper Point
			    const half _tempIncandescenseMax = 2000.0;
			    
			    fixed _temp = _temperature * _tempMax;
			    fixed _incandescense = (_temp - _tempDraperPoint) / (_tempIncandescenseMax - _tempDraperPoint);
			    return _incandescense * step(_tempDraperPoint, _temp);
			}
			
			fixed4 GetPixelForChem(fixed _chemAmount, fixed3 _chemColor, fixed _chemState, fixed _morphingPerlin, fixed _staticPerlin){
			    if(_chemAmount == 0) {
			        return 0;
			    }
			    
                fixed _chemStateSolid = _chemState - 0.0;
                fixed _chemStateLiquid = _chemState - 1.0;
                fixed _chemStateGas = _chemState - 2.0;
			
			    fixed _solidMod = floor(_staticPerlin + lerp(0.25, 1.0, _chemAmount * 1.0) * ceil(_chemAmount));
                fixed _liquidMod = floor(_staticPerlin + lerp(0.25, 1.0, _chemAmount * 2.0) * ceil(_chemAmount));
			    fixed _gasMod = (_morphingPerlin + _chemAmount) * 10.0 * _chemAmount;
			    
			    fixed _solidLiquidMod = lerp(_solidMod, _liquidMod, 0.5);
			    fixed _liquidGasMod = lerp(_liquidMod, _gasMod, 0.5);

				float _alpha = 0.0;
				_alpha += TryGetValueInterpolatedTowardsLowerState(_solidMod, _solidMod, _chemStateSolid);
				_alpha += TryGetValueInterpolatedTowardsHigherState(_solidMod, _solidLiquidMod, _chemStateSolid);
				
				_alpha += TryGetValueInterpolatedTowardsLowerState(_solidLiquidMod, _liquidMod, _chemStateLiquid);
				_alpha += TryGetValueInterpolatedTowardsHigherState(_liquidMod, _liquidGasMod, _chemStateLiquid);
				
				_alpha += TryGetValueInterpolatedTowardsLowerState(_liquidGasMod, _gasMod, _chemStateGas);
				_alpha += TryGetValueInterpolatedTowardsHigherState(_gasMod, 1.0, _chemStateGas);
				
                fixed3 _color = _chemColor + 0.25;
                _color += 0.25 * (_solidMod > 0.0);// step(0.0, _solidMod);
                _color += 0.0 * (_liquidMod > 0.0);// step(0.0, _liquidMod);
                _color += 0.5 * (_gasMod > 0.0);// step(0.0, _gasMod);
                
                
                daodaudu9w // continue debugging this - why is the final color always seemingly white?
				return fixed4(_gasMod, _gasMod, _gasMod, _gasMod);
			}
			
			fixed4 frag(v2f i) : COLOR { // TODO: I could possibly move some stuff to vert - using the UV I could probably reverse the interpolation!
				TryApplyTextures(i.UV, tex, nrmTex, emTex, palTex);

				float2 _nodeGridPosExact = i.WorldPos + GRID_SIZE * 0.5 + 0.5;
				
				int2 _nodeGridPos = floor(_nodeGridPosExact);
				float2 _nodeGridPosDecimals = _nodeGridPosExact - _nodeGridPos;

				float2 _gridMax = GRID_SIZE + float2(1.0, 1.0);
				float2 _nodeGridUVBL = (_nodeGridPos) / _gridMax;
				float2 _nodeGridUVTL = (_nodeGridPos + int2(0, 1)) / _gridMax;
				float2 _nodeGridUVTR = (_nodeGridPos + int2(1, 1)) / _gridMax;
				float2 _nodeGridUVBR = (_nodeGridPos + int2(1, 0)) / _gridMax;

				fixed4 _chemAmountsAndTemperatureBL = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVBL);
				fixed4 _chemAmountsAndTemperatureTL = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVTL);
				fixed4 _chemAmountsAndTemperatureTR = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVTR);
				fixed4 _chemAmountsAndTemperatureBR = tex2D(_ChemAmountsAndTemperatureTex, _nodeGridUVBR);

				fixed4 _lerpAmountsAndTemperatureBLToBR = lerp(_chemAmountsAndTemperatureBL, _chemAmountsAndTemperatureBR, _nodeGridPosDecimals.x);
				fixed4 _lerpAmountsAndTemperatureTLToTR = lerp(_chemAmountsAndTemperatureTL, _chemAmountsAndTemperatureTR, _nodeGridPosDecimals.x);

				fixed4 _chemAmountsAndTemperature = lerp(_lerpAmountsAndTemperatureBLToBR, _lerpAmountsAndTemperatureTLToTR, _nodeGridPosDecimals.y);
				
				fixed3 _chemStatesBL = tex2D(_ChemStatesTex, _nodeGridUVBL) * 3.0;
				fixed3 _chemStatesTL = tex2D(_ChemStatesTex, _nodeGridUVTL) * 3.0;
				fixed3 _chemStatesTR = tex2D(_ChemStatesTex, _nodeGridUVTR) * 3.0;
				fixed3 _chemStatesBR = tex2D(_ChemStatesTex, _nodeGridUVBR) * 3.0;

				fixed3 _chemStatesL = lerp(_chemStatesBL, _chemStatesTL, _nodeGridPosDecimals.y);
				fixed3 _chemStatesR = lerp(_chemStatesBR, _chemStatesTR, _nodeGridPosDecimals.y);
				fixed3 _chemStates = lerp(_chemStatesL, _chemStatesR, _nodeGridPosDecimals.x);
				
				int3 _chemColorIndicesBL = tex2D(_ChemColorsTex, _nodeGridUVBL) * (COLOR_COUNT - 1);
				int3 _chemColorIndicesTL = tex2D(_ChemColorsTex, _nodeGridUVTL) * (COLOR_COUNT - 1);
				int3 _chemColorIndicesTR = tex2D(_ChemColorsTex, _nodeGridUVTR) * (COLOR_COUNT - 1);
				int3 _chemColorIndicesBR = tex2D(_ChemColorsTex, _nodeGridUVBR) * (COLOR_COUNT - 1);
				
				int2 _nodeGridPosDecimalsRounded = round(_nodeGridPosDecimals);
				
				fixed3 _chemColor_0 = lerp(
				    lerp(allColors[_chemColorIndicesBL.x], allColors[_chemColorIndicesTL.x], allColors[_nodeGridPosDecimalsRounded.y]), 
				    lerp(allColors[_chemColorIndicesBR.x], allColors[_chemColorIndicesTR.x], allColors[_nodeGridPosDecimalsRounded.y]), 
				    _nodeGridPosDecimalsRounded.x
				);
				fixed3 _chemColor_1 = lerp(
				    lerp(allColors[_chemColorIndicesBL.y], allColors[_chemColorIndicesTL.y], allColors[_nodeGridPosDecimalsRounded.y]), 
				    lerp(allColors[_chemColorIndicesBR.y], allColors[_chemColorIndicesTR.y], allColors[_nodeGridPosDecimalsRounded.y]), 
				    _nodeGridPosDecimalsRounded.x
				);
				fixed3 _chemColor_2 = lerp(
				    lerp(allColors[_chemColorIndicesBL.z], allColors[_chemColorIndicesTL.z], allColors[_nodeGridPosDecimalsRounded.y]), 
				    lerp(allColors[_chemColorIndicesBR.z], allColors[_chemColorIndicesTR.z], allColors[_nodeGridPosDecimalsRounded.y]), 
				    _nodeGridPosDecimalsRounded.x
				);
				
				fixed _morphingPerlin;
				fixed _staticPerlin;
				GetMorphingPerlinSample(i.WorldPos, _morphingPerlin, _staticPerlin);
				
				fixed4 _pixelChem_0 = GetPixelForChem(_chemAmountsAndTemperature.r, _chemColor_0, _chemStates.r, _morphingPerlin, _staticPerlin);
				fixed4 _pixelChem_1 = GetPixelForChem(_chemAmountsAndTemperature.g, _chemColor_1, _chemStates.g, _morphingPerlin, _staticPerlin);
				fixed4 _pixelChem_2 = GetPixelForChem(_chemAmountsAndTemperature.b, _chemColor_2, _chemStates.b, _morphingPerlin, _staticPerlin);
                return _pixelChem_0;
                half _blending_01 = _chemAmountsAndTemperature.y / max(1.0, _chemAmountsAndTemperature.x);
                fixed4 _blendedPixelChem_01 = lerp(_pixelChem_0, _pixelChem_1, _blending_01);
                fixed _blendedState_01 = lerp(_chemStates.r, _chemStates.g, _blending_01);
                
                half _blending_012 = _chemAmountsAndTemperature.z / max(1.0, _chemAmountsAndTemperature.x + _chemAmountsAndTemperature.y);
                fixed4 _blendedPixelChem_012 = lerp(_blendedPixelChem_01, _pixelChem_2, _blending_012);
                fixed _blendedState_012 = lerp(_blendedState_01, _chemStates.b, _blending_012);
                
                
				fixed4 _finalPixel = tex;
				_finalPixel.rgb *= GetColorToolColorToUse(floor(palTex.r * 10.0), i.ColorIndices0to2, i.ColorIndices3to5, i.ColorIndices6to8); // coloring 
				_finalPixel.rgb = lerp(_finalPixel.rgb, _blendedPixelChem_012.rgb, _blendedPixelChem_012.a); // chems

//                _finalPixel.rgb *= i.VColor.rgb * i.VColor.a; // lighting
//				_finalPixel.rgb = lerp(_finalPixel.rgb, lerp(emTex.rgb, emTex.rgb * _blendedPixelChem_012.rgb, _blendedPixelChem_012.a), emTex.a); // emissive
//				
//				_finalPixel = clamp(_finalPixel, 0.0, 1.0);
//				_finalPixel.rgb = lerp(_finalPixel.rgb, 0.0, (1.0 - any(tex.rgb)) * 1.0);

                return _finalPixel;				
			}
			
			ENDCG
		}
	}
}