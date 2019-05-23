using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityDrone : Ability {

    public GameObject dronePrefab;
    GameObject drone;

    //public AbilityDrone(Character c)
    //{
    //    caster = c;
    //}

	public override void Init()
	{
		name = "Drone";

		timer = 0f;
		active = false;

	}

	public override void UseAbility ()
	{
        CmdAbility();
	}

    [Command]
    private void CmdAbility()
    {
        if (drone)
        {
            CmdDestroySelf();
        }

        drone = Instantiate(dronePrefab, caster.handler.cam.transform.position + (caster.handler.cam.transform.forward * 1f), Quaternion.LookRotation(caster.handler.cam.transform.forward, Vector3.up));
        drone.GetComponent<Drone>().owner = caster;
        NetworkServer.Spawn(drone);
    }

	public override void AbilityExpired ()
	{
        if (drone)
        {
            CmdDestroySelf();
        }
    }

    public override void ForceEnd()
    {
        if (drone)
        {
            CmdDestroySelf();
        }

        timer = 0f;
        active = false;
    }

    [Command]
    private void CmdDestroySelf()
    {
        drone.GetComponent<Drone>().Die();
        NetworkServer.Destroy(drone);
    }
}
