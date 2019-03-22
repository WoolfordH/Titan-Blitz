using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    //movement variables
	public Controls controls;
	public float speed = 350f;
	public float sprintMultiplier = 2f;

	public float climbAngleMax;

	public float jumpForce = 10f;
	public float jumpVelMod = .01f;
	public Rigidbody rb;
	public CapsuleCollider mainCollider;

	public GameObject hitMarker;
	public Text timerLbl;
	public Image healthBar;
	public Image armourBar;
	public Image ultBar;
	public Text ammoLbl;
	public Image abl1Icon;
	public Image abl2Icon;

	public Character character;

	public RawImage ally1Avatar;
	public RawImage ally2Avatar;

	//public Material avatarMat;
	public RenderTexture avatarRT;
	public Camera avatarCam;

    //this is a translation which makes vectors relative to the surface 
	Quaternion surfaceRotation;


	Vector3 momentumVel = Vector3.zero;

	public float lookSpeed = 2f;
	float initLookSpeed;

	float initFOV;
	bool FOVbumped = false;

	public float minLookHeight = -60f;
	public float maxLookHeight = 60f;

	private int lastDamageRecievedFrom = -1;

	public bool grounded;

	public bool frozen = false;
	public bool freezeLook = false;

    public Camera cam;
    public Transform headPos;
	public Transform muzzlePos;
    public Animator gunAnim;

    public List<Powerup> powerups = new List<Powerup>();

    private int teamNum = 0;



	//temporary
	bool paused = false;

    private bool started = false;


	// Use this for initialization
	void Start ()
    {
		//AVATAR INIT
		avatarRT = new RenderTexture(512,512, 16);
		avatarCam.targetTexture = avatarRT;
		//avatarMat = new Material (Shader.Find("Unlit/Texture"));
		//avatarMat.mainTexture = avatarRT;
	}

    private void Initiate()
    {
        ServerLog.current.LogData("Starting player " + ((hasAuthority && tag != "Dummy") ? "with " : "without ") + "authority");
        //intitialise variables
        if (!cam)
            cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();

        initLookSpeed = lookSpeed;
        if (cam)
            initFOV = cam.fieldOfView;

        if (tag != "Dummy" && hasAuthority) //if this is the character of this client
        {
            ServerLog.current.LogData("Start Character Handler");
            
            //enable the camera 
            cam.gameObject.SetActive(true);
            PlayerConnection.current.activeCamera = cam.gameObject;

            //pass the local controller this object
            PlayerConnection.current.playerObject = this.gameObject;

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


			//ally1Avatar.texture = allies[0].avatarRT;
			//ally2Avatar.texture = allies[1].avatarRT;

			//UpdateUI();


            Cursor.lockState = CursorLockMode.Locked;            
        }
        else
        {
            this.rb.isKinematic = true;
        }
        started = true;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        if (!started)
            Initiate();
        if (tag != "Dummy" && hasAuthority) //if this character belongs to this client
        {
            //If has authority and was not started with authority call start again - this fixes a bug on the server
            
                   

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
                gunAnim.SetFloat("speed", (float)(rb.velocity.magnitude / (speed*Time.deltaTime)));
                momentumVel = rb.velocity;

                //jump
                if (Input.GetKeyDown(controls.jump))
                {
                    //rb.isKinematic = false;
                    rb.velocity = momentumVel + new Vector3(0f, jumpForce * jumpMod, 0f);
                    //gun bob animation
                    gunAnim.SetBool("moving", false);
                }
                else
                {
                    if (rb.velocity.magnitude > 0)
                    {
                        //gun bob animation
                        gunAnim.SetBool("moving", true);
                    }
                    else
                    {
                        //gun bob animation
                        gunAnim.SetBool("moving", false);
                    }
                }

			}
			else
			{
				speedmod *= jumpVelMod;

				//apply movement velocity ontop of current momentum
				rb.velocity = momentumVel + ((transform.TransformDirection (movement) * speed * speedmod) * Time.deltaTime);
			}
				
			//avatarCam.Render();


			if (cam && tag != "Dummy" && hasAuthority)
			{
				//update clock UI
				int minutes = (int)GameHandler.current.timeRemaining / 60;
				int seconds = (int)GameHandler.current.timeRemaining % 60;

				timerLbl.text = minutes.ToString ("00") + ":" + seconds.ToString ("00");

				//UpdateUI ();
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


	protected IEnumerator HitMarkerFlash()
	{
		hitMarker.SetActive (true);
		yield return new WaitForSeconds (.05f);
		hitMarker.SetActive (false);
	}

	protected void UpdateUI()
	{
		healthBar.fillAmount = ((float)character.health/(float)character.maxHealth);
		armourBar.fillAmount = ((float)character.armour/(float)character.maxArmour);
		ultBar.fillAmount = (character.abilities[2].cooldown - character.abilities[2].timer)/character.abilities[2].cooldown;

		abl1Icon.fillAmount = (character.abilities[2].cooldown - character.abilities[2].timer)/character.abilities[2].cooldown;
		abl2Icon.fillAmount = (character.abilities[3].cooldown - character.abilities[3].timer)/character.abilities[3].cooldown;
	}


	public virtual void Hit(DAMAGE dmg)
	{
		CmdHit(dmg);
	}

	[Command]
	protected virtual void CmdHit(DAMAGE dmg)
	{
		if (enabled)
		{
			if (dmg.armourPiercing)
			{
				character.health -= dmg.damage;
			}
			else
			{
				if (character.armour > 0)
				{
					character.armour -= dmg.damage;
				}
				else
				{
					character.health -= dmg.damage;
				}
			}

			if (character.health <= 0)
			{
				Die();
			}
			else
			{
				RpcUpdateHealth(character.armour,character.health);
			}
		}
	}

	[ClientRpc]
	protected virtual void RpcUpdateHealth(int a_armour, int a_health)
	{
		character.armour = a_armour;
		character.health = a_health;

		//team ui update

		//enemy ui update

		if(hasAuthority)
		{
			//update self ui
		}
	}


	public virtual void Hit(HitData hit)
	{
		if (GameManager.current.GetPlayerObject(hit.senderID).GetComponentInChildren<CharacterHandler>().GetTeam() != GetTeam())
		{
			GameObject bloodSpurt = Instantiate(GameHandler.current.bloodSpurt, hit.hitPoint, Quaternion.LookRotation(hit.hitNormal));
			NetworkServer.Spawn(bloodSpurt);

			if (isServer) //hit should only be called from the server - this is a 
			{
				//RpcTellHit(hit.hitPoint, hit.hitNormal, hit.damage.damage);
				GameManager.current.GetPlayerObject(hit.senderID).GetComponentInChildren<CharacterHandler>().RpcTellHit(hit.hitPoint, hit.hitNormal, hit.damage.damage);
			}
			lastDamageRecievedFrom = hit.senderID;
			Hit(hit.damage);
		}
	}

	[ClientRpc]
	public void RpcTellHit(Vector3 hitPoint, Vector3 hitNormal,int damage)
	{
		if (hasAuthority)//if this is the clients player 
		{
			GameObject indicator = Instantiate(GameHandler.current.damageIndicator, hitPoint, Quaternion.LookRotation(hitNormal));
			//indicator.GetComponentInChildren<Billboard>().cam = GameHandler.current.playerCam;
			indicator.GetComponentInChildren<Text>().text = damage.ToString();
		}
	}


	public virtual void Die()
	{
		//Die
		Debug.Log(GetType() + " Died!");


		RpcDie();
		//rpc kill - simple disable, if authority enable lobby cam
		//playerconnection.SetRespawn
	}

	[ClientRpc]
	private void RpcDie()
	{
		this.gameObject.SetActive(false);
		if(hasAuthority)//if local player
		{
			PlayerConnection.current.StartRespawnTimer();
			cam.gameObject.SetActive(false);
			GameManager.current.lobbyCam.SetActive(true);
			PlayerConnection.current.activeCamera = GameManager.current.lobbyCam;
		}
	}

	[ClientRpc]
	public void RpcRespawn()
	{
		rb.velocity = Vector3.zero;
		character.health = character.maxHealth;
		character.armour = character.maxArmour;


		//reset everything
		this.gameObject.SetActive(true);
		if (hasAuthority)//if local player
		{
			transform.position = GameManager.current.GetSpawnPos(GetTeam());
			cam.gameObject.SetActive(true);
			GameManager.current.lobbyCam.SetActive(false);
		}
	}





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
    public void RpcSetTeam(int a_teamNum)
    {
        teamNum = a_teamNum;

        ServerLog.current.LogData("Team " + teamNum.ToString());
    }

    public int GetTeam()
    {
        return teamNum;
    }
}
