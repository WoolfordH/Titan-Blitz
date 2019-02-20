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

	public HitData(DAMAGE dmg, Vector3 point, Vector3 normal)
	{
		damage = dmg;
		hitPoint = point;
		hitNormal = normal;
	}

	public HitData(int dmg, Vector3 point, Vector3 normal)
	{
		damage = new DAMAGE(dmg);
		hitPoint = point;
		hitNormal = normal;
	}
}

public struct Ability
{
	public string name;
	public float cooldown;
	public float duration; //must always be less than cooldown
	public float timer;

	public Ability(string n, float cool)
	{
		name = n;
		cooldown = cool;
		duration = 0f;
		timer = 0f;
	}

	public Ability(string n, float cool, float dur)
	{
		name = n;
		cooldown = cool;
		duration = dur;
		timer = 0f;
	}

	public void StartTimer()
	{
		timer = cooldown;
	}

	public bool isDurationElapsed()
	{
		if(timer < (cooldown - duration))
		{
			return true;
		}
		return false;
	}
}

[RequireComponent(typeof(CharacterHandler))]
public abstract class Character : NetworkBehaviour
{
	public int id = 0;
	public int team = 0;

	public Character[] allies;

	public CharacterHandler handler;
	public GameObject hitMarker;
	public Text timerLbl;
	public Image healthBar;
	public Image armourBar;
	public Image ultBar;
	public Text ammoLbl;
	public Image abl1Icon;
	public Image abl2Icon;

	public RawImage ally1Avatar;
	public RawImage ally2Avatar;

	//public Material avatarMat;
	public RenderTexture avatarRT;
	public Camera avatarCam;

	public int maxHealth;
	public int health;

	public int maxArmour;
	public int armour;

	public DAMAGE primaryDmg;
	public float primaryRange;
	public DAMAGE secondaryDmg;
	public float secondaryRange;

	public Ability[] abilities = new Ability[5];

	bool start = true;


	// Use this for initialization
	protected virtual void Start ()
    {
		handler = GetComponent<CharacterHandler> ();

		//AVATAR INIT
		avatarRT = new RenderTexture(512,512, 16);
		avatarCam.targetTexture = avatarRT;
		//avatarMat = new Material (Shader.Find("Unlit/Texture"));
		//avatarMat.mainTexture = avatarRT;

	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
		if (start)
		{
			start = false;


			//ult starts uncharged
			abilities[4].StartTimer();


			if (tag != "Dummy")
			{

				ally1Avatar.texture = allies [0].avatarRT;
				ally2Avatar.texture = allies [1].avatarRT;

				UpdateUI ();
			}
		}

		if (tag != "Dummy" && hasAuthority)
        {
            avatarCam.Render();


            if (!handler.frozen)
            {
                HandleAttacks();
            }
            HandleCooldowns();
        }


		if (handler.cam && tag != "Dummy")
		{
			//update clock UI
			int minutes = (int)GameHandler.current.timeRemaining / 60;
			int seconds = (int)GameHandler.current.timeRemaining % 60;

			timerLbl.text = minutes.ToString ("00") + ":" + seconds.ToString ("00");

			UpdateUI ();
		}
	}

	protected virtual void HandleAttacks()
	{
		if (Input.GetKeyDown (handler.controls.primaryAtk))
		{
			if (abilities [0].timer <= 0)
			{
				PrimaryAttack ();
			}
		}

		if (Input.GetKeyDown (handler.controls.Ability1))
		{
			if (abilities [2].timer <= 0)
			{
				Ability1 ();
			}
		}

		if (Input.GetKeyDown (handler.controls.Ability2))
		{
			if (abilities [3].timer <= 0)
			{
				Ability2 ();
			}
		}

		if (Input.GetKeyDown (handler.controls.ult))
		{
			if (abilities [4].timer <= 0)
			{
				Ultimate ();
			}
		}
	}

	protected virtual void HandleCooldowns()
	{
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

			if (abilities [i].isDurationElapsed ())
			{
				OnAbilityExpired (i);
			}
		}
	}

	public virtual void Hit(DAMAGE dmg)
	{
		if (enabled)
		{
			if (dmg.armourPiercing)
			{
				health -= dmg.damage;
			}
			else
			{
				if (armour > 0)
				{
					armour -= dmg.damage;
				}
				else
				{
					health -= dmg.damage;
				}
			}

			if (health <= 0)
			{
				Die ();
			}
		}
	}

	public virtual void Hit(HitData hit)
	{
		Instantiate(GameHandler.current.bloodSpurt, hit.hitPoint, Quaternion.LookRotation(hit.hitNormal));
		Hit (hit.damage);
	}

	public virtual void Die()
	{
		//Die
		Debug.Log(GetType() + " Died!");
	}

	protected IEnumerator HitMarkerFlash()
	{
		hitMarker.SetActive (true);
		yield return new WaitForSeconds (.05f);
		hitMarker.SetActive (false);
	}

	protected void UpdateUI()
	{
		healthBar.fillAmount = ((float)health/(float)maxHealth);
		armourBar.fillAmount = ((float)armour/(float)maxArmour);
		ultBar.fillAmount = (abilities[4].cooldown - abilities[4].timer)/abilities[4].cooldown;

		abl1Icon.fillAmount = (abilities[2].cooldown - abilities[2].timer)/abilities[2].cooldown;
		abl2Icon.fillAmount = (abilities[3].cooldown - abilities[3].timer)/abilities[3].cooldown;
	}

	public abstract void PrimaryAttack ();

	public abstract void Ability1 ();

	public abstract void Ability2 ();

	public abstract void Ultimate ();

	public abstract void OnAbilityExpired (int index);
}
