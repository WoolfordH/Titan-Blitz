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
	Vector3 dirToTarget;

	public Vector3 desiredPosition;


	// Use this for initialization
	void Start () {
		dirToTarget = muzzle.forward;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 target = transform.position + owner.transform.forward;
	
		//check if any enemy players are within radius and look at them if they are
		List<Collider> cols = new List<Collider>(Physics.OverlapSphere (transform.position, viewRadius, GameHandler.current.playerLayer));
		if (cols.Exists(x=> x.GetComponentInParent<Character>().team != owner.team))
		{
			List<Vector3> positions = new List<Vector3>();

			//loop through colliders
			foreach (Collider col in cols)
			{
				
				//if collider is on enemy team
				if (col.GetComponentInParent<Character> ().team != owner.team)
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

			if (positions.Count > 0)
			{
				//Get closest enemy
				target = positions [GetClosest (positions.ToArray ())];
			}
		}

		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (Vector3.ProjectOnPlane(target - transform.position, Vector3.up)), (rotationSpeed * 100) * Time.deltaTime);

		Vector3 newPos = new Vector3 (transform.position.x, target.y, transform.position.z);

		//if the new position is not too far from the owner, move to aim at enemy
		//if (Vector3.Distance (newPos, owner.transform.position) <= maxDistFromOwner)
		//{
			//Move Drone as close to desired location as possible
		//	MoveToDesiredLocation (newPos.y);
		//} 
		//else
		//{

			//Move Drone as close to desired location as possible
			MoveToDesiredLocation (owner.transform.TransformPoint (desiredPosition).y);
		//}
	}

	void Fire()
	{
		//raycast from muzzle and hit first thing found
		RaycastHit hit;
		if (Physics.Raycast (muzzle.position, transform.forward, out hit))
		{
			hit.collider.gameObject.SendMessageUpwards ("Hit", new HitData(damage, hit.point, hit.normal), SendMessageOptions.DontRequireReceiver);
		}
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
		desiredWorldPos = new Vector3 (desiredWorldPos.x, altitude, desiredWorldPos.y);

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
