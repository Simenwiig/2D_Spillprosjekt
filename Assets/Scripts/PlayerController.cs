using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    AudioSource audioSource;
    Animator animator;

    float temp_Guncooldown;

    public bool isDead { get { return healthLevel <= 0; } }

				[Header("Settings")]
    public float moveSpeed = 2f;
    public float sprintSpeedModifier = 2f;
    public float damageCooldown = 1f;
    float damageTimer;

    [Header("Attributes")]
    public float healthLevel = 100;
    public float hungerLevel = 100;
    public float thirstLevel = 100;
    public float smellLevel;
    public float speedLevel { get { return velocity.magnitude; } }
    public Vector3 velocity;
    public Vector3 truePosition;

    [Header("Melee Weapon")]
    public float Melee_Damage = 55;
    public float Melee_AttackSpeed = 1;
    public float Melee_Reach = 0.5f;



    [Header("Ranged Weapon")]
    public float Ranged_Damage = 55;
    public float Ranged_AttackSpeed = 1;
    public AudioClip Sound_Gunshot;

    [Header("Inventory")]
    public bool haveCrowbar = false;

    void Awake()
    {
        transform.tag = "Player";
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        damageTimer -= Time.fixedDeltaTime;

        Walk();

        Aim();

        Resources();
       
        transform.position += velocity * Time.fixedDeltaTime;
    }

    void Walk()
    {
        int vertical = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        int horizontal = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);

        velocity = (Vector2.up * vertical + Vector2.right * horizontal).normalized * moveSpeed;

        if(animator != null)
								{
            animator.SetBool("isWalkingUp", vertical > 0);
            animator.SetBool("isWalkingDown", vertical < 0);

            animator.SetBool("isWalkingSideways", horizontal != 0);

            if(horizontal != 0)
                GetComponent<SpriteRenderer>().flipX = horizontal < 0;
        }
    }

    void Aim()
    {
        int vertical = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);
        int horizontal = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) + (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0);

        Vector2 aimDirection = (Vector2.up * vertical + Vector2.right * horizontal).normalized;
        bool isAttacking = aimDirection.magnitude > 0;

        temp_Guncooldown -= Time.deltaTime;

        if (isAttacking && temp_Guncooldown < 0)
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

        float drainRate = 1f/20 * Time.fixedDeltaTime * (isWalking ? 2 : 1);
        float healthRegenRate = 1f/20 * Time.fixedDeltaTime * (isWalking ? 0.66f : 1);

        if (hungerLevel > 0)
            hungerLevel -= drainRate;
        else
            healthLevel -= drainRate;

        if (thirstLevel > 0)
            thirstLevel -= drainRate * 1.33f;
        else
            healthLevel -= drainRate * 1.33f;

        if (damageTimer < 0 && hungerLevel > 0 && thirstLevel > 0 && healthLevel < 100)
        {
            healthLevel += healthRegenRate;
            hungerLevel -= healthRegenRate;
        }
    }

    public void HurtPlayer(int damage, Vector3 knockBack = default(Vector3))
    {
        if (isDead || damageTimer > 0)
            return;

        damageTimer = damageCooldown;
        healthLevel -= damage;

        if (isDead)
        {
            damageTimer = 99999999;
        }

        velocity += knockBack;

        Debug.Log("Bonk");

    }
}
