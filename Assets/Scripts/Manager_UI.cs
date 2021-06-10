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


    [Header("The Options Menu")]
    public Button Options_ExitGame;

    public Button Options_Sound;
    public GameObject Options_Sound_Tab;
    public Slider Options_Sound_MainVolume;
    public Slider Options_Sound_MusicVolume;
    public Slider Options_Sound_SFXVolume;
    public Slider Options_Sound_ZombieVolume;

    public Button Options_Controls;
    public GameObject Options_Controls_Tab;
    public Slider Options_Controls_DeadZone;
    public Slider Options_Controls_TwinstickX;
    public Slider Options_Controls_TwinstickY;

    public Button Options_Gameplay;
    public GameObject Options_Gameplay_Tab;
    public Slider Options_Gameplay_Difficulty;

    AudioManager audioManager;


    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        gameOverFadeTimer = gameOverFadeDuration;

        SwitchingToMelee();

        Button_Melee.onClick.AddListener(SwitchingToMelee);
        Button_Ranged.onClick.AddListener(SwitchingToFireArm);

        Button_Blueprint.onClick.AddListener(OnSelectingBlueprint);
        Button_Unpause.onClick.AddListener(OnUnpausing);

        Button_Settings.onClick.AddListener(OpenSettings);

        UI_Options.GetComponentInChildren<Button>().onClick.AddListener(OnUnpausing);

        Options_ExitGame.onClick.AddListener(CloseGame);

        Options_Sound.onClick.AddListener(Option_Click_Sound);
        Options_Controls.onClick.AddListener(Option_Click_Controls);
        Options_Gameplay.onClick.AddListener(Option_Click_Gameplay);

        Options_ChangeTab(0);

        if (PlayerPrefs.GetInt("Difficulty") == 0) //  Reset settings to default
            Options_FactorySettings();
        

								{ // Assigning bars correctly.

            Options_Sound_MainVolume.value = PlayerPrefs.GetFloat("MainVolume");
            Options_Sound_MusicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
            Options_Sound_SFXVolume.value = PlayerPrefs.GetFloat("SFXVolume");
            Options_Sound_ZombieVolume.value = PlayerPrefs.GetFloat("ZombieVolume");

            Options_Controls_DeadZone.value = PlayerPrefs.GetFloat("DeadZone");
            Options_Controls_TwinstickX.value = PlayerPrefs.GetInt("TwinStick_X");
            Options_Controls_TwinstickY.value = PlayerPrefs.GetInt("TwinStick_Y");

            Options_Gameplay_Difficulty.value = PlayerPrefs.GetInt("Difficulty");

            Options_Volume_MenuFeedback();
            Options_Controls_MenuFeedback();
            Options_Gameplay_MenuFeedback();
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Input.GetKey(KeyCode.Space))
            Options_FactorySettings();

        UI_Bar_Health.transform.localScale = new Vector3(player.healthLevel / 100, 1, 1);
        UI_Bar_Hunger.transform.localScale = new Vector3(player.hungerLevel / 100, 1, 1);

        if (gameIsOver || player.isDead)
            GameOver();

        if (UI_Options.gameObject.activeInHierarchy && options_index == 0)
            Options_Volume_MenuFeedback();

        if (UI_Options.gameObject.activeInHierarchy && options_index == 1)
            Options_Controls_MenuFeedback();

        if (UI_Options.gameObject.activeInHierarchy && options_index == 2)
            Options_Gameplay_MenuFeedback();
    }

				#region Inputs
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

				#endregion

				#region UI Buttons
				public void SwitchingToMelee()
    {
        if (isPaused)
            return;

        Button_Melee.interactable = false;
        Button_Melee.transform.GetChild(0).gameObject.SetActive(true);
        Button_Ranged.interactable = true;
        Button_Ranged.transform.GetChild(0).gameObject.SetActive(false);

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
        Button_Melee.transform.GetChild(0).gameObject.SetActive(false);
        Button_Ranged.interactable = false;
        Button_Ranged.transform.GetChild(0).gameObject.SetActive(true);

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



    #endregion

    #region Misc. & Tools
    public void GameOver(bool playerWon = false)
    {
        if (gameOverFadeTimer < 0 || playerWon)
        {
            SceneManager.LoadScene("StartMenu");
            return;
        }
        gameIsOver = true;

        gameOverFadeTimer -= Time.fixedDeltaTime;

        UI_Screen_GameOver.color = new Color(0, 0, 0, 1f - (gameOverFadeTimer / gameOverFadeDuration));

        //  UI_Screen_GameOver.CrossFadeColor(new Color(1,1,1,255), gameOverFadeDuration, false, true);
    }

    public static Manager_UI GetManager()
    {
        return GameObject.Find("_Canvas").GetComponent<Manager_UI>();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    #endregion

    #region Options Menu

    public void OpenSettings()
    {
        isPaused = true;

        Button_Unpause.gameObject.SetActive(true);
        UI_Options.gameObject.SetActive(true);

        Button_Settings.gameObject.SetActive(false);
    }

    void Option_Click_Sound()
    {
        Options_ChangeTab(0);
    }

    void Option_Click_Controls()
    {
        Options_ChangeTab(1);
    }

    void Option_Click_Gameplay()
    {
        Options_ChangeTab(2);
    }

    int options_index = 0;

    public void Options_ChangeTab(int index)
    {
        if (options_index == index)
            return;

        options_index = index;

        Options_Sound.interactable = index != 0;
        Options_Sound_Tab.SetActive(index == 0);

        Options_Controls.interactable = index != 1;
        Options_Controls_Tab.SetActive(index == 1);

        Options_Gameplay.interactable = index != 2;
        Options_Gameplay_Tab.SetActive(index == 2);
    }

    public void Options_Volume_MenuFeedback()
    {
        PlayerPrefs.SetFloat("MainVolume", Options_Sound_MainVolume.value);
        PlayerPrefs.SetFloat("MusicVolume", Options_Sound_MusicVolume.value);
        PlayerPrefs.SetFloat("SFXVolume", Options_Sound_SFXVolume.value);
        PlayerPrefs.SetFloat("ZombieVolume", Options_Sound_ZombieVolume.value);

        audioManager.BGM.volume = Options_Sound_MainVolume.value * Options_Sound_MusicVolume.value;
    }
    public void Options_Controls_MenuFeedback()
    {
        PlayerPrefs.SetFloat("DeadZone", Options_Controls_DeadZone.value);

        PlayerPrefs.SetInt("TwinStick_X", (int)Options_Controls_TwinstickX.value);
        PlayerPrefs.SetInt("TwinStick_Y", (int)Options_Controls_TwinstickY.value);

        LeftStick.rectTransform.anchoredPosition = new Vector3((int)Options_Controls_TwinstickX.value, (int)Options_Controls_TwinstickY.value);
        RightStick.rectTransform.anchoredPosition = new Vector3(-(int)Options_Controls_TwinstickX.value, (int)Options_Controls_TwinstickY.value);
    }

    public void Options_Gameplay_MenuFeedback()
    {
        int value = (int)Options_Gameplay_Difficulty.value;

        if (value != (int)player.currentDifficulty)
        {
            Vector3 skullScale = Vector3.one * value;
            Image skullImage = Options_Gameplay_Tab.GetComponentInChildren<Image>();
            skullImage.rectTransform.localScale = skullScale;
            skullImage.color = value == 3 ? Color.red : Color.white;

            player.currentDifficulty = (PlayerController.DifficultyOptions)value;
            PlayerPrefs.SetInt("Difficulty", value);
        }
    }


    public void Options_FactorySettings()
    {
        PlayerPrefs.SetFloat("MainVolume", 0.5f);
        PlayerPrefs.SetFloat("MusicVolume", 1f);
        PlayerPrefs.SetFloat("SFXVolume", 1f);
        PlayerPrefs.SetFloat("ZombieVolume", 0.5f);

        PlayerPrefs.SetFloat("DeadZone", 0.10f);
        PlayerPrefs.SetInt("TwinStick_X", (int)LeftStick.rectTransform.position.x);
        PlayerPrefs.SetInt("TwinStick_Y", (int)LeftStick.rectTransform.position.y);

        PlayerPrefs.SetInt("Difficulty", (int)player.currentDifficulty); // The default difficulty set in the inspector


        { // Assigning bars correctly.

            Options_Sound_MainVolume.value = PlayerPrefs.GetFloat("MainVolume");
            Options_Sound_MusicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
            Options_Sound_SFXVolume.value = PlayerPrefs.GetFloat("SFXVolume");
            Options_Sound_ZombieVolume.value = PlayerPrefs.GetFloat("ZombieVolume");

            Options_Controls_DeadZone.value = PlayerPrefs.GetFloat("DeadZone");
            Options_Controls_TwinstickX.value = PlayerPrefs.GetInt("TwinStick_X");
            Options_Controls_TwinstickY.value = PlayerPrefs.GetInt("TwinStick_Y");

            Options_Gameplay_Difficulty.value = PlayerPrefs.GetInt("Difficulty");

            Options_Volume_MenuFeedback();
            Options_Controls_MenuFeedback();
            Options_Gameplay_MenuFeedback();
        }
    }
				#endregion
}
