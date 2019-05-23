using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityGrapple : Ability {

	public GameObject grabberPrefab;
	public float maxDist;
	public float startSpeed;
    public float returnSpeed;
    public Transform tip;

    //public AbilityGrapple(Character c)
    //{
    //    caster = c;
    //}
    //
    //public AbilityGrapple(Character c, GameObject grabber)
    //{
    //    caster = c;
    //    grabberPrefab = grabber;
    //}

    public override void Init()
	{
		name = "Grapple";

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
		GameObject grabber = GameObject.Instantiate(grabberPrefab, tip.transform.position, caster.handler.cam.transform.rotation);

        //audio
        if (caster.enableClip[0] != null)
        {
            caster.CmdPlaySoundEnable(0);
        }

        NetworkServer.Spawn(grabber);//, connectionToServer);
        grabber.GetComponent<Grabber>().Init(caster, tip, forward, initVel, startSpeed, returnSpeed, maxDist);
	}

	public override void AbilityExpired ()
	{

	}

    public override void ForceEnd()
    {

    }
}
