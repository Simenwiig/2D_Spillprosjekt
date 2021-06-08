using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Event_Victory : MonoBehaviour
{
    PlayerController player;
    Camera playerCamera;

    bool hasStarted;
    float sequenceProgress;
    Manager_UI manager_UI;

    [Header("Sequence Settings")]
    public float cameraPanSpeed = 1;
    public Animation helicopter;
    public float endCreditFlowSpeed = 100;
    public float endCreditDuration = 10;


    public AudioClip endTheme;

    AudioSource audioSource;

    public int creditSpeedModifier = 1;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerCamera = player.GetComponentInChildren<Camera>();

        manager_UI = GameObject.Find("_Canvas").GetComponent<Manager_UI>();

        audioSource = GetComponent<AudioSource>();
    }


    void FixedUpdate()
    {
        float playerDistance = (player.transform.position - transform.position).magnitude;


        if (hasStarted)
            Sequence(false);

        if (playerDistance < 10f && player.transform.position.y > transform.position.y && !player.isDead && !hasStarted)
        {
            transform.position = player.transform.position;
            hasStarted = true;
            Sequence(true);
        }
    }

    void Sequence(bool onActivation)
    {
        
        if (onActivation)
        {
            player.moveSpeed = 0;
            playerCamera.transform.parent = transform;

            helicopter.Play();

            player.damageTimer = 999999999;
            player.isInCutscene = true;

            audioSource.PlayOneShot(endTheme);

            manager_UI.isPaused = true;

            manager_UI.UI_Screen_GameOver.gameObject.SetActive(true);
            manager_UI.UI_Victory.gameObject.SetActive(true);

        }

        sequenceProgress += Time.fixedDeltaTime * creditSpeedModifier;
        playerCamera.transform.localPosition += Vector3.up * Time.fixedDeltaTime * cameraPanSpeed;
        manager_UI.UI_Screen_GameOver.color = new Color(0, 0, 0, (sequenceProgress / helicopter.clip.length));

        if (sequenceProgress > 1f)
        {
            manager_UI.UI_Screen_GameOver.GetComponent<Button>().onClick.RemoveAllListeners();
            manager_UI.UI_Screen_GameOver.GetComponent<Button>().onClick.AddListener(FastForwardToggle);
        }

        if (sequenceProgress > 2.3f)
            player.GetComponentInChildren<SpriteRenderer>().enabled = false;


            if (sequenceProgress > helicopter.clip.length)
        {
            manager_UI.UI_Victory.rectTransform.position += new Vector3(0, Time.fixedDeltaTime * endCreditFlowSpeed * creditSpeedModifier);
        }

      

        if (sequenceProgress > helicopter.clip.length + endCreditDuration)
            manager_UI.GameOver();
    }


    void FastForwardToggle()
    {
        if (creditSpeedModifier == 1)
            creditSpeedModifier = 5;
        else
            creditSpeedModifier = 1;

        helicopter["Helicopter_EndSequence"].speed = creditSpeedModifier;

        if (creditSpeedModifier == 1)
            audioSource.pitch = 1;
        else
            audioSource.pitch = 2f;
    }
}
