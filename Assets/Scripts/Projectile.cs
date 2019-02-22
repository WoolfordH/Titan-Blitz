using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public List<Transform> owners = new List<Transform> ();
	public float speed = 1f;
	public ParticleSystem hitEffect;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * (speed * Time.deltaTime);

        if (!GetComponent<ParticleSystem>().isPlaying)
        {
			Disipate();
        }
	}

	void Disipate()
	{
		speed = 0f;
		Destroy(gameObject, hitEffect.main.duration);
		hitEffect.Play ();
	}



	void OnCollisionEnter(Collision other)
	{
		Debug.Log (other.gameObject.name);

		if (!owners.Exists(x=>x.root == other.transform.root)) //if it hits a collider that is not in the list of owners, disipate
		{
			Disipate ();
		}
	}
}
