Shader "Custom/CustomLight" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ShadowColor ("Shadow Color", Color) = (1,1,1,1)
		_UVScale ("UV Scale", float) = 1.0
	}

	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+50" }

		// shared grabpass (only runs once, so no changes)
		GrabPass{"_OriginalTexture"}

		// individual grabpass (current state of pixels)
		GrabPass{ }

		// mark these pixels as Ref 2 to mask ShadowQuad (Ref 1)
		Stencil{
			Ref 2
			Comp gequal
			Pass replace
		}
		Pass {
			Cull Off
			ZTest LEqual
			ZWrite On
			AlphaTest Off
			Lighting Off
			ColorMask RGBA
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"		//Include Unity's predefined inputs and macros

			//uniform sampler2D _MainTex;					//Define _MainTex from Texture Unit 0 to be sampled in 2D
			//uniform float4 _MainTex_ST;					//Use the Float _MainTex_ST to pass the Offset and Tiling for the texture(s)
			uniform fixed4 _Color;
			uniform fixed4 _ShadowColor;
			uniform float _UVScale;

		
			struct appData {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
				float4 grabPos : TEXCOORD1;
			};

			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = half2(v.texcoord.x, v.texcoord.y) * _UVScale;
				o.grabPos = ComputeGrabScreenPos(o.pos);
				return o;
			}

			sampler2D _OriginalTexture;
			sampler2D _GrabTexture;
			fixed4 frag(v2f i) : COLOR {
				// original pixel color
				fixed4 org = tex2Dproj(_OriginalTexture, i.grabPos);
				return org;


				
				fixed4 orgWithColor = (org + _Color) * 0.5;
				
				// current pixel color
				fixed4 col = tex2Dproj(_GrabTexture, i.grabPos);
				
				// pixel as lit by this light only
				fixed4 val = lerp(orgWithColor, _ShadowColor, sqrt((i.uv.x * i.uv.x) + (i.uv.y * i.uv.y)));

				// if pixel hasn't changed yet, use val
				if(!any(org - col))
					return val;

				// else add this light to existing light, clamped to avoid overexposure
				return min(col + (max(val - col, 0)), org);
			}

			ENDCG
		}
	}
}