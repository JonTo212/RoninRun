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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetBool("Jump", true);
        }
        else if (playerController.isGrounded)
        {
            anim.SetBool("Jump", false);
        }
    }
}
