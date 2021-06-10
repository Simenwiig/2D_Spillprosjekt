using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager_Button : MonoBehaviour
{
 

    [Header("Start Menu")]

    public Button StartMenu;
    public Button StartCredits;
    public Button Exit;

    [Header("In Game")]
    public Button Restart;

    [Header("Credit Settings")]

    public Image UI_CreditReel;
    public Image UI_ScreenTint;
    public Button UI_ScreenTint_Button;

    public int credit_BootLength = 1;
    public int creditDuration = 28;

    public int creditFlowSpeed = 10;

    public float creditProgress = -1;
    float creditProgress_Modifier = 1;

    void Awake()
    {
        StartMenu.onClick.RemoveAllListeners();
        StartMenu.onClick.AddListener(StartGame);

        StartCredits.onClick.RemoveAllListeners();
        StartCredits.onClick.AddListener(RollCredits);

        Exit.onClick.RemoveAllListeners();
        Exit.onClick.AddListener(Application.Quit);

        UI_ScreenTint.gameObject.SetActive(false);

        creditProgress = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (creditProgress != -1)
        {
            if (creditProgress < credit_BootLength)
           {
                float fadeProgress = creditProgress / credit_BootLength;

                UI_ScreenTint.color = new Color(0, 0, 0, fadeProgress);
            }
           else
                UI_CreditReel.rectTransform.position += new Vector3(0, creditFlowSpeed * creditProgress_Modifier) * Time.deltaTime;


            if (creditProgress > creditDuration + credit_BootLength)
            {
                float fadeProgress = 1 - (creditProgress - creditDuration - credit_BootLength) / credit_BootLength;

                UI_ScreenTint.color = new Color(0, 0, 0, fadeProgress);
            }




            creditProgress += Time.deltaTime * creditProgress_Modifier;


            if (creditProgress > creditDuration + credit_BootLength * 2)
            {
                creditProgress = -1;
   
                UI_ScreenTint.gameObject.SetActive(false);
                UI_CreditReel.rectTransform.anchoredPosition = new Vector3(0,0);
            }
        }
       
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Hub_prototype");

    }

    public void RollCredits()
    {

        creditProgress = 0;

        UI_ScreenTint.gameObject.SetActive(true);

        UI_ScreenTint_Button.onClick.RemoveAllListeners();
        UI_ScreenTint_Button.onClick.AddListener(SpeedUpCredits);

    }

    public void SpeedUpCredits()
    {
        if (creditProgress_Modifier == 1)
            creditProgress_Modifier = 5;
        else
            creditProgress_Modifier = 1;
    }
}
