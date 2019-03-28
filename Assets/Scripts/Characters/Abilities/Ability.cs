using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Ability : NetworkBehaviour
{
    [HideInInspector]
	public Character caster;

    [HideInInspector]
    public string name;
    public float cooldown;
    public float duration; //must always be less than cooldown
    [HideInInspector]
    public float timer;
    [HideInInspector]
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

    private void Start()
    {
        caster = GetComponent<Character>();
    }

    public abstract void Init ();

	public virtual void UseAbility()
	{
		Debug.Log (name + " was used!");
	}

	public abstract void AbilityExpired ();

    public abstract void ForceEnd();
}
