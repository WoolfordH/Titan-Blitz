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
public class Grabber : NetworkBehaviour
{
	public Character owner;
    private Transform tip;
    [SyncVar]
    private Vector3 tipPos;

	public float maxDist;
	private Vector3 dir; //world-space
	public float startSpeed;
    public float returnSpeed;

	public GrappleType grappleType;

	private Vector3 initPos;
	private Rigidbody rb;

    public LineRenderer lr;

	private bool returning = false;
    private bool grappleActive = false;

	private Character otherChar;
	private Transform mover; // this is the object that will move after collision has happened
    private Vector3 moverTarget;

    private Vector3 previousPosition;

    //AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip grabClip;

	public void Init (Character a_owner, Transform a_tip, Vector3 a_dir, Vector3 initVelocity, float a_startSpeed, float a_returnSpeed, float a_maxDist) 
	{
        startSpeed = a_startSpeed;
        returnSpeed = a_returnSpeed;
        maxDist = a_maxDist;

		rb = GetComponent<Rigidbody> ();

		owner = a_owner;
        tip = a_tip;
		dir = a_dir;
        //*= owner.handler.speed;

        initPos = transform.position;

        //maxPos = owner.transform.position + (dir * maxDist);
        //vecToMax = maxPos - owner.transform.position;
        rb.velocity = dir * startSpeed;// + initVelocity

        //audioSource = GetComponent<AudioSource>();
        //audioSource.PlayOneShot(fireClip);
	}
	
	// Update is called once per frame
	void Update () 
	{
        lr.SetPosition(0, this.transform.position);
        lr.SetPosition(1, tipPos);

        if(hasAuthority)
        {
            tipPos = tip.position;

            if(!returning)
            {
                //it keeps its velocity, do nothing
                if (Vector3.Distance(this.transform.position, owner.transform.position) > maxDist)//too far away
                {
                    mover = this.transform;
                    Return();
                }
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
            moverTarget = owner.handler.cam.transform.position;//transform.position + new Vector3(0, 1, 0);
        }
        else
        {
            if(grappleType == GrappleType.Grab)
            {
                moverTarget = owner.transform.position;
            }
            else //grapple type = grapple
            {
                moverTarget = this.transform.position; //player transform is a floor, correct this
                
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
                            //audio
                            //audioSource.PlayOneShot(grabClip);

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
                        //audio
                        //audioSource.PlayOneShot(grabClip);

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