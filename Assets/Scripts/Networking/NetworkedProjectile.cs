using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedProjectile : NetworkBehaviour
{
    private bool initiated = false;
    private float speed;

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
                speed = GetComponent<Projectile>().speed;
                CmdPassProjectilePath(this.transform.position, this.transform.forward, speed, System.DateTime.UtcNow.Millisecond);
            }
        }
        else
        {

        }
	}


    [Command]
    private void CmdPassProjectilePath(Vector3 position, Vector3 forward, float a_speed, int sentUtc)
    {
        RpcReceiveProjectilePath(position, forward, a_speed, sentUtc);
    }

    [ClientRpc]
    private void RpcReceiveProjectilePath(Vector3 position, Vector3 forward, float a_speed, int sentUtc)
    {
        if (!initiated)
        {
            this.transform.forward = forward;
            speed = a_speed;
            this.transform.position = position + forward * speed * (System.DateTime.UtcNow.Millisecond - sentUtc) / 1000;

            initiated = true;
            ToggleRenderers(true);
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
