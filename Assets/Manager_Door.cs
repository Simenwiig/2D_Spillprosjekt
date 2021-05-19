using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Door : MonoBehaviour
{
    [System.Serializable]
    public class DoorSet
    {
        /// This is mostly just to make it easier to remember, but certain items also look for door names.
        public string name;
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
    }

    public DoorSet[] Doors;

    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < Doors.Length; i++)
        {
            DoorSet door = Doors[i];

            door.Doorway1.isTrigger = true;
            door.Doorway2.isTrigger = true;
        }
    }

				void OnTriggerEnter(Collider other)
				{
								
				}
}
