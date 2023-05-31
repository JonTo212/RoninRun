using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JettController : MonoBehaviour
{
    //Dash particles
    [SerializeField] ParticleSystem forwardDashParticles;
    [SerializeField] ParticleSystem backwardDashParticles;
    [SerializeField] ParticleSystem rightDashParticles;
    [SerializeField] ParticleSystem leftDashParticles;

    //Updraft particles
    [SerializeField] ParticleSystem updraftParticles;

    //Floating particles
    [SerializeField] ParticleSystem floatingParticles;


    //Updraft variables
    public bool isUpdrafting = false;
    bool isTryingToUpdraft;
    float lastTimeUpdrafted = 0f;
    float updraftHeight = 15f;
    float updraftDelay = 0.2f;
    public float updraftAttempts = 0;
    public float maxUpdrafts = 2;
    //bool hasUpdrafted = false; //For floating only after updrafting

    //Dashing variables
    public bool isDashing;
    public bool dashCooldownActive;
    float dashStartTime;
    Vector3 currentForward;
    Vector3 currentVel;
    public float dashDelay = 2;
    public float dashCooldown = 0;
    //bool hasDashed = false;

    //Floating variables
    float lastJumpVelocityY = 0f;
    bool isFalling = false;
    float defaultGravity;

    [SerializeField] LucioSurf lucioSurf;
    PlayerController playerController;
    CharacterController characterController;
    [SerializeField] GameObject arms;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        defaultGravity = playerController.gravity;
        dashCooldown = dashDelay;
    }
    void Update()
    {
        HandleDash();
        HandleUpdraft();
        HandleIsFalling();
        //No floating if lucio surf wall bouncing
        if (lucioSurf != null)
        {
            if (!lucioSurf.wallHit)
            {
                HandleFloat();
            }
            else if (lucioSurf.wallHit)
            {
                updraftAttempts = 0;
            }
        }
        else if (lucioSurf == null)
        {
            HandleFloat();
        }
        if (playerController.isGrounded)
        {
            ResetUpdraft();
            //ResetDash() //For updrafting only after dashing
        }
    }

    #region Dashing
    void HandleDash()
    {
        bool isTryingToDash = Input.GetKeyDown(KeyCode.E);
        dashCooldown += Time.deltaTime;

        if (isTryingToDash && !isDashing && dashCooldown > dashDelay)
        {
            OnStartDash();
            dashCooldown = 0;
        }

        if (isDashing)
        {
            if (Time.time - dashStartTime <= 0.4f)
            {
                if (currentVel.Equals(Vector3.zero))
                {
                    //No input, dash forward
                    characterController.Move(currentForward * 30f * Time.deltaTime);
                }
                else
                {
                    characterController.Move(currentVel * 30f * Time.deltaTime);
                }
            }
            else
            {
                OnDashEnd();
            }
        }
    }

    /*void ResetDash()
    {
        hasDashed = false;
    }*/

    void OnStartDash()
    {
        playerController.playerVelocity.z = 0;
        isDashing = true;
        dashStartTime = Time.time;
        currentForward = transform.forward;
        currentVel = playerController.wishdir;
        dashCooldownActive = false;
        PlayDashParticles();
        if (arms != null)
        {
            arms.SetActive(false);
        }
    }

    void OnDashEnd()
    {
        isDashing = false;
        dashStartTime = 0;
        dashCooldownActive = true;
        if (arms != null)
        {
            arms.SetActive(true);
        }
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
    void HandleUpdraft()
    {
        isTryingToUpdraft = Input.GetKeyDown(KeyCode.Q);

        if (Time.time - lastTimeUpdrafted < updraftDelay)
        {
            if (isUpdrafting)
            {
                OnUpdraftEnd();
            }
            return;
        }

        if (isTryingToUpdraft && updraftAttempts < maxUpdrafts && !playerController.isGrounded)
        {
            OnUpdraftStart();
            Updraft();
        }
    }

    void Updraft()
    {
        playerController.playerVelocity.y = updraftHeight;
        /*if (updraftAttempts == 2 && playerController.playerVelocity.y > 0)
        {
            playerController.playerVelocity.y = updraftHeight * 0.75f;
        }
        else
        {
            playerController.playerVelocity.y = updraftHeight;
        }*/
    }

    void ResetUpdraft()
    {
        updraftAttempts = 0;
        //hasUpdrafted = false;
    }

    void OnUpdraftStart()
    {
        isUpdrafting = true;
        lastTimeUpdrafted = Time.time;
        updraftAttempts++;
        updraftParticles.Play();
    }

    void OnUpdraftEnd()
    {
        isUpdrafting = false;
        //hasUpdrafted = true;
    }

    #endregion

    #region Floating
    void HandleIsFalling()
    {
        if (!playerController.isGrounded &&
            playerController.playerVelocity.y <= 0
            && playerController.playerVelocity.y < lastJumpVelocityY)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        lastJumpVelocityY = playerController.playerVelocity.y;

    }

    float timer = 0;
    void HandleFloat()
    {
        bool isTryingToFloat = isFalling && Input.GetKey(KeyCode.Space);
        if (isTryingToFloat)
        {
            //floatingParticles.Emit(1);
            playerController.gravity = defaultGravity / 5f;
        }
        else
        {
            playerController.gravity = defaultGravity;
        }


        if (isFalling)
        {
            timer += Time.deltaTime;
        }
        if (playerController.isGrounded)
        {
            timer = 0;
        }
        if (isTryingToFloat && timer > 0.5f)
        {
            floatingParticles.Emit(1);
        }
    }
    #endregion
}