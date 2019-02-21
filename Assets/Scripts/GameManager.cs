using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

struct PlayerData
{
    public int characterChoice;
    public int team;//0 = no team; 1 = team 1; 2 = team 2
    public PlayerConnection connection;
    public GameObject playerObject;
}

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

    private PlayerData[] players = new PlayerData[0];
   
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
            Log.current.LogData(players.Length.ToString());
            for (int i = 0; i < players.Length; i++)//PlayerConnection connection in connections)
            {
                PlayerConnection connection = players[i].connection;
                //put on teams - should be added to a player struct so its settable 
                //if ((i % 2) == 0)
                //{
                //    //team 1
                //    Vector3 spawnPos = team1SpawnPoint;
                //    connection.CmdSpawnPlayer(1, spawnPos, characterPrefabs[players[i].characterChoice]); 
                //}
                //else
                //{
                //    //team 2
                //    Vector3 spawnPos = team2SpawnPoint;
                //    connection.CmdSpawnPlayer(2, spawnPos, characterPrefabs[characterChoices[i]]);  
                //}
                int team = players[i].team;

                connection.CmdSpawnPlayer(team, (team == 1)?team1SpawnPoint:team2SpawnPoint, characterPrefabs[players[i].characterChoice]);

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
        //duplicate the array
        PlayerData[] tempPlayers = new PlayerData[players.Length + 1];

        for (int i = 0; i < players.Length; i++)
        {
            tempPlayers[i] = players[i];
        }
        //add another entry
        tempPlayers[tempPlayers.Length - 1].connection = connection.GetComponent<PlayerConnection>();
        tempPlayers[tempPlayers.Length - 1].characterChoice = 1; //1 is the default choice
        tempPlayers[tempPlayers.Length - 1].team = 0; //this should automatically assign a team 

        players = tempPlayers;

        Debug.Log("Current connections " + players.Length);

        return players.Length-1;
    }

    [Command]
    public void CmdAddNewPlayer(GameObject player)
    {
        //duplicate the array
        PlayerData[] tempPlayers = new PlayerData[players.Length + 1];

        for (int i = 0; i < players.Length; i++)
        {
            tempPlayers[i] = players[i];
        }
        //add new entry
        tempPlayers[tempPlayers.Length - 1].playerObject = player;

        players = tempPlayers;
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
        players[connectionNum].characterChoice = characterChoice;
        RpcChooseCharacter(characterChoice, connectionNum);
    }
    
    [ClientRpc] //tells other clients so they can visualise the choice
    private void RpcChooseCharacter(int characterChoice, int connectionNum)
    {
        Log.current.LogData("Player " + connectionNum.ToString() + " chose character " + characterChoice.ToString());
        //update ui
    }



    public void ChooseTeam(int teamNum)
    {
        CmdChooseTeam(teamNum, PlayerConnection.connectionID);
    }

    [Command] //tells server the choice
    private void CmdChooseTeam(int teamNum, int connectionNum)
    {
        players[connectionNum].team = teamNum;
        RpcChooseTeam(teamNum, connectionNum);
    }

    [ClientRpc] //tells other clients so they can visualise the choice
    private void RpcChooseTeam(int teamNum, int connectionNum)
    {
        Log.current.LogData("Player " + connectionNum.ToString() + " chose team " + teamNum.ToString());
        //update ui
    }
}
