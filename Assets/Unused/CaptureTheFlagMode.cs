using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaptureTheFlagMode : NetworkBehaviour
{
    private bool team1Flag = true; //false if held by enemy team
    private bool team2Flag = true;
    //private int team1Captures = 0;
    //private int team2Captures = 0;

    public GameObject team1FlagObject;
    public GameObject team2FlagObject;

    public GameObject ctfUI;

    // Update is called once per frame
    void Update ()
    {
		
	}



    public void StartGame()
    {
        ResetGame();
        EnableUI();
        //Enable the flags - Not needed if this will be the only game mode
        team1FlagObject.SetActive(true);
        team2FlagObject.SetActive(true);
    }

    private void EnableUI()
    {
        
    }

    private void ResetGame()
    {
        //team1Captures = 0;
        //team2Captures = 0;
        if(team1Flag == false)
        {
            //Reset the object
            team1Flag = true;
        }
        if(team2Flag == false)
        {
            //Reset the object
            team2Flag = true;
        }

    }
}
