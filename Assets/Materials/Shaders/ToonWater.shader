﻿Shader "Custom/ToonWater"
{
	Properties
	{
		_DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
		_DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
		_DepthMaxDistance("Depth Max Distance", Float) = 1

		_SurfaceNoise("Surface Noise", 2D) = "white" {}
		_SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
		
		_FoamMaxDistance("Foam Max Distance", Float) = 0.4
		_FoamMinDistance("Foam Min Distance", Float) = 0.04

		_SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
		_SurfaceDistortion("Surface Distortion", 2D) = "white" {}
		_SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float2 noiseUV : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 screenPosition : TEXCOORD2;
				float2 distortUV : TEXCOORD1;
				float3 viewNormal : NORMAL;
			};

			sampler2D _SurfaceNoise;
			float4 _SurfaceNoise_ST;
			float _SurfaceNoiseCutoff;
			
			sampler2D _SurfaceDistortion;
			float4 _SurfaceDistortion_ST;

			float _SurfaceDistortionAmount;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
				o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
				o.screenPosition = ComputeScreenPos(o.vertex);
				o.viewNormal = COMPUTE_VIEW_NORMAL;
				return o;
			}
			
			float4 _DepthGradientShallow;
			float4 _DepthGradientDeep;
			float _DepthMaxDistance;

			float _FoamMaxDistance;
			float _FoamMinDistance;
			float2 _SurfaceNoiseScroll;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraNormalsTexture;


			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = (1, 1, 1, 1);

				float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
				float existingDepthLinear = LinearEyeDepth(existingDepth01);
				
				float depthDifference = existingDepthLinear - i.screenPosition.w;

				float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
				float4 waterColour = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

				float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition));
				float3 normalDot = saturate(dot(existingNormal, i.viewNormal));

				float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
				float foamDepthDifference01 = saturate(depthDifference / foamDistance);
				float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;

				float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;

				float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, (i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);

				float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;
				float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;

				return waterColour + surfaceNoise;
			}
			ENDCG
		}
	}
}
