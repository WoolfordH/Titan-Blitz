using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Network_Transform : NetworkBehaviour
{
    public Transform networkedTransform;
    public Rigidbody rb;

    private bool serverControl = false;

    public bool toggleServerControl = false;
    private bool toggleCheck = false;

    private Vector3 savedPos;
    private Quaternion savedRotation;

    // Use this for initialization
    void Awake()
    {
        if (!networkedTransform)
            networkedTransform = this.transform;
        rb = networkedTransform.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((hasAuthority && !serverControl) || (isServer && serverControl))
        {
            if (Vector3.Distance(savedPos, networkedTransform.position) > 0.01)//moved enough or other checks
            {
                savedPos = networkedTransform.position;

                CmdUpdatePosition(networkedTransform.position);
            }
        }
        if(hasAuthority)
        {
            if (networkedTransform.localRotation != savedRotation)
            {
                CmdUpdateRotation(networkedTransform.localRotation);
                savedRotation = networkedTransform.localRotation;
            }
        }


        //debugging thing, allows server control to be given through inspector
        if(toggleServerControl != toggleCheck)
        {
            if(toggleServerControl)
            {
                CmdTakeServerControl();
                toggleCheck = toggleServerControl;
            }
            else
            {
                CmdRemoveServerControl();
                toggleCheck = toggleServerControl;
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
        //if (!((hasAuthority && !serverControl) || (isServer && serverControl)))
        if(!hasAuthority)
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

        if(hasAuthority && !isServer)
        {
            if(rb)
                rb.isKinematic = true;
        }
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

        if (hasAuthority && !isServer)
        {
            if(rb)
                rb.isKinematic = false;
        }
    }
}
