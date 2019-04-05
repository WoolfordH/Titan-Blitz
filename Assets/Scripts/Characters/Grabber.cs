﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum GrappleType
{
	Grab,
	Grapple
}

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grabber : NetworkBehaviour {

	public Character owner;
	public float maxDist;
	private Vector3 dir; //world-space
	public float startSpeed;
    public float returnSpeed;

	public GrappleType grappleType;

	private Vector3 initPos;
	private Rigidbody rb;

	private bool returning = false;
    private bool grappleActive = false;

	private Character otherChar;
	private Transform mover; // this is the object that will move after collision has happened
    private Vector3 moverTarget;

    private Vector3 previousPosition;

	public void Init (Character a_owner, Vector3 a_dir, Vector3 initVelocity) 
	{
		rb = GetComponent<Rigidbody> ();

		owner = a_owner;
		dir = a_dir;
        //*= owner.handler.speed;

        initPos = transform.position;

        //maxPos = owner.transform.position + (dir * maxDist);
        //vecToMax = maxPos - owner.transform.position;
        rb.velocity = initVelocity + dir * startSpeed; //(dir * speed * Time.deltaTime);
	}
	
	// Update is called once per frame
	void Update () 
	{
        if(hasAuthority)
        {
            if(!returning)
            {
                //it keeps its velocity, do nothing
            }
            else //is returning
            {
                FindMoverTarget();
                MoveMover();
                if(grappleActive)
                    if(grappleType == GrappleType.Grab)
                    {
                        Vector3 movement = mover.transform.position - previousPosition;
                        otherChar.transform.position += movement;
                        previousPosition = mover.transform.position;
                    }
                    else
                    {
                        //grapple, nothing else needs to happen
                    }                
            }
        }

        if(Vector3.Distance(this.transform.position,owner.transform.position)>maxDist)//too far away
        {
            mover = this.transform;
            Return();
        }

	}

    private void FindMoverTarget()
    {
        if (!grappleActive)
        {
            moverTarget = owner.transform.position;
        }
        else
        {
            if(grappleType == GrappleType.Grab)
            {
                moverTarget = owner.transform.position;
            }
            else //grapple type = grapple
            {
                moverTarget = this.transform.position;
            }
        }
    }

    private void MoveMover()
    {
        Vector3 dir = (moverTarget - mover.transform.position).normalized;
        mover.transform.position += dir * returnSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
	{
        if (hasAuthority)
        {
            if (!other.gameObject.transform.IsChildOf(owner.gameObject.transform)) //if not self
            {
                if (grappleType == GrappleType.Grab)
                {
                    if (other.gameObject.GetComponentInParent<Character>()) //if is a character
                    {
                        if (other.gameObject.GetComponentInParent<Character>().handler.GetTeam() != owner.handler.GetTeam()) //if enemy
                        {
                            FreezeCharacter(other.gameObject.GetComponentInParent<Character>());
                            otherChar = other.gameObject.GetComponentInParent<Character>();
                            mover = this.transform; //the grabber will move back towards the player and the character will be moved with it.
                            grappleActive = true;
                            previousPosition = mover.transform.position;
                            Return();
                        }
                    }
                    else
                    {
                        mover = this.transform;
                        Return();
                    }
                }
                else if (grappleType == GrappleType.Grapple)
                {
                    if (!other.gameObject.GetComponentInParent<Character>())//not a player
                    {
                        
                        FreezeCharacter(owner);
                        otherChar = owner;
                        mover = owner.transform;
                        owner.handler.rb.useGravity = false;
                        grappleActive = true;
                        Return();
                    }
                }
            }
            else //is self
            {
                if (returning)//returning
                {
                    GrabberDestroy();

                }
                //else just spawned.
            }
        }
	}

    private void FreezeCharacter(Character character)
    {
        character.GetComponent<Network_Transform>().CmdTakeServerControl();
        character.GetComponent<CharacterHandler>().Freeze(false);
        character.handler.rb.isKinematic = true;
    }

    private void UnFreezeCharacter(Character character)
    {
        character.GetComponent<Network_Transform>().CmdRemoveServerControl();
        character.GetComponent<CharacterHandler>().Unfreeze();
    }

    private void Return()
    {
        returning = true;
        this.rb.isKinematic = true;
    }

    private void GrabberDestroy()
    {
        if (otherChar)
        {
            UnFreezeCharacter(otherChar);
            otherChar.handler.rb.useGravity = true;
            otherChar = null;
        }
        returning = false;
        mover = null;
        grappleActive = false;

        NetworkServer.Destroy(this.gameObject);
    }
}