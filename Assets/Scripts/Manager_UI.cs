using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager_UI : MonoBehaviour
{
    PlayerController player;
    bool gameIsOver = false;

    [Header("Settings")]
    public float gameOverFadeDuration = 3f;
    float gameOverFadeTimer = 3f;

    [Header("UI Elements")]
    public Image UI_Bar_Health;
    public Image UI_Bar_Hunger;
    public Image UI_Bar_Thirst;

    public Image UI_Screen_GameOver;
    public Image UI_Victory;

    [Header("Interface Elements")]
    public Image LeftStick;
    public Image LeftStick_Dot;
    public Image RightStick;
    public Image RightStick_Dot;

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
            GameOver(!player.isDead);
    }

    public void GameOver(bool playerWon = false)
    {
        gameIsOver = true;

        gameOverFadeTimer -= Time.fixedDeltaTime;

        UI_Screen_GameOver.color = new Color(0, 0, 0,  1f - (gameOverFadeTimer / gameOverFadeDuration));

        if(playerWon)
            UI_Victory.color = new Color(1, 1, 1, 1f -(gameOverFadeTimer / gameOverFadeDuration));

        //  UI_Screen_GameOver.CrossFadeColor(new Color(1,1,1,255), gameOverFadeDuration, false, true);

        if (gameOverFadeTimer < 0)
        {
            SceneManager.LoadScene("StartMenu");
        }
    }

    /// returns the relative direction from the controlPad to pixelCordinates
   public static Vector3 StickController(Image stick, Image stickCircle, Vector2 pixelCordinates, bool isHeldDown, Camera camera)
    {
        float stickRange = 50;

        Vector3 position = pixelCordinates - (Vector2)stick.rectTransform.position;
        stickCircle.rectTransform.position = stick.rectTransform.position + position.normalized * Mathf.Min(position.magnitude, stickRange);

        Debug.DrawRay(stick.rectTransform.position, position, Color.red, Time.fixedDeltaTime);

        return position;
    }
}
