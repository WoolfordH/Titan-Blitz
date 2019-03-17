using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tank : Character {

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		//handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (30);

		primaryRange = 2f;

		//abilities [0] = new Ability ("Primary", 3f);
		//abilities [1] = new Ability ("N/A", 0f);
		//abilities [2] = new Ability ("Grab", 20f);
		//abilities [3] = new Ability ("Shield", 40f, 15f);
		//abilities [4] = new Ability ("Ultimate", 300f);
	}

	protected override void Update()
	{
		base.Update ();
	}

	public override void PrimaryAttack()
	{

		float dmgMod = 1f;

		if (handler.powerups.Exists (x => x.powerupType.ToString () == "Damage"))
		{
			dmgMod = handler.powerups.Find (x => x.powerupType.ToString () == "Damage").multiplier;
		}



		//MELEE

		RaycastHit hit;
		//raycast from center of camera and send hit message on first thing hit

		Debug.DrawRay (handler.cam.transform.position, handler.cam.transform.forward * primaryRange, Color.red);

		if (Physics.SphereCast (handler.cam.transform.position, 1f, handler.cam.transform.forward, out hit, primaryRange))
		{
			hit.collider.gameObject.SendMessageUpwards ("Hit", new HitData((int)(primaryDmg.damage * dmgMod), hit.point, hit.normal, PlayerConnection.current.connectionID), SendMessageOptions.DontRequireReceiver);
		}


		Debug.Log (abilities [0].name + " was Used!");

		//set cooldown
		abilities[0].StartTimer();
	}





}
