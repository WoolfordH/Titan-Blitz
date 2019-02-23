using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//public struct Controls
//{
//	public KeyCode forward;
//	public KeyCode back;
//	public KeyCode left;
//	public KeyCode right;
//
//	public KeyCode jump;
//	public KeyCode sprint;
//
//	public KeyCode primaryAtk;
//	public KeyCode secondaryAtk;
//	public KeyCode ult;
//}

public class CharacterController : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public GameObject tip;

	Controls controls;
	public float speed;
	public float sprintMultiplier;

	public float lookSpeed;
	float initLookSpeed;

	public Camera cam;

	//temporary
	bool paused = false;

	// Use this for initialization
	void Start ()
    {

        //if not us ignore
        if (hasAuthority)
        {

            Debug.Log("Start My Player");
            //Debug.Log(this.gameObject.tag);
            //Log.current.LogData(this.gameObject.tag);

            //enable camera
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

            Cursor.lockState = CursorLockMode.Locked;

            initLookSpeed = lookSpeed;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (hasAuthority)
        {
            //the server does not have authority during start so make a post start call to start
            if(isServer)
                if(cam.gameObject.activeInHierarchy == false)//if start has been called the camera will be on - could cause bugs later 
                {
                    Start();
                }


            //movement
            ProcessMovement();

            //shoot
            ProcessShoot();

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
		Vector3 movement = Vector3.zero;
		float speedmod = 1;

		//forward
		if (Input.GetKey (controls.forward))
		{
			movement += new Vector3 (0f,0f,1f);
		}

		//back
		if (Input.GetKey (controls.back))
		{
			movement += new Vector3 (0f,0f,-1f);
		}

		//left
		if (Input.GetKey (controls.left))
		{
			movement += new Vector3 (-1f,0f,0f);
		}

		//right
		if (Input.GetKey (controls.right))
		{
			movement += new Vector3 (1f,0f,0f);
		}

        movement.Normalize();

        if (Input.GetKey (controls.sprint))
		{
			speedmod *= sprintMultiplier;
		}


        //apply movement locally
        transform.position += (transform.TransformDirection(movement) * speed * speedmod * Time.deltaTime);


		//mouse look

		float xRot, yRot;

		xRot = Input.GetAxis ("Mouse X");
		yRot = Input.GetAxis ("Mouse Y");

		transform.Rotate (Vector3.up, xRot * lookSpeed);
		//if looking up or down goes over 60 deg in either direction, clamp
		cam.transform.Rotate (Vector3.right, (-yRot) * lookSpeed);
	}

    private void ProcessShoot()
    {
        //Debug.Log("process Shoot");
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CmdShoot();
        }
    }


    //commands
    [Command]
    private void CmdShoot()
    {

        Debug.Log("shoot");
        GameObject bullet = Instantiate(bulletPrefab, tip.transform.position, tip.transform.rotation);

        NetworkServer.Spawn(bullet);

        RpcShoot();
    }

    //RPC
    [ClientRpc]
    private void RpcShoot()
    {
        //Play animation
    }

    [ClientRpc]
    public void RpcSetTeam(int teamNum)
    {
        if (teamNum == 1)
            tag = "Team1";
        else
            tag = "Team2";

        ServerLog.current.LogData("Team " + teamNum.ToString());
    }

}
