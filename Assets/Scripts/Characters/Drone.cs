using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {

	public Transform muzzle;
	public int damage;
	public float viewRadius;
	public float rotationSpeed = .1f;
	public float movementSpeed = 2f;
	public Character owner;
	public float maxDistFromOwner;
	public float fireDelay = 1f;
	float timer = 0f;
	bool hasTarget = false;
	bool readyToFire = false;

	public Vector3 desiredPosition;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		CheckForTarget ();
		UpdateTimer ();

		if (hasTarget)
		{
			if (readyToFire)
			{
				Fire ();
			}
		}
	}
		

	void UpdateTimer()
	{
		if (timer <= 0f)
		{
			readyToFire = true;
		}
		else
		{
			readyToFire = false;
			timer -= Time.deltaTime;
		}
	}

	void CheckForTarget()
	{
		Vector3 target = owner.transform.TransformPoint(desiredPosition) + owner.transform.forward;

        List<Vector3> positions = new List<Vector3>();

        //check if any enemy players are within radius and look at them if they are
        List<Collider> cols = new List<Collider>(Physics.OverlapSphere (transform.position, viewRadius, GameHandler.current.playerLayer));
		if (cols.Exists(x=> x.GetComponentInParent<CharacterHandler>().GetTeam() != owner.handler.GetTeam()))
		{
			
			//loop through colliders
			foreach (Collider col in cols)
			{

				//if collider is on enemy team
				if (col.GetComponentInParent<CharacterHandler>().GetTeam() != owner.handler.GetTeam())
				{
					Vector3 pos = col.GetComponentInParent<Character> ().handler.headPos.position;

					//if it hasnt already been added
					if (!positions.Contains (pos))
					{
						//line of sight check
						RaycastHit hit;
						if(Physics.Raycast(transform.position, pos - transform.position, out hit, viewRadius, GameHandler.current.playerLayer))
						{
							Debug.DrawRay (transform.position, (pos - transform.position) * viewRadius, Color.red);

							//if we hit the same object add the position to the list of enemy positions
							if (hit.collider.transform.root == col.transform.root)
							{
								positions.Add (pos);
							}
						}
					}
				}
			}

		}

        if (positions.Count > 0)
        {
            //Get closest enemy
            target = positions[GetClosest(positions.ToArray())];
            hasTarget = true;
        }
        else
        {
            hasTarget = false;
        }

        transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (Vector3.ProjectOnPlane(target - transform.position, Vector3.up)), (rotationSpeed * 100) * Time.deltaTime);

		Vector3 newPos = new Vector3 (transform.position.x, target.y, transform.position.z);
		Debug.DrawLine (transform.position, newPos, Color.green);

		//Move Drone as close to desired location as possible
		MoveToDesiredLocation (target.y);
	}

	void Fire()
	{

		//Debug.Log ("Drone Fired!");

        //raycast from muzzle and hit first thing found

        //RaycastHit hit;
        //if (Physics.Raycast (muzzle.position, transform.forward, out hit))
        //{
        //	hit.collider.gameObject.SendMessageUpwards ("Hit", new HitData(damage, hit.point, hit.normal), SendMessageOptions.DontRequireReceiver);
        //}

		Projectile proj = Instantiate(GameHandler.current.projectile, muzzle.position, muzzle.rotation).GetComponent<Projectile>();
		proj.owners.Add(this.transform);
		proj.owners.Add(owner.transform);
        proj.senderID = owner.id;
		proj.dmg = new DAMAGE (damage);

		timer = fireDelay;
	}


	int GetClosest(Vector3[] positions)
	{
		float closestDist = 10000000;
		int closestIndex = -1;

		for (int i = 0; i < positions.Length; i++)
		{
			float dist = Vector3.Distance (transform.position, positions [i]);

			if (dist < closestDist)
			{
				closestDist = dist;
				closestIndex = i;
			}
		}

		return closestIndex;
	}


	void MoveToDesiredLocation(float altitude)
	{
		RaycastHit hit;
		Vector3 targetPosition = Vector3.zero;
		Vector3 desiredWorldPos = owner.transform.TransformPoint (desiredPosition);
		desiredWorldPos = new Vector3 (desiredWorldPos.x, altitude, desiredWorldPos.z);

		//raycast between desired location and owner
		if (Physics.Raycast (owner.handler.headPos.position, desiredWorldPos - owner.handler.headPos.position, out hit, (desiredWorldPos - owner.handler.headPos.position).magnitude))
		{
			//if it doesnt hit the owner or this
			if (hit.collider.transform.root != owner.transform.root && hit.collider.transform.root != transform.root)
			{
				//target position becomes equal to 80% of the vector to the hit point
				targetPosition = ((hit.point - owner.handler.headPos.position) * .8f) + owner.handler.headPos.position;
			}
		}
		else
		{
			targetPosition = desiredWorldPos;
		}


		transform.position = Vector3.Lerp(transform.position, targetPosition, movementSpeed * Time.deltaTime);
	}
		
		
}
