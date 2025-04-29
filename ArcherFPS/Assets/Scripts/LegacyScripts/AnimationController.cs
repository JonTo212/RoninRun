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
    LucioSurf wallrun;

    [SerializeField]
    Bow bow;

    [SerializeField]
    JettController jett;

    [SerializeField]
    LucioSurf lucio;

    void Update()
    {
        SetAnim();
    }

    void SetAnim()
    {
        anim.SetBool("isAiming", bow.isAiming);
        anim.SetFloat("xInput", Input.GetAxis("Horizontal"));
        anim.SetFloat("zInput", Input.GetAxis("Vertical"));
        anim.SetBool("crouching", playerController.crouched);
        anim.SetBool("dashInput", jett.isDashing);
        anim.SetBool("updraft", jett.isUpdrafting);
        anim.SetBool("slide", playerController.slide);
        anim.SetBool("isWallRunning", lucio.wallRunning);

        if (Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("inAir", true);
        }
        if (playerController.isGrounded)
        {
            anim.SetBool("inAir", false);
        }

        if (wallrun != null)
        {
            anim.SetBool("wallLeft", wallrun.wallLeft);
            anim.SetBool("wallRight", wallrun.wallRight);
        }
    }
}
