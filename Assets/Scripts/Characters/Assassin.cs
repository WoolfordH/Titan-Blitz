using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Assassin : Character
{

	public MainCamXRayEffect XRayEffect;
	public float XRayFadeSpeed = 1f;
	float timer;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		//handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (100);
		secondaryDmg = new DAMAGE (0);

		primaryRange = 100f;
		secondaryRange = 0f;

		abilities [0] = new Ability ("Sniper", 2f);
		abilities [1] = new Ability ("N/A", 0f);
		abilities [2] = new Ability ("Ability 1", 0f);
		abilities [3] = new Ability ("Ability 2", 0f);
		abilities [4] = new Ability ("Sonar", 10f, 5f);

	}


	public override void PrimaryAttack()
	{
		//SNIPER

		RaycastHit hit;
		//raycast from center of camera and send hit message on first thing hit

		Debug.DrawRay (handler.cam.transform.position, handler.cam.transform.forward * primaryRange, Color.red);

		if (Physics.Raycast (handler.cam.transform.position, handler.cam.transform.forward, out hit, primaryRange))
		{
            //if the hit object does not have a network identity it will not be interactable so no need to pass it 
            NetworkIdentity netIdentity = hit.collider.gameObject.GetComponentInParent<NetworkIdentity>();
            NetworkInstanceId netID = new NetworkInstanceId();
            if(netIdentity)
            {
                netID = netIdentity.netId;
            }
            
            CmdPrimaryHit(netID, hit.point, hit.normal, this.transform.position, PlayerConnection.current.connectionID);
        }

        //set cooldown
        abilities[0].StartTimer();
	}

    [Command]
    private void CmdPrimaryHit(NetworkInstanceId netID, Vector3 hitPoint, Vector3 hitNormal, Vector3 originPoint, int sender)
    {
        //Perform checks that this hit was possible 
        GameObject collider;
        if(collider = NetworkServer.FindLocalObject(netID)) //if false the collider did not have a network ID or does not exist on the server
        {
            //get weapon damage
            float dmgMod = 1f;
            if (handler.powerups.Exists(x => x.powerupType.ToString() == "Damage"))
            {
                dmgMod = handler.powerups.Find(x => x.powerupType.ToString() == "Damage").multiplier;
            }

            //send message to all children - if we have multiple hit receivers on 1 object this will cause issues 
            collider.BroadcastMessage("Hit", new HitData((int)(primaryDmg.damage * dmgMod), hitPoint, hitNormal, sender), SendMessageOptions.DontRequireReceiver);

            if (collider.gameObject.GetComponentInChildren<CharacterHandler>()) //if is a player
                if (collider.gameObject.GetComponentInChildren<CharacterHandler>().GetTeam() != this.handler.GetTeam()) //if other team
                {
                    
                    
                }


        }
    }



	public override void Ability1()
	{
		//set cooldown
		abilities[2].StartTimer();
	}

	public override void Ability2()
	{
		//set cooldown
		abilities[3].StartTimer();
	}

	public override void Ultimate()
	{
		StartCoroutine (FadeXRay(true));

		//set cooldown
		abilities[4].StartTimer();
	}


	public override void OnAbilityExpired (int index)
	{
		switch (index)
		{
		case 4: //turn off the xray cam
			StartCoroutine (FadeXRay(false));
			break;
		}
	}



	IEnumerator FadeXRay(bool fadeIn)
	{
		if (fadeIn)
		{
			timer = 0;

			while (timer <= 1)
			{
				XRayEffect.effectAmount = timer;
				timer += XRayFadeSpeed * Time.deltaTime;

				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			timer = 1;

			while (timer >= 0)
			{
				XRayEffect.effectAmount = timer;
				timer -= XRayFadeSpeed * Time.deltaTime;

				yield return new WaitForEndOfFrame();
			}
		}
	}
}
