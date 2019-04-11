using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterLight : Character {

    public GameObject grabberPrefab;

    // Use this for initialization
    protected override void Start () {

		base.Start ();
		//handler = GetComponent<CharacterHandler> ();

		primaryRange = 100f;

		//abilities [0] = new AbilityGrapple (this, grabberPrefab);
		//abilities [1] = new AbilityXRay(this);
		//abilities [2] = new AbilityXRay(this);

        InitialiseAbilities();

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

            CmdFire();
            primaryTimer = primaryDelay;
        }

        
    }

    //Projectiles should be spawned from the server 
    [Command]
	void CmdFire()
	{
        //position gun to aim at target
        RaycastHit hit;
        Quaternion initRot = handler.gun.transform.rotation;

        if (Physics.Raycast(handler.cam.transform.position, handler.cam.transform.forward, out hit, 1000f, GameHandler.current.shootableLayer))
        {
            Debug.Log("Aim Aligned");
            handler.gun.transform.LookAt(hit.point);
        }
        else
        {
            handler.gun.transform.LookAt(handler.cam.transform.position + (handler.cam.transform.forward * 1000f));
        }

        Projectile proj = Instantiate(primaryProj, handler.muzzlePos.position, handler.muzzlePos.rotation).GetComponent<Projectile>();
        NetworkServer.Spawn(proj.gameObject);
		proj.owners.Add(this.transform);
        proj.senderID = id;
        proj.dmg = new DAMAGE((int)(primaryDmg.damage * dmgMod), primaryDmg.armourPiercing);

        handler.gun.transform.rotation = initRot;
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
