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
        anim.SetFloat("xInput", Input.GetAxis("Horizontal"));
        anim.SetFloat("zInput", Input.GetAxis("Vertical"));

        if (Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("InAir", true);
        }
        if (playerController.isGrounded)
        {
            anim.SetBool("InAir", false);
        }
    }
}
