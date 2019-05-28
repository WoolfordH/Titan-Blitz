using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterHeavy : Character {

    public GameObject shieldPrefab;

    // Use this for initialization
    protected override void Start () {
		base.Start ();
		//handler = GetComponent<CharacterHandler> ();


		primaryRange = 2f;

		//abilities [0] = new AbilityShield (this, shieldPrefab);
		//abilities [1] = new AbilityDrone(this);
		//abilities [2] = new AbilityDrone(this);

        InitialiseAbilities();

	}

    protected override void Update()
	{
		base.Update ();

	}

	public override void PrimaryAttack()
	{
        Debug.Log("DAMAGE MOD = " + dmgMod);

        //Fire gun
        if (primaryTimer <= 0f)
        {

            //Debug.DrawRay(handler.cam.transform.position, handler.cam.transform.forward * Vector3.Distance(handler.cam.transform.position, hit.point), Color.green, 5f);
            //Debug.DrawRay(handler.muzzlePos.position, handler.muzzlePos.forward * Vector3.Distance(handler.muzzlePos.position, hit.point), Color.red, 5f);

            //animate recoil
            handler.gunAnim.SetTrigger("fired");

            CmdFire(handler.muzzlePos.position, System.DateTime.UtcNow.Millisecond);
            primaryTimer = primaryDelay;
        }

        
    }

    [Command]
	void CmdFire(Vector3 firePos, int shotTime)
    {
        //position gun to aim at target
        RaycastHit hit;
        Quaternion initRot = handler.gun.transform.rotation;

        if (Physics.Raycast(handler.muzzlePos.position, handler.muzzlePos.forward, out hit, 1000f, GameHandler.current.shootableLayer))
        {
            //Debug.Log("Aim Aligned");
            //Debug.Log("Aiming at: " + hit.collider.gameObject.name);
            handler.gun.transform.LookAt(hit.point);
        }
        else
        {
            handler.gun.transform.LookAt(handler.muzzlePos.position + (handler.muzzlePos.forward * 1000f));
        }

        Projectile proj = Instantiate(primaryProj, firePos, handler.muzzlePos.rotation).GetComponent<Projectile>();
        NetworkServer.SpawnWithClientAuthority(proj.gameObject, GameManager.current.GetPlayerConnection(id).gameObject);
        proj.owners.Add(this.transform);
        proj.senderID = id;
		proj.dmg = new DAMAGE((int)(primaryDmg.damage * dmgMod), primaryDmg.armourPiercing);
        proj.shotTime = shotTime;

        handler.gun.transform.rotation = initRot;
    }

}
