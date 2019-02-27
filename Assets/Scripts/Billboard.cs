using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

	public Camera cam;
	
	// Update is called once per frame
	void LateUpdate () 
	{
		transform.LookAt (transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);	
	}
		
}
