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
    private float captureRate;
    private float additionalPlayerMultiplier;
    //gameobject or something for the capture animation stuff

    public Collider pointCollider;

    private GameObject[] playersOnPoint = new GameObject[6];
    private int playersOnPointCount = 0;
    private int teamOnPointDifference = 0; //positive for team 1, negative for team 2

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
        pointCollider.enabled = true;
        capturePercent = 0;
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
            if (FindPlayerInArray(character.gameObject) != -1)
            {
                if (true)//team 1
                {
                    playersOnPoint[playersOnPointCount] = character.gameObject;
                    playersOnPointCount++;
                    teamOnPointDifference++;
                    //add to colliding players
                    //count++
                    //difference++/--
                }
                else
                {

                }
            }
        }
    }

    [Server]
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<CharacterHandler>()) //if it has a character handle - is a player
        {
            if (true)//team 1
            {

            }
            else
            {

            }
        }
    }
}
