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
    public bool isPaused;

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
    public Button Button_Melee;
    public Button Button_Ranged;
    public Button Button_Blueprint;
    public Button Button_Blueprint_Close;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        gameOverFadeTimer = gameOverFadeDuration;


        Button_Melee.onClick.AddListener(SwitchingToMelee);
        Button_Ranged.onClick.AddListener(SwitchingToFireArm);

        Button_Blueprint.onClick.AddListener(OnSelectingBlueprint);
        Button_Blueprint_Close.onClick.AddListener(OnClosingBlueprint);
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

    public void GameOver(bool playerWon = false)
    {
        if (gameOverFadeTimer < 0 || playerWon)
        {
            SceneManager.LoadScene("StartMenu");
            return;
        }
        gameIsOver = true;

        gameOverFadeTimer -= Time.fixedDeltaTime;

        UI_Screen_GameOver.color = new Color(0, 0, 0,  1f - (gameOverFadeTimer / gameOverFadeDuration));

        //  UI_Screen_GameOver.CrossFadeColor(new Color(1,1,1,255), gameOverFadeDuration, false, true);


    }

    /// returns the relative direction from the controlPad to pixelCordinates
   public static Vector3 StickController(Image stick, Image stickCircle, Vector2 pixelCordinates, Camera camera)
    {
        float stickRange = 200;

        Vector3 position = pixelCordinates - (Vector2)stick.rectTransform.position;

        if (position.magnitude > stickRange * 2)
            position = Vector3.zero;


        stickCircle.rectTransform.position = stick.rectTransform.position + position.normalized * Mathf.Min(position.magnitude, stickRange);

        Debug.DrawRay(stick.rectTransform.position, position, Color.red, Time.fixedDeltaTime);

        return position.normalized * Mathf.Min(position.magnitude, stickRange) / stickRange;
    }



    public void SwitchingToMelee()
    {
        Button_Melee.interactable = false;
        Button_Ranged.interactable = true;

        if (player.weapons[1].isUnlocked)

            player.currentWeapon = player.weapons[1];
        else
            player.currentWeapon = player.weapons[0];

        player.currentWeapon.cooldown = 0.25f;
    }

    public void SwitchingToFireArm()
    {
        if (!player.weapons[2].isUnlocked)
            return;

        Button_Melee.interactable = true;
        Button_Ranged.interactable = false;

        player.currentWeapon = player.weapons[2];
        player.currentWeapon.cooldown = 0.25f;
    }

    public void OnSelectingBlueprint()
    {
        Button_Blueprint_Close.gameObject.SetActive(true);
        Button_Blueprint.gameObject.SetActive(false);

        isPaused = true;

    }
    public void OnClosingBlueprint()
    {
        Button_Blueprint_Close.gameObject.SetActive(false);
        Button_Blueprint.gameObject.SetActive(true);

        isPaused = false;
    }
}
