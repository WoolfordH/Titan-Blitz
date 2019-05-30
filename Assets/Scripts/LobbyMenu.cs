﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyMenu : NetworkBehaviour 
{
    struct Players
    {
        public int team;
        public int character;
        public string name;

        public void intiate()
        {
            team = 0;
            character = 0;
            name = "PlayerName";
        }
    }

    private Players[] players = new Players[8];
    private int self = -1;

    public LobbyMenuPlayerBar[] team1Holders;
    public LobbyMenuPlayerBar[] team2Holders;

	// Use this for initialization
	void Start ()
    {
        for(int i = 0; i<players.Length; i++)
            players[i].intiate();
        UpdateDisplay();
    }
	
	// Update is called once per frame
	void Update ()
    {
		//if(self == -1)       
        //    if(PlayerConnection.current)
        //        if (PlayerConnection.current.connectionID != -1)
        //            self = PlayerConnection.current.connectionID;        
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

    public void UpdateMenuData(int indice, int character, int team)
    {
        if(character != -1)
            players[indice].character = character;
        if(team != -1)
            players[indice].team = team;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int team1Count = 0;
        int team2Count = 0;

        foreach(LobbyMenuPlayerBar holder in team1Holders)
        {
            holder.DeActivateBar();
        }
        foreach (LobbyMenuPlayerBar holder in team2Holders)
        {
            holder.DeActivateBar();
        }

        for (int i = 0; i < players.Length; i++)
        {
            Players player = players[i];
            if (player.team == 1)
            {
                team1Holders[team1Count].ActivateBar(player.name, player.character, (i == self));
                team1Count++;
            }
            else if(player.team == 2)
            {
                team2Holders[team2Count].ActivateBar(player.name, player.character, (i == self));
                team2Count++;
            }
            else //team 0 - spectator
            {

            }
        }
    }

    public void RefreshMenuData()
    {

    }

    public void SetSelf(int ID)
    {
        self = ID;
    }
}
