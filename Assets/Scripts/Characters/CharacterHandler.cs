﻿using System.Collections;
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

    public List<int> allies = new List<int>();

    public Character character;
    public Renderer[] modelRenderers;
    public Animator characterAnim;

    public GameObject canvas;
    public GameObject hitMarker;
    public Animator crosshair;
	public Text timerLbl;
	public Image healthBar;
	public Image armourBar;
	public Image ultBar;
	public Text ammoLbl;
	public Image abl1Icon;
	public Image abl2Icon;

    public RectTransform objMarker;
    Vector2 objMarkerScreenSize;

    public Image minimap;
    public RectTransform minimapHolder;

    public GameObject captureBarHolder;
    public Image captureBar;

    public KillFeed killFeedUI;

    public Image powerupDmgIcon;
    public Image powerupJumpIcon;
    public Image powerupSpeedIcon;

    public RawImage ally1Avatar;
	public RawImage ally2Avatar;

	//public Material avatarMat;
	public RenderTexture avatarRT;
	public Camera avatarCam;


    ////////////////SOUNDS////////////////
    [HideInInspector]
    public AudioSource audioSource;

    public AudioClip[] walkClips;
    public AudioClip landClip; //Not yet implemented
    public AudioClip[] damageClips;
    public AudioClip dieClip;

    public AudioClip powerupClip;

    public float footstepDelay;
    float footstepTimer;

    //////////////////////////////////////


    //this is a translation which makes vectors relative to the surface 
    Quaternion surfaceRotation;


	Vector3 momentumVel = Vector3.zero;
    public float stickToGroundForce = 3f;


    public float lookSpeed = 2f;
	float initLookSpeed;

	float initFOV;
	bool FOVbumped = false;

	public float minLookHeight = -60f;
	public float maxLookHeight = 60f;

	private int lastDamageRecievedFrom = -1;

	public bool grounded;
    bool groundedLastFrame;

    bool jumping;

    public bool frozen = false;
	public bool freezeLook = false;

    

    public GameObject camHolder; //the object the camera and things that move with the camera are childed to 
    public IKControl ikControl;
    public GameObject characterModel;
    public Renderer teamRenderer;
    MaterialPropertyBlock propertyBlock;
    public Camera cam; //the camera 
    public Transform headPos;
	public Transform muzzlePos;
    public Animator gunAnim;
    public GameObject gun;

    public List<PowerUpData> powerups = new List<PowerUpData>();

    private int teamNum = 0;

    float armourRegenDelayTimer;

    [HideInInspector]
	public bool paused = false;

    private bool started = false;


	// Use this for initialization
	void Start ()
    {
        audioSource = GetComponent<AudioSource>();

        captureBarHolder.SetActive(false);

        //AVATAR INIT
        avatarRT = new RenderTexture(512,512, 16);
		avatarCam.targetTexture = avatarRT;

        powerupDmgIcon.enabled = false;
        powerupJumpIcon.enabled = false;
        powerupSpeedIcon.enabled = false;
        //avatarMat = new Material (Shader.Find("Unlit/Texture"));
        //avatarMat.mainTexture = avatarRT;


        objMarkerScreenSize = objMarker.rect.size * canvas.GetComponent<Canvas>().scaleFactor;
    }

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
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

        Transform XRayCam = transform.Find("CameraHolder/ViewCamera/XRayCam");
        if (XRayCam != null)
        {
            XRayCam.GetComponent<Camera>().fieldOfView = initFOV;
        }

        if (tag != "Dummy" && hasAuthority) //if this is the character of this client
        {
            ServerLog.current.LogData("Start Character Handler");

            //enable the camera holder
            cam.gameObject.SetActive(true);
            //camHolder.SetActive(true);
            characterModel.SetActive(false);
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

            //////////////////////
            //HARRY: Find allies//
            //////////////////////

			//ally1Avatar.texture = allies[0].avatarRT;
			//ally2Avatar.texture = allies[1].avatarRT;

			//UpdateUI();


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

            grounded = Physics.SphereCast(transform.position + new Vector3(0f, (mainCollider.radius - .1f) + .1f, 0f), mainCollider.radius - .1f, Vector3.down, out hit, .2f, GameHandler.current.groundLayer);

            rb.useGravity = !grounded;

            surfaceRotation = Quaternion.identity;

            characterAnim.SetBool("grounded", grounded);

            if (grounded)
            {
                if (Vector3.Angle(transform.up, hit.normal) <= climbAngleMax)
                {
                    surfaceRotation = Quaternion.FromToRotation(transform.up, hit.normal);
                }

            }


            CheckPowerups();

            if (!paused)
            {
                //movement
                ProcessMovement();
            }

            //unlock cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                paused = !paused;
                GameManager.current.PauseGame(paused);
            }
            UpdateUI();
        }


        if (isServer)
        {
            //recharge armour
            if (armourRegenDelayTimer < 0f)
            {
                //regen armour
                RechargeArmour();
            }
            else
            {
                armourRegenDelayTimer -= Time.deltaTime;
            }
        }


        if (footstepTimer > 0f)
        {
            footstepTimer -= Time.deltaTime;
        }

	}

    public void Pause(bool pause)
    {
        paused = pause;

        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lookSpeed = 0f;
            crosshair.gameObject.SetActive(false);
        }
        else
        { 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lookSpeed = initLookSpeed;
            crosshair.gameObject.SetActive(true);
        }
    }

    void ProcessMovement()
	{
        //animation things here - probably some state management would be helpful

        //if you werent grounded but now are (landing)
        if (!groundedLastFrame && grounded)
        {
            jumping = false;
        }


		momentumVel = rb.velocity; //save current velocity


		if (!frozen)
		{

			Vector3 movement = Vector3.zero;
			float speedmod = 1;
			float jumpMod = 1;


			movement = GetInput ().normalized; //Get Directional Input


			//speedPowerup
			if (powerups.Exists (x => x.type.ToString () == "Speed"))
			{
				speedmod *= powerups.Find (x => x.type.ToString () == "Speed").multiplier;
			}

			//jumpPowerup
			if (powerups.Exists (x => x.type.ToString () == "Jump"))
			{
				jumpMod *= powerups.Find (x => x.type.ToString () == "Jump").multiplier;
			}

            Vector3 moveDir;


            if ((grounded || groundedLastFrame) && !jumping)
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


                moveDir = (/*surfaceRotation * */ transform.TransformDirection(movement)) * speed * speedmod;

                //apply movement velocity

                moveDir.y = -stickToGroundForce;

                rb.velocity =  moveDir * Time.deltaTime;
                gunAnim.SetFloat("speed", (float)(rb.velocity.magnitude / (speed*Time.deltaTime)));
                momentumVel = rb.velocity;

              
                //Play Footstep Sound
                if (footstepTimer <= 0f)
                {
                    if (rb.velocity.magnitude > 0.2f)
                    {
                        //audio
                        CmdPlaySoundFootStep();

                        //normal walk speed = (speed) * deltaTime
                        footstepTimer = footstepDelay / (rb.velocity.magnitude / (speed * Time.deltaTime));
                    }
                    
                }

                characterAnim.SetFloat("velocity", 1f);
                //characterAnim.SetFloat("velocity", rb.velocity.magnitude / (speed * Time.deltaTime));

                //jump
                if (Input.GetKeyDown(controls.jump))
                {
                    jumping = true;
                    //rb.isKinematic = false;
                    rb.velocity = momentumVel + new Vector3(0f, jumpForce * jumpMod, 0f);
                    //gun bob animation
                    gunAnim.SetBool("moving", false);
                    characterAnim.SetTrigger("jump");
                }
                else
                {
                    if (movement.magnitude > 0)
                    {
                        //gun bob animation
                        gunAnim.SetBool("moving", true);
                        characterAnim.SetBool("moving", true);
                    }
                    else
                    {
                        //gun bob animation
                        gunAnim.SetBool("moving", false);
                        characterAnim.SetBool("moving", false);
                    }
                }

			}
			else
			{
				speedmod *= jumpVelMod;

                moveDir = (/*surfaceRotation * */ transform.TransformDirection(movement)) * speed * speedmod;
                moveDir += Physics.gravity * Time.deltaTime;

                //apply movement velocity ontop of current momentum
                rb.velocity = momentumVel + (moveDir * Time.deltaTime);
			}

            groundedLastFrame = grounded;

            //avatarCam.Render();


            if (cam && tag != "Dummy" && hasAuthority)
			{
				//update clock UI
				int minutes = (int)GameHandler.current.timeRemaining / 60;
				int seconds = (int)GameHandler.current.timeRemaining % 60;

				timerLbl.text = minutes.ToString ("00") + ":" + seconds.ToString ("00");

				//UpdateUI();
			}
		}

		if (!freezeLook)
		{

			//mouse look

			float xRot, yRot;

			xRot = Input.GetAxis ("Mouse X");
			yRot = Input.GetAxis ("Mouse Y");

            transform.Rotate(Vector3.up, xRot * lookSpeed);

			//if looking up or down goes over 60 deg in either direction, clamp
			//cam.transform.Rotate (Vector3.right, (-yRot) * lookSpeed);

			Quaternion camRot = camHolder.transform.localRotation * Quaternion.Euler (-yRot * lookSpeed, 0f, 0f);

			camRot = ClampRotationAroundXAxis (camRot);

            camHolder.transform.localRotation = camRot;
            ikControl.spineRotation = Quaternion.Euler(camRot.eulerAngles.z, camRot.eulerAngles.y, -camRot.eulerAngles.x);// * Quaternion.Euler(0f, 0f, 14f);


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

    //[ClientRpc]
    //public void RpcFreeze(bool a_freezeLook = true)
    //{
    //    if(hasAuthority)
    //    {
    //        Freeze(a_freezeLook);
    //    }
    //}

	public void Unfreeze()
	{
		if (tag != "Dummy")
		{
			frozen = false;
		}

        if (hasAuthority)
        {
            rb.isKinematic = false;
        }
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

        character.dmgMod = 1f;

		for (int i = 0; i < powerups.Count; i++)
		{
			powerups[i].ReduceTimer(Time.deltaTime);

			//Debug.Log (powerups [i].powerupType.ToString () + ": " + powerups [i].timer);

			if (powerups[i].timer <= 0f)
			{
				//mark powerup to remove if expired
				toRemove.Add(i);

                switch (powerups[i].type)
                {
                    case PowerupType.Damage:
                        powerupDmgIcon.enabled = false;
                        break;

                    case PowerupType.Jump:
                        powerupJumpIcon.enabled = false;
                        break;

                    case PowerupType.Speed:
                        powerupSpeedIcon.enabled = false;
                        break;
                }
            }
        }

		//remove all marked powerups
		for(int i = 0; i < toRemove.Count; i++)
		{
			powerups.RemoveAt (toRemove [i]);
		}

        if (powerups.Exists(x => x.type.ToString() == "Damage"))
        {
            character.dmgMod *= powerups.Find(x => x.type.ToString() == "Damage").multiplier;
        }
    }


	private void FOVUp()
	{
		cam.fieldOfView += 10f;

        Transform XRayCam = transform.Find("CameraHolder/ViewCamera/XRayCam");
        if (XRayCam != null)
        {
            XRayCam.GetComponent<Camera>().fieldOfView = cam.fieldOfView;
        }

        FOVbumped = true;
	}

	private void ResetFOV()
	{
		cam.fieldOfView = initFOV;

        Transform XRayCam = transform.Find("CameraHolder/ViewCamera/XRayCam");
        if (XRayCam != null)
        {
            XRayCam.GetComponent<Camera>().fieldOfView = cam.fieldOfView;
        }

        FOVbumped = false;
	}

    //TODO: When player is hit, call HitMarkerFlash on Attacker's client!
	protected IEnumerator HitMarkerFlash()
	{
		hitMarker.SetActive (true);
		yield return new WaitForSeconds (.05f);
		hitMarker.SetActive (false);
	}

	protected void UpdateUI()
	{
        //ServerLog.current.LogData("Update UI");
        //ServerLog.current.LogData(character.id.ToString() + " " + character.health.ToString() + " " + character.armour.ToString());
        //ServerLog.current.LogData("Update UI");

        healthBar.fillAmount = ((float)character.health/(float)character.maxHealth);
		armourBar.fillAmount = ((float)character.armour/(float)character.maxArmour);
		ultBar.fillAmount = (character.abilities[1].cooldown - character.abilities[1].timer)/character.abilities[1].cooldown;
        /////////////

        //update allies ui HERE!

        ////////////

        if (!character.abilities[0].active)
        {
            abl1Icon.fillAmount = (character.abilities[0].cooldown - character.abilities[0].timer) / character.abilities[0].cooldown;
        }
        if (!character.abilities[1].active)
        {
            abl2Icon.fillAmount = (character.abilities[1].cooldown - character.abilities[1].timer) / character.abilities[1].cooldown;
        }


        //Objective marker

        //Get Objective
        Transform objective = CTPManager.current.currentObj;

        //get direction to objective and place objective marker at screen point intersecting direction
        Vector3 dirToObj = objective.position - cam.transform.position;
        Vector3 screenPos = cam.WorldToScreenPoint(objective.position + new Vector3(0f, 5f, 0f));

        //screenPos = new Vector3(Mathf.Clamp(screenPos.x, 0f, cam.pixelWidth), Mathf.Clamp(screenPos.y, 0f, cam.pixelHeight), screenPos.z);

        //use z coordinate to determine if position is infront of or behind cam.
        if (screenPos.z <= 0f)
        {
            //point is behind cam

            if (screenPos.x <= Screen.width * 0.5f)
            {
                screenPos.x = Screen.width - (objMarkerScreenSize.x);
            }
            else
            {
                screenPos.x = objMarkerScreenSize.x;
            }

            if (screenPos.y <= Screen.height * 0.5f)
            {
                screenPos.y = Screen.height - objMarkerScreenSize.y;
            }
            else
            {
                screenPos.y = objMarkerScreenSize.y;
            }
        }
        else
        {
            if (screenPos.x > Screen.width - objMarkerScreenSize.x)
            {
                screenPos.x = Screen.width - objMarkerScreenSize.x;
            }
            else if(screenPos.x < objMarkerScreenSize.x)
            {
                screenPos.x = objMarkerScreenSize.x;
            }

            if (screenPos.y > Screen.height - objMarkerScreenSize.y)
            {
                screenPos.y = Screen.height - objMarkerScreenSize.y;
            }
            else if (screenPos.y < objMarkerScreenSize.y)
            {
                screenPos.y = objMarkerScreenSize.y;
            }
        }

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, null, out localPos);

        objMarker.localPosition = localPos;


        //MINIMAP

        //get player's forward vector in world space
        Vector2 playerForward = new Vector2(transform.forward.x, transform.forward.z).normalized;
        Vector2 worldForward = new Vector2(0f, 1f);
        //get angle between player's forward and world forward
        float angle = Angle(playerForward, worldForward);
        //rotate minimap by angle
        minimapHolder.localRotation = Quaternion.Euler(0f, 0f, angle);
        minimap.rectTransform.localPosition = (new Vector2(-transform.position.x / 150f, -transform.position.z / 150f) * 435f) * GameHandler.current.minimapScale;
    }

    float Angle(Vector2 v1, Vector2 v2)
    {
        float sign = Mathf.Sign(v1.x * v2.y - v1.y * v2.x);
        return Vector2.Angle(v1, v2) * sign;
    }


	public virtual void Hit(DAMAGE dmg)
	{
		CmdHit(dmg);

	}

	[Server]
	protected virtual void CmdHit(DAMAGE dmg)
	{
        //ServerLog.current.LogData("CmdHit");
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
                    if (dmg.damage > character.armour)
                    {
                        float excessDmg = dmg.damage - character.armour;
                        character.armour = 0;
                        character.health -= excessDmg;
                    }
                    else
                    {
                        character.armour -= dmg.damage;
                    }
                    

				}
				else
				{
					character.health -= dmg.damage;
				}
			}

            armourRegenDelayTimer = character.armourRegenDelay;
            

            if (character.health <= 0)
			{
				Die();
			}
			else
			{
                //play hit audio
                CmdPlaySoundDamage();

                RpcUpdateHealth(character.armour,character.health);
			}
		}
	}


    //=============================AUDIO NETWORKING============================\\
    [Command]
    private void CmdPlaySoundFootStep()
    {
        RpcPlaySoundFootStep();
    }

    [ClientRpc]
    private void RpcPlaySoundFootStep()
    {
        audioSource.PlayOneShot(walkClips[Random.Range(0, walkClips.Length)]);
    }

    [Command]
    private void CmdPlaySoundDamage()
    {
        RpcPlaySoundDamage();
    }

    [ClientRpc]
    private void RpcPlaySoundDamage()
    {
        audioSource.PlayOneShot(damageClips[Random.Range(0, damageClips.Length)]);
    }

    [Command]
    private void CmdPlaySoundPowerup()
    {
        RpcPlaySoundPowerup();
    }

    [ClientRpc]
    private void RpcPlaySoundPowerup()
    {
        audioSource.PlayOneShot(powerupClip);
    }


    //=========================================================================\\

    void RechargeArmour()
    {
        if (character.armour < character.maxArmour)
        {
            character.armour += character.armourRegenSpeed * Time.deltaTime;
            character.armour = Mathf.Clamp(character.armour, 0, character.maxArmour);

            RpcUpdateHealth(character.armour, character.health);
        }
    }

	[ClientRpc]
	protected virtual void RpcUpdateHealth(float a_armour, float a_health)
	{
		character.armour = a_armour;
		character.health = a_health;

		//team ui update

		//enemy ui update

		if(hasAuthority && tag != "Dummy")
		{
            //UpdateUI();
		}
	}


	public virtual void Hit(HitData hit)
	{
        //ServerLog.current.LogData("Hit Received");
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


		RpcDie(lastDamageRecievedFrom);
		//rpc kill - simple disable, if authority enable lobby cam
		//playerconnection.SetRespawn
	}

	[ClientRpc]
	private void RpcDie(int lastDamageRecieved)
	{
        if (PlayerConnection.current && PlayerConnection.current.playerObject && PlayerConnection.current.playerObject.GetComponent<CharacterHandler>())
        {
            PlayerConnection.current.playerObject.GetComponent<CharacterHandler>().UpdateKillFeed(lastDamageRecieved, GetComponent<Character>().id);
        }

        //this.gameObject.SetActive(false);
        if (hasAuthority)//if local player
		{
            

            //TODO: Play Die clip before respawning
            //characterAnim.SetTrigger("dead");
            //audioSource.PlayOneShot(dieClip);

            character.ResetAbilities();
            powerups.Clear();
            powerupDmgIcon.enabled = false;
            powerupJumpIcon.enabled = false;
            powerupSpeedIcon.enabled = false;

            transform.position = new Vector3(100, -100, 100);

			PlayerConnection.current.StartRespawnTimer();
			cam.gameObject.SetActive(false);
			GameManager.current.lobbyCam.SetActive(true);
			PlayerConnection.current.activeCamera = GameManager.current.lobbyCam;
		}
	}

    public IEnumerator FinishDeath()
    {
        //characterAnim.GetCurrentAnimatorStateInfo.

        yield return new WaitForSeconds(5);


        RpcDie(lastDamageRecievedFrom);
    }

    public void UpdateKillFeed(int killer, int killed)
    {
        //killFeedUI.SetPlayers("Player " + lastDamageRecievedFrom, "YOU");

        
        if (killed == character.id)
        {
            killFeedUI.ShowBar(KillBarType.Die);

            if (killer == -1)
            {
                killFeedUI.SetPlayers("Sharks", "YOU");
            }
            else
            {
                killFeedUI.SetPlayers("Player " + killer, "YOU");
            }

            killFeedUI.AddDeath();
        }
        else if(killer == character.id)
        {
            killFeedUI.ShowBar(KillBarType.Kill);

            killFeedUI.SetPlayers("YOU", "Player " + killed);
            killFeedUI.AddKill();
        }
        else
        {
            if (GameManager.current.GetPlayerObject(killer).GetComponent<CharacterHandler>().GetTeam() == 1)
            {
                killFeedUI.ShowBar(KillBarType.RedKill);
            }
            else if (GameManager.current.GetPlayerObject(killer).GetComponent<CharacterHandler>().GetTeam() == 2)
            {
                killFeedUI.ShowBar(KillBarType.BlueKill);
            }
            else
            {
                Debug.Log("Spectator is a murderer!");
            }

            killFeedUI.SetPlayers("Player " + killer, "Player " + killed);
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
		if (hasAuthority && tag != "Dummy")//if local player
		{
            //UpdateUI();
            Vector3 spawnPos;
            Quaternion spawnRot;
            GameManager.current.GetSpawnPos(GetTeam(), out spawnPos, out spawnRot);
            transform.position = spawnPos;
            transform.rotation = spawnRot;

            characterAnim.SetTrigger("respawn");

			cam.gameObject.SetActive(true);
			GameManager.current.lobbyCam.SetActive(false);
		}
	}

    [ClientRpc]
    public void RpcSetID(int iD)
    {
        GetComponent<Character>().id = iD;
    }
    
    [ClientRpc]
    //assigns the character a team - should inform more things
    public void RpcSetTeam(int a_teamNum)
    {
        teamNum = a_teamNum;

        if (teamRenderer.gameObject.activeSelf)
        {
            teamRenderer.GetPropertyBlock(propertyBlock);
        
            if (teamNum == 1)
            {
                propertyBlock.SetColor("_Colour", GameHandler.current.team1Col);
            }
            else if (teamNum == 2)
            {
                propertyBlock.SetColor("_Colour", GameHandler.current.team2Col);
            }
        
            propertyBlock.SetFloat("_TintOn", 1f);
            teamRenderer.SetPropertyBlock(propertyBlock);
        }

        ServerLog.current.LogData("Team " + teamNum.ToString());
    }

    public int GetTeam()
    {
        return teamNum;
    }

   



    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Powerup>())
        {
            if (isServer)
            {
                Powerup powerup = other.GetComponent<Powerup>();

                RpcGiveClientPowerUp((int)powerup.powerupType);
                                

                //instantiate explosion
                GameObject explosion = Instantiate(GameHandler.current.PowerupExplosion, other.transform.position, Quaternion.identity);
                NetworkServer.Spawn(explosion);
                explosion.GetComponent<Powerup>().CmdMakeExplosionsHappenPlz((int)powerup.powerupType);

                //explosion.GetComponent<Powerup>().colour = powerup.colour;

                powerup.spawn.SetSpawned(false);

                //destroy the object on all clients
                NetworkServer.Destroy(other.gameObject);
                //This can cause an error if 2 clients do this at the same time
            }
        }

        else if (other.GetComponent<ControlPoint>())
        {
            if (other.GetComponent<ControlPoint>().controlState == ControlPointState.active)
            {
                captureBarHolder.SetActive(true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<ControlPoint>())
        {
            if (other.GetComponent<ControlPoint>().controlState == ControlPointState.inactive)
            {
                captureBarHolder.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ControlPoint>())
        {
            captureBarHolder.SetActive(false);
        }
    }

    [ClientRpc]
    private void RpcGiveClientPowerUp(int powerupNum)
    {
        if (hasAuthority)
        {
            Powerup powerup = null;

            switch (powerupNum)
            {
                case 0:
                    powerup = GameHandler.current.powerupSpeedPrefab.GetComponent<Powerup>();
                    break;
                case 1:
                    powerup = GameHandler.current.powerupDamagePrefab.GetComponent<Powerup>();
                    break;

                case 2:
                    powerup = GameHandler.current.powerupJumpPrefab.GetComponent<Powerup>();
                    break;                    
            }


            //Powerup powerup = other.GetComponent<Powerup>();

            //check if powerup type is already in effect
            if (powerups.Exists(x => x.type.ToString() == powerup.powerupType.ToString()))
            {
                //if effect is already in use, just add to timer
                powerups.Find(x => x.type.ToString() == powerup.powerupType.ToString()).SetTimer(powerup.timer);
            }
            else
            {
                //if powerup is not in effect, start effect
                powerups.Add(new PowerUpData(powerup.name, powerup.powerupType, powerup.timer, powerup.multiplier));

                switch (powerup.powerupType)
                {
                    case PowerupType.Damage:
                        powerupDmgIcon.enabled = true;
                        break;

                    case PowerupType.Jump:
                        powerupJumpIcon.enabled = true;
                        break;

                    case PowerupType.Speed:
                        powerupSpeedIcon.enabled = true;
                        break;
                }
            }

            //audio
            CmdPlaySoundPowerup();
        }
    }

}