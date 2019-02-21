using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GlowReplacement : MonoBehaviour
{
	public Shader glowShader;
	public MainCamXRayEffect mainCam;
	RenderTexture rt;

	void OnEnable()
	{
		rt = new RenderTexture(Screen.width, Screen.height, 16);
		GetComponent<Camera> ().targetTexture = rt;

		GetComponent<Camera>().SetReplacementShader(glowShader, "XRay");
		mainCam.glowRT = rt;
	}

}
