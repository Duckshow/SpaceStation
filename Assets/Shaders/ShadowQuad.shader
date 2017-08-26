Shader "Custom/ShadowQuad" {
	Properties {
		_Color ("Color", Color) = (1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+100" }
		Color [_Color]
		
		Stencil {
			Ref 1
			Comp Equal
		}
		Pass{
			/*Cull Back
			ZWrite Off
			AlphaTest Off
			Lighting Off
			ColorMask 0*/
		}

		//GrabPass {  }

		//Stencil {
		//	Ref 2
		//	Comp Equal
		//}
		//Pass {
		//	Cull Back
		//	ZTest LEqual
		//	ZWrite On
		//	AlphaTest Off
		//	Lighting Off
		//	ColorMask RGBA
		//	Blend SrcAlpha OneMinusSrcAlpha

		//	CGPROGRAM
		//	#pragma fragment frag
		//	#pragma vertex vert
		//	#include "UnityCG.cginc"		//Include Unity's predefined inputs and macros

		//	uniform fixed4 _Color;
		//
		//	struct appData {
		//		float4 vertex : POSITION;
		//		half2 texcoord : TEXCOORD0;
		//	};

		//	struct v2f {
		//		float4 pos : POSITION;
		//		half2 uv : TEXCOORD0;
		//		float4 grabPos : TEXCOORD1;
		//	};

		//	v2f vert(appData v) {
		//		v2f o;
		//		o.pos = UnityObjectToClipPos(v.vertex);
		//		o.uv = half2(v.texcoord.x, v.texcoord.y);
		//		o.grabPos = ComputeGrabScreenPos(o.pos);
		//		return o;
		//	}

		//	sampler2D _GrabTexture;
		//	fixed4 frag(v2f i) : COLOR {

		//		//float4 cellSize = float4(1, 1, 1, 1) * _ScreenParams.xyxy;
		//		float4 steppedUV = i.grabPos;// = i.grabPos.xy/i.grabPos.w;
		//		//steppedUV /= cellSize;
		//		float4 divUV = steppedUV;
		//		steppedUV = round(steppedUV) - 1;
		//		//float4 steppedUV2 = divUV + ((1 - (steppedUV - divUV)) * -sign(steppedUV - divUV));

		//		float4 steppedUV2 = steppedUV + 1;

		//		//float4 steppedUV2 = divUV - round(steppedUV - divUV);
		//		//steppedUV *= cellSize;
		//		//steppedUV2 *= cellSize;



		//		//float4 newGrabPos = (i.grabPos * 10000) * 0.001;
		//		//fixed4 pxl = tex2Dproj(_GrabTexture, newGrabPos);

		//		//return fixed4(newGrabPos.x, newGrabPos.y, 1, 1);
		//		return tex2Dproj(_GrabTexture, lerp(steppedUV2, steppedUV, steppedUV - divUV));

		//	}

		//	ENDCG
		//}
	}
}