using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    public enum GameState
    {
        lobby,
        playing
    }

    public static GameManager current;

    //public NetworkManager network;
    //public NetworkServer server;

    private PlayerConnection[] connections = new PlayerConnection[0];
    public GameObject[] players = new GameObject[0];
    public GameObject lobbyCam;

    public Vector3 team1SpawnPoint;
    public Vector3 team2SpawnPoint;

    private GameState gameState = GameState.lobby;

    private void Awake()
    {
        current = this;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Server managment code
        if (isServer)
        {
            //wait in lobby until 
            if (gameState == GameState.lobby)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    StartGame();
                }
            }
        }
	}

    //only the server will call this
    //this is called to transition from lobby to gameplay
    private void StartGame()
    {
        //Choose teams
        //initialise players
        Log.current.LogData(connections.Length.ToString());
        for(int i =0;i<connections.Length;i++)//PlayerConnection connection in connections)
        {
            PlayerConnection connection = connections[i];

            if((i % 2) == 0)
            {
                //team 1
                Vector3 spawnPos = team1SpawnPoint;
                connection.CmdSpawnPlayer(1,spawnPos); //should pass a spawn position - split to spawn and initialise 
            }
            else
            {
                //team 2
                Vector3 spawnPos = team2SpawnPoint;
                connection.CmdSpawnPlayer(2,spawnPos); //should pass a spawn position - split to spawn and initialise 
            }
            
            //Disable the main camera  
            RpcDisableMainCamera();
        }
        //Start 

        gameState = GameState.playing;
    }

    private void CreateServer()
    {
       ////network 
       //Network.ip
       // NetworkServer.Listen(,);
       // NetworkClient.
    }

    public void AddNewconnection(GameObject connection)
    {
        PlayerConnection[] temp = new PlayerConnection[connections.Length + 1];

        for (int i = 0; i < connections.Length; i++)
        {
            temp[i] = connections[i];
        }
        temp[temp.Length - 1] = connection.GetComponent<PlayerConnection>();

        connections = temp;

        Debug.Log("Current connections " + connections.Length);
    }

    /////////////commands\\\\\\\\\\\\\
    [Command]
    public void CmdAddNewPlayer(GameObject player)
    {
        GameObject[] temp = new GameObject[players.Length + 1];

        for(int i = 0; i<players.Length;i++)
        {
            temp[i] = players[i];
        }
        temp[temp.Length - 1] = player;

        players = temp;
    }

     
    //////RPC\\\\\\\\
    [ClientRpc]
    private void RpcDisableMainCamera()
    {
        lobbyCam.SetActive(false);
    }
}
