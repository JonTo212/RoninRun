using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    Animator anim;

    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    Bow bow;

    void Update()
    {
        SetAnim();
    }

    void SetAnim()
    {
        anim.SetBool("Aiming", bow.isAiming);
        anim.SetFloat("xInput", playerController.inputVector.x);
        anim.SetFloat("zInput", playerController.inputVector.z);
        anim.SetBool("InAir", !playerController.isGrounded);
    }
}
