using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    //the identity of this client
    public static int connectionID;
    //the prefab for the player object

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
    public void CmdSpawnPlayer(int teamNum, Vector3 spawnPos, GameObject playerPrefab)
    {
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

    [Command]
    private void CmdAddToGameManager()
    {
        connectionID = GameManager.current.AddNewconnection(this.gameObject);
    }
}
