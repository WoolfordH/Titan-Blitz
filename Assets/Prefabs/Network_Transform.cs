using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Network_Transform : NetworkBehaviour
{

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hasAuthority)
        {
            if (true)//moved enough
            {
                CmdUpdatePosition(transform.position);
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
        if(!hasAuthority)
            transform.position = position;
    }
}
