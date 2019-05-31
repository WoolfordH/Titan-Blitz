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

	public CharacterHandler handler;

	public int maxHealth;
	public float health;

	public int maxArmour;
	public float armour;

    public float armourRegenSpeed;
    public float armourRegenDelay;

	public DAMAGE primaryDmg;
    public int initDamage;
    [HideInInspector]
    public float dmgMod = 1f;
    public GameObject primaryProj;
	protected float primaryTimer;
	public float primaryDelay;
	public float primaryRange;
    public float timeToMaxSpread;
    public float crosshairResetSpeed;
    protected float firingTimer = 0f;
    protected bool firedThisFrame;

    public Ability[] abilities = new Ability[2];

    public AudioClip primaryFireClip;
    public AudioClip[] enableClip;
    public AudioClip[] disableClip;

	bool start = true;


	// Use this for initialization
	protected virtual void Start ()
    {
		handler = GetComponent<CharacterHandler> ();
        primaryDmg = new DAMAGE(initDamage);
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
                abilities[1].StartTimer(TimerType.cooldown);
                abilities[1].SetActive(false);

            }
		}

		if (hasAuthority && tag != "Dummy")
		{
            if (!handler.frozen && !handler.paused)
            {
                HandleAttacks();
            }
            else
            {
                firedThisFrame = false;
            }

            if (firingTimer >= 0f)
            {
                if (!firedThisFrame)
                {
                    firingTimer -= Time.deltaTime;
                }
                handler.crosshair.Play("Crosshair", 0, firingTimer / timeToMaxSpread);
                handler.crosshair.speed = 0f;
            }

            HandleCooldowns();
		}
	}

	protected virtual void HandleAttacks(bool automatic = false)
	{
 
        if (automatic)
        {
            if (Input.GetKey(handler.controls.primaryAtk))
            {
                if (primaryTimer <= 0)
                {
                    //audio
                    if (primaryFireClip != null)
                    {
                        CmdPlaySoundFire();
                    }

                    PrimaryAttack();
                }
            }
            else
            {
                firedThisFrame = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(handler.controls.primaryAtk))
            {
                if (primaryTimer <= 0)
                {
                    //audio
                    if (primaryFireClip != null)
                    {
                        CmdPlaySoundFire();
                    }

                    PrimaryAttack();
                }
            }
            else
            {
                firedThisFrame = false;
            }
        }

        

        //Debug.Log("firing timer: " + firingTimer);

        if (Input.GetKeyDown (handler.controls.Ability1))
		{
			if (!abilities[0].active && abilities [0].timer <= 0)
			{
				UseAbility (0);
			}
		}

		//if (Input.GetKeyDown (handler.controls.Ability2))
		//{
        //    if (!abilities[1].active && abilities[1].timer <= 0)
        //    {
		//		UseAbility (1);
		//	}
		//}

		if (Input.GetKeyDown (handler.controls.ult))
		{
            if (!abilities[1].active && abilities[1].timer <= 0)
            {
				UseAbility (1);
			}
		}
	}

    //=============================AUDIO NETWORKING============================\\
    [Command]
    private void CmdPlaySoundFire()
    {
        RpcPlaySoundFire();
    }

    [ClientRpc]
    private void RpcPlaySoundFire()
    {
        handler.audioSource.PlayOneShot(primaryFireClip);
    }

    [Command]
    public void CmdPlaySoundEnable(int abilityIndex)
    {
        RpcPlaySoundEnable(abilityIndex);
    }

    [ClientRpc]
    private void RpcPlaySoundEnable(int abilityIndex)
    {
        handler.audioSource.PlayOneShot(enableClip[abilityIndex]);
    }

    [Command]
    public void CmdPlaySoundDisable(int abilityIndex)
    {
        RpcPlaySoundDisable(abilityIndex);
    }

    [ClientRpc]
    private void RpcPlaySoundDisable(int abilityIndex)
    {
        handler.audioSource.PlayOneShot(disableClip[abilityIndex]);
    }

    //=========================================================================\\

    protected virtual void HandleCooldowns()
	{
		if (primaryTimer > 0f)
		{
			primaryTimer -= Time.deltaTime;
		}

		for (int i = 0; i < abilities.Length; i++)
		{
            if (abilities[i].active)
            {
                if (abilities[i].isDurationElapsed())
                {
                    abilities[i].SetActive(false);
                    abilities[i].StartTimer(TimerType.cooldown);
                    OnAbilityExpired(i);
                }
            }
		}
	}


    protected void InitialiseAbilities()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            abilities[i].Init();
            //abilities[i].audioSource = handler.audioSource;
        }
    }

    public void ResetAbilities()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            abilities[i].ForceEnd();
        }
    }


    public abstract void PrimaryAttack ();

	public void UseAbility (int index)
	{
		abilities [index].UseAbility ();
        abilities[index].StartTimer(TimerType.duration);
    }

	public void OnAbilityExpired (int index)
	{
		abilities [index].AbilityExpired ();
	}
}
