using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct Controls
{
	public KeyCode forward;
	public KeyCode back;
	public KeyCode left;
	public KeyCode right;

	public KeyCode jump;
	public KeyCode crouch;
	public KeyCode sprint;

	public KeyCode primaryAtk;
	public KeyCode secondaryAtk;
	public KeyCode ult;

	public KeyCode Ability1;
	public KeyCode Ability2;
}

[RequireComponent(typeof(Rigidbody))]
public class CharacterHandler: NetworkBehaviour
{

	public Controls controls;
	public float speed = 350f;
	public float sprintMultiplier = 2f;

	public float climbAngleMax;

	public float jumpForce = 10f;
	public float jumpVelMod = .01f;
	public Rigidbody rb;
	public CapsuleCollider mainCollider;

    //this is a translation which makes vectors relative to the surface 
	Quaternion surfaceRotation;


	Vector3 momentumVel = Vector3.zero;

	public float lookSpeed = 2f;
	float initLookSpeed;

	float initFOV;
	bool FOVbumped = false;

	public Camera cam;
	public float minLookHeight = -60f;
	public float maxLookHeight = 60f;

	public Transform headPos;

	public bool grounded;

	public bool frozen = false;
	public bool freezeLook = false;

	public List<Powerup> powerups = new List<Powerup>();

	//temporary
	bool paused = false;

    private bool startedWithauthority = false;


	// Use this for initialization
	void Start ()
    {
        
        //intitialise variables
        if(!cam)
		    cam = GetComponentInChildren<Camera> ();
		rb = GetComponent<Rigidbody> ();

        initLookSpeed = lookSpeed;
        if (cam)
            initFOV = cam.fieldOfView;
        

		if (tag != "Dummy" && hasAuthority) //if this is the character of this client
        {
            Log.current.LogData("Start Character Handler");
            //enable the camera 
            cam.gameObject.SetActive(true);


            //initialise controls
            controls.forward = KeyCode.W;
            controls.back = KeyCode.S;
            controls.left = KeyCode.A;
            controls.right = KeyCode.D;

            controls.jump = KeyCode.Space;
            controls.sprint = KeyCode.LeftShift;

            controls.primaryAtk = KeyCode.Mouse0;
            controls.secondaryAtk = KeyCode.Mouse1;
            controls.ult = KeyCode.LeftAlt;

            controls.Ability1 = KeyCode.Alpha1;
            controls.Ability2 = KeyCode.Alpha2;


            Cursor.lockState = CursorLockMode.Locked;

            startedWithauthority = true;
        }        
	}

	// Update is called once per frame
	void Update ()
    {
		if (tag != "Dummy" && hasAuthority) //if this character belongs to this client
        {
            //If has authority and was not started with authority call start again - this fixes a bug on the server
            if (!startedWithauthority)
                Start();
                   

            RaycastHit hit;

            grounded = Physics.SphereCast(transform.position + new Vector3(0f, (mainCollider.radius - .1f) + .1f, 0f), mainCollider.radius - .1f, Vector3.down, out hit, .15f, GameHandler.current.groundLayer);

            rb.useGravity = !grounded;

            surfaceRotation = Quaternion.identity;

            if (grounded)
            {
                if (Vector3.Angle(transform.up, hit.normal) <= climbAngleMax)
                {
                    surfaceRotation = Quaternion.FromToRotation(transform.up, hit.normal);
                }

            }



            CheckPowerups();

            //movement
            ProcessMovement();



            //unlock cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                paused = !paused;

                if (paused)
                {
                    Cursor.lockState = CursorLockMode.None;
                    lookSpeed = 0f;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    lookSpeed = initLookSpeed;
                }
            }
        }
	}

	void ProcessMovement()
	{
        //animation things here - probably some state management would be helpful


		momentumVel = rb.velocity; //save current velocity


		if (!frozen)
		{

			Vector3 movement = Vector3.zero;
			float speedmod = 1;
			float jumpMod = 1;


			movement = GetInput (); //Get Directional Input


			//speedPowerup
			if (powerups.Exists (x => x.powerupType.ToString () == "Speed"))
			{
				speedmod *= powerups.Find (x => x.powerupType.ToString () == "Speed").multiplier;
			}

			//jumpPowerup
			if (powerups.Exists (x => x.powerupType.ToString () == "Jump"))
			{
				jumpMod *= powerups.Find (x => x.powerupType.ToString () == "Jump").multiplier;
			}

				

			if (grounded)
			{
				//sprint

				if (Input.GetKey (controls.sprint))
				{
					if (!FOVbumped)
					{
						FOVUp ();
					}
					speedmod *= sprintMultiplier;
				}
				else
				{
					if (FOVbumped)
					{
						ResetFOV ();
					}
				}


				//apply movement velocity
				rb.velocity = ((surfaceRotation * transform.TransformDirection (movement))  * speed * speedmod) * Time.deltaTime;

				momentumVel = rb.velocity;

				//jump
				if (Input.GetKeyDown (controls.jump))
				{
					//rb.isKinematic = false;
					rb.velocity = momentumVel + new Vector3 (0f, jumpForce * jumpMod, 0f);
				}

			}
			else
			{
				speedmod *= jumpVelMod;

				//apply movement velocity ontop of current momentum
				rb.velocity = momentumVel + ((transform.TransformDirection (movement) * speed * speedmod) * Time.deltaTime);
			}
				

		}

		if (!freezeLook)
		{

			//mouse look

			float xRot, yRot;

			xRot = Input.GetAxis ("Mouse X");
			yRot = Input.GetAxis ("Mouse Y");

			transform.Rotate (Vector3.up, xRot * lookSpeed);

			//if looking up or down goes over 60 deg in either direction, clamp
			//cam.transform.Rotate (Vector3.right, (-yRot) * lookSpeed);

			Quaternion camRot = cam.transform.localRotation * Quaternion.Euler (-yRot * lookSpeed, 0f, 0f);

			camRot = ClampRotationAroundXAxis (camRot);

			cam.transform.localRotation = camRot;

		}

	}

    //locks character movement and camera
	public void Freeze(bool a_freezeLook = true)
	{
		if (tag != "Dummy")
		{
			freezeLook = a_freezeLook;
			frozen = true;
		}
		else
		{
			rb.isKinematic = true;
		}
	}

	public void Unfreeze()
	{
		if (tag != "Dummy")
		{
			frozen = false;
		}

		rb.isKinematic = false;
	}



	private Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

		angleX = Mathf.Clamp (angleX, minLookHeight, maxLookHeight);

		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

    //returns the combined directional input
	private Vector3 GetInput()
	{
		Vector3 movement = Vector3.zero;

		//forward
		if (Input.GetKey (controls.forward))
		{
			movement += new Vector3 (0f, 0f, 1f);
		}

		//back
		if (Input.GetKey (controls.back))
		{
			movement += new Vector3 (0f, 0f, -1f);
		}

		//left
		if (Input.GetKey (controls.left))
		{
			movement += new Vector3 (-1f, 0f, 0f);
		}

		//right
		if (Input.GetKey (controls.right))
		{
			movement += new Vector3 (1f, 0f, 0f);
		}

		return movement;
	}

	private void CheckPowerups()
	{
		List<int> toRemove = new List<int> ();

		for (int i = 0; i < powerups.Count; i++)
		{
			powerups[i].timer -= Time.deltaTime;

			//Debug.Log (powerups [i].powerupType.ToString () + ": " + powerups [i].timer);

			if (powerups[i].timer <= 0f)
			{
				//mark powerup to remove if expired
				toRemove.Add(i);
			}
		}

		//remove all marked powerups
		for(int i = 0; i < toRemove.Count; i++)
		{
			powerups.RemoveAt (toRemove [i]);
		}
	}


	private void FOVUp()
	{
		cam.fieldOfView += 10f;
		FOVbumped = true;
	}

	private void ResetFOV()
	{
		cam.fieldOfView = initFOV;
		FOVbumped = false;
	}


	//void OnCollisionEnter(Collision other)
	//{
	//
	//	//if object is ground, ground player
	//	if (GameHandler.current.groundLayer == (GameHandler.current.groundLayer | (1 << other.gameObject.layer)))
	//	{
	//		RaycastHit hit;
	//		if (Physics.Raycast(transform.position + new Vector3(0f, .2f, 0f), Vector3.down, out hit, .3f, GameHandler.current.groundLayer))
	//		{ 
	//			if (other.contacts [0].normal.y >= 0.6f)
	//			{
	//				grounded = true;
	//				//rb.useGravity = false;
	//				rb.isKinematic = true;
	//				//transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
	//
	//				surfaceRotation = Quaternion.FromToRotation (transform.up, hit.normal);
	//			}
	//
	//			//Debug.DrawRay(transform.position, (surfaceRotation * transform.forward) * 100, Color.red);
	//
	//		}
	//	}
	//}

	//void OnCollisionExit(Collision other)
	//{
	//	
	//	//if object is ground, ground player
	//	if (GameHandler.current.groundLayer == (GameHandler.current.groundLayer | (1 << other.gameObject.layer)))
	//	{
	//		grounded = false;
	//        rb.useGravity = true;
	//		rb.isKinematic = false;
	//    }
	//}

	public void OnTriggerEnter(Collider other)
	{
        if (other.GetComponent<Powerup>())
        {
            if (hasAuthority)
            {
            
                Powerup powerup = other.GetComponent<Powerup>();

                //check if powerup type is already in effect
                if (powerups.Exists(x => x.powerupType.ToString() == powerup.powerupType.ToString()))
                {
                    //if effect is already in use, just add to timer
                    powerups.Find(x => x.powerupType.ToString() == powerup.powerupType.ToString()).timer += powerup.timer;
                }
                else
                {
                    //if powerup is not in effect, start effect
                    powerups.Add(powerup);
                }

                //destroy the object on all clients
                NetworkServer.Destroy(other.gameObject);
                //This can cause an error if 2 clients do this at the same time 
            }
        }
	}

    
    [ClientRpc]
    //assigns the character a team - should inform more things
    public void RpcSetTeam(int teamNum)
    {
        if (teamNum == 1)
            tag = "Team1";
        else
            tag = "Team2";

        Log.current.LogData("Team " + teamNum.ToString());
    }
}
