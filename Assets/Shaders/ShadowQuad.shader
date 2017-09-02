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
			Cull Back
			ZTest LEqual
			ZWrite On
			AlphaTest Off
			Lighting Off
			ColorMask RGBA
			Blend SrcAlpha OneMinusSrcAlpha
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

		//		return _Color;

		//		float4 cellSize = ((float4(0.2, 0.2, 0.2, 0.2) / (_ScreenParams.xyxy / 1000)) / unity_OrthoParams.y);
		//		float4 pixelPos = i.grabPos;
		//		
		//		// scale down to cellsize
		//		pixelPos /= cellSize;

		//		// round to closest ints
		//		float4 ceiledPos = pixelPos + 1;// ceil(pixelPos);
		//		float4 flooredPos = pixelPos - 1;// floor(pixelPos);
		//		
		//		// reset to old size (but rounded)
		//		pixelPos *= cellSize;
		//		ceiledPos *= cellSize;
		//		flooredPos *= cellSize;

		//		float4 ceiledFlooredPos = float4(ceiledPos.x, flooredPos.y, 1, 1);
		//		float4 flooredCeiledPos = float4(flooredPos.x, ceiledPos.y, 1, 1);

		//		//float tx = (pixelPos.x - flooredPos.x) / (ceiledPos.x - flooredPos.x);
		//		//float ty = (pixelPos.y - flooredPos.y) / (ceiledPos.y - flooredPos.y);

		//		//float t1 = (length(pixelPos - flooredPos)) / (length(ceiledPos - flooredPos));
		//		//float t2 = (length(pixelPos - flooredCeiledPos)) / (length(ceiledFlooredPos - flooredCeiledPos));

		//		//fixed4 col1 = lerp(tex2Dproj(_GrabTexture, flooredPos), tex2Dproj(_GrabTexture, ceiledPos), t1);
		//		//fixed4 col2 = lerp(tex2Dproj(_GrabTexture, flooredCeiledPos), tex2Dproj(_GrabTexture, ceiledFlooredPos), t2);

		//		fixed4 col1 = tex2Dproj(_GrabTexture, flooredPos);
		//		fixed4 col2 = tex2Dproj(_GrabTexture, flooredCeiledPos);
		//		fixed4 col3 = tex2Dproj(_GrabTexture, ceiledFlooredPos);
		//		fixed4 col4 = tex2Dproj(_GrabTexture, ceiledPos);

		//		return tex2Dproj(_GrabTexture, pixelPos);
		//	}

		//	ENDCG
		//}
	}
}