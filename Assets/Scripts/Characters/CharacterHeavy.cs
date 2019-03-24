using System;
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

		abilities [0] = new AbilityShield (this);
		abilities [1] = new AbilityDrone(this);
		abilities [2] = new AbilityDrone(this);

        InitialiseAbilities();

	}

    protected override void Update()
	{
		base.Update ();

	}

	public override void PrimaryAttack()
	{
		if (handler.powerups.Exists (x => x.powerupType.ToString () == "Damage"))
		{
			dmgMod *= handler.powerups.Find (x => x.powerupType.ToString () == "Damage").multiplier;
		}


        //position gun to aim at target
        RaycastHit hit;
        Quaternion initRot = handler.gun.transform.rotation;

        //Fire gun
        if (primaryTimer <= 0f)
        {
            if (Physics.Raycast(handler.cam.transform.position, handler.cam.transform.forward, out hit, 1000f, ~GameHandler.current.projectileLayer))
            {
                Debug.Log("Aim Aligned");
                handler.gun.transform.LookAt(hit.point);
            }
            else
            {
                handler.gun.transform.LookAt(handler.cam.transform.position + (handler.cam.transform.forward * 1000f));
            }
            //Debug.DrawRay(handler.cam.transform.position, handler.cam.transform.forward * Vector3.Distance(handler.cam.transform.position, hit.point), Color.green, 5f);
            //Debug.DrawRay(handler.muzzlePos.position, handler.muzzlePos.forward * Vector3.Distance(handler.muzzlePos.position, hit.point), Color.red, 5f);

            //animate recoil
            handler.gunAnim.SetTrigger("fired");

            CmdFire();
            primaryTimer = primaryDelay;
        }

        handler.gun.transform.rotation = initRot;
    }

    [Command]
	void CmdFire()
	{
		Projectile proj = Instantiate(primaryProj, handler.muzzlePos.position, handler.muzzlePos.rotation).GetComponent<Projectile>();
        NetworkServer.Spawn(proj.gameObject);
        proj.owners.Add(this.transform);
		proj.dmg = new DAMAGE((int)(primaryDmg.damage * dmgMod), primaryDmg.armourPiercing);
	}

}
