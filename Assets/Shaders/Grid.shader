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
				float2 uv : TEXCOORD0;
				float4 uv12 : TEXCOORD1;
				float4 uv34 : TEXCOORD2;
				float2 uv5 : TEXCOORD3;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 worldPos : NORMAL;
				fixed4 vColor : COLOR;
				float2 uv  : TEXCOORD0;
				float4 uv12 : TEXCOORD1;
				float4 uv34 : TEXCOORD2;
				float2 uv5 : TEXCOORD3;
			};

			uniform fixed colorIndices [10];
			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vColor = v.vColor;
				o.uv = float2(v.uv.x, v.uv.y);
				o.uv12 = float4(v.uv12.x, v.uv12.y, v.uv12.z, v.uv12.w);
				o.uv34 = float4(v.uv34.x, v.uv34.y, v.uv34.z, v.uv34.w);
				o.uv5 = float2(v.uv5.x, v.uv5.y);
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				tex = tex2D(_MainTex, i.uv);
				nrmTex = tex2D(_NrmMap, i.uv);
				emTex = tex2D(_EmissiveMap, i.uv);
				palTex = tex2D(_PalletteMap, i.uv);

				return i.vColor;

				colorIndices[0] = floor(i.uv12.x);
				colorIndices[1] = floor(i.uv12.y);
				colorIndices[2] = floor(i.uv12.z);
				colorIndices[3] = floor(i.uv12.w);
				colorIndices[4] = floor(i.uv34.x);
				colorIndices[5] = floor(i.uv34.y);
				colorIndices[6] = floor(i.uv34.z);
				colorIndices[7] = floor(i.uv34.w);
				colorIndices[8] = floor(i.uv5.x);
				colorIndices[9] = floor(i.uv5.y);

				fixed indexToUse = 10 - floor(palTex.r * 10);
				colorToUse = _allColors[colorIndices[indexToUse]];

				// normals: R (0->1 == down->up), A (0->1 == left->right)

				half2 gridUV = (ceil(i.worldPos) + 23.5) / 47.5;
				dotXsTex = tex2D(_DotXs, gridUV);
				dotYsTex = tex2D(_DotYs, gridUV);
				fixed4 mod0 = 
				max(0, 															// make sure it's over zero (not sure how, but that happens >.>)
					i.vColor * (												// multiply with the vertex color (total lighting color set in CustomLight.cs)
						1 - min(												// pick the smallest; floored Alpha channel or ceiled-nrm/equation. Unless A is 1, pixel will be unlit.
								floor(nrmTex.a),
								min(											// pick the smallest; ceiled normals or light-equation. Unless normals are zero, equation wins.
									ceil(										// ceil normals, so anything above 0 becomes 1 (thus equal to maximum lighting)
										abs(nrmTex.r) +							// add normals together
										abs(nrmTex.g)
									),
									saturate( 									// clamp01 so we don't get weird values and invert
										floor( 									// floor bc we want diffs below 1 to be 0, so they get 100% lit, no falloff
											0.01 + 								// tolerance (prevents some weirdness)
											max( 								// max val tells us true diff
												abs(
													(nrmTex.r * 2 - 1) - 		// get diff between nrm, dot
													(dotXsTex.r * 2 - 1) 		// convert nrm, dot from 0->1 to -1->1
												), 
												abs(
													(nrmTex.g * 2 - 1) - 
													(dotYsTex.r * 2 - 1)
												)
											)
										)
									)
								)
							)
					)
				);
				fixed4 mod1 = max(0, i.vColor * (1 - min(floor(nrmTex.a), min(ceil(abs(nrmTex.r) + abs(nrmTex.g)), saturate(floor(0.1 + max(abs((nrmTex.r * 2 - 1) - (dotXsTex.g * 2 - 1)), abs((nrmTex.g * 2 - 1) - (dotYsTex.g * 2 - 1))))))))); 
				fixed4 mod2 = max(0, i.vColor * (1 - min(floor(nrmTex.a), min(ceil(abs(nrmTex.r) + abs(nrmTex.g)), saturate(floor(0.1 + max(abs((nrmTex.r * 2 - 1) - (dotXsTex.b * 2 - 1)), abs((nrmTex.g * 2 - 1) - (dotYsTex.b * 2 - 1))))))))); 
				fixed4 mod3 = max(0, i.vColor * (1 - min(floor(nrmTex.a), min(ceil(abs(nrmTex.r) + abs(nrmTex.g)), saturate(floor(0.1 + max(abs((nrmTex.r * 2 - 1) - (dotXsTex.a * 2 - 1)), abs((nrmTex.g * 2 - 1) - (dotYsTex.a * 2 - 1))))))))); 


				// mush together
				mod0 += mod1 + mod2 + mod3;

				// final apply
				finalColor.rgb = (tex.rgb * colorToUse.rgb) * mod0;
				finalColor.a = tex.a;
				return finalColor;
			}
			ENDCG
		}
	}
}