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



        [Header("Doorway Gameobject (Remember, use two Boxcollider2D)")]
        public GameObject DoorObject;
        public bool UseTheNewSystem = false;

        [Header("Doorway One (Legacy)")]
        public BoxCollider2D Doorway1;

        [Header("Doorway Two (Legacy)")]
        public BoxCollider2D Doorway2;

        [Header("Settings")]
        /// The door does not teleport you.
        public bool isLocked;
        /// The door can only be entered from "Doorway One".
        public bool isOneWay;
        /// The "door" is actually a hole in the floor.
        public bool isFloorHole;

   



        [Header("Optional Sounds")]
        public AudioClip enteringSound;

        bool isHorizontalDoor { get { return Doorway1.size.x > Doorway1.size.y; } }

        public bool EnterDoor(PlayerController player)
        {
            for (int i = 0; i < 2; i++)
            {
                if (isLocked || (i == 1 && isOneWay))
                    continue;

                BoxCollider2D doorWayIn = i == 0 ? Doorway1 : Doorway2;
                BoxCollider2D doorWayOut = i == 0 ? Doorway2 : Doorway1;

                Vector2 playerPosition = player.transform.position;

                Vector2 doorwayPosition = doorWayIn.transform.position + (Vector3)doorWayIn.offset;

                bool isWithinX = Mathf.Abs(doorwayPosition.x - playerPosition.x) < doorWayIn.size.x / 2;
                bool isWithinY = Mathf.Abs(doorwayPosition.y - playerPosition.y) < doorWayIn.size.y / 2;

                bool isMovingThowards = Vector3.Dot((doorwayPosition - playerPosition).normalized, player.velocity.normalized) > 0f;

                if (isWithinX && isWithinY && (isMovingThowards || isFloorHole))
                {
                    if (isFloorHole)
                    {
                        if (!player.isFalling)
                            player.Falling(true);


                        if (!player.isHalfDoneFalling)
                            continue;
                    }

                    Vector3 deltaPosition = playerPosition - doorwayPosition;
                    Vector3 deltaScale = doorWayOut.size / doorWayIn.size;

                    deltaPosition.x = deltaPosition.x * deltaScale.x * (isHorizontalDoor ? -1 : 1);
                    deltaPosition.y = deltaPosition.y * deltaScale.y * (isHorizontalDoor ? 1 : -1);

                    player.transform.position = doorWayOut.transform.position + (Vector3)doorWayOut.offset - deltaPosition * (!isFloorHole ? 1 : -1);

                    if (enteringSound != null)
                        player.GetComponent<AudioSource>().PlayOneShot(enteringSound);

                    return true;
                }
            }
          return false;
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
    bool hasRecentlyFallen;

    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        for (int i = 0; i < Doors.Length; i++)
        {
            DoorSet door = Doors[i];

            if (door.UseTheNewSystem)
            {
                BoxCollider2D[] doorObjectColliders = door.DoorObject.GetComponents<BoxCollider2D>();

                if (doorObjectColliders.Length != 2)
                {
                    Debug.Log("HEY! " + door.name + " requires 2 (two) Boxcolliders, not " + doorObjectColliders.Length + ".");
                    continue;
                }
                door.Doorway1 = doorObjectColliders[0];
                door.Doorway2 = doorObjectColliders[1];
            }


            door.Doorway1.isTrigger = true;
            door.Doorway2.isTrigger = true;
        }
    }

    void Update()
    {
        Vector2 playerPosition = player.transform.position;

        if (hasRecentlyFallen)
            hasRecentlyFallen = player.isFalling;


        for (int i = 0; i < Doors.Length; i++)
        {
            DoorSet door = Doors[i];


            if (door.isFloorHole && hasRecentlyFallen)
                return;


            bool playerWasTeleported = door.EnterDoor(player);

            if (door.isFloorHole && playerWasTeleported)
                hasRecentlyFallen = true;

        }

        
    }
}
