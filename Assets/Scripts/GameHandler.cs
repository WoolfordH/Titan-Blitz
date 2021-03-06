﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour {

	public static GameHandler current;

	public Character[,] characters;

	public float timeRemaining;

	public LayerMask groundLayer;
	public LayerMask playerLayer;
    public LayerMask projectileLayer;
    public LayerMask shootableLayer;

	public GameObject bloodSpurt;
    public GameObject projectile;
	public GameObject damageIndicator;
    public GameObject scorchPrefab;
    public GameObject PowerupExplosion;
    public GameObject powerupSpeedPrefab, powerupDamagePrefab, powerupJumpPrefab;
    public Color[] powerupCols;

    public float minimapScale = 1f;


    public Camera playerCam;

    public Color team1Col, team2Col;


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
