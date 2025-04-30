using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //FPS components
    float fpsDisplayRate = 4.0f; // 4 updates per sec
    int frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;
    public TMP_Text fpsText;
    public TMP_Text timerText;
    float timer;

    //Speed components
    public TMP_Text speedText;
    public TMP_Text maxSpeedText;
    public PlayerControllerV2 playerControllerV2;

    //Abilities
    [SerializeField] Image dashCooldownImage;
    [SerializeField] Button updraftFadeOut;
    [SerializeField] PlayerAbilities playerAbilities;

    //Shurikens
    [SerializeField] Button shurikenFadeOut;
    [SerializeField] ShurikenManager manager;
    [SerializeField] TMP_Text shurikenCountText;
    [SerializeField] TMP_Text shurikenTypeText;
    [SerializeField] Image currentImage;
    int[] originalStarCounts;
    [SerializeField] Sprite[] shurikenImages;
    [SerializeField] string[] shurikenTypes;

    bool gameStart = true;

    void Start()
    {
        /*Time.timeScale = 0;
        gameStart = false;*/

        originalStarCounts = (int[])manager.shurikenCounts.Clone();
    }

    void Update()
    {
        /*if (Input.GetButtonDown("Fire1") && !gameStart)
        {
            gameStart = true;
            Time.timeScale = 1;
        }*/
        if (gameStart)
        {
            RunTimer();
            FPSDisplay();
            SpeedDisplays();

            DashCooldown();
            UpdraftCooldown();
            UpdateShurikenIcon();
        }
        if (Time.timeScale == 1)
        {
            Cursor.visible = false;
        }
    }

    #region FPS, Speed and Timer displays
    void RunTimer()
    {
        timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer % 60F);
        int milliseconds = Mathf.FloorToInt((timer * 100F) % 100F);
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliseconds.ToString("00");
    }

    void FPSDisplay()
    {
        // Do FPS calculation
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0 / fpsDisplayRate)
        {
            fps = Mathf.Round(frameCount / dt);
            frameCount = 0;
            dt -= 1.0f / fpsDisplayRate;
        }

        fpsText.text = ("FPS: ") + fps.ToString();
    }

    void SpeedDisplays()
    {
        /*if (playerController != null)
        {
            //Velocity
            var playerVel = playerController.playerVelocity;
            playerVel.y = 0;
            speedText.text = "Velocity: " + (Mathf.Round(playerVel.magnitude * 100) / 100).ToString();

            //Max velocity
            var maxVel = playerController.playerTopVelocity;
            maxSpeedText.text = "Max Velocity: " + (Mathf.Round(maxVel * 100) / 100).ToString();
        }*/
        if(playerControllerV2 != null)
        {
            //Velocity
            var playerVel = playerControllerV2.clampedVel;
            playerVel.y = 0;
            speedText.text = "Velocity: " + (Mathf.Round(playerVel.magnitude * 100) / 100).ToString();

            //Max velocity
            var maxVel = playerControllerV2.playerTopVelocity;
            maxSpeedText.text = "Max Velocity: " + (Mathf.Round(maxVel * 100) / 100).ToString();
        }
    }
    #endregion

    #region Ability icons

    void DashCooldown()
    {
        dashCooldownImage.fillAmount = playerAbilities.dashCooldown / playerAbilities.dashDelay;
    }

    void UpdraftCooldown()
    {
        if (playerAbilities.canUpdraft)
        {
            updraftFadeOut.interactable = true;
        }
        else
        {
            updraftFadeOut.interactable = false;
        }
    }

    #endregion

    #region Shuriken counts

    void UpdateShurikenIcon()
    {
        shurikenCountText.text = $"{manager.shurikenCounts[manager.selectedIndex]} / {originalStarCounts[manager.selectedIndex]}";
        shurikenTypeText.text = $"{shurikenTypes[manager.selectedIndex]} Shuriken";
        currentImage.sprite = shurikenImages[manager.selectedIndex];
        currentImage.SetNativeSize();

        if (manager.shurikenCounts[manager.selectedIndex] <= 0)
        {
            shurikenFadeOut.interactable = false;
        }
        else
        {
            shurikenFadeOut.interactable = true;
        }
    }

    #endregion
}
