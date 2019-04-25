using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

enum CTPManagerState
{
    inactive,
    team1Advantage,
    neutral,
    team2Advantage,
    gameEnd
}

public class CTPManager : NetworkBehaviour
{
    public static CTPManager current;

    public ControlPoint centrePoint;
    public ControlPoint team1Point;
    public ControlPoint team2Point;

    public Transform currentObj;

    private CTPManagerState state = CTPManagerState.inactive;


    private void Awake()
    {
        current = this;
    }

    // Use this for initialization
    void Start ()
    {
	    if(isServer)
        {
            //if(controlPoints.Length != 3)
            //{
            //    throw new System.NotImplementedException(controlPoints.Length.ToString() + " control points no handler for this number");
            //}
        }

        //for(int i=0;i<controlPoints.Length;i++)
        //{
        //    controlPoints[i].Initialise(i);
        //}

        centrePoint.Initialise(0);
        team1Point.Initialise(1);
        team2Point.Initialise(2);
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    [Command]
    public void CmdStartGame()
    {
        if (state == CTPManagerState.inactive)
        {
            centrePoint.Activate();
            team1Point.Deactivate();
            team1Point.Deactivate();
            state = CTPManagerState.neutral;
            RpcSetObjective(0);
        }
        else
        {
            throw new System.Exception("Activate called on active CTPManager");
        }
    }

    [ClientRpc]
    public void RpcSetObjective(int num)
    {
        if (num == 0)
            currentObj = centrePoint.transform;
        else if (num == 1)
            currentObj = team1Point.transform;
        else if (num == 2)
            currentObj = team2Point.transform;
    }

    public void CapturePoint(int team, int pointIdentity)
    {
        if(state == CTPManagerState.neutral)
        {
            if(pointIdentity == 0)//error check - only point 0 should be capture-able when neutral
            {
                if(team == 1)
                {
                    //centre point only cap-able by team 2
                    team2Point.Activate();
                    centrePoint.Deactivate();
                    state = CTPManagerState.team1Advantage;
                    RpcSetObjective(2);
                }
                else //team 2
                {
                    centrePoint.Deactivate();
                    team1Point.Activate();
                    state = CTPManagerState.team2Advantage;
                    RpcSetObjective(1);
                }
            }
        }
        else if(state == CTPManagerState.team1Advantage)
        {
            if (pointIdentity == 2) // team 2 point
            {
                if (team == 1)
                {
                    //team 1 victory
                    ServerLog.current.LogData("Team 1 victory");
                    GameManager.current.CmdGameEnd(1);
                    state = CTPManagerState.gameEnd;
                }
                else //team 2
                {
                    //team2 push back
                    team2Point.Deactivate();
                    centrePoint.Activate();
                    state = CTPManagerState.neutral;
                    RpcSetObjective(0);
                }
            }
            else
            {
                throw new Exception("an inactive capture point was just captured");
            }
        }
        else if(state == CTPManagerState.team2Advantage)
        {
            if (pointIdentity == 1) // team 1 point
            {
                if (team == 2)
                {
                    //team 2 victory
                    ServerLog.current.LogData("Team 2 victory");
                    GameManager.current.CmdGameEnd(2);
                    state = CTPManagerState.gameEnd;
                }
                else //team 1
                {
                    //team2 push back
                    team1Point.Deactivate();
                    centrePoint.Activate();
                    state = CTPManagerState.neutral;
                    RpcSetObjective(0);
                }
            }
            else
            {
                throw new Exception("an inactive capture point was just captured");
            }
        }
        else
        {
            //throw new Exception("unexpected call in CTPManager CapturePoint()");
        }
    }
}
