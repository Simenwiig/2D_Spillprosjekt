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

    public Image UI_Screen_GameOver;
    public Image UI_Victory;
    public Image UI_Blueprints;
    public Image UI_Options;

    [Header("Interface Elements")]
    public Image LeftStick;
    public Image LeftStick_Dot;
    public Image RightStick;
    public Image RightStick_Dot;
    public Button Button_Melee;
    public Button Button_Ranged;
    public Button Button_Blueprint;
    public Button Button_Unpause;
    public Button Button_Settings;

    public Image[] Tutorial_Elements;




    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        gameOverFadeTimer = gameOverFadeDuration;


        Button_Melee.onClick.AddListener(SwitchingToMelee);
        Button_Ranged.onClick.AddListener(SwitchingToFireArm);

        Button_Blueprint.onClick.AddListener(OnSelectingBlueprint);
        Button_Unpause.onClick.AddListener(OnUnpausing);

       Button_Settings.onClick.AddListener(OpenSettings);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UI_Bar_Health.transform.localScale = new Vector3(player.healthLevel / 100, 1, 1);
        UI_Bar_Hunger.transform.localScale = new Vector3(player.hungerLevel / 100, 1, 1);

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

    static float stickRange = 160;

    /// returns the relative direction from the controlPad to pixelCordinates
    public static Vector3 StickController(Image stick, Image stickCircle, Vector2 pixelCordinates, Camera camera)
    {
        Vector3 position = pixelCordinates - (Vector2)stick.rectTransform.position;

        stickCircle.rectTransform.position = stick.rectTransform.position + position.normalized * Mathf.Min(position.magnitude, stickRange);

        Debug.DrawRay(stick.rectTransform.position, position, Color.red, Time.fixedDeltaTime);

        return position.normalized * Mathf.Min(position.magnitude, stickRange) / stickRange;
    }

    public static void StickReset(Image stick, Image stickCircle)
    {
        stickCircle.rectTransform.position = stick.rectTransform.position;
    }

    public static bool IsInRangeOfStick(Image stick, Vector2 pixelCordinates, Camera camera)
    {
        Vector3 position = pixelCordinates - (Vector2)stick.rectTransform.position;

        return position.magnitude < stickRange;
    }



    public void SwitchingToMelee()
    {
        if (isPaused)
            return;

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
        if (isPaused || !player.weapons[2].isUnlocked)
            return;

        Button_Melee.interactable = true;
        Button_Ranged.interactable = false;

        player.currentWeapon = player.weapons[2];
        player.currentWeapon.cooldown = 0.25f;
    }

    public void OnSelectingBlueprint()
    {
        if (isPaused)
            return;

        Button_Unpause.gameObject.SetActive(true);
        UI_Blueprints.gameObject.SetActive(true);
        Button_Blueprint.gameObject.SetActive(false);

        isPaused = true;

    }
    public void OnUnpausing()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Button_Blueprint.gameObject.SetActive(true);
        Button_Settings.gameObject.SetActive(true);

        UI_Blueprints.gameObject.SetActive(false);
        Button_Unpause.gameObject.SetActive(false);
        UI_Options.gameObject.SetActive(false);

        for (int i = 0; i < Tutorial_Elements.Length; i++)
            Tutorial_Elements[i].gameObject.SetActive(false);   
    }

   

    public void ActivateTutorialElement(int index)
    {
        isPaused = true;
        for (int i = 0; i < Tutorial_Elements.Length; i++)
            Tutorial_Elements[i].gameObject.SetActive(false);

        Tutorial_Elements[index].gameObject.SetActive(true);

        Button_Unpause.gameObject.SetActive(true);
    }

    public void OpenSettings()
    {
        isPaused = true;

        Button_Unpause.gameObject.SetActive(true);
        UI_Options.gameObject.SetActive(true);

        Button_Settings.gameObject.SetActive(false);

        //Application.Quit();
    }

    public static Manager_UI GetManager()
    {
        return GameObject.Find("_Canvas").GetComponent<Manager_UI>();
    }
}
