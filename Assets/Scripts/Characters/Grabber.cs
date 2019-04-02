using System.Collections;
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
        //if (hasAuthority)
        //{
        //    if (returning)
        //    {
        //        if (grappleType == GrappleType.Grab)
        //        {
        //            //make sure to always get back to owner
        //            Vector3 dirBack = (owner.handler.cam.transform.position - transform.position).normalized;
        //            rb.velocity = dirBack * startSpeed * Time.deltaTime;
        //            if(mover)//if holding another player
        //            {
        //                //move the player root by the amount the grabber has moved
        //                mover.transform.position += (this.transform.position - previousPosition);
        //                previousPosition = this.transform.position;
        //            }
        //        }
        //        else if (grappleType == GrappleType.Grapple)
        //        {
        //            //move player
        //            Vector3 dirToGrapple = (transform.position - owner.handler.cam.transform.position).normalized;
        //            owner.transform.position += dirToGrapple * startSpeed * Time.deltaTime;
        //
        //            if (owner.handler.grounded)
        //            {
        //                owner.handler.rb.velocity = Vector3.zero;
        //                owner.handler.Unfreeze(); //let go of player
        //                owner.handler.rb.useGravity = true;
        //                mover.GetComponentInChildren<Network_Transform>().CmdRemoveServerControl();
        //
        //
        //                NetworkServer.Destroy(transform.root.gameObject);
        //            }
        //        }
        //
        //        //if close enough to owner, destroy
        //        if (Vector3.Distance(transform.position, owner.transform.position) <= 1f)
        //        {
        //            if (grappleType == GrappleType.Grab)
        //            {
        //                if (otherChar)
        //                {
        //                    otherChar.handler.Unfreeze(); //let go of other char
        //                    mover.parent = null;
        //                    mover.GetComponentInChildren<Network_Transform>().CmdRemoveServerControl(); //give the player back control of their position
        //                }
        //            }
        //            else if (grappleType == GrappleType.Grapple)
        //            {
        //                owner.handler.rb.velocity = Vector3.zero;
        //                owner.handler.Unfreeze(); //let go of player
        //                owner.handler.rb.useGravity = true;
        //                mover.GetComponentInChildren<Network_Transform>().CmdRemoveServerControl();
        //            }
        //
        //            NetworkServer.Destroy(transform.root.gameObject);
        //        }
        //    }
        //    else
        //    {
        //        //if the grabber gets too far, return it
        //        if (Vector3.Distance(transform.position, initPos) >= maxDist)
        //        {
        //            Return();
        //        }
        //    }
        //}




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



    //private void Return()
	//{
	//	returning = true;
    //
	//	if (grappleType == GrappleType.Grab)
	//	{
	//		//move grabber
	//		Vector3 dirBack = (owner.handler.cam.transform.position - transform.position).normalized;
	//		rb.velocity = dirBack * startSpeed * Time.deltaTime;
	//	}
	//	else if (grappleType == GrappleType.Grapple)
	//	{ 
	//		rb.velocity = Vector3.zero;
    //
    //        owner.GetComponentInChildren<Network_Transform>().CmdTakeServerControl();
    //
    //        //jump un petite peux
    //        //owner.handler.transform.position += new Vector3(0f, .5f, 0f);
    //        //gun bob animation
    //        owner.handler.gunAnim.SetBool("moving", false);
    //        owner.handler.grounded = false;
    //
    //        //move player
    //        Vector3 dirFromPlayer = (transform.position - owner.handler.cam.transform.position).normalized;
	//		owner.handler.Freeze (false);
	//		owner.handler.rb.useGravity = false;
	//		owner.handler.rb.velocity = dirFromPlayer * startSpeed * Time.deltaTime;
	//	}
	//}


    private void FreezeCharacter(Character character)
    {
        character.GetComponent<Network_Transform>().CmdTakeServerControl();
        character.GetComponent<CharacterHandler>().Freeze(false);
    }

    private void UnFreezeCharacter(Character character)
    {
        character.GetComponent<Network_Transform>().CmdRemoveServerControl();
        character.GetComponent<CharacterHandler>().Unfreeze();
    }

    private void GrabberDestroy()
    {
        if (otherChar)
        {
            UnFreezeCharacter(otherChar);
            otherChar = null;
        }
        returning = false;
        mover = null;
        grappleActive = false;

        NetworkServer.Destroy(this.gameObject);
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
                            FreezeCharacter(other.gameObject.GetComponent<Character>());
                            otherChar = other.gameObject.GetComponent<Character>();
                            mover = this.transform; //the grabber will move back towards the player and the character will be moved with it.
                            returning = true;
                            grappleActive = true;
                        }
                    }
                    else
                    {
                        returning = true;
                        mover = this.transform;
                    }
                }
                else if (grappleType == GrappleType.Grapple)
                {
                    if (!other.gameObject.GetComponentInParent<Character>())//not a player
                    {
                        FreezeCharacter(owner);
                        otherChar = owner;
                        mover = owner.transform;
                        returning = true;
                        grappleActive = true;
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

        //    if (hasAuthority)
        //{
        //    //ignore hitting owner
        //    if (!other.gameObject.transform.IsChildOf(owner.gameObject.transform))
        //    {
        //        if (!returning)
        //        {
        //            //on collision with anything, stop and go back.
        //
        //            if (grappleType == GrappleType.Grab)
        //            {
        //                //if its a player, bring them with you
        //                if (other.gameObject.GetComponentInParent<Character>() && //if is player
        //                    (other.gameObject.GetComponentInParent<Character>().handler.GetTeam() != owner.handler.GetTeam())) //and not on same team
        //                {
        //                    //bring the hit character back with us
        //                    otherChar = other.gameObject.GetComponentInParent<Character>();
        //                    otherChar.handler.Freeze(false); //freeze other character
        //                    mover = otherChar.transform.root; //hold reference to the root of the player
        //                    previousPosition = this.transform.position; //set to position when the player was grabbed
        //                    mover.GetComponentInChildren<Network_Transform>().CmdTakeServerControl(); //give the server control over the players position
        //                    //otherRoot.SetParent(this.transform);
        //                }
        //            }
        //
        //            Return(); 
        //        }
        //        else if (otherChar)
        //        {
        //            if (!other.gameObject.transform.IsChildOf(mover.gameObject.transform))
        //            {
        //                if (grappleType == GrappleType.Grab)
        //                {
        //                    if (otherChar)
        //                    {
        //                        otherChar.handler.Unfreeze(); //let go of other char
        //                        mover.parent = null;
        //                        mover.GetComponentInChildren<Network_Transform>().CmdRemoveServerControl(); //give the player back control of their position
        //                    }
        //
        //                    NetworkServer.Destroy(transform.root.gameObject);
        //                }
        //            }
        //        }
        //    }
        //    else //if hit owner
        //    {
        //        if (returning)
        //        {
        //            if (grappleType == GrappleType.Grab)
        //            {
        //                if (otherChar)
        //                {
        //                    otherChar.handler.Unfreeze(); //let go of other char
        //                    mover.parent = null;
        //                    mover.GetComponentInChildren<Network_Transform>().CmdRemoveServerControl(); //give the player back control of their position
        //                }
        //            }
        //            else if (grappleType == GrappleType.Grapple)
        //            {
        //                owner.handler.rb.velocity = Vector3.zero;
        //                owner.handler.Unfreeze(); //let go of player
        //                owner.handler.rb.useGravity = true;
        //                owner.GetComponentInChildren<Network_Transform>().CmdRemoveServerControl(); //give the player back control of their position
        //            }
        //
        //            NetworkServer.Destroy(transform.root.gameObject);
        //        }
        //    }
        //}
	}
}
