using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DestroyParticle : MonoBehaviour {

	ParticleSystem ps;
    public float delay;
    float timer;

	// Use this for initialization
	void Start () {
		ps = GetComponent<ParticleSystem> ();
        timer = delay;
	}
	
	// Update is called once per frame
	void Update () {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if (ps.isStopped)
            {
                Destroy(transform.root.gameObject);
            }
        }
	}
}
