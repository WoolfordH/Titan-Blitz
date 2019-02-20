Shader "Glow/XRayImageEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GlowTex ("Glow Texture", 2D) = "white" {}
		amount ("Effect Amount", float) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _GlowTex;
			float amount;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float4 glowCol = tex2D(_GlowTex, i.uv);

				// saturate the image based on amount val
				col.rgb = float3(col.r, clamp(lerp(col.r, col.g, 1-amount), 0, 1), clamp(lerp(col.r, col.b, 1-amount), 0, 1));

				//add glowRT
				col.rgb = float3(clamp(lerp(col.r, glowCol.r, glowCol.r), 0, 1), clamp(lerp(col.g, glowCol.g, glowCol.g), 0, 1), clamp(lerp(col.b,glowCol.b, glowCol.b), 0, 1));

				return col;
			}
			ENDCG
		}
	}
}
