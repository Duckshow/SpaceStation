Shader "Custom/CustomLight" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ShadowColor ("Shadow Color", Color) = (1,1,1,1)
		_UVScale ("UV Scale", float) = 1.0
	}

	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+50" }
		Stencil {
			Ref 1
			Comp equal
			Pass incrWrap
		}
		Pass {

			Cull Off
			ZTest LEqual
			ZWrite On			//On | Off - Z coordinates from pixel positions will not be written to the Z/Depth buffer
			AlphaTest Off		//0.0	//Less | Greater | LEqual | GEqual | Equal | NotEqual | Always   (also 0.0 (float value) | [_AlphaTestThreshold]) - All pixels will continue through the graphics pipeline because alpha testing is Off
			Lighting Off		//On | Off - Lighting will not be calculated or applied
			ColorMask RGBA		//RGBA | RGB | A | 0 | any combination of R, G, B, A - Color channels allowed to be modified in the backbuffer are: RGBA
			// //BlendOp	//Add	// Min | Max | Sub | RevSub - BlendOp is not being used and will default to an Add operation when combining the source and destination parts of the blend mode
			Blend SrcAlpha OneMinusSrcAlpha			//SrcFactor DstFactor (also:, SrcFactorA DstFactorA) = One | Zero | SrcColor | SrcAlpha | DstColor | DstAlpha | OneMinusSrcColor | OneMinusSrcAlpha | OneMinusDstColor | OneMinusDstAlpha - Blending between shader output and the backbuffer will use blend mode 'Alpha Blend'
								//Blend SrcAlpha OneMinusSrcAlpha     = Alpha blending
								//Blend One One                       = Additive
								//Blend OneMinusDstColor One          = Soft Additive
								//Blend DstColor Zero                 = Multiplicative
								//Blend DstColor SrcColor             = 2x Multiplicative

			CGPROGRAM						//Start a program in the CG language
			//#pragma target 2.0				//Run this shader on at least Shader Model 2.0 hardware (e.g. Direct3D 9)
			#pragma fragment frag			//The fragment shader is named 'frag'
			#pragma vertex vert				//The vertex shader is named 'vert'
			#include "UnityCG.cginc"		//Include Unity's predefined inputs and macros

			//Unity variables to be made accessible to Vertex and/or Fragment shader
			//uniform sampler2D _MainTex;					//Define _MainTex from Texture Unit 0 to be sampled in 2D
			//uniform float4 _MainTex_ST;					//Use the Float _MainTex_ST to pass the Offset and Tiling for the texture(s)
			uniform fixed4 _Color;							//Use the Color _Color provided by Unity
			uniform fixed4 _ShadowColor;							//Use the Color _Color provided by Unity
			uniform float _UVScale;
		
			struct appData {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = half2(v.texcoord.x, v.texcoord.y) * _UVScale;
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				return fixed4(0, 0, 0, 0);
				return fixed4(lerp(fixed4(0, 0, 0, 0), _ShadowColor, sqrt((i.uv.x * i.uv.x) + (i.uv.y * i.uv.y))));	//Output radial gradient
			}

			ENDCG
		}
		Pass {

			Cull Off
			ZTest LEqual
			ZWrite On			//On | Off - Z coordinates from pixel positions will not be written to the Z/Depth buffer
			AlphaTest Off		//0.0	//Less | Greater | LEqual | GEqual | Equal | NotEqual | Always   (also 0.0 (float value) | [_AlphaTestThreshold]) - All pixels will continue through the graphics pipeline because alpha testing is Off
			Lighting Off		//On | Off - Lighting will not be calculated or applied
			ColorMask RGBA		//RGBA | RGB | A | 0 | any combination of R, G, B, A - Color channels allowed to be modified in the backbuffer are: RGBA
			// //BlendOp	//Add	// Min | Max | Sub | RevSub - BlendOp is not being used and will default to an Add operation when combining the source and destination parts of the blend mode
			Blend SrcAlpha OneMinusSrcAlpha			//SrcFactor DstFactor (also:, SrcFactorA DstFactorA) = One | Zero | SrcColor | SrcAlpha | DstColor | DstAlpha | OneMinusSrcColor | OneMinusSrcAlpha | OneMinusDstColor | OneMinusDstAlpha - Blending between shader output and the backbuffer will use blend mode 'Alpha Blend'
								//Blend SrcAlpha OneMinusSrcAlpha     = Alpha blending
								//Blend One One                       = Additive
								//Blend OneMinusDstColor One          = Soft Additive
								//Blend DstColor Zero                 = Multiplicative
								//Blend DstColor SrcColor             = 2x Multiplicative

			CGPROGRAM						//Start a program in the CG language
			//#pragma target 2.0				//Run this shader on at least Shader Model 2.0 hardware (e.g. Direct3D 9)
			#pragma fragment frag			//The fragment shader is named 'frag'
			#pragma vertex vert				//The vertex shader is named 'vert'
			#include "UnityCG.cginc"		//Include Unity's predefined inputs and macros

			//Unity variables to be made accessible to Vertex and/or Fragment shader
			//uniform sampler2D _MainTex;					//Define _MainTex from Texture Unit 0 to be sampled in 2D
			//uniform float4 _MainTex_ST;					//Use the Float _MainTex_ST to pass the Offset and Tiling for the texture(s)
			uniform fixed4 _Color;							//Use the Color _Color provided by Unity
			uniform fixed4 _ShadowColor;							//Use the Color _Color provided by Unity
			uniform float _UVScale;
		
			struct appData {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appData v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = half2(v.texcoord.x, v.texcoord.y) * _UVScale;
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				return fixed4(lerp(_Color, fixed4(_Color.rgb, 0), sqrt((i.uv.x * i.uv.x) + (i.uv.y * i.uv.y))));	//Output radial gradient
			}

			ENDCG
		}
	}
}