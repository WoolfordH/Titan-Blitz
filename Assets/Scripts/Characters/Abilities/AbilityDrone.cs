using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDrone : Ability {

    //public AbilityDrone(Character c)
    //{
    //    caster = c;
    //}

	public override void Init()
	{
		name = "Drone";
		cooldown = 10f;
		duration = 5f;

		timer = 0f;
		active = false;

	}

	public override void UseAbility ()
	{
		base.UseAbility ();
	}

	public override void AbilityExpired ()
	{

	}
}
