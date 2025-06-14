using UnityEngine;

public enum PlayerState
{
    Default,
    WallRunning,
    Grappling,
    Throwing
}

[RequireComponent(typeof(PlayerControllerV2))]
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Default;

    private PlayerControllerV2 controller;
    private WallRun wallRun;
    private Throw throwSystem;
    private PlayerGrapple grapple;


}
