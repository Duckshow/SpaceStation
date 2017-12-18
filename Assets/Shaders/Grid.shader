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
				float4 doubleDots : TEXCOORD3;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 worldPos : NORMAL;
				fixed4 vColor : COLOR;
				float4 uv01  : TEXCOORD0;
				float4 uv23 : TEXCOORD1;
				float4 uv45 : TEXCOORD2;
				float4 dotXs : TEXCOORD3;
				float4 dotYs : TEXCOORD4;
			};

			float4 ConvertDoubleDotsToDotXs(int4 doubleDots){
				return float4(
					(doubleDots.x & 0xFFFF) * 0.001, 
					(doubleDots.y & 0xFFFF) * 0.001, 
					(doubleDots.z & 0xFFFF) * 0.001, 
					(doubleDots.w & 0xFFFF) * 0.001
				);
			}
			float4 ConvertDoubleDotsToDotYs(int4 doubleDots){
				return float4(
					((doubleDots.x) >> 16 & 0xFFFF) * 0.001, 
					((doubleDots.y) >> 16 & 0xFFFF) * 0.001, 
					((doubleDots.z) >> 16 & 0xFFFF) * 0.001, 
					((doubleDots.w) >> 16 & 0xFFFF) * 0.001
				);
			}

			uniform fixed colorIndices [10];
			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vColor = v.vColor;
				o.uv01 = v.uv01;
				o.uv23 = v.uv23;
				o.uv45 = v.uv45;

				float4 dx = ConvertDoubleDotsToDotXs(v.doubleDots);
				float4 dy = ConvertDoubleDotsToDotYs(v.doubleDots);
				o.dotXs = (dx * 2 - 1) * saturate(ceil(dx)); // convert 0->1 to -1->1 and multiply so that unless dx > 0, this becomes 0
				o.dotYs = (dy * 2 - 1) * saturate(ceil(dy));
				return o;
			}

			fixed4 CalculateLighting(fixed4 vColor, fixed4 normals, fixed dotX, fixed dotY){
				return min(															// return the darkest; total lighting (set in CustomLight.cs) or the normal-dependant lighting
					vColor, 												
					1 - min(
							floor(normals.a),
							min(													// pick the smallest; ceiled normals or light-equation. Unless normals are zero, equation wins.
								ceil(normals.r + normals.g),						// ceil normals, so anything above 0 becomes 1 (thus equal to maximum lighting)
								floor(saturate( 											// clamp01 so we don't get weird values and invert
										max( 										// max val tells us true diff
											abs((normals.r * 2 - 1) - dotX), 		// diff between surface-normal X and horizontal dot product to light
											abs((normals.g * 2 - 1) - dotY)			// diff between surface-normal Y and vertical dot product to light
										)
								))
							)
					)
						
				) * saturate(ceil(abs(dotX + dotY))); 								// unless dotX and dotY forced to zero (otherwise impossible), this equals 1
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

				fixed4 mod0 = CalculateLighting(i.vColor, nrmTex, i.dotXs.r, i.dotYs.r);
				fixed4 mod1 = CalculateLighting(i.vColor, nrmTex, i.dotXs.g, i.dotYs.g);
				fixed4 mod2 = CalculateLighting(i.vColor, nrmTex, i.dotXs.b, i.dotYs.b);
				fixed4 mod3 = CalculateLighting(i.vColor, nrmTex, i.dotXs.a, i.dotYs.a);
				mod0 += mod1 + mod2 + mod3;

				// final apply
				finalColor.rgb = min((tex.rgb * colorToUse.rgb) * mod0, i.vColor);
				finalColor.a = tex.a;
				return finalColor;
			}
			
			ENDCG
		}
	}
}