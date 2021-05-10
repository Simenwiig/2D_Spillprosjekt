using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public bool isPickup;
    public bool isObstacle;

    public float despawnTime = 1f;

    float shrinkagePercentage = 1;


    public int DoorIndexToUnlock = -1;

    public Manager_Doorways manager_DoorWay;

    public AudioClip breakCrate;

    public string itemName = "";

    Manager_UI manager_UI;

    PlayerController player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        manager_UI = GameObject.Find("_Canvas").GetComponent<Manager_UI>();
    }

    bool isCarried;

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (isPickup && !isCarried && distance < 0.5f)
            OnPickup();

        if (isObstacle && !isCarried && distance < 1f)
            BreakObject();


        if (isCarried)
        {
            transform.localScale = Vector3.one * shrinkagePercentage;


            shrinkagePercentage -= Time.deltaTime / despawnTime;
        }
    }


    void OnPickup()
				{
        transform.parent = player.transform;

        transform.localPosition = Vector2.up * 1;

        isCarried = true;

        GameObject.Destroy(gameObject, despawnTime);

        if (itemName.Length > 0)
        {
            if (itemName == "Crowbar")
            {
                player.weapons[1].isUnlocked = true;
                player.currentWeapon = player.weapons[1];

                manager_UI.SwitchingToMelee();

                return;
            }

            if (itemName == "ParkKey")
            {
                player.haveParkKey = true;

                return;
            }

            if (itemName == "Prison Key")
            {
                player.havePrisonKey = true;

                return;
            }

            if (itemName == "Pistol")
            {
                player.weapons[2].isUnlocked = true;
                player.currentWeapon = player.weapons[2];
                manager_UI.SwitchingToFireArm();

                return;
            }

        }

       if (DoorIndexToUnlock != -1)
       {
                manager_DoorWay.doorways[DoorIndexToUnlock].isLocked = false;
       }
        
				}

    public void BreakObject()
    {

        if ((player.weapons[1].isUnlocked && itemName == "Crate") || (player.haveParkKey && itemName == "ParkDoor"))
        {

            transform.parent = player.transform;

            transform.localPosition = Vector2.up * 1;

            isCarried = true;

            GameObject.Destroy(gameObject, despawnTime);

            GetComponent<Collider2D>().isTrigger = true;


            gameObject.AddComponent<AudioSource>().PlayOneShot(breakCrate);
         }
    }

}


