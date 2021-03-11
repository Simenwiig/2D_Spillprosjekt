using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [Header("Settings")]
    public float patrolSpeed = 1f;
    public float chaseSpeed = 2f;

    public float detection_HearingRadius = 10f;
    public float detection_SightRadius = 10f;

    /// How long will the zombie investigate the player?
    public float investigationDuration = 10;

    [Header("Status")]
    public bool isPatroling = true;
    public float healthLevel;
    public Vector3 velocity;

    PlayerController player;
    bool isDead = false;

    public List<PlayerPathNode> playerPathNodes = new List<PlayerPathNode>();

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
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void OnDeath()
    {
        isDead = true;
        transform.eulerAngles = new Vector3(0, 0, 90);
        GetComponent<Collider2D>().enabled = false;
    }


    void FixedUpdate()
    {
        if (isDead)
            return;

        Vector2 directionToPlayer = (player.transform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        bool isWithinRange = distanceToPlayer < Mathf.Max(detection_SightRadius, detection_SightRadius);

        if (!isWithinRange)
            return;

        bool canSeeYou = Physics2D.Raycast(transform.position, directionToPlayer.normalized, detection_SightRadius).transform == player.transform;


        if (isPatroling == false)
        {
            if (canSeeYou)
                MoveTo(player.transform.position);
            else
            {
                bool recentlySeenPlayer = false;
                for (int i = playerPathNodes.Count - 1; i >= 0; i--)
                    if (playerPathNodes[i].couldSeePlayer)
                    {
                        recentlySeenPlayer = true;
                        MoveTo(playerPathNodes[i].playerLocation);

                        /// Add a new behavior "tracking" where the zombies track recently known positions, starting (but not ending) at last seen positions.
                        /// The zombies then forget about you, based on [variable] + Smell. Add some randomoness to not have all the zombies give up at once.
                        break;
                    }

                if(!recentlySeenPlayer)
                {
                    //isPatroling = true;
                    // To summarise, the zombies lose interest if they lose track of the player twice in a row. Very low object-permanance.
                    // If further object-permanance is needed, I'd probably check for LoS for every new checkpoint, and make interest timebased instead.
                }
            }

        }

        playerPathNodes.Add(new PlayerPathNode(player.transform, canSeeYou));

        if (playerPathNodes.Count > investigationDuration / Time.fixedDeltaTime)
            playerPathNodes.RemoveAt(0);

        transform.position += velocity * Time.fixedDeltaTime;
        velocity = Vector3.zero;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        velocity = (targetPosition - transform.position).normalized * chaseSpeed;
    }
}
