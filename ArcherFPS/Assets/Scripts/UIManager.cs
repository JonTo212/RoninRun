using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public PlayerController playerController;
    public PlayerControllerV2 playerControllerV2;

    bool gameStart = true;

    /*void Start()
    {
        Time.timeScale = 0;
        gameStart = false;
    }*/

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
        if (playerController != null)
        {
            //Velocity
            var playerVel = playerController.playerVelocity;
            playerVel.y = 0;
            speedText.text = "Velocity: " + (Mathf.Round(playerVel.magnitude * 100) / 100).ToString();

            //Max velocity
            var maxVel = playerController.playerTopVelocity;
            maxSpeedText.text = "Max Velocity: " + (Mathf.Round(maxVel * 100) / 100).ToString();
        }
        else if(playerControllerV2 != null)
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
}
