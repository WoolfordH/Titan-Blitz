using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

    AudioSource audioSource;
    public AudioClip gameWinClip;
    public AudioClip gameLoseClip;

    public GameObject[] characterPrefabs;

    private PlayerData[] players = new PlayerData[0];

    public GameObject lobbyCam;

    public LobbyMenu lobbyMenu;

    public Transform team1SpawnPoint;
    public Transform team2SpawnPoint;

    public float respawnTime = 5.0f;

    public GameObject pauseMenu;
    public GameObject endScreen;
    public Text winLoseText;

    public GameObject respawnCam;
    public Text respawnText;

    public float gameTimer = 9999999;
    public Text gameLengthChoice;


    private GameState gameState = GameState.lobby;

    private void Awake()
    {
        current = this;
        audioSource = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        //Server managment code
        if (isServer)
        {
            //wait in lobby until 
            if (gameState == GameState.lobby)
            {

            }
        }
        gameTimer -= Time.deltaTime;
        if (gameState == GameState.playing)
        {
            
            if(isServer && gameTimer<0)
            {
                CTPManager.current.RpcTimeout();
                gameTimer = 9999999;
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
            SendClientsPlayerData();
            for (int i = 0; i < players.Length; i++)//PlayerConnection connection in connections)
            {
                PlayerConnection connection = players[i].connection;

                int team = players[i].team;
                Vector3 spawnPos;
                Quaternion spawnRot;
                GetSpawnPos(team, out spawnPos, out spawnRot);

                players[i].playerObject = connection.SpawnPlayer(team, spawnPos, spawnRot, characterPrefabs[players[i].characterChoice]);


            }
            //Start 
            if (CTPManager.current)
            {
                CTPManager.current.CmdStartGame();
            }

            //Disable the main camera  
            RpcDisableMainCamera();

            string timeString = gameLengthChoice.text;

            timeString = timeString.Remove(timeString.IndexOf(' '));

            float time = float.Parse(timeString);
            RpcStartTimer(time*60);


            gameState = GameState.playing;
        }
    }

    [ClientRpc]
    private void RpcStartTimer(float gameLength)
    {
        gameTimer = gameLength;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(false);
        }

        PlayerConnection.current.playerObject.GetComponent<CharacterHandler>().Pause(pause);
    }

    private void SendClientsPlayerData()
    {
        //initialise player array
        RpcInitializeClientPlayerData(players.Length);
        //for each player send player data
        for (int i = 0; i < players.Length; i++)
        {
            RpcPassClientsPlayerData(i, players[i].characterChoice, players[i].team);
        }
    }

    [ClientRpc]
    private void RpcInitializeClientPlayerData(int arraySize)
    {
        if(!isServer)
        {
            players = new PlayerData[arraySize];
        }
    }

    [ClientRpc]
    private void RpcPassClientsPlayerData(int indice, int charChoice, int teamNum)
    {
        players[indice].characterChoice = charChoice;
        players[indice].team = teamNum;

        //visualise
        lobbyMenu.UpdateMenuData(indice, charChoice, teamNum);
    }
        


    //public int characterChoice;
    //public int team;//0 = no team; 1 = team 1; 2 = team 2
    //public PlayerConnection connection;
    //public GameObject playerObject;

    [Command]
    public void CmdGameEnd(int endCondition)
    {
        RpcGameEnd(endCondition);
    }

    [ClientRpc]
    public void RpcGameEnd(int endCondition)
    {
        ServerLog.current.LogData("Victory team " + endCondition.ToString());
        switch (endCondition)
        {
            case 0:
                //time expired
                winLoseText.text = "DRAW";
                break;

            case 1:
                //team 1 win
                if(players[PlayerConnection.current.connectionID].team == 1)
                {
                    //you won
                    winLoseText.text = "VICTORY";
                    audioSource.PlayOneShot(gameWinClip);
                }
                else if(players[PlayerConnection.current.connectionID].team == 2)
                {
                    //you lost
                    winLoseText.text = "DEFEAT";
                    audioSource.PlayOneShot(gameLoseClip);
                }

                break;

            case 2:
                //team 2 win

                if (players[PlayerConnection.current.connectionID].team == 1)
                {
                    //you lost
                    winLoseText.text = "DEFEAT";
                    audioSource.PlayOneShot(gameLoseClip);
                }
                else if (players[PlayerConnection.current.connectionID].team == 2)
                {
                    //you won
                    winLoseText.text = "VICTORY";
                    audioSource.PlayOneShot(gameWinClip);
                }

                break;
        }

        endScreen.SetActive(true);
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


        SendClientsPlayerData();
        //return players.Length-1;
    }

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

    [Server] //this can only be called on the server as only the server knows about other players - this should be changed
    public GameObject GetPlayerObject(int playerID)
    {
        return players[playerID].playerObject;
    }

    
    public PlayerConnection GetPlayerConnection(int playerID)
    {
        return players[playerID].connection;
    }

    public void GetSpawnPos(int teamNum , out Vector3 spawnPos, out Quaternion spawnRot)
    {
        float minX, maxX, minZ, maxZ;

        if (teamNum == 1)
        {
            minX = team1SpawnPoint.position.x - team1SpawnPoint.GetComponent<Collider>().bounds.extents.x;
            maxX = team1SpawnPoint.position.x + team1SpawnPoint.GetComponent<Collider>().bounds.extents.x;
            minZ = team1SpawnPoint.position.z - team1SpawnPoint.GetComponent<Collider>().bounds.extents.x;
            maxZ = team1SpawnPoint.position.z + team1SpawnPoint.GetComponent<Collider>().bounds.extents.z;

            spawnPos = new Vector3(UnityEngine.Random.Range(minX, maxX), team1SpawnPoint.position.y, UnityEngine.Random.Range(minZ, maxZ));
            spawnRot = team1SpawnPoint.transform.rotation;
        }
        else if (teamNum == 2)
        {
            minX = team2SpawnPoint.position.x - team2SpawnPoint.GetComponent<Collider>().bounds.extents.x;
            maxX = team2SpawnPoint.position.x + team2SpawnPoint.GetComponent<Collider>().bounds.extents.x;
            minZ = team2SpawnPoint.position.z - team2SpawnPoint.GetComponent<Collider>().bounds.extents.x;
            maxZ = team2SpawnPoint.position.z + team2SpawnPoint.GetComponent<Collider>().bounds.extents.z;

            spawnPos = new Vector3(UnityEngine.Random.Range(minX, maxX), team2SpawnPoint.position.y, UnityEngine.Random.Range(minZ, maxZ));
            spawnRot = team2SpawnPoint.transform.rotation;
        }
        else //team 0 
        {
            spawnPos = lobbyCam.transform.position;
            spawnRot = lobbyCam.transform.rotation;
        }
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
        lobbyMenu.UpdateMenuData(connectionNum, characterChoice, -1);
    }

    [Command]
    public void CmdChooseTeam(int teamChoice, int connectionNum)
    {       
        RpcChooseTeam(teamChoice, connectionNum);
    }

    [ClientRpc] //tells other clients so they can visualise the choice
    private void RpcChooseTeam(int teamNum, int connectionNum)
    {
        ServerLog.current.LogData("Player " + connectionNum.ToString() + " chose team " + teamNum.ToString());
        players[connectionNum].team = teamNum;
        //update ui
        lobbyMenu.UpdateMenuData(connectionNum, -1, teamNum);
    }

    public void ResetGame()
    {
        // reset player connection
        PlayerConnection.current.ResetGame();
        lobbyCam.SetActive(true);
        lobbyUI.SetActive(true);
        PlayerConnection.current.activeCamera = lobbyCam;
        // reset objectives
        endScreen.SetActive(false);

        gameState = GameState.lobby;
        // enable lobby screen
    }


    //private void OnPlayerDisconnected(NetworkPlayer player)
    //{
    //    ServerLog.current.LogData("A player has been disconnected");
    //}


    public void Quit()
    {
        Application.Quit();
    }
}
