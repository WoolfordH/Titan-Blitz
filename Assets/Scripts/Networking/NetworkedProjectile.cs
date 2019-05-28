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
        transform.position += transform.forward * speed * Time.deltaTime;

        if(!initiated)
        {
            if(hasAuthority)
            {
                

                initiated = true;
                speed = GetComponent<Projectile>().speed;
                int shotTime = GetComponent<Projectile>().shotTime;
                float timePassed = (System.DateTime.UtcNow.Millisecond - shotTime) * 0.001f;
                if (timePassed < 0)
                    timePassed = 0;

                Vector3 position = this.transform.position + this.transform.forward * speed * timePassed;
                CmdPassProjectilePath(this.transform.position, this.transform.forward, speed, System.DateTime.UtcNow.Millisecond);

                Debug.Log("Ping: " + timePassed.ToString());
            }
        }
        else
        {
            if(hasAuthority)
            {
                if(speed != GetComponent<Projectile>().speed)
                {
                    CmdUpdateSpeed(GetComponent<Projectile>().speed);
                }
            }
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

    [Command]
    private void CmdUpdateSpeed(float a_speed)
    {
        RpcUpdateSpeed(a_speed);
    }

    [ClientRpc]
    private void RpcUpdateSpeed(float a_speed)
    {
        speed = a_speed;
    }
}
