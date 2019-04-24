Shader "Custom/CelShadingGlowTintMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintMask ("Tint Mask", 2D) = "white" {}
		[PerRendererData]
		_Colour ("Tint Colour", Color) = (1,1,1,1)

		_GlowMap("Glow Map", 2D) = "white" {}
        _GlowColour("Glow Colour", Color) = (1,1,1,1)

		_Ramp ("Lighting Ramp", 2D) = "white"{}

		[HDR]
		_AmbientColour("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)

		[HDR]
		_SpecularColour("Specular Colour", Color) = (0.9, 0.9, 0.9, 1)
		_Glossiness("Glossiness", Float) = 32

		[HDR]
		_RimColour("Rim Colour", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
	}
	SubShader
	{

		Pass
		{
			Tags
			{ 
				"LightMode" = "ForwardBase" 
				"Pass Flags" = "OnlyDirectional"   
				"XRay" = "True" 
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
                float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;

				SHADOW_COORDS(2)

				//UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				TRANSFER_SHADOW(o)
				return o;
			}

			sampler2D _TintMask;

			sampler2D _Ramp;

			float4 _Colour;

			float4 _AmbientColour;

			float _Glossiness;
			float4 _SpecularColour;

			float4 _RimColour;
			float _RimAmount;
			float _RimThreshold;
			
			
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);


				float NdotL = dot(_WorldSpaceLightPos0, normal);

				float2 rampUV = float2(1 - (NdotL * 0.5 + 0.5), 0.5);
				

				float shadow = SHADOW_ATTENUATION(i);


				float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);

				float4 light = lightIntensity * _LightColor0;


				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);


				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColour;


				float4 rimDot = 1 - dot(viewDir, normal);


				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColour;

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				fixed4 tintMask = tex2D(_TintMask, i.uv);
				

				return (light + _AmbientColour + specular + rim) * lerp(col, (tintMask * _Colour), tintMask);
			}
			ENDCG
		}

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
