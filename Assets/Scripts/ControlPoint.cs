using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum ControlPointState
{
    inactive,
    active

    //Team1Owned,
    //Team2Owned,
    //Team1Capturing,
    //Team2Capturing,
    //Neutral
}

//[ExecuteInEditMode]
public class ControlPoint : NetworkBehaviour
{
    private int pointIdentity;

    public ControlPointState controlState = ControlPointState.inactive;

    [Range(-100f, 100f)]
    public float capturePercent = 0; //positive for team 1, negative for team 2
    [Tooltip("Percentage per second to capture the point")]
    public float captureRate;
    [Tooltip("Increase in capture speed per additional player")]
    public float additionalPlayerMultiplier;


    public GameObject pointCollider;
    public ParticleSystem psMain;
    ParticleSystem[] psAux;

    public GameObject captureInstance;
    public ParticleSystem ps2Main;
    ParticleSystem[] ps2Aux;

    public AudioSource audioSource;

    private List<GameObject> playersOnPoint = new List<GameObject>();
    private int teamOnPointDifference = 0; //positive for team 1, negative for team 2

    //visualisation 
    public Material mat;
    Color colour;
    public Gradient grad;

    // Use this for initialization
    void Start ()
    {
        if(pointCollider)
        {
            //pointCollider.enabled = false;
        }
        else
        {
            throw new Exception("No collider assigned to control point");
        }

        //Activate();
        //psMain = GetComponent<ParticleSystem>();
        CalculateColour();
        //Material mat = this.GetComponent<Material>();
    }

    public void Initialise(int pointID)
    {
        pointIdentity = pointID;
    }

    // Update is called once per frame
    void Update ()
    {
        if (controlState == ControlPointState.active)
        {
            if (isServer)
            {
                CaptureChange();
        
                if (capturePercent >= 100)
                {
                    //team 1 capture
                    CTPManager.current.CapturePoint(1, pointIdentity);
                    
                }
                else if (capturePercent <= -100)
                {
                    //team 2 capture
                    CTPManager.current.CapturePoint(2, pointIdentity);
                }


                
            }
        }


        //Clear player list at after capture change has been 

        if (isServer)
        {
            playersOnPoint.Clear();
            teamOnPointDifference = 0;
        }

        //if (controlState == ControlPointState.inactive)
        //{
        //    Activate();
        //}
        CalculateColour();


    }

    private void Visualise()
    {
        if (controlState == ControlPointState.active)
        {
            if (PlayerConnection.current.playerObject.GetComponent<CharacterHandler>().GetTeam() == 1)
            {
                if (capturePercent > 0)
                {
                    mat.color = new Color(0, Mathf.Abs(capturePercent) * 0.01f, 0, 0.33f); //green
                }
                else
                {
                    mat.color = new Color(Mathf.Abs(capturePercent) * 0.01f, 0, 0, 0.33f); //red
                }
            }
            else
            {
                if (capturePercent < 0)
                {
                    mat.color = new Color(0, Mathf.Abs(capturePercent) * 0.01f, 0, 0.33f); //green
                }
                else
                {
                    mat.color = new Color(Mathf.Abs(capturePercent) * 0.01f, 0, 0, 0.33f); //red
                }
            }
        }
        else
        {
            mat.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }

    private void CaptureChange()
    {
        float newPercent = capturePercent;

        if(teamOnPointDifference != 0)
        {
            //increase the percentage by the capture rate * the players on the point 
            //this increases for team 1 and decreases for team 2
            float increase = teamOnPointDifference * captureRate * Time.deltaTime;
            //include scaling so 2 players can not just be double the point gain


            newPercent +=  increase;


        }

        newPercent = Mathf.Clamp(newPercent, -100f, 100f);

        if (newPercent != capturePercent)
        {
            //get all players on point and update their capture bar UI
            //foreach (GameObject c in playersOnPoint)
            //{
            //    c.GetComponent<CharacterHandler>().captureBar.fillAmount = newPercent / 100;
            //}

            RpcUpdateCapturePercent(newPercent);
        }
    }


    [ClientRpc]
    private void RpcUpdateCapturePercent(float percent)
    {
        capturePercent = percent;
        if(PlayerConnection.current.playerObject.GetComponent<CharacterHandler>())
            PlayerConnection.current.playerObject.GetComponent<CharacterHandler>().captureBar.fillAmount = Mathf.Abs(percent) / 100;
        //update ui
    }



    public void Activate()
    {
        RpcActivate();
    }

    [ClientRpc]
    private void RpcActivate()
    {
        capturePercent = 0;
        RpcUpdateCapturePercent(0);
        captureInstance.SetActive(false);
        pointCollider.SetActive(true);
        psMain.Play();
        controlState = ControlPointState.active;
        audioSource.Play();
    }

    public void Deactivate()
    {
        RpcDeactivate();
    }

    [ClientRpc]
    private void RpcDeactivate()
    {
        capturePercent = 0;
        RpcUpdateCapturePercent(0);
        pointCollider.SetActive(false);
        psMain.Stop();
        SetColour(true);
        captureInstance.SetActive(true);

        audioSource.Stop();

        foreach (ParticleSystem ps in psMain.transform.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop();
        }

            controlState = ControlPointState.inactive;
    }


    void CalculateColour()
    {
        if (controlState == ControlPointState.active)
        {
            colour = grad.Evaluate(((capturePercent / 100f)+1f)/2f);
        }

        SetColour();
    }

    void SetColour(bool secondary = false)
    {
        ParticleSystem.MainModule main;
        ParticleSystem.Particle[] particles;
        int partCount;

        if (secondary)
        {
            main = ps2Main.main;
            particles = new ParticleSystem.Particle[ps2Main.main.maxParticles];
            partCount = ps2Main.GetParticles(particles);
        }
        else
        {
            main = psMain.main;
            particles = new ParticleSystem.Particle[psMain.main.maxParticles];
            partCount = psMain.GetParticles(particles);
        }

        

        for (int i = 0; i < partCount; i++)
        {
            particles[i].color = new Color(colour.r, colour.g, colour.b, main.startColor.color.a); ;
        }


        if (secondary)
        {
            ps2Main.SetParticles(particles, partCount);
        }
        else
        {
            psMain.SetParticles(particles, partCount);
        }

        ParticleSystem[] systems;

        if (secondary)
        {
             systems = ps2Main.transform.GetComponentsInChildren<ParticleSystem>();
        }
        else
        {
            systems = psMain.transform.GetComponentsInChildren<ParticleSystem>();
        }

        foreach (ParticleSystem ps in systems)
        {
            ParticleSystem.MainModule auxMain = ps.main;

            if (ps.gameObject.name == "Rings Inner")
            {
                Color col = new Color(colour.r, colour.g, colour.b, auxMain.startColor.color.a);

                float h, s, v;
                Color.RGBToHSV(col, out h, out s, out v);
                h -= .05f;
                s += .3f;
                v -= .2f;
                col = Color.HSVToRGB(h, s, v);

                //auxMain.startColor = col;
                //ps.GetComponent<ParticleSystemRenderer>().sharedMaterial.color = col;
                particles = new ParticleSystem.Particle[ps.main.maxParticles];
                partCount = ps.GetParticles(particles);

                for (int i = 0; i < partCount; i++)
                {
                    particles[i].color = col;
                }

                ps.SetParticles(particles, partCount);
            }
            else if (ps.gameObject.name == "Energy Wave" || ps.gameObject.name == "Center" || ps.gameObject.name == "Particles" || ps.gameObject.name == "Particles Stretched")
            {
                Color col = new Color(colour.r, colour.g, colour.b, auxMain.startColor.color.a);

                Color col1 = new Color(col.r * 2f, col.g * 2f, col.b + .1f, col.a);
                Color col2 = new Color(col.r, col.g * 2f, col.b + .3f, col.a);

                ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient(col1, col2);
                grad.mode = ParticleSystemGradientMode.TwoColors;

                auxMain.startColor = grad;
            }
            else
            {
                //auxMain.startColor = new Color(colour.r, colour.g, colour.b, auxMain.startColor.color.a);
            }
        }
    }


    //if the player is in the array return its index else return -1
    private int FindPlayerInArray(GameObject player)
    {
        for(int i = 0; i < playersOnPoint.Count; i++)
        {
            if(playersOnPoint[i] == player)
            {
                return i;
            }
        }

        return -1;
    }

    //[Server]
    //private void OnTriggerEnter(Collider other)
    //{
    //    CharacterHandler character = other.GetComponentInParent<CharacterHandler>();
    //    if (character) //if it has a character handle - is a player
    //    {
    //        if (FindPlayerInArray(character.gameObject) == -1) //if the player is not already in the list
    //        {
    //            if (character.GetTeam() == 1)//team 1
    //            {
    //                playersOnPoint[playersOnPoint.Count] = character.gameObject;
    //                //playersOnPointCount++;
    //                teamOnPointDifference++;
    //            }
    //            else if(character.GetTeam() == 2)
    //            {
    //                playersOnPoint[playersOnPoint.Count] = character.gameObject;
    //                //playersOnPointCount++;
    //                teamOnPointDifference--;
    //            }
    //            else
    //            {
    //                //team 0
    //            }
    //        }
    //    }
    //}

    [Server]
    private void OnTriggerStay(Collider other)
    {
        CharacterHandler character = other.GetComponentInParent<CharacterHandler>();
        if (character) //if it has a character handle - is a player
        {
            if (character.GetTeam() == 1)//team 1
            {
                playersOnPoint.Add(character.gameObject);
                //playersOnPointCount++;
                teamOnPointDifference++;
            }
            else if (character.GetTeam() == 2)
            {
                playersOnPoint.Add(character.gameObject);
                //playersOnPointCount++;
                teamOnPointDifference--;
            }
            else
            {
                //team 0
            }
        }
    }

    //[Server]
    //private void OnTriggerExit(Collider other)
    //{
    //    CharacterHandler character = other.GetComponentInParent<CharacterHandler>();
    //    if (character) //if it has a character handle - is a player
    //    {
    //        int index = FindPlayerInArray(character.gameObject);
    //        if (index != -1)//player was in array
    //        {
    //            //remove from array
    //            for(int i = index;i< playersOnPoint.Count - 1;i++)
    //            {
    //                playersOnPoint[i] = playersOnPoint[i + 1];
    //            }
    //            if(character.GetTeam() == 1)//is team 1
    //            {
    //                teamOnPointDifference--;
    //            }
    //            else if(character.GetTeam() == 2)
    //            {
    //                teamOnPointDifference++;
    //            }
    //
    //            //playersOnPointCount--;
    //
    //
    //        }
    //
    //    }
    //}
}
