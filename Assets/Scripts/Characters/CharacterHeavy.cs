using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterHeavy : Character {


	// Use this for initialization
	protected override void Start () {
		base.Start ();
		//handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (30);

		primaryRange = 2f;

		abilities [0] = new AbilityShield ();
		abilities [1] = new AbilityDrone();
		abilities [2] = new AbilityDrone();

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


        //position gun to aim at target
        RaycastHit hit;
        Physics.Raycast(handler.cam.transform.position, handler.cam.transform.forward, out hit);
        Quaternion initRot = handler.gun.transform.rotation;

        //Fire gun
        if (primaryTimer <= 0f)
        {
            Debug.Log("Aim Aligned");
            handler.gun.transform.LookAt(hit.point);
            Debug.DrawRay(handler.cam.transform.position, handler.cam.transform.forward * Vector3.Distance(handler.cam.transform.position, hit.point), Color.green, 5f);
            Debug.DrawRay(handler.muzzlePos.position, handler.muzzlePos.forward * Vector3.Distance(handler.muzzlePos.position, hit.point), Color.red, 5f);

            Fire();
            primaryTimer = primaryDelay;
        }

        handler.gun.transform.rotation = initRot;
    }

	void Fire()
	{
		Projectile proj = Instantiate(primaryProj, handler.muzzlePos.position, handler.muzzlePos.rotation).GetComponent<Projectile>();
		proj.owners.Add(this.transform);
		proj.dmg = primaryDmg;
	}

}
