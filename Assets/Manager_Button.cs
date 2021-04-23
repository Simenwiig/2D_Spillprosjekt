using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager_Button : MonoBehaviour
{
    [Header("Start Menu")]

    public Button StartMenu;
    public Button Exit;

    [Header("In Game")]
    public Button Restart;

    void Awake()
    {
        StartMenu.onClick.RemoveAllListeners();
        StartMenu.onClick.AddListener(StartGame);


        Exit.onClick.RemoveAllListeners();
        Exit.onClick.AddListener(Application.Quit);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif

        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Hub_prototype");

    }
}
