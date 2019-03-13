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
        PlayerConnection.current.CmdChooseCharacter(characterChoice, PlayerConnection.current.connectionID);
    }

    public void ChooseTeam(int teamChoice)
    {
        PlayerConnection.current.CmdChooseTeam(teamChoice, PlayerConnection.current.connectionID);
    }

    public void StartGame()
    {
        ServerLog.current.LogData("Start game button");
        PlayerConnection.current.CmdStartGame();
    }
}
