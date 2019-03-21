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

    public float respawnTime = 5.0f;

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

    //this is called to transition from lobby to gameplay
    [Command]
    public void CmdStartGame()
    {
        //check game can be started
        if (gameState == GameState.lobby)
        {
            //Choose teams
            //initialise players
            ServerLog.current.LogData("Starting game with " + players.Length.ToString() + " players");
            for (int i = 0; i < players.Length; i++)//PlayerConnection connection in connections)
            {
                PlayerConnection connection = players[i].connection;

                int team = players[i].team;
                Vector3 spawnPos;
                if(team == 1)
                {
                    spawnPos = team1SpawnPoint;
                }
                else if(team == 2)
                {
                    spawnPos = team2SpawnPoint;
                }
                else //team 0 
                {
                    spawnPos = lobbyCam.transform.position;
                }

                players[i].playerObject = connection.SpawnPlayer(team, spawnPos, characterPrefabs[players[i].characterChoice]);


            }
            //Start 
            if (CTPManager.current)
            {
                CTPManager.current.CmdStartGame();
            }

            //Disable the main camera  
            RpcDisableMainCamera();

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
    [Command]
    public void CmdAddNewconnection(GameObject connection)
    {
        ServerLog.current.LogData("Adding connection to server");
        //duplicate the array
        PlayerData[] tempPlayers = new PlayerData[players.Length + 1];

        for (int i = 0; i < players.Length; i++)
        {
            tempPlayers[i] = players[i];
        }
        //add another entry
        tempPlayers[tempPlayers.Length - 1].connection = connection.GetComponent<PlayerConnection>();
        tempPlayers[tempPlayers.Length - 1].characterChoice = 1; //1 is the default choice
        tempPlayers[tempPlayers.Length - 1].team = 1; //this should automatically assign a team 

        //overwrite current player data
        players = tempPlayers;

        ServerLog.current.LogData(players.Length.ToString() + " players connected");

        if(gameState == GameState.playing)
        {
            throw new NotImplementedException("Player connected mid game no handler implemented");
        }

        connection.GetComponent<PlayerConnection>().RpcAssignID(players.Length - 1);
        //return players.Length-1;
    }

    //[Command]
    //public void CmdAddNewPlayer(GameObject player)
    //{
    //    //duplicate the array
    //    PlayerData[] tempPlayers = new PlayerData[players.Length + 1];
    //
    //    for (int i = 0; i < players.Length; i++)
    //    {
    //        tempPlayers[i] = players[i];
    //    }
    //    //add new entry
    //    tempPlayers[tempPlayers.Length - 1].playerObject = player;
    //
    //    players = tempPlayers;
    //}

    [ClientRpc]
    private void RpcDisableMainCamera()
    {
        ServerLog.current.LogData("Disabling lobby cam");
        lobbyCam.SetActive(false);
        lobbyUI.SetActive(false);
    }

    [Command]
    public void CmdAddPlayerObject(GameObject player, int connectionID)
    {
        players[connectionID].playerObject = player;
    }

    [Server] //this can only be called on the server as only the server knows about other players - this should be fixed
    public GameObject GetPlayerObject(int playerID)
    {
        return players[playerID].playerObject;
    }

    public Vector3 GetSpawnPos(int teamNum)
    {
        Vector3 returnVector;

        if(teamNum == 1)
        {
            returnVector = team1SpawnPoint;
        }
        else if(teamNum == 2)
        {
            returnVector = team2SpawnPoint;
        }
        else
        {
            //spectator
            returnVector = Vector3.zero;
        }



        return returnVector;
    }

    public float GetSpawnTime()
    {
        //possibly some things to scale spawn time

        return respawnTime;
    }


//////Buttons
    
    [Command] 
    public void CmdChooseCharacter(int characterChoice, int connectionNum)
    {
        players[connectionNum].characterChoice = characterChoice;
        RpcChooseCharacter(characterChoice, connectionNum);
    }

    [ClientRpc] //tells other clients so they can visualise the choice
    private void RpcChooseCharacter(int characterChoice, int connectionNum)
    {
        ServerLog.current.LogData("Player " + connectionNum.ToString() + " chose character " + characterChoice.ToString());
        //update ui
    }

    [Command]
    public void CmdChooseTeam(int teamChoice, int connectionNum)
    {
        players[connectionNum].team  = teamChoice;
        RpcChooseTeam(teamChoice, connectionNum);
    }

    [ClientRpc] //tells other clients so they can visualise the choice
    private void RpcChooseTeam(int teamNum, int connectionNum)
    {
        ServerLog.current.LogData("Player " + connectionNum.ToString() + " chose team " + teamNum.ToString());
        //update ui
    }

    //private void OnPlayerDisconnected(NetworkPlayer player)
    //{
    //    ServerLog.current.LogData("A player has been disconnected");
    //}
}
