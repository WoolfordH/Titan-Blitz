using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tank : Character {

	public GameObject grabberPrefab;
	public float grabberMaxDist;
	public float grabberSpeed;
	public GameObject shieldPrefab;
	public Vector3 shieldPosition = new Vector3(0f, 1.3f, 1.8f);
	GameObject shield;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		//handler = GetComponent<CharacterHandler> ();

		primaryDmg = new DAMAGE (30);
		secondaryDmg = new DAMAGE (0);

		primaryRange = 2f;
		secondaryRange = 0f;

		abilities [0] = new Ability ("Primary", 3f);
		abilities [1] = new Ability ("N/A", 0f);
		abilities [2] = new Ability ("Grab", 20f);
		abilities [3] = new Ability ("Shield", 40f, 15f);
		abilities [4] = new Ability ("Ultimate", 300f);
	}

	protected override void Update()
	{
		base.Update ();

		if (shield)
		{
			if (abilities [3].isDurationElapsed())
			{
				Destroy (shield);
			}
		}
	}

	public override void PrimaryAttack()
	{

		float dmgMod = 1f;

		if (handler.powerups.Exists (x => x.powerupType.ToString () == "Damage"))
		{
			dmgMod = handler.powerups.Find (x => x.powerupType.ToString () == "Damage").multiplier;
		}



		//MELEE

		RaycastHit hit;
		//raycast from center of camera and send hit message on first thing hit

		Debug.DrawRay (handler.cam.transform.position, handler.cam.transform.forward * primaryRange, Color.red);

		if (Physics.SphereCast (handler.cam.transform.position, 1f, handler.cam.transform.forward, out hit, primaryRange))
		{
			hit.collider.gameObject.SendMessageUpwards ("Hit", new HitData((int)(primaryDmg.damage * dmgMod), hit.point, hit.normal), SendMessageOptions.DontRequireReceiver);
		}


		Debug.Log (abilities [0].name + " was Used!");

		//set cooldown
		abilities[0].StartTimer();
	}




	public override void Ability1()
	{
		//grab
		//Debug.Log (grabberSpeed);

		GameObject grabber = Instantiate(grabberPrefab, handler.cam.transform.position, handler.cam.transform.rotation);
        grabber.GetComponent<Grabber>().Init(this, grabberMaxDist, handler.cam.transform.forward, grabberSpeed, handler.rb.velocity);

        NetworkServer.SpawnWithClientAuthority(grabber, PlayerConnection.clientIdentity);

        

		Debug.Log (abilities [2].name + " was Used!");

		//set cooldown
		abilities[2].StartTimer();
	}

	public override void Ability2()
	{
		//shield
		//Instantiate a large see-through wall infront of the tank 
		//1.3f y, 1.8f z

		shield = Instantiate(shieldPrefab, transform.TransformPoint(shieldPosition), transform.rotation, transform);

		Debug.Log (abilities [3].name + " was Used!");

		//set cooldown
		abilities[3].StartTimer();
	}

	public override void Ultimate()
	{
		Debug.Log (abilities [4].name + " was Used!");

		//set cooldown
		abilities[4].StartTimer();
	}

	public override void OnAbilityExpired (int index)
	{

	}
}
