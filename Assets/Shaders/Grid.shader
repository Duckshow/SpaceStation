// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grid" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainTex1("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex2("Bugfix (Don't assign)", 2D) = "white" {} 
		_MainTex3("Bugfix (Don't assign)", 2D) = "white" {}
		_DotXs("DotXs (Don't assign)", 2D) = "white" {}
		_DotYs("DotYs (Don't assign)", 2D) = "white" {}
		_NrmMap ("Abnormal", 2D) = "white" {}
		_PalletteMap ("Pallette", 2D) = "white" {}
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

		Pass {

			CGPROGRAM
			#pragma vertex vert 
			//#pragma alpha:fade
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NrmMap;
			sampler2D _PalletteMap;
			sampler2D _EmissiveMap;
			sampler2D _DotXs;
			sampler2D _DotYs;

			fixed _Emission;

			uniform fixed4 _allColors[128];
			fixed4 colorToUse;

			fixed4 tex;
			fixed4 nrmTex;
			fixed4 emTex;
			fixed4 palTex;
			fixed4 dotXsTex;
			fixed4 dotYsTex;

			fixed4 finalColor;


			struct appData {
				float4 vertex : POSITION;
				float4 vColor : COLOR; // vertex color
				float4 uv01 : TEXCOORD0;
				float4 uv23 : TEXCOORD1;
				float4 uv45 : TEXCOORD2;
				float4 angles : TEXCOORD3;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 worldPos : NORMAL;
				fixed4 vColor : COLOR;
				float4 uv01  : TEXCOORD0;
				float4 uv23 : TEXCOORD1;
				float4 uv45 : TEXCOORD2;
				float4 angles : TEXCOORD3;
			};

			uniform fixed colorIndices [10];
			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vColor = v.vColor;
				o.uv01 = v.uv01;
				o.uv23 = v.uv23;
				o.uv45 = v.uv45;
				o.angles = v.angles;
				return o;
			}

			// fixed4 CalculateLighting(fixed4 vColor, fixed4 normals, fixed dotX, fixed dotY){
			// 	return min(												// return the darkest; total lighting (set in CustomLight.cs) or the normal-dependant lighting
			// 		vColor, 												
			// 		1 - min(											// pick the smallest; floored Alpha channel or ceiled-nrm/equation. Unless A is 1, pixel will be unlit.
			// 				floor(normals.a),
			// 				min(										// pick the smallest; ceiled normals or light-equation. Unless normals are zero, equation wins.
			// 					ceil(									// ceil normals, so anything above 0 becomes 1 (thus equal to maximum lighting)
			// 						abs(normals.r) +					// add normals together
			// 						abs(normals.g)
			// 					),
			// 					saturate( 								// clamp01 so we don't get weird values and invert
			// 						floor( 								// floor bc we want diffs below 1 to be 0, so they get 100% lit, no falloff
			// 							0.01 + 							// tolerance (prevents some weirdness)
			// 							max( 							// max val tells us true diff
			// 								abs(
			// 									(normals.r * 2 - 1) - 	// get diff between nrm, dot
			// 									(dotX * 2 - 1) 			// convert nrm, dot from 0->1 to -1->1
			// 								), 
			// 								abs(
			// 									(normals.g * 2 - 1) - 
			// 									(dotY * 2 - 1)
			// 								)
			// 							)
			// 						)
			// 					)
			// 				)
			// 		)
						
			// 	);
			// }
			// fixed4 CalculateLighting(fixed4 vColor, fixed4 normals, fixed angle){
			// 	return min(															// return the darkest; total lighting (set in CustomLight.cs) or the normal-dependant lighting
			// 		vColor, 												
			// 		1 - min(														// pick the smallest; floored Alpha channel or ceiled-nrm/equation. Unless A is 1, pixel will be unlit.
			// 				floor(normals.a),
			// 				min(													// pick the smallest; ceiled normals or light-equation. Unless normals are zero, equation wins.
			// 					ceil(												// ceil normals, so anything above 0 becomes 1 (thus equal to maximum lighting)
			// 						abs(normals.r) +								// add normals together
			// 						abs(normals.g)
			// 					),
			// 					saturate( 											// clamp01 so we don't get weird values and invert
			// 						floor( 											// floor bc we want diffs below 1 to be 0, so they get 100% lit, no falloff
			// 							0.01 + 										// tolerance (prevents some weirdness)
			// 							max( 										// max val tells us true diff
			// 								abs(
			// 									(normals.r * 2 - 1) - 				// get diff between nrm, dot
			// 									(abs(angle - 0.25) * 2 * 2 - 1)	// angle rel to left (0.25), times 2 so 0->1 from left to right, times 2 so 0->2, minus 1 so -1->1
			// 								), 
			// 								abs(
			// 									(normals.g * 2 - 1) - 
			// 									(abs(angle) * 2 * 2 - 1)	// angle rel to bottom (0), times 2 so 0->1 from left to right, times 2 so 0->2, minus 1 so -1->1
			// 								)
			// 							)
			// 						)
			// 					)
			// 				)
			// 		)
						
			// 	);
			// }

			fixed4 CalculateLighting(fixed4 vColor, fixed4 normals, fixed angle){

				float dotX = 1 - 2 * abs((angle - 0.25) * 2 - 1); 	// the angle to the light expressed as 0->1, and only horizontally (0 == left)
				float dotY = 1 - 2 * abs(angle * 2 - 1); 			// the angle to the light expressed as 0->1, and only vertically (0 == bottom)

				return min(															// return the darkest; total lighting (set in CustomLight.cs) or the normal-dependant lighting
					vColor, 												
					1 - min(														// pick the smallest; floored Alpha channel or ceiled-nrm/equation. Unless A is 1, pixel will be unlit.
							floor(normals.a),
							min(													// pick the smallest; ceiled normals or light-equation. Unless normals are zero, equation wins.
								ceil(												// ceil normals, so anything above 0 becomes 1 (thus equal to maximum lighting)
									normals.r +										// add normals together
									normals.g
								),
								saturate( 											// clamp01 so we don't get weird values and invert
										//0.01 + 										// tolerance (prevents some weirdness)
										max( 										// max val tells us true diff
											abs((normals.r * 2 - 1) - dotY), 		// diff between surface-normal Y and vertical dot product to light
											abs((normals.g * 2 - 1) - dotX)			// diff between surface-normal X and horizontal dot product to light
										)
								)
							)
					)
						
				);
			}

			fixed4 frag(v2f i) : COLOR {
				tex = tex2D(_MainTex, i.uv01.xy);
				nrmTex = tex2D(_NrmMap, i.uv01.xy);
				emTex = tex2D(_EmissiveMap, i.uv01.xy);
				palTex = tex2D(_PalletteMap, i.uv01.xy);

				colorIndices[0] = floor(i.uv01.z);
				colorIndices[1] = floor(i.uv01.w);
				colorIndices[2] = floor(i.uv23.x);
				colorIndices[3] = floor(i.uv23.y);
				colorIndices[4] = floor(i.uv23.z);
				colorIndices[5] = floor(i.uv23.w);
				colorIndices[6] = floor(i.uv45.x);
				colorIndices[7] = floor(i.uv45.y);
				colorIndices[8] = floor(i.uv45.z);
				colorIndices[9] = floor(i.uv45.w);

				fixed indexToUse = 10 - floor(palTex.r * 10);
				colorToUse = _allColors[colorIndices[indexToUse]];

				// normals: R (0->1 == down->up), A (0->1 == left->right)

				// half2 gridUV = (ceil(i.worldPos) + 23.5) / 47.5;
				// dotXsTex = tex2D(_DotXs, gridUV);
				// dotYsTex = tex2D(_DotYs, gridUV);

				// fixed4 mod0 = CalculateLighting(i.vColor, nrmTex, dotXsTex.r, dotYsTex.r);
				// fixed4 mod1 = CalculateLighting(i.vColor, nrmTex, dotXsTex.g, dotYsTex.g);
				// fixed4 mod2 = CalculateLighting(i.vColor, nrmTex, dotXsTex.b, dotYsTex.b);
				// fixed4 mod3 = CalculateLighting(i.vColor, nrmTex, dotXsTex.a, dotYsTex.a);
				fixed4 mod0 = CalculateLighting(i.vColor, nrmTex, i.angles.r);
				fixed4 mod1 = CalculateLighting(i.vColor, nrmTex, i.angles.g);
				fixed4 mod2 = CalculateLighting(i.vColor, nrmTex, i.angles.b);
				fixed4 mod3 = CalculateLighting(i.vColor, nrmTex, i.angles.a);
				mod0 += mod1 + mod2 + mod3;


				float dotX = 1 - 2 * abs((i.angles.r - 0.25) * 2 - 1); 	// the angle to the light expressed as 0->1, and only horizontally (0 == left)
				float dotY = 1 - 2 * abs(i.angles.r * 2 - 1); 	

				i.angles.a = 1;
				i.angles.r = dotX;
				return i.angles.rrra;

				// final apply
				finalColor.rgb = min((tex.rgb * colorToUse.rgb) * mod0, i.vColor);
				finalColor.a = tex.a;
				return finalColor;
			}
			
			ENDCG
		}
	}
}