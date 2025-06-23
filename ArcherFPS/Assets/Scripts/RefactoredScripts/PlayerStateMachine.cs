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

    private void CheckForStateSwitch()
    {
        if(wallRun.WallCheck())
        {
            currentState = PlayerState.WallRunning;
            return;
        }
    }

    private void RunStateContinuousFunctions()
    {
        wallRun.ApplyCameraTilt();

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
        
    }

    private void HandleWallRunningState()
    {
        if(wallRun.canWallBounce && Input.GetKeyUp(KeyCode.Space))
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
        
    }

    private void HandleThrowingState()
    {
        
    }

    private void HandleLedgeGrabbingState()
    {
        
    }

}
