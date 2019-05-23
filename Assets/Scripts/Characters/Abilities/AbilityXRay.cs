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

        //audio
        if (caster.enableClip[1] != null)
        {
            caster.CmdPlaySoundEnable(1);
        }
    }

	public override void AbilityExpired ()
	{
        //StartCoroutine (FadeXRay(false));
        fading = true;
        fadeIn = false;
        fadeTimer = 1f;

        //audio
        if (caster.disableClip[1] != null)
        {
            caster.CmdPlaySoundDisable(1);
        }
    }

    protected override void Update()
    {
        base.Update();

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

    public override void ForceEnd()
    {
        XRayEffect.effectAmount = 0f;
        fadeTimer = 0f;
        fading = false;

        timer = 0f;
        active = false;
    }
}
