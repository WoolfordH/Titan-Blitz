using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public List<Transform> owners = new List<Transform> ();
	public float speed = 1f;
	public ParticleSystem[] hitEffects;
	public GameObject explosion;
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

                //Debug.DrawRay(startPos, (transform.forward * 1.3f), Color.blue);

                List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(startPos, transform.forward, 3f));

                if (hits.Count > 0)
                {
                    if (hits.Exists(x=>x.collider == other))
                    {
                        Debug.Log("Projectile Realigned");
						int index = hits.FindIndex (x => x.collider == other);
                        transform.position = hits[index].point;
						other.gameObject.SendMessageUpwards ("Hit", new HitData(dmg, hits[index].point, hits[index].normal,-1), SendMessageOptions.DontRequireReceiver);//needs changing
                    }
                }
					
                Dissipate();
            }
        }
	}
}
