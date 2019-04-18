using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour {

    Animator anim;
    public bool useIK = true;
    public Transform gripPointL, gripPointR;
    [HideInInspector]
    public bool deathEnd = false;

    [HideInInspector]
    public Quaternion spineRotation;

	// Use this for initialization
	void Start ()
    {
        anim = GetComponent<Animator>();
        spineRotation = Quaternion.Euler(0f, 0f, 14f);
	}

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            if (useIK)
            {
                if (gripPointL != null)
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, gripPointL.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, gripPointL.rotation);
                }
                if (gripPointR != null)
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    anim.SetIKPosition(AvatarIKGoal.RightHand, gripPointR.position);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, gripPointR.rotation);
                }

                anim.SetBoneLocalRotation(HumanBodyBones.Neck, spineRotation);
            }
        }
    }

    public void DeathEnd()
    {
        deathEnd = true;
    }
}
