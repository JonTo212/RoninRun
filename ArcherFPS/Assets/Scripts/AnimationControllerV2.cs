using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerV2 : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] PlayerControllerV2 playerController;
    [SerializeField] PlayerAbilities playerAbilities;
    [SerializeField] Throw shurikenThrow;

    bool isJumping;

    void Update()
    {
        SetAnim();
    }

    void SetAnim()
    {
        anim.SetFloat("xInput", playerController.animXInput);
        anim.SetFloat("zInput", playerController.animZInput);
        anim.SetBool("isCrouching", playerController.crouched);
        anim.SetBool("isUpdrafting", playerAbilities.isUpdrafting);
        anim.SetBool("isDashing", playerAbilities.isDashing);
        anim.SetBool("isWallRunning", playerAbilities.wallRunning);
        anim.SetBool("wallLeft", playerAbilities.wallLeft);
        anim.SetBool("wallRight", playerAbilities.wallRight);
        anim.SetBool("isAiming", shurikenThrow.isAiming);

        //Detect if the player is pressing the jump key and hasn't landed yet
        if (Input.GetKey(KeyCode.Space) && !playerController.isGrounded)
        {
            anim.SetBool("isGrounded", false);
            isJumping = true;
        }

        //Player has landed but bhop input detected
        else if (playerController.isGrounded && Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("isGrounded", false);
            isJumping = true;
        }

        //Player has properly landed
        else if (playerController.isGrounded)
        {
            anim.SetBool("isGrounded", true);
            isJumping = false;
        }
    }
}
