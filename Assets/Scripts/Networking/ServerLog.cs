using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerLog : MonoBehaviour
{
    public static ServerLog current;
    public GameObject log;
    public Text text;

    public int maxLogEntries = 20;
    private int currentLogEntries = 0;

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
        currentLogEntries++;
        if(currentLogEntries > maxLogEntries) //if we will have more entries than we want 
        {
            //remove the oldest entry
            string currentString = text.text;
            //currentString.Trim();
            int newEnd =  currentString.LastIndexOf('\n');
            string newString ="";
            for(int i =0; i<newEnd; i++)
            {
                newString += currentString[i];
            }
            text.text = newString;

            //currentString.Remove(newEnd);
            //newEnd = currentString.LastIndexOf('\n');
            //currentString.Remove(newEnd, currentString.Length - newEnd);
            //text.text = currentString;
        }
        text.text = data + '\n' + text.text;
    }
}
