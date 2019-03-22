﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public List<Transform> owners = new List<Transform> ();
    public int senderID;
	public float speed = 1f;
	public ParticleSystem[] hitEffects;
	public GameObject explosion;
    public float effectRadius;
    bool playing = false;
    bool destroying = false;
	public DAMAGE dmg;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * (speed * Time.deltaTime);

        if (GetComponent<ParticleSystem>().isPlaying)
        {
            playing = true;
        }


        if (!GetComponent<ParticleSystem>().isPlaying && playing)
        {
            playing = false;
			Dissipate();
        }
	}

	void Dissipate()
	{
        if (!destroying)
        {
            GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

            destroying = true;

            speed = 0f;
            Destroy(gameObject, hitEffects[0].main.duration);
            foreach (ParticleSystem hitEffect in hitEffects)
            {
                hitEffect.Play();
            }

			if (explosion)
			{
				Instantiate (explosion, transform.position, Quaternion.identity);
			}
        }
	}



    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag != "Projectile")
        {
            if (!owners.Exists(x => x.root == other.transform.root)) //if it hits a collider that is not in the list of owners, dissipate
            {
                Vector3 startPos = transform.position - (transform.forward * 1.3f);

                //Debug.DrawRay(startPos, (transform.forward * 3), Color.blue, 10);

                List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(startPos, transform.forward, 3f));

                //raycast from adjusted position to the collider and readjust projectile position so that effect does not start inside object

                if (hits.Count > 0)
                {
                    Vector3 normal, point;

                    if (hits.Exists(x => x.collider == other))
                    {
                        Debug.Log("Projectile Realigned");
                        int index = hits.FindIndex(x => x.collider == other);
                        transform.position = hits[index].point;
                        point = hits[index].point;
                        normal = hits[index].normal;
                    }
                    else
                    {
                        point = transform.position;
                        normal = -transform.forward;
                    }

                    //apply blast radius
                    if (effectRadius > 0f)
                    {
                        Collider[] targets = Physics.OverlapSphere(transform.position, effectRadius);
                        foreach (Collider target in targets)
                        {
                            Debug.Log(target.transform.root.name + " was caught in the blast!");

                            //calculate multiplier based on distance from explosion
                            float multiplier = 1 - (Mathf.Clamp(Vector3.Distance(transform.position, target.ClosestPoint(transform.position)), 0f, effectRadius) / effectRadius);
                            int finalDmg = (int)(dmg.damage * multiplier);
                            target.gameObject.SendMessageUpwards("Hit", new HitData(new DAMAGE(finalDmg, dmg.armourPiercing), point, normal, senderID), SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    //or dont
                    else
                    {
                        other.gameObject.SendMessageUpwards("Hit", new HitData(dmg, point, normal, senderID), SendMessageOptions.DontRequireReceiver);//needs changing
                    }
                }
					
                //play destroy effect
                Dissipate();
            }
        }
	}
}
