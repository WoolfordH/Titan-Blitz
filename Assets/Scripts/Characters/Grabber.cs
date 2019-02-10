using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrappleType
{
	Grab,
	Grapple
}

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grabber : MonoBehaviour {

	public Character owner;
	public float maxDist;
	public Vector3 dir; //world-space
	public float speed;

	public GrappleType grappleType;

	Vector3 initPos;
	Rigidbody rb;
	bool returning = false;


	Character otherChar;
	Transform otherRoot;

	public void Init (Character a_owner, float a_maxDist, Vector3 a_dir, float a_speed, Vector3 initVelocity) 
	{
		rb = GetComponent<Rigidbody> ();

		owner = a_owner;
		maxDist = a_maxDist;
		dir = a_dir;
		speed = a_speed * owner.handler.speed;

		initPos = transform.position;

		//maxPos = owner.transform.position + (dir * maxDist);
		//vecToMax = maxPos - owner.transform.position;
		rb.velocity = initVelocity + (dir * speed * Time.deltaTime);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (returning)
		{
			if (grappleType == GrappleType.Grab)
			{
				//make sure to always get back to owner
				Vector3 dirBack = (owner.handler.cam.transform.position - transform.position).normalized;
				rb.velocity = dirBack * speed * Time.deltaTime;
			}
			else if (grappleType == GrappleType.Grapple)
			{
				//move player
				Vector3 dirFromPlayer = (transform.position - owner.handler.cam.transform.position).normalized;
				owner.handler.rb.velocity = dirFromPlayer * speed * Time.deltaTime;
			}
				
			if (grappleType == GrappleType.Grapple)
			{
				if (owner.handler.grounded)
				{
					owner.handler.rb.velocity = Vector3.zero;
					owner.handler.Unfreeze (); //let go of player
					owner.handler.rb.useGravity = true;

					Destroy (transform.root.gameObject);
				}
			}

			//if close enough to owner, destroy
			if (Vector3.Distance (transform.position, owner.transform.position) <= 1f)
			{
				if (grappleType == GrappleType.Grab)
				{
					if (otherChar)
					{
						otherChar.handler.Unfreeze (); //let go of other char
						otherRoot.parent = null;
					}
				}
				else if (grappleType == GrappleType.Grapple)
				{
					owner.handler.rb.velocity = Vector3.zero;
					owner.handler.Unfreeze (); //let go of player
					owner.handler.rb.useGravity = true;
				}

				Destroy (transform.root.gameObject);
			}	
		}
		else
		{
			//if the grabber gets too far, return it
			if (Vector3.Distance (transform.position, initPos) >= maxDist)
			{
				Return ();
			}
		}
	}

	void Return()
	{
		returning = true;

		if (grappleType == GrappleType.Grab)
		{
			//move grabber
			Vector3 dirBack = (owner.handler.cam.transform.position - transform.position).normalized;
			rb.velocity = dirBack * speed * Time.deltaTime;
		}
		else if (grappleType == GrappleType.Grapple)
		{
			rb.velocity = Vector3.zero;

			//move player
			Vector3 dirFromPlayer = (transform.position - owner.handler.cam.transform.position).normalized;
			owner.handler.Freeze (false);
			owner.handler.rb.useGravity = false;
			owner.handler.rb.velocity = dirFromPlayer * speed * Time.deltaTime;
		}
	}


	void OnTriggerEnter(Collider other)
	{
		//ignore hitting owner
		if (!other.gameObject.transform.IsChildOf (owner.gameObject.transform))
		{
			if (!returning)
			{
				//on collision with anything, stop and go back.

				if (grappleType == GrappleType.Grab)
				{
					//if its a player, bring them with you
					if (other.gameObject.GetComponentInParent<Character> ())
					{
						//bring the hit character back with us
						otherChar = other.gameObject.GetComponentInParent<Character> ();
						otherChar.handler.Freeze (false); //freeze other character
						otherRoot = otherChar.transform.root;
						otherRoot.SetParent (this.transform);
					}
				}

				Return ();
			}
			else if(otherChar)
			{
				if (!other.gameObject.transform.IsChildOf (otherRoot.gameObject.transform))
				{
					if (grappleType == GrappleType.Grab)
					{
						if (otherChar)
						{
							otherChar.handler.Unfreeze (); //let go of other char
							otherRoot.parent = null;
						}

						Destroy (transform.root.gameObject);
					}
				}
			}
		}
		else
		{
			if (returning)
			{
				if (grappleType == GrappleType.Grab)
				{
					if (otherChar)
					{
						otherChar.handler.Unfreeze (); //let go of other char
						otherRoot.parent = null;
					}
				}
				else if (grappleType == GrappleType.Grapple)
				{
					owner.handler.rb.velocity = Vector3.zero;
					owner.handler.Unfreeze (); //let go of player
					owner.handler.rb.useGravity = true;
				}

				Destroy (transform.root.gameObject);
			}
		}
	}
}
