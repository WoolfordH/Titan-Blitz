using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
	Speed,
	Damage,
	Jump
}

public class Powerup : MonoBehaviour {

	public float multiplier;
	public PowerupType powerupType;
	public float timer;
}
