using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerV2 : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] PlayerControllerV2 playerController;
    [SerializeField] PlayerAbilities playerAbilities;
    [SerializeField] SlowMotionDash dash;
    [SerializeField] WallRun wallRun;
    [SerializeField] Throw shurikenThrow;

    void Update()
    {
        SetAnim();
    }

    void SetAnim()
    {
        anim.SetFloat("xInput", playerController.animXInput);
        anim.SetFloat("zInput", playerController.animZInput);
        anim.SetBool("isCrouching", playerController.crouched);
        anim.SetBool("isSliding", playerController.slide || playerController.slopeSlide);
        anim.SetBool("isUpdrafting", playerAbilities.isUpdrafting);
        anim.SetBool("isDashing", playerAbilities.isDashing || dash.isDashing);
        anim.SetBool("isWallRunning", wallRun.wallRunning);
        anim.SetBool("wallLeft", wallRun.wallLeft);
        anim.SetBool("wallRight", wallRun.wallRight);
        anim.SetBool("isAiming", shurikenThrow.isAiming);
        anim.SetBool("stopThrow", shurikenThrow.stopThrow);

        //Detect if the player is pressing the jump key and hasn't landed yet
        if (Input.GetKey(KeyCode.Space) && !playerController.isGrounded)
        {
            anim.SetBool("isGrounded", false);
        }

        //Player has landed but bhop input detected
        else if (playerController.isGrounded && Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("isGrounded", false);
        }

        //Player has properly landed
        else if (playerController.isGrounded)
        {
            anim.SetBool("isGrounded", true);
        }
    }
}
