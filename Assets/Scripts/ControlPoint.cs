using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

enum ControlPointState
{
    inactive,
    active

    //Team1Owned,
    //Team2Owned,
    //Team1Capturing,
    //Team2Capturing,
    //Neutral
}

public class ControlPoint : NetworkBehaviour
{
    private int pointIdentity;
    private ControlPointState controlState = ControlPointState.inactive;
    private float capturePercent = 0; //positive for team 1, negative for team 2
    public float captureRate;
    public float additionalPlayerMultiplier;
    //gameobject or something for the capture animation stuff

    public Collider pointCollider;

    private GameObject[] playersOnPoint = new GameObject[6];
    private int playersOnPointCount = 0;
    private int teamOnPointDifference = 0; //positive for team 1, negative for team 2

    //visualisation 
    public Material mat;

    // Use this for initialization
    void Start ()
    {
        if(pointCollider)
        {
            pointCollider.enabled = false;
        }
        else
        {
            throw new Exception("No collider assigned to control point");
        }
        //Material mat = this.GetComponent<Material>();
    }

    public void Initialise(int pointID)
    {
        pointIdentity = pointID;
    }

    // Update is called once per frame
    void Update ()
    {
        if (controlState == ControlPointState.active)
        {
            if (isServer)
            {


                CaptureChange();

                if (capturePercent > 100)
                {
                    //team 1 capture
                    CTPManager.current.CapturePoint(1, pointIdentity);
                    
                }
                else if (capturePercent < -100)
                {
                    //team 2 capture
                }
            }
        }
        Visualise();
    }

    private void Visualise()
    {
        if (controlState == ControlPointState.active)
        {
            if (PlayerConnection.current.playerObject.GetComponent<CharacterHandler>().GetTeam() == 1)
            {
                if (capturePercent > 0)
                {
                    mat.color = new Color(0, Mathf.Abs(capturePercent) * 0.01f, 0, 0.33f); //green
                }
                else
                {
                    mat.color = new Color(Mathf.Abs(capturePercent) * 0.01f, 0, 0, 0.33f); //red
                }
            }
            else
            {
                if (capturePercent < 0)
                {
                    mat.color = new Color(0, Mathf.Abs(capturePercent) * 0.01f, 0, 0.33f); //green
                }
                else
                {
                    mat.color = new Color(Mathf.Abs(capturePercent) * 0.01f, 0, 0, 0.33f); //red
                }
            }
        }
        else
        {
            mat.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }

    private void CaptureChange()
    {
        float newPercent = capturePercent;

        if(teamOnPointDifference != 0)
        {
            //increase the percentage by the capture rate * the players on the point 
            //this increases for team 1 and decreases for team 2
            float increase = teamOnPointDifference * captureRate * Time.deltaTime;
            //include scaling so 2 players can not just be double the point gain


            newPercent +=  increase;


        }


        if(newPercent != capturePercent)
        {
            RpcUpdateCapturePercent(newPercent);
        }
    }


    [ClientRpc]
    private void RpcUpdateCapturePercent(float percent)
    {
        capturePercent = percent;
        //update ui
    }



    public void Activate()
    {
        //enable collider
        capturePercent = 0;
        playersOnPointCount = 0;
        playersOnPoint = new GameObject[playersOnPoint.Length];
        pointCollider.enabled = true;
        controlState = ControlPointState.active;
    }

    public void Deactivate()
    {
        pointCollider.enabled = false;
        controlState = ControlPointState.inactive;
    }


    //if the player is in the array return its index else return -1
    private int FindPlayerInArray(GameObject player)
    {
        for(int i = 0; i < playersOnPointCount; i++)
        {
            if(playersOnPoint[i] = player)
            {
                return i;
            }
        }

        return -1;
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        CharacterHandler character = other.GetComponentInParent<CharacterHandler>();
        if (character) //if it has a character handle - is a player
        {
            if (FindPlayerInArray(character.gameObject) == -1) //if the player is not already in the list
            {
                if (character.GetTeam() == 1)//team 1
                {
                    playersOnPoint[playersOnPointCount] = character.gameObject;
                    playersOnPointCount++;
                    teamOnPointDifference++;
                }
                else if(character.GetTeam() == 2)
                {
                    playersOnPoint[playersOnPointCount] = character.gameObject;
                    playersOnPointCount++;
                    teamOnPointDifference--;
                }
                else
                {
                    //team 0
                }
            }
        }
    }

    [Server]
    private void OnTriggerExit(Collider other)
    {
        CharacterHandler character = other.GetComponentInParent<CharacterHandler>();
        if (character) //if it has a character handle - is a player
        {
            int index = FindPlayerInArray(character.gameObject);
            if (index != -1)//player was in array
            {
                //remove from array
                for(int i = index;i<playersOnPointCount-1;i++)
                {
                    playersOnPoint[i] = playersOnPoint[i + 1];
                }
                if(character.GetTeam() == 1)//is team 1
                {
                    teamOnPointDifference--;
                }
                else if(character.GetTeam() == 2)
                {
                    teamOnPointDifference++;
                }

                playersOnPointCount--;


            }

        }
    }
}
