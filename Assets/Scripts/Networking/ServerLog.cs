using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerLog : MonoBehaviour
{
    public static ServerLog current;
    public GameObject log;
    public Text text;

    private void Awake()
    {
        current = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote)) // `
        {
            log.SetActive(!log.activeInHierarchy);
        }
    }

    public void LogData(string data)
    {
        text.text = data + '\n' + text.text;
    }
}
