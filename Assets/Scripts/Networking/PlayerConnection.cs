using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    public GameObject playerPrefab;

	// Use this for initialization
	void Start ()
    {
        //this object is created on every client 
        //check if this belongs to this client
		if(isLocalPlayer)
        {
            Debug.Log("Added connection");
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

    [Command]
    public void CmdSpawnPlayer(int teamNum, Vector3 spawnPos)
    {
        //spawns to server
        GameObject player = Instantiate(playerPrefab,spawnPos,Quaternion.identity);

        
        //passes to all clients 
        NetworkServer.SpawnWithClientAuthority(player,connectionToClient);
        player.GetComponent<CharacterHandler>().RpcSetTeam(teamNum);
    }

    [Command]
    private void CmdAddToGameManager()
    {
        GameManager.current.AddNewconnection(this.gameObject);
    }
}
