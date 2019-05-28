Shader "Custom/CelShadingGlowTintMask"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[PerRendererData]
		_Colour("Tint Colour", Color) = (1,1,1,1)

		_NormalTex("Normal Map", 2D) = "white" {}
		_NormalAmount("Normal Amount", Range(0, 1)) = 1.0

		[MaterialToggle][PerRendererData]
		_TintOn("Tint On", Float) = 0
		_TintMask("Tint Mask", 2D) = "white" {}

		_GlowMap("Glow Map", 2D) = "white" {}
		_GlowColour("Glow Colour", Color) = (1,1,1,1)

		_Ramp("Lighting Ramp", 2D) = "white"{}

		[HDR]
		_AmbientColour("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)

		[HDR]
		_SpecularColour("Specular Colour", Color) = (0.9, 0.9, 0.9, 1)
		_Glossiness("Glossiness", Float) = 32
		_Smoothness("Smoothness", Range(0, 1)) = 1

		[HDR]
		_RimColour("Rim Colour", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(.002, 0.03)) = .005
	}
	SubShader
	{
		Tags
		{
			"XRay" = "True"
		}
		Pass
		{
			Tags
			{ 
				"LightMode" = "ForwardBase" 
				"Pass Flags" = "OnlyDirectional"
			 }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 normalUV : TEXCOORD1;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float2 normalUV : TEXCOORD1;
				float3 viewDir : TEXCOORD2;

				SHADOW_COORDS(3)

				//UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NormalTex;
			float4 _NormalTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normalUV = TRANSFORM_TEX(v.normalUV, _NormalTex);

				TRANSFER_SHADOW(o)
				return o;
			}

			sampler2D _TintMask;
			bool _TintOn;

			sampler2D _Ramp;

			float _NormalAmount;

			float4 _Colour;

			float4 _AmbientColour;

			float _Glossiness;
			float4 _SpecularColour;
			float _Smoothness;

			float4 _RimColour;
			float _RimAmount;
			float _RimThreshold;

			float4 _OutlineColour;
			float _OutlineThickness;
			
			
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				float3 normalMap = normalize(UnityObjectToWorldNormal(tex2D(_NormalTex, i.normalUV))) * _NormalAmount;

				float NdotL = dot(_WorldSpaceLightPos0, normal);

				float2 rampUV = float2(1 - (NdotL * 0.5 + 0.5), 0.5);


				float shadow = SHADOW_ATTENUATION(i);


				float lightIntensity = tex2D(_Ramp, rampUV).z; //smoothstep(0, 0.01, NdotL * shadow);

				float4 light = lightIntensity * _LightColor0;


				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);


				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = (specularIntensitySmooth * _SpecularColour) * _Smoothness;


				float4 rimDot = 1 - dot(viewDir, normal);


				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = (rimIntensity * _RimColour) * _Smoothness;

				float outlineIntensity = rimDot * NdotL;
				outlineIntensity = smoothstep(_OutlineThickness - 0.01, _OutlineThickness + 0.01, outlineIntensity);
				float4 outline = outlineIntensity * _OutlineColour;

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				fixed4 tintMask; 
				if (_TintOn)
				{
					tintMask = tex2D(_TintMask, i.uv);
					outline = outlineIntensity * tintMask;
				}
				else
				{
					tintMask = fixed4(0.0, 0.0, 0.0, 1.0);
				}

				return (light + _AmbientColour + specular + rim) * lerp(col, (tintMask * _Colour), tintMask);
			}
			ENDCG
		}

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
