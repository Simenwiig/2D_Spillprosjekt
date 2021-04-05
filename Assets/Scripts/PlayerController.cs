using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{


    [Header("Settings")]

    public float moveSpeed = 2f;
    public float sprintSpeedModifier = 2f;
    public bool usePixelPerfect = true;

    [Header("Attributes")]

    public float healthLevel;
    public float hungerLevel;
    public float thirstLevel;
    public float smellLevel;
    public float speedLevel { get { return velocity.magnitude; } }
    public Vector3 velocity;
    public Vector3 truePosition;

    float temp_Guncooldown;

    public AudioClip Sound_Gunshot;
    AudioSource audioSource;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        transform.tag = "Player";
        audioSource = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {


    }

    void FixedUpdate()
    {
        Walk();

        Aim();

        Resources();
        if (usePixelPerfect)
        {
            truePosition += velocity * Time.fixedDeltaTime;

            Vector2 pixelPosition = Vector2.zero;

            float ratio = 1 / 32;

            pixelPosition.x = Mathf.Round(truePosition.x * 32) / 32;
            pixelPosition.y = Mathf.Round(truePosition.y * 32) / 32;

            transform.position = pixelPosition;
        }
        else
            transform.position += velocity * Time.fixedDeltaTime;
    }

    void Walk()
    {
        int vertical = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        int horizontal = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);

        velocity = (Vector2.up * vertical + Vector2.right * horizontal).normalized * moveSpeed;
    }

  

    void Aim()
    {
        int vertical = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);
        int horizontal = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) + (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0);

  

        Vector2 aimDirection = (Vector2.up * vertical + Vector2.right * horizontal).normalized;

        bool pullTrigger = aimDirection.magnitude > 0;

        temp_Guncooldown -= Time.deltaTime;

        if (pullTrigger && temp_Guncooldown < 0)
        {
            temp_Guncooldown = 0.4f;

            audioSource.PlayOneShot(Sound_Gunshot);

            Debug.DrawRay(transform.position - Vector3.forward, aimDirection * 99, Color.yellow, 0.1f);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, 99);

            if (hit.transform != null && hit.transform.tag == "GameController")
            {
                hit.transform.GetComponent<ZombieController>().OnDeath();
            }
        }

    }

    void Resources()
    {
        bool isWalking = speedLevel > 0;
        bool isRunning = speedLevel > moveSpeed;

        

    }

}
