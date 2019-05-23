using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum TimerType
{
    cooldown,
    duration
}

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

    [HideInInspector]
    //public AudioSource audioSource;
    //public AudioClip[] enableClip;
    //public AudioClip[] disableClip;

    public void StartTimer(TimerType type)
	{
        switch (type)
        {
            case TimerType.cooldown:
                timer = cooldown;
                break;

            case TimerType.duration:
                timer = duration;
                SetActive(true);
                break;
        }		
	}

    protected virtual void Update()
    {
        if (hasAuthority)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                timer = 0f;
            }
        }
    }

    public bool isDurationElapsed()
	{
        if (active)
        {
            if (timer <= 0)
            {
                return true;
            }
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
