using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum KillBarType
{
    RedKill,
    BlueKill,
    Kill,
    Die
}

public class KillFeed : MonoBehaviour {

    public GameObject textHolder;

    public Text txtP1;
    public Text txtP2;

    public GameObject   killBarRed,
                        killBarBlue,
                        killBarKill,
                        killBarDie;

    float timer = 0f;
    public float screenDuration = 3f;
    bool active = false;

    // Use this for initialization
    void Start ()
    {
        HideBar();
    }

    private void Update()
    {
        if (active)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;

                if (killBarBlue.activeSelf)
                {
                    Color col = killBarBlue.GetComponent<Image>().color;
                    killBarBlue.GetComponent<Image>().color = new Color(col.r, col.g, col.b, (timer / screenDuration));
                }
                else if (killBarRed.activeSelf)
                {
                    Color col = killBarRed.GetComponent<Image>().color;
                    killBarRed.GetComponent<Image>().color = new Color(col.r, col.g, col.b, (timer / screenDuration));
                }
                else if (killBarKill.activeSelf)
                {
                    Color col = killBarKill.GetComponent<Image>().color;
                    killBarKill.GetComponent<Image>().color = new Color(col.r, col.g, col.b, (timer / screenDuration));
                }
                else if (killBarDie.activeSelf)
                {
                    Color col = killBarDie.GetComponent<Image>().color;
                    killBarDie.GetComponent<Image>().color = new Color(col.r, col.g, col.b, (timer / screenDuration));
                }
            }
            else
            {
                HideBar();
            }
        }
    }

    public void ShowBar(KillBarType type)
    {
        switch (type)
        {
            case KillBarType.RedKill:
                killBarRed.SetActive(true);
                break;

            case KillBarType.BlueKill:
                killBarBlue.SetActive(true);
                break;

            case KillBarType.Kill:
                killBarKill.SetActive(true);
                break;

            case KillBarType.Die:
                killBarDie.SetActive(true);
                break;
        }

        textHolder.SetActive(true);

        timer = screenDuration;

        active = true;
    }

    void HideBar()
    {
        txtP1.text = "";
        txtP2.text = "";

        textHolder.SetActive(false);
        killBarRed.SetActive(false);
        killBarBlue.SetActive(false);
        killBarKill.SetActive(false);
        killBarDie.SetActive(false);

        Color col = killBarRed.GetComponent<Image>().color;
        killBarRed.GetComponent<Image>().color = new Color(col.r, col.g, col.b, 1f);
        col = killBarBlue.GetComponent<Image>().color;
        killBarBlue.GetComponent<Image>().color = new Color(col.r, col.g, col.b, 1f);
        col = killBarKill.GetComponent<Image>().color;
        killBarKill.GetComponent<Image>().color = new Color(col.r, col.g, col.b, 1f);
        col = killBarDie.GetComponent<Image>().color;
        killBarDie.GetComponent<Image>().color = new Color(col.r, col.g, col.b, 1f);

        active = false;
    }

    public void SetPlayers(string p1Name, string p2Name)
    {
        txtP1.text = p1Name;
        txtP2.text = p2Name;
    }
}
