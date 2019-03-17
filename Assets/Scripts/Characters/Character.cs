using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public struct DAMAGE
{
	public int damage;
	public bool armourPiercing;

	public DAMAGE(int d)
	{
		damage = d;
		armourPiercing = false;
	}

	public DAMAGE(int d, bool piercing)
	{
		damage = d;
		armourPiercing = piercing;
	}
}

public struct HitData
{
	public DAMAGE damage;
	public Vector3 hitPoint;
	public Vector3 hitNormal;
    public int senderID;

	public HitData(DAMAGE dmg, Vector3 point, Vector3 normal, int sender)// = -1)
	{
		damage = dmg;
		hitPoint = point;
		hitNormal = normal;
        senderID = sender;
	}

	public HitData(int dmg, Vector3 point, Vector3 normal, int sender)// = -1)
	{
		damage = new DAMAGE(dmg);
		hitPoint = point;
		hitNormal = normal;
        senderID = sender;
    }
}
	
[RequireComponent(typeof(CharacterHandler))]
public abstract class Character : NetworkBehaviour
{
	public int id = 0;
//	public int team = 0;

	public Character[] allies;

	public CharacterHandler handler;

	public int maxHealth;
	public int health;

	public float atkTimer;

	public int maxArmour;
	public int armour;

	public DAMAGE primaryDmg;
	public GameObject primaryProj;
	protected float primaryTimer;
	public float primaryDelay;
	public float primaryRange;

	public Ability[] abilities = new Ability[3];

	bool start = true;


	// Use this for initialization
	protected virtual void Start ()
    {
		handler = GetComponent<CharacterHandler> ();

	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
		if (start)
		{
			start = false;

            if (hasAuthority)
            {
                //ult starts uncharged
                abilities[2].StartTimer();
                abilities[2].SetActive(false);

            }
		}

		if (hasAuthority && tag != "Dummy")
		{
			if (!handler.frozen)
			{
				HandleAttacks();
			}
			HandleCooldowns();
		}
	}

	protected virtual void HandleAttacks()
	{
		if (Input.GetKeyDown (handler.controls.primaryAtk))
		{
			if (atkTimer <= 0)
			{
				PrimaryAttack ();
			}
		}

		if (Input.GetKeyDown (handler.controls.Ability1))
		{
			if (abilities [0].timer <= 0)
			{
				UseAbility (0);
			}
		}

		if (Input.GetKeyDown (handler.controls.Ability2))
		{
			if (abilities [1].timer <= 0)
			{
				UseAbility (1);
			}
		}

		if (Input.GetKeyDown (handler.controls.ult))
		{
			if (abilities [2].timer <= 0)
			{
				UseAbility (2);
			}
		}
	}

	protected virtual void HandleCooldowns()
	{
		if (primaryTimer > 0f)
		{
			primaryTimer -= Time.deltaTime;
		}

		for (int i = 0; i < abilities.Length; i++)
		{
			if (abilities [i].timer > 0f)
			{
				abilities [i].timer -= Time.deltaTime;
			}

			if (abilities [i].timer <= 0f)
			{
				abilities [i].timer = 0f;
			}

			if (abilities [i].isDurationElapsed () && abilities[i].active)
			{
				abilities [i].SetActive (false);
				OnAbilityExpired (i);
			}
		}
	}



	public abstract void PrimaryAttack ();

	public void UseAbility (int index)
	{
		abilities [index].UseAbility ();
	}

	public void OnAbilityExpired (int index)
	{
		abilities [index].AbilityExpired ();
	}
}
