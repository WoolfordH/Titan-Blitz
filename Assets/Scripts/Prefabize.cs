using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Prefabize : MonoBehaviour {

    public GameObject[] prefabs;

	// Use this for initialization
	void Start ()
    {
        foreach (GameObject prefab in prefabs)
        {
            //get name
            string name = prefab.name;

            //get array of all objects with name
            GameObject[] sceneObjects = FindObjectsOfType<GameObject>();
            List<GameObject> replaceObjects = new List<GameObject>();

            foreach (GameObject obj in sceneObjects)
            {
                if(obj.name.Contains(name))
                {
                    replaceObjects.Add(obj);
                }
            }

            foreach (GameObject obj in replaceObjects)
            {
                //replace each object with prefab
                GameObject newPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                newPrefab.transform.position = obj.transform.position;
                newPrefab.transform.rotation = obj.transform.rotation;
                newPrefab.transform.parent = obj.transform.parent;
                newPrefab.transform.localScale = obj.transform.localScale;

                //destroy object
                Destroy(obj);
            }

        }

        Debug.Log("Prefab Replace Complete!");
	}
}
