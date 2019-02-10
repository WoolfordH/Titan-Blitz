using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FlagStand : NetworkBehaviour
{
    public int teamNum;
    private bool hasFlag = true;
    public GameObject flag;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Flag Stand " + teamNum.ToString() + " Touched");
        if (hasFlag)
        {
            //if colliding with an enemy
            if ((other.tag == "Team1" && teamNum == 2) ||
                (other.tag == "Team2" && teamNum == 1))
            {
                //transfer flag to player 
                hasFlag = false;
            }
            //if colliding with an ally
            else if ((other.tag == "Team1" && teamNum == 1) ||
                     (other.tag == "Team2" && teamNum == 2))
            {
                if(false)//if other is holding a flag
                {
                    //Reset flag
                    //give 1 point
                }
            }
        }
        //else display something 
    }
}
