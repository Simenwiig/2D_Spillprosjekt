using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_UI : MonoBehaviour
{
    [Header("Settings")]
    public float gameOverFadeDuration = 3f;
    float gameOverFadeTimer = 3f;

    [Header("UI Elements")]
    public Image UI_Bar_Health;
    public Image UI_Bar_Hunger;
    public Image UI_Bar_Thirst;

    public Image UI_Screen_GameOver;

    PlayerController player;

    bool gameIsOver = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gameOverFadeTimer = gameOverFadeDuration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UI_Bar_Health.fillAmount = player.healthLevel / 100;
        UI_Bar_Hunger.fillAmount = player.hungerLevel / 100;
        UI_Bar_Thirst.fillAmount = player.thirstLevel / 100;

        if (gameIsOver || player.isDead)
            GameOver();
    }

    public void GameOver()
    {
        gameIsOver = true;

        gameOverFadeTimer -= Time.fixedDeltaTime;

        UI_Screen_GameOver.color = new Color(0, 0, 0,  1- (gameOverFadeTimer / gameOverFadeDuration));

      //  UI_Screen_GameOver.CrossFadeColor(new Color(1,1,1,255), gameOverFadeDuration, false, true);


    }
}
