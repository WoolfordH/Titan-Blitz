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

    public GameObject lobbyUI;

    public GameObject[] characterPrefabs;

    private PlayerConnection[] connections = new PlayerConnection[0];
    private int[] characterChoices = new int[0];
    private GameObject[] players = new GameObject[0];
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
                
            }
        }
	}

    public void StartGame()
    {
        CmdStartGame();
    }

    //only the server will call this
    //this is called to transition from lobby to gameplay
    [Command]
    private void CmdStartGame()
    {
        //check game can be started
        if (gameState == GameState.lobby)
        {
            //Choose teams
            //initialise players
            Log.current.LogData(connections.Length.ToString());
            for (int i = 0; i < connections.Length; i++)//PlayerConnection connection in connections)
            {
                PlayerConnection connection = connections[i];
                //put on teams - should be added to a player struct so its settable 
                if ((i % 2) == 0)
                {
                    //team 1
                    Vector3 spawnPos = team1SpawnPoint;
                    connection.CmdSpawnPlayer(1, spawnPos, characterPrefabs[characterChoices[i]]); //should pass a spawn position - split to spawn and initialise 
                }
                else
                {
                    //team 2
                    Vector3 spawnPos = team2SpawnPoint;
                    connection.CmdSpawnPlayer(2, spawnPos, characterPrefabs[characterChoices[i]]); //should pass a spawn position - split to spawn and initialise 
                }

                //Disable the main camera  
                RpcDisableMainCamera();
            }
            //Start 

            gameState = GameState.playing;
        }
    }

    //private void CreateServer()
    //{
    //   ////network 
    //   //Network.ip
    //   // NetworkServer.Listen(,);
    //   // NetworkClient.
    //}

    public int AddNewconnection(GameObject connection)
    {
        //increase the size of the arrays and in the new value 
        PlayerConnection[] tempConnects = new PlayerConnection[connections.Length + 1];
        int[] tempChoices = new int[characterChoices.Length + 1];

        for (int i = 0; i < connections.Length; i++)
        {
            tempConnects[i] = connections[i];
            tempChoices[i] = characterChoices[i];
        }
        tempConnects[tempConnects.Length - 1] = connection.GetComponent<PlayerConnection>();
        tempChoices[tempChoices.Length - 1] = 1; //1 is the default choice

        connections = tempConnects;
        characterChoices = tempChoices;

        Debug.Log("Current connections " + connections.Length);

        return connections.Length-1;
    }

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

    [ClientRpc]
    private void RpcDisableMainCamera()
    {
        lobbyCam.SetActive(false);
        lobbyUI.SetActive(false);
    }


//////Buttons
    
    public void ChooseCharacter(int characterChoice)
    {
        CmdChooseCharacter(characterChoice, PlayerConnection.connectionID);
    }
    
    [Command]   //tells server the choice
    private void CmdChooseCharacter(int characterChoice, int connectionNum)
    {
        characterChoices[connectionNum] = characterChoice;
        RpcChooseCharacter(characterChoice, connectionNum);
    }
    
    [ClientRpc] //tells other clients so they can visualise the choice
    private void RpcChooseCharacter(int characterChoice, int connectionNum)
    {
        Log.current.LogData("Player " + connectionNum.ToString() + " chose character " + characterChoice.ToString());
        //update ui
    }

}
