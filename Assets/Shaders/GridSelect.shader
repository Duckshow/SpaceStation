// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/GridSelect"
{
	Properties
	{
		_Color ("Color", Color) = (1, 0, 0, 0.5)
		_Pos1 ("Pos #1", Vector) = (0, 0, 0)
		_Pos2 ("Pos #2", Vector) = (0, 0, 0)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD0;
			};

			float4 _Color;
			uniform float3 _Pos1;
			uniform float3 _Pos2;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if ((i.worldPos.x > _Pos1.x && i.worldPos.x < _Pos2.x && i.worldPos.y > _Pos1.y && i.worldPos.y < _Pos2.y)
					|| (i.worldPos.x < _Pos1.x && i.worldPos.x > _Pos2.x && i.worldPos.y < _Pos2.y && i.worldPos.y > _Pos2.y))
				{
					return _Color;
				}
				else 
				{
					return float4(0, 0, 0, 0);
				}
			}
			ENDCG
		}
	}
}
