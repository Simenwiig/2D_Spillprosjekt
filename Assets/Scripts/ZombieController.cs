using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    public enum BehaviorState
    {
        Idle, Chasing, Searching
    }

    Animator animator;
    Vector3 moveDirection;
    Collider2D[] colliders;
    SpriteRenderer spriteRenderer;

    [Header("Settings")]
    public float moveSpeed = 0.25f;
    public float chaseSpeed = 5;
    public float friction = 15f;
    public int damage = 10;
    public float attackReach = 0.5f;
    public float detection_SightRadius = 10f;

    /// How long will the zombie investigate the player?
    public float trackLostChance = 5;

    [Header("Status")]
    public BehaviorState behaviorState = BehaviorState.Idle;
    public List<Vector3> validLocations = new List<Vector3>();
    Vector2 previousPlayerPosition;
    public float healthLevel;
    public Vector3 velocity;
    public LayerMask zombieLayer;

    [Header("Audio Clips")]
    public AudioClip[] Idle;
    public AudioClip[] Footstep;
    public AudioClip[] Attack;
    public AudioClip[] Hurt;
    public AudioClip[] Death;

    [Header("UI")]
    public SpriteRenderer HealthBar_Full;

    PlayerController player;
    AudioSource audioSource;
    
    bool isDead = false;
    float damageTimer;

    bool canSpawnMoreZombies = true;
    bool playerEnterRangeOnce= false;

    void Start()
    {
        transform.tag = "GameController";
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        colliders = GetComponents<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        HurtZombie(0, Vector3.zero); // Turns the healthbar invisible as I start with full health.
        damageTimer = 0; // Stops them from flashing from taking "damage".
    }


    void Update()
    {

								#region Pausing
								animator.speed = player.isPaused ? 0 : 1;

        if (player.isPaused)
            return;
        #endregion

        #region Damage Blink
        float redBlink = 1 - damageTimer * 2;
        spriteRenderer.color = damageTimer > Time.deltaTime ? new Color(1, redBlink, redBlink) : Color.white;
        #endregion

        if (player == null)
								{
            Debug.LogError("Advarsel: Zombiene kan ikke finne spilleren, n�r det ikke er noen spiller i Scenen.");
            return;
								}


        spriteRenderer.sortingOrder = transform.position.y > player.transform.position.y ? -1 : 1;

        MovePosition();

        transform.position += velocity * Time.deltaTime;

        damageTimer -= Time.deltaTime;

        if (isDead)
            return;

        ToggleColliders(false);

        Vector3 directionToPlayer = (player.transform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        ZombieBehvior();

        if (distanceToPlayer <= attackReach)
        {
            if (player.HurtPlayer(damage, directionToPlayer.normalized * 10))
            {
                PlayerController.PlayAudioClipFromArray(Attack, audioSource, PlayerPrefs.GetFloat("ZombieVolume"));
                
                velocity -= directionToPlayer.normalized * 2;
                damageTimer = 0.15f;

                Debug.DrawRay(transform.position, player.transform.position, Color.red);
            }
        }

        ToggleColliders(true);

        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
    }

				public void MoveTowardsLocation(Vector3 targetPosition)
    {
        moveDirection = (targetPosition - transform.position).normalized * moveSpeed;
    }

    public void MovePosition()
    {
        float difficultyBasedSpeedBoost = player.currentDifficulty == PlayerController.DifficultyOptions.ZoomerMode ? 1.00f : player.currentDifficulty == PlayerController.DifficultyOptions.Medium ? 1.20f : 1.5f;


        float frictionStep = friction * Time.deltaTime;

        velocity -= velocity * frictionStep;
        
        if(!isDead && damageTimer < 0)
          velocity += moveDirection.normalized * chaseSpeed * difficultyBasedSpeedBoost * frictionStep;

        if (animator != null && damageTimer < 0 && !isDead)
        {
            bool isMoving = velocity.magnitude > 0.1f;
            bool isMovingVertically = Mathf.Abs(velocity.y) >= Mathf.Abs(velocity.x);

            animator.SetBool("isWalkingUp", isMoving && isMovingVertically && velocity.y > 0);
            animator.SetBool("isWalkingDown", isMoving && isMovingVertically && velocity.y < 0);
            animator.SetBool("isWalkingSideways", isMoving && !isMovingVertically);

            if (isMoving)
                spriteRenderer.flipX = velocity.x < 0 && !isMovingVertically;
        }

        moveDirection = Vector2.zero;
    }

    public void HurtZombie(float damage, Vector3 knockBack)
    {
        if (damageTimer > 0 || isDead)
            return;

        healthLevel -= damage;

        velocity = knockBack * (moveSpeed == 0 ? 0 : 1) / (int)player.currentDifficulty;

        damageTimer = 0.5f;
       
        if (healthLevel <= 0)
        {
            isDead = true;
            damageTimer = 0.25f; // The red flash is a bit short when they die.
            ToggleColliders(false);
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;

            animator.SetBool("isDead", true);

            GetComponentInChildren<SpriteRenderer>().sortingOrder = -1;
        }

        if (HealthBar_Full != null)
        {
            float healthPercentage = healthLevel / 100;

            HealthBar_Full.transform.localScale = new Vector2(healthPercentage, 1f);
            HealthBar_Full.color = new Color(1 - healthPercentage, healthPercentage, 0);

            HealthBar_Full.transform.parent.gameObject.SetActive(healthLevel != 100 && !isDead);
        }

        if(damage > 0)
            PlayerController.PlayAudioClipFromArray( (isDead && Death.Length != 0) ? Death : Hurt, audioSource, PlayerPrefs.GetFloat("ZombieVolume"));
    }

    void ZombieBehvior()
    {
        Vector3 currentPlayerPosition = player.transform.position;
        Vector3 directionToPlayer = currentPlayerPosition - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float rayThickness = 0.2f;

        bool playerUsedLoudWeapon = player.currentWeapon.alertsZombies && player.currentWeapon.cooldown == 0;


        if (playerUsedLoudWeapon && distanceToPlayer > detection_SightRadius && canSpawnMoreZombies)
            SpawnMoreZombies(0.2f * (int)player.currentDifficulty);

       


        if (distanceToPlayer < detection_SightRadius * 3) // The player is simply too far away.
        {
            if (!playerEnterRangeOnce)
                SpawnMoreZombies((int)player.currentDifficulty * 0.2f);



            RaycastHit2D hit = Physics2D.CircleCast(transform.position, rayThickness, directionToPlayer.normalized, detection_SightRadius * (playerUsedLoudWeapon ? 20 : 1), ~zombieLayer, 0);

            bool canSeePlayer = hit.transform == player.transform;

            /// Can I see the player?
            /// Then I will follow you until I no longer see them.
            /// I will always start tracking them.

            if (canSeePlayer)
            {
                behaviorState = BehaviorState.Chasing;
                MoveTowardsLocation(player.transform.position);

                Debug.DrawLine(transform.position, player.transform.position, Color.blue);
            }

            /// If I can not see the player?
            if (!canSeePlayer)
            {
                /// If I can not see the player, but I haven't seen them recently either, I will just wander.
                if (behaviorState == BehaviorState.Idle && Random.Range(0, (int)(3 / Time.deltaTime)) == 0)
                {
                    velocity += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 1.5f;
                    PlayerController.PlayAudioClipFromArray(Idle, audioSource, PlayerPrefs.GetFloat("ZombieVolume"));
                }

                /// I am now searching.
                if (behaviorState == BehaviorState.Searching) 
                {
                    /// Have I lost interest in tracking the player?
                    if (Random.Range(0, (int)(trackLostChance / Time.deltaTime)) == 0 || validLocations.Count == 0)
                    {
                        behaviorState = BehaviorState.Idle;
                        PlayerController.PlayAudioClipFromArray(Idle, audioSource, PlayerPrefs.GetFloat("ZombieVolume"));
                        validLocations.Clear();

                        PlayerController.PlayAudioClipFromArray(Idle, audioSource, PlayerPrefs.GetFloat("ZombieVolume"));
                        Debug.Log("I lost track of the Player.");
                    }
                    /// If not:
                    else
                    {
                        // Do I need to create a new valid location since the player broke LoS to the last one?

                        Vector3 validLocation = validLocations[validLocations.Count - 1];

                        Vector3 directionFromPointToPlayer = currentPlayerPosition - validLocation;

                        bool canStillSeeOldPoint = directionFromPointToPlayer.magnitude < rayThickness || Physics2D.CircleCast(validLocation, rayThickness, directionFromPointToPlayer, detection_SightRadius).transform == player.transform;

                        if (!canStillSeeOldPoint)
                        {
                            validLocations.Add(previousPlayerPosition);

                            Debug.DrawRay(validLocation, directionFromPointToPlayer, Color.cyan, 3);
                        }
                      

                        // If I still can not see the player, I will move thowards the oldest valid location
                        MoveTowardsLocation(validLocations[0]);

                        Debug.DrawLine(transform.position, validLocations[0], Color.blue);

                        float distanceToOldestLocation = (validLocations[0] - transform.position).magnitude;

                        if (distanceToOldestLocation < 0.1f)
                            validLocations.RemoveAt(0);
                    }
                }

                /// If I just lost track of the player, I will start searching, beginning where I last saw the player.
                if (behaviorState == BehaviorState.Chasing) // OnLosingTrack
                {
                    behaviorState = BehaviorState.Searching;

                    validLocations.Clear();
                    validLocations.Add(previousPlayerPosition);

                    Debug.Log("The player broke LoS.");
                }

            }
            previousPlayerPosition = player.transform.position;
            playerEnterRangeOnce = true;
        }
    }

    void ToggleColliders(bool enabled) // Amazingly you are not allowed to make induvidual raycasts ignore Triggers. Crazy.
    {
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = enabled && !isDead;
    }

    public void SpawnMoreZombies(float chance)
    {
        if (moveSpeed == 0) // Dummies can't spawn more.
            return;

        if (Random.Range(0f, 1f) > chance)
        {
            ZombieController newZombie = GameObject.Instantiate(this).GetComponent<ZombieController>();

            newZombie.canSpawnMoreZombies = false;
            newZombie.transform.position = transform.position;
            newZombie.velocity = player.transform.position - transform.position * 0.3f;
            newZombie.name = "Zombie Minion";
           

            newZombie.Start();
            newZombie.Update();

        }

        canSpawnMoreZombies = false;
    }
}
