using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : Character {

	// Use this for initialization
	protected override void Start () {

		base.Start ();
		handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (2);
		secondaryDmg = new DAMAGE (0);

		primaryRange = 100f;
		secondaryRange = 0f;

		abilities [0] = new Ability ("Primary", .5f);
		abilities [1] = new Ability ("N/A", 0f);
		abilities [2] = new Ability ("Ability 1", 0f);
		abilities [3] = new Ability ("Ability 2", 0f);
		abilities [4] = new Ability ("Ultimate", 0f);
	}


	public override void PrimaryAttack()
	{
		float dmgMod = 1f;

		if (handler.powerups.Exists (x => x.powerupType.ToString () == "Damage"))
		{
			dmgMod = handler.powerups.Find (x => x.powerupType.ToString () == "Damage").multiplier;
		}



		//Gun

		RaycastHit hit;
		//raycast from center of camera and send hit message on first thing hit

		Debug.DrawRay (handler.cam.transform.position, handler.cam.transform.forward * primaryRange, Color.red);

		if (Physics.Raycast (handler.cam.transform.position, handler.cam.transform.forward, out hit, primaryRange))
		{
			hit.collider.gameObject.SendMessageUpwards ("Hit", new HitData((int)(primaryDmg.damage * dmgMod), hit.point, hit.normal), SendMessageOptions.DontRequireReceiver);
		}

		//set cooldown
		abilities[0].StartTimer();
	}




	public override void Ability1()
	{
		//set cooldown
		abilities[2].StartTimer();
	}

	public override void Ability2()
	{
		//set cooldown
		abilities[3].StartTimer();
	}

	public override void Ultimate()
	{
		//set cooldown
		abilities[4].StartTimer();
	}

	public override void OnAbilityExpired (int index)
	{
		
	}
}
