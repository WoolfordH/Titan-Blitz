﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityGrapple : Ability {

	public GameObject grabberPrefab;
	public float grabberMaxDist;
	public float grabberSpeed;

	public override void Init()
	{
		name = "Grapple";
		cooldown = 10f;
		duration = 5f;

		timer = 0f;
		active = false;

	}

	public override void UseAbility ()
	{
		//////Grabber\\\\\\

		//Debug.Log (grabberSpeed);

		CmdSpawnGrabber(caster.handler.cam.transform.forward, caster.handler.rb.velocity);
	}

	[Command]
	private void CmdSpawnGrabber(Vector3 forward, Vector3 initVel)
	{
		GameObject grabber = GameObject.Instantiate(grabberPrefab, caster.handler.cam.transform.position, caster.handler.cam.transform.rotation);


		NetworkServer.Spawn(grabber);//, connectionToServer);
		grabber.GetComponent<Grabber>().Init(caster, grabberMaxDist, forward, grabberSpeed, initVel);
	}

	public override void AbilityExpired ()
	{

	}
}