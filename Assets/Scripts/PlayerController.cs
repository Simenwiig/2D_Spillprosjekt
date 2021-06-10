using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class PlayerController : MonoBehaviour
{
    #region Classes

    public enum DifficultyOptions
    {
        ZoomerMode = 1, Medium = 2, Hard = 3,
    }


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
        public bool alertsZombies;
        public AudioClip sound;
        public float attackWalkspeedPenality = 1;

        [HideInInspector]
        public float cooldown;

        public bool readyToFire { get { return cooldown > (1f / attacksPerSecond); } }
        public bool useBackSwing;
    }

    public class Weapon_Bullet
    {
        public Transform transform;
        public Vector2 velocity;

        public bool doOrbit;
        static float orbitStrength = 5;

        float lifeTime;

        public bool isDead { get { return lifeTime < 0; } }

        public Weapon_Bullet (Vector2 bulletPosition, Vector2 fireDirection, float projectileSpeed, float distance, bool isMeleeSwipe = false)
        {
            if (distance == 0)
                distance = 99;

            distance = Mathf.Max(distance, 1.3f);


            transform = new GameObject().transform;
            transform.position = bulletPosition;
            transform.up = fireDirection;

            velocity += fireDirection * projectileSpeed;
            lifeTime = distance / Mathf.Abs(projectileSpeed); // If the projectile speed is negative, so is the lifetime.

            TrailRenderer trail = transform.gameObject.AddComponent<TrailRenderer>();
            trail.startWidth = 0.1f;
            trail.endWidth = 0;
            trail.minVertexDistance = 0;
            trail.material = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<SpriteRenderer>().material;

            if (!isMeleeSwipe)
            {
                trail.startColor = Color.yellow;
                trail.endColor = Color.white;
            }

            doOrbit = isMeleeSwipe;

            if (doOrbit)
                velocity += (Vector2)transform.right * orbitStrength;
        }

        public void UpdateBullet(float timeStep)
        {
            if (doOrbit)
                velocity -= (Vector2)transform.right * (orbitStrength / lifeTime) * timeStep;

            transform.position += (Vector3)velocity * timeStep;

            lifeTime -= timeStep;

            if (isDead)
                Destroy(transform.gameObject);
        }
    }
    #endregion

    Animator animator;
    AudioSource audioSource;
    Camera camera;
    Collider2D collider;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Manager_UI manager_UI;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float friction = 1f;
    public float sprintSpeedModifier = 2f;
    public float speedLevel { get { return Mathf.Round(velocity.magnitude* 10) / 10; } }
    public Vector2 velocity;

    [Header("Settings")]
    public DifficultyOptions currentDifficulty = DifficultyOptions.Medium;

    [HideInInspector] public float damageTimer;
    public bool isInCutscene;
    public bool isPaused { get { return manager_UI.isPaused; } }
    public bool isInGodmode;
  
    [Header("Resources")]
    public float healthLevel = 100;
    public float hungerLevel = 100;
    public float smellLevel;

    [Header("Inventory")]
    public WeaponStat[] weapons;
    [HideInInspector] public WeaponStat currentWeapon;
    List<Weapon_Bullet> bullets = new List<Weapon_Bullet>();

    [Header("Audio Clips")]
    public AudioClip[] Footsteps;
    public AudioClip[] Hurt;
    public AudioClip[] Death;

    [Header("Input")]
    public Vector2 rightStick;
    public Vector2 leftStick;

    public bool isDead { get { return healthLevel <= 0; } }
    float footStepCooldown;

    int thumb_Right = -1;
    int thumb_Left = -1;

    [HideInInspector]
    public bool playingOnPC = true;

    float fallingDuration = -1;
    public bool isFalling { get { return fallingDuration != -1; } }
    public bool isHalfDoneFalling { get { return fallingDuration < 0.8f; } }

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

        if (!Application.isEditor)
        {
            Input.multiTouchEnabled = true;
            playingOnPC = false;
        }
    }

				private void Start()
				{
        
				}

				void Update()
    {
        rightStick = Vector2.zero;
        leftStick = Vector2.zero;

								#region Pause, Cutscene & Falling
								animator.speed = (isPaused && !isFalling) ? 0 : 1;

        if (isInCutscene || isPaused || isFalling)
        {
            if (isFalling)
                Falling(false);

            return;
        }
								#endregion

        #region PC (Editor) controls
        if (playingOnPC)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Manager_UI.IsInRangeOfStick(manager_UI.RightStick, Input.mousePosition, camera))
                    thumb_Right = 1;

                if (Manager_UI.IsInRangeOfStick(manager_UI.LeftStick, Input.mousePosition, camera))
                    thumb_Left = 1;
            }

            if (thumb_Right == 1)
                rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, Input.mousePosition, camera);

            if (thumb_Left == 1)
                leftStick = Manager_UI.StickController(manager_UI.LeftStick, manager_UI.LeftStick_Dot, Input.mousePosition, camera);
            else
            {
                leftStick.y = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
                leftStick.x = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);
                leftStick.Normalize();
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                thumb_Right = -1;
                thumb_Left = -1;

                Manager_UI.StickReset(manager_UI.RightStick, manager_UI.RightStick_Dot);
                Manager_UI.StickReset(manager_UI.LeftStick, manager_UI.LeftStick_Dot);
            }
        }
        #endregion

        #region Phone controls
        if (!playingOnPC)
        {
            for (int i = 0; i < Input.touchCount; i++)  // The oldest touch is checked first. Touching a second finger at the same side will not changed the designated thumb.
            {
                Touch currentTouch = Input.GetTouch(i);

                if (thumb_Right == -1 && Manager_UI.IsInRangeOfStick(manager_UI.RightStick, currentTouch.position, camera))
                    thumb_Right = currentTouch.fingerId;

                if (thumb_Left == -1 && Manager_UI.IsInRangeOfStick(manager_UI.LeftStick, currentTouch.position, camera))
                    thumb_Left = currentTouch.fingerId;

                if(currentTouch.fingerId == thumb_Right)
																{
                    rightStick = Manager_UI.StickController(manager_UI.RightStick, manager_UI.RightStick_Dot, currentTouch.position, camera);

                    if (currentTouch.phase == TouchPhase.Ended)
                    {
                        thumb_Right = -1;
                        Manager_UI.StickReset(manager_UI.RightStick, manager_UI.RightStick_Dot);
                    }
                }

                if (currentTouch.fingerId == thumb_Left)
                {
                    leftStick = Manager_UI.StickController(manager_UI.LeftStick, manager_UI.LeftStick_Dot, currentTouch.position, camera);

                    if (currentTouch.phase == TouchPhase.Ended)
                    {
                        thumb_Left = -1;
                        Manager_UI.StickReset(manager_UI.LeftStick, manager_UI.LeftStick_Dot);
                    }
                }
            }
        }

        #endregion

        collider.enabled = false;

        Walk(leftStick);
        Aim(rightStick);
        Animations(leftStick, rightStick);
        Resources();

        collider.enabled = true;
        damageTimer -= Time.deltaTime;

        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i].UpdateBullet(Time.deltaTime);

            if (bullets[i].isDead)
                bullets.RemoveAt(i);
        }
    }

				private void FixedUpdate()
				{
        if (isInCutscene || isPaused || isFalling)
            return;

        rigid.MovePosition ((Vector2)transform.position + velocity * Time.fixedDeltaTime);
    }

    void Walk(Vector2 moveDirection)
    {
        if (moveDirection.magnitude < PlayerPrefs.GetFloat("DeadZone"))
            moveDirection = Vector2.zero;

        float attackingSpeedPenality = currentWeapon.readyToFire ? 1 : currentWeapon.attackWalkspeedPenality;
        float frictionStep = friction * Time.deltaTime;

        velocity -= velocity * frictionStep;
        if(!isDead)
            velocity += moveDirection * moveSpeed * attackingSpeedPenality * frictionStep;

        float speed = velocity.magnitude;

        footStepCooldown -= Time.deltaTime * (speed / moveSpeed);

        if (speed > moveSpeed / 2 && footStepCooldown < 0)
        {
            footStepCooldown = 0.833333333f / 2; // The footstep animation is 0.83 seconds long, and you take two steps during it.
            PlayAudioClipFromArray(Footsteps, audioSource, 0.2f);
        }
    }

    void Aim(Vector2 attackDiretion)
    {
        Vector3 playerPosition = transform.position - Vector3.up * transform.GetChild(0).localPosition.y / 2;
        bool isAttacking = attackDiretion.magnitude > PlayerPrefs.GetFloat("DeadZone");
        currentWeapon.cooldown += Time.deltaTime;

        if (isAttacking && currentWeapon.readyToFire)
        {
            currentWeapon.cooldown = 0;

            audioSource.PlayOneShot(currentWeapon.sound, PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume"));

            float reverseStartDistance = 0.1f;

            Debug.DrawRay(playerPosition - (Vector3)attackDiretion.normalized * reverseStartDistance, attackDiretion.normalized * currentWeapon.range, Color.yellow, 0.1f);

            RaycastHit2D hit = Physics2D.Raycast(playerPosition - (Vector3)attackDiretion.normalized * reverseStartDistance, attackDiretion.normalized, currentWeapon.range + reverseStartDistance);
            if (hit.transform != null && hit.transform.tag == "GameController")
                hit.transform.GetComponent<ZombieController>().HurtZombie(currentWeapon.damage, attackDiretion.normalized * currentWeapon.knockBack);

            if (hit.transform == null)
                hit.distance = 99f;

            if (currentWeapon.visibleBullet)
            {
                bullets.Add(new Weapon_Bullet(playerPosition, attackDiretion.normalized, 75f, hit.distance + 0.1f));

                if(hit.transform != null)
                    Debug.DrawLine(playerPosition, hit.point);
            }
            else
            {
                int swingDirection = currentWeapon.useBackSwing ? -1 : 1;
                Vector3 playerForward = (Vector3)attackDiretion.normalized * (currentWeapon.range - 0.2f);
                Vector3 playerSide = Vector3.Cross(transform.forward, attackDiretion.normalized);

                bullets.Add(new Weapon_Bullet(playerPosition + playerForward - (playerSide * swingDirection * 0.6f), playerSide, 10 * swingDirection, 1, true));

                currentWeapon.useBackSwing = !currentWeapon.useBackSwing;
            }
        }
        if (currentWeapon.readyToFire)
            currentWeapon.useBackSwing = false;
    }

    void Resources()
    {
        if (isInGodmode)
            return;

        bool isWalking = speedLevel > 0;

        float drainRate = 0.5f * (isWalking ? 1 : 0) * Time.deltaTime * (int)currentDifficulty;
        float healthRegenRate = 10f  * (isWalking ? 0.5f : 1) * Time.deltaTime;

        hungerLevel -= hungerLevel > 0 ? drainRate : 0;

        if(currentDifficulty != DifficultyOptions.ZoomerMode)
            healthLevel -= (hungerLevel > 0 ? 0 : drainRate);

        if (damageTimer < -5 && hungerLevel > 0 && healthLevel < 100)
        {
            healthLevel += healthRegenRate;
            hungerLevel -= healthRegenRate;
        }
    }

    void Animations(Vector2 moveDirection, Vector2 attackDiretion)
    {
        #region Damage Blink
        float redBlink = 1 - damageTimer * 2;
        spriteRenderer.color = damageTimer > Time.deltaTime ? new Color(1, redBlink, redBlink) : Color.white;
								#endregion

								if (animator == null)
        {
            Debug.LogError("There is no Animator attached to this object.");
            return;
        }

  
        bool isAttacking = attackDiretion.magnitude > PlayerPrefs.GetFloat("DeadZone");
        bool isMoving = moveDirection.magnitude > PlayerPrefs.GetFloat("DeadZone");

        animator.SetBool("isStandingStill", !isMoving);

        animator.SetBool("isShootingPistol", isAttacking && currentWeapon.name == "Pistol");
        animator.SetBool("isSwingingCrowbar", isAttacking && currentWeapon.name == "Crowbar");
        animator.SetBool("isSwingingUnarmed", isAttacking && currentWeapon.name == "Unarmed");

  
        if (!isMoving && !isAttacking && !currentWeapon.readyToFire)
        {
            return;
        }

        if (isAttacking)
        {
            isMoving = true;
            moveDirection = attackDiretion;
        }

        bool isMovingVertically = Mathf.Abs(moveDirection.y) >= Mathf.Abs(moveDirection.x);

        animator.SetBool("isWalkingUp", isMoving && isMovingVertically && moveDirection.y > 0);
        animator.SetBool("isWalkingDown", isMoving && isMovingVertically && moveDirection.y < 0);
        animator.SetBool("isWalkingSideways", isMoving && !isMovingVertically);

        if(isMoving || isAttacking) // If I am idle, this will never need to be updated.
            spriteRenderer.flipX = moveDirection.x < 0 && !isMovingVertically;
    }

    public bool HurtPlayer(int damage, Vector3 knockBack = default(Vector3))
    {
        if (isDead || damageTimer > 0)
            return false;

        damage = damage * (int)currentDifficulty;

        damageTimer = 1f; // The brief invulnerability you get when hit.
        healthLevel -= isInGodmode ? 0 : damage;

        if (isDead)
        {
            damageTimer = 0;
            manager_UI.isPaused = true;
            healthLevel = 0;
        }

        PlayAudioClipFromArray((isDead && Death.Length != 0) ? Death : Hurt, audioSource);
        velocity = knockBack;

        return true;
    }
    /// Returns the length of the clip.
    static public float PlayAudioClipFromArray(AudioClip[] audioArray, AudioSource audioSource, float volumeScale = 1)
    {
        if (audioArray.Length == 0)
            return 0;

        int index = Random.Range(0, audioArray.Length);

        audioSource.PlayOneShot(audioArray[index], PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume") * volumeScale);

        return audioArray[index].length;
    }

    public void Falling(bool onFalling)
    {
        fallingDuration -= Time.deltaTime;

        if (onFalling)
        {
            velocity = Vector2.zero;
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
            animator.SetBool("isStandingStill", true);

            animator.SetBool("isWalkingDown", true);

            animator.Update(Time.deltaTime);
        }
    }
}
