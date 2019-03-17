using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityXRay : Ability {

	public MainCamXRayEffect XRayEffect;
	public float XRayFadeSpeed = 1f;

	public override void Init()
	{
		name = "XRay";
		cooldown = 10f;
		duration = 5f;

		timer = 0f;
		active = false;

		XRayEffect = caster.handler.cam.gameObject.AddComponent<MainCamXRayEffect> ();

	}

	public override void UseAbility ()
	{
		caster.StartCoroutine (FadeXRay(true));
	}

	public override void AbilityExpired ()
	{
		caster.StartCoroutine (FadeXRay(false));
	}


	IEnumerator FadeXRay(bool fadeIn)
	{
		if (fadeIn)
		{
			timer = 0;

			while (timer <= 1)
			{
				XRayEffect.effectAmount = timer;
				timer += XRayFadeSpeed * Time.deltaTime;

				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			timer = 1;

			while (timer >= 0)
			{
				XRayEffect.effectAmount = timer;
				timer -= XRayFadeSpeed * Time.deltaTime;

				yield return new WaitForEndOfFrame();
			}
		}
	}
}
