using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        sequenceProgress += Time.fixedDeltaTime;
        if (onActivation)
        {
            player.moveSpeed = 0;
            playerCamera.transform.parent = transform;

            helicopter.Play();

            player.damageTimer = 999999999;
            player.isInCutscene = true;

            audioSource.PlayOneShot(endTheme);

            manager_UI.isPaused = true;
        }

        playerCamera.transform.localPosition += Vector3.up * Time.fixedDeltaTime * cameraPanSpeed;

        if (sequenceProgress > 2.5f)
            player.GetComponentInChildren<SpriteRenderer>().enabled = false;

       manager_UI.UI_Screen_GameOver.color = new Color(0, 0, 0, (sequenceProgress / helicopter.clip.length));


        if (sequenceProgress > helicopter.clip.length)
        {
            manager_UI.UI_Victory.rectTransform.position += new Vector3(0, Time.fixedDeltaTime * endCreditFlowSpeed);
        }

        if (sequenceProgress > helicopter.clip.length + endCreditDuration)
            manager_UI.GameOver();
    }
}
