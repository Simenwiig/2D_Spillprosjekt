using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class PlayerController : MonoBehaviour
{
				#region Enums & Classes
				public enum ControlType
    {Mouse, TouchScreen }

    [System.Serializable]
    public class WeaponStat
    {
        public string name = "unnamed";
        public bool isUnlocked;
        public int damage = 34;
        public float knockBack = 1;
        public float attacksPerSecond = 1;
        public float range = 10;
        public bool visibleBullet;
        public AudioClip sound;

        [HideInInspector]
        public float cooldown;
    }

    public class Weapon_Bullet
    {
        public Vector2 position;
        public Vector2 direction;

        public float speed;
        
        float lifeTime;

        public bool isDead { get { return lifeTime < 0; } }

        public Weapon_Bullet (Vector2 bulletPosition, Vector2 fireDirection, float projectileSpeed, float distance)
        {
            if (distance == 0)
                distance = 99;

            position = bulletPosition;
            direction = fireDirection;

            speed = projectileSpeed;
            lifeTime = distance / projectileSpeed;
        }

        public Vector3 UpdateBullet(float timeStep)
        {
            position += direction * speed * timeStep;
            lifeTime -= timeStep;

            if (isDead)
                position = Vector3.one * 99999;

            return position;
        }
    }
				#endregion

				AudioSource audioSource;
    Collider2D collider;
    Rigidbody2D rigid;
 
    Animator animator;
    Camera camera;
    SpriteRenderer spriteRenderer;

    Manager_UI manager_UI;



				[Header("Settings")]
    public float moveSpeed = 2f;
    public float friction = 1f;
    public float sprintSpeedModifier = 2f;
    [HideInInspector] public float damageTimer;
    public ControlType controlType = ControlType.Mouse;
    public float deadZone = 0.10f;
    public bool isInCutscene;
    public bool isPaused { get { return manager_UI.isPaused; } }

    [Header("Attributes")]
    public float healthLevel = 100;
    public float hungerLevel = 100;
    public float thirstLevel = 100;
    public float smellLevel;
    public float speedLevel { get { return velocity.magnitude; } }
    public Vector3 velocity;
    public Vector3 truePosition;

    [Header("Inventory")]
    public WeaponStat[] weapons;
    [HideInInspector] public WeaponStat currentWeapon;
    public Transform bullet;
    List<Weapon_Bullet> bullets = new List<Weapon_Bullet>();

    [Header("Audio Clips")]
    public AudioClip[] Footsteps;
    public AudioClip[] Hurt;
    public AudioClip[] Death;

    public bool isDead { get { return healthLevel <= 0; } }
    float footStepCooldown;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider2D>();
        rigid = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();
        camera = GetComponentInChildren<Camera>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        manager_UI = GameObject.Find("_Canvas").GetComponent<Manager_UI>();

        transform.tag = "Player";
        currentWeapon = weapons[0];
        bullet.parent = null;

        if (!Application.isEditor)
        {
            controlType = ControlType.TouchScreen;
            Input.multiTouchEnabled = true;
        }
    }

    void FixedUpdate()
    {
        animator.speed = (isPaused && !isFalling) ? 0 : 1;

        if (isInCutscene || isPaused || isFalling)
        {
            if (isFalling)
                Falling(false);

            return;
        }

        Vector2 leftStick = Vector2.zero;
        Vector2 rightStick = Vector2.zero;

        Manager_UI.StickReset(manager_UI.RightStick, manager_UI.RightStick_Dot);
        Manager_UI.StickReset(manager_UI.LeftStick, manager_UI.LeftStick_Dot);

								#region User Inputs
								if (controlType == ControlType.Mouse)
        {
            rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, Input.mousePosition, camera);

            leftStick.y = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
            leftStick.x = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);
            leftStick.Normalize();
        }
        else if (controlType == ControlType.TouchScreen)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                bool rightFinger = camera.ScreenToWorldPoint(Input.GetTouch(i).position).x > camera.transform.position.x;

                if(rightFinger)
                    rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, Input.GetTouch(i).position, camera);
                else
                    leftStick = Manager_UI.StickController(manager_UI.LeftStick, manager_UI.LeftStick_Dot, Input.GetTouch(i).position, camera);
            }

            if (Application.isEditor)
            {
                rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, Input.mousePosition, camera);
                leftStick = Manager_UI.StickController(manager_UI.LeftStick, manager_UI.LeftStick_Dot, Input.mousePosition, camera);
            }
        }
								#endregion

        collider.enabled = false;

        Walk(leftStick);
        Aim(rightStick);
        Animations(leftStick, rightStick);
        Resources();

        collider.enabled = true;

        GetComponent<Rigidbody2D>().MovePosition ((Vector2)transform.position +  (Vector2)velocity * Time.fixedDeltaTime);
        damageTimer -= Time.fixedDeltaTime;

     

        for (int i = 0; i < bullets.Count; i++)
        {
            bullet.position = bullets[i].UpdateBullet(Time.fixedDeltaTime);

            if (bullets[i].isDead)
                bullets.RemoveAt(i);
        }


        if (Input.GetKey(KeyCode.Space))
            Falling(true);
    }

    void Walk(Vector2 moveDirection)
    {
        if (moveDirection.magnitude < deadZone)
            moveDirection = Vector2.zero;

        float frictionStep = friction * Time.fixedDeltaTime;
        velocity -= velocity * frictionStep;
        if(!isDead)
            velocity += (Vector3)moveDirection * moveSpeed * frictionStep;

        float speed = velocity.magnitude;

        footStepCooldown -= Time.fixedDeltaTime;

        if (speed > 0.1f && footStepCooldown < 0)
        {
            footStepCooldown = 0.5f;

            PlayAudioClipFromArray(Footsteps, audioSource);
        }
    }

    void Aim(Vector2 attackDiretion)
    {
        Vector3 playerPosition = transform.position - Vector3.up * transform.GetChild(0).localPosition.y / 2;
        bool isAttacking = attackDiretion.magnitude > deadZone;
        currentWeapon.cooldown += Time.fixedDeltaTime;

        if (isAttacking && currentWeapon.cooldown > (1f / currentWeapon.attacksPerSecond))
        {
            currentWeapon.cooldown = 0;

            audioSource.PlayOneShot(currentWeapon.sound);

            Debug.DrawRay(playerPosition, attackDiretion.normalized * currentWeapon.range, Color.yellow, 0.1f);

            RaycastHit2D hit = Physics2D.Raycast(playerPosition, attackDiretion.normalized, currentWeapon.range);
            if (hit.transform != null && hit.transform.tag == "GameController")
                hit.transform.GetComponent<ZombieController>().HurtZombie(currentWeapon.damage, attackDiretion.normalized * currentWeapon.knockBack);

            if (currentWeapon.visibleBullet)
                bullets.Add(new Weapon_Bullet(playerPosition, attackDiretion.normalized, 50, hit.distance));
        }
    }

    void Resources()
    {
        bool isWalking = speedLevel > 0;
        float drainRate = 1 * (isWalking ? 1 : 0.5f) * Time.fixedDeltaTime;
        float healthRegenRate = 10f  * (isWalking ? 0.5f : 1) * Time.fixedDeltaTime;

        hungerLevel -= hungerLevel > 0 ? drainRate : 0;
        thirstLevel -= thirstLevel > 0 ? drainRate : 0;
        healthLevel -= (hungerLevel > 0 ? 0 : drainRate) + (thirstLevel > 0 ? 0 : drainRate);

        if (damageTimer < -5 && hungerLevel > 0 && thirstLevel > 0 && healthLevel < 100)
        {
            healthLevel += healthRegenRate;
            hungerLevel -= healthRegenRate;
        }
    }

    void Animations(Vector2 moveDirection, Vector2 attackDiretion)
    {
        #region Damage Blink
        float redBlink = 1 - damageTimer * 2;
        spriteRenderer.color = damageTimer > Time.fixedDeltaTime ? new Color(1, redBlink, redBlink) : Color.white;
								#endregion

								if (animator == null)
        {
            Debug.LogError("There is no Animator attached to this object.");
            return;
        }

        animator.SetBool("isTrulyWalking", moveDirection.magnitude > deadZone);

        bool isAttacking = attackDiretion.magnitude > deadZone;

        animator.SetBool("isShootingPistol", isAttacking && currentWeapon.name == "Pistol");
        animator.SetBool("isSwingingCrowbar", isAttacking && currentWeapon.name == "Crowbar");
        animator.SetBool("isSwingingUnarmed", isAttacking && currentWeapon.name == "Unarmed");

        if (isAttacking)
            moveDirection = attackDiretion;

        bool isMoving = moveDirection.magnitude > deadZone;
        bool isMovingVertically = Mathf.Abs(moveDirection.y) >= Mathf.Abs(moveDirection.x);

        animator.SetBool("isWalkingUp", isMoving && isMovingVertically && moveDirection.y > 0);
        animator.SetBool("isWalkingDown", isMoving && isMovingVertically && moveDirection.y < 0);
        animator.SetBool("isWalkingSideways", isMoving && !isMovingVertically);

        if (isMoving)
            spriteRenderer.flipX = moveDirection.x < 0;
    }

    public bool HurtPlayer(int damage, Vector3 knockBack = default(Vector3))
    {
        if (isDead || damageTimer > 0)
            return false;

        damageTimer = 0.5f; // The brief invulnerability you get when hit.
        healthLevel -= damage;

        if (isDead)
        {
            damageTimer = 0;
            manager_UI.isPaused = true;
            healthLevel = 0;
        }

        PlayAudioClipFromArray((isDead && Death.Length != 0) ? Death : Hurt, audioSource);

        velocity = knockBack;

       

        Debug.Log("SLAP! A Zombie hit the player.");

        return true;
    }

    static public void PlayAudioClipFromArray(AudioClip[] audioArray, AudioSource audioSource)
    {
        if (audioArray.Length == 0)
            return;

        int index = Random.Range(0, audioArray.Length);

        audioSource.PlayOneShot(audioArray[index]);
    }

    float fallingDuration = -1;
    bool isFalling { get { return fallingDuration != -1; } }
    public void Falling(bool onFalling)
    {
        fallingDuration -= Time.fixedDeltaTime;

        

        if (onFalling)
        {
            manager_UI.isPaused = true;
            fallingDuration = 2f;

            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isWalkingSideways", false);

            animator.SetBool("isFalling", true);
        }


        if (isFalling && fallingDuration < 0)
        {
            fallingDuration = -1;
            manager_UI.isPaused = false;
            animator.SetBool("isFalling", false);
        }

    }
}
