Shader "Glow/Glow"
{
	Properties
	{
		_GlowColour("Glow Colour", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" "XRay" = "True"}
		LOD 100

		ZWrite Off
		ZTest Always
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD1;
			};

			float4 _GlowColour;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// colour is equal to world space normal
				float nDotV = 1 - dot(i.normal, i.viewDir) * 1.5;
				float4 col = nDotV * _GlowColour;

				return col;
			}
			ENDCG
		}
	}
}
