using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JettAnimationController : MonoBehaviour
{
    [SerializeField] Animator armsAnimator;

    void Start()
    {
        armsAnimator.speed = 3;
    }

    public void PlayForwardDashAnimation()
    {
        armsAnimator.Play("ForwardDash");
    }

    public void PlayRightDashAnimation()
    {
        armsAnimator.Play("RightDash");
    }

    public void PlayBackwardDashAnimation()
    {
        armsAnimator.Play("BackwardDash");
    }

    public void PlayLeftDashAnimation()
    {
        armsAnimator.Play("LeftDash");
    }
}
