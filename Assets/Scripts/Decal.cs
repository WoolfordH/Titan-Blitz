using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Decal : MonoBehaviour {

    public float delay;
    public float fadeLength;
    float timer;
    Material mat;

	// Use this for initialization
	void Start () {
        timer = delay;
        mat = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
        if (delay <= 0f)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            delay -= Time.deltaTime;
        }
	}

    IEnumerator FadeOut()
    {
        float fadeTimer = fadeLength;


        while (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            float a = fadeTimer / fadeLength;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, a);

            yield return new WaitForEndOfFrame();
        }

        NetworkServer.Destroy(this.gameObject);
    }
}
