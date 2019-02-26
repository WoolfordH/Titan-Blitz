using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    //the number assigned to this client
    public static int connectionID;
    public static NetworkConnection clientIdentity;
    public static PlayerConnection current;
    //the prefab for the player object

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

        }
	}


    //Command scripts - these are only run on the server

    //spawns this players game object 
    public void SpawnPlayer(int teamNum, Vector3 spawnPos, GameObject playerPrefab)
    {
        ServerLog.current.LogData("Spawning Player");
        //spawns to server
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        //passes to all clients 
        NetworkServer.SpawnWithClientAuthority(player, GetComponent<NetworkIdentity>().clientAuthorityOwner);
        //NetworkServer.Spawn(player);
        if (player.GetComponent<CharacterHandler>())
        {
            player.GetComponent<CharacterHandler>().RpcSetTeam(teamNum);
        }
    }

    //Registers this connection to the game manager
    [Command]
    private void CmdAddToGameManager()
    {
        connectionID = GameManager.current.CmdAddNewconnection(this.gameObject);
    }


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

    private void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        ServerLog.current.LogData("Disconnected from server");
    }
}
