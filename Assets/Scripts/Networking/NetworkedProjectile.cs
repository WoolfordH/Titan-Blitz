using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedProjectile : NetworkBehaviour
{
    private bool initiated = false;

    private void Awake()
    {
        ToggleRenderers(false);
    }

    // Update is called once per frame
    void Update ()
    {
        if(!initiated)
        {
            if(hasAuthority)
            {
                initiated = true;
                CmdPassProjectilePath(this.transform.position, this.transform.forward, System.DateTime.UtcNow.Millisecond);
            }
        }
	}


    [Command]
    private void CmdPassProjectilePath(Vector3 position, Vector3 forward, int sentUtc)
    {
        RpcReceiveProjectilePath(position, forward, sentUtc);
    }

    [ClientRpc]
    private void RpcReceiveProjectilePath(Vector3 position, Vector3 forward, int sentUtc)
    {
        if (!initiated)
        {
            this.transform.forward = forward;
            this.transform.position = position + forward * (System.DateTime.UtcNow.Millisecond - sentUtc) / 1000;
        }
    }


    private void ToggleRenderers(bool setEnabled)
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
}
