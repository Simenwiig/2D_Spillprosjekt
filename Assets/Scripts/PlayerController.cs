using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum ControlType
    {Mouse, TouchScreen }

    [System.Serializable]
    public class WeaponStat
    {
        public string name = "unnamed";
        public bool canBeUsed;
        public int damage = 34;
        public float knockBack = 1;
        public float attacksPerSecond = 1;
        public float range = 10;
        public AudioClip sound;

        [HideInInspector]
        public float cooldown;
    }


    AudioSource audioSource;
    Animator animator;
    Camera camera;

    Manager_UI manager_UI;
    public bool isDead { get { return healthLevel <= 0; } }

				[Header("Settings")]
    public float moveSpeed = 2f;
    public float friction = 1f;
    public float sprintSpeedModifier = 2f;
    public float damageCooldown = 1f;
    float damageTimer;
    public ControlType controlType = ControlType.Mouse;
    public float deadZone = 0.10f;

    [Header("Attributes")]
    public float healthLevel = 100;
    public float hungerLevel = 100;
    public float thirstLevel = 100;
    public float smellLevel;
    public float speedLevel { get { return velocity.magnitude; } }
    public Vector3 velocity;
    public Vector3 truePosition;

    [Header("Inventory")]
    public bool haveCrowbar = false;
    public bool haveParkKey = false;
    public WeaponStat[] weapons;

    [Header("Audio Clips")]
    public AudioClip[] Footstep;
    public AudioClip[] MeleeAttack;
    public AudioClip[] Hurt;
    public AudioClip[] Death;

    void Awake()
    {
        transform.tag = "Player";
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        camera = GetComponentInChildren<Camera>();
        manager_UI = GameObject.Find("_Canvas").GetComponent<Manager_UI>();
    }

    void FixedUpdate()
    {
        Vector2 leftStick = Vector2.zero;
        Vector2 rightStick = Vector2.zero;
   
        if (controlType == ControlType.Mouse)
        {
            rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, Input.mousePosition, true, camera);
            // leftStick = Manager_UI.StickController(manager_UI.LeftStick, manager_UI.LeftStick_Dot, Input.mousePosition, true, camera);

            leftStick.y = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
            leftStick.x = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);
            leftStick.Normalize();
        }
        else if (controlType == ControlType.TouchScreen)
        {
            rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, Input.mousePosition, true, camera);
            leftStick = Manager_UI.StickController(manager_UI.LeftStick, manager_UI.LeftStick_Dot, Input.mousePosition, true, camera);
        }


        Walk(leftStick);

        Aim(rightStick);

        Animations(leftStick, rightStick);

        Resources();

        transform.position += velocity * Time.fixedDeltaTime;
        damageTimer -= Time.fixedDeltaTime;
    }

    void Walk(Vector2 moveDirection)
    {
        if (moveDirection.magnitude < deadZone)
            moveDirection = Vector2.zero;

        float frictionStep = friction * Time.fixedDeltaTime;
        velocity -= velocity * frictionStep;
        velocity += (Vector3)moveDirection * moveSpeed * frictionStep;
    }

    void Aim(Vector2 attackDiretion)
    {
        bool isAttacking = attackDiretion.magnitude > deadZone;

        WeaponStat currentWeapon = weapons[2];

        currentWeapon.cooldown += Time.fixedDeltaTime;

        if (isAttacking && currentWeapon.cooldown > (1f / currentWeapon.attacksPerSecond))
        {
            currentWeapon.cooldown = 0;

            audioSource.PlayOneShot(currentWeapon.sound);

            Debug.DrawRay(transform.position - Vector3.forward, attackDiretion.normalized * currentWeapon.range, Color.yellow, 0.1f);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, attackDiretion.normalized, currentWeapon.range);
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

    void Animations(Vector2 moveDirection, Vector2 attackDiretion)
    {
        if (animator == null)
        {
            Debug.LogError("There is no Animator attached to this object.");
            return;
        }

        bool isAttacking = attackDiretion.magnitude > deadZone;

        animator.SetBool("isShootingPistol", isAttacking && true);
        animator.SetBool("isSwingingCrowbar", isAttacking && false);
        animator.SetBool("isSwingingUnarmed", isAttacking && false);

        if (isAttacking)
            moveDirection = attackDiretion;

      //  if (!isAttacking)
      //      return;  // If I stop attacking, I won't automatically transition back to the correct idle animation. This hack makes it registrer as not attacking before it registrer as not moving, fixing the problem.

        bool isMoving = moveDirection.magnitude > deadZone;
        bool isMovingVertically = Mathf.Abs(moveDirection.y) >= Mathf.Abs(moveDirection.x);

        animator.SetBool("isWalkingUp", isMoving && isMovingVertically && moveDirection.y > 0);
        animator.SetBool("isWalkingDown", isMoving && isMovingVertically && moveDirection.y < 0);
        animator.SetBool("isWalkingSideways", isMoving && !isMovingVertically);

        if (isMoving)
            GetComponentInChildren<SpriteRenderer>().flipX = moveDirection.x < 0;
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

        Debug.Log("SLAP! A Zombie hit the player.");
    }
}
