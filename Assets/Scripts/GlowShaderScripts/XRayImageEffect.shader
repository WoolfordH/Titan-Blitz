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

				//get the highest colour value from the main colour
				float colourVal = max(max(col.r, col.g), max(col.b, 0));

				// saturate the image based on the inverse of amount val
				col.rgb = float3(clamp(lerp(colourVal, col.r, 1-amount), 0, 1), clamp(lerp(colourVal, col.g, 1-amount), 0, 1), clamp(lerp(colourVal, col.b, 1-amount), 0, 1));

				//get the highest colour value from the glow colour
				colourVal = max(max(glowCol.r, glowCol.g), max(glowCol.b, 0));

				colourVal = min(colourVal, amount);

				//lerp between the original image and the glow image based on how colourful the current pixel is
				col.rgb = float3(clamp(lerp(col.r, glowCol.r, colourVal), 0, 1), clamp(lerp(col.g, glowCol.g, colourVal), 0, 1), clamp(lerp(col.b,glowCol.b, colourVal), 0, 1));

				return col;
			}
			ENDCG
		}
	}
}
