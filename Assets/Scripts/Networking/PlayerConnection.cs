﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    public static PlayerConnection current;

    //the number assigned to this client
    public int connectionID;
    public NetworkConnection clientIdentity;

    public GameObject playerObject;

    private float respawnTimer = 0;
    private bool waitingRespawn = false;

    public GameObject activeCamera; //lobby cam to start

	// Use this for initialization
	void Start ()
    {
        //this object is created on every client 
        //check if this belongs to this client
		if(hasAuthority)
        {
            ServerLog.current.LogData("Start own connection");
            clientIdentity = GetComponent<NetworkIdentity>().clientAuthorityOwner;
            current = this;
            CmdAddToGameManager();
        }
	}

    // Update is called once per frame
    void Update ()
    {
		if(isLocalPlayer)
        {
            if(waitingRespawn)//if the respawn timer is active
            {
                respawnTimer -= Time.deltaTime;
                if(respawnTimer<0)
                {
                    CmdRespawn();
                    waitingRespawn = false;
                }
            }
        }
	}


    //Command scripts - these are only run on the server

    //spawns this players game object 
    public GameObject SpawnPlayer(int teamNum, Vector3 spawnPos, GameObject playerPrefab)
    {
        ServerLog.current.LogData("Spawning Player");
        //spawns to server
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        //passes to all clients 
        NetworkServer.SpawnWithClientAuthority(player, GetComponent<NetworkIdentity>().clientAuthorityOwner);

        playerObject = player;

        if (player.GetComponent<CharacterHandler>())
        {
            player.GetComponent<CharacterHandler>().RpcSetTeam(teamNum);
        }

        return player;
    }

    public void StartRespawnTimer()
    {
        respawnTimer = GameManager.current.GetSpawnTime();
        waitingRespawn = true;
    }

    //Registers this connection to the game manager
    [Command]
    private void CmdAddToGameManager()
    {
        //connectionID = GameManager.current.CmdAddNewconnection(this.gameObject);
        GameManager.current.CmdAddNewconnection(this.gameObject);
    }

    

    [Command]
    private void CmdRespawn()
    {
        this.playerObject.GetComponent<CharacterHandler>().RpcRespawn();
    }


    [ClientRpc]
    public void RpcAssignID(int ID)
    {
        if (isLocalPlayer)
            connectionID = ID;
    }

    //private void OnDisconnectedFromServer(NetworkDisconnection info)
    //{
    //    ServerLog.current.LogData("Disconnected from server");
    //}

    //lobby buttons
    [Command]
    public void CmdChooseCharacter(int characterChoice, int connectionNum)
    {
        GameManager.current.CmdChooseCharacter(characterChoice, connectionNum);
    }

    [Command]
    public void CmdChooseTeam(int teamChoice, int connectionNum)
    {
        GameManager.current.CmdChooseTeam(teamChoice, connectionNum);
    }

    [Command]
    public void CmdStartGame()
    {
        GameManager.current.CmdStartGame();
    }
}
