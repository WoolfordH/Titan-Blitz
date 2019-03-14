using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

	//public Camera cam;

    private void Start()
    {
		//HARRY: ENABLE FOR NETWORKING
        //cam = PlayerConnection.localPlayer.GetComponent<CharacterHandler>().cam;
    }

    // Update is called once per frame
    void LateUpdate () 
	{
        GameObject cam = PlayerConnection.current.activeCamera;
        if (cam)
        {

            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        }
	}
		
}
