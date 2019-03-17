using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability
{
	public Character caster;

	public string name;
	public float cooldown;
	public float duration; //must always be less than cooldown
	public float timer;
	public bool active;

	public void StartTimer()
	{
		timer = cooldown;
		SetActive (true);
	}

	public bool isDurationElapsed()
	{
		if(timer < (cooldown - duration))
		{
			return true;
		}
		return false;
	}

	public void SetActive(bool a_active)
	{
		active = a_active;
	}

	public abstract void Init ();

	public virtual void UseAbility()
	{
		Debug.Log (name + " was used!");
	}

	public abstract void AbilityExpired ();
}
