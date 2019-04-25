using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITurntable : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler{

    public Transform turntableObj;

    bool mouseOver;
    public float anglePerSec;

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (mouseOver)
        {
            turntableObj.transform.Rotate(0f, anglePerSec * Time.deltaTime, 0f, Space.Self);
        }
	}
}
