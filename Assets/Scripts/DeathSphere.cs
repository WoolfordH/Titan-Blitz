using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSphere : MonoBehaviour {

	public int damage;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerStay(Collider other)
	{
		other.SendMessageUpwards ("Hit", new DAMAGE(damage), SendMessageOptions.DontRequireReceiver);
	}
}
