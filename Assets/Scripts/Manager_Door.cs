using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Door : MonoBehaviour
{
    [System.Serializable]
    public class DoorSet
    {
        /// This is mostly just to make it easier to remember, but certain items also look for door names.
        public string name = "Name Me Something";
        [Header("Doorway One")]
        public BoxCollider2D Doorway1;

        [Header("Doorway Two")]
        public BoxCollider2D Doorway2;

        [Header("Settings")]
        /// The door does not teleport you.
        public bool isLocked;
        /// The door can only be entered from "Doorway One".
        public bool isOneWay;
        /// The "door" is actually a hole in the floor.
        public bool isAFall;

        [Header("Optional Sounds")]
        public AudioClip lockedSound;
        public AudioClip enteringSound;


        public void EnterDoor(PlayerController player)
        {
            for (int i = 0; i < 2; i++)
            {
                    BoxCollider2D doorWayIn = i == 0 ? Doorway1 : Doorway2;
                    BoxCollider2D doorWayOut = i == 0 ? Doorway2 : Doorway1;

                    Vector2 playerPosition = player.transform.position;

                    Vector2 doorwayPosition = doorWayIn.transform.position + (Vector3)doorWayIn.offset;

                    bool isWithinX = Mathf.Abs(doorwayPosition.x - playerPosition.x) < doorWayIn.size.x / 2;
                    bool isWithinY = Mathf.Abs(doorwayPosition.y - playerPosition.y) < doorWayIn.size.y / 2;

                    bool isMovingThowards = Vector3.Dot((doorwayPosition - playerPosition).normalized, player.velocity.normalized) > 0f;

                    if (isWithinX && isWithinY && isMovingThowards)
                    {
                        Debug.Log("isWorking");

                        player.transform.position = doorWayOut.transform.position + (Vector3)doorWayOut.offset;

                    }
              }
        }

        public static DoorSet GetDoor(DoorSet[] doors, string name, Collider2D coll)
								{
            for (int i = 0; i < doors.Length; i++)
            {
                DoorSet door = doors[i];

                if (door.name == name || (coll != null && (door.Doorway1 == coll || door.Doorway2 == coll)))
                    return door;
            }

            return null;
        }
    }

    public DoorSet[] Doors;

    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        for (int i = 0; i < Doors.Length; i++)
        {
            DoorSet door = Doors[i];

            door.Doorway1.isTrigger = true;
            door.Doorway2.isTrigger = true;
        }
    }

    private void FixedUpdate()
    {
        Vector2 playerPosition = player.transform.position;

        for (int i = 0; i < Doors.Length; i++)
        {
            DoorSet door = Doors[i];

            door.EnterDoor(player);
        }
    }

				void OnTriggerEnter(Collider2D other)
				{
        DoorSet door = DoorSet.GetDoor(Doors, "", other);

       
				}
}
