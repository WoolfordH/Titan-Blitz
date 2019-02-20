using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MainCamXRayEffect : MonoBehaviour {

	public Material imageEffect;
	public RenderTexture glowRT;
	[Range(0,1)]
	public float effectAmount = 0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		imageEffect.SetFloat ("amount", effectAmount);
		imageEffect.SetTexture ("_GlowTex", glowRT);
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit (src, imageEffect);
	}
}
