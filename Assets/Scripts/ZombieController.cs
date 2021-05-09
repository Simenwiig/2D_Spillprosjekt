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

    public List<PlayerPathNode> playerTrail = new List<PlayerPathNode>();

    [Header("Settings")]
    public float moveSpeed = 0.25f;
    public float chaseSpeed = 5;
    public float friction = 15f;

    public float attackReach = 0.5f;

    public float detection_HearingRadius = 10f;
    public float detection_SightRadius = 10f;

    /// How long will the zombie investigate the player?
    public float investigationDuration = 10;
    public float trackLostChance = 5;

    [Header("Status")]
    public BehaviorState behavior = BehaviorState.Idle;
    public float healthLevel;
    public Vector3 velocity;

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


    [System.Serializable]
    public struct PlayerPathNode
    {
       public bool couldSeePlayer;
       public Vector2 playerLocation;

        public PlayerPathNode(Transform player, bool canSee)
        {
            couldSeePlayer = canSee;
            playerLocation = player.position;
        }
    }

    void Start()
    {
        transform.tag = "GameController";
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        HurtZombie(0, Vector3.zero); // Turns the healthbar invisible as I start with full health.
    }

    void FixedUpdate()
    {
        if(player == null)
								{
            Debug.LogError("Advarsel: Zombiene kan ikke finne spilleren, når det ikke er noen spiller i Scenen.");
            return;
								}

        MovePosition();

        if (isDead)
            return;


        Vector3 directionToPlayer = (player.transform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        bool isWithinRange = distanceToPlayer < Mathf.Max(detection_SightRadius, detection_SightRadius);

        if (!isWithinRange)
            return;

        damageTimer -= Time.fixedDeltaTime;

        bool canSeeYou = Physics2D.Raycast(transform.position, directionToPlayer.normalized, detection_SightRadius).transform == player.transform;




        if (canSeeYou && behavior != BehaviorState.Chasing)
            behavior = BehaviorState.Chasing;

        if (behavior == BehaviorState.Idle)
        {
            if (Random.Range(0, 3 / Time.fixedDeltaTime) < 1)
            {
                velocity += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 1.5f;

                PlayerController.PlayAudioClipFromArray(Idle, audioSource);
            }
            

            if (canSeeYou)
                behavior = BehaviorState.Chasing;
        }

        if (behavior == BehaviorState.Chasing)
        {
            playerTrail.Add(new PlayerPathNode(player.transform, canSeeYou));

            // I start tracking the player.
            // If I have only briefly seen the player, I won't chase for long.

            if (canSeeYou)
            {
                MoveTowardsLocation(player.transform.position);
            }
            else if (!canSeeYou) // Reduant statement, but makes it more readable
            {
                // I just lost track of the player.
                // So I go back to see when I most recently saw the player.

                for(int i = 0; i < playerTrail.Count; i++)
                {
                    int index = playerTrail.Count - 1 - i;

                    if(playerTrail[index].couldSeePlayer)
																				{
                        playerTrail.RemoveRange(0, index);

                        break;
																				}
                }


                MoveTowardsLocation(playerTrail[0].playerLocation);

                if(Vector2.Distance(playerTrail[0].playerLocation, transform.position) < 0.1f)
                    behavior = BehaviorState.Searching;
            }
        }

        if (behavior == BehaviorState.Searching)
        {
            /// Add a new behavior "tracking" where the zombies track recently known positions, starting (but not ending) at last seen positions.
            /// The zombies then forget about you, based on [variable] + Smell. Add some randomoness to not have all the zombies give up at once.

            playerTrail.Add(new PlayerPathNode(player.transform, canSeeYou));


            MoveTowardsLocation(playerTrail[0].playerLocation);
            playerTrail.RemoveAt(0);

            bool lostTrack = Random.Range(0, trackLostChance / Time.fixedDeltaTime) < 1;
            if (lostTrack)
            {
                playerTrail.Clear();
                behavior = BehaviorState.Idle;
                Debug.Log("Lost Track of Player.");
            }

            if(canSeeYou)
												{
                playerTrail.Clear();
                behavior = BehaviorState.Chasing;
            }
        }

        if (distanceToPlayer <= attackReach)
        {
            player.HurtPlayer(35, directionToPlayer.normalized * 5);
            PlayerController.PlayAudioClipFromArray(Attack, audioSource);
        }
				}

				public void MoveTowardsLocation(Vector3 targetPosition)
    {
        moveDirection = (targetPosition - transform.position).normalized * moveSpeed;
    }



    public void MovePosition()
    {
        float frictionStep = friction * Time.fixedDeltaTime;
        velocity -= velocity * frictionStep;
        
        if(!isDead)
          velocity += moveDirection.normalized * (behavior == BehaviorState.Chasing ? chaseSpeed : moveSpeed) * frictionStep;

        transform.position += velocity * Time.fixedDeltaTime;

        if (animator != null && damageTimer < 0)
        {
            bool isMoving = velocity.magnitude > 0.1f;
            bool isMovingVertically = Mathf.Abs(velocity.y) >= Mathf.Abs(velocity.x);

            animator.SetBool("isWalkingUp", isMoving && isMovingVertically && velocity.y > 0);
            animator.SetBool("isWalkingDown", isMoving && isMovingVertically && velocity.y < 0);
            animator.SetBool("isWalkingSideways", isMoving && !isMovingVertically);

            if (isMoving)
                GetComponentInChildren<SpriteRenderer>().flipX = velocity.x < 0;
        }
    }

    public void HurtZombie(float damage, Vector3 knockBack)
    {
        if (damageTimer > 0)
            return;

        healthLevel -= damage;
        velocity = knockBack;

        damageTimer = 0.5f;
       
        if (healthLevel <= 0)
        {
            isDead = true;
            GetComponent<Collider2D>().enabled = false;

            animator.SetBool("isDead", true);
        }

        if (HealthBar_Full != null)
        {
            float healthPercentage = healthLevel / 100;

            HealthBar_Full.transform.localScale = new Vector2(healthPercentage, 1f);
            HealthBar_Full.color = new Color(1 - healthPercentage, healthPercentage, 0);

            HealthBar_Full.transform.parent.gameObject.SetActive(healthLevel != 100 && !isDead);
        }

        if(damage > 0)
            PlayerController.PlayAudioClipFromArray( isDead ? Death : Hurt, audioSource);
    }
}
