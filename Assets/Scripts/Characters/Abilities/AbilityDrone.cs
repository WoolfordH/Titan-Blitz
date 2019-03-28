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
		cooldown = 10f;
		duration = 5f;

		timer = 0f;
		active = false;

	}

	public override void UseAbility ()
	{
        if (drone)
        {
            NetworkServer.Destroy(drone);
        }

        drone = Instantiate(dronePrefab, caster.handler.cam.transform.position + (caster.handler.cam.transform.forward * 1f), Quaternion.LookRotation(caster.handler.cam.transform.forward, Vector3.up));
        drone.GetComponent<Drone>().owner = caster;
        NetworkServer.Spawn(drone);
	}

	public override void AbilityExpired ()
	{

	}
}
