using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public float speed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * (speed * Time.deltaTime);

        if (!GetComponent<ParticleSystem>().isPlaying)
        {
            Destroy(gameObject);
        }
	}
}
