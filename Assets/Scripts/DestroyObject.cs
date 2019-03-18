using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour {

	public void DestroyRoot()
	{
		Destroy (transform.root.gameObject);
	}

	public void DestroySelf()
	{
		Destroy (gameObject);
	}
}
