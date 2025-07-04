using UnityEngine;

public enum PlayerState
{
    Default,
    WallRunning,
    Grappling,
    Throwing,
    LedgeGrabbing
}

[RequireComponent(typeof(PlayerControllerV2))]
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Default;

    private PlayerControllerV2 controller;
    private PlayerControllerV2 playerController;
    private WallRun wallRun;
    private Throw throwSystem;
    private PlayerGrapple grapple;
    private LedgeGrab ledgeGrab;

    private void Awake()
    {
        playerController = GetComponent<PlayerControllerV2>();
        wallRun = GetComponent<WallRun>();
        throwSystem = GetComponent<Throw>();
        grapple = GetComponent<PlayerGrapple>();
        ledgeGrab = GetComponent<LedgeGrab>();
    }

    private void Update()
    {
        CheckForStateSwitch();
        RunStateContinuousFunctions();
    }

    private void TransitionToState(PlayerState newState)
    {
        CancelAllOtherStates(newState);
        currentState = newState;
    }

    private void CheckForStateSwitch()
    {
        if (wallRun.WallCheck()) // && currentState != PlayerState.WallRunning) <- this makes it stutter because it tries the new state and then restarts the current one
        {
            TransitionToState(PlayerState.WallRunning);
            return;
        }
        else if (grapple.CheckGrapple()) // && currentState != PlayerState.Grappling)
        {
            TransitionToState(PlayerState.Grappling);
            return;
        }
        else if(throwSystem.CheckThrow())  // && currentState != PlayerState.Throwing)
        {
            TransitionToState(PlayerState.Throwing);
            return;
        }
        else if (ledgeGrab.CanStartLedgeGrab())
        {
            ledgeGrab.StartLedgeGrab();
            TransitionToState(PlayerState.LedgeGrabbing);
            return;
        }
    }

    private void RunStateContinuousFunctions()
    {
        wallRun.ApplyCameraTilt();
        throwSystem.ChooseShurikenType();
        throwSystem.HandleFOV();
        ledgeGrab.UpdateVaultBuffer();

        switch (currentState)
        {
            case PlayerState.Default:
                HandleDefaultState();
                break;

            case PlayerState.WallRunning:
                HandleWallRunningState();
                break;

            case PlayerState.Grappling:
                HandleGrapplingState();
                break;

            case PlayerState.Throwing:
                HandleThrowingState();
                break;

            case PlayerState.LedgeGrabbing:
                HandleLedgeGrabbingState();
                break;

            default:
                currentState = PlayerState.Default;
                HandleDefaultState();
                break;
        }
    }

    private void HandleDefaultState()
    {
        if (ledgeGrab.CanVault())
        {
            ledgeGrab.LedgeJump();
            return;
        }
    }

    private void HandleWallRunningState()
    {
        if(wallRun.canWallBounce && Input.GetKeyDown(KeyCode.Space))
        {
            wallRun.WallJump();
            currentState = PlayerState.Default;
            return;
        }
        else if(!wallRun.WallCheck())
        {
            wallRun.StopWallRunning();
            currentState = PlayerState.Default;
            return;
        }

        wallRun.CalculateCameraTilt();
        wallRun.HandleWallRun();
    }

    private void HandleGrapplingState()
    {
        if(Input.GetMouseButtonUp(1))
        {
            grapple.ReleaseGrapple();
            currentState = PlayerState.Default;
            return;
        }
        else if(playerController.isGrounded || !grapple.IsInValidGrappleRange())
        {
            grapple.StopGrapple();
            currentState = PlayerState.Default;
            return;
        }

        grapple.HandleGrapplePull();
    }

    private void HandleThrowingState()
    {
        if(Input.GetMouseButtonDown(1))
        {
            throwSystem.StopThrow(true);
            currentState = PlayerState.Default;
            return;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            throwSystem.ReleaseThrow();
            currentState = PlayerState.Default;
            return;
        }

        throwSystem.SetIndicatorObjectsActive();
        throwSystem.HandleThrowPower();
    }

    private void HandleLedgeGrabbingState()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            ledgeGrab.LedgeJump();
            currentState = PlayerState.Default;
            return;
        }
        else if(!ledgeGrab.isGrabbingLedge)
        {
            ledgeGrab.Detach();
            currentState = PlayerState.Default;
        }

        ledgeGrab.HandleLedgeHang();
    }

    private void CancelAllOtherStates(PlayerState newState)
    {
        if (newState != PlayerState.WallRunning) wallRun.StopWallRunning();
        if (newState != PlayerState.Grappling) grapple.StopGrapple();
        if (newState != PlayerState.Throwing) throwSystem.StopThrow(true);
        if (newState != PlayerState.LedgeGrabbing) ledgeGrab.Detach();
    }
}
