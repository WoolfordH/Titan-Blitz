using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityShield : Ability
{

	public GameObject shieldPrefab;
	public Vector3 shieldPosition = new Vector3(0f, 0f, 1.8f);
	GameObject shield;

    //public AbilityShield(Character c)
    //{
    //    caster = c;
    //}
    //
    //public AbilityShield(Character c, GameObject prefab)
    //{
    //    caster = c;
    //    shieldPrefab = prefab;
    //}

    public override void Init()
	{
		name = "Shield";

		timer = 0f;
		active = false;

	}

	public override void UseAbility ()
	{
        CmdAbility();
    }

    [Command]
    private void CmdAbility()
    {
        //destroys the current shield (if any)
        if (shield)
        {
            NetworkServer.Destroy(shield);
        }

        //places new shield
        shield = Instantiate(shieldPrefab, caster.transform.TransformPoint(shieldPosition), caster.transform.rotation);

        NetworkServer.Spawn(shield);
    }

	public override void AbilityExpired ()
	{
		
	}

    public override void ForceEnd()
    {
        if (shield)
        {
            NetworkServer.Destroy(shield);
        }
        timer = 0f;
        active = false;
    }
}
