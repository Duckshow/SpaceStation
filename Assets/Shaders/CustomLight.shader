Shader "Custom/CustomLight" {
	Properties {
		// _Color ("Color", Color) = (1,1,1,1)
		// _ShadowColor ("Shadow Color", Color) = (1,1,1,1)
		//_UVScale ("UV Scale", float) = 1.0
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry-100" }
		// Stencil{
		// 	Ref 1
		// 	Comp gequal
		// 	Pass replace
		// }
		Pass {
			Cull Back
			ZWrite Off
			AlphaTest Off
			Lighting Off
			ColorMask A //RGBA

			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert

			struct appData { float4 vertex : POSITION; };

			struct v2f { float4 pos : POSITION; };

			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				return fixed4(0, 0, 0, 0);
			}

			ENDCG
		}
	}
}