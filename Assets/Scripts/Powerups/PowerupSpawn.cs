using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PowerupSpawn : NetworkBehaviour {

    public PowerupType powerup;
    public float respawnDelay;
    float timer;
    bool spawned = false;

	// Use this for initialization
	void Start () {
        timer = 0f;
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer)
        {
            if (!spawned)
            {
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    GameObject spawnObj = null;

                    switch (powerup)
                    {
                        case PowerupType.Damage:
                            spawnObj = Instantiate(GameHandler.current.powerupDamagePrefab, transform.position, Quaternion.identity);
                            break;
                        case PowerupType.Speed:
                            spawnObj = Instantiate(GameHandler.current.powerupSpeedPrefab, transform.position, Quaternion.identity);
                            break;
                        case PowerupType.Jump:
                            spawnObj = Instantiate(GameHandler.current.powerupJumpPrefab, transform.position, Quaternion.identity);
                            break;
                    }

                    spawnObj.GetComponent<Powerup>().spawn = this;

                    NetworkServer.Spawn(spawnObj);
                    spawned = true;
                }
            }
        }
	}

    public void SetSpawned(bool isSpawned)
    {
        spawned = isSpawned;

        if (!isSpawned)
        {
            timer = respawnDelay;
        }
    }
}
