using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    //Dash particles
    private ParticleSystem forwardDashParticles;
    private ParticleSystem backwardDashParticles;
    private ParticleSystem rightDashParticles;
    private ParticleSystem leftDashParticles;

    //Updraft particles
    private ParticleSystem updraftParticles;

    //Dashing variables
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool dashCooldownActive;
    public float dashPower = 30f;
    public float dashDelay = 2;
    [HideInInspector] public float dashCooldown = 0;
    [HideInInspector] public bool hasDashed;
    private Vector3 dashDir;

    //Updraft variables
    [HideInInspector] public bool canUpdraft;
    [HideInInspector] public bool isUpdrafting;
    public float updraftHeight = 15f;
    [HideInInspector] public bool updraftInput;

    private PlayerControllerV2 playerController;
    private CharacterController characterController;

    void Start()
    {
        playerController = GetComponent<PlayerControllerV2>();
        characterController = GetComponent<CharacterController>();
        canUpdraft = true;
        GetParticles();
    }

    private void Update()
    {
        HandleDash();
        HandleUpdraft();
    }

    private void GetParticles()
    {
        forwardDashParticles = GameObject.FindGameObjectWithTag("ForwardDashParticles").GetComponent<ParticleSystem>();
        backwardDashParticles = GameObject.FindGameObjectWithTag("BackwardDashParticles").GetComponent<ParticleSystem>();
        rightDashParticles = GameObject.FindGameObjectWithTag("RightDashParticles").GetComponent<ParticleSystem>();
        leftDashParticles = GameObject.FindGameObjectWithTag("LeftDashParticles").GetComponent<ParticleSystem>();
        updraftParticles = GameObject.FindGameObjectWithTag("UpdraftParticles").GetComponent<ParticleSystem>();
    }

    #region Dashing
    void HandleDash()
    {
        bool isTryingToDash = Input.GetKeyDown(KeyCode.E);
        dashCooldown += Time.deltaTime;

        if (isTryingToDash && !isDashing && dashCooldown > dashDelay)
        {
            StartDash();
            dashCooldown = 0;
        }
    }

    void StartDash()
    {
        playerController.playerVelocity.z = 0;
        isDashing = true;
        dashCooldownActive = false;
        hasDashed = true;
        canUpdraft = true;
        PlayDashParticles();
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        float dashDuration = 0.4f;
        float elapsedTime = 0f;

        dashDir = (playerController.wishdir == Vector3.zero ? transform.forward : playerController.wishdir).normalized;
        playerController.playerVelocity = dashDir.normalized * dashPower;

        while (elapsedTime <= dashDuration)
        {
            if (dashDir == Vector3.zero)
            {
                //Dash forward when 0 input
                //playerController.playerVelocity = transform.forward * dashPower;
                characterController.Move(transform.forward * dashPower * Time.deltaTime);
            }
            else
            {
                //playerController.playerVelocity = playerController.wishdir * dashPower;
                characterController.Move(dashDir * dashPower * Time.deltaTime);
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        EndDash();
    }

    void EndDash()
    {
        isDashing = false;
        dashCooldownActive = true;
        if (dashDir != Vector3.zero)
            playerController.playerVelocity = dashDir * dashPower;
        else
            playerController.playerVelocity = transform.forward * dashPower;
    }

    void PlayDashParticles()
    {
        Vector3 inputVector = playerController.inputVector;

        if (inputVector.z > 0 && Mathf.Abs(inputVector.x) <= inputVector.z)
        {
            //Forward and forward-diagonal dashes play the forward dash particles
            forwardDashParticles.Play();
            return;
        }

        if (inputVector.z < 0 && Mathf.Abs(inputVector.x) >= inputVector.z)
        {
            //Backward and backward-diagonal dashes play the backward dash particles
            backwardDashParticles.Play();
            return;
        }

        if (inputVector.x > 0)
        {
            rightDashParticles.Play();
            return;
        }

        if (inputVector.x < 0)
        {
            leftDashParticles.Play();
            return;
        }

        forwardDashParticles.Play();
    }
    #endregion

    #region Updrafting
    public void HandleUpdraft()
    {
        updraftInput = Input.GetKeyDown(KeyCode.Q);

        if (updraftInput && canUpdraft && !playerController.isGrounded)
        {
            StartUpdraft();
        }
    }

    IEnumerator Updraft()
    {
        float updraftDuration = 0.4f;
        float elapsedTime = 0f;

        while (elapsedTime <= updraftDuration)
        {
            playerController.playerVelocity.y = updraftHeight;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        EndUpdraft();
    }

    void StartUpdraft()
    {
        isUpdrafting = true;
        updraftParticles.Play();
        canUpdraft = false;
        StartCoroutine(Updraft());
    }

    void EndUpdraft()
    {
        isUpdrafting = false;
        updraftInput = false;
    }
    #endregion
}