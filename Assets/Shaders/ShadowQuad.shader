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

		Pass {}
	}
}