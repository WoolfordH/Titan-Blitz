using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyMenu : MonoBehaviour 
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void ChooseCharacter(int characterChoice)
    {
        ServerLog.current.LogData("Choose char step 1");
        PlayerConnection.current.CmdChooseCharacter(characterChoice, PlayerConnection.connectionID);
    }

    public void ChooseTeam(int teamChoice)
    {
        ServerLog.current.LogData("Choose team step 1");
        PlayerConnection.current.CmdChooseTeam(teamChoice, PlayerConnection.connectionID);
    }

    public void StartGame()
    {
        ServerLog.current.LogData("Start game");
        PlayerConnection.current.CmdStartGame();
    }
}
