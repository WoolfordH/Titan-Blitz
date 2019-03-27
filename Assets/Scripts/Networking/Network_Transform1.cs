﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Network_Transform1 : NetworkBehaviour
{
    public Transform networkedTransform;

    public bool serverControl = false;

    public bool testcontrol = false;
    private bool testtest = false;

    // Use this for initialization
    void Start()
    {
        if(!networkedTransform)
        {
            networkedTransform = this.transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((hasAuthority && !serverControl) || (isServer && serverControl))
        {
            if (true)//moved enough or other checks
            {
                //ServerLog.current.LogData(networkedTransform.position.ToString());
                CmdUpdatePosition(networkedTransform.position);
                CmdUpdateRotation(networkedTransform.localRotation);
            }
        }


        //debugging thing, allows server control to be given through inspector
        if(testcontrol != testtest)
        {
            if(testcontrol)
            {
                CmdTakeServerControl();
                testtest = testcontrol;
            }
            else
            {
                CmdRemoveServerControl();
                testtest = testcontrol;
            }
        }

    }

    [Command]
    private void CmdUpdatePosition(Vector3 position)
    {
        RpcUpdatePosition(position);
    }

    [ClientRpc]
    private void RpcUpdatePosition(Vector3 position)
    {
        //if (!((!(hasAuthority && !serverControl)) || (!(isServer && serverControl))))
        if(!((hasAuthority && !serverControl) || (isServer && serverControl)))
        {
            //ServerLog.current.LogData(position.ToString());
            networkedTransform.position = position;
        }
    }

    [Command]
    private void CmdUpdateRotation(Quaternion rotation)
    {
        RpcUpdateRotation(rotation);
    }

    [ClientRpc]
    private void RpcUpdateRotation(Quaternion rotation)
    {
        if (!((hasAuthority && !serverControl) || (isServer && serverControl)))
        {
            //ServerLog.current.LogData("Receiving position");
            networkedTransform.localRotation = rotation;
        }
    }



    [Command]
    public void CmdTakeServerControl()
    {
        RpcTakeServerControl();
    }

    [ClientRpc]
    private void RpcTakeServerControl()
    {
        serverControl = true;
    }

    [Command]
    public void CmdRemoveServerControl()
    {
        RpcRemoveServerControl();
    }

    [ClientRpc]
    private void RpcRemoveServerControl()
    {
        serverControl = false;
    }
}