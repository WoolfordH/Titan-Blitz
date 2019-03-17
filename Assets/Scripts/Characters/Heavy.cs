using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Heavy : Character {


	// Use this for initialization
	protected override void Start () {
		base.Start ();
		//handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (30);

		primaryRange = 2f;

		abilities [0] = new AbilityShield ();
		abilities [1] = new AbilityDrone();

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


		//Gun
		if (primaryTimer <= 0f)
		{
			Fire ();
		}
	}

	void Fire()
	{
		Projectile proj = Instantiate(primaryProj, handler.muzzlePos.position, handler.muzzlePos.rotation).GetComponent<Projectile>();
		proj.owners.Add(this.transform);
		proj.dmg = primaryDmg;

		primaryTimer = primaryDelay;
	}

}
