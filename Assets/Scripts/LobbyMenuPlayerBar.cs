using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenuPlayerBar : MonoBehaviour
{
    public Text playerName;
    public GameObject lightIcon;
    public GameObject heavyIcon;
    public GameObject selfIcon;

    public void ActivateBar(string name, int character, bool isSelf)
    {
        this.gameObject.SetActive(true);
        playerName.text = name;
        if(character == 1)
        {
            lightIcon.SetActive(true);
            heavyIcon.SetActive(false);
        }
        else if (character == 2)
        {
            lightIcon.SetActive(false);
            heavyIcon.SetActive(true);
        }

        if(isSelf)
        {
            selfIcon.SetActive(true);
        }
    }

    public void DeActivateBar()
    {
        this.gameObject.SetActive(false);
        playerName.text = "";
        lightIcon.SetActive(false);
        heavyIcon.SetActive(false);
        selfIcon.SetActive(false);
    }
}
