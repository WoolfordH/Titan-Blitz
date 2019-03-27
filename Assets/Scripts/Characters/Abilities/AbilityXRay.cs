using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityXRay : Ability {

	public MainCamXRayEffect XRayEffect;
	public float XRayFadeSpeed = 1f;
    bool fading = false;
    bool fadeIn = false;
    float fadeTimer;

    //public AbilityXRay(Character c)
    //{
    //    caster = c;
    //}

    public override void Init()
	{
		name = "XRay";
		cooldown = 10f;
		duration = 5f;

		timer = 0f;
		active = false;

		XRayEffect = caster.handler.cam.gameObject.GetComponent<MainCamXRayEffect> ();

	}

	public override void UseAbility ()
	{
        //StartCoroutine (FadeXRay(true));
        fading = true;
        fadeIn = true;
        fadeTimer = 0f;
    }

	public override void AbilityExpired ()
	{
        //StartCoroutine (FadeXRay(false));
        fading = true;
        fadeIn = false;
        fadeTimer = 1f;
    }

    void Update()
    {
        if (fading)
        {
            if (fadeIn)
            {
                if (fadeTimer < 1f)
                {
                    XRayEffect.effectAmount = fadeTimer;
                    fadeTimer += XRayFadeSpeed * Time.deltaTime;
                }
                else
                {
                    fadeTimer = 1f;
                    fading = false;
                }
            }
            else
            {
                if (fadeTimer >= 0f)
                {
                    XRayEffect.effectAmount = fadeTimer;
                    fadeTimer -= XRayFadeSpeed * Time.deltaTime;
                }
                else
                {
                    fadeTimer = 0f;
                    fading = false;
                }
            }
        }

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

                Debug.Log("Fade in: " + timer.ToString());

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

                Debug.Log("Fade out: " + timer.ToString());

                yield return new WaitForEndOfFrame();
			}
		}
	}
}
