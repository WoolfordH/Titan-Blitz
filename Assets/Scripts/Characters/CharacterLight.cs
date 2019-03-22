using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLight : Character {

	// Use this for initialization
	protected override void Start () {

		base.Start ();
		handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (2);

		primaryRange = 100f;

		abilities [0] = new AbilityGrapple ();
		abilities [1] = new AbilityXRay();
		abilities [2] = new AbilityXRay();

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
        proj.senderID = id;
		proj.dmg = primaryDmg;
	}



    protected override void HandleAttacks()
    {
        if (Input.GetKey(handler.controls.primaryAtk))
        {
            if (primaryTimer <= 0)
            {
                PrimaryAttack();
            }
        }

        if (Input.GetKeyDown(handler.controls.Ability1))
        {
            if (abilities[0].timer <= 0)
            {
                UseAbility(0);
            }
        }

        if (Input.GetKeyDown(handler.controls.Ability2))
        {
            if (abilities[1].timer <= 0)
            {
                UseAbility(1);
            }
        }

        if (Input.GetKeyDown(handler.controls.ult))
        {
            if (abilities[2].timer <= 0)
            {
                UseAbility(2);
            }
        }
    }

}
