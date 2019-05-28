Shader "Custom/CelShadingGlow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
		Tags
		{
			"XRay" = "True"
		}
			
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
	
			Fallback "Custom/CelShading"
}
