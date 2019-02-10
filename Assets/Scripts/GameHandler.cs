using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour {

	public static GameHandler current;

	public Character[,] characters;

	public float timeRemaining;

	public LayerMask groundLayer;
	public LayerMask playerLayer;

	public GameObject bloodSpurt;


	// Use this for initialization
	void Start () {
		if (GameHandler.current) 
		{
			if (GameHandler.current != this) 
			{
				Destroy (GameHandler.current.gameObject);
			}
		}

		GameHandler.current = this;
	}
	
	// Update is called once per frame
	void Update () {
		timeRemaining -= Time.deltaTime;
	}
}
