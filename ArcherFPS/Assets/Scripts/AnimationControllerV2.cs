using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerV2 : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] PlayerControllerV2 playerController;
    [SerializeField] PlayerAbilities playerAbilities;
    [SerializeField] Bow bow;

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
        anim.SetBool("updraft", playerAbilities.isUpdrafting);
        anim.SetBool("slide", playerAbilities.isSliding);
        anim.SetBool("isWallRunning", playerAbilities.wallRunning);

        if (Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("inAir", true);
        }
        if (playerController.isGrounded)
        {
            anim.SetBool("inAir", false);
        }
    }
}
