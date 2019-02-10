using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {

	public Transform muzzle;
	public int damage;
	Vector3 dirToTarget;

	// Use this for initialization
	void Start () {
		dirToTarget = muzzle.forward;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Fire()
	{
		//raycast from muzzle and hit first thing found
		RaycastHit hit;
		if (Physics.Raycast (muzzle.position, dirToTarget, out hit))
		{
			hit.collider.gameObject.SendMessageUpwards ("Hit", new HitData(damage, hit.point, hit.normal), SendMessageOptions.DontRequireReceiver);
		}
	}
}
