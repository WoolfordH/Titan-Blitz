using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
	Speed,
	Damage,
	Jump
}

[ExecuteInEditMode]
public class Powerup : MonoBehaviour {

	public float multiplier;
	public PowerupType powerupType;
	public float timer;
    public Color colour;
    ParticleSystem psMain;
    ParticleSystem[] psAux;

    void Start()
    {
        psMain = GetComponent<ParticleSystem>();
        SetColour();
    }

    void Update()
    {
        SetColour();
    }

    void SetColour()
    {
       
        ParticleSystem.MainModule main = psMain.main;
        main.startColor = new Color(colour.r, colour.g, colour.b, main.startColor.color.a);

        foreach (ParticleSystem ps in psMain.transform.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule auxMain = ps.main;

            if (ps.gameObject.name == "Beam")
            {
                Color col = new Color(colour.r, colour.g, colour.b, auxMain.startColor.color.a);

                float h, s, v;
                Color.RGBToHSV(col, out h, out s, out v);
                s -= .4f;
                v = 1f;
                col = Color.HSVToRGB(h, s, v);

                auxMain.startColor = col;
            }
            else if (ps.gameObject.name == "Smoke")
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
                auxMain.startColor = new Color(colour.r, colour.g, colour.b, auxMain.startColor.color.a);
            }
        }
    }
}
